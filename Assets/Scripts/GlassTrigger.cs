using UnityEngine;
using System.Collections.Generic;

public class GlassTrigger : MonoBehaviour 
{
    public GameObject Player;
    public GameObject Wipe;
    public List<GameObject> smudges = new List<GameObject>();
    public bool IsPlayerInside = false;
    private bool isCleaning = false;
    public GameObject PositionSet;
    public float PositionSpeed = 5f;
    public float RotationSpeed = 5f;
    public Camera Camera;
    public Collider TriggerCollider;
    [SerializeField] private float glassZOffset = 0.1f;
    [SerializeField] private float wipeSensitivity = 0.05f;
    [SerializeField] private Vector2 wipeScreenBoundsMin = new Vector2(0.1f, 0.1f);
    [SerializeField] private Vector2 wipeScreenBoundsMax = new Vector2(0.9f, 0.9f);
    private Vector3 virtualMousePos;

    void Awake() 
{ 
    // 1. Find the player
    if (Player == null) 
    { 
        Player = GameObject.FindGameObjectWithTag("Player"); 
        if (Player == null) Debug.LogWarning("GlassTrigger: Could not find any GameObject with 'Player' tag!"); 
    } 

    // 2. Look INSIDE the player for the Wipe object
    if (Wipe == null && Player != null) 
    { 
        // This searches all children and deep sub-children of the Player for an object named "Wipe"
        Transform playerWipe = Player.transform.Find("Wipe"); 
        
        // If it's buried deep inside the player's hands/camera, look deeper:
        if (playerWipe == null) {
            playerWipe = Player.GetComponentInChildren<Transform>(true); // Looks through inactive objects too
            foreach(Transform child in Player.GetComponentsInChildren<Transform>(true)) {
                if(child.name == "Wipe") {
                    playerWipe = child;
                    break;
                }
            }
        }

        if (playerWipe != null) 
        { 
            Wipe = playerWipe.gameObject; 
        } 
        else 
        { 
            Debug.LogError($"GlassTrigger on {gameObject.name}: Could not find a 'Wipe' object inside the Player!"); 
        } 
    } 
}


    void Start() 
    {
        if (Wipe != null) Wipe.SetActive(false);
    } 

    void Update() 
    {
        if(smudges.Count == 0) 
        {
            TriggerCollider.enabled = false;
        } 
        else 
        {
            TriggerCollider.enabled = true;
        }
        
        smudges.RemoveAll(item => item == null);

        // 1. Toggle Cleaning Mode with 'E'
        if (IsPlayerInside && Input.GetKeyDown(KeyCode.E)) 
        {
            isCleaning = !isCleaning;
            if (Wipe != null) Wipe.SetActive(isCleaning);
            
            if (isCleaning) 
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                virtualMousePos = new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);

                if (Player != null) 
                {
                    PlayerMovement movement = Player.GetComponent<PlayerMovement>();
                    if (movement != null) 
                    {
                        movement.ResetLook();
                        movement.enabled = false;
                    }
                }
                if (Camera != null) 
                {
                    Camera.transform.localRotation = Quaternion.identity;
                }
            } 
            else 
            {
                WipeFinish();
            }
        }

        // 2. Force exit cleaning if player leaves zone
        if (!IsPlayerInside && isCleaning) 
        {
            WipeFinish();
        }

        // 3. Move the wipe & force its world rotation to match the window frame (0, -90, 0)
        if (isCleaning && Wipe != null) 
        {
            MoveWipeWithHiddenMouse();
            Wipe.transform.rotation = Quaternion.Euler(0f, -90f, 0f);
        }

        // 4. Smoothly Lerp player to target position
        if (isCleaning && PositionSet != null) 
        {
            Player.transform.position = Vector3.MoveTowards(
                Player.transform.position, 
                PositionSet.transform.position, 
                PositionSpeed * Time.deltaTime
            );

            Player.transform.rotation = Quaternion.RotateTowards(
                Player.transform.rotation, 
                PositionSet.transform.rotation, 
                RotationSpeed * 100f * Time.deltaTime
            );

            if (Camera != null) 
            {
                Camera.transform.localRotation = Quaternion.identity;
            }

            if (Wipe != null) 
            {
                Wipe.transform.rotation = Quaternion.Euler(0f, -90f, 0f);
            }
        }

        // 5. AUTO-FINISH
        if (isCleaning && smudges.Count == 0) 
        {
            WipeFinish();
            Debug.Log("Finished Wiping!");
        }
    }

    public void OnTriggerEnter(Collider other) 
    {
        if (other.CompareTag("Player")) IsPlayerInside = true;
    }

    public void OnTriggerExit(Collider other) 
    {
        if (other.CompareTag("Player")) IsPlayerInside = false;
    }

    void MoveWipeWithHiddenMouse() 
    {
        float mouseX = Input.GetAxis("Mouse X") * wipeSensitivity * Screen.width;
        float mouseY = Input.GetAxis("Mouse Y") * wipeSensitivity * Screen.height;
        
        virtualMousePos.x += mouseX;
        virtualMousePos.y += mouseY;
        virtualMousePos.x = Mathf.Clamp(virtualMousePos.x, Screen.width * wipeScreenBoundsMin.x, Screen.width * wipeScreenBoundsMax.x);
        virtualMousePos.y = Mathf.Clamp(virtualMousePos.y, Screen.height * wipeScreenBoundsMin.y, Screen.height * wipeScreenBoundsMax.y);

        Camera targetCam = Camera != null ? Camera : Camera.main;
        float distanceToGlass = targetCam.WorldToScreenPoint(transform.position).z;
        virtualMousePos.z = distanceToGlass - glassZOffset;
        
        Vector3 mouseWorldPos = targetCam.ScreenToWorldPoint(virtualMousePos);
        Wipe.transform.position = mouseWorldPos;
        Wipe.transform.rotation = Quaternion.Euler(0f, -90f, 0f);
    }

    void WipeFinish() 
    {
        isCleaning = false;
        if (Wipe != null) Wipe.SetActive(false);
        if (Camera != null) 
        {
            Camera.transform.localRotation = Quaternion.identity;
        }

        if (Player != null) 
        {
            Player.transform.rotation = Quaternion.Euler(0f, -90f, 0f);
            PlayerMovement movement = Player.GetComponent<PlayerMovement>();
            if (movement != null) 
            {
                movement.ResetLook();
                movement.enabled = true;
            }
        }
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
