public class FallingNoteData
{
    public string Key { get; private set; }
    public int StartingTick { get; private set; }
    public int AccumulatedDeltaTimeStart { get; private set; }
    public int EndingTick { get; set; }
    public int AccumulatedDeltaTimeEnd { get; set; }

    public FallingNoteData(string key, int startingTick, int accumulatedDeltaTimeStart)
    {
        Key = key;
        StartingTick = startingTick;
        AccumulatedDeltaTimeStart = accumulatedDeltaTimeStart;
    }
}
