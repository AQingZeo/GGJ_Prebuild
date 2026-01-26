using UnityEngine;
using UnityEngine.UI;
using GameContracts;

/// <summary>
/// For Menu state.
/// Handle the button callback and enables continue if HaveSave(), should not handle the logic.
/// </summary>
public class MenuController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button quitButton;

    private void Start()
    {
        // Setup button callbacks
        if (startButton != null)
        {
            startButton.onClick.AddListener(OnStartClicked);
        }

        if (continueButton != null)
        {
            continueButton.onClick.AddListener(OnContinueClicked);
            // Enable continue button if HasSave() returns true
            continueButton.interactable = HasSave();
        }

        if (quitButton != null)
        {
            quitButton.onClick.AddListener(OnQuitClicked);
        }
    }

    private void OnDestroy()
    {
        // Clean up button listeners
        if (startButton != null)
            startButton.onClick.RemoveListener(OnStartClicked);
        if (continueButton != null)
            continueButton.onClick.RemoveListener(OnContinueClicked);
        if (quitButton != null)
            quitButton.onClick.RemoveListener(OnQuitClicked);
    }

    /// <summary>
    /// Check if there is a save file.
    /// </summary>
    private bool HasSave()
    {
        if (GameManager.Instance == null || GameManager.Instance.Save == null)
            return false;
        
        return GameManager.Instance.Save.HasSave();
    }

    /// <summary>
    /// Handle Start button click. Should not handle the logic.
    /// </summary>
    private void OnStartClicked()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("<color=red>【MenuController】GameManager.Instance is null! Bootstrap scene should be loaded first.</color>");
            return;
        }
        
        Debug.Log("<color=cyan>【MenuController】Start button clicked, calling GameManager.StartGame()</color>");
        GameManager.Instance.StartGame();
    }

    /// <summary>
    /// Handle Continue button click. Should not handle the logic.
    /// </summary>
    private void OnContinueClicked()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoadGame();
        }
    }

    /// <summary>
    /// Handle Quit button click. Should not handle the logic.
    /// </summary>
    private void OnQuitClicked()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.QuitGame();
        }
    }
}
