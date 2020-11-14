
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class ToggleDildoButton : UdonSharpBehaviour
{
    public DogBall dogBall;

    void Interact()
    {
        if (Networking.LocalPlayer == null)
        {
            dogBall.ToggleDildo();
        }
        else
        {
            dogBall.SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, "ToggleDildo");
        }
    }
}
