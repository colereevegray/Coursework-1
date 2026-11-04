using UnityEngine;
using UnityEngine.SceneManagement;

public class BackButton : MonoBehaviour
{
    public void LoadMainMenu()
    {
       SceneManager.LoadScene("mainmenu");
    }
}
