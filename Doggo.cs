
using UdonSharp;
using UnityEngine;
using UnityEngine.AI;
using VRC.SDKBase;
using VRC.Udon;

public class Doggo : UdonSharpBehaviour
{
    NavMeshAgent myNavAgent;
    // float timeUntilNextMovement = 0;
    Transform shibaAvatarTransform;
    public DogBall dogBall;
    public Animator doggoAnimatorController;
    string currentAnimationName = "Idle";
    public float distanceBeforeDropBall = 3f;
    public float distanceBeforePickup = 1f;
    bool isRunningToBallThrower = false;
    Vector3 agentDestination;
    public Transform fakePlayerPosition;
    bool isPickingUpBall = false;
    float timeUntilRunToOwner;
    bool isWaitingForNewThrow = false;
    public Transform respawnPosition;

    void Start()
    {
        myNavAgent = GetComponent<NavMeshAgent>();
        shibaAvatarTransform = transform.GetChild(0);
    }

    void Update()
    {
        if (myNavAgent == null)
        {
            return;
        }

        Vector3 ballThrowerPositionFlat = GetBallThrowerPosition();
        // ballThrowerPositionFlat.y = 0;

        Vector3 dogPallPositionFlat = dogBall.transform.position;
        // dogPallPositionFlat.y = 0;

        if (Vector3.Distance(ballThrowerPositionFlat, dogPallPositionFlat) > distanceBeforeDropBall)
        {
            isWaitingForNewThrow = false;
        }

        if (isRunningToBallThrower)
        {
            agentDestination = ballThrowerPositionFlat;

            if (GetIsOwner())
            {

                //Debug.Log("[AGENT] Moving to ball thrower...");

                myNavAgent.isStopped = false;
                myNavAgent.SetDestination(agentDestination);
            }

            float distanceBetweenDogAndBallOwner = Vector3.Distance(transform.position, ballThrowerPositionFlat);

            if (distanceBetweenDogAndBallOwner < distanceBeforeDropBall)
            {
                //Debug.Log("[DOGGO] Drop for owner");
                isRunningToBallThrower = false;
                isWaitingForNewThrow = true;

                dogBall.SetIsInDogMouth(false);

                if (GetIsOwner())
                {
                    if (Networking.LocalPlayer == null)
                    {
                        dogBall.TeleportToOwner();
                    }
                    else
                    {
                        dogBall.SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, "TeleportToOwner");
                    }
                }
            }
        }
        else if (isWaitingForNewThrow)
        {
            if (GetIsOwner())
            {
                //Debug.Log("[AGENT] Stopped - waiting for new throw");
                myNavAgent.isStopped = true;
            }
        }
        else
        {
            agentDestination = dogBall.transform.position;

            if (GetIsOwner())
            {
                myNavAgent.isStopped = false;
                myNavAgent.SetDestination(agentDestination);
            }

            Vector3 doggoPosition = transform.position;
            // doggoPosition.y = 0;

            float distanceBetweenDogAndBall = Vector3.Distance(doggoPosition, dogPallPositionFlat);

            if (isPickingUpBall == false && distanceBetweenDogAndBall < distanceBeforePickup)
            {
                //Debug.Log("Picking up ball");
                isPickingUpBall = true;
                timeUntilRunToOwner = Time.time + 2f;
            }
        }

        if (isPickingUpBall || isRunningToBallThrower)
        {
            dogBall.SetIsInDogMouth(true);
        }
        else
        {
            dogBall.SetIsInDogMouth(false);
        }

        if (isPickingUpBall)
        {
            if (timeUntilRunToOwner != null && Time.time > timeUntilRunToOwner)
            {
                //Debug.Log("Running to thrower with ball");
                isPickingUpBall = false;
                isRunningToBallThrower = true;
            }
            else
            {
                if (GetIsOwner())
                {
                    //Debug.Log("Picking up ball");
                    myNavAgent.isStopped = true;
                }
            }
        }

        if (isWaitingForNewThrow || isPickingUpBall)
        {
            if (currentAnimationName != "Idle")
            {
                currentAnimationName = "Idle";
                doggoAnimatorController.SetInteger("State", 0);
            }
        }
        else
        {
            if (currentAnimationName != "Running")
            {
                currentAnimationName = "Running";
                doggoAnimatorController.SetInteger("State", 1);
            }
        }

        FaceTarget();
    }

    void FaceTarget()
    {
        if (shibaAvatarTransform == null || agentDestination == null)
        {
            return;
        }

        Vector3 lookAtPosition = agentDestination - shibaAvatarTransform.position;
        lookAtPosition.y = 0;
        Quaternion targetRotation = Quaternion.LookRotation(lookAtPosition);
        float strength = 2f;
        float str = Mathf.Min(strength * Time.deltaTime, 1);
        shibaAvatarTransform.rotation = Quaternion.Lerp(shibaAvatarTransform.rotation, targetRotation, str);
    }

    Vector3 GetBallThrowerPosition()
    {
        if (Networking.LocalPlayer == null)
        {
            return fakePlayerPosition.position;
        }

        return Networking.GetOwner(dogBall.gameObject).GetPosition();
    }

    bool GetIsOwner()
    {
        if (Networking.LocalPlayer == null)
        {
            return true;
        }
        return Networking.GetOwner(gameObject) == Networking.LocalPlayer;
    }

    public void Respawn()
    {
        transform.position = respawnPosition.position;
    }

    // public Vector3 GetRandomNavmeshLocation(float radius)
    // {
    //     Vector3 randomDirection = Random.insideUnitSphere * radius;
    //     randomDirection += transform.position;
    //     UnityEngine.AI.NavMeshHit hit;
    //     Vector3 finalPosition = Vector3.zero;
    //     if (UnityEngine.AI.NavMesh.SamplePosition(randomDirection, out hit, radius, 1))
    //     {
    //         finalPosition = hit.position;
    //     }
    //     return finalPosition;
    // }
}
