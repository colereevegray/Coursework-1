using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PersistentMusic : MonoBehaviour
{
    private static PersistentMusic instance;
    private AudioSource audioSource;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            audioSource = GetComponent<AudioSource>();
            audioSource.loop = true;
        }
        else
        {
            Destroy(gameObject);
        }
    }
}