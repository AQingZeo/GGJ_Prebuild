using UnityEngine;

public class PlayerAnimState : MonoBehaviour
{
    [SerializeField] Animator anim;
    public static PlayerAnimState Current { get; private set; }
    void OnEnable()
    {
        Current = this;
    }

    void OnDisable()
    {
        if (Current == this) Current = null;
    }
    void Reset()
    {
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        // Movement input example
        float x = Input.GetAxisRaw("Horizontal"); // -1,0,1

        // If checking, don't overwrite MoveX logic (optional)
        if (anim.GetBool("IsChecking")) return;

        anim.SetFloat("MoveX", x);
    }

    // Call this when you click an item_back object
    public void StartCheck()
    {
        anim.SetBool("IsChecking", true);
        anim.SetFloat("MoveX", 0f); // forces Idle/Look_back stability
    }

    // Call this when done checking (second click, timer, etc.)
    public void StopCheck()
    {
        anim.SetBool("IsChecking", false);
    }
}
