using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class babyStab : MonoBehaviour
{
    public GameObject baseObj;
    // Start is called before the first frame update
    void Start()
    {
        TurnAllAnimtorsOff(baseObj.transform, false);
        StartCoroutine(Death());
    }

    // Update is called once per frame
    IEnumerator Death()
    {
        yield return new WaitForSeconds(.1f);
        TurnAllAnimtorsOff(baseObj.transform, true);
    }
    private void TurnAllAnimtorsOff(Transform root, bool condition)
    {
        Rigidbody[] animators = root.GetComponentsInChildren<Rigidbody>();

        foreach (Rigidbody a in animators)
        {
            a.useGravity = condition;
        }
    }
}
