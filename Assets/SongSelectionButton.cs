using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SongSelectionButton : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textObj;

    public SongData SongData { get; set; }

    void Awake()
    {
        Debug.Log("adding listener");
        GetComponent<Button>().onClick.AddListener(OnClick);
    }

    public void Setup(SongData song)
    {
        SongData = song;
        Debug.Log($"Setting button text to {song.name}", this);
        textObj.SetText(song.name);
    }

    public void OnClick()
    {
        PianoManager.LoadSong(SongData);
        SceneManager.LoadScene(MainMenu.PIANO_SCENE_NAME);
    }
}
