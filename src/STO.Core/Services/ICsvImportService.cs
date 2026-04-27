using STO.Core.Models;

namespace STO.Core.Services;

public interface ICsvImportService
{
    Task<ImportResult> ImportCharactersAsync(Stream csvStream, int accountId);
    Task<ImportResult> ImportInventoryAsync(Stream csvStream, int characterId);
    Task<ImportResult> ImportValuableItemsAsync(Stream csvStream, int accountId);
    Task<Stream> ExportCharactersAsync(int accountId);
    Task<Stream> ExportInventoryAsync(int characterId);
}
