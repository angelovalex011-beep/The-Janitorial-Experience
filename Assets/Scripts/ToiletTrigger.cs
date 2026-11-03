using UnityEngine;

public class ToiletTrigger : MonoBehaviour
{
    // The one toilet currently running the minigame
    public static ToiletTrigger ActiveToilet;
    public bool IsEngaged => isEngaged;

    public GameObject SkillCheck;
    public GameObject Plunger;
    private bool IsPlayerInTrigger; // set by PlayerCollisionHandler
    private bool isEngaged;         // true once interaction has actually started
    private bool clogFinishSent;    // makes sure ClogFinish is only set once

    [SerializeField] private PointerTrigger pointertrigger;
    public Animator targetAnimator;
    public PlayerMovement MovementScript;
    public GameObject Player;
    public GameObject targetLocation;
    public ToiletStateHandler ToiletState;
    public Animator PlayerModel;

    [Header("Camera Culling")]
    public Camera playerCamera;                       // the camera that should reveal the layer
    public string hiddenLayerName = " Hide for Player"; // exact layer name from Project Settings > Tags and Layers
    private int hiddenLayerMask;

    void Awake()
    {
        // Prefer the ToiletStateHandler on this same toilet, if there is one
        ToiletStateHandler own = GetComponent<ToiletStateHandler>();
        if (own != null) ToiletState = own;

        // Fall back to the main camera if none was assigned
        if (playerCamera == null) playerCamera = Camera.main;

        int layerIndex = LayerMask.NameToLayer(hiddenLayerName);
        if (layerIndex == -1)
        {
            Debug.LogWarning($"Layer '{hiddenLayerName}' not found. Check the name in Tags and Layers.", this);
            hiddenLayerMask = 0;
        }
        else
        {
            hiddenLayerMask = 1 << layerIndex;
        }
    }

    void Start()
    {
        // Leave the Plunger ENABLED in the Inspector so the Animator captures
        // "on" as its default, then hide it here once the scene is running.
        if (Plunger != null) Plunger.SetActive(false);
    }

    // Called by PlayerCollisionHandler to say whether the player is at this toilet
    public void SetPlayerInside(bool inside)
    {
        bool leaving = IsPlayerInTrigger && !inside;
        IsPlayerInTrigger = inside;

        // Walked away without starting: put the shared score back to its default
        if (leaving && !isEngaged && ActiveToilet == null && pointertrigger != null)
        {
            pointertrigger.ToiletScore = 5;
        }
    }

    void Update()
    {
        // Start the interaction ONCE on a single Space press
        if (IsPlayerInTrigger && !isEngaged && ActiveToilet == null && Input.GetKeyDown(KeyCode.Space))
        {
            StartInteraction();
        }

        // Per-frame logic while engaged (no Space requirement, no Play() calls)
        if (isEngaged)
        {
            if (pointertrigger.ToiletScore == -1)
            {
                ToiletFail();
                return;
            }

            if (pointertrigger.ToiletScore == 15)
            {
                SkillCheck.SetActive(false);

                // Tell the Animator once; let its transitions handle Sink -> Unclog
                if (!clogFinishSent)
                {
                    clogFinishSent = true;
                    targetAnimator.SetBool("ClogFinish", true);
                }
            }

            // Keep the player locked to the toilet position
            if (Player != null && targetLocation != null)
            {
                Player.transform.position = targetLocation.transform.position;
                Player.transform.rotation = targetLocation.transform.rotation;
            }
        }
    }

    private void StartInteraction()
    {
        Debug.Log($"{name} starting. targetLocation = {(targetLocation != null ? targetLocation.name : "NONE")} at {(targetLocation != null ? targetLocation.transform.position.ToString() : "-")}", this);
        ActiveToilet = this;

        isEngaged = true;
        clogFinishSent = false;

        SkillCheck.SetActive(true);
        Plunger.SetActive(true);
        MovementScript.enabled = false;
        PlayerModel.enabled = false;

        SetHiddenLayerVisible(true);   // show the "Hide for Player" layer

        targetAnimator.enabled = true;
        targetAnimator.SetBool("ClogFinish", false);
        targetAnimator.Play("Plunger Set"); // once only
    }

    public void ExecuteUnclog()
    {
        if (pointertrigger.ToiletScore == 15)
        {
            ToiletUnclog();
        }
    }

    public void ToiletUnclog()
    {
        targetAnimator.SetBool("ClogFinish", false);
        targetAnimator.Rebind();
        targetAnimator.enabled = false;

        SkillCheck.SetActive(false);
        MovementScript.enabled = true;
        PlayerModel.enabled = true;
        pointertrigger.ToiletScore = 5;
        isEngaged = false;
        clogFinishSent = false;
        IsPlayerInTrigger = false;
        ToiletState.currentstate = 0;

        SetHiddenLayerVisible(false);  // hide the layer again

        Plunger.SetActive(false);   // last, so nothing can undo it

        if (ActiveToilet == this) ActiveToilet = null;

        if (Player != null)
        {
            Player.GetComponent<PlayerCollisionHandler>()?.ResetAllToiletTriggers();
        }
    }

    public void ToiletFail()
    {
        targetAnimator.SetBool("ClogFinish", false);
        targetAnimator.Rebind();
        targetAnimator.enabled = false;

        SkillCheck.SetActive(false);
        MovementScript.enabled = true;
        PlayerModel.enabled = true;
        pointertrigger.ToiletScore = 5;   // don't leave it at -1
        isEngaged = false;
        clogFinishSent = false;
        IsPlayerInTrigger = false;

        SetHiddenLayerVisible(false);  // hide the layer again

        Plunger.SetActive(false);   // last

        if (ActiveToilet == this) ActiveToilet = null;

        if (Player != null)
        {
            Player.GetComponent<PlayerCollisionHandler>()?.ResetAllToiletTriggers();
        }
    }

    // Adds or removes the "Hide for Player" layer from the camera's culling mask
    private void SetHiddenLayerVisible(bool visible)
    {
        if (playerCamera == null || hiddenLayerMask == 0) return;

        if (visible)
            playerCamera.cullingMask |= hiddenLayerMask;    // turn layer ON
        else
            playerCamera.cullingMask &= ~hiddenLayerMask;   // turn layer OFF
    }

    void OnDrawGizmosSelected()
    {
        if (targetLocation == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, targetLocation.transform.position);
        Gizmos.DrawWireSphere(targetLocation.transform.position, 0.3f);
    }
}