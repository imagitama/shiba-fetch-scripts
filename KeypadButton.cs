
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

public class KeypadButton : UdonSharpBehaviour
{
    public Keypad keypad;
    public int buttonNumber;

    void Start()
    {
        Text label = transform.Find("label/Text").GetComponent<Text>();
        label.text = buttonNumber.ToString();
    }

    void Interact()
    {
        if (Networking.LocalPlayer != null)
        {
            Networking.SetOwner(Networking.LocalPlayer, keypad.gameObject);
        }

        keypad.OnPressButton(buttonNumber);
    }
}
