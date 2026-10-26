using UnityEngine;

public class SongSelectionMenu : MonoBehaviour
{
    [SerializeField] private Transform selectSongScrollViewContent;
    [SerializeField] private GameObject songNameButtonPrefab;
    private bool _loadedSongs = false;

    private void Awake()
    {
        if (!_loadedSongs)
        {
            CreateSongButtons();
            _loadedSongs = true;
        }
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
}
