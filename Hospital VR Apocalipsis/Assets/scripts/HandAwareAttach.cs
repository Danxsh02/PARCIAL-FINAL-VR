using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRGrabInteractable))]
public class HandAwareAttach : MonoBehaviour
{
    [Header("Attach Points")]
    public Transform attachLeft;
    public Transform attachRight;

    [Header("Hands Controllers")]
    public Transform leftHandController;
    public Transform rightHandController;

    private XRGrabInteractable grab;

    void Awake()
    {
        grab = GetComponent<XRGrabInteractable>();
    }

    void Start()
    {
        // asignar un attach inicial
        if (attachRight != null)
            grab.attachTransform = attachRight;
    }

    void Update()
    {
        if (grab == null) return;

        // Solo cambiar attach cuando no está siendo agarrado
        if (!grab.isSelected && leftHandController != null && rightHandController != null)
        {
            float distLeft = Vector3.Distance(transform.position, leftHandController.position);
            float distRight = Vector3.Distance(transform.position, rightHandController.position);

            // Elige la mano más cercana
            if (distRight < distLeft && attachRight != null)
            {
                if (grab.attachTransform != attachRight)
                    grab.attachTransform = attachRight;
            }
            else if (attachLeft != null)
            {
                if (grab.attachTransform != attachLeft)
                    grab.attachTransform = attachLeft;
            }
        }
    }
}
