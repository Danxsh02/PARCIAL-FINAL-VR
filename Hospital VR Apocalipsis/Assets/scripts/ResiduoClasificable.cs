using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class ResiduoClasificable : MonoBehaviour
{
    public enum ColorTarro { Rojo, Amarillo, Verde, Azul, Naranja }
    public enum TipoResiduo { Biologico, Quimico, Organico, Reciclable, Ordinario }

    [Header("Configuración del objeto")]
    public bool esTarro = false; // Indica si el objeto es un tarro o un residuo

    [Header("Solo para residuos")]
    public TipoResiduo tipoResiduo;

    [Header("Solo para tarros")]
    public ColorTarro colorTarro;
    public TipoResiduo tipoAceptado;

    [Header("Configuración de destrucción del residuo")]

    //Tiempo antes de destruir el residuo correcto
    public float tiempoAntesDestruir = 0.5f;

    //Animar antes de destruir
    public bool usarAnimacion = true;

    private UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor socket;

    private void OnEnable()
    {
        //Si el objeto es un tarro, obtenemomsos el socket interactor y validamos residuos que entren
        if (esTarro)
        {
            socket = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor>();
            if (socket != null)
                socket.selectEntered.AddListener(ValidarResiduo);
        }
    }

    private void OnDisable()
    {
        //Si el tarro se desactiva, removemos el listener
        if (esTarro && socket != null)
            socket.selectEntered.RemoveListener(ValidarResiduo);
    }

    private void ValidarResiduo(SelectEnterEventArgs args)
    {
        //Obtenemos el componente ResiduoClasificable del objeto que entra en el tarro
        ResiduoClasificable residuo = args.interactableObject.transform.GetComponent<ResiduoClasificable>();

        if (residuo != null && !residuo.esTarro)
        {
            if (residuo.tipoResiduo == tipoAceptado)
            {
                Debug.Log($" {residuo.tipoResiduo} clasificado correctamente en {colorTarro}");

                // Registrar en el GameManager
                GameManager.Instance.RegistrarDeposito(residuo.tipoResiduo);


                // Destruir residuo después de un tiempo 
                if (usarAnimacion)
                {
                    StartCoroutine(DestruirConAnimacion(residuo.gameObject));
                }
                else
                {
                    GameManager.Instance.RegistrarResiduoRecogido();

                    Destroy(residuo.gameObject, tiempoAntesDestruir);
                }
            }
            else
            {
                Debug.Log($"{residuo.tipoResiduo} NO pertenece al tarro {colorTarro}");

                ExpulsarResiduo(args.interactableObject.transform);
            }
        }
    }

    // Expulsar residuo incorrecto
    private void ExpulsarResiduo(Transform residuo)
    {
        var grabInteractable = residuo.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        var interactable = grabInteractable as UnityEngine.XR.Interaction.Toolkit.Interactables.IXRSelectInteractable;

        // Cancela la selección del interactable y empuja hacia arriba
        if (interactable != null && socket != null)
        {
            socket.interactionManager.CancelInteractableSelection(interactable);
            residuo.GetComponent<Rigidbody>().AddForce(Vector3.up * 0.5f, ForceMode.Impulse);
        }
    }

    //  Animación de desaparición
    private IEnumerator DestruirConAnimacion(GameObject obj)
    {
        // Desactivar interacción XR para que no se pueda agarrar
        var grabInteractable = obj.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        if (grabInteractable != null)
        {
            grabInteractable.enabled = false;
        }

        // Desactivar físicas
        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        // Esperar un momento antes de empezar la animación
        yield return new WaitForSeconds(tiempoAntesDestruir);

        // Animar reducción de escala
        float duracion = 0.3f;
        float tiempo = 0f;
        Vector3 escalaInicial = obj.transform.localScale;

        while (tiempo < duracion)
        {
            float t = tiempo / duracion;
            float curva = Mathf.Sin(t * Mathf.PI * 0.5f);
            obj.transform.localScale = Vector3.Lerp(escalaInicial, Vector3.zero, curva);

            tiempo += Time.deltaTime;
            yield return null;
        }

        GameManager.Instance.RegistrarResiduoRecogido();

        // Destruir el objeto
        Destroy(obj);
        Debug.Log($"Residuo destruido");
    }

}