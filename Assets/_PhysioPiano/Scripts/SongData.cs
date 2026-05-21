using System;
using System.Collections.Generic;

[Serializable]
public class SongData
{
    public string name;
    public float bpm;
    public int ppqn;
    public List<TickData> ticks;
}

[Serializable]
public class TickData
{
    public int deltaTime;
    public List<string> cmd;
}
