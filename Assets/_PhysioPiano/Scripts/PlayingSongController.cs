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

    private readonly HashSet<FallingNoteData> _fallingNotes = new();
    private readonly GameObject _fallingNotePrefab;

    private PianoPlayingMode _playingMode = PianoPlayingMode.NONE;

    private readonly HashSet<string> _currentTickPressedKeys = new();
    private readonly HashSet<string> _previouslyPressedKeys = new();
    private readonly HashSet<string> _currentTickReleasedKeys = new();
    private readonly HashSet<string> _previouslyReleasedKeys = new();

    private readonly PianoManager _pianoManager;
    private readonly Dictionary<string, PianoKey> _keysMap;

    public PlayingSongController(SongData data, float tutorialCountdownTime, PianoManager pianoManager, Dictionary<string, PianoKey> keysMap, GameObject fallingNotePrefab, ParticleSystem correctPressParticle, ParticleSystem latePressParticle, ParticleSystem missedPressParticle)
    {
        _data = data;
        _pianoManager = pianoManager;
        _keysMap = keysMap;
        _tutorialCountdownTime = tutorialCountdownTime;
        _fallingNotePrefab = fallingNotePrefab;
        
        _correctPressParticle = correctPressParticle;
        _latePressParticle = latePressParticle;
        _missedPressParticle = missedPressParticle;

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

    public bool IsCountdownComplete()
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

    private void ProcessFallingNotes()
    {
        Dictionary<string, FallingNoteData> pressedKeysTicks = new();
        int accumulatedDeltaTime = 0;

        for (int i = 0; i < _data.ticks.Count; i++)
        {
            TickData tick = _data.ticks[i];
            accumulatedDeltaTime += tick.deltaTime;

            foreach (string cmd in tick.cmd)
            {
                if (!TryIdentifyActionAndKey(cmd, out string actionStr, out TickCommandAction action, out string keyName, out PianoKey key)) continue;

                FallingNoteData fallingNoteData;
                switch (action)
                {
                    case TickCommandAction.ON:
                        fallingNoteData = new(keyName, i, accumulatedDeltaTime);
                        pressedKeysTicks.Add(keyName, fallingNoteData);
                        break;
                    case TickCommandAction.ON_LEFT:
                        fallingNoteData = new(keyName, i, accumulatedDeltaTime);
                        pressedKeysTicks.Add(keyName, fallingNoteData);
                        break;
                    case TickCommandAction.OFF:
                        if (!pressedKeysTicks.Remove(keyName, out fallingNoteData))
                        {
                            Debug.LogError($"Comando OFF em tecla desligada: {keyName}");
                            continue;
                        }

                        fallingNoteData.EndingTick = i;
                        fallingNoteData.AccumulatedDeltaTimeEnd = accumulatedDeltaTime;
                        _fallingNotes.Add(fallingNoteData);
                        break;
                    default:
                        Debug.LogError($"Acao nao reconhecida: {actionStr}");
                        break;
                }
            }
        }

        foreach (FallingNoteData fallingNoteData in _fallingNotes)
        {
            SpawnFallingNote(fallingNoteData);
        }
    }

    private void SpawnFallingNote(FallingNoteData fallingNoteData)
    {
        GameObject fallingNoteObj = UnityEngine.Object.Instantiate(_fallingNotePrefab);
        FallingNote fallingNote = fallingNoteObj.GetComponent<FallingNote>();

        fallingNote.Init(this, _keysMap[fallingNoteData.Key], fallingNoteData.AccumulatedDeltaTimeStart, fallingNoteData.AccumulatedDeltaTimeEnd, _data.ppqn, _data.bpm);
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
            if (!TryIdentifyActionAndKey(cmd, out string actionStr, out TickCommandAction action, out string keyName, out PianoKey key)) continue;

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

    private bool TryIdentifyActionAndKey(string cmd, out string actionStr, out TickCommandAction action, out string keyName, out PianoKey key)
    {
        actionStr = null;
        action = default;
        keyName = null;
        key = null;

        string[] parts = cmd.Split(':');

        if (parts.Length != 2)
        {
            Debug.LogError($"Commando invalido: {cmd}");
            return false;
        }

        actionStr = parts[0].ToUpper();
        keyName = parts[1];

        if (!_keysMap.TryGetValue(keyName, out key))
        {
            Debug.LogError($"Tecla inexistente no piano: {keyName}");
            return false;
        }

        if (!Enum.TryParse(actionStr, out action))
        {
            Debug.LogError($"Acao nao reconhecida: {actionStr}");
            return false;
        }

        return true;
    }

    [SerializeField] private ParticleSystem _correctPressParticle;
    [SerializeField] private ParticleSystem _latePressParticle;
    [SerializeField] private ParticleSystem _missedPressParticle;

    public void KeyCommand(string key, Vector3 pos, TickCommandAction command)
    {
        ParticleSystem particle = null;
        if (command == TickCommandAction.ON)
        {
            if (_currentTickPressedKeys.Remove(key))
            {
                particle = _correctPressParticle;
                Debug.Log($"ACERTO: {key} ON");
            }
            else if (_previouslyPressedKeys.Remove(key))
            {
                particle = _latePressParticle;
                Debug.Log($"MEIO ACERTO: {key} ON; COM ATRASO");
            }
            else
            {
                particle = _missedPressParticle;
                Debug.Log($"ERRO: {key} ON");
            }
        }
        else if (command == TickCommandAction.OFF)
        {
            if (_currentTickReleasedKeys.Remove(key))
            {
                particle = _correctPressParticle;
                Debug.Log($"ACERTO: {key} OFF");
            }
            else if (_previouslyReleasedKeys.Remove(key))
            {
                particle = _latePressParticle;
                Debug.Log($"MEIO ACERTO: {key} OFF; COM ATRASO");
            }
            else
            {
                particle = _missedPressParticle;
                Debug.Log($"ERRO: {key} OFF");
            }
        }

        if (particle == null) return;
        particle.transform.position = new Vector3(pos.x, pos.y + 0.02f, pos.z);
        particle.Emit(3);
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

        _fallingNotes.Clear();
        ProcessFallingNotes();

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