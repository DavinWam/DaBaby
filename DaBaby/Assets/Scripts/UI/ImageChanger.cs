using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class ImageChanger : MonoBehaviour
{
    Image image;
    public Sprite bananaSprite;
    public Sprite grapeSprite;
    public Sprite appleSprite;
    public Sprite pearSprite;
    public Sprite eggSprite;
    public Sprite emptySprite;
    public Sprite iPadSprite;
    public Sprite hugSprite;
    void Start()
    {
        image = GetComponent<Image>();
        // Initially hide the food image
        HideImage();
    }

    public void ChangeFoodImage(FoodType foodType)
    {
        switch (foodType)
        {
            case FoodType.Banana:
                SetImage(bananaSprite);
                break;
            case FoodType.Grape:
                SetImage(grapeSprite);
                break;
            case FoodType.Apple:
                SetImage(appleSprite);
                break;
            case FoodType.Pear:
                SetImage(pearSprite);
                break;
            case FoodType.Egg:
                SetImage(eggSprite);
                break;
            case FoodType.Empty:
                SetImage();
                break;
            default:
                // Handle the default case, if needed
                break;
        }
    }
     public void ChangeImage(string want){
        switch (want)
        {
            case "ipad":
                SetImage(iPadSprite);
                break;
            case "hug":
                SetImage(hugSprite);
                break;
        }

     }
    void SetImage(Sprite sprite = null)
    {
        if(sprite == null){
            HideImage();
            return;
        }

        image.sprite = sprite;
        // Set transparency to full (alpha = 1)
        Color imageColor = image.color;
        imageColor.a = 1f;
        image.color = imageColor;
        // Show the food image
        image.gameObject.SetActive(true);
    }

    public void HideImage()
    { 
        // Hide the food image
        Color imageColor = image.color;
        imageColor.a = 0f;
        image.color = imageColor;
    }
}
