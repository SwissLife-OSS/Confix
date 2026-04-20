namespace Confix.Variables;

public interface IOnePasswordCli
{
    Task<string> ReadAsync(
        string vault,
        string item,
        string field,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<OnePasswordItemSummary>> ListItemsAsync(
        string vault,
        CancellationToken cancellationToken);

    Task EditItemFieldAsync(
        string vault,
        string item,
        string field,
        string value,
        CancellationToken cancellationToken);

    Task CreateItemAsync(
        string vault,
        string item,
        string field,
        string value,
        CancellationToken cancellationToken);
}

public record OnePasswordItemSummary(string Id, string Title);
