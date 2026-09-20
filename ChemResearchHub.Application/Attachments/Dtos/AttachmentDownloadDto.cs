namespace ChemResearchHub.Application.Attachments.Dtos;

public class AttachmentDownloadDto
{
    public string FileName { get; init; } = string.Empty;

    public string ContentType { get; init; } = "application/octet-stream";

    public Stream Content { get; init; } = null!;
}