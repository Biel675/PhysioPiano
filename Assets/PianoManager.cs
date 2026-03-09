using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PianoManager : MonoBehaviour
{
    public const string MENUS_SCENE_NAME = "Menus Scene";
    private Dictionary<string, PianoKey> keysMap = new Dictionary<string, PianoKey>();
    private static float SecondsPerTick { get; set; }
    private static float Timer { get; set; }
    private static Queue<TickData> Ticks { get; set; } = null;

    private const string CMD_ON = "on"; // right hand is default
    private const string CMD_ON_LEFT = "on_left";
    private const string CMD_OFF = "off";

    void Awake()
    {
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



        if (Ticks == null)
        {
            return;
        }

        Timer += Time.deltaTime;

        if (Ticks.Count == 0)
        {
            Ticks = null;
            return;
        }

        if (Timer < SecondsPerTick)
        {
            return;
        }

        TickData tick = Ticks.Dequeue();
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
                    key.State = "tutorial";
                    break;
                case CMD_ON_LEFT:
                    key.State = "tutorial_left";
                    break;
                case CMD_OFF:
                    key.StopKey();
                    break;
                default:
                    Debug.LogError($"Comando nao especificado: {action}");
                    break;
            }
        }

        Timer -= SecondsPerTick;
    }

    public static void LoadSong(SongData song)
    {
        float bpm = song.bpm;

        SecondsPerTick = 60 / bpm;
        Timer = 0f;
        Ticks = new Queue<TickData>(song.ticks);

        Debug.Log($"Carregando musica com {bpm} bpm e {song.ticks.Count} ticks");
    }
}