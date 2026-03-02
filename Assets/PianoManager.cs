using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class PianoManager : MonoBehaviour
{
    private Dictionary<string, PianoKey> keysMap = new Dictionary<string, PianoKey>();
    private float secondsPerTick;
    private float timer;
    private Queue<TickData> ticks = null;

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
        timer += Time.deltaTime;

        if (ticks != null && ticks.Count > 0 && timer >= secondsPerTick)
        {
            TickData tick = ticks.Dequeue();
            foreach (string command in tick.cmd)
            {
                string[] parts = command.Split(":");
                string action = parts[0];
                string keyName = parts[1];

                if (!keysMap.ContainsKey(keyName))
                {
                    Debug.LogError($"Tecla nao identificada: {keyName}");
                    return;
                }
                PianoKey key = keysMap[keyName];

                if (action == "on")
                {
                    key.State = "tutorial";
                }
                else if (action == "on_left")
                {
                    key.State = "tutorial_left";
                }
                else if (action == "off")
                {
                    key.StopKey();
                }
                else
                {
                    Debug.LogError($"Comando nao especificado: {action}");
                }
            }

            timer -= secondsPerTick;
        }
        else if (ticks != null && ticks.Count == 0)
        {
            ticks = null;
        }
        else if (Input.GetKey(KeyCode.K))
        {
            string jsonString = File.ReadAllText("Assets/chopsticks_two_hands.json");
            SongData songData = JsonUtility.FromJson<SongData>(jsonString);

            float beatsPerMinute = songData.bpm;
            secondsPerTick = 60 / beatsPerMinute;
            if (songData.ticks != null)
            {
                ticks = new Queue<TickData>(songData.ticks);
            }
            timer = 0f;

            Debug.Log($"Carregando musica com {beatsPerMinute} bpm e {ticks.Count} ticks");
        }
    }
}