using UnityEngine;
using UnityEngine.SceneManagement;

public class Playbutton : MonoBehaviour
{
    public void PlayLevels()
    {
        SceneManager.LoadScene("jump_tut");
    }
}
