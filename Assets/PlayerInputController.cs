using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerInputController : MonoBehaviour
{
    private const string MENUS_SCENE_NAME = "Menus Scene";
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SceneManager.LoadScene(MENUS_SCENE_NAME);
        }
    }
}
