using UnityEngine;
using UnityEngine.SceneManagement;

public class StartUp : MonoBehaviour
{
    void Start()
    {
        SceneManager.LoadSceneAsync("Home", LoadSceneMode.Single);
    }
}
