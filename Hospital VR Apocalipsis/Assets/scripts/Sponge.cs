using UnityEngine;

public enum SpongeState
{
    Dry,
    Wet,
    Dirty
}

[RequireComponent(typeof(Renderer))]
public class Sponge : MonoBehaviour
{
    [Header("Estado actual de la esponja")]
    public SpongeState currentState = SpongeState.Wet; // Comenzar mojada para poder limpiar

    [Header("Materiales para cada estado")]
    public Material dryMaterial;
    public Material wetMaterial;
    public Material dirtyMaterial;

    [Header("Debug")]
    public bool showDebugLogs = true;

    private Renderer rend;

    private void Start()
    {
        rend = GetComponent<Renderer>();
        UpdateMaterial();

        if (showDebugLogs)
        {
            Debug.Log($"Esponja inicializada. Estado: {currentState}");
        }
    }

    /// <summary>
    /// Cambia el estado de la esponja
    /// </summary>
    public void SetState(SpongeState newState)
    {
        if (showDebugLogs)
        {
            Debug.Log($"Esponja cambió de {currentState} a {newState}");
        }

        currentState = newState;
        UpdateMaterial();
    }

    /// <summary>
    /// Actualiza el material según el estado actual
    /// </summary>
    private void UpdateMaterial()
    {
        if (rend == null) return;

        switch (currentState)
        {
            case SpongeState.Dry:
                if (dryMaterial != null)
                {
                    rend.material = dryMaterial;
                }
                else if (showDebugLogs)
                {
                    Debug.LogWarning("Material Dry no asignado en la esponja");
                }
                break;

            case SpongeState.Wet:
                if (wetMaterial != null)
                {
                    rend.material = wetMaterial;
                }
                else if (showDebugLogs)
                {
                    Debug.LogWarning("Material Wet no asignado en la esponja");
                }
                break;

            case SpongeState.Dirty:
                if (dirtyMaterial != null)
                {
                    rend.material = dirtyMaterial;
                }
                else if (showDebugLogs)
                {
                    Debug.LogWarning("Material Dirty no asignado en la esponja");
                }
                break;
        }
    }

    /// <summary>
    /// Verifica si la esponja puede limpiar (debe estar mojada)
    /// </summary>
    public bool CanClean()
    {
        return currentState == SpongeState.Wet;
    }

    /// <summary>
    /// Usa la esponja (la ensucia si está mojada)
    /// </summary>
    public void Use()
    {
        if (currentState == SpongeState.Wet)
        {
            SetState(SpongeState.Dirty);

            if (showDebugLogs)
            {
                Debug.Log("Esponja usada - ahora está sucia");
            }
        }
    }

    /// <summary>
    /// Moja la esponja (útil para resetear o mojar en un balde)
    /// </summary>
    public void WetSponge()
    {
        SetState(SpongeState.Wet);

        if (showDebugLogs)
        {
            Debug.Log("Esponja mojada - lista para limpiar");
        }
    }

    /// <summary>
    /// Seca la esponja
    /// </summary>
    public void DrySponge()
    {
        SetState(SpongeState.Dry);

        if (showDebugLogs)
        {
            Debug.Log("Esponja secada");
        }
    }
}