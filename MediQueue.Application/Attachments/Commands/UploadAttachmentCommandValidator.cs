using System;
using System.IO;
using System.Linq;
using FluentValidation;

namespace MediQueue.Application.Attachments.Commands;

public class UploadAttachmentCommandValidator : AbstractValidator<UploadAttachmentCommand>
{
    private static readonly string[] AllowedContentTypes =
    {
        "image/jpeg", "image/png", "image/webp",
        "application/pdf",
        "application/msword",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
    };
    private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10MB

    public UploadAttachmentCommandValidator()
    {
        RuleFor(x => x.PatientId)
            .NotEmpty().WithMessage("PatientId is required.");

        RuleFor(x => x.FileName)
            .NotEmpty()
            .MaximumLength(255)
            .Must(name => !name.Contains("..") && !name.Contains("/") && !name.Contains("\\"))
                .WithMessage("Filename contains invalid characters.");

        RuleFor(x => x.ContentType)
            .NotEmpty()
            .Must(ct => AllowedContentTypes.Contains(ct))
                .WithMessage($"File type not allowed. Allowed types: {string.Join(", ", AllowedContentTypes)}");

        RuleFor(x => x.FileSize) // adjusted property name from FileSizeBytes to FileSize to match actual command
            .GreaterThan(0)
            .LessThanOrEqualTo(MaxFileSizeBytes)
                .WithMessage("File exceeds maximum allowed size of 10MB.");

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Invalid attachment type.");

        When(x => x.ClinicalVisitId.HasValue, () =>
        {
            RuleFor(x => x.ClinicalVisitId)
                .NotEmpty().WithMessage("ClinicalVisitId must not be empty if provided.");
        });
    }
}
