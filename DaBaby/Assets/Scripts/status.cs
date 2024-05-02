using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Receiver.Primitives;

public class status : MonoBehaviour
{
    public float hungerStatus = 400.0f;
    public float happyStatus = 400.0f;
    public float hungerDrain = 0.01f;
    public float happyDrain = 0.01f;

    // Start is called before the first frame update
/*    void Start()
    {
        
    }*/

    // Update is called once per frame
    void Update()
    {
        /*needs checks for feeding or emotional support based on public booleans vars from other scripts*/
        hungerStatus = hungerStatus - hungerDrain;
        happyStatus = happyStatus - happyDrain;
    }
}
