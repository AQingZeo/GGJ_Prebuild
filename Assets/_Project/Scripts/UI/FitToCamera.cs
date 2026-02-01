using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class FitToCamera : MonoBehaviour
{
    void Start()
    {
        var cam = Camera.main;
        var sr = GetComponent<SpriteRenderer>();

        float worldHeight = cam.orthographicSize * 2f;
        float worldWidth = worldHeight * cam.aspect;

        Vector2 spriteSize = sr.sprite.bounds.size;

        transform.localScale = new Vector3(
            worldWidth / spriteSize.x,
            worldHeight / spriteSize.y,
            1f
        );
    }
}

