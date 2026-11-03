using UnityEngine;

public class SmudgeWipe : MonoBehaviour
{
    public GameObject Wipe;

    void Awake()
    {
        // Automatically find the wipe if it isn't manually assigned
        if (Wipe == null)
        {
            // Method 1: Find by Name in the scene
            Wipe = GameObject.Find("Wipe");

            // Method 2 (Alternative): Find by Tag if you give your Wipe a custom tag
            // Wipe = GameObject.FindGameObjectWithTag("Wipe");

            if (Wipe == null)
            {
                Debug.LogWarning($"SmudgeWipe on {gameObject.name}: Could not find a 'Wipe' object in the scene!");
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // Check if the object entering the trigger is our Wipe object
        if (other.gameObject == Wipe)
        {
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                Color currentColor = sr.color;

                // Subtract a random amount from the current transparency
                currentColor.a -= Random.Range(0.05f, 0.15f);

                // Make sure the transparency does not go lower than 0
                if (currentColor.a <= 0f)
                {
                    currentColor.a = 0f;
                    sr.color = currentColor;
                    
                    Debug.Log("Smudge fully cleaned!");
                    Destroy(gameObject); // Deletes the smudge from the game
                    return; // Stops the rest of the code from running
                }

                sr.color = currentColor;
                Debug.Log("Smudge Wiped. Current alpha: " + currentColor.a);
            }
        }
    }
}
