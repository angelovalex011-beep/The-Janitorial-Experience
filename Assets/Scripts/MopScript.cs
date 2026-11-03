using UnityEngine;
using UnityEngine.UI;

public class MopScript : MonoBehaviour
{

    public Camera PlayerCamera;
    public float raycastDistance = 3f;
        // The time required for the log to appear (in seconds)
    public float requiredHoldTime = 5.0f;
    // A timer to track how long the key has been held
    public float holdTimer = 0.0f;
      public GameObject  uiSlider; // Drag your slider GameObject here

      public GameObject TargetMop; // Assign this in the Unity Inspector
private Animator targetAnimator;
    void Start()
    {
          targetAnimator = TargetMop.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
             uiSlider.GetComponent<Slider>().value = holdTimer;
        if (Input.GetKey(KeyCode.E))
        {
                  TargetMop.SetActive(true);
                  targetAnimator.SetBool("PlayAnimation", true);
                  uiSlider.SetActive(true);
               holdTimer += Time.deltaTime;
          if (holdTimer >= requiredHoldTime){
            int layerMask = LayerMask.GetMask("Destroy");

             // Create a Ray starting from the camera's position and going forward
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit; // Variable to store information about the hit object
        
        if (Physics.Raycast(ray, out hit,raycastDistance, layerMask)){
            // Destroy the object
            Debug.Log("Destroying: " + hit.collider.name); 
            Destroy(hit.collider.gameObject);
            holdTimer = 0.0f;
               uiSlider.SetActive(false); // Hide after destroy
               TargetMop.SetActive(false);
            } 
              else
            {
                // Looking at nothing/wrong object
                holdTimer = 0.0f;
                uiSlider.SetActive(false);
                 TargetMop.SetActive(false);
            }
}
}
         else // The 'E' key is not being held down
        {
            // Reset the timer and the log flag
            holdTimer = 0.0f;
             uiSlider.SetActive(false);
              TargetMop.SetActive(false);
}
}

void AnimateMop()
    {
      
    }


}
 
