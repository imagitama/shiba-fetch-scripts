
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

public class Keypad : UdonSharpBehaviour
{
    public Text displayText;
    string currentValue = "";
    [UdonSynced] string syncedCurrentValue = "";
    int maxLength = 4;
    float timeUntilClearDisplay = 0;
    public GameObject ToggleDildoButton;
    bool isToggleDildoButtonVisible = false;
    [UdonSynced] bool syncedIsToggleDildoButtonVisible = false;
    public GameObject TogglePeanutButton;
    bool isTogglePeanutButtonVisible = false;
    [UdonSynced] bool syncedIsTogglePeanutButtonVisible = false;

    void Update()
    {
        displayText.text = currentValue;

        if (timeUntilClearDisplay != 0 && Time.time > timeUntilClearDisplay)
        {
            timeUntilClearDisplay = 0;
            currentValue = "";
            syncedCurrentValue = "";
        }

        ToggleDildoButton.SetActive(isToggleDildoButtonVisible);
        TogglePeanutButton.SetActive(isTogglePeanutButtonVisible);
    }

    void OnDeserialization()
    {
        currentValue = syncedCurrentValue;
        isToggleDildoButtonVisible = syncedIsToggleDildoButtonVisible;
        isTogglePeanutButtonVisible = syncedIsTogglePeanutButtonVisible;
    }

    public void OnPressButton(int buttonNumber)
    {
        if (timeUntilClearDisplay != 0)
        {
            return;
        }

        if (currentValue.Length >= maxLength)
        {
            currentValue = buttonNumber.ToString();
        }
        else
        {
            currentValue = $"{currentValue}{buttonNumber.ToString()}";

            if (currentValue.Length >= maxLength)
            {
                bool isSuccess = false;

                switch (currentValue)
                {
                    case "8008":
                        ToggleToggleDildoButton();
                        isSuccess = true;
                        break;
                    case "1756":
                        ToggleTogglePeanutButton();
                        isSuccess = true;
                        break;
                }

                if (isSuccess)
                {
                    currentValue = "SUCCESS";
                }
                else
                {
                    currentValue = "FAIL";
                }

                timeUntilClearDisplay = Time.time + 2f;
            }
        }

        syncedCurrentValue = currentValue;
    }

    void ToggleToggleDildoButton()
    {
        isToggleDildoButtonVisible = !isToggleDildoButtonVisible;
        syncedIsToggleDildoButtonVisible = isToggleDildoButtonVisible;
    }

    void ToggleTogglePeanutButton()
    {
        isTogglePeanutButtonVisible = !isTogglePeanutButtonVisible;
        syncedIsTogglePeanutButtonVisible = isTogglePeanutButtonVisible;
    }
}
