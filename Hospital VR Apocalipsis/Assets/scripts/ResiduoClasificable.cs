using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class ResiduoClasificable : MonoBehaviour
{
    public enum ColorTarro { Rojo, Amarillo, Verde, Azul, Naranja }
    public enum TipoResiduo { Biologico, Quimico, Reciclable, PapelLimpio, Ordinario }

    [Header("Configuración general")]
    public bool esTarro = false;

    [Header("Solo para residuos")]
    public TipoResiduo tipoResiduo;

    [Header("Solo para tarros")]
    public ColorTarro colorTarro;
    public TipoResiduo tipoAceptado;

    private UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor socket;

    private void OnEnable()
    {
        if (esTarro)
        {
            socket = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor>();
            if (socket != null)
                socket.selectEntered.AddListener(ValidarResiduo);
        }
    }

    private void OnDisable()
    {
        if (esTarro && socket != null)
            socket.selectEntered.RemoveListener(ValidarResiduo);
    }

    private void ValidarResiduo(SelectEnterEventArgs args)
    {
        ResiduoClasificable residuo = args.interactableObject.transform.GetComponent<ResiduoClasificable>();
        if (residuo != null && !residuo.esTarro)
        {
            if (residuo.tipoResiduo == tipoAceptado)
            {
                GameManager.Instance.RegistrarDeposito(residuo.tipoResiduo);
                FeedbackCorrecto();
            }
            else
            {
                FeedbackError();
                ExpulsarResiduo(args.interactableObject.transform);
            }
        }
    }

    private void ExpulsarResiduo(Transform residuo)
    {
        var grabInteractable = residuo.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        var interactable = grabInteractable as UnityEngine.XR.Interaction.Toolkit.Interactables.IXRSelectInteractable;

        if (interactable != null && socket != null)
        {
            socket.interactionManager.CancelInteractableSelection(interactable);
            residuo.GetComponent<Rigidbody>().AddForce(Vector3.up * 0.5f, ForceMode.Impulse);
        }
    }

    private void FeedbackCorrecto()
    {
        // Luz verde, sonido, vibración
    }

    private void FeedbackError()
    {
        // Luz roja, sonido de error, vibración
    }
}
