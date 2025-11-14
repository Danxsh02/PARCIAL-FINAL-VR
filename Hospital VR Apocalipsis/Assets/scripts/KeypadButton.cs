using UnityEngine;

public class KeypadButton : MonoBehaviour
{
    public string value; // Número o letra
    public KeypadManager keypad; // Referencia al keypad

    public void Press()
    {
        if (keypad != null)
            keypad.AddInput(value);
    }
}
