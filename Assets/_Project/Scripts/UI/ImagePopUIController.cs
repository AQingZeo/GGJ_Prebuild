using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Shows an image as an overlay (on top). Register with GameManager on Awake.
/// Assign Panel (root to enable/disable) and Image (sprite display) in Inspector.
/// Wire a close Button on the panel to call Hide().
/// </summary>
public class ImagePopUIController : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private Image image;

    private void Awake()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.SetImagePopController(this);
        if (panel != null)
            panel.SetActive(false);
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.SetImagePopController(null);
    }

    public void Show(Sprite sprite)
    {
        if (image != null)
            image.sprite = sprite;
        if (panel != null)
            panel.SetActive(true);
    }

    /// <summary>Call from a Button onClick or elsewhere to close the overlay.</summary>
    public void Hide()
    {
        if (panel != null)
            panel.SetActive(false);
    }
}
