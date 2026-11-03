using UnityEngine;

public enum ToiletState
{
    Water,
    Pee,
    Poop,
}

public class ToiletStateHandler : MonoBehaviour
{
    public ToiletState currentstate;
    public PointerTrigger SkillCheckScript;
    public ToiletTrigger ToiletTrigger;
    public ToiletUIRotater RotateScript;
    public Animator ToiletAnimator;
    public PlayerCollisionHandler PlayerCollision; // assign the player's PlayerCollisionHandler in Inspector
    private ToiletState previousState;

    void Awake()
    {
        // Prefer the ToiletTrigger on this same toilet, if there is one
        ToiletTrigger own = GetComponent<ToiletTrigger>();
        if (own != null) ToiletTrigger = own;
    }

    void Update()
    {
        // 1. Tell the Animator exactly what state we are in (0, 1, or 2)
        ToiletAnimator.SetInteger("toiletState", (int)currentstate);

        // 2. Keep your trigger check for the transition effect
        if ((previousState == ToiletState.Pee || previousState == ToiletState.Poop) && currentstate == ToiletState.Water)
        {
            ToiletAnimator.SetTrigger("Unclog");
        }

        // The state changed: let the player's handler re-decide which toilets are enabled.
        // (This script no longer touches ToiletTrigger.enabled itself.)
        if (currentstate != previousState && PlayerCollision != null)
        {
            PlayerCollision.RefreshToilets();
        }

        // Only the toilet the player is actually engaged with should drive the shared UI rotator
        if (PlayerCollision != null && PlayerCollision.ToiletTriggerScript == ToiletTrigger)
        {
            if (currentstate == ToiletState.Pee)
            {
                RotateScript.rotationspeed = 350f;
            }
            else if (currentstate == ToiletState.Poop)
            {
                RotateScript.rotationspeed = 450f;
            }
        }

        // Save state at the end
        previousState = currentstate;
    }
}