using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Dial puzzle: when all dials match the target sequence, invokes OnSolved (e.g. wire to ImagePopUIController.NotifyPuzzleSolved).
/// Each DialFace can have different characters; assign dials in order (0..5 for 6-letter), set target combination string.
/// </summary>
public class DialPuzzleController : MonoBehaviour
{
    [Tooltip("Dials in order (e.g. 6 dials for 6-letter combination).")]
    [SerializeField] private DialFace[] dials = new DialFace[0];
    [Tooltip("Target combination (one character per dial, e.g. \"OPENED\"). Case-sensitive unless ignoreCase is set.")]
    [SerializeField] private string targetCombination = "";
    [Tooltip("If true, comparison is case-insensitive.")]
    [SerializeField] private bool ignoreCase = true;
    [Tooltip("Optional: show this when solved (e.g. success text container).")]
    [SerializeField] private GameObject successReveal;
    [Tooltip("Invoke when puzzle is solved. Wire to ImagePopUIController.NotifyPuzzleSolved().")]
    [SerializeField] private UnityEvent onSolved = new UnityEvent();

    private bool _solved;

    private void Start()
    {
        if (dials == null) return;
        foreach (var dial in dials)
        {
            if (dial != null)
            {
                dial.ValueChanged += OnDialChanged;
            }
        }
        if (successReveal != null)
            successReveal.SetActive(false);
    }

    private void OnDestroy()
    {
        if (dials == null) return;
        foreach (var dial in dials)
        {
            if (dial != null)
                dial.ValueChanged -= OnDialChanged;
        }
    }

    private void OnDialChanged(char c)
    {
        if (_solved) return;
        CheckSolved();
    }

    private void CheckSolved()
    {
        if (dials == null || dials.Length == 0) return;
        string target = targetCombination ?? "";
        if (ignoreCase) target = target.ToUpperInvariant();

        var current = GetCurrentCombination();
        if (current.Length != target.Length) return;

        bool match = true;
        for (int i = 0; i < current.Length && i < target.Length; i++)
        {
            char a = current[i];
            char b = target[i];
            if (ignoreCase) { a = char.ToUpperInvariant(a); b = char.ToUpperInvariant(b); }
            if (a != b) { match = false; break; }
        }

        if (match)
        {
            _solved = true;
            if (successReveal != null)
                successReveal.SetActive(true);
            onSolved?.Invoke();
        }
    }

    private string GetCurrentCombination()
    {
        if (dials == null) return "";
        var sb = new System.Text.StringBuilder(dials.Length);
        foreach (var dial in dials)
        {
            if (dial != null)
                sb.Append(dial.CurrentChar);
        }
        return sb.ToString();
    }
}
