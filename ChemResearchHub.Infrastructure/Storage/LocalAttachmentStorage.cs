using ChemResearchHub.Application.Attachments.Storage;

namespace ChemResearchHub.Infrastructure.Storage;

public class LocalAttachmentStorage : IAttachmentStorage
{
    private readonly string _rootPath;

    public LocalAttachmentStorage(
        string rootPath)
    {
        if (string.IsNullOrWhiteSpace(rootPath))
        {
            throw new ArgumentException(
                "Attachment storage root path is required.",
                nameof(rootPath));
        }

        _rootPath =
            Path.GetFullPath(rootPath);

        Directory.CreateDirectory(
            _rootPath);
    }

    public async Task<string> SaveAsync(
        Stream content,
        string extension,
        CancellationToken cancellationToken = default)
    {
        if (content is null)
        {
            throw new ArgumentNullException(
                nameof(content));
        }

        var safeExtension =
            extension?.Trim().ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(safeExtension))
        {
            throw new ArgumentException(
                "File extension is required.",
                nameof(extension));
        }

        var storedFileName =
            $"{Guid.NewGuid():N}{safeExtension}";

        var fullPath =
            Path.Combine(
                _rootPath,
                storedFileName);

        await using var fileStream =
            new FileStream(
                fullPath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None);

        await content.CopyToAsync(
            fileStream,
            cancellationToken);

        return Path.Combine(
                _rootPath,
                storedFileName)
            .Replace('\\', '/');
    }

    public Task<Stream?> OpenReadAsync(
        string storagePath,
        CancellationToken cancellationToken = default)
    {
        var fullPath =
            GetSafeFullPath(
                storagePath);

        if (!File.Exists(fullPath))
        {
            return Task.FromResult<Stream?>(null);
        }

        Stream stream =
            new FileStream(
                fullPath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read);

        return Task.FromResult<Stream?>(stream);
    }

    public Task DeleteAsync(
        string storagePath,
        CancellationToken cancellationToken = default)
    {
        var fullPath =
            GetSafeFullPath(
                storagePath);

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        return Task.CompletedTask;
    }

    private string GetSafeFullPath(
        string storagePath)
    {
        if (string.IsNullOrWhiteSpace(storagePath))
        {
            throw new InvalidOperationException(
                "Attachment storage path is empty.");
        }

        var fileName =
            Path.GetFileName(
                storagePath);

        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new InvalidOperationException(
                "Invalid attachment storage path.");
        }

        var fullPath =
            Path.GetFullPath(
                Path.Combine(
                    _rootPath,
                    fileName));

        var root =
            Path.GetFullPath(
                _rootPath);

        if (!fullPath.StartsWith(
                root + Path.DirectorySeparatorChar,
                StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(
                fullPath,
                root,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Invalid attachment storage path.");
        }

        return fullPath;
    }
}