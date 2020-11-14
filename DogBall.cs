
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class DogBall : UdonSharpBehaviour
{
    public Doggo doggo;
    Rigidbody rigidbody;
    bool isPickedUp = false;
    [UdonSynced] bool syncedIsPickedUp = false;
    bool isInDogMouth = false;
    Vector3 dogMouthPosition;
    public Transform fakePlayerPosition;
    public Transform respawnPosition;
    bool isDildo = false;
    [UdonSynced] bool syncedIsDildo = false;
    GameObject ballObject;
    GameObject dildoObject;

    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
        ballObject = transform.Find("ball").gameObject;
        dildoObject = transform.Find("dildo").gameObject;
    }

    void OnDeserialization()
    {
        isPickedUp = syncedIsPickedUp;
        isDildo = syncedIsDildo;
    }

    void OnPickup()
    {
        isPickedUp = true;
        syncedIsPickedUp = true;

        // fix the dog running after a laggy ball
        if (Networking.LocalPlayer != null)
        {
            Networking.SetOwner(Networking.LocalPlayer, doggo.gameObject);
        }
    }

    void OnDrop()
    {
        isPickedUp = false;
        syncedIsPickedUp = false;
    }

    void Update()
    {
        if (rigidbody == null || dildoObject == null || ballObject == null)
        {
            return;
        }

        dildoObject.SetActive(isDildo == true);
        ballObject.SetActive(isDildo == false);

        if (isInDogMouth == false || dogMouthPosition == null)
        {
            return;
        }

        if (isInDogMouth == true)
        {
            rigidbody.MovePosition(dogMouthPosition);
        }
    }

    public void SetIsInDogMouth(bool newVal)
    {
        isInDogMouth = newVal;
        dogMouthPosition = doggo.transform.position;
    }

    Vector3 GetOwnerPosition()
    {
        if (Networking.LocalPlayer == null)
        {
            return fakePlayerPosition.position;
        }

        return Networking.GetOwner(gameObject).GetPosition();
    }

    public void TeleportToOwner()
    {
        Vector3 ownerPosition = GetOwnerPosition();

        // fix weird physics bug where it falls through map and respawns
        ownerPosition.y = ownerPosition.y + 2;

        rigidbody.MovePosition(ownerPosition);
    }

    bool GetIsPickedUp()
    {
        return isPickedUp;
    }

    public void Respawn()
    {
        rigidbody.MovePosition(respawnPosition.position);
    }

    public void ToggleDildo()
    {
        isDildo = !isDildo;
        syncedIsDildo = isDildo;
    }
}
