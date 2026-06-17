using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PianoManager : MonoBehaviour
{
    private readonly Dictionary<string, PianoKey> _keysMap = new();

    public string TutorialCountdownMessage { get; private set; } = "%seconds_remaining%\nSe prepare!";
    [field: SerializeField] public TextMeshPro TutorialCountdownText { get; set; }

    public PlayingSongController SongController { get; private set; } = null;
    public static SongData SelectedSongData { get; set; } = null;

    [SerializeField] private GameObject _fallingNotePrefab;
    [SerializeField] private ParticleSystem _correctPressParticle;
    [SerializeField] private ParticleSystem _latePressParticle;
    [SerializeField] private ParticleSystem _missedPressParticle;

    [SerializeField] private GameObject fullPiano;
    [SerializeField] private GameObject reducedPiano;
    public static string CurrentPiano { get; set; } = "Full";

    [SerializeField] private Color _pressedKeyColor = Color.blue;
    [SerializeField] private Color _tutorialKeyColor = Color.red;
    [SerializeField] private Color _tutorialLeftKeyColor = Color.cyan;

    void Awake()
    {
        if (CurrentPiano == "Full")
        {
            fullPiano.SetActive(true);
            reducedPiano.SetActive(false);
        }
        else
        {
            fullPiano.SetActive(false);
            reducedPiano.SetActive(true);
        }
        
        PianoKey[] keys = GetComponentsInChildren<PianoKey>();

        foreach (PianoKey key in keys)
        {
            string keyNote = key.name.Split("_")[1];
            _keysMap.Add(keyNote, key);
            key.Init(this, _pressedKeyColor, _tutorialKeyColor, _tutorialLeftKeyColor);
        }

        Debug.Log("Piano carregado com " + _keysMap.Count + " teclas");
    }

    void Start()
    {
        if (SelectedSongData != null)
        {
            LoadSong(SelectedSongData);
            SelectedSongData = null;
        }
    }

    void Update()
    {
        if (SongController != null)
        {
            SongController.Run(Time.deltaTime);
        }
    }

    public void LoadSong(SongData data)
    {
        SongController = new(data, Config.TUTORIAL_COOLDOWN_SECS, this, _keysMap, _fallingNotePrefab, _correctPressParticle, _latePressParticle, _missedPressParticle);
    }
}