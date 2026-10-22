using TMPro;
using UnityEngine;

public class legtext : MonoBehaviour
{
    [SerializeField] private GameObject player; 
    private TextMeshProUGUI text;
    void Start()
    {
        text = GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
