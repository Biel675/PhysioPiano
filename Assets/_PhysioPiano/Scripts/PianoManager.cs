using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PianoManager : MonoBehaviour
{
    private Config _config = Config.INSTANCE;
    
    private static readonly string[] PIANO_KEYS = {
        "WhiteKey_A0", "BlackKey_As0", "WhiteKey_B0",
        "WhiteKey_C1", "BlackKey_Cs1", "WhiteKey_D1", "BlackKey_Ds1", "WhiteKey_E1", "WhiteKey_F1", "BlackKey_Fs1", "WhiteKey_G1", "BlackKey_Gs1", "WhiteKey_A1", "BlackKey_As1", "WhiteKey_B1",
        "WhiteKey_C2", "BlackKey_Cs2", "WhiteKey_D2", "BlackKey_Ds2", "WhiteKey_E2", "WhiteKey_F2", "BlackKey_Fs2", "WhiteKey_G2", "BlackKey_Gs2", "WhiteKey_A2", "BlackKey_As2", "WhiteKey_B2",
        "WhiteKey_C3", "BlackKey_Cs3", "WhiteKey_D3", "BlackKey_Ds3", "WhiteKey_E3", "WhiteKey_F3", "BlackKey_Fs3", "WhiteKey_G3", "BlackKey_Gs3", "WhiteKey_A3", "BlackKey_As3", "WhiteKey_B3",
        "WhiteKey_C4", "BlackKey_Cs4", "WhiteKey_D4", "BlackKey_Ds4", "WhiteKey_E4", "WhiteKey_F4", "BlackKey_Fs4", "WhiteKey_G4", "BlackKey_Gs4", "WhiteKey_A4", "BlackKey_As4", "WhiteKey_B4",
        "WhiteKey_C5", "BlackKey_Cs5", "WhiteKey_D5", "BlackKey_Ds5", "WhiteKey_E5", "WhiteKey_F5", "BlackKey_Fs5", "WhiteKey_G5", "BlackKey_Gs5", "WhiteKey_A5", "BlackKey_As5", "WhiteKey_B5",
        "WhiteKey_C6", "BlackKey_Cs6", "WhiteKey_D6", "BlackKey_Ds6", "WhiteKey_E6", "WhiteKey_F6", "BlackKey_Fs6", "WhiteKey_G6", "BlackKey_Gs6", "WhiteKey_A6", "BlackKey_As6", "WhiteKey_B6",
        "WhiteKey_C7", "BlackKey_Cs7", "WhiteKey_D7", "BlackKey_Ds7", "WhiteKey_E7", "WhiteKey_F7", "BlackKey_Fs7", "WhiteKey_G7", "BlackKey_Gs7", "WhiteKey_A7", "BlackKey_As7", "WhiteKey_B7", "WhiteKey_C8"};

    private string _firstKey, _lastKey;
    
    private readonly Dictionary<string, PianoKey> _keysMap = new();

    public string TutorialCountdownMessage { get; private set; } = "Prepare-se!\n%seconds_remaining%";
    [field: SerializeField] public TextMeshPro TutorialCountdownText { get; set; }

    public PlayingSongController SongController { get; private set; } = null;
    public static SongData SelectedSongData { get; set; } = null;

    [SerializeField] private GameObject _fallingNotePrefab;
    [SerializeField] private ParticleSystem _correctPressParticle;
    [SerializeField] private ParticleSystem _latePressParticle;
    [SerializeField] private ParticleSystem _missedPressParticle;

    [SerializeField] private GameObject piano;

    [SerializeField] private Color _pressedKeyColor = Color.blue;
    [SerializeField] private Color _tutorialKeyColor = Color.red;
    [SerializeField] private Color _tutorialLeftKeyColor = Color.cyan;

    void Awake()
    {
        PreloadPiano(_config.PianoConfiguration);
        
        PianoKey[] keys = GetComponentsInChildren<PianoKey>();

        foreach (PianoKey key in keys)
        {
            string keyNote = key.name.Split("_")[1];
            _keysMap.Add(keyNote, key);
            key.Init(_config, this, _pressedKeyColor, _tutorialKeyColor, _tutorialLeftKeyColor);
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
        SongController = new(_config, data, this, _keysMap, _fallingNotePrefab, _correctPressParticle, _latePressParticle, _missedPressParticle);
    }

    private void PreloadPiano(string pianoOption)
    {
        switch (pianoOption)
        {
            case "reduced":
                _firstKey = "WhiteKey_C3";
                _lastKey = "WhiteKey_C5";
                break;
            case "keyboard":
                _firstKey = "WhiteKey_C1";
                _lastKey = "WhiteKey_C6";
                break;
            default:
                _firstKey = "WhiteKey_A0";
                _lastKey = "WhiteKey_C8";
                break;
        }
        LoadSelectedPiano(_firstKey, _lastKey);
    }

    private void LoadSelectedPiano(string firstKey, string lastKey)
    {
        var disableKeys = true;
        foreach (var currKeyName in PIANO_KEYS)
        {
            if (currKeyName == firstKey)
            {
                disableKeys = false;
            }

            if (disableKeys)
            {
                var pathPrefix = currKeyName.StartsWith("WhiteKey") ? "White Keys" : "Black Keys";
                var keyTransform = piano.transform.Find($"{pathPrefix}/{currKeyName}");
                keyTransform.gameObject.SetActive(false);
            }
            
            if (currKeyName == lastKey)
            {
                disableKeys = true;
            }
        }
    }
}