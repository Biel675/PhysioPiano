using System;
using UnityEngine;

[Serializable]
public class Config
{
    public static readonly Config INSTANCE = new();
    private Config() {}
    
    [field: SerializeField]
    public int TutorialCooldownSeconds { get; set; } = 5;
    
    [field: SerializeField]
    public int SongFirstTickDelaySeconds { get; set; } = 3;
    
    [field: SerializeField]
    public bool PauseSongWhenWaitingForInput { get; set; } = false;
    
    [field: SerializeField]
    public bool UseLightsKeyboard { get; set; } = false;

    [field: SerializeField]
    public bool FallingNotesUseFrontalPlane { get; set; } = false;

    [field: SerializeField]
    public string PianoConfiguration { get; set; } = "full";
}
