using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public const string PIANO_SCENE_NAME = "Piano Scene";
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject selectSongMenu;
    [SerializeField] private GameObject selectPianoMenu;

    [SerializeField] private Transform selectSongScrollViewContent;

    [SerializeField] private GameObject songNameButtonPrefab;

    private bool _loadedSongs = false;

    private void ShowMainMenu()
    {
        mainMenu.SetActive(true);
        selectSongMenu.SetActive(false);
        selectPianoMenu.SetActive(false);
    }

    private void CreateSongButtons()
    {
        TextAsset[] songs = Resources.LoadAll<TextAsset>("Songs/");
        
        foreach (TextAsset song in songs)
        {
            SongData songData = JsonUtility.FromJson<SongData>(song.text);

            GameObject obj = Instantiate(songNameButtonPrefab, selectSongScrollViewContent);

            SongSelectionButton songButton = obj.GetComponent<SongSelectionButton>();
            songButton.Setup(songData);
        }
    }

    void Awake()
    {
        if (!_loadedSongs)
        {
            CreateSongButtons();
            _loadedSongs = true;
        }
        ShowMainMenu();
    }

    public void OnClickPractice()
    {
        SceneManager.LoadScene(PIANO_SCENE_NAME);
    }

    public void OnClickSelectSongs()
    {
        mainMenu.SetActive(false);
        selectSongMenu.SetActive(true);
    }

    public void OnClickSelectPiano()
    {
        mainMenu.SetActive(false);
        selectPianoMenu.SetActive(true);
    }

    public void OnClickSelectFullPiano()
    {
        PianoManager.CurrentPiano = "Full";
        selectPianoMenu.SetActive(false);
        mainMenu.SetActive(true);
    }

    public void OnClickSelecctReducedPiano()
    {
        PianoManager.CurrentPiano = "Reduced";
        selectPianoMenu.SetActive(false);
        mainMenu.SetActive(true);
    }

    public void OnClickQuit()
    {
        #if UNITY_EDITOR
            EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    public void OnClickBackToMainMenu()
    {
        ShowMainMenu();
    }
}
