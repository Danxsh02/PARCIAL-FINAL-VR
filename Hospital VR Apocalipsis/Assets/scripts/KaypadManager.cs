using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class KeypadManager : MonoBehaviour
{
    [Header("UI del Keypad")]
    public TextMeshProUGUI display;

    [Header("Puertas asociadas a este keypad")]
    public List<DoorController> doors;

    private string currentInput = "";

    public void AddInput(string value)
    {
        currentInput += value;
        display.text = currentInput;

        // Chequear si algún código coincide
        foreach (var door in doors)
        {
            if (currentInput.Length >= door.accessCode.Length)
            {
                if (door.ValidateCode(currentInput))
                {
                    door.OpenDoor();
                    Debug.Log($"Código correcto para {door.gameObject.name}");
                    currentInput = "";
                    display.text = "";
                    return;
                }
            }
        }

        // Si ningún código coincide y la longitud ya alcanzó el más largo
        int maxCodeLength = 0;
        foreach (var d in doors) if (d.accessCode.Length > maxCodeLength) maxCodeLength = d.accessCode.Length;

        if (currentInput.Length >= maxCodeLength)
        {
            Debug.Log("Código incorrecto");
            currentInput = "";
            display.text = "";
        }
    }
}
