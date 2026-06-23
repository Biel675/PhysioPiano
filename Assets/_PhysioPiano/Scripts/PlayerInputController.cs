using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.XR.CoreUtils;

public class PlayerInputController : MonoBehaviour
{
    public static Vector3 PlayerPosition { get; set; } = new(0, 1.5f, -1f);
    [SerializeField] private GameObject _xrOriginObj;
    private XROrigin xrOrigin;
    [SerializeField] private GameObject _mainCamera;
    private const string MENUS_SCENE_NAME = "Menus Scene";

    private readonly float _moveSpeed = 0.5f;

    void Start()
    {
        xrOrigin = _xrOriginObj.GetComponent<XROrigin>();
        xrOrigin.MoveCameraToWorldLocation(PlayerPosition);
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            PlayerPosition = _mainCamera.transform.position;
            SceneManager.LoadScene(MENUS_SCENE_NAME);
        }

        float horizontal = Input.GetAxis("Horizontal") * _moveSpeed * Time.deltaTime;
        float vertical = Input.GetAxis("Vertical") * _moveSpeed * Time.deltaTime;
        float upDown = 0f;
        if (Input.GetKey(KeyCode.E))
        {
            upDown = _moveSpeed * Time.deltaTime;
        }
        else if (Input.GetKey(KeyCode.Q))
        {
            upDown = -_moveSpeed * Time.deltaTime;
        }

        _mainCamera.transform.Translate(horizontal, upDown, vertical);
    }
}
