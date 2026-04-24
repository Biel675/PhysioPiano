using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;

[Serializable]
public class SongData
{
    public string name;
    public float bpm;
    public int ppqn;
    public List<TickData> ticks;
    public int CurrentTick { get; set; } = 0;
    public float SecondsPerTick { get; set; }
}

[Serializable]
public class TickData
{
    public int deltaTime;
    public List<string> cmd;
}
