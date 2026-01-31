using System;
using System.Collections.Generic;
using UnityEngine;

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
    [SerializeField] private Collider2D optionalCollider;

    private void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void ApplyState(int state)
    {
        if (spriteRenderer == null) return;

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
