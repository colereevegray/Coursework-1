using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonController : MonoBehaviour
{
    [SerializeField] private string sceneName;
    private Collider2D cb;

    void Start()
    {
        cb = GetComponent<Collider2D>();
    }
    public void RestartScene()
    {
        // If sceneName is left blank, reload the current active scene
        if (string.IsNullOrEmpty(sceneName))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        else
        {
            SceneManager.LoadScene(sceneName);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (cb != null && other.gameObject.CompareTag("Player"))
        {
            RestartScene();
        }

    }
}
