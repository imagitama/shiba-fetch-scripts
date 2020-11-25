
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
    float distanceBeforeDropBall = 3f;
    float distanceBeforePickup = 0.1f;
    Vector3 agentDestination;
    public Transform fakePlayerPosition;
    float timeUntilRunToOwner;
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
    public Transform tongue;
    float delayWhilePickingUpBall = 1f;
    public AudioSource barkSound;
    public AudioSource pantingSound;
    public AudioSource runningSound;
    bool isAudioEnabled = true;
    // int is much better performance for sync than string
    int currentState;
    [UdonSynced] int syncedCurrentState;
    const int stateWaitingForThrow = 0;
    const int stateRunning = 1;
    const int statePickingUpBall = 2;
    const int stateRunningToThrower = 3;
    public Transform head;
    public Transform[] eyes;
    float scale = 0.4f;
    [UdonSynced] float syncedScale = 0.4f;
    const string animationBodyParameterName = "BodyState";
    const int animationBodyStateIdle = 0;
    const int animationBodyStateRun = 1;
    const string animationMouthParameterName = "MouthState";
    const int animationMouthStateIdle = 0;
    const int animationMouthStatePant = 1;
    const int animationMouthStateBark = 2;

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

        shibaAvatarTransform.localScale = new Vector3(scale, scale, scale);

        if (!GetIsOwner())
        {
            return;
        }

        // doesnt work in Start()
        // if (currentState == null)
        // {
        //     SetState(stateRunning);
        // }

        Vector3 ballThrowerPositionFlat = GetBallThrowerPosition();
        // ballThrowerPositionFlat.y = 0;

        Vector3 dogPallPositionFlat = dogBall.transform.position;
        // dogPallPositionFlat.y = 0;

        if (currentState == stateWaitingForThrow && Vector3.Distance(ballThrowerPositionFlat, dogPallPositionFlat) > distanceBeforeDropBall)
        {
            Debug.Log("[Doggo] Owner has thrown it! Running!");
            SetState(stateRunning);
        }

        if (currentState == stateRunningToThrower)
        {
            agentDestination = ballThrowerPositionFlat;

            //Debug.Log("[AGENT] Moving to ball thrower...");

            myNavAgent.isStopped = false;
            myNavAgent.SetDestination(agentDestination);

            float distanceBetweenDogAndBallOwner = Vector3.Distance(transform.position, ballThrowerPositionFlat);

            if (distanceBetweenDogAndBallOwner < distanceBeforeDropBall)
            {
                Debug.Log("[Doggo] Giving ball to owner");

                SetState(stateWaitingForThrow);

                dogBall.SetIsInDogMouth(false);

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
        else if (currentState == stateWaitingForThrow || currentState == statePickingUpBall)
        {
            //Debug.Log("[AGENT] Stopped - waiting for new throw");
            myNavAgent.isStopped = true;
        }
        else
        {
            agentDestination = dogBall.transform.position;

            myNavAgent.isStopped = false;
            myNavAgent.SetDestination(agentDestination);

            Vector3 doggoPosition = transform.position;
            // doggoPosition.y = 0;

            float distanceBetweenDogAndBall = Vector3.Distance(doggoPosition, dogPallPositionFlat);

            if (currentState != statePickingUpBall && distanceBetweenDogAndBall < distanceBeforePickup)
            {
                Debug.Log("[Doggo] Picking up ball");
                SetState(statePickingUpBall);

                timeUntilRunToOwner = Time.time + delayWhilePickingUpBall;
            }
        }

        if (currentState == statePickingUpBall || currentState == stateRunningToThrower)
        {
            dogBall.SetIsInDogMouth(true);
        }
        else
        {
            dogBall.SetIsInDogMouth(false);
        }

        if (currentState == statePickingUpBall)
        {
            if (timeUntilRunToOwner != null && Time.time > timeUntilRunToOwner)
            {
                Debug.Log("[Doggo] Running to thrower with ball");
                SetState(stateRunningToThrower);
            }
            else
            {
                myNavAgent.isStopped = true;
            }
        }

        NavMeshHit hit;

        if (NavMesh.FindClosestEdge(transform.position, out hit, NavMesh.AllAreas))
        {
            Debug.DrawRay(hit.position, Vector3.up, Color.blue);

            if (currentState != stateWaitingForThrow && hit.distance < 0.1f)
            {
                Debug.Log("[Doggo] Waiting for throw pls");
                SetState(stateWaitingForThrow);
            }
        }

        Debug.DrawRay(agentDestination, Vector3.up, Color.red);

        FaceTarget();
    }

    void LateUpdate()
    {
        if (shibaAvatarTransform == null)
        {
            return;
        }

        if (currentState == stateWaitingForThrow)
        {
            Vector3 playerHead = GetBallThrowerHeadPosition();

            head.LookAt(playerHead);

            float parentRotation = shibaAvatarTransform.eulerAngles.y;
            float headRotation = head.eulerAngles.y;

            float clampedAngleY = headRotation - parentRotation;

            if (clampedAngleY < 0)
            {
                clampedAngleY = 360 + clampedAngleY;
            }

            if (clampedAngleY >= 180 && clampedAngleY <= 315)
            {
                clampedAngleY = 315;
            }
            else if (clampedAngleY >= 45 && clampedAngleY <= 315)
            {
                clampedAngleY = 45;
            }

            clampedAngleY = clampedAngleY + parentRotation;

            if (clampedAngleY > 360)
            {
                clampedAngleY = clampedAngleY - 360;
            }

            head.eulerAngles = new Vector3(head.eulerAngles.x, clampedAngleY, head.eulerAngles.z);

            foreach (Transform eye in eyes)
            {
                eye.LookAt(playerHead);
            }
        }
    }

    void OnDeserialization()
    {
        isPeanut = syncedIsPeanut;
        scale = syncedScale;

        if (syncedCurrentState != currentState)
        {
            currentState = syncedCurrentState;

            PlayAudioBasedOnState();
            PlayAnimationBasedOnState();
        }
    }

    void SetState(int newState)
    {
        if (newState == currentState)
        {
            return;
        }

        Debug.Log("[Doggo] State " + newState.ToString());

        currentState = newState;
        syncedCurrentState = currentState;

        PlayAudioBasedOnState();
        PlayAnimationBasedOnState();
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

    Vector3 GetBallThrowerHeadPosition()
    {
        if (Networking.LocalPlayer == null)
        {
            Vector3 fakePosition = fakePlayerPosition.position;
            fakePosition.y = fakePosition.y + 2;
            return fakePosition;
        }

        return Networking.GetOwner(dogBall.gameObject).GetBonePosition(HumanBodyBones.Head);
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

        // always enable it in case owner switches when it is suiciding
        myNavAgent.enabled = true;

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

    public void ToggleAudio()
    {
        isAudioEnabled = !isAudioEnabled;

        PlayAudioBasedOnState();
    }

    void PlayAudioBasedOnState()
    {
        if (!isAudioEnabled)
        {
            runningSound.Stop();
            pantingSound.Stop();
            barkSound.Stop();
            return;
        }

        switch (currentState)
        {
            case stateWaitingForThrow:
                pantingSound.Play();
                runningSound.Pause();
                barkSound.Play();
                break;
            case statePickingUpBall:
                pantingSound.Play();
                runningSound.Pause();
                barkSound.Stop();
                break;
            case stateRunning:
            case stateRunningToThrower:
                pantingSound.Pause();
                runningSound.Play();
                barkSound.Stop();
                break;
        }
    }

    void PlayAnimationBasedOnState()
    {
        switch (currentState)
        {
            case stateWaitingForThrow:
            case statePickingUpBall:
                doggoAnimatorController.SetInteger(animationBodyParameterName, animationBodyStateIdle);
                break;
            case stateRunning:
            case stateRunningToThrower:
                doggoAnimatorController.SetInteger(animationBodyParameterName, animationBodyStateRun);
                break;
        }

        switch (currentState)
        {
            case stateWaitingForThrow:
                doggoAnimatorController.SetInteger(animationMouthParameterName, animationMouthStateBark);
                break;

            case stateRunning:
            case stateRunningToThrower:
                doggoAnimatorController.SetInteger(animationMouthParameterName, animationMouthStatePant);
                break;

            default:
                doggoAnimatorController.SetInteger(animationMouthParameterName, animationMouthStateIdle);
                break;
        }
    }

    public void SetScale(float newScale)
    {
        Debug.Log("[Doggo] Set scale " + newScale.ToString());
        scale = newScale;
        syncedScale = scale;
    }
}
