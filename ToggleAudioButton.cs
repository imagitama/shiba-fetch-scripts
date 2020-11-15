
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class ToggleAudioButton : UdonSharpBehaviour
{
    public Doggo doggo;

    void Interact()
    {
        if (Networking.LocalPlayer == null)
        {
            doggo.ToggleAudio();
        }
        else
        {
            doggo.SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, "ToggleAudio");
        }
    }
}
