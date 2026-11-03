using UnityEngine;

public class ToiletUIRotater : MonoBehaviour
{

        [Tooltip("Speed of rotation in degrees per second")]
        public float rotationspeed = 100f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //Rotate countinoussly on the z axis
        transform.Rotate(new Vector3(0, 0,rotationspeed * Time.deltaTime));
    }
}
