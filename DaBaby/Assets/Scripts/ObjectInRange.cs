using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectInRange : MonoBehaviour
{
    public GameObject item;
    public bool isAlreadyOpen;

    void Start()
    {
        isAlreadyOpen = false;
    }

    // Update is called once per frame
    void Update()
    {
        // code derived from https://www.reddit.com/r/Unity3D/comments/jrl85f/detect_if_is_in_range/
        if (inRange() && !isAlreadyOpen)
        {
            rotateOpen();
        }
        else if (!inRange() && isAlreadyOpen) 
        {
            rotateClose();
        }

    }

    bool inRange() {
        Vector3 Offset = item.transform.position - gameObject.transform.position;
        float Distance = Offset.magnitude;

        return (Distance < 3);
    }

    void rotateOpen()
    {
        isAlreadyOpen = true;
        item.transform.rotation = Quaternion.AngleAxis(90, Vector3.down);
    }

    void rotateClose()
    {
        isAlreadyOpen = false;
        item.transform.rotation = Quaternion.AngleAxis(0, Vector3.down);
    }
}
