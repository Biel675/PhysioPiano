using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEditor.Rendering;
using System.Xml;

public class PianoManager : MonoBehaviour
{
    public const string MENUS_SCENE_NAME = "Menus Scene";
    private Dictionary<string, PianoKey> keysMap = new Dictionary<string, PianoKey>();
    public static string Mode { get; set; }
    private static SongData CurrentSong { get; set; }
    private static float Timer { get; set; }

    private const string CMD_ON = "on"; // right hand is default
    private const string CMD_ON_LEFT = "on_left";
    private const string CMD_OFF = "off";

    [SerializeField] private GameObject fullPiano;
    [SerializeField] private GameObject reducedPiano;
    public static string CurrentPiano { get; set; } = "Full";

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
            keysMap.Add(keyNote, key);
        }

        Debug.Log("Piano carregado com " + keysMap.Count + " teclas");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SceneManager.LoadScene(MENUS_SCENE_NAME);
        }

        if (Mode != "tutorial" && Mode != "guided")
        {
            return;
        }

        if (CurrentSong == null)
        {
            Mode = "";
            return;
        }

        Timer += Time.deltaTime;

        TickData tick = CurrentSong.ticks[CurrentSong.CurrentTick];
        
        if (Timer < (CurrentSong.SecondsPerTick * tick.deltaTime))
        {
            return;
        }        
        
        foreach (string command in tick.cmd)
        {
            string[] parts = command.Split(":");
            string action = parts[0];
            string keyName = parts[1];

            if (!keysMap.TryGetValue(keyName, out PianoKey key))
            {
                Debug.LogError($"Tecla nao identificada: {keyName}");
                    return;
            }

            switch (action)
            {
                case CMD_ON:
                    key.State = Mode;
                    break;
                case CMD_ON_LEFT:
                    key.State = Mode + "_left";
                    break;
                case CMD_OFF:
                    key.StopKey();
                    break;
                default:
                    Debug.LogError($"Comando nao especificado: {action}");
                    break;
            }
        }

        if (CurrentSong.CurrentTick == (CurrentSong.ticks.Count - 1))
        {
            if (Mode == "guided")
            {
                Mode = "";
                return;
            }
            
            CurrentSong.CurrentTick = 0;
            Timer = 0f;
            Mode = "guided";
            Debug.Log("Trocando modo tutorial para guided");
            return;
        }

        CurrentSong.CurrentTick++;
        Timer -= CurrentSong.SecondsPerTick * tick.deltaTime;
    }

    public static void LoadSong(SongData song)
    {
        song.SecondsPerTick = 60 / (song.bpm * song.ppqn);
        Timer = 0f;
        CurrentSong = song;
        Mode = "tutorial";

        Debug.Log($"Carregando musica {song.name}");
    }
}