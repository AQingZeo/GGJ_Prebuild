using UnityEngine;
using UnityEngine.UI;
using GameContracts;

/// <summary>
/// General pop overlay. The panel in the scene is the frame (you set it up once). Content comes from the interactable: contentPrefab (e.g. lock box), optional imageSprite, or puzzlePrefab.
/// For interactable prefabs (with InteractableCore), assign worldContentAnchor so they spawn in world space and OnMouseDown works.
/// </summary>
public class ImagePopUIController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject panel;
    [SerializeField] private Image image;
    [Tooltip("Parent Transform where UI puzzle prefab is instantiated (under Canvas). Leave null if only using sprite mode.")]
    [SerializeField] private Transform contentParent;
    [Tooltip("Button that closes the overlay. Assign in Inspector.")]
    [SerializeField] private Button closeButton;

    [Header("World-Space Interactable Content")]
    [Tooltip("Transform in WORLD SPACE (not under Canvas) where interactable prefabs spawn. Create an empty GameObject at the position you want (e.g. center of screen in world coords). If assigned, prefabs with InteractableCore spawn here so OnMouseDown works.")]
    [SerializeField] private Transform worldContentAnchor;

    private GameObject _currentPuzzleInstance;
    private string _pendingSolvedFlagKey;
    private string _pendingInteractableId;
    private int _pendingInteractableState;

    private void Awake()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.SetImagePopController(this);
        if (panel != null)
            panel.SetActive(false);
        if (closeButton != null)
            closeButton.onClick.AddListener(Hide);
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.SetImagePopController(null);
    }

    /// <summary>Show a sprite in the image. Optional contentPrefab: if interactable and worldContentAnchor assigned, spawns in world space so OnMouseDown works.</summary>
    public void Show(Sprite sprite, GameObject contentPrefab = null)
    {
        ClearPuzzleInstance();
        if (image != null)
        {
            image.gameObject.SetActive(true);
            image.sprite = sprite;
        }

        if (contentPrefab != null)
        {
            bool isInteractable = contentPrefab.GetComponent<InteractableCore>() != null
                               || contentPrefab.GetComponentInChildren<InteractableCore>(true) != null;

            if (isInteractable && worldContentAnchor != null)
            {
                _currentPuzzleInstance = Instantiate(contentPrefab, worldContentAnchor.position, worldContentAnchor.rotation);
                _currentPuzzleInstance.SetActive(true);
                if (contentParent != null)
                    contentParent.gameObject.SetActive(false);
            }
            else if (contentParent != null)
            {
                contentParent.gameObject.SetActive(true);
                contentParent.SetAsLastSibling();
                _currentPuzzleInstance = Instantiate(contentPrefab, contentParent);
                _currentPuzzleInstance.SetActive(true);
                StretchRectTransform(_currentPuzzleInstance.transform);
                EnsureContentRendersAsUI(_currentPuzzleInstance);
            }
        }
        else if (contentParent != null)
        {
            contentParent.gameObject.SetActive(false);
        }

        if (panel != null)
            panel.SetActive(true);
    }

    /// <summary>Show a sprite in the pop (e.g. lock box). Uses the panel's Image—same sprite asset as SpriteRenderer. Stretches the Image to fill the content area so the sprite isn't tiny in the center.</summary>
    public void ShowContent(Sprite sprite)
    {
        ClearPuzzleInstance();
        if (contentParent != null)
            contentParent.gameObject.SetActive(false);
        if (image != null)
        {
            image.gameObject.SetActive(true);
            image.sprite = sprite;
            image.preserveAspect = false;
            var rect = image.rectTransform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
        if (panel != null)
            panel.SetActive(true);
    }

    /// <summary>Show content prefab (e.g. lock box). If prefab has InteractableCore and worldContentAnchor is assigned, spawns in world space so OnMouseDown works. Otherwise spawns under Canvas (display only).</summary>
    public void ShowContent(GameObject contentPrefab)
    {
        ClearPuzzleInstance();
        if (image != null)
            image.gameObject.SetActive(false);

        if (contentPrefab == null)
        {
            Debug.LogWarning("ImagePopUIController: Content Prefab is null. Assign the lock box (or content) prefab in the interactable's ImagePop action.");
            if (panel != null) panel.SetActive(true);
            return;
        }

        // Check if prefab is interactable (has InteractableCore or Collider2D)
        bool isInteractable = contentPrefab.GetComponent<InteractableCore>() != null
                           || contentPrefab.GetComponentInChildren<InteractableCore>(true) != null;

        if (isInteractable && worldContentAnchor != null)
        {
            // Spawn in world space so OnMouseDown works (sorting layer/order from prefab)
            _currentPuzzleInstance = Instantiate(contentPrefab, worldContentAnchor.position, worldContentAnchor.rotation);
            _currentPuzzleInstance.SetActive(true);
            if (contentParent != null)
                contentParent.gameObject.SetActive(false);
        }
        else
        {
            // Spawn under Canvas (UI mode, display only)
            if (contentParent == null)
            {
                Debug.LogWarning("ImagePopUIController: Content Parent is not assigned. Assign the Transform where content should appear (e.g. child of panel).");
                if (panel != null) panel.SetActive(true);
                return;
            }
            contentParent.gameObject.SetActive(true);
            contentParent.SetAsLastSibling();
            _currentPuzzleInstance = Instantiate(contentPrefab, contentParent);
            _currentPuzzleInstance.SetActive(true);
            StretchRectTransform(_currentPuzzleInstance.transform);
            EnsureContentRendersAsUI(_currentPuzzleInstance);
        }

        if (panel != null)
            panel.SetActive(true);
    }

    /// <summary>Show a puzzle prefab in the content area. When solved, set flag and update interactable state then hide.</summary>
    /// <param name="prefab">Puzzle prefab (e.g. dial box). Must call NotifyPuzzleSolved() when solved (e.g. via UnityEvent).</param>
    /// <param name="solvedFlagKey">Flag key to set to true when puzzle is solved.</param>
    /// <param name="interactableId">Interactable id to update visual state (e.g. "self" or specific id).</param>
    /// <param name="newState">State value to set on the interactable (for visual state change).</param>
    public void ShowPuzzle(GameObject prefab, string solvedFlagKey, string interactableId, int newState)
    {
        ClearPuzzleInstance();
        if (image != null)
            image.gameObject.SetActive(false);
        if (contentParent == null || prefab == null)
        {
            if (panel != null) panel.SetActive(true);
            return;
        }

        _pendingSolvedFlagKey = solvedFlagKey ?? "";
        _pendingInteractableId = interactableId ?? "";
        _pendingInteractableState = newState;

        contentParent.gameObject.SetActive(true);
        _currentPuzzleInstance = Instantiate(prefab, contentParent);
        if (panel != null)
            panel.SetActive(true);
    }

    /// <summary>Call from puzzle prefab when solved (e.g. wire UnityEvent from DialPuzzleController.OnSolved). Sets flag, updates interactable state, hides popup.</summary>
    public void NotifyPuzzleSolved()
    {
        if (GameManager.Instance?.Flags != null && !string.IsNullOrEmpty(_pendingSolvedFlagKey))
            GameManager.Instance.Flags.Set(_pendingSolvedFlagKey, true);

        if (GameManager.Instance?.Interactables != null && !string.IsNullOrEmpty(_pendingInteractableId))
            GameManager.Instance.Interactables.SetState(_pendingInteractableId, _pendingInteractableState);

        EventBus.Publish(new InteractableStateChanged(_pendingInteractableId, _pendingInteractableState));

        _pendingSolvedFlagKey = "";
        _pendingInteractableId = "";
        _pendingInteractableState = 0;
        Hide();
    }

    /// <summary>Call from a Button onClick or elsewhere to close the overlay.</summary>
    public void Hide()
    {
        ClearPuzzleInstance();
        if (panel != null)
            panel.SetActive(false);
        if (image != null)
            image.gameObject.SetActive(true);
        if (contentParent != null)
            contentParent.gameObject.SetActive(false);
    }

    /// <summary>Make instantiated UI fill the content area so it's visible. Tries root, then first child with RectTransform.</summary>
    private static void StretchRectTransform(Transform t)
    {
        var rect = t as RectTransform;
        if (rect == null) rect = t.GetComponent<RectTransform>();
        if (rect == null && t.childCount > 0)
            rect = t.GetChild(0).GetComponent<RectTransform>();
        if (rect == null) return;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.localScale = Vector3.one;
    }

    /// <summary>If the content prefab uses SpriteRenderer, convert it to UI Image so it renders on the Canvas (on top of the overlay frame) and is visible in Game view. Display only—no state sync.</summary>
    private static void EnsureContentRendersAsUI(GameObject instance)
    {
        var sr = instance.GetComponent<SpriteRenderer>();
        if (sr == null && instance.transform.childCount > 0)
            sr = instance.transform.GetChild(0).GetComponent<SpriteRenderer>();
        if (sr == null || sr.sprite == null) return;

        Sprite sprite = sr.sprite;
        Transform target = sr.transform;
        var rect = target as RectTransform ?? target.GetComponent<RectTransform>();
        if (rect == null) return;

        var img = target.GetComponent<Image>();
        if (img == null)
            img = target.gameObject.AddComponent<Image>();
        img.sprite = sprite;
        img.raycastTarget = false;
        img.preserveAspect = true;

        sr.enabled = false;
        foreach (var other in instance.GetComponentsInChildren<SpriteRenderer>(true))
            other.enabled = false;
    }

    private void ClearPuzzleInstance()
    {
        if (_currentPuzzleInstance != null)
        {
            Destroy(_currentPuzzleInstance);
            _currentPuzzleInstance = null;
        }
        _pendingSolvedFlagKey = "";
        _pendingInteractableId = "";
        _pendingInteractableState = 0;
    }
}
