/***********************************************************************************************************

                                            BABY ANIMATION MANAGER

To use: Place this script on the baby gameobject. Baby starts out in idle animation.

Call the public function SwitchAnimation(int indexNum) to switch the animation to a new one. The baby will 
stay in that animation until a new animation is called.

0 - Idle
1 - Crawling
2 - Angry/Tantrum
3 - Being Held Up
4 - Crying after falling back (fyi the transition from crawling to this is awkward)
5 - Sitting in chair
6 - Eating --- Can only be done when the baby is already sitting
7 - Happy Arm Flap
8 - iPad watching (also has awkward transition to crawling)
9 - Waddle

***********************************************************************************************************/

using System;
using System.Collections;
using UnityEngine;

public class BabyAnimationController : MonoBehaviour
{
    enum Faces
    {
        Neutral,
        Angry,
        Crying,
        EatingClosed,
        EatingOpen,
        Happy
    }


    Faces currentFace = Faces.Neutral;
    Animator animator;
    public Material[] faceMaterials;
    public GameObject head, hand, iPad;
    Renderer headRenderer;
    GameObject ipadInScene = null;
    Coroutine iPadCoroutine;

    // Start is called before the first frame update
    void Awake()
    {
        animator = GetComponent<Animator>();
        headRenderer = head.GetComponent<Renderer>();
        StartCoroutine(BlinkingControl());
    }
    public int SwitchAnimation(int indexNum)
    {
        animator.SetInteger("State", indexNum);
        int anim = -1;
        switch (indexNum)
        {
            //Neutrals
            case 0:
                anim = 0;
                break;
            case 1:
                anim = 1;
                break;
            case 9:
                currentFace = Faces.Neutral;
                headRenderer.material = faceMaterials[9];
                anim = 9;
                break;
            //Angries
            case 2:
                currentFace = Faces.Angry;
                headRenderer.material = faceMaterials[0];
                anim = 2;
                break;
            //Crying
            case 4:
                currentFace = Faces.Crying;
                headRenderer.material = faceMaterials[2];
                anim = 4;
                break;
            //EatingOpen
            case 5:
                currentFace = Faces.EatingOpen;
                headRenderer.material = faceMaterials[5];
                anim = 5;
                break;
            //EatingClosed
            case 6:
                currentFace = Faces.EatingClosed;
                headRenderer.material = faceMaterials[3];
                anim = 6;
                break;
            //Happy
            case 3:
                anim = 3;
                break;
            case 7:
                anim = 7;
                break;
            case 8:
                currentFace = Faces.Happy;
                headRenderer.material = faceMaterials[7];
                anim = 8;
                break;
        }

        //handle iPadCreation
        if (indexNum == 8 && ipadInScene == null)
        {
            iPadCoroutine = StartCoroutine(IPadTracking());

        }
        else if (indexNum != 8 && ipadInScene != null)
        {
            StartCoroutine(IPadExitAnim());
        }
        return anim;
    }

    public void DebugSwitch(string indexNum)
    {
        try
        {
            SwitchAnimation(Int32.Parse(indexNum));
        }
        catch (Exception E)
        {
            print(E);
        }
    }

    IEnumerator BlinkingControl()
    {
        float cooldown = 0;
        while (true)
        {
            cooldown = UnityEngine.Random.Range(3.5f, 4.2f);
            yield return new WaitForSeconds(cooldown);

            switch (currentFace)
            {
                case Faces.Neutral:
                    headRenderer.material = faceMaterials[10];
                    yield return new WaitForSeconds(.2f);
                    headRenderer.material = faceMaterials[9];
                    break;
                case Faces.Angry:
                    headRenderer.material = faceMaterials[1];
                    yield return new WaitForSeconds(.2f);
                    headRenderer.material = faceMaterials[0];
                    break;
                case Faces.EatingOpen:
                    headRenderer.material = faceMaterials[6];
                    yield return new WaitForSeconds(.2f);
                    headRenderer.material = faceMaterials[5];
                    break;
                case Faces.EatingClosed:
                    headRenderer.material = faceMaterials[4];
                    yield return new WaitForSeconds(.2f);
                    headRenderer.material = faceMaterials[3];
                    break;
                case Faces.Happy:
                    headRenderer.material = faceMaterials[8];
                    yield return new WaitForSeconds(.2f);
                    headRenderer.material = faceMaterials[7];
                    break;
            }
                
        }
    }

    IEnumerator IPadTracking()
    {
        while (!(animator.GetCurrentAnimatorStateInfo(0).IsName("idle2")))
        {
            yield return null;
        }

        yield return new WaitForSeconds(1.5f);
        ipadInScene = Instantiate(iPad);
        while (true)
        {
            ipadInScene.transform.position = hand.transform.position;
            yield return null;
        }
    }
    IEnumerator IPadExitAnim()
    {
        yield return new WaitForSeconds(1);
        StopCoroutine(iPadCoroutine);
        Destroy(ipadInScene);
    }
}
