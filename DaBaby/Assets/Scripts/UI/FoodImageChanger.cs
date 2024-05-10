using UnityEngine;
using UnityEngine.UI;

public class FoodImageChanger : MonoBehaviour
{
    public Image foodImage;
    public Sprite bananaSprite;
    public Sprite grapeSprite;
    public Sprite appleSprite;
    public Sprite pearSprite;
    public Sprite eggSprite;
    public Sprite emptySprite;

    void Start()
    {
        // Initially hide the food image
        HideFoodImage();
    }

    public void ChangeFoodImage(FoodType foodType)
    {
        switch (foodType)
        {
            case FoodType.Banana:
                SetFoodImage(bananaSprite);
                break;
            case FoodType.Grape:
                SetFoodImage(grapeSprite);
                break;
            case FoodType.Apple:
                SetFoodImage(appleSprite);
                break;
            case FoodType.Pear:
                SetFoodImage(pearSprite);
                break;
            case FoodType.Egg:
                SetFoodImage(eggSprite);
                break;
            case FoodType.Empty:
                SetFoodImage(emptySprite);
                break;
            default:
                // Handle the default case, if needed
                break;
        }
    }

    void SetFoodImage(Sprite sprite)
    {
        foodImage.sprite = sprite;
        // Set transparency to full (alpha = 1)
        Color imageColor = foodImage.color;
        imageColor.a = 1f;
        foodImage.color = imageColor;
        // Show the food image
        foodImage.gameObject.SetActive(true);
    }

    public void HideFoodImage()
    {
        // Hide the food image
        Color imageColor = foodImage.color;
        imageColor.a = 0f;
        foodImage.color = imageColor;
    }
}
