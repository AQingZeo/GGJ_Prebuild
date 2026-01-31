/// <summary>
/// Contract for world objects that accept inventory item use.
/// Implement on MonoBehaviours that react to "Use Item" + click.
/// </summary>
public interface IItemUseTarget
{
    /// <summary>
    /// Try to use the given item on this target.
    /// </summary>
    /// <param name="itemId">The selected inventory item id.</param>
    /// <returns>True if the item was accepted and consumed (if consumeOnUse).</returns>
    bool TryUseItem(string itemId);
}
