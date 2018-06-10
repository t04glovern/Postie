using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoadManager : MonoBehaviour {

    public void ChangeScene()
    {
        SceneManager.LoadScene("MainGame");
    }

    public void Death()
    {
        SceneManager.LoadScene("Menu");
    }
}
