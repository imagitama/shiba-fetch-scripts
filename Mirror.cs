
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class Mirror : UdonSharpBehaviour
{
    bool isMirrorVisible = false;
    public GameObject mirror;

    public void DoInteract() {
        isMirrorVisible = !isMirrorVisible;

        mirror.SetActive(isMirrorVisible);
    }
}
