using UnityEngine;

public class InteractionController : MonoBehaviour
{
    [Header("ÃΩ≤‚…Ë÷√")]
    public float radius = 1.5f;          
    public LayerMask interactableLayer;  

    public void TryInteract()
    {
        Collider2D hit = Physics2D.OverlapCircle(transform.position, radius, interactableLayer);

        if (hit != null)
        {
            IInteractable target = hit.GetComponent<IInteractable>();
            if (target != null)
            {
                target.Interact();
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}