using UnityEngine;
using UnityEngine.SceneManagement;

public class StartUp : MonoBehaviour
{
    void OnEnable()
    {
        SceneManager.LoadSceneAsync("Home", LoadSceneMode.Additive);
    }
}
