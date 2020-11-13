
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

    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
    }

    void OnDeserialization()
    {
        isPickedUp = syncedIsPickedUp;
    }

    void OnPickup()
    {
        isPickedUp = true;
        syncedIsPickedUp = true;
    }

    void OnDrop()
    {
        isPickedUp = false;
        syncedIsPickedUp = false;
    }

    void Update()
    {
        if (isInDogMouth == false || dogMouthPosition == null || rigidbody == null)
        {
            return;
        }

        rigidbody.MovePosition(dogMouthPosition);
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
}
