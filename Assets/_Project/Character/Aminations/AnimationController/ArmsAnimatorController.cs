using UnityEngine;

public class ArmsAnimatorController : MonoBehaviour
{
    [Header("Animator")]
    public Animator animator;

    private void Awake()
    {
        if (!animator)
            animator = GetComponent<Animator>();

        Debug.Log("[ArmsAnimatorController] Animator = " + animator);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            PlayAttack();
        }
    }

    void PlayAttack()
    {
        if (!animator) return;

        animator.SetTrigger("Attack");
        Debug.Log("[ArmsAnimatorController] Attack trigger set");
    }
}
