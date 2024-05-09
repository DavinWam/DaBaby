using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class statusUI : MonoBehaviour
{
    public BabyStatus statusScript;
    public Sprite happyState;
    public Sprite neutralState;
    public Sprite madState;
    /*public Sprite statusSprite; */
    public Image statusImage;
    // Start is called before the first frame update
    void Start()
    {
        statusImage = GetComponent<Image>(); //get image
    }

    // Update is called once per frame
    void Update()
    {
        if (statusScript != null && statusImage != null) {
            float overallStat = statusScript.overallStatus;
            if (overallStat <= 100f && overallStat > 75f) {
                statusImage.sprite = happyState;
            } else if (overallStat <= 75f && overallStat > 25f) {
                statusImage.sprite = neutralState;
            } else {
                statusImage.sprite = madState;
            }
        }
    }
}
