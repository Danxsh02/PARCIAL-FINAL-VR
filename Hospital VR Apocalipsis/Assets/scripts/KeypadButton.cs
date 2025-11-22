using UnityEngine;

public class KeypadButton : MonoBehaviour
{
    public string value; // Numero o letra
    public KeypadManager keypad; // Referencia al keypad

    public void Press()
    {
        if (keypad != null)
            keypad.AddInput(value);
    }
}
