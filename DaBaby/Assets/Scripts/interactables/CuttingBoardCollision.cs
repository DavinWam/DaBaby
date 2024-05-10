using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class CuttingBoardCollision : MonoBehaviour
{
    private GameObject fruit;
    public Transform spawnCenter;
    public float offsetHeight = 0.1f;
    Rigidbody rb;
    public void Start(){
        rb = gameObject.GetComponent<Rigidbody>();
    }
    public void Update(){
        if(fruit) fruit.transform.parent = spawnCenter;
    }
    void OnCollisionEnter(Collision collision)
    {
        Fruit fruitComponent = collision.gameObject.GetComponent<Fruit>();
        if (fruitComponent != null && collision.gameObject.CompareTag("Food") && !fruit && rb.velocity.magnitude < 0.02f)
        {
            fruit = collision.gameObject;


            // Disable interaction and make rigidbody kinematic
            XRGrabInteractable grabInteractable = collision.gameObject.GetComponent<XRGrabInteractable>();
            if (grabInteractable != null)
            {
                grabInteractable.enabled = false;
            }
            // Move and re-parent the fruit
            collision.gameObject.transform.position = spawnCenter.position + Vector3.up * offsetHeight;
            collision.gameObject.transform.parent = spawnCenter;
            
            // Set the rotation specified in the Fruit component
            collision.gameObject.transform.rotation = fruitComponent.desiredRotation;

            Rigidbody rb = collision.gameObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
                rb.useGravity = false;
            }
        }
    }
}
