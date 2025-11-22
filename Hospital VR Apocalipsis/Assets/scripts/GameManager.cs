using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Texto de Descripcion")]
    [TextArea(3, 10)]
    public string textoDescripcion = "Clasifica los residuos correctamente:\n\n" +
                                      "ROJO: Biologicos\n" +
                                      "AMARILLO: Quimicos\n" +
                                      "VERDE: Reciclables\n" +
                                      "AZUL: Papel Limpio\n" +
                                      "NARANJA: Ordinarios";

    public TextMeshProUGUI panelTexto;

    [Header("Panel de Reporte Final")]
    public TextMeshProUGUI textoReporte;

    [Header("Recompensas por Rendimiento")]
    [TextArea(2, 3)]
    public string recompensa100 = "¡CONTRATADO! Salario: $2.000.000 mensuales";
    [TextArea(2, 3)]
    public string recompensa50 = "Contrato temporal. Salario: $1.000.000 mensuales";
    [TextArea(2, 3)]
    public string recompensa0 = "No fuiste contratado. Intenta de nuevo.";

    [Header("Prefabs de bolsas por tipo")]
    public GameObject bolsaRoja;
    public GameObject bolsaAmarilla;
    public GameObject bolsaVerde;
    public GameObject bolsaAzul;
    public GameObject bolsaNaranja;

    [Header("Datos de clasificacion de basura")]
    public Dictionary<ResiduoClasificable.TipoResiduo, ResiduoClasificable.ColorTarro> mapaClasificacion = new();
    public Dictionary<ResiduoClasificable.TipoResiduo, int> totalPorTipo = new();
    public Dictionary<ResiduoClasificable.TipoResiduo, int> depositadosPorTipo = new();

    [Header("Conteo de residuos")]
    [SerializeField] private int totalResiduosEscena;
    [SerializeField] private int residuosRecogidos;

    [Header("Conteo de bolsas")]
    [SerializeField] private int totalBolsasEscena;
    [SerializeField] private int bolsasDepositadas;

    [Header("Conteo de manchas")]
    [SerializeField] private int totalManchasEscena;
    [SerializeField] private int manchasLimpiadas;

    [Header("Logros")]
    public GameObject imagenLogroResiduos; 
    public GameObject imagenLogroBolsas; 
    public GameObject imagenLogroManchas; 
    public GameObject imagenLogroPerfecto; 

    [SerializeField] private int intentosReporte = 0;
    [SerializeField] private bool logroResiduosObtenido = false;
    [SerializeField] private bool logroBolsasObtenido = false;
    [SerializeField] private bool logroManchasObtenido = false;
    [SerializeField] private bool logroPerfectoObtenido = false;

    [Header("Limite de Intentos")]
    public int maxIntentos = 3;
    public string nombreEscenaFinal = "FinalMalo"; 
    public string nombreEscenaPerfecta = "FinalBueno"; 
    public float tiempoEsperaAntesCambio = 10f; 

    [Header("Evento final")]
    [SerializeField] private bool eventoActivado = false;

    [Header("Hack")]
    public bool cheatsModoDesarrolladorActivo = true;
    public KeyCode teclaTrampa = KeyCode.Alpha8;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        // colores para cada tipo de residuo
        mapaClasificacion = new Dictionary<ResiduoClasificable.TipoResiduo, ResiduoClasificable.ColorTarro>
        {
            { ResiduoClasificable.TipoResiduo.Biologico, ResiduoClasificable.ColorTarro.Rojo },
            { ResiduoClasificable.TipoResiduo.Quimico, ResiduoClasificable.ColorTarro.Amarillo },
            { ResiduoClasificable.TipoResiduo.Organico, ResiduoClasificable.ColorTarro.Verde },
            { ResiduoClasificable.TipoResiduo.Reciclable, ResiduoClasificable.ColorTarro.Azul },
            { ResiduoClasificable.TipoResiduo.Ordinario, ResiduoClasificable.ColorTarro.Naranja }
        };
    }

    private void Start()
    {
        if (panelTexto != null)
        {
            panelTexto.text = textoDescripcion;
        }

        // Limpiar el panel de reporte al inicio
        if (textoReporte != null)
        {
            textoReporte.text = "Presiona el botón para finalizar";
        }

        // Ocultar imágenes de logros al inicio
        if (imagenLogroResiduos != null) imagenLogroResiduos.SetActive(false);
        if (imagenLogroBolsas != null) imagenLogroBolsas.SetActive(false);
        if (imagenLogroManchas != null) imagenLogroManchas.SetActive(false);
        if (imagenLogroPerfecto != null) imagenLogroPerfecto.SetActive(false);

        // cantidad de residuos al iniciar
        var residuos = FindObjectsByType<ResiduoClasificable>(FindObjectsSortMode.None);
        totalResiduosEscena = residuos.Count(r => !r.esTarro);

        // cantidad de manchas al iniciar
        var manchas = FindObjectsByType<BloodStainFade>(FindObjectsSortMode.None);
        totalManchasEscena = manchas.Length;

        // residuos necesita cada tipo para generar su bolsa
        totalPorTipo = new Dictionary<ResiduoClasificable.TipoResiduo, int>
        {
            { ResiduoClasificable.TipoResiduo.Biologico, 3 },
            { ResiduoClasificable.TipoResiduo.Quimico, 3 },
            { ResiduoClasificable.TipoResiduo.Organico, 3 },
            { ResiduoClasificable.TipoResiduo.Reciclable, 3 },
            { ResiduoClasificable.TipoResiduo.Ordinario, 3 }
        };

        Debug.Log($"GameManager iniciado - Total de residuos: {totalResiduosEscena} | Manchas: {totalManchasEscena}");
    }

    // Registrar cada residuo depositado 
    public void RegistrarDeposito(ResiduoClasificable.TipoResiduo tipo)
    {
        if (!depositadosPorTipo.ContainsKey(tipo))
            depositadosPorTipo[tipo] = 0;

        depositadosPorTipo[tipo]++;

        Debug.Log($"Clasificado correctamente: {tipo} ({depositadosPorTipo[tipo]}/{totalPorTipo[tipo]})");

        // Verificar si se completo la cantidad 
        if (depositadosPorTipo[tipo] >= totalPorTipo[tipo])
        {
            Debug.Log($"{tipo} completado");
            GenerarBolsaParabolica(tipo);
        }
    }

    private void Update()
    {
        // Completar todo de una
        if (cheatsModoDesarrolladorActivo && Input.GetKeyDown(teclaTrampa))
        {
            CheatCompletarTodo();
        }
    }
    public void RegistrarResiduoRecogido()
    {
        residuosRecogidos++;
        Debug.Log($"Residuo recogido, Total: {residuosRecogidos}/{totalResiduosEscena}");
        RevisarFinDelJuego();
    }

    public void RegistrarBolsaDepositada()
    {
        bolsasDepositadas++;
        Debug.Log($"Bolsa depositada, Total bolsas: {bolsasDepositadas}/{totalBolsasEscena}");
        RevisarFinDelJuego();
    }

    public void RegistrarManchaLimpiada()
    {
        manchasLimpiadas++;
        Debug.Log($"Mancha limpiada, Total: {manchasLimpiadas}/{totalManchasEscena}");
        RevisarFinDelJuego();
    }

    private GameObject ObtenerPrefabBolsa(ResiduoClasificable.TipoResiduo tipo)
    {
        switch (tipo)
        {
            case ResiduoClasificable.TipoResiduo.Biologico: return bolsaRoja;
            case ResiduoClasificable.TipoResiduo.Quimico: return bolsaAmarilla;
            case ResiduoClasificable.TipoResiduo.Organico: return bolsaVerde;
            case ResiduoClasificable.TipoResiduo.Reciclable: return bolsaAzul;
            case ResiduoClasificable.TipoResiduo.Ordinario: return bolsaNaranja;
            default: return bolsaRoja;
        }
    }

    // Generar bolsa con color 
    private void GenerarBolsaParabolica(ResiduoClasificable.TipoResiduo tipo)
    {
        // Busca el tarro correspondiente a ese tipo
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
            Debug.LogWarning($"No se encontró tarro para el tipo {tipo}");
            return;
        }

        // Busca el punto de spawn dentro del tarro
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
            Debug.LogWarning($"El tarro {tarroOrigen.name} no tiene Spawn, usando posicion por defecto.");
        }

        // Instanciar la bolsa en el punto exacto
        GameObject prefab = ObtenerPrefabBolsa(tipo);
        if (prefab == null)
        {
            Debug.LogWarning($"No hay prefab de bolsa asignado para {tipo}");
            return;
        }

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

        // DESACTIVAR COLLIDER TEMPORALMENTE para evitar atasco
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
            float fuerza = 3.5f;
            rb.AddForce(direccion * fuerza, ForceMode.Impulse);

            // Pequeña rotación aleatoria
            rb.AddTorque(Random.insideUnitSphere * 1f, ForceMode.Impulse);

            // Animación de aparición
            bolsa.transform.localScale = Vector3.zero;
            StartCoroutine(AnimarAparicion(bolsa.transform));
        }
        else
        {
            Debug.LogWarning($"La bolsa {bolsa.name} no tiene Rigidbody asignado.");
        }

        Debug.Log($"Bolsa de {tipo} lanzada desde SpawnPoint de {tarroOrigen.name}");
    }

    // Animacion de aparicion (pop suave)
    private IEnumerator AnimarAparicion(Transform obj)
    {
        float duracion = 0.25f;
        float tiempo = 0f;
        Vector3 escalaInicial = Vector3.zero;
        Vector3 escalaFinal = Vector3.one;

        while (tiempo < duracion)
        {
            float t = tiempo / duracion;
            float curva = Mathf.Sin(t * Mathf.PI * 0.5f);
            obj.localScale = Vector3.Lerp(escalaInicial, escalaFinal, curva);

            tiempo += Time.deltaTime;
            yield return null;
        }

        obj.localScale = escalaFinal;
    }

    // Activar el collider despues de un delay
    private IEnumerator ActivarColliderDespues(Collider col, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (col != null)
            col.enabled = true;
    }

    // Obtener color segun tipo
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

    private void RevisarFinDelJuego()
    {
        if (eventoActivado) return;

        bool todosResiduosRecogidos = residuosRecogidos >= totalResiduosEscena;
        bool todasBolsasDepositadas = bolsasDepositadas >= totalBolsasEscena;
        bool todasManchasLimpiadas = manchasLimpiadas >= totalManchasEscena;

        Debug.Log($"Revision: Residuos {residuosRecogidos}/{totalResiduosEscena} | Bolsas {bolsasDepositadas}/{totalBolsasEscena} | Manchas {manchasLimpiadas}/{totalManchasEscena}");

        if (todosResiduosRecogidos && todasBolsasDepositadas && todasManchasLimpiadas)
        {
            eventoActivado = true;
            ActivarEventoFinal();
        }
    }

    private void ActivarEventoFinal()
    {
        Debug.Log("Tarea completada, Todos los residuos clasificados, bolsas depositadas y manchas limpiadas");
    }

    public void FinalizarJuego()
    {
        intentosReporte++;
        Debug.Log($"Finalizando juego - Intento #{intentosReporte}");

        // Calcular totales primero
        int totalTareas = totalResiduosEscena + totalBolsasEscena + totalManchasEscena;
        int tareasCompletadas = residuosRecogidos + bolsasDepositadas + manchasLimpiadas;

        float porcentaje = (totalTareas > 0) ?
            ((float)tareasCompletadas / totalTareas) * 100f : 0f;

        Debug.Log($"Porcentaje total: {porcentaje:F1}%");

        // verifica si trodo se completo al 100
        bool completoTodo = porcentaje >= 100f;

        // Verificar logros solo en el primer intento
        if (intentosReporte == 1)
        {
            VerificarLogros();
        }

        // Si completo todo para ir al final bueno
        if (completoTodo)
        {
            Debug.Log($"completado todo en el intento #{intentosReporte}! Redirigiendo a FinalBueno");

            // Mostrar mensaje de éxito antes de cambiar
            if (textoReporte != null)
            {
                string reporteExito = "<size=48><b>¡EXCELENTE!</b></size>\n\n";
                reporteExito += $"<size=28>Porcentaje: <b>100%</b></size>\n";
                reporteExito += $"<size=16>Completado en intento #{intentosReporte}</size>\n\n";
                reporteExito += $"<size=20>Bolsas: {bolsasDepositadas}/{totalBolsasEscena} ✓</size>\n";
                reporteExito += $"<size=20>Residuos: {residuosRecogidos}/{totalResiduosEscena} ✓</size>\n";
                reporteExito += $"<size=20>Manchas: {manchasLimpiadas}/{totalManchasEscena} ✓</size>\n\n";

                // Mostrar logros si los obtuvo en el primero
                if (intentosReporte == 1 && (logroResiduosObtenido || logroBolsasObtenido || logroManchasObtenido || logroPerfectoObtenido))
                {
                    reporteExito += "<size=18><b> Logros Desbloqueados:</b></size>\n";
                    if (logroPerfectoObtenido)
                    {
                        reporteExito += "<size=16> Trabajo Perfecto (100% al primer intento)</size>\n";
                    }
                    else
                    {
                        if (logroResiduosObtenido) reporteExito += "<size=16>✓ Clasificador Experto</size>\n";
                        if (logroBolsasObtenido) reporteExito += "<size=16>✓ Depositador Pro</size>\n";
                        if (logroManchasObtenido) reporteExito += "<size=16>✓ Limpiador Perfecto</size>\n";
                    }
                    reporteExito += "\n";
                }

                reporteExito += $"<size=22><b>{recompensa100}</b></size>";
                textoReporte.text = reporteExito;
            }

            StartCoroutine(CambiarEscenaDespuesDeTiempo(nombreEscenaPerfecta));
            return; // Salir para ir a FinalBueno
        }

        // Si 3 intento y no completo para el final malo
        if (intentosReporte >= 3)
        {
            Debug.Log("Intento 3 fallido, yendo al Final Malo");

            if (textoReporte != null)
            {
                string reporteFallo = "<size=48><b>INSUFICIENTE</b></size>\n\n";
                reporteFallo += $"<size=28>Porcentaje: <b>{porcentaje:F1}%</b></size>\n";
                reporteFallo += $"<size=16>Intento final (3/3)</size>\n\n";
                reporteFallo += $"<size=20>Bolsas: {bolsasDepositadas}/{totalBolsasEscena}</size>\n";
                reporteFallo += $"<size=20>Residuos: {residuosRecogidos}/{totalResiduosEscena}</size>\n";
                reporteFallo += $"<size=20>Manchas: {manchasLimpiadas}/{totalManchasEscena}</size>\n\n";
                reporteFallo += $"<size=22><b>{recompensa0}</b></size>";
                textoReporte.text = reporteFallo;
            }

            StartCoroutine(CambiarEscenaDespuesDeTiempo(nombreEscenaFinal));
            return;
        }

        //  Mostrar el reporte del los intentos de las tareas
        string resultado = "";
        string recompensa = "";

        if (porcentaje >= 50f)
        {
            resultado = "ACEPTABLE";
            recompensa = recompensa50;
        }
        else
        {
            resultado = "INSUFICIENTE";
            recompensa = recompensa0;
        }

        // Generar texto del reporte
        string reporte = $"<size=48><b>{resultado}</b></size>\n\n";
        reporte += $"<size=28>Porcentaje: <b>{porcentaje:F1}%</b></size>\n";
        reporte += $"<size=16>Intento #{intentosReporte} de 3</size>\n\n";

        // Resumen
        reporte += $"<size=20>Bolsas: {bolsasDepositadas}/{totalBolsasEscena}</size>\n";
        reporte += $"<size=20>Residuos: {residuosRecogidos}/{totalResiduosEscena}</size>\n";
        reporte += $"<size=20>Manchas: {manchasLimpiadas}/{totalManchasEscena}</size>\n\n";

        reporte += $"<size=18><color=yellow>Te quedan {3 - intentosReporte} intento(s)</color></size>\n\n";
        reporte += $"<size=22><b>{recompensa}</b></size>";

        // Mostrar panel de reporte
        if (textoReporte != null)
        {
            textoReporte.text = reporte;
        }

        Debug.Log("Reporte mostrado. El jugador puede seguir intentando.");
    }

    private void VerificarLogros()
    {
        Debug.Log("Verificando logros del primer intento");

        // Logro: Completar todos los residuos
        if (residuosRecogidos >= totalResiduosEscena && totalResiduosEscena > 0)
        {
            logroResiduosObtenido = true;
            if (imagenLogroResiduos != null)
            {
                imagenLogroResiduos.SetActive(true);
                Debug.Log("Logro desbloqueado: Clasificador Experto");
            }
        }

        // Logro: Completar todas las bolsas
        if (bolsasDepositadas >= totalBolsasEscena && totalBolsasEscena > 0)
        {
            logroBolsasObtenido = true;
            if (imagenLogroBolsas != null)
            {
                imagenLogroBolsas.SetActive(true);
                Debug.Log("Logro desbloqueado, Depositador Pro");
            }
        }

        // Logro: Completar todas las manchas
        if (manchasLimpiadas >= totalManchasEscena && totalManchasEscena > 0)
        {
            logroManchasObtenido = true;
            if (imagenLogroManchas != null)
            {
                imagenLogroManchas.SetActive(true);
                Debug.Log("Logro desbloqueado, Limpiador Perfecto");
            }
        }

        // Logro Perfecto: Completar las 3 tareas al primer intento
        if (logroResiduosObtenido && logroBolsasObtenido && logroManchasObtenido)
        {
            logroPerfectoObtenido = true;
            if (imagenLogroPerfecto != null)
            {
                imagenLogroPerfecto.SetActive(true);
                Debug.Log("logro desbloqueado, Trabajo perfecto");
            }

            // Ocultar los logros individuales si se consiguió el perfecto
            if (imagenLogroResiduos != null) imagenLogroResiduos.SetActive(false);
            if (imagenLogroBolsas != null) imagenLogroBolsas.SetActive(false);
            if (imagenLogroManchas != null) imagenLogroManchas.SetActive(false);
        }
    }

    // Metodo publico para cargar la escena final (para botones en el panel)
    public void IrAEscenaFinal()
    {
        Debug.Log($"Cargando escena final: {nombreEscenaFinal}");
        SceneManager.LoadScene(nombreEscenaFinal);
    }

    // Coroutine para cambiar de escena después de un tiempo
    private IEnumerator CambiarEscenaDespuesDeTiempo(string nombreEscena)
    {
        Debug.Log($"Esperando {tiempoEsperaAntesCambio} segundos antes de cambiar a {nombreEscena}...");
        yield return new WaitForSeconds(tiempoEsperaAntesCambio);
        Debug.Log($"Cargando escena: {nombreEscena}");
        SceneManager.LoadScene(nombreEscena);
    }

    public void CheatCompletarTodo()
    {
        Debug.Log(" Hack, complentando todas las tareas");

        //  Marcar todos los residuos como recogidos
        residuosRecogidos = totalResiduosEscena;
        Debug.Log($" Residuos completados: {residuosRecogidos}/{totalResiduosEscena}");

        // Marcar todas las bolsas como depositadas
        bolsasDepositadas = totalBolsasEscena;
        Debug.Log($" Bolsas completadas: {bolsasDepositadas}/{totalBolsasEscena}");

        // Marcar todas las manchas como limpiadas
        manchasLimpiadas = totalManchasEscena;
        Debug.Log($" Manchas completadas: {manchasLimpiadas}/{totalManchasEscena}");

        // Revisar si se completó el juego
        RevisarFinDelJuego();

        Debug.Log("Hack al 100%, Presionar boton final");
    }
}