// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Infrastructure\ExternalServices\DashboardJobs.cs
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MediQueue.Application.Dashboard.Queries;
using MediatR;
using MediQueue.Application.Interfaces;

namespace MediQueue.Infrastructure.ExternalServices;

public class DashboardJobs
{
    private readonly IMediator _mediator;
    private readonly IEmailService _emailService;
    private readonly ILogger<DashboardJobs> _logger;

    public DashboardJobs(IMediator mediator, IEmailService emailService, ILogger<DashboardJobs> logger)
    {
        _mediator = mediator;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task SendDailyRevenueReportAsync()
    {
        _logger.LogInformation("Starting Daily Revenue Report Job at {Time}", DateTime.UtcNow);

        var yesterday = DateTime.UtcNow.AddDays(-1).Date;
        var result = await _mediator.Send(new GetRevenueReportQuery(yesterday, yesterday));

        if (result.IsSuccess)
        {
            var report = result.Value;
            var body = $"<h1>Daily Revenue Report - {yesterday:yyyy-MM-dd}</h1>" +
                       $"<p>Total Revenue: {report!.TotalRevenue:C}</p>";

            await _emailService.SendEmailAsync("admin@mediqueue.com", "Daily Revenue Report", body);
            _logger.LogInformation("Daily Revenue Report sent successfully.");
        }
        else
        {
            _logger.LogError("Failed to generate Daily Revenue Report: {Error}", result.Error);
        }
    }
}
