using UnityEngine;

public class PointerTrigger : MonoBehaviour
{

    public BoxCollider2D PointerCollider;
    public int ToiletScore;
    private bool IsPointerInside = false;
    public GameObject PointerRotate;
       public Animator PlayerAnimator;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if(IsPointerInside == true)
        {
            if (Input.GetKeyDown(KeyCode.Space))
        {
            Interact();
                PlayerAnimator.SetTrigger("UnClog");

        }
        }
        else
        {
              if(IsPointerInside == false)
            {
                 if (Input.GetKeyDown(KeyCode.Space))
                {
                    Debug.Log("Didn't Hit");
                      ToiletScore--;
                }
            }
        }

        if(ToiletScore == 15)
        {
            PlayerAnimator.SetBool("ClogFinish", true);
        }

    }

    void OnTriggerEnter2D(Collider2D PointerColider)
    {
        IsPointerInside = true;
    }

    
    void OnTriggerExit2D(Collider2D PointerColider)
    {
        IsPointerInside = false;
    }

     void Interact()
    {
          Debug.Log("Pointer Hit");

        PointerRotate.transform.Rotate(0, 0, Random.Range(0f, 360f));
          ToiletScore++;


    }




}
