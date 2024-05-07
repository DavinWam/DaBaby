using UnityEngine;

public class EggCooking : MonoBehaviour
{
    public enum EggState
    {
        Raw,
        Cooked,
        Burnt
    }

    public Texture cookedTexture;
    public Texture burntTexture;
    public float timeToCook = 5.0f;
    public float timeToBurn = 10.0f;

    private EggState currentState = EggState.Raw;
    private float cookingTime = 0f;
    private bool isOnPan = false;
    private Renderer eggRenderer;
    private PanCooking pan;
    void Start()
    {
        eggRenderer = GetComponent<Renderer>(); // Get the Renderer component at start
    }

    void Update()
    {
        
        if (isOnPan && pan && pan.isOnStove)
        {
            cookingTime += Time.deltaTime;
            UpdateEggState();
        }
    }

    private void UpdateEggState()
    {
        if (cookingTime >= timeToBurn && currentState != EggState.Burnt)
        {
            currentState = EggState.Burnt;
            TextureChanger.ChangeTexture(eggRenderer, burntTexture);
        }
        else if (cookingTime >= timeToCook && currentState == EggState.Raw)
        {
            currentState = EggState.Cooked;
            TextureChanger.ChangeTexture(eggRenderer, cookedTexture);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Pan")
        {
            isOnPan = true;
            if( collision.gameObject.GetComponent<PanCooking>()){
                pan = collision.gameObject.GetComponent<PanCooking>();
            }

            // Set the current object as a child of the pan
           // transform.SetParent(pan.transform);

            Debug.Log("Egg placed on the pan.");
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.tag == "Pan")
        {
            isOnPan = false;
            Debug.Log("Egg removed from the pan.");
            cookingTime = 0f; // Reset cooking time when egg is removed

            // Remove the current object from being a child of the pan
         //   transform.parent = null;
            // No need to update the texture to raw as it's assumed to be the default
        }
    }

}
