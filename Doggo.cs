
using UdonSharp;
using UnityEngine;
using UnityEngine.AI;
using VRC.SDKBase;
using VRC.Udon;

public class Doggo : UdonSharpBehaviour
{
    NavMeshAgent myNavAgent;
    Transform shibaAvatarTransform;
    public DogBall dogBall;
    public Animator doggoAnimatorController;
    string currentAnimationName = "Idle";
    float distanceBeforeDropBall = 3f;
    float distanceBeforePickup = 0.1f;
    bool isRunningToBallThrower = false;
    Vector3 agentDestination;
    public Transform fakePlayerPosition;
    bool isPickingUpBall = false;
    float timeUntilRunToOwner;
    bool isWaitingForNewThrow = false;
    public Transform respawnPosition;
    bool isPeanut = false;
    [UdonSynced] bool syncedIsPeanut = false;
    public Material defaultShibaBodyMaterial;
    public Material defaultShibaTailMaterial;
    public Material peanutShibaBodyMaterial;
    public Material peanutShibaTailMaterial;
    public Renderer shibaRenderer;
    public Renderer tailRenderer;
    public GameObject bandana;
    public GameObject collar;
    bool isCommittingSuicide = false;
    float distanceUntilRespawn = -100f;
    float distanceBeforeYeet = 0.2f;
    Vector3 suicideStartPosition;
    Vector3 suicideEndPosition;
    public Transform tongue;
    float delayWhilePickingUpBall = 1f;

    void Start()
    {
        myNavAgent = GetComponent<NavMeshAgent>();
        shibaAvatarTransform = transform.GetChild(0);
    }

    void Update()
    {
        if (myNavAgent == null || shibaRenderer == null)
        {
            return;
        }

        if (isPeanut)
        {
            Material[] newMaterials = shibaRenderer.materials;
            newMaterials[0] = peanutShibaBodyMaterial;
            newMaterials[1] = peanutShibaBodyMaterial;
            shibaRenderer.materials = newMaterials;
            tailRenderer.material = peanutShibaTailMaterial;
            bandana.SetActive(true);
            collar.SetActive(false);
        }
        else
        {
            Material[] newMaterials = shibaRenderer.materials;
            newMaterials[0] = defaultShibaBodyMaterial;
            newMaterials[1] = defaultShibaBodyMaterial;
            shibaRenderer.materials = newMaterials;
            tailRenderer.material = defaultShibaTailMaterial;
            bandana.SetActive(false);
            collar.SetActive(true);
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

            if (GetIsOwner() && myNavAgent.enabled)
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

            if (GetIsOwner() && myNavAgent.enabled)
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
                timeUntilRunToOwner = Time.time + delayWhilePickingUpBall;
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

        if (GetIsOwner())
        {
            NavMeshHit hit;
            if (NavMesh.FindClosestEdge(transform.position, out hit, NavMesh.AllAreas))
            {
                // if (isCommittingSuicide == false && hit.distance < distanceBeforeYeet)
                // {
                //     Debug.Log("[Doggo] Committing suicide");
                //     isCommittingSuicide = true;
                //     myNavAgent.enabled = false;
                //     suicideStartPosition = transform.position;
                //     suicideEndPosition = transform.forward * 10f;
                // }

                Debug.DrawRay(hit.position, Vector3.up, Color.blue);
            }

            // if (isCommittingSuicide)
            // {
            //     MoveTowardsDeath();
            // }

            if (transform.position.y < distanceUntilRespawn)
            {
                Respawn();
            }
        }

        Debug.DrawRay(agentDestination, Vector3.up, Color.red);

        FaceTarget();
    }

    void OnDeserialization()
    {
        isPeanut = syncedIsPeanut;
    }

    void MoveTowardsDeath()
    {
        if (suicideStartPosition == null)
        {
            return;
        }

        float speed = 1f;
        float arcHeight = 10f;
        float x0 = suicideStartPosition.x;
        float x1 = suicideEndPosition.x;
        float dist = x1 - x0;
        float nextX = Mathf.MoveTowards(transform.position.x, x1, speed * Time.deltaTime);
        float baseY = Mathf.Lerp(suicideStartPosition.y, suicideEndPosition.y, (nextX - x0) / dist);
        float arc = arcHeight * (nextX - x0) * (nextX - x1) / (-0.25f * dist * dist);
        Vector3 nextPos = new Vector3(nextX, baseY + arc, transform.position.z);

        transform.position = nextPos;
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
        Debug.Log("[Doggo] Respawn");

        // isCommittingSuicide = false;

        // always enable it in case owner switches when it is suiciding
        myNavAgent.enabled = true;

        // if (GetIsOwner())
        // {
        //     isCommittingSuicide = false;
        // }

        transform.position = respawnPosition.position;
    }

    public void ToggleIsPeanut()
    {
        isPeanut = !isPeanut;
        syncedIsPeanut = isPeanut;
    }

    public Vector3 GetMouthPosition()
    {
        return tongue.position;
    }
}
