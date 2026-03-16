namespace Loca.Application.Interfaces;

public interface IFileStorageService
{
    Task<string> UploadAsync(Stream stream, string fileName, string contentType, CancellationToken ct = default);
    Task<string> UploadAndResizeAsync(Stream stream, string fileName, string contentType, int maxWidth, int maxHeight, CancellationToken ct = default);
    Task DeleteAsync(string fileUrl, CancellationToken ct = default);
}
