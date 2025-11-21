using UnityEngine;
using UnityEngine.XR.Content.Interaction;

public class TrashChuteTrigger : MonoBehaviour
{
    [Header("Referencias")]
    public XRLever palanca;
    public Transform puntoDetencion; 

    [Header("Configuración")]
    public float velocidadAspiracion = 5f;
    public float distanciaDestruccion = 0.5f; // Distancia desde el punto para destruir

    private GameObject bolsaActual;
    private bool bolsaEnContacto = false;
    private Vector3 posicionOriginalBolsa;
    private Rigidbody rbBolsa;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($" Trigger Enter detectado: {other.gameObject.name} con tag: {other.tag}");

        if (other.CompareTag("Bolsa") && bolsaActual == null)
        {
            Debug.Log($"Bolsa detectada correctamente: {other.gameObject.name}");

            bolsaActual = other.gameObject;
            bolsaEnContacto = true;

            // DETENER todas las coroutines que puedan estar corriendo
            MonoBehaviour[] scripts = bolsaActual.GetComponents<MonoBehaviour>();
            Debug.Log($"Scripts encontrados en la bolsa: {scripts.Length}");

            foreach (MonoBehaviour script in scripts)
            {
                if (script != null && script.enabled)
                {
                    Debug.Log($" Deteniendo coroutines en: {script.GetType().Name}");
                    script.StopAllCoroutines();
                }
            }

            // DESACTIVAR el componente ResiduoClasificable para evitar destrucción automática
            ResiduoClasificable residuo = bolsaActual.GetComponent<ResiduoClasificable>();
            if (residuo != null)
            {
                Debug.Log($" Desactivando ResiduoClasificable (usarAnimacion: {residuo.usarAnimacion}, tiempoAntesDestruir: {residuo.tiempoAntesDestruir})");
                residuo.enabled = false;
            }
            else
            {
                Debug.LogWarning("⚠️ No se encontró componente ResiduoClasificable en la bolsa");
            }

            // Desactivar XRGrabInteractable para que no se pueda agarrar
            var grabInteractable = bolsaActual.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
            if (grabInteractable != null)
            {
                Debug.Log(" Desactivando XRGrabInteractable");
                grabInteractable.enabled = false;
            }

            // Guardar referencia al Rigidbody
            rbBolsa = bolsaActual.GetComponent<Rigidbody>();

            if (rbBolsa != null)
            {
                Debug.Log($"Configurando Rigidbody - isKinematic: true, useGravity: false");
                // Hacer kinematic para "congelar" la bolsa
                rbBolsa.isKinematic = true;
                rbBolsa.linearVelocity = Vector3.zero;
                rbBolsa.angularVelocity = Vector3.zero;
                rbBolsa.useGravity = false;
            }
            else
            {
                Debug.LogWarning("No se encontró Rigidbody en la bolsa");
            }

            // Cancelar cualquier Destroy programado (esto es lo MÁS importante)
            CancelInvoke();
            Debug.Log(" CancelInvoke() ejecutado");

            // Mover la bolsa al punto de detención
            if (puntoDetencion != null)
            {
                Debug.Log($"📍 Moviendo bolsa a punto de detención: {puntoDetencion.position}");
                bolsaActual.transform.position = puntoDetencion.position;
                bolsaActual.transform.rotation = puntoDetencion.rotation;
            }
            else
            {
                Debug.LogError(" No hay punto de detención asignado!");
            }

            Debug.Log(" Bolsa configurada correctamente en el chute");
        }
        else if (bolsaActual != null)
        {
            Debug.Log("Ya hay una bolsa en el chute");
        }
    }

    private void Update()
    {
        if (bolsaActual != null && bolsaEnContacto)
        {
            // Verificar si la bolsa sigue existiendo
            if (bolsaActual == null)
            {
                Debug.LogError(" LA BOLSA SE DESTRUYÓ! Limpiando referencias...");
                bolsaEnContacto = false;
                rbBolsa = null;
                return;
            }

            // Si la palanca está activada, aspirar la bolsa
            if (palanca != null && !palanca.value)
            {
                Debug.Log(" Palanca activada - Aspirando bolsa");
                AspirarBolsa();
            }
        }
    }

    private void AspirarBolsa()
    {
        // Mover la bolsa hacia abajo (o la dirección que necesites)
        Vector3 direccionAspiracion = -transform.up; // Ajusta según tu necesidad
        bolsaActual.transform.position += direccionAspiracion * velocidadAspiracion * Time.deltaTime;

        // Verificar si llegó lo suficientemente lejos para destruirla
        float distancia = Vector3.Distance(bolsaActual.transform.position, puntoDetencion.position);

        Debug.Log($"📏 Distancia recorrida: {distancia:F2} / {distanciaDestruccion:F2}");

        if (distancia >= distanciaDestruccion)
        {
            Debug.Log("🎯 Distancia alcanzada - Destruyendo bolsa");

            // Registrar en el GameManager
            GameManager.Instance.RegistrarBolsaDepositada();

            // Destruir la bolsa
            Destroy(bolsaActual);
            Debug.Log("💥 Bolsa destruida exitosamente por el TrashChuteTrigger");

            bolsaActual = null;
            bolsaEnContacto = false;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Si la bolsa sale del trigger antes de ser aspirada
        if (other.CompareTag("Bolsa") && other.gameObject == bolsaActual && !palanca.value)
        {
            // Restaurar física si es necesario
            if (rbBolsa != null)
            {
                rbBolsa.isKinematic = false;
            }

            bolsaActual = null;
            bolsaEnContacto = false;
        }
    }
}