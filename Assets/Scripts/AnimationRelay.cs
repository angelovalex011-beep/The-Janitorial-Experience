using UnityEngine;

public class AnimationRelay : MonoBehaviour
{
    public ToiletTrigger toiletTriggerScript;

    // Call this from the Animation Event at the end of the Unclog clip
    public void OnUnclogAnimationComplete()
    {
        Debug.Log("Relay event fired", this);

        // Prefer the toilet that is actually engaged; fall back to the assigned one
        ToiletTrigger target = ToiletTrigger.ActiveToilet != null
            ? ToiletTrigger.ActiveToilet
            : toiletTriggerScript;

        if (target != null)
        {
            target.ExecuteUnclog();
        }
        else
        {
            Debug.Log("No toilet to unclog", this);
        }
    }
}