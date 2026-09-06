using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MediQueue.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MediQueue.Infrastructure.ExternalServices;

/// <summary>
/// Drug interaction checker powered by OpenAI GPT-4o-mini.
/// Sends the current drug list + new drug to the model and parses structured JSON output.
/// </summary>
public class GptDrugInteractionService : IDrugInteractionService
{
    private readonly HttpClient _httpClient;
    private readonly GptDrugInteractionOptions _options;
    private readonly ILogger<GptDrugInteractionService> _logger;

    public GptDrugInteractionService(
        HttpClient httpClient,
        IOptions<GptDrugInteractionOptions> options,
        ILogger<GptDrugInteractionService> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;

        _httpClient.BaseAddress = new Uri("https://api.openai.com/");
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _options.ApiKey);
    }

    public async Task<DrugInteractionResult> CheckAsync(
        IEnumerable<string> currentDrugNames,
        string newDrugName,
        CancellationToken cancellationToken = default)
    {
        var currentDrugs = currentDrugNames.ToList();
        if (!currentDrugs.Any())
            return new DrugInteractionResult { HasInteractions = false, Summary = "No current medications to check against." };

        var prompt = BuildPrompt(currentDrugs, newDrugName);

        try
        {
            var requestBody = new
            {
                model = _options.Model,
                messages = new[]
                {
                    new
                    {
                        role = "system",
                        content = """
                            You are a clinical pharmacology expert. Your job is to check drug-drug interactions.
                            Always respond with ONLY valid JSON in the exact structure requested. No markdown, no explanation outside JSON.
                            Severity levels: Minor | Moderate | Major | Contraindicated
                            """
                    },
                    new { role = "user", content = prompt }
                },
                temperature = 0.1,   // Low temperature for consistent clinical output
                max_tokens = 1000,
                response_format = new { type = "json_object" }
            };

            var response = await _httpClient.PostAsJsonAsync(
                "v1/chat/completions", requestBody, cancellationToken);

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            return ParseResponse(json, newDrugName);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "GPT-4 drug interaction API call failed for drug: {Drug}", newDrugName);
            // Fail gracefully — return a warning rather than crashing the prescription workflow
            return new DrugInteractionResult
            {
                HasInteractions = false,
                Summary = "Drug interaction check is temporarily unavailable. Please consult clinical references manually."
            };
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse GPT-4 drug interaction response for drug: {Drug}", newDrugName);
            return new DrugInteractionResult
            {
                HasInteractions = false,
                Summary = "Drug interaction check returned an unexpected response. Please consult clinical references manually."
            };
        }
    }

    private static string BuildPrompt(List<string> currentDrugs, string newDrug)
    {
        var currentList = string.Join(", ", currentDrugs);
        // Use $$""" so {{ }} are literal JSON braces; interpolated vars use {{ }}
        return $$"""
            Patient is currently taking: {{currentList}}
            Doctor wants to add: {{newDrug}}

            Check all interactions between "{{newDrug}}" and each of the current medications.
            Respond ONLY with this JSON structure:
            {
              "hasInteractions": true/false,
              "summary": "brief overall summary for the doctor",
              "interactions": [
                {
                  "drug1": "existing drug name",
                  "drug2": "{{newDrug}}",
                  "severity": "Minor|Moderate|Major|Contraindicated",
                  "description": "what happens clinically",
                  "recommendation": "what the doctor should do"
                }
              ]
            }
            If no interactions found, return hasInteractions: false and an empty interactions array.
            """;
    }

    private static DrugInteractionResult ParseResponse(string json, string newDrugName)
    {
        using var doc = JsonDocument.Parse(json);

        // GPT response structure: { choices: [ { message: { content: "<json>" } } ] }
        var content = doc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString() ?? "{}";

        using var inner = JsonDocument.Parse(content);
        var root = inner.RootElement;

        var hasInteractions = root.TryGetProperty("hasInteractions", out var hiProp) && hiProp.GetBoolean();
        var summary = root.TryGetProperty("summary", out var sProp) ? sProp.GetString() ?? "" : "";

        var interactions = new List<DrugInteraction>();
        if (root.TryGetProperty("interactions", out var interactionsProp))
        {
            foreach (var item in interactionsProp.EnumerateArray())
            {
                interactions.Add(new DrugInteraction
                {
                    Drug1 = item.TryGetProperty("drug1", out var d1) ? d1.GetString() ?? "" : "",
                    Drug2 = item.TryGetProperty("drug2", out var d2) ? d2.GetString() ?? newDrugName : newDrugName,
                    Severity = item.TryGetProperty("severity", out var sev) ? sev.GetString() ?? "Unknown" : "Unknown",
                    Description = item.TryGetProperty("description", out var desc) ? desc.GetString() ?? "" : "",
                    Recommendation = item.TryGetProperty("recommendation", out var rec) ? rec.GetString() ?? "" : ""
                });
            }
        }

        return new DrugInteractionResult
        {
            HasInteractions = hasInteractions,
            Summary = summary,
            Interactions = interactions
        };
    }
}

/// <summary>Configuration for the GPT-based drug interaction service.</summary>
public class GptDrugInteractionOptions
{
    public const string SectionName = "OpenAI";

    /// <summary>OpenAI API Key — load from environment variable: OpenAI__ApiKey</summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>Model to use — default: gpt-4o-mini (cost-effective for structured JSON tasks)</summary>
    public string Model { get; set; } = "gpt-4o-mini";
}
