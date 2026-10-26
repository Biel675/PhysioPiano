using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
public class StartMenu : MonoBehaviour
{
    private const string PIANO_SCENE_NAME = "Piano Scene";
    [SerializeField] private GameObject songSelectionMenu;
    
    public void OnClickPractice()
    {
        SceneManager.LoadScene(PIANO_SCENE_NAME);
        Debug.Log("Loading scene: " + PIANO_SCENE_NAME);
    }
    
    public void OnClickSelectSongs()
    {
        gameObject.SetActive(false);
        songSelectionMenu.SetActive(true);
    }
    
    public void OnClickQuit()
    {
        #if UNITY_EDITOR
            EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}