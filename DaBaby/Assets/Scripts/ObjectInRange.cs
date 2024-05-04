using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectInRange : MonoBehaviour
{
    public GameObject fridgeDoor, trashLid;
    public bool isAlreadyOpenFridge, isAlreadyOpenTrash;

    void Start()
    {
        isAlreadyOpenFridge = false;
        isAlreadyOpenTrash = false;
    }

    // Update is called once per frame
    void Update()
    {
        // code derived from https://www.reddit.com/r/Unity3D/comments/jrl85f/detect_if_is_in_range/
        if (inRangeFridge() && !isAlreadyOpenFridge)
        {
            rotateOpenFridge();
        }
        else if (!inRangeFridge() && isAlreadyOpenFridge) 
        {
            rotateCloseFridge();
        }

        if (inRangeTrash() && !isAlreadyOpenTrash)
        {
            rotateOpenTrash();
        }
        else if (!inRangeTrash() && isAlreadyOpenTrash) 
        {
            rotateCloseTrash();
        }

    }

    bool inRangeFridge() {
        Vector3 Offset = fridgeDoor.transform.position - gameObject.transform.position;
        float Distance = Offset.magnitude;

        return (Distance < 3);
    }

    void rotateOpenFridge()
    {
        isAlreadyOpenFridge = true;
        fridgeDoor.transform.rotation = Quaternion.AngleAxis(90, Vector3.down);
    }

    void rotateCloseFridge()
    {
        isAlreadyOpenFridge = false;
        fridgeDoor.transform.rotation = Quaternion.AngleAxis(0, Vector3.down);
    }

    bool inRangeTrash() {
        Vector3 Offset = trashLid.transform.position - gameObject.transform.position;
        float Distance = Offset.magnitude;

        return (Distance < 2);
    }

    void rotateOpenTrash()
    {
        isAlreadyOpenTrash = true;
        trashLid.transform.rotation = Quaternion.AngleAxis(90, Vector3.left);
    }

    void rotateCloseTrash()
    {
        isAlreadyOpenTrash = false;
        trashLid.transform.rotation = Quaternion.AngleAxis(180, Vector3.left);
    }
}
