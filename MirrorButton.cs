
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class MirrorButton : UdonSharpBehaviour
{
    bool isMirrorActive = false;
    GameObject mirror;

    void Start()
    {
        mirror = transform.parent.Find("VRCMirror").gameObject;
    }

    void Interact()
    {
        isMirrorActive = !isMirrorActive;
        mirror.SetActive(isMirrorActive);
    }
}
