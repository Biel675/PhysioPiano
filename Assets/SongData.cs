using System;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public class SongData
{
    public float bpm;
    public List<TickData> ticks;
}

[Serializable]
public class TickData
{
    public List<string> cmd;
}

/*

[Serializable]
public class TickData
{
    public List<TickCommand> commands;
}

[Serializable]
public class TickCommand
{
    public string command;
    public string key;

    public TickCommand(string command, string key)
    {
        this.command = command;
        this.key = key;
    }
}*/