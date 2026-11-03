using System.Collections.Generic;
using UnityEngine;

public class PlayerCollisionHandler : MonoBehaviour
{
    public AnimationRelay animationRelayScript;
    public ToiletTrigger ToiletTriggerScript;

    // How many of each toilet's colliders the player is currently inside
    // (a toilet has several colliders, so we count instead of using a bool)
    private readonly Dictionary<ToiletTrigger, int> overlaps = new Dictionary<ToiletTrigger, int>();
    private ToiletTrigger lastEntered;

    private void Start()
    {
        // Disable ALL GlassTrigger scripts across the entire scene on startup
        GlassTrigger[] allGlassTriggers = FindObjectsByType<GlassTrigger>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (GlassTrigger trigger in allGlassTriggers)
        {
            trigger.enabled = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if the object we stepped into has the GlassTrigger script
        GlassTrigger glassScript = other.GetComponent<GlassTrigger>();

        if (glassScript != null)
        {
            // Enable ONLY this specific glass trigger script
            glassScript.enabled = true;
        }

        // Handle Toilet Trigger logic
        if (other.CompareTag("Toilet"))
        {
            ToiletTrigger toilet = other.GetComponentInParent<ToiletTrigger>();
            if (toilet == null) return;

            overlaps.TryGetValue(toilet, out int count);
            overlaps[toilet] = count + 1;
            lastEntered = toilet;

            RefreshToilets();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Toilet"))
        {
            ToiletTrigger toilet = other.GetComponentInParent<ToiletTrigger>();
            if (toilet == null) return;

            if (overlaps.TryGetValue(toilet, out int count))
            {
                count--;
                if (count <= 0) overlaps.Remove(toilet);
                else overlaps[toilet] = count;
            }

            // If we left the toilet we last entered, fall back to any toilet we're still inside
            if (lastEntered == toilet && !overlaps.ContainsKey(toilet))
            {
                lastEntered = null;
                foreach (var pair in overlaps)
                {
                    lastEntered = pair.Key;
                    break;
                }
            }

            RefreshToilets();
        }
    }

    // The ONE place that decides which ToiletTrigger scripts are enabled:
    // only the toilet the player is in (and only if it isn't clean),
    // plus any toilet that is mid-minigame.
    public void RefreshToilets()
    {
        ToiletTrigger current = ToiletTrigger.ActiveToilet != null ? ToiletTrigger.ActiveToilet : lastEntered;
        ToiletTriggerScript = current;

        if (animationRelayScript != null && current != null)
        {
            animationRelayScript.toiletTriggerScript = current;
        }

        ToiletTrigger[] allToilets = FindObjectsByType<ToiletTrigger>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (ToiletTrigger toilet in allToilets)
        {
            bool isCurrent = (toilet == current);
            bool isClean = toilet.ToiletState != null && toilet.ToiletState.currentstate == ToiletState.Water;

            toilet.SetPlayerInside(isCurrent && overlaps.ContainsKey(toilet));
            toilet.enabled = toilet.IsEngaged || (isCurrent && !isClean);
        }
    }

    // Kept so the existing calls from ToiletTrigger still work
    public void ResetAllToiletTriggers()
    {
        RefreshToilets();
    }
}