using System.Collections;
using UnityEngine;

/// <summary>
/// Handles a fullscreen fade for room transitions.
/// </summary>
public class RoomTransitionController : MonoBehaviour
{
    [Tooltip("CanvasGroup that covers the screen with a solid black image.")]
    [SerializeField] private CanvasGroup canvasGroup;

    [Tooltip("Duration of the fade in seconds.")]
    [SerializeField] private float fadeDuration = 0.4f;

    [Tooltip("Should we block raycasts on the CanvasGroup while fading?")]
    [SerializeField] private bool blockRaycastsDuringFade = true;

    private void Awake()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
        }
    }

    public IEnumerator FadeOut()
    {
        yield return FadeRoutine(0f, 1f);
    }

    public IEnumerator FadeIn()
    {
        yield return FadeRoutine(1f, 0f);
    }

    private IEnumerator FadeRoutine(float from, float to)
    {
        if (canvasGroup == null)
            yield break;

        float timer = 0f;
        canvasGroup.blocksRaycasts = blockRaycastsDuringFade;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / fadeDuration);
            canvasGroup.alpha = Mathf.Lerp(from, to, t);
            yield return null;
        }

        canvasGroup.alpha = to;
        canvasGroup.blocksRaycasts = to > 0 ? blockRaycastsDuringFade : false;
    }
}
