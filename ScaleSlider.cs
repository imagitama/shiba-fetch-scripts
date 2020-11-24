
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using VRC.SDKBase;
using VRC.Udon;

public class ScaleSlider : UdonSharpBehaviour
{
    Slider slider;
    public Doggo doggo;

    void Start()
    {
        slider = GetComponent<Slider>();
    }

    public void SliderUpdate()
    {
        doggo.SetScale(slider.value);
    }
}
