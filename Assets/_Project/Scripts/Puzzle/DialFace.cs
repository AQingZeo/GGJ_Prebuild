using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// One dial in a dial puzzle. Cycles through characters on click.
/// Each dial can have different characters (set per instance in Inspector).
/// </summary>
public class DialFace : MonoBehaviour
{
    [Tooltip("Characters this dial cycles through (e.g. \"ABC\" or \"0123456789\"). Different per dial is supported.")]
    [SerializeField] private string characters = "ABCD";
    [SerializeField] private int startIndex = 0;
    [SerializeField] private TMP_Text label;
    [SerializeField] private Button button;

    private int _currentIndex;
    public event System.Action<char> ValueChanged;

    public char CurrentChar => characters != null && characters.Length > 0
        ? characters[(_currentIndex % characters.Length + characters.Length) % characters.Length]
        : '\0';

    private void Awake()
    {
        if (button == null)
            button = GetComponent<Button>();
        if (button != null)
            button.onClick.AddListener(OnClick);
        _currentIndex = startIndex;
        if (characters != null && characters.Length > 0)
            _currentIndex = ((_currentIndex % characters.Length) + characters.Length) % characters.Length;
        UpdateLabel();
    }

    private void OnClick()
    {
        if (characters == null || characters.Length == 0) return;
        _currentIndex = (_currentIndex + 1) % characters.Length;
        UpdateLabel();
        ValueChanged?.Invoke(CurrentChar);
    }

    private void UpdateLabel()
    {
        if (label != null)
            label.text = CurrentChar.ToString();
    }

    public void SetCharacters(string chars)
    {
        characters = chars ?? "";
        _currentIndex = 0;
        UpdateLabel();
    }
}
