
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class DoggoRespawner : UdonSharpBehaviour
{
    public Doggo doggo;

    void OnCollision(Collision collision)
    {
        GameObject gameObject = collision.gameObject;

        Debug.Log("Collision: " + gameObject.name);
    }

    void OnCollisionEnter(Collision collision)
    {
        GameObject gameObject = collision.gameObject;

        Debug.Log("Collision: " + gameObject.name);

        if (gameObject.GetComponent<Doggo>() != null)
        {
            if (Networking.LocalPlayer == null)
            {
                doggo.Respawn();
            }
            else
            {
                doggo.SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, "Respawn");
            }
        }
    }
}
