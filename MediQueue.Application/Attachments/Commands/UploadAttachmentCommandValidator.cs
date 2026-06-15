using System;
using System.IO;
using FluentValidation;

namespace MediQueue.Application.Attachments.Commands;

public class UploadAttachmentCommandValidator : AbstractValidator<UploadAttachmentCommand>
{
    private static readonly string[] AllowedMimeTypes =
    [
        "image/jpeg",
        "image/png",
        "image/webp",
        "application/pdf"
    ];

    private const long MaxFileSize = 10 * 1024 * 1024;

    public UploadAttachmentCommandValidator()
    {
        RuleFor(x => x.PatientId)
            .NotEmpty().WithMessage("PatientId is required.");

        RuleFor(x => x.FileName)
            .NotEmpty().WithMessage("File name is required.")
            .Must(IsValidFilename).WithMessage("File name contains invalid characters.");

        RuleFor(x => x.ContentType)
            .NotEmpty().WithMessage("Content type is required.")
            .Must(AllowedMimeTypes.Contains)
            .WithMessage($"File type is not supported. Allowed: {string.Join(", ", AllowedMimeTypes)}");

        RuleFor(x => x.FileSize)
            .GreaterThan(0).WithMessage("File is empty.")
            .LessThanOrEqualTo(MaxFileSize).WithMessage($"File exceeds the {MaxFileSize / (1024 * 1024)} MB limit.");

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Invalid attachment type.");

        When(x => x.ClinicalVisitId.HasValue, () =>
        {
            RuleFor(x => x.ClinicalVisitId)
                .NotEmpty().WithMessage("ClinicalVisitId must not be empty if provided.");
        });
    }

    private static bool IsValidFilename(string filename)
    {
        if (string.IsNullOrWhiteSpace(filename))
            return false;

        if (filename.Contains("..") || filename.Contains("/") ||
            filename.Contains("\\") || filename.Contains('\0'))
            return false;

        var extension = Path.GetExtension(filename);
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp", ".pdf" };
        return !string.IsNullOrEmpty(extension) &&
               allowedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase);
    }
}
