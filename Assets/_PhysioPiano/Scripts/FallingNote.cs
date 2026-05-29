using UnityEngine;

public class FallingNote : MonoBehaviour
{
    private float _endingY;
    private float _speed;
    private PlayingSongController _songController;
    [SerializeField] private float _noteBaseOffset = 0.025f;

    public void Init(PlayingSongController songController, PianoKey key, int accumulatedDeltaTimeStart, int accumulatedDeltaTimeEnd, int ppqn, float bpm)
    {
        _songController = songController;

        float length = key.transform.localScale.x;
        float quarterNotes = (float) (accumulatedDeltaTimeEnd - accumulatedDeltaTimeStart) / ppqn;
        float height = length * quarterNotes;
        transform.localScale = new Vector3(length, height, 0);

        _endingY = key.transform.position.y + _noteBaseOffset;
        float timeOffset = (float) accumulatedDeltaTimeStart / ppqn * length;
        float startingY = _endingY + timeOffset + (height / 2f);
        _speed = length * bpm / 60f;

        float posX = key.transform.position.x;
        float posY = startingY;
        float posZ = key.transform.position.z - key.transform.localScale.z / 2f;
        transform.position = new Vector3(posX, posY, posZ);
    }

    void Update()
    {
        if (!_songController.IsCountdownComplete()) return;

        transform.position += _speed * Time.deltaTime * Vector3.down;

        float topPosY = transform.position.y + transform.localScale.y / 2f;
        if (topPosY < _endingY)
        {
            Destroy(gameObject);
        }
    }
}
