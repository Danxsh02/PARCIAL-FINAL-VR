using System.Collections;
using UnityEngine;
using TMPro;

public class IntroJuego : MonoBehaviour
{
    [Header("UI")]
    public GameObject panelIntro;
    public GameObject canvasContenido;
    public TextMeshProUGUI textoIntro;

    [Header("Escritura")]
    [TextArea(4, 10)]
    public string[] lineasDeTexto;
    public float velocidadEscritura = 0.05f;
    public float esperaEntreLineas = 1.5f;
    public float esperaDespuesDePar = 2f; // Tiempo que espera antes de borrar
    [Range(1, 4)]
    public int lineasPorPantalla = 2; // Cuántas líneas mostrar antes de borrar

    void Start()
    {
        StartCoroutine(MostrarIntro());
    }

    IEnumerator MostrarIntro()
    {
        if (canvasContenido != null) canvasContenido.SetActive(false);
        if (panelIntro != null) panelIntro.SetActive(true);

        textoIntro.text = "";

        for (int i = 0; i < lineasDeTexto.Length; i++)
        {
            // Escribir la línea actual
            yield return StartCoroutine(EscribirLinea(lineasDeTexto[i]));

            // Si no es la última línea del grupo, agregar espacio
            if ((i + 1) % lineasPorPantalla != 0 && i < lineasDeTexto.Length - 1)
            {
                textoIntro.text += "\n\n";
                yield return new WaitForSeconds(esperaEntreLineas);
            }
            else
            {
                // Completamos un grupo de líneas
                yield return new WaitForSeconds(esperaDespuesDePar);

                // Si no es la última línea, borramos para el siguiente grupo
                if (i < lineasDeTexto.Length - 1)
                {
                    textoIntro.text = "";
                    yield return new WaitForSeconds(0.3f); // Pequeña pausa después de borrar
                }
            }
        }

        // Espera final antes de cerrar
        yield return new WaitForSeconds(2f);

        panelIntro.SetActive(false);
        if (canvasContenido != null) canvasContenido.SetActive(true);
    }

    IEnumerator EscribirLinea(string linea)
    {
        foreach (char letra in linea.ToCharArray())
        {
            textoIntro.text += letra;
            yield return new WaitForSeconds(velocidadEscritura);
        }
    }
}