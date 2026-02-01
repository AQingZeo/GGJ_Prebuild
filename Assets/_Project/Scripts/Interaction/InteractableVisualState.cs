using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class StateSprite
{
    public int state;
    public Sprite sprite;
}

public class InteractableVisualState : MonoBehaviour
{
    [SerializeField] private List<StateSprite> stateSprites = new List<StateSprite>();
    [SerializeField] private Sprite defaultSprite;
    [SerializeField] private bool hideIfStateMissing = false;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private UnityEngine.UI.Image image;
    [SerializeField] private Collider2D optionalCollider;

    private void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
        if (image == null)
            image = GetComponent<UnityEngine.UI.Image>();
    }

    /// <summary>Use UI Image for state (e.g. when content is shown in ImagePop). Set at runtime if needed.</summary>
    public void SetImage(UnityEngine.UI.Image img) { image = img; }

    public void ApplyState(int state)
    {
        Sprite spriteToUse = null;
        foreach (var entry in stateSprites)
        {
            if (entry.state == state && entry.sprite != null)
            {
                spriteToUse = entry.sprite;
                break;
            }
        }
        if (spriteToUse == null)
            spriteToUse = defaultSprite;

        if (image != null)
        {
            if (spriteToUse != null)
            {
                image.sprite = spriteToUse;
                if (!image.gameObject.activeSelf)
                    image.gameObject.SetActive(true);
            }
            else if (hideIfStateMissing)
            {
                image.gameObject.SetActive(false);
            }
            if (optionalCollider != null)
                optionalCollider.enabled = (spriteToUse != null);
            return;
        }

        if (spriteRenderer == null) return;

        if (spriteToUse != null)
        {
            spriteRenderer.sprite = spriteToUse;
            if (!spriteRenderer.gameObject.activeSelf)
                spriteRenderer.gameObject.SetActive(true);
        }
        else if (hideIfStateMissing)
        {
            spriteRenderer.gameObject.SetActive(false);
        }

        if (optionalCollider != null)
            optionalCollider.enabled = (spriteToUse != null);
    }
}
