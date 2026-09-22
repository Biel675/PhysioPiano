using UnityEngine;

public class FallingNote : MonoBehaviour
{
    private Config _config;
    [SerializeField] private float _noteBaseOffset = 0f;
    
    private float _endingY;
    private float _endingZ;
    private float _speed;

    private float _length;
    private Vector3 _direction;

    public void Init(Config config, PianoKey key, int accumulatedDeltaTimeStart, int accumulatedDeltaTimeEnd, int ppqn, float bpm, Color color)
    {
        _config = config;
        
        float width = key.transform.localScale.x;
        float depth = transform.localScale.z;
        
        float quarterNotes = (float) (accumulatedDeltaTimeEnd - accumulatedDeltaTimeStart) / ppqn;
        _length = width * quarterNotes;

        float timeOffset = (float) accumulatedDeltaTimeStart / ppqn * width;
        float startingY, startingZ;
        if (_config != null && _config.FallingNotesUseFrontalPlane)
        {
            transform.localScale = new Vector3(width, _length, depth);
            _direction = Vector3.down;
            
            _endingY = key.transform.position.y + (key.transform.localScale.y / 2f) + _noteBaseOffset;
            startingY = _endingY + timeOffset + (_length / 2f);
            startingZ = key.transform.position.z - key.transform.localScale.z / 2f;
        }
        else
        {
            transform.localScale = new Vector3(width, depth, _length);
            _direction = Vector3.forward;
            
            _endingZ = key.transform.position.z - (key.transform.localScale.z / 2f) - _noteBaseOffset;
            startingY = key.transform.position.y - key.transform.localScale.y / 2f;
            startingZ = _endingZ - timeOffset - (_length / 2f);
        }
        
        _speed = width * bpm / 60f;

        float posX = key.transform.position.x;
        transform.position = new Vector3(posX, startingY, startingZ);

        Renderer coloredNoteRenderer = transform.Find("Cube").GetComponent<Renderer>();
        coloredNoteRenderer.material.color = color;
    }

    public bool UpdatePosition()
    {
        transform.position += _speed * Time.deltaTime * _direction;

        float topPosY = transform.position.y + _length / 2f;
        float topPosZ = transform.position.z - _length / 2f;
        if ((_config.FallingNotesUseFrontalPlane && topPosY <= _endingY) || (!_config.FallingNotesUseFrontalPlane && topPosZ >= _endingZ))
        {
            Destroy(gameObject);
            return false;
        }

        return true;
    }
}
