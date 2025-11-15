using UnityEngine;

public class DoorController : MonoBehaviour
{
    [Header("Código de acceso de esta puerta")]
    public string accessCode = "2580";

    [Header("Punto de bisagra (Empty que rota la puerta)")]
    public Transform hinge;

    [Header("Ángulo de apertura")]
    public float openAngle = 90f;

    public float speed = 2f;
    private bool isOpen = false;

    private Quaternion closedRot;
    private Quaternion openRot;

    private void Awake()
    {
        closedRot = hinge.localRotation;
        openRot = Quaternion.Euler(0, openAngle, 0);
    }

    public void OpenDoor()
    {
        if (!isOpen)
        {
            isOpen = true;
            StopAllCoroutines();
            StartCoroutine(OpenRoutine());
        }
    }

    private System.Collections.IEnumerator OpenRoutine()
    {
        while (Quaternion.Angle(hinge.localRotation, openRot) > 0.5f)
        {
            hinge.localRotation = Quaternion.Lerp(
                hinge.localRotation,
                openRot,
                Time.deltaTime * speed
            );
            yield return null;
        }
    }

    public bool ValidateCode(string code)
    {
        return code == accessCode;
    }
}
