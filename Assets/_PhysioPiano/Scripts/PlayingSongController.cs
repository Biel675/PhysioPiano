using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayingSongController
{
    private readonly SongData _data;
    private float _timer = 0f;
    private readonly float _tutorialCountdownTime;
    private float _tutorialCounter;

    private int _currentTick;
    private readonly float _secondsPerTick;

    private PianoPlayingMode _playingMode = PianoPlayingMode.NONE;

    private readonly HashSet<string> _currentTickPressedKeys = new();
    private readonly HashSet<string> _previouslyPressedKeys = new();
    private readonly HashSet<string> _currentTickReleasedKeys = new();
    private readonly HashSet<string> _previouslyReleasedKeys = new();

    private readonly PianoManager _pianoManager;
    private readonly Dictionary<string, PianoKey> _keysMap;

    public PlayingSongController(SongData data, float tutorialCountdownTime, PianoManager pianoManager, Dictionary<string, PianoKey> keysMap)
    {
        _data = data;
        _pianoManager = pianoManager;
        _keysMap = keysMap;
        _tutorialCountdownTime = tutorialCountdownTime;

        _secondsPerTick = 60f / (_data.bpm * _data.ppqn);

        SetupSong(PianoPlayingMode.TUTORIAL);
    }

    public void Run(float deltaTime)
    {
        if (_playingMode == PianoPlayingMode.NONE) return;

        if (!IsCountdownComplete())
        {
            UpdateCountdown(deltaTime);
            return;
        }

        UpdateSongPlayback(deltaTime);
    }

    private bool IsCountdownComplete()
    {
        return _tutorialCounter <= 0;
    }

    private void UpdateCountdown(float deltaTime)
    {
        _tutorialCounter -= deltaTime;

        if (_tutorialCounter > 0)
        {
            string newSecondsRemaining = Math.Ceiling(_tutorialCounter).ToString();
            _pianoManager.TutorialCountdownText.text = _pianoManager.TutorialCountdownMessage.Replace("%seconds_remaining%", newSecondsRemaining);
        } else {
            _pianoManager.TutorialCountdownText.gameObject.SetActive(false);
        }
    }

    private void UpdateSongPlayback(float deltaTime)
    {
        _timer += deltaTime;

        while (_currentTick < _data.ticks.Count)
        {
            TickData tick = _data.ticks[_currentTick];
            float targetTime = _secondsPerTick * tick.deltaTime;

            if (_timer < targetTime) break;

            ExecuteTickCommands(tick);

            if (_currentTick == _data.ticks.Count - 1)
            {
                if (_playingMode == PianoPlayingMode.GUIDED)
                {
                    _playingMode = PianoPlayingMode.NONE;
                    return;
                }

                SetupSong(PianoPlayingMode.GUIDED);
                return;
            }

            _timer -= targetTime;
            _currentTick++;
        }
    }

    private void ExecuteTickCommands(TickData tick)
    {
        _previouslyPressedKeys.UnionWith(_currentTickPressedKeys);
        _currentTickPressedKeys.Clear();
        _previouslyReleasedKeys.UnionWith(_currentTickReleasedKeys);
        _currentTickReleasedKeys.Clear();

        foreach (string cmd in tick.cmd)
        {
            string[] parts = cmd.Split(':');
            string actionStr = parts[0].ToUpper();
            string keyName = parts[1];

            if (!_keysMap.TryGetValue(keyName, out PianoKey key))
            {
                Debug.LogError($"Tecla inexistente no piano: {keyName}");
                continue;
            }

            if (!Enum.TryParse(actionStr, out TickCommandAction action))
            {
                Debug.LogError($"Acao nao reconhecida: {actionStr}");
                continue;
            }
            KeyState newKeyState;
            switch (action)
            {
                case TickCommandAction.ON:
                    newKeyState = (_playingMode == PianoPlayingMode.TUTORIAL) ? KeyState.TUTORIAL : KeyState.GUIDED;
                    _currentTickPressedKeys.Add(keyName);
                    break;
                case TickCommandAction.ON_LEFT:
                    newKeyState = (_playingMode == PianoPlayingMode.TUTORIAL) ? KeyState.TUTORIAL_LEFT : KeyState.GUIDED_LEFT;
                    _currentTickPressedKeys.Add(keyName);
                    break;
                case TickCommandAction.OFF:
                    newKeyState = KeyState.NONE;
                    _previouslyPressedKeys.Remove(keyName);
                    _currentTickReleasedKeys.Add(keyName);
                    break;
                default:
                    newKeyState = KeyState.NONE;
                    Debug.LogError($"Acao nao reconhecida: {actionStr}");
                    break;
            }
            key.ChangeKeyState(newKeyState);
        }
    }

    public void KeyCommand(string key, TickCommandAction command)
    {
        if (command == TickCommandAction.ON)
        {
            if (_currentTickPressedKeys.Remove(key))
            {
                Debug.Log($"ACERTO: {key} ON");
            }
            else if (_previouslyPressedKeys.Remove(key))
            {
                Debug.Log($"MEIO ACERTO: {key} ON; COM ATRASO");
            }
            else
            {
                Debug.Log($"ERRO: {key} ON");
            }
        }
        else if (command == TickCommandAction.OFF)
        {
            if (_currentTickReleasedKeys.Remove(key))
            {
                Debug.Log($"ACERTO: {key} OFF");
            }
            else if (_previouslyReleasedKeys.Remove(key))
            {
                Debug.Log($"MEIO ACERTO: {key} OFF; COM ATRASO");
            }
            else
            {
                Debug.Log($"ERRO: {key} OFF");
            }
        }
    }

    public bool ExpectsInput()
    {
        return _playingMode == PianoPlayingMode.GUIDED && IsCountdownComplete();
    }

    private void SetupSong(PianoPlayingMode playingMode)
    {
        _timer = 0f;
        _tutorialCounter = _tutorialCountdownTime;
        _currentTick = 0;
        _playingMode = playingMode;

        _pianoManager.TutorialCountdownText.gameObject.SetActive(true);

        _currentTickPressedKeys.Clear();
        _previouslyPressedKeys.Clear();
        _currentTickReleasedKeys.Clear();
        _previouslyReleasedKeys.Clear();

        Debug.Log($"Iniciando musica no modo {playingMode}");
    }
}

public enum PianoPlayingMode
{
    NONE,
    TUTORIAL,
    GUIDED
}

public enum TickCommandAction
{
    ON,
    ON_LEFT,
    OFF
}