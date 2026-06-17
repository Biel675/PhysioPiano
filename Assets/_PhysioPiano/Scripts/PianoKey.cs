using System.Collections;
using Bhaptics.SDK2;
using Bhaptics.SDK2.Glove;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
[RequireComponent(typeof(AudioSource))]
public class PianoKey : MonoBehaviour
{
    public float maxDepression = 0.02f;
    public float maxAngle = 5f;
    public float rotationSpeed = 15f;

    private KeyState _state;
    private PianoManager _pianoManager;
    private string _keyName;
    private bool _isPlayerPressing;
    
    private AudioSource _audioSource;
    private AudioClip _noteAudioClip;
    private bool _wasNotePlayed;

    private Renderer _objectRenderer;
    [SerializeField] private Color _scriabinColor;
    private Color _pressedKeyColor;
    private Color _tutorialKeyColor;
    private Color _tutorialLeftKeyColor;
    private Color _originalColor;
    private bool _isFlickering = false;
    private static readonly float FLICKER_DURATION_SECS = 0.1f;

    public void Init(PianoManager pianoManager, Color pressedKeyColor, Color tutorialKeyColor, Color tutorialLeftKeyColor)
    {
        _pianoManager = pianoManager;
        _pressedKeyColor = pressedKeyColor;
        _tutorialKeyColor = tutorialKeyColor;
        _tutorialLeftKeyColor = tutorialLeftKeyColor;
    }

    void Start()
    {
        _keyName = name.Split("_")[1];

        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null) Debug.LogError("Missing AudioSource on " + name);
        _noteAudioClip = Resources.Load<AudioClip>("PianoNotes/" + _keyName);

        _objectRenderer = GetComponent<Renderer>();
        _originalColor = _objectRenderer.material.color;
    }

    void Update()
    {
        if (!_isPlayerPressing)
        {
            Quaternion restRotation = Quaternion.identity;
            transform.localRotation = Quaternion.Slerp(
                transform.localRotation,
                restRotation,
                Time.deltaTime * rotationSpeed
            );
        }

        if (!_wasNotePlayed && (_isPlayerPressing || _state == KeyState.TUTORIAL || _state == KeyState.TUTORIAL_LEFT))
        {
            _audioSource.PlayOneShot(_noteAudioClip);
            _wasNotePlayed = true;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!TryGetFingerData(other, out FingerHapticData fingerData) || !string.IsNullOrEmpty(fingerData.pressedKey)) return;
        fingerData.pressedKey = _keyName;

        Debug.Log($"Tecla pressionada: {_keyName}");

        _isPlayerPressing = true;

        if (_pianoManager.SongController != null && _pianoManager.SongController.ExpectsInput())
        {
            _pianoManager.SongController.KeyCommand(_keyName, transform.position, TickCommandAction.ON);
        }

        UpdateKeyColor();
    }

    void OnTriggerStay(Collider other)
    {
        if (!TryGetFingerData(other, out FingerHapticData fingerData) || fingerData.pressedKey != _keyName) return;

        Vector3 localFingerPosition = transform.InverseTransformPoint(other.transform.position);

        float currentDepth = Mathf.Clamp(-localFingerPosition.z, 0f, maxDepression);

        float depressionPercent = Mathf.InverseLerp(0f, maxDepression, currentDepth);

        float targetAngle = Mathf.Lerp(0f, maxAngle, depressionPercent);

        Quaternion targetRotation = Quaternion.Euler(targetAngle, 0f, 0f);

        transform.localRotation = Quaternion.Slerp(
            transform.localRotation,
            targetRotation,
            Time.deltaTime * rotationSpeed
        );

        float relativeDistance = Mathf.Clamp01(currentDepth / maxDepression);
        PositionType posType = fingerData.isLeftHand ? PositionType.GloveL : PositionType.GloveR;
        BhapticsPhysicsGlove.Instance.SendStayHaptic(posType, fingerData.fingerIndex, relativeDistance);
    }

    void OnTriggerExit(Collider other)
    {
        if (!TryGetFingerData(other, out FingerHapticData fingerData) || fingerData.pressedKey != _keyName) return;
        fingerData.pressedKey = null;

        PositionType posType = fingerData.isLeftHand ? PositionType.GloveL : PositionType.GloveR;
        BhapticsPhysicsGlove.Instance.SendExitHaptic(posType, fingerData.fingerIndex);  

        _isPlayerPressing = false;
        _wasNotePlayed = false;

        if (_pianoManager.SongController != null && _pianoManager.SongController.ExpectsInput())
        {
            _pianoManager.SongController.KeyCommand(_keyName, transform.position, TickCommandAction.OFF);   
        }

        UpdateKeyColor();
    }

    public void ChangeKeyState(KeyState newState)
    {
        if (!_isPlayerPressing)
        {
            if (newState == KeyState.NONE && (_state == KeyState.TUTORIAL || _state == KeyState.TUTORIAL_LEFT))
            {
                _wasNotePlayed = false;
            } else if (newState != KeyState.NONE && !_isFlickering)
            {
                StartCoroutine(FlickerKeyTutorial(newState));
                return;
            }
        }
        _state = newState;
        UpdateKeyColor();
    }

    private void UpdateKeyColor()
    {
        if (_isPlayerPressing)
        {
            _objectRenderer.material.color = _pressedKeyColor;
        }
        else if (_state == KeyState.NONE)
        {
            _objectRenderer.material.color = _originalColor;
        }
        else
        {
            bool isLeftHand = _state == KeyState.TUTORIAL_LEFT || _state == KeyState.GUIDED_LEFT;
            _objectRenderer.material.color = GetTutorialColor(Config.USE_LIGHTS_KEYBOARD, isLeftHand);
        }
    }

    public Color GetTutorialColor(bool usingLightsKeyboard, bool isLeftHand)
    {
        if (usingLightsKeyboard) return _scriabinColor;
        if (isLeftHand) return _tutorialLeftKeyColor;
        return _tutorialKeyColor;
    }

    private IEnumerator FlickerKeyTutorial(KeyState targetState)
    {
        _isFlickering = true;

        _state = KeyState.NONE;
        UpdateKeyColor();

        yield return new WaitForSeconds(FLICKER_DURATION_SECS);

        _state = targetState;
        UpdateKeyColor();

        _isFlickering = false;
    }

    private bool TryGetFingerData(Collider fingerCollider, out FingerHapticData fingerData)
    {
        fingerData = fingerCollider.GetComponent<FingerHapticData>();
        if (fingerData == null)
        {
            Debug.LogError("Erro ao ler FingerHapticData");
            return false;
        }
        return true;
    }
}

public enum KeyState
{
    NONE,
    TUTORIAL,
    TUTORIAL_LEFT,
    GUIDED,
    GUIDED_LEFT
}
