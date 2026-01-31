using UnityEngine;
using TMPro;

/// <summary>
/// Optional component on choice button prefab. Assign text in prefab so
/// ChoiceUIController does not need GetComponentInChildren.
/// </summary>
public class ChoiceButtonView : MonoBehaviour
{
    [SerializeField] private TMP_Text text;

    public TMP_Text Text => text;
}
