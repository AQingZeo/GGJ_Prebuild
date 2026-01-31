using UnityEngine;
using UnityEngine.UI;
using GameContracts;

/// <summary>
/// Bootstrap UI overlay for Pause state.
/// Handles CanvasGroup visibility and Time.timeScale.
/// </summary>
public class PauseUIController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private CanvasGroup pauseCanvasGroup;

    private void Awake()
    {
        // Make PauseUIController persistent (Bootstrap should not be destroyed)
        DontDestroyOnLoad(gameObject);

        // Get CanvasGroup if not assigned
        if (pauseCanvasGroup == null)
        {
            pauseCanvasGroup = GetComponent<CanvasGroup>();
        }

        // Hide pause UI initially
        Hide();
    }

    /// <summary>
    /// Show pause UI overlay and pause the game.
    /// </summary>
    public void Show()
    {
        if (pauseCanvasGroup != null)
        {
            pauseCanvasGroup.alpha = 1f;
            pauseCanvasGroup.interactable = true;
            pauseCanvasGroup.blocksRaycasts = true;
        }

        // Pause game time
        Time.timeScale = 0f;
    }

    /// <summary>
    /// Hide pause UI overlay and resume the game.
    /// </summary>
    public void Hide()
    {
        if (pauseCanvasGroup != null)
        {
            pauseCanvasGroup.alpha = 0f;
            pauseCanvasGroup.interactable = false;
            pauseCanvasGroup.blocksRaycasts = false;
        }

        // Resume game time
        Time.timeScale = 1f;
    }

    /// <summary>Called by Resume button. Return to previous state (e.g. Explore).</summary>
    public void Resume()
    {
        Hide();
        if (GameStateMachine.Instance != null)
            GameStateMachine.Instance.ReturnToPreviousState();
    }

    /// <summary>Called by Save button. Writes flags, player, interactables to disk.</summary>
    public void SaveGame()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.SaveGame();
    }

    /// <summary>Called by Back to Menu button. Unpauses and switches to Menu scene.</summary>
    public void BackToMenu()
    {
        Hide();
        if (GameStateMachine.Instance != null)
            GameStateMachine.Instance.SetState(GameState.Menu);
    }
}
