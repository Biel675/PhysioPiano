using System;
using Bhaptics.SDK2;
using Bhaptics.SDK2.Glove;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;

public class PianoKey : MonoBehaviour
{
    public float maxDepression = 0.02f;
    public float maxAngle = 5f;
    public float rotationSpeed = 15f;

    public bool isBeingPressed = false;
    public string keyNotation;

    public AudioClip noteSound;
    private AudioSource audioSource;
    private bool isNotePlayed = false;

    private Color originalColor;
    public Color pressedColor = Color.green;
    public Color guidedColor = Color.red;
    public Color guidedColorLeft = Color.cyan;
    private Renderer objectRenderer;

    private static readonly float FLICKER_DURATION_SECS = 0.1f;
    private float lastPlayedTime;

    public string State { get; set; } = "off";

    void LateUpdate()
    {
        if (State == "off")
        {
            return;
        }

        float currentTime = Time.time;

        if (!isNotePlayed && (State == "tutorial" || State == "tutorial_left" || isBeingPressed))
        {
            audioSource.PlayOneShot(noteSound);
            isNotePlayed = true;
            lastPlayedTime = currentTime;
        }

        if (isBeingPressed)
        {
            return;
        }

        if (State == "tutorial" || State == "tutorial_left" || State == "guided" || State == "guided_left")
        {
            if ((currentTime - lastPlayedTime) < FLICKER_DURATION_SECS)
            {
                objectRenderer.material.color = originalColor;
            }
            else if (State == "tutorial" || State == "guided")
            {
                objectRenderer.material.color = guidedColor;
            }
            else
            {
                objectRenderer.material.color = guidedColorLeft;
            }
        }
    }

    public void StopKey()
    {
        State = "off";
        isNotePlayed = false;
        objectRenderer.material.color = originalColor;
    }

    void Start()
    {
        keyNotation = name.Split("_")[1];
        noteSound = Resources.Load<AudioClip>("PianoNotes/" + keyNotation);

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) Debug.LogError("Missing AudioSource on " + name);

        objectRenderer = GetComponent<Renderer>();
        originalColor = objectRenderer.material.color;
    }

    void Update()
    {
        if (!isBeingPressed)
        {
            Quaternion restRotation = Quaternion.identity;
            transform.localRotation = Quaternion.Slerp(
                transform.localRotation,
                restRotation,
                Time.deltaTime * rotationSpeed
            );
        }
    }

    void OnTriggerEnter(Collider other)
    {
        FingerHapticData fingerData = other.GetComponent<FingerHapticData>();
        if (fingerData == null || fingerData.pressedKey != null) return;
        fingerData.pressedKey = keyNotation;

        isBeingPressed = true;

        objectRenderer.material.color = pressedColor;
        State = "on";

        Debug.Log($"Key pressed: {keyNotation}");
    }

    void OnTriggerStay(Collider other)
    {
        isBeingPressed = true;

        FingerHapticData fingerData = other.GetComponent<FingerHapticData>();
        if (fingerData == null || fingerData.pressedKey != keyNotation) return;

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
        FingerHapticData fingerData = other.GetComponent<FingerHapticData>();
        if (fingerData == null)
        {
            Debug.LogWarning("Erro na FingerData");
            return;
        }
        fingerData.pressedKey = null;

        isBeingPressed = false;

        PositionType posType = fingerData.isLeftHand ? PositionType.GloveL : PositionType.GloveR;
        BhapticsPhysicsGlove.Instance.SendExitHaptic(posType, fingerData.fingerIndex);  
        
        StopKey();
        objectRenderer.material.color = originalColor;
    }
}
