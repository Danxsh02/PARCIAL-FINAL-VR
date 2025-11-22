using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class LoadScene : MonoBehaviour
{
    [Header("Nombre de la escena a cargar")]
    public string sceneToLoad = "NombreDeTuEscena";

    /// <summary>
    /// Carga la escena especificada.
    /// </summary>
    public void SceneLoad()
    {
        Debug.Log($"🔄 Cargando escena: {sceneToLoad}");
        SceneManager.LoadScene(sceneToLoad);
    }

    /// <summary>
    /// Sale del modo Play en el editor o cierra la aplicación en build.
    /// </summary>
    public void ExitSceneOrPlayMode()
    {
#if UNITY_EDITOR
        Debug.Log("⏹ Saliendo del modo Play en el editor...");
        EditorApplication.isPlaying = false;
#else
        Debug.Log("🚪 Cerrando la aplicación...");
        Application.Quit();
#endif
    }
}