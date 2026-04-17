namespace Confix.Variables;

/// <summary>
/// Abstraction over the 1Password CLI (<c>op</c>) for vault operations.
/// </summary>
public interface IOnePasswordCli
{
    /// <summary>
    /// Reads a single field value from a 1Password item.
    /// </summary>
    /// <param name="vault">The name or identifier of the vault.</param>
    /// <param name="item">The name or identifier of the item.</param>
    /// <param name="field">The field label to read.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The field value as a string.</returns>
    Task<string> ReadAsync(
        string vault,
        string item,
        string field,
        CancellationToken cancellationToken);

    /// <summary>
    /// Lists all items in the specified vault.
    /// </summary>
    /// <param name="vault">The name or identifier of the vault.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A read-only list of item summaries.</returns>
    Task<IReadOnlyList<OnePasswordItemSummary>> ListItemsAsync(
        string vault,
        CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves the full details of a specific item in a vault.
    /// </summary>
    /// <param name="vault">The name or identifier of the vault.</param>
    /// <param name="item">The name or identifier of the item.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The item details including all fields.</returns>
    Task<OnePasswordItemDetail> GetItemAsync(
        string vault,
        string item,
        CancellationToken cancellationToken);

    /// <summary>
    /// Edits an existing field on a 1Password item.
    /// </summary>
    /// <param name="vault">The name or identifier of the vault.</param>
    /// <param name="item">The name or identifier of the item.</param>
    /// <param name="field">The field label to edit.</param>
    /// <param name="value">The new value for the field.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task EditItemFieldAsync(
        string vault,
        string item,
        string field,
        string value,
        CancellationToken cancellationToken);

    /// <summary>
    /// Creates a new item with a single field in the specified vault.
    /// </summary>
    /// <param name="vault">The name or identifier of the vault.</param>
    /// <param name="item">The name for the new item.</param>
    /// <param name="field">The field label to create.</param>
    /// <param name="value">The value for the field.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task CreateItemAsync(
        string vault,
        string item,
        string field,
        string value,
        CancellationToken cancellationToken);
}

/// <summary>
/// A summary of a 1Password item containing its identifier and title.
/// </summary>
/// <param name="Id">The unique identifier of the item.</param>
/// <param name="Title">The display title of the item.</param>
public record OnePasswordItemSummary(string Id, string Title);

/// <summary>
/// Information about a single field within a 1Password item.
/// </summary>
/// <param name="Id">The unique identifier of the field.</param>
/// <param name="Label">The display label of the field.</param>
/// <param name="Value">The value of the field, or <c>null</c> if empty.</param>
public record OnePasswordFieldInfo(string Id, string Label, string? Value);

/// <summary>
/// Full details of a 1Password item including all its fields.
/// </summary>
/// <param name="Id">The unique identifier of the item.</param>
/// <param name="Title">The display title of the item.</param>
/// <param name="Fields">The list of fields belonging to this item.</param>
public record OnePasswordItemDetail(
    string Id,
    string Title,
    IReadOnlyList<OnePasswordFieldInfo> Fields);
