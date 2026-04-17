using UnityEngine;

public class HandsController : MonoBehaviour
{
    public Transform handPoint;

    GameObject currentHands;

    public enum HandState
    {
        None,
        Weapon,
        Carry
    }

    HandState currentState;

    // ------------------------
    // —Œ—“ŒﬂÕ»ﬂ
    // ------------------------

    public void SetWeapon(GameObject weaponPrefab)
    {
        ClearHands();

        currentHands = Instantiate(weaponPrefab, handPoint);

        FixAnimator(currentHands);

        currentState = HandState.Weapon;
    }

    public void SetCarry(GameObject carryPrefab)
    {
        ClearHands();

        currentHands = Instantiate(carryPrefab, handPoint);

        FixAnimator(currentHands);

        currentState = HandState.Carry;
    }

    public void ClearHands()
    {
        if (currentHands != null)
        {
            Destroy(currentHands);
            currentHands = null;
        }

        currentState = HandState.None;
    }

    // ------------------------
    // FIX ANIMATOR
    // ------------------------

    void FixAnimator(GameObject obj)
    {
        Animator anim = obj.GetComponentInChildren<Animator>();

        if (anim != null)
        {
            anim.enabled = true;
            anim.Rebind();
            anim.Update(0f);
        }
    }
}