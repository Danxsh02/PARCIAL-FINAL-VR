using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    // Nuevo: registros de incorrectos
    public Dictionary<ResiduoClasificable.TipoResiduo, int> depositadosIncorrectosPorTipo = new();
    public int erroresTotales = 0;

    [Header("Prefabs de bolsas por tipo")]
    public GameObject bolsaRoja;
    public GameObject bolsaAmarilla;
    public GameObject bolsaVerde;
    public GameObject bolsaAzul;
    public GameObject bolsaNaranja;

    [Header("Conteo de residuos")]
    [SerializeField] private int totalResiduosEscena;
    [SerializeField] private int residuosRecogidos;

    [Header("Conteo de bolsas")]
    [SerializeField] private int totalBolsasEscena;
    [SerializeField] private int bolsasDepositadas;

    [Header("Evento final")]
    [SerializeField] private bool eventoActivado = false;

    // ---- Campos para finales ----
    [Header("Final bueno")]
    public AudioSource telefonoAudio;
    public GameObject panelFinalBueno; // panel UI con mensaje bueno

    [Header("Final malo (video 360)")]
    public Transform posicionVideo360; // punto dentro del "shader/video360"
    public Transform jugador; // XR Origin o Main Camera parent que teletransportarás
    public GameObject panelFinalMalo; // panel UI con mensaje malo
    public GameObject video360GameObject; // objeto controlador del video 360 (activar/reproducir)

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
        // Cuenta la cantidad de residuos al iniciar
        var residuos = FindObjectsByType<ResiduoClasificable>(FindObjectsSortMode.None);
        totalResiduosEscena = residuos.Count(r => !r.esTarro);

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

    public void RegistrarResiduoRecogido()
    {
        residuosRecogidos++;
        RevisarFinDelJuego();
    }

    private GameObject ObtenerPrefabBolsa(ResiduoClasificable.TipoResiduo tipo)
    {
        switch (tipo)
        {
            case ResiduoClasificable.TipoResiduo.Biologico: return bolsaRoja;
            case ResiduoClasificable.TipoResiduo.Quimico: return bolsaAmarilla;
            case ResiduoClasificable.TipoResiduo.Reciclable: return bolsaVerde;
            case ResiduoClasificable.TipoResiduo.PapelLimpio: return bolsaAzul;
            case ResiduoClasificable.TipoResiduo.Ordinario: return bolsaNaranja;
            default: return bolsaRoja;
        }
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

    // ---- Nuevo: Registrar error (deposito incorrecto) ----
    public void RegistrarError(ResiduoClasificable.TipoResiduo tipo)
    {
        if (!depositadosIncorrectosPorTipo.ContainsKey(tipo))
            depositadosIncorrectosPorTipo[tipo] = 0;

        depositadosIncorrectosPorTipo[tipo]++;
        erroresTotales++;

        Debug.Log($"❌ ERROR registrado para {tipo}. Total errores: {erroresTotales}");
        ActualizarPanel(); // opcional: muestra cambios en panel si quieres
    }

    // 🔹 Actualizar texto del panel
    void ActualizarPanel()
    {
        if (panelTexto == null) return;

        string texto = "";
        foreach (var tipo in totalPorTipo.Keys)
        {
            int correctos = depositadosPorTipo.GetValueOrDefault(tipo, 0);
            int faltan = totalPorTipo[tipo] - correctos;
            int incorrectos = depositadosIncorrectosPorTipo.GetValueOrDefault(tipo, 0);

            texto += $"{tipo}: {(faltan > 0 ? $"Faltan {faltan}" : "¡Completado!")} (err: {incorrectos})\n";
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
        GameObject prefab = ObtenerPrefabBolsa(tipo);
        GameObject bolsa = Instantiate(prefab, spawnPos, spawnRot);

        bolsa.name = $"Bolsa_{tipo}";

        // Registrar bolsa creada
        totalBolsasEscena++;

        // Aplicar color dinámico según el tipo de residuo
        Renderer renderer = bolsa.GetComponentInChildren<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = ObtenerColorDelResiduo(tipo);
        }

        // 🔹 DESACTIVAR COLLIDER TEMPORALMENTE para evitar atasco
        Collider bolsaCollider = bolsa.GetComponent<Collider>();
        if (bolsaCollider != null)
        {
            bolsaCollider.enabled = false;
            StartCoroutine(ActivarColliderDespues(bolsaCollider, 0.3f));
        }

        // Configurar físicas
        Rigidbody rb = bolsa.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = true;
            rb.isKinematic = false;

            // Dirección de salida = eje Z del SpawnPoint + leve inclinación hacia arriba
            Vector3 direccion = (spawnRot * Vector3.forward + Vector3.up * 0.15f).normalized;
            float fuerza = 3.5f; // ajustada para lanzamiento más suave
            rb.AddForce(direccion * fuerza, ForceMode.Impulse);

            // Pequeña rotación aleatoria para que no salga rígida
            rb.AddTorque(Random.insideUnitSphere * 1f, ForceMode.Impulse);

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

    // ⏱️ Activar el collider después de un delay
    private IEnumerator ActivarColliderDespues(Collider col, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (col != null)
            col.enabled = true;
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

    public void RegistrarBolsaDepositada()
    {
        bolsasDepositadas++;
        RevisarFinDelJuego();
    }

    private void RevisarFinDelJuego()
    {
        if (eventoActivado) return;

        bool todosResiduosRecogidos = residuosRecogidos >= totalResiduosEscena;
        bool todasBolsasDepositadas = bolsasDepositadas >= totalBolsasEscena;

        if (todosResiduosRecogidos && todasBolsasDepositadas)
        {
            eventoActivado = true;
            ActivarEventoFinal();
        }
    }

    private void ActivarEventoFinal()
    {
        Debug.Log("🎉 TODOS LOS RESIDUOS recogidos y TODAS LAS BOLSAS depositadas");

        // Elegir final según erroresTotales
        if (erroresTotales == 0)
        {
            Debug.Log("🏆 FINAL BUENO: Clasificación perfecta");
            StartCoroutine(FinalBueno());
        }
        else
        {
            Debug.Log("💀 FINAL MALO: Hubo errores en la clasificación");
            StartCoroutine(FinalMalo());
        }
    }

    // ------------------ Corutinas de final ------------------

    private IEnumerator FinalBueno()
    {
        // Pequeña espera para dramatizar
        yield return new WaitForSeconds(1.2f);

        // Reproducir sonido del teléfono si está asignado
        if (telefonoAudio != null)
            telefonoAudio.Play();

        // Mostrar panel final bueno
        if (panelFinalBueno != null)
            panelFinalBueno.SetActive(true);

        // Opcional: puedes pausar input, bloquear movimiento, etc.
        Debug.Log("🏆 Mostrado panel final bueno");
    }

    private IEnumerator FinalMalo()
    {
        yield return new WaitForSeconds(1.0f);

        // Teletransportar al jugador dentro del espacio del video 360
        if (jugador != null && posicionVideo360 != null)
        {
            // Opcional: guardar rotación anterior si quieres restaurarla luego
            jugador.position = posicionVideo360.position;
            jugador.rotation = posicionVideo360.rotation;
        }

        // Activar/arrancar el video 360 si tienes un GameObject que lo controla
        if (video360GameObject != null)
            video360GameObject.SetActive(true);

        // Mostrar panel final malo
        if (panelFinalMalo != null)
            panelFinalMalo.SetActive(true);

        Debug.Log("💀 Teletransportado al jugador y mostrado panel final malo");
    }
}
