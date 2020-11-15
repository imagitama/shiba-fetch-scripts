
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class TogglePeanutButton : UdonSharpBehaviour
{
    public Doggo doggo;

    void Interact()
    {
        if (Networking.LocalPlayer == null)
        {
            doggo.ToggleIsPeanut();
        }
        else
        {
            doggo.SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, "ToggleIsPeanut");
        }
    }
}
