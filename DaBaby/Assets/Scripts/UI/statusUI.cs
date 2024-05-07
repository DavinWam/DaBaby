using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class statusUI : MonoBehaviour
{
   // public status statusScript;
    public Sprite happyState;
    public Sprite neutralState;
    public Sprite madState;
    /*public Sprite statusSprite; */
    public Image statusImage;
    // Start is called before the first frame update
    void Start()
    {
        //statusScript = GameObject.FindGameObjectWithTag("Player").GetComponent<status>(); //get status from baby object
        statusImage = GetComponent<Image>(); //get image
    }

    // Update is called once per frame
    void Update()
    {
/*        if (statusScript != null && statusImage != null) {
            float hunS = statusScript.hungerStatus;
            float hapS = statusScript.happyStatus;
            if ((hunS <= 400.0f && hunS > 300.0f) || (hapS <= 400.0f && hapS > 300.0f)) {
                statusImage.sprite = happyState;
            } else if ((hunS <= 300.0f && hunS > 150.0f) || (hapS <= 300.0f && hapS > 150.0f)) {
                statusImage.sprite = neutralState;
            } else {
                statusImage.sprite = madState;
            }
        }*/
    }
}
