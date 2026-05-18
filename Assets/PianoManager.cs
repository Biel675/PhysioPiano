using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using TMPro;

public class PianoManager : MonoBehaviour
{
    private Dictionary<string, PianoKey> keysMap = new Dictionary<string, PianoKey>();
    public static string Mode { get; set; }
    private static SongData CurrentSong { get; set; }
    private static float Timer { get; set; }
    private static float TutorialCountdown { get; set; } = 10f;
    [SerializeField] private TextMeshPro tutorialCountdownText;

    private static readonly HashSet<string> CurrentTickPressedKeys = new HashSet<string>();
    private static readonly HashSet<string> HeldKeys = new HashSet<string>();
    private static readonly HashSet<string> CurrentTickReleasedKeys = new HashSet<string>();
    private static readonly HashSet<string> PreviouslyReleasedKeys = new HashSet<string>();

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
        if (Mode != "tutorial" && Mode != "guided")
        {
            return;
        }

        if (CurrentSong == null)
        {
            Mode = "";
            CurrentTickPressedKeys.Clear();
            HeldKeys.Clear();
            CurrentTickReleasedKeys.Clear();
            return;
        }

        if (TutorialCountdown >= 0)
        {
            TutorialCountdown -= Time.deltaTime;
            string secondsRemaining = Math.Ceiling(TutorialCountdown).ToString();
            tutorialCountdownText.text = secondsRemaining + "\nSe prepare!";
            return;
        }
        else if (tutorialCountdownText.gameObject.activeInHierarchy)
        {
            tutorialCountdownText.gameObject.SetActive(false);
        }

        Timer += Time.deltaTime;

        TickData tick = CurrentSong.ticks[CurrentSong.CurrentTick];
        
        if (Timer < (CurrentSong.SecondsPerTick * tick.deltaTime))
        {
            return;
        }

        PreviouslyReleasedKeys.UnionWith(CurrentTickReleasedKeys);
        CurrentTickReleasedKeys.Clear();

        HeldKeys.UnionWith(CurrentTickPressedKeys);
        CurrentTickPressedKeys.Clear();
        
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
                    CurrentTickPressedKeys.Add(keyName);
                    break;
                case CMD_ON_LEFT:
                    key.State = Mode + "_left";
                    CurrentTickPressedKeys.Add(keyName);
                    break;
                case CMD_OFF:
                    key.StopKey();
                    HeldKeys.Remove(keyName);
                    CurrentTickReleasedKeys.Add(keyName);
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
            TutorialCountdown = 10f;
            tutorialCountdownText.gameObject.SetActive(true);

            Mode = "guided";
            Debug.Log("Trocando modo tutorial para guided");
            return;
        }

        CurrentSong.CurrentTick++;
        Timer -= CurrentSong.SecondsPerTick * tick.deltaTime;
    }

    
    public static void KeyCommand(string key, string command)
    {
        if (command == "on")
        {
            if (CurrentTickPressedKeys.Contains(key))
            {
                Debug.Log($"ACERTO: {key} ON");
                CurrentTickPressedKeys.Remove(key);
            }
            else if (HeldKeys.Contains(key))
            {
                Debug.Log($"MEIO ACERTO: {key} ON; COM ATRASO");
                HeldKeys.Remove(key);
            }
            else
            {
                Debug.Log($"ERRO: {key} ON");
            }
        }
        else if (command == "off")
        {
            if (CurrentTickReleasedKeys.Contains(key))
            {
                Debug.Log($"ACERTO: {key} OFF");
                CurrentTickReleasedKeys.Remove(key);
            }
            else if (PreviouslyReleasedKeys.Contains(key))
            {
                Debug.Log($"MEIO ACERTO: {key} OFF; COM ATRASO");
                PreviouslyReleasedKeys.Remove(key);
            }
            else
            {
                Debug.Log($"ERRO: {key} OFF");
            }
        }
    }

    public static bool ExpectsInput()
    {
        return Mode == "guided" && TutorialCountdown < 0;
    }

    public static void LoadSong(SongData song)
    {
        song.SecondsPerTick = 60 / (song.bpm * song.ppqn);
        Timer = 0f;
        TutorialCountdown = 10f;
        CurrentSong = song;
        Mode = "tutorial";

        Debug.Log($"Carregando musica {song.name}");
    }
}