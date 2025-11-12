using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("🔹 Configuración general")]
    public TextMeshProUGUI panelTexto;
    public GameObject prefabBolsa;

    [Header("📊 Datos de clasificación")]
    public Dictionary<ResiduoClasificable.TipoResiduo, ResiduoClasificable.ColorTarro> mapaClasificacion = new();
    public Dictionary<ResiduoClasificable.TipoResiduo, int> totalPorTipo = new();
    public Dictionary<ResiduoClasificable.TipoResiduo, int> depositadosPorTipo = new();

    private void Awake()
    {
        Instance = this;

        // Mapa de colores para cada tipo de residuo
        mapaClasificacion = new Dictionary<ResiduoClasificable.TipoResiduo, ResiduoClasificable.ColorTarro>
        {
            { ResiduoClasificable.TipoResiduo.Biologico, ResiduoClasificable.ColorTarro.Rojo },
            { ResiduoClasificable.TipoResiduo.Quimico, ResiduoClasificable.ColorTarro.Amarillo },
            { ResiduoClasificable.TipoResiduo.Reciclable, ResiduoClasificable.ColorTarro.Verde },
            { ResiduoClasificable.TipoResiduo.PapelLimpio, ResiduoClasificable.ColorTarro.Azul },
            { ResiduoClasificable.TipoResiduo.Ordinario, ResiduoClasificable.ColorTarro.Naranja }
        };
    }

    private void Start()
    {
        // Cuántos residuos necesita cada tipo para generar su bolsa
        totalPorTipo = new Dictionary<ResiduoClasificable.TipoResiduo, int>
        {
            { ResiduoClasificable.TipoResiduo.Biologico, 1 },
            { ResiduoClasificable.TipoResiduo.Quimico, 2 },
            { ResiduoClasificable.TipoResiduo.Reciclable, 4 },
            { ResiduoClasificable.TipoResiduo.PapelLimpio, 2 },
            { ResiduoClasificable.TipoResiduo.Ordinario, 3 }
        };

        ActualizarPanel();
    }

    // 🔸 Registrar cada residuo depositado correctamente
    public void RegistrarDeposito(ResiduoClasificable.TipoResiduo tipo)
    {
        if (!depositadosPorTipo.ContainsKey(tipo))
            depositadosPorTipo[tipo] = 0;

        depositadosPorTipo[tipo]++;

        Debug.Log($"✅ {tipo} clasificado correctamente ({depositadosPorTipo[tipo]}/{totalPorTipo[tipo]})");

        // Verificar si se completó la cantidad requerida
        if (depositadosPorTipo[tipo] >= totalPorTipo[tipo])
        {
            Debug.Log($"🎉 ¡{tipo} completado!");
            GenerarBolsaParabolica(tipo);
        }

        ActualizarPanel();
    }

    // 🔹 Actualizar texto del panel
    void ActualizarPanel()
    {
        if (panelTexto == null) return;

        string texto = "";
        foreach (var tipo in totalPorTipo.Keys)
        {
            int faltan = totalPorTipo[tipo] - depositadosPorTipo.GetValueOrDefault(tipo, 0);
            texto += $"{tipo}: {(faltan > 0 ? $"Faltan {faltan}" : "¡Completado!")}\n";
        }

        panelTexto.text = texto;
        Debug.Log(texto);
    }

    // 👜 Generar bolsa con color y trayectoria parabólica desde el SpawnPoint
    private void GenerarBolsaParabolica(ResiduoClasificable.TipoResiduo tipo)
    {
        if (prefabBolsa == null)
        {
            Debug.LogWarning("⚠️ Falta asignar el prefabBolsa en el Inspector.");
            return;
        }

        // Buscar el tarro correspondiente a ese tipo
        ResiduoClasificable tarroOrigen = null;
        foreach (var t in FindObjectsOfType<ResiduoClasificable>())
        {
            if (t.esTarro && t.tipoAceptado == tipo)
            {
                tarroOrigen = t;
                break;
            }
        }

        if (tarroOrigen == null)
        {
            Debug.LogWarning($"⚠️ No se encontró tarro para el tipo {tipo}");
            return;
        }

        // ✅ Buscar el punto de spawn dentro del tarro
        Transform spawnPoint = tarroOrigen.transform.Find("SpawnPoint");
        Vector3 spawnPos;
        Quaternion spawnRot;

        if (spawnPoint != null)
        {
            spawnPos = spawnPoint.position;
            spawnRot = spawnPoint.rotation;
        }
        else
        {
            // fallback por si no existe el SpawnPoint
            spawnPos = tarroOrigen.transform.position + tarroOrigen.transform.up * 0.45f;
            spawnRot = tarroOrigen.transform.rotation;
            Debug.LogWarning($"⚠️ El tarro {tarroOrigen.name} no tiene SpawnPoint, usando posición por defecto.");
        }

        // Instanciar la bolsa en el punto exacto
        GameObject bolsa = Instantiate(prefabBolsa, spawnPos, spawnRot);
        bolsa.name = $"Bolsa_{tipo}";

        // Aplicar color dinámico según el tipo de residuo
        Renderer renderer = bolsa.GetComponentInChildren<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = ObtenerColorDelResiduo(tipo);
        }

        // Configurar físicas
        Rigidbody rb = bolsa.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = true;
            rb.isKinematic = false;

            // Dirección de salida = eje Z del SpawnPoint + leve inclinación hacia arriba
            Vector3 direccion = (spawnRot * Vector3.forward + Vector3.up * 0.2f).normalized;
            float fuerza = 6.5f; // ajusta según la masa del Rigidbody
            rb.AddForce(direccion * fuerza, ForceMode.Impulse);

            // Pequeña rotación aleatoria para que no salga rígida
            rb.AddTorque(Random.insideUnitSphere * 2f, ForceMode.Impulse);

            // Animación de aparición
            bolsa.transform.localScale = Vector3.zero;
            StartCoroutine(AnimarAparicion(bolsa.transform));
        }
        else
        {
            Debug.LogWarning($"⚠️ La bolsa {bolsa.name} no tiene Rigidbody asignado.");
        }

        Debug.Log($"👜 Bolsa de {tipo} lanzada desde SpawnPoint de {tarroOrigen.name}");
    }

    // ✨ Animación de aparición (pop suave)
    private IEnumerator AnimarAparicion(Transform obj)
    {
        float duracion = 0.25f;
        float tiempo = 0f;
        Vector3 escalaInicial = Vector3.zero;
        Vector3 escalaFinal = Vector3.one;

        while (tiempo < duracion)
        {
            float t = tiempo / duracion;
            float curva = Mathf.Sin(t * Mathf.PI * 0.5f); // suavizado
            obj.localScale = Vector3.Lerp(escalaInicial, escalaFinal, curva);

            tiempo += Time.deltaTime;
            yield return null;
        }

        obj.localScale = escalaFinal;
    }

    // 🎨 Obtener color según tipo
    private Color ObtenerColorDelResiduo(ResiduoClasificable.TipoResiduo tipo)
    {
        switch (mapaClasificacion[tipo])
        {
            case ResiduoClasificable.ColorTarro.Rojo: return new Color(0.8f, 0.1f, 0.1f);
            case ResiduoClasificable.ColorTarro.Amarillo: return new Color(1f, 0.9f, 0.1f);
            case ResiduoClasificable.ColorTarro.Verde: return new Color(0.1f, 0.8f, 0.1f);
            case ResiduoClasificable.ColorTarro.Azul: return new Color(0.1f, 0.4f, 0.9f);
            case ResiduoClasificable.ColorTarro.Naranja: return new Color(1f, 0.55f, 0f);
            default: return Color.white;
        }
    }
}
