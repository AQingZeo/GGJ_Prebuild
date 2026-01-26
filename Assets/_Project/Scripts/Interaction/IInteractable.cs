using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Interface for objects that can be interacted with.
/// Implement this interface on any GameObject that should respond to player interaction.
/// </summary>
public interface IInteractable
{
    /// <summary>
    /// Check if this object can currently be interacted with.
    /// </summary>
    /// <returns>True if interaction is possible, false otherwise</returns>
    bool CanInteract();

    /// <summary>
    /// Perform the interaction action.
    /// Called by InteractionController when the player interacts with this object.
    /// </summary>
    void Interact();
}
