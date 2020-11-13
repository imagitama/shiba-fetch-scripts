
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class RespawnDoggoButton : UdonSharpBehaviour
{
    public Doggo doggo;
    public DogBall dogBall;

    void Interact()
    {
        if (Networking.LocalPlayer == null)
        {
            doggo.Respawn();
            dogBall.Respawn();
        }
        else
        {
            doggo.SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, "Respawn");
            dogBall.SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, "Respawn");
        }
    }
}
