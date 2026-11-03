using UnityEngine;

public class WaterTrigger : MonoBehaviour
{

      public MopScript MopScript; 
       public GameObject uiSlider;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
            GameObject targetGameObject = GameObject.Find("Player");
              MopScript = targetGameObject.GetComponent<MopScript>();
              uiSlider = GameObject.Find("Mop Proggress Canvas"); 
              
                 

    }

    // Update is called once per frame
    void Update()
    {
          if (MopScript != null) {
        // Access it like this:
       // Debug.Log("Current Hold Time: " + MopScript.holdTimer);
    }
    }

    void OnTriggerEnter(Collider other) 
    {
         if (other.CompareTag("Player"))
    {
      MopScript.enabled = true;
      uiSlider.SetActive(true);
    }
    }

     void OnTriggerExit(Collider other) 
    {
         if (other.CompareTag("Player"))
    {
        MopScript.holdTimer = 0.0f;
        MopScript.enabled = false;
        uiSlider.SetActive(false);
    }
    }

     void OnDestroy()
        {
        MopScript.holdTimer = 0.0f;
           if (MopScript != null) {
        MopScript.enabled = false;
        uiSlider.SetActive(false);
        }
}
}


