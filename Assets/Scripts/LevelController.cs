using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelController : MonoBehaviour
{
    [SerializeField] private string sceneName;

    public void SelectLevel()
    {
        SceneManager.LoadSceneAsync(sceneName);
    }
}
