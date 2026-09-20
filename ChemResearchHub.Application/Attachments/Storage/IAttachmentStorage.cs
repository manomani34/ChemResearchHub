namespace ChemResearchHub.Application.Attachments.Storage;

public interface IAttachmentStorage
{
    Task<string> SaveAsync(
        Stream content,
        string extension,
        CancellationToken cancellationToken = default);

    Task<Stream?> OpenReadAsync(
        string storagePath,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        string storagePath,
        CancellationToken cancellationToken = default);
}