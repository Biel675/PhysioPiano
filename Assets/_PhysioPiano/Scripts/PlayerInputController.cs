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
    }
}
