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
        Hurt  // State when the baby has hurt itself trying to reach something
    }

    public enum NeedCategory
    {
        Food,
        Attention,
        Activity
    }

    public BabyStatus status;
    public BabyState currentState;
    private NavMeshAgent agent;
    public float range = 5.0f;
    public float grabRange = 1.5f;  // The reach or grab range of the baby
    private float idleCheckCooldown = 5f;
    LayerMask groundLayer;
    LayerMask grabbableLayer;
    List<string> interests = new List<string> { "Food", "Toy", "iPad" };  // Extendable list of interests
    List<string> hazardTags = new List<string> { "Stove", "Knife", "Trash" };  // List of specific hazards
    public Transform leftArm, rightArm;  // Transforms for baby's arms
    private Transform targetObject;  // Target object to move towards
    public int desireRating;  // How much the baby wants the target object
    XRGrabInteractable grabInteractable;
    bool isBeingHeld = false;
    float timeHeld = 0f;
    float requiredComfortTime = 5f;
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        groundLayer = LayerMask.GetMask("Ground");
        grabbableLayer = LayerMask.GetMask("Grabbable");
        ChangeState(BabyState.Idle);

        grabInteractable = GetComponent<XRGrabInteractable>();
        grabInteractable.selectEntered.AddListener(OnGrabbed);
        grabInteractable.selectExited.AddListener(OnReleased);
    }

    void OnGrabbed(SelectEnterEventArgs args)
    {
        isBeingHeld = true;
        timeHeld = 0f;  // Reset the time held counter
        Debug.Log("Baby has been picked up.");
    }

    void OnReleased(SelectExitEventArgs args)
    {
        isBeingHeld = false;
        Debug.Log("Baby has been released.");
    }
    void Update()
    {
        status.UpdateStatus();
        if(objectInLeftHand)  objectInLeftHand.localPosition = Vector3.zero;
        if(objectInRightHand) objectInRightHand.localPosition = Vector3.zero;

        HandleStates();
    }
    void HandleShowingNeeds()
    {
        if (!agent.isStopped)
        {
            agent.isStopped = true;  // Stop the baby from moving
            Debug.Log("Needs: " + targetObject.name + " brought over.");
        }

        // Check if the correct object is brought close enough to interact
        if (Vector3.Distance(transform.position, targetObject.position) <= grabRange)
        {
            Debug.Log("Correct object brought close. Now Satiated.");
            ChangeState(BabyState.Satiated);  // Change state to Satiated
        }
    }
    void HandleHurt()
    {
        grabInteractable.enabled = true;
        if (!agent.isStopped)
        {
            agent.isStopped = true;  // Stop the baby from moving
        }

        if (isBeingHeld)
        {
            timeHeld += Time.deltaTime;  // Update the time held

            if (timeHeld > requiredComfortTime)
            {
                Debug.Log("Baby comforted enough. Now Satiated.");
                ChangeState(BabyState.Satiated);
                ForceRelease();  // Force release the baby
            }
        }
    }

    void ForceRelease()
    {
        grabInteractable.enabled = false;
        // if (grabInteractable.isSelected)
        // {
        //     var interactor = grabInteractable.selectingInteractor;
        //     interactor.interactionManager.ForceDeselect(interactor);
        //     Debug.Log("Forced release of the baby.");
        // }
    }

    void HandleStates()
    {
        switch (currentState)
        {
            case BabyState.Idle:
                HandleIdle();
                break;
            case BabyState.TargetlessRoaming:
                HandleTargetlessRoaming();
                break;
            case BabyState.Roaming:
                HandleRoaming();
                break;
            case BabyState.ShowingNeeds:
                HandleShowingNeeds();  // Enhanced to handle showing needs based on various scenarios
                break;
            case BabyState.Satiated:
                HandleSatiated();
                break;
            case BabyState.Hurt:
                HandleHurt();  // Handle behavior when hurt
                break;
        }
    }

    void ChangeState(BabyState newState, Transform target = null)
    {
        // Cancel any repeating invocations when changing states to avoid overlap of behaviors
        CancelInvoke();

        currentState = newState;
        targetObject = target;  // Set or reset the target object when changing states
        if (targetObject != null)
            desireRating = EvaluateDesire(targetObject);  // Evaluate how much the baby wants this object

        switch (newState)
        {
            case BabyState.Roaming:
                if (targetObject != null)
                    agent.SetDestination(targetObject.position);
                break;
            case BabyState.TargetlessRoaming:
                // Invoke random movement periodically, similar to idle but with movement
                InvokeRepeating(nameof(HandleRandomMovement), 2f, 5f);
                break;
                // Implement other states as needed
        }
    }

    void HandleIdle()
    {
        idleCheckCooldown -= Time.deltaTime;
        if (idleCheckCooldown <= 0)
        {
            LookForObjectsOfInterest();
            idleCheckCooldown = 5f / (status.energy / 100);  // Reset cooldown
        }
    }
    NeedCategory DetermineNeedCategory()
    {
        return NeedCategory.Activity;

        float totalWeight = status.hunger + status.happiness + (100 - status.energy); // Define total weight for normalization
        float hungerWeight = status.hunger / totalWeight;
        float happinessWeight = (100 - status.happiness) / totalWeight; // Inverse because lower happiness increases the weight for attention
        float energyWeight = (100 - status.energy) / totalWeight; // Assuming lower energy increases the need for rest or less active engagement

        float randomValue = Random.value; // Get a random value between 0 and 1

        // Determine the need category based on weighted probabilities
        if (randomValue < hungerWeight)
        {
            return NeedCategory.Food;
        }
        else if (randomValue < (hungerWeight + happinessWeight))
        {
            return NeedCategory.Attention;
        }
        else
        {
            return NeedCategory.Activity;
        }
    }

    void LookForObjectsOfInterest()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, range);
        // Randomize the order of hitColliders
        ShuffleColliders(hitColliders);

        NeedCategory targetCategory = DetermineNeedCategory(); // Determine the current need based on status

        foreach (var hitCollider in hitColliders)
        {
            if (IsValidTarget(hitCollider, targetCategory))
            {
                Debug.Log("Found " + targetCategory + ": " + hitCollider.name);
                ChangeState(BabyState.Roaming, hitCollider.transform);  // Change to roaming and set the target
                return;
            }
        }
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
                return ((1 << collider.gameObject.layer) & LayerMask.GetMask("Grabbable")) != 0 || interests.Contains(collider.tag) || hazardTags.Contains(collider.tag);
            default:
                return false;
        }
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


    void HandleTargetlessRoaming()
    {
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
            idleCheckCooldown = 5f / (status.energy / 100);  // Reset cooldown based on energy
        }
    }

    // Class level variables
    private float timeSpentTryingToReach = 0f;
    private float maxTimeToReach = 10f;  // Maximum time in seconds to try reaching an object

    void HandleRoaming()
    {
        if (targetObject != null && agent.remainingDistance <= agent.stoppingDistance)
        {
            float distanceToTarget = Vector3.Distance(transform.position, targetObject.position);

            // Update the time spent trying regardless of the object's immediate reachability
            if (timeSpentTryingToReach < maxTimeToReach)
            {
                timeSpentTryingToReach += Time.deltaTime;  // Update time spent trying
            }
            else
            {
                // If the time spent trying exceeds the maximum time, change state to ShowingNeeds
                Debug.Log("Took too long to reach " + targetObject.name + ", now showing needs.");
                ChangeState(BabyState.ShowingNeeds);
                ResetReachTimer();  // Reset the timer for future attempts
                return;  // Exit the method to prevent further processing
            }

            if (distanceToTarget <= grabRange)
            {
                if (((1 << targetObject.gameObject.layer) & grabbableLayer) != 0)
                {
                    // Object is within reach and is grabbable, attach it to an available hand
                    AttachToHand(targetObject);
                    ChangeState(BabyState.Satiated);
                    ResetReachTimer();  // Reset the reach timer on successful grab
                }
                else
                {
                    // Object is within reach but not in the grabbable layer, inspect then go to targetless roaming
                    InspectObject();  // A method to inspect the object
                    Invoke("ChangeToTargetlessRoaming", 2.0f);  // Give some time for inspection
                    ResetReachTimer();  // Reset the reach timer after inspection
                }
            }
            else
            {
                // Object is visible but out of reach
                if (desireRating > 80)  // High desire, decide on action
                {
                    AttemptToReach();  // Risky attempt to reach the object
                }
                else
                {
                    Debug.Log("Can see but can't reach " + targetObject.name);
                    ChangeToTargetlessRoaming();  // Object is out of reach and not highly desired
                    ResetReachTimer();  // Reset the timer
                }
            }
        }
    }


    void ResetReachTimer()
    {
        timeSpentTryingToReach = 0f;  // Resets the timer used to track the duration of reaching attempts
    }


    void ChangeToTargetlessRoaming()
    {
        Debug.Log("lost interest and is now looking around");
        ChangeState(BabyState.TargetlessRoaming);
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
        Transform hand = null;
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

        // Attach the new object to the selected hand
        objectToGrab.SetParent(hand);
        objectToGrab.localPosition = Vector3.zero; // Set the local position to zero

        // Change state to Satiated assuming grabbing the object satisfies the baby
        ChangeState(BabyState.Satiated);
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


    void AttemptToReach()
    {
        float initialTryChance = 0.7f; // 70% initial chance to decide to try to reach the object
        float successChance = 0.6f; // 60% chance to successfully reach after deciding to try
        float hurtChance = 0.2f; // 20% chance to get hurt if the attempt is made

        while (true)
        {
            // First decide if the baby will attempt to reach at all
            if (Random.value < initialTryChance)
            {
                // Baby decides to try to reach the object
                Debug.Log("Decides to try to reach the object.");

                // Check if the reach attempt is successful
                if (Random.value < successChance)
                {
                    // Successfully grabs the object
                    AttachToHand(targetObject);
                    ChangeState(BabyState.Satiated);
                    Debug.Log("Successfully reached and grabbed " + targetObject.name);
                    break; // Exit the loop as the baby has succeeded
                }
                else if (Random.value < hurtChance)
                {
                    // Failed and got hurt
                    Debug.Log("Tried to reach " + targetObject.name + " and got hurt.");
                    ChangeState(BabyState.Hurt);
                    break; // Exit the loop as the baby has hurt itself
                }
                // If not successful or hurt, decide again at the top of the loop whether to try
            }
            else
            {
                // Decides not to try to reach the object
                Debug.Log("Decides not to try to reach " + targetObject.name + " anymore.");
                ChangeState(BabyState.ShowingNeeds);
                break; // Exit the loop as the baby decides not to attempt anymore
            }
        }
    }




    void HandleSatiated()
    {
        if (!IsInvoking(nameof(EndSatiated)))
        {
            status.shouldDecay = false; // Stop the status decay
            Invoke(nameof(EndSatiated), 10f); // Set a timer for 10 seconds of being satiated
        }
    }

    void EndSatiated()
    {
        status.shouldDecay = true; // Resume status decay
        ChangeState(BabyState.Idle); // Return to Idle state
    }

    void HandleRandomMovement()
    {
        Vector3 randomDirection = Random.insideUnitSphere * range;
        randomDirection += transform.position;
        NavMeshHit hit;
        NavMesh.SamplePosition(randomDirection, out hit, range, 1);
        agent.SetDestination(hit.position);
    }

    int EvaluateDesire(Transform target)
    {
        // Simple random desire rating for demonstration; customize this based on game logic
        return Random.Range(1, 101);
    }

}