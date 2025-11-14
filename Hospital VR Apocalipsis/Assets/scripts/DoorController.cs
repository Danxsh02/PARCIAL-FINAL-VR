using UnityEngine;

public class DoorController : MonoBehaviour
{
    [Header("Código de acceso de esta puerta")]
    public string accessCode = "2580";

    [Header("Animación de apertura")]
    public Transform door;
    public Vector3 openRotation;
    public float speed = 2f;

    private bool isOpen = false;
    private Quaternion closedRot;
    private Quaternion targetRot;

    private void Awake()
    {
        closedRot = door.localRotation;
        targetRot = Quaternion.Euler(openRotation);
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
        while (Quaternion.Angle(door.localRotation, targetRot) > 0.5f)
        {
            door.localRotation = Quaternion.Lerp(door.localRotation, targetRot, Time.deltaTime * speed);
            yield return null;
        }
    }

    public bool ValidateCode(string code)
    {
        return code == accessCode;
    }
}
