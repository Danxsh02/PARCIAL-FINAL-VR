using System.Collections;
using UnityEngine;
using TMPro;

public class IntroJuego : MonoBehaviour
{
    [Header("UI")]
    public GameObject panelIntro;
    public TextMeshProUGUI textoIntro;

    [Header("Escritura")]
    [TextArea(4, 10)]
    public string[] lineasDeTexto;
    public float velocidadEscritura = 0.05f;
    public float esperaEntreLineas = 1.5f;
    public float esperaDespuesDePar = 2f;
    [Range(1, 4)]
    public int lineasPorPantalla = 2;

    void Start()
    {
        StartCoroutine(MostrarIntro());
    }

    IEnumerator MostrarIntro()
    {
        // Muestra el panel
        if (panelIntro != null) panelIntro.SetActive(true);

        textoIntro.text = "";

        for (int i = 0; i < lineasDeTexto.Length; i++)
        {
            yield return StartCoroutine(EscribirLinea(lineasDeTexto[i]));

            if ((i + 1) % lineasPorPantalla != 0 && i < lineasDeTexto.Length - 1)
            {
                textoIntro.text += "\n\n";
                yield return new WaitForSeconds(esperaEntreLineas);
            }
            else
            {
                yield return new WaitForSeconds(esperaDespuesDePar);

                if (i < lineasDeTexto.Length - 1)
                {
                    textoIntro.text = "";
                    yield return new WaitForSeconds(0.3f);
                }
            }
        }

        // Espera final antes de cerrar
        yield return new WaitForSeconds(2f);

        //  Oculta el panel al final del dialogo
        panelIntro.SetActive(false);
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
