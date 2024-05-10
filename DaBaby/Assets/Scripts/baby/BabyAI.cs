using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.XR.Interaction.Toolkit;
public class BabyAI : MonoBehaviour
{
    public enum BabyState
    {
        Idle,
        Roaming,
        ShowingNeeds,
        Satiated,
        TargetlessRoaming,
        Hurt,
        seated,
        Hungry,
        Lonely,
        None
    }

    public enum NeedCategory
    {
        Food,
        Attention,
        Activity
    }

    public BabyStatus status;
    public XRDirectInteractor left;
    public XRDirectInteractor right;
    public BabyState currentState;
    private NavMeshAgent agent;
    public float range = 5.0f;
    public float grabRange = 1.5f;  // The reach or grab range of the baby
    private float idleCheckCooldown = 2.5f;
    LayerMask groundLayer;
    LayerMask grabbableLayer;
    List<string> interests = new List<string> { "iPad" };  // Extendable list of interests
    List<string> hazardTags = new List<string> { "Stove", "Knife", "Trash" };  // List of specific hazards
    public Transform leftArm, rightArm;  // Transforms for baby's arms
    private Transform targetObject;  // Target object to move towards
    private NeedCategory need;
    private FoodType wantedFood;
    private string wantedTag;
    private bool gotNeed = false;
    public int desireRating;  // How much the baby wants the target object
    private XRGrabInteractable grabInteractable;
    public bool isBeingHeld = false;
    bool isSeated = false;
    private bool canSit = true;
    public int currentAnimation;
    public Transform seatLocation;
    float timeHeld = 0f;
    float requiredComfortTime = 5f;
    private Rigidbody rb;
    public Transform player;
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        rb = GetComponent<Rigidbody>(); // Get the Rigidbody component
        agent = GetComponent<NavMeshAgent>();
        grabInteractable = GetComponent<XRGrabInteractable>();
        groundLayer = LayerMask.GetMask("Ground");
        grabbableLayer = LayerMask.GetMask("Grabbable");
        StartCoroutine(ChangeState(BabyState.Idle));
        status.hunger = 60;
        status.happiness = 100;
        status.energy = 100;
        status.love = 20;

    }
    void Update()
    {
        status.UpdateStatus();
        //if(objectInLeftHand)  objectInLeftHand.localPosition = Vector3.zero;
        if (objectInLeftHand)
        {
            objectInLeftHand.SetParent(rightArm);
            objectInLeftHand.localPosition = Vector3.zero;
        }
        if (objectInRightHand)
        {
            objectInRightHand.SetParent(rightArm);
            objectInRightHand.localPosition = Vector3.zero;
        }
        if (isBeingHeld)
        {
            // Ensure the player reference is not null
            if (player != null)
            {
                // Get the direction from this object to the player
                Vector3 directionToPlayer = player.position - transform.position;

                // Project the direction onto the XZ plane (ignoring height differences)
                directionToPlayer.y = 0f;

                // Rotate the object to look at the player
                transform.rotation = Quaternion.LookRotation(directionToPlayer.normalized, Vector3.up);
            }
        }
        HandleStates();
    }
    void FixedUpdate()
    {
        // Check if the current velocity exceeds maxVelocity
        if (rb.velocity.magnitude > 10)
        {
            // Cap the velocity at the maximum speed in the same direction
            rb.velocity = rb.velocity.normalized * 10;
        }
    }
    void OnCollisionEnter(Collision collision)
    {
        // Debug.Log(collision.gameObject.name);
        if (collision.gameObject.name == "babySeat" && isBeingHeld && canSit)
        {
            Debug.Log("seated");
            ForceRelease();
            StartCoroutine(ChangeState(BabyState.seated));
        }
        if (currentState == BabyState.ShowingNeeds && collision.gameObject.tag == wantedTag)
        {
            AttachToHand(collision.transform);
            gotNeed = true;
            targetObject = null;
        }
    }
    public void OnGrabbed(SelectEnterEventArgs args)
    {

        agent.enabled = false;
        isBeingHeld = true;
        canSit = false;
        StartCoroutine(CanSit());
        GetComponent<BabyAnimationController>().SwitchAnimation(3);
        // drop any held items if picked up by player
        DropObject(rightArm);
        DropObject(leftArm);
        timeHeld = 0f;
        Debug.Log("Baby has been picked up.");
    }
    private IEnumerator CanSit()
    {
        yield return new WaitForSeconds(.5f);
        canSit = true;
    }
    public void OnReleased(SelectExitEventArgs args)
    {
        agent.enabled = true;
        isBeingHeld = false;
        GetComponent<BabyAnimationController>().SwitchAnimation(currentAnimation);
        Debug.Log("you let go of baby.");

    }
    void HandleLonely()
    {
        status.shouldDecay = true;
        if (isBeingHeld)
        {
            timeHeld += Time.deltaTime;
            status.shouldDecay = false;
            Debug.Log(timeHeld);
        }
        if (timeHeld > 10f)
        {
            timeHeld = 0f;
            audioSource.clip = laughClips[1];
            audioSource.Play();
            StartCoroutine(ChangeState(BabyState.Satiated));
        }
    }
    void HandleStates()
    {
        switch (currentState)
        {
            case BabyState.Idle:
                HandleIdle();
                break;
            case BabyState.Roaming:
                HandleRoaming();
                break;
            case BabyState.TargetlessRoaming:
                HandleTargetlessRoaming();
                break;
            case BabyState.Hungry:
                HandleHungry();
                break;
            case BabyState.ShowingNeeds:
                HandleShowingNeeds();
                break;
            case BabyState.Satiated:
                HandleSatiated();
                break;
            case BabyState.Hurt:
                HandleHurt();
                break;
            case BabyState.seated:
                HandleSeated();
                break;
            case BabyState.Lonely:
                HandleLonely();
                break;
        }
    }
    IEnumerator WaitAndChangeState(BabyState newState, float delay)
    {
        yield return new WaitForSeconds(delay);
        yield return StartCoroutine(ChangeState(newState));
    }
    public ImageChanger imageChanger;
    IEnumerator ChangeState(BabyState newState, Transform target = null)
    {
        Debug.Log("changingstate");
        // Cancel any repeating invocations when changing states to avoid overlap of behaviors
        CancelInvoke();
        GetComponent<CapsuleCollider>().height = .6f;
        isSeated = false;
        
        isTalking = false;
        currentState = BabyState.None;


        //        Debug.Log(newState);
        if (newState == BabyState.Idle)
        {
            // Start the coroutine for playing audio periodically
            StartCoroutine(PlayAudioPeriodically());
            imageChanger.HideImage();
            if (status.energy >= 50)
            {
                currentAnimation = GetComponent<BabyAnimationController>().SwitchAnimation(0);
            }
            else
            {
                currentAnimation = GetComponent<BabyAnimationController>().SwitchAnimation(9);
            }

            yield return new WaitForSeconds(5f);
        }
        else if (newState == BabyState.Roaming)
        {
            Debug.Log("changing to roaming");

            currentAnimation = GetComponent<BabyAnimationController>().SwitchAnimation(1);
            yield return new WaitForSeconds(5f);
            imageChanger.ChangeImage("ipad");
        }
        else if (newState == BabyState.TargetlessRoaming)
        {
            // Start the coroutine for playing audio periodically
            StartCoroutine(PlayAudioPeriodically());
            imageChanger.HideImage();
            currentAnimation = GetComponent<BabyAnimationController>().SwitchAnimation(1);
            yield return new WaitForSeconds(5f);
        }

        else if (newState == BabyState.Hungry)
        {
            audioSource.clip = cryClips[0];
            audioSource.loop = true;
            audioSource.Play();
            currentAnimation = GetComponent<BabyAnimationController>().SwitchAnimation(2);
            // Update want icon
            imageChanger.ChangeFoodImage(wantedFood);
        }
        else if (newState == BabyState.ShowingNeeds)
        {
            audioSource.clip =  cryClips[2];
            
            audioSource.Play();
            Debug.Log("Needs: " + targetObject.name + " brought over.");
            currentAnimation = GetComponent<BabyAnimationController>().SwitchAnimation(2);
        }
        else if (newState == BabyState.Satiated)
        {
            Debug.Log("gotwanted " + wantedTag);
            if (wantedTag == "iPad")
            {
                Debug.Log(objectInRightHand);
                currentAnimation = GetComponent<BabyAnimationController>().SwitchAnimation(8);
            }
            else
            {
                currentAnimation = GetComponent<BabyAnimationController>().SwitchAnimation(7);
            }
            yield return new WaitForSeconds(5f);
        }
        else if (newState == BabyState.Hurt)
        {
            currentAnimation = GetComponent<BabyAnimationController>().SwitchAnimation(4);
        }
        else if (newState == BabyState.seated)
        {
            currentAnimation = GetComponent<BabyAnimationController>().SwitchAnimation(5);
        }
        else if (newState == BabyState.Lonely)
        {
            audioSource.clip =  cryClips[1];
            audioSource.loop = true;
            audioSource.Play();
            currentAnimation = GetComponent<BabyAnimationController>().SwitchAnimation(4);
            imageChanger.ChangeImage("hug");
        }


        currentState = newState;
        switch (currentState)
        {
            case BabyState.Idle:
                agent.isStopped = true;
                isTalking = true;
                break;
            case BabyState.Roaming:
                agent.isStopped = false;  // Stop the baby from moving
                isTalking = true;
                break;
            case BabyState.TargetlessRoaming:
                agent.isStopped = false;  // Stop the baby from moving
                isTalking = true;
                break;
            case BabyState.Hungry:
                agent.isStopped = true;  // Stop the baby from moving
                break;
            case BabyState.ShowingNeeds:
                agent.isStopped = true;  // Stop the baby from moving;
                break;
            case BabyState.Satiated:
                if (isBeingHeld)
                {
                    ForceRelease();
                }
                status.love += 12;
                agent.isStopped = true;  // Stop the baby from moving
                break;
            case BabyState.Hurt:
                agent.isStopped = true;  // Stop the baby from moving
                break;
            case BabyState.seated:
                agent.isStopped = true;  // Stop the baby from moving
                break;
            case BabyState.Lonely:
                agent.isStopped = true;
                break;
        }

        targetObject = target;  // Set or reset the target object when changing states
        if (targetObject != null)
        {
            desireRating = EvaluateDesire(targetObject);  // Evaluate how much the baby wants this object
        }

        yield return null;
    }
    int EvaluateDesire(Transform target)
    {
        // Simple random desire rating for demonstration; customize this based on game logic
        return Random.Range(1, 101);
    }
    void ShuffleColliders(Collider[] colliders)
    {
        for (int i = colliders.Length - 1; i > 0; i--)
        {
            int rnd = Random.Range(0, i + 1);
            Collider temp = colliders[i];
            colliders[i] = colliders[rnd];
            colliders[rnd] = temp;
        }
    }
    void LookForObjectsOfInterest()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, range);
        // Randomize the order of hitColliders
        ShuffleColliders(hitColliders);

        NeedCategory targetCategory = DetermineNeedCategory(); // Determine the current need based on status
        if (targetCategory == NeedCategory.Attention)
        {
            StartCoroutine(ChangeState(BabyState.Lonely));
        }
        else
        {
            foreach (var hitCollider in hitColliders)
            {
                if (IsValidTarget(hitCollider, targetCategory))
                {
                    Debug.Log("Found " + targetCategory + ": " + hitCollider.name);
                    if (isBeingHeld) { ForceRelease(); }

                    switch (targetCategory)
                    {
                        case NeedCategory.Activity:
                            wantedTag = hitCollider.tag;
                            StartCoroutine(ChangeState(BabyState.Roaming, hitCollider.transform));  // Change to roaming and set the target
                            break;
                        case NeedCategory.Food:
                            wantedFood = hitCollider.gameObject.GetComponent<Fruit>().foodType;
                            StartCoroutine(ChangeState(BabyState.Hungry, hitCollider.transform));
                            break;
                    }
                    return;
                }
            }
        }

    }

    NeedCategory DetermineNeedCategory()
    {
        // Define total weight for normalization
        float totalWeight = status.hunger + status.love + (100 - status.energy) + status.happiness;
        float hungerWeight = (100 - status.energy / 2) / totalWeight;
        float loveWeight = status.love / totalWeight;
        float energyWeight = (100 - status.energy) / totalWeight;
        float happinessWeight = status.happiness / totalWeight;

        // Compute the probabilities for each need category
        float foodProbability = hungerWeight;
        float attentionProbability = loveWeight;
        float activityProbability = (happinessWeight + energyWeight) / 3; // Adjusting for energy and happiness

        // Get a random value between 0 and 1
        float randomValue = Random.value;

        NeedCategory need;

        // Determine the need category based on weighted probabilities
        if (randomValue < foodProbability)
        {
            need = NeedCategory.Food;
        }
        else if (randomValue < attentionProbability)
        {
            need = NeedCategory.Attention;
        }
        else if (randomValue < activityProbability)
        {
            need = NeedCategory.Activity;
        }
        else
        {
            // Select the need category with the highest probability
            float maxProbability = Mathf.Max(foodProbability, attentionProbability, activityProbability);
            if (maxProbability == foodProbability)
            {
                need = NeedCategory.Food;
            }
            else if (maxProbability == attentionProbability)
            {
                need = NeedCategory.Attention;
            }
            else
            {
                need = NeedCategory.Activity;
            }
        }
        float sum = foodProbability + attentionProbability + activityProbability;
        // Print probabilities for debugging purposes
        Debug.Log("Food Probability: " + foodProbability/sum);
        Debug.Log("Attention Probability: " + attentionProbability/sum);
        Debug.Log("Activity Probability: " + activityProbability/sum);

        return need;
    }


    // Check if the collider is a valid target for the current need
    bool IsValidTarget(Collider collider, NeedCategory category)
    {
        switch (category)
        {
            case NeedCategory.Food:
                return collider.CompareTag("Food");
            case NeedCategory.Attention:
                return collider.CompareTag("Player") || collider.CompareTag("Toy"); // Assuming "Parent" tag represents caregiver
            case NeedCategory.Activity:
                return interests.Contains(collider.tag); //|| hazardTags.Contains(collider.tag) || (1 << collider.gameObject.layer) & (LayerMask.GetMask("Grabbable")) != 0 ||;
            default:
                return false;
        }
    }


    void HandleIdle()
    {
        if (!isBeingHeld)
        {
            idleCheckCooldown -= Time.deltaTime;

            if (idleCheckCooldown <= 0)
            {
                Debug.Log("done idling");
                LookForObjectsOfInterest();
                idleCheckCooldown = 8f/ ((200 - status.energy) / 100);  // Reset cooldown
            }
        }

    }
    void HandleSeated()
    {
        if (isBeingHeld) StartCoroutine(ChangeState(BabyState.Idle));


        isSeated = true;
        transform.position = seatLocation.position;
        transform.rotation = Quaternion.Euler(0, 180, 0);
        GetComponent<CapsuleCollider>().height = .8f;

    }
    void MoveToClosestReachablePoint(Vector3 targetPosition)
    {
        NavMeshPath path = new NavMeshPath();
        agent.CalculatePath(targetPosition, path);

        if (agent.hasPath)
        {
            agent.stoppingDistance = .5f;  // Normal stopping distance
            agent.SetDestination(targetPosition);
            //            Debug.Log("Path complete, agent can reach the target.");
        }
        else
        {
            // Find the closest reachable point on the path
            if (path.corners.Length > 0)
            {
                agent.stoppingDistance = .8f;  // longer stopping distance since the location isn't exact
                Vector3 closestReachablePoint = path.corners[path.corners.Length - 1];
                agent.SetDestination(closestReachablePoint);
                Debug.Log("Path incomplete. Moving to the closest reachable point: " + closestReachablePoint);
            }
            else
            {
                Debug.Log("No path to the target could be found.");
            }
        }
    }


    // Class level variables
    private float timeSpentTryingToReach = 0f;
    private float maxTimeToReach = 200f;  // Maximum time in seconds to try reaching an object

    void HandleRoaming()
    {
        if (isBeingHeld)
        {
            agent.isStopped = true;
            Debug.Log("grabbed while roaming");
            if (targetObject != null)
            {
                InvokeRepeating(nameof(AttemptBreakAway), 5f, 5f);
            }
            else
            {
                imageChanger.HideImage();
                StartCoroutine(ChangeState(BabyState.Idle));
            }
            return; // Stop further processing to handle current interaction
        }
        else
        {
            agent.isStopped = false;
        }

        if (targetObject != null)
        {

            Vector3 targetPosition = targetObject.position;
            // Debug.Log("object position " + targetPosition + " from position: " + gameObject.transform.position);
            MoveToClosestReachablePoint(targetPosition);

            // Debug.Log("Moving towards target " + agent.destination + " from position: " + gameObject.transform.position);


            // Debug.Log("Path Status: " + agent.pathStatus);
            // Debug.Log("Is Path Calculated: " + agent.hasPath);
            // Debug.Log("Is Agent Stopped: " + agent.isStopped);


            // Debug.Log("remmaining: "+ agent.remainingDistance);

            // Debug.Log("stopping: " + agent.stoppingDistance + " \n" +(agent.remainingDistance > agent.stoppingDistance) );
            if (agent.remainingDistance > agent.stoppingDistance)
            {


                //Debug.Log("time:" + timeSpentTryingToReach);
                // Update the time spent trying regardless of the object's immediate reachability
                if (timeSpentTryingToReach < maxTimeToReach)
                {
                    timeSpentTryingToReach += Time.deltaTime;  // Update time spent trying
                }
                else
                {
                    // If the time spent trying exceeds the maximum time, change state to ShowingNeeds
                    Debug.Log("Took too long to reach " + targetObject.name + ", now showing needs.");

                    StartCoroutine(ChangeState(BabyState.ShowingNeeds, targetObject));
                    timeSpentTryingToReach = 0f;  // Resets the timer used to track the duration of reaching attempts
                    return;  // Exit the method to prevent further processing
                }


            }
            else
            {
                float distanceToTarget = Vector3.Distance(transform.position, targetObject.position);
                Debug.Log("distance from want:" + distanceToTarget + " " + (distanceToTarget <= grabRange));
                if (distanceToTarget <= grabRange)
                {
                    if (((1 << targetObject.gameObject.layer) & grabbableLayer) != 0)
                    {
                        Debug.Log("baby picked up an object");
                        // Object is within reach and is grabbable, attach it to an available hand
                        AttachToHand(targetObject);
                        imageChanger.HideImage();
                        audioSource.clip =  laughClips[2];
                        audioSource.Play();
                        StartCoroutine(ChangeState(BabyState.Satiated));
                        timeSpentTryingToReach = 0f;  // Resets the timer used to track the duration of reaching attempts
                    }
                    else
                    {
                        Debug.Log("baby found object it likes");
                        // Object is within reach but not in the grabbable layer, inspect then go to targetless roaming
                        InspectObject();  // A method to inspect the object
                        imageChanger.HideImage();
                        audioSource.clip =  laughClips[2];
                        audioSource.Play();
                        StartCoroutine(ChangeState(BabyState.Satiated));
                        timeSpentTryingToReach = 0f;  // Resets the timer used to track the duration of reaching attempts
                    }
                }
                else
                {
                    Debug.Log("Can see but can't reach " + targetObject.name);
                    Debug.Log("Does baby really want object:" + ((desireRating) * (status.happiness / 100f) > 30));
                    // Object is visible but out of reach
                    if ((desireRating) * (status.happiness / 100f) > 30)  // High desire, decide on action, baby is more likely to pout if it's sad
                    {
                        Debug.Log("baby can't reach object so is asking for " + targetObject.gameObject.name);
                        StartCoroutine(ChangeState(BabyState.ShowingNeeds, targetObject));
                        timeSpentTryingToReach = 0f;  // Resets the timer used to track the duration of reaching attempts
                    }
                    else
                    {
                        Debug.Log("lost interest and is now looking around");
                        imageChanger.HideImage();
                        StartCoroutine(ChangeState(BabyState.TargetlessRoaming));
                        timeSpentTryingToReach = 0f;  // Resets the timer used to track the duration of reaching attempts
                    }
                }
            }
        }
        else
        {
            imageChanger.HideImage();
            StartCoroutine(ChangeState(BabyState.TargetlessRoaming));
        }
    }
    void AttemptBreakAway()
    {
        Debug.Log("Baby attempts to break away.");
        GetComponent<BabyAnimationController>().SwitchAnimation(2); // Play specific animation for breaking away
        StartCoroutine(DelayedRelease());
    }
    IEnumerator DelayedRelease()
    {
        currentAnimation = GetComponent<BabyAnimationController>().SwitchAnimation(2);
        yield return new WaitForSeconds(4f);
        ForceRelease();
        currentAnimation = GetComponent<BabyAnimationController>().SwitchAnimation(1);
        yield return new WaitForSeconds(4f);
    }
    void ForceRelease()
    {
        // Code to force release the baby

        if (isBeingHeld)
        {
            grabInteractable.enabled = false;
            isBeingHeld = false;
            Debug.Log("Forced release of the baby.");
            agent.enabled = true;
            StartCoroutine(EnableGrabInteractable());
            //Invoke("EnableGrabInteractable", 2.0f); // Re-enable after 1 second or appropriate time
        }

    }

    IEnumerator EnableGrabInteractable()
    {
        yield return new WaitForSeconds(2f);
        grabInteractable.enabled = true;
        CancelInvoke(nameof(AttemptBreakAway)); // Cancel the repeating break away attempts when released
        Debug.Log("baby can be grabbed again");
    }

    void InspectObject()
    {
        Debug.Log("Inspecting " + targetObject.name);
        // Add any specific inspection behaviors here
    }


    // Class-level variables to store references to the objects each hand is currently holding
    public Transform objectInRightHand;
    public Transform objectInLeftHand;

    void AttachToHand(Transform objectToGrab)
    {
        // Check if either hand is free or decide which object to drop
        Transform hand = rightArm;
        if (objectInRightHand == null)
        {
            hand = rightArm;
            objectInRightHand = objectToGrab;
        }
        else if (objectInLeftHand == null)
        {
            hand = leftArm;
            objectInLeftHand = objectToGrab;
        }
        else
        {
            // Both hands are occupied, decide to drop the right hand's object
            DropObject(rightArm);
            hand = rightArm;
            objectInRightHand = objectToGrab;
        }

        Rigidbody rb = objectToGrab.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true; // Make the object not respond to physics
            rb.velocity = Vector3.zero; // Stop linear velocity
            rb.angularVelocity = Vector3.zero; // Stop angular velocity
        }
        Debug.Log(objectToGrab.gameObject.name);

        // Attach the new object to the selected hand
        objectToGrab.SetParent(hand);
        objectToGrab.localPosition = Vector3.zero; // Set the local position to zero
        Debug.Log(objectToGrab.transform.parent.name);

    }
    void DropObject(Transform hand)
    {
        Transform objectToDrop = (hand == rightArm) ? objectInRightHand : objectInLeftHand;

        if (objectToDrop != null)
        {
            objectToDrop.SetParent(null); // Detach the object from the hand
            Rigidbody rb = objectToDrop.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false; // Make the object respond to physics again
                rb.velocity = Vector3.zero; // Stop linear velocity
                rb.angularVelocity = Vector3.zero; // Stop angular velocity
            }

            // Clear the reference
            if (hand == rightArm)
            {
                objectInRightHand = null;
            }
            else
            {
                objectInLeftHand = null;
            }
        }
    }

    void HandleTargetlessRoaming()
    {
        InvokeRepeating(nameof(RandomMovement), 2f, 5f);
        if (!agent.hasPath || agent.remainingDistance < 0.5f)
        {
            // Randomly move to a new position on the NavMesh within a defined range
            Vector3 randomDirection = Random.insideUnitSphere * range;
            randomDirection += transform.position;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomDirection, out hit, range, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
            }
        }

        // Periodically check for objects of interest
        idleCheckCooldown -= Time.deltaTime;
        if (idleCheckCooldown <= 0)
        {
            LookForObjectsOfInterest();
            idleCheckCooldown = 12f / ((200 - status.energy) / 100);  // Reset cooldown based on energy
        }
    }
    void RandomMovement()
    {
        Vector3 randomDirection = Random.insideUnitSphere * range;
        randomDirection += transform.position;
        NavMeshHit hit;
        NavMesh.SamplePosition(randomDirection, out hit, range, 1);
        agent.SetDestination(hit.position);
    }

    void HandleHungry()
    {
        //does nothing for now but should decrease status more
    }
    public bool Eat(FoodType spoonFood)
    {
        //decrease hunger if the correct food was given

        if (isSeated)
        {
            if (spoonFood == wantedFood)
            {
                GetComponent<BabyAnimationController>().SwitchAnimation(6);
                status.hunger += 30;
                imageChanger.HideImage();
                audioSource.loop = false;
                audioSource.clip = laughClips[0];
                audioSource.Play();
                StartCoroutine(WaitAndChangeState(BabyState.Satiated, 5f));
            }
            else
            {
                GetComponent<BabyAnimationController>().SwitchAnimation(2);
                status.hunger += 15;
                imageChanger.HideImage();
                audioSource.loop = false;
                audioSource.clip = talkClips[3];
                audioSource.Play();
                StartCoroutine(WaitAndChangeState(BabyState.Idle, 5f));
            }            
            
            return true;
        }
        else
        {
            return false;
        }

    }

    void HandleShowingNeeds()
    {

        // Check if the correct object is brought close enough to interact
        if (gotNeed)
        {
            Debug.Log("Correct object brought close. Now Satiated.");
            status.happiness += 20;
            audioSource.clip =  laughClips[2];
            audioSource.loop = false;
            audioSource.Play();
            StartCoroutine(ChangeState(BabyState.Satiated));  // Change state to Satiated
        }
    }
    void HandleHurt()
    {

        if (isBeingHeld)
        {
            timeHeld += Time.deltaTime;  // Update the time held

            if (timeHeld > requiredComfortTime)
            {
                Debug.Log("Baby comforted enough. Now Satiated.");
                StartCoroutine(ChangeState(BabyState.Satiated));
                ForceRelease();  // Force release the baby
            }
        }
    }

    void HandleSatiated()
    {
        audioSource.loop = true;
        wantedTag = null;
        wantedFood = FoodType.Empty;
        DropObject(rightArm);
        DropObject(leftArm);
        status.shouldDecay = false; // Stop the status decay
        StartCoroutine(EndSatiated());
    }

    IEnumerator EndSatiated()
    {
        yield return new WaitForSeconds(5f);
        status.shouldDecay = true; // Resume status decay
        status.energy += 15;
        audioSource.loop = false;
        yield return StartCoroutine(ChangeState(BabyState.TargetlessRoaming)); // Return to Idle state
    }

    public List<AudioClip> talkClips;
    public List<AudioClip> laughClips;
    public List<AudioClip> cryClips;
    public float intervalMin = 5f;
    public float intervalMax = 10f;
    public bool isTalking = false;
    private AudioSource audioSource;
    
    IEnumerator PlayAudioPeriodically()
    {
        while (isTalking)
        {
            // Wait for a random interval before playing the next audio
            yield return new WaitForSeconds(Random.Range(intervalMin, intervalMax));

            // Select a random audio clip from the list
            AudioClip clipToPlay = talkClips[Random.Range(0, talkClips.Count)];

            // Play the selected audio clip
            audioSource.clip = clipToPlay;
            audioSource.Play();
        }
    }
}
