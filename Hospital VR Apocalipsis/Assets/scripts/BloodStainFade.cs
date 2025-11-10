using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(Renderer))]
public class BloodStainFade : MonoBehaviour
{
    [Header("Configuración de Limpieza")]
    [Tooltip("Cuánto se reduce la opacidad por cada click")]
    public float fadeAmountPerClick = 0.25f; // 4 clicks para limpiar completamente

    [Header("Feedback Visual (Opcional)")]
    public bool useScaleEffect = true;
    public float scaleEffectDuration = 0.1f;
    public float scaleEffectAmount = 0.9f;

    [Header("Debug")]
    public bool showDebugLogs = true;

    private Material stainMaterial;
    private float currentAlpha = 1f;
    private Sponge spongeInContact = null;
    private XRGrabInteractable spongeGrabInteractable = null;

    void Start()
    {
        Renderer rend = GetComponent<Renderer>();

        // Crear una instancia del material para no afectar otros objetos
        stainMaterial = new Material(rend.material);
        rend.material = stainMaterial;

        SetAlpha(currentAlpha);

        if (showDebugLogs)
        {
            Debug.Log($"BloodStainFade iniciado en {gameObject.name}. Alpha inicial: {currentAlpha}");
        }
    }

    void OnTriggerEnter(Collider other)
    {
        Sponge sponge = other.GetComponent<Sponge>();

        if (sponge != null)
        {
            if (showDebugLogs)
            {
                Debug.Log($"Esponja detectada: {sponge.gameObject.name}, Estado: {sponge.currentState}");
            }

            if (sponge.CanClean())
            {
                spongeInContact = sponge;

                // Obtener el XRGrabInteractable de la esponja
                spongeGrabInteractable = sponge.GetComponent<XRGrabInteractable>();

                if (spongeGrabInteractable != null)
                {
                    // Suscribirse al evento activated (cuando se presiona el gatillo)
                    spongeGrabInteractable.activated.AddListener(OnSpongeActivated);

                    if (showDebugLogs)
                    {
                        Debug.Log("Suscrito al evento activated de la esponja");
                    }
                }
                else
                {
                    Debug.LogWarning("La esponja no tiene XRGrabInteractable!");
                }
            }
            else
            {
                if (showDebugLogs)
                {
                    Debug.Log("Esponja no puede limpiar (debe estar mojada)");
                }
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        Sponge sponge = other.GetComponent<Sponge>();

        if (sponge != null && sponge == spongeInContact)
        {
            // Desuscribirse del evento
            if (spongeGrabInteractable != null)
            {
                spongeGrabInteractable.activated.RemoveListener(OnSpongeActivated);

                if (showDebugLogs)
                {
                    Debug.Log("Desuscrito del evento activated de la esponja");
                }
            }

            spongeInContact = null;
            spongeGrabInteractable = null;
        }
    }

    // Este método se llama cuando se presiona el gatillo mientras se sostiene la esponja
    private void OnSpongeActivated(ActivateEventArgs args)
    {
        if (spongeInContact != null && spongeInContact.CanClean())
        {
            OnClick();
        }
    }

    void OnClick()
    {
        if (showDebugLogs)
        {
            Debug.Log($"¡Click en mancha! Alpha antes: {currentAlpha}");
        }

        // Reducir la opacidad
        currentAlpha -= fadeAmountPerClick;
        SetAlpha(currentAlpha);

        // Efecto visual opcional
        if (useScaleEffect)
        {
            StartCoroutine(ScaleEffect());
        }

        // Ensuciar la esponja
        if (spongeInContact != null)
        {
            spongeInContact.Use();
        }

        if (showDebugLogs)
        {
            Debug.Log($"Alpha después: {currentAlpha}");
        }

        // Destruir si está completamente limpio
        if (currentAlpha <= 0f)
        {
            if (showDebugLogs)
            {
                Debug.Log("¡Mancha completamente limpia! Destruyendo objeto...");
            }

            // Desuscribirse antes de destruir
            if (spongeGrabInteractable != null)
            {
                spongeGrabInteractable.activated.RemoveListener(OnSpongeActivated);
            }

            Destroy(gameObject);
        }
    }

    void SetAlpha(float a)
    {
        if (stainMaterial == null) return;

        Color c = stainMaterial.color;
        c.a = Mathf.Clamp01(a);
        stainMaterial.color = c;
    }

    IEnumerator ScaleEffect()
    {
        Vector3 originalScale = transform.localScale;
        Vector3 targetScale = originalScale * scaleEffectAmount;

        float elapsed = 0f;

        // Reducir escala
        while (elapsed < scaleEffectDuration / 2)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / (scaleEffectDuration / 2);
            transform.localScale = Vector3.Lerp(originalScale, targetScale, t);
            yield return null;
        }

        elapsed = 0f;

        // Restaurar escala
        while (elapsed < scaleEffectDuration / 2)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / (scaleEffectDuration / 2);
            transform.localScale = Vector3.Lerp(targetScale, originalScale, t);
            yield return null;
        }

        transform.localScale = originalScale;
    }

    void OnDestroy()
    {
        // Limpiar suscripciones al destruir
        if (spongeGrabInteractable != null)
        {
            spongeGrabInteractable.activated.RemoveListener(OnSpongeActivated);
        }
    }
}