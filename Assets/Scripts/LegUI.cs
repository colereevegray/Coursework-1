using UnityEngine;
using System.Collections;

public class LegUI : MonoBehaviour
{
    [SerializeField] public GameObject[] LegOutlines;
    [SerializeField] public GameObject flash;
    private Movement playerMovement;
    private int previousLegs = 8;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            playerMovement = player.GetComponent<Movement>();
        }
        if (flash != null)
        {
            flash.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (playerMovement != null)
        {
            int legsLeft = playerMovement.jumpCount;
            if (legsLeft< previousLegs)
            {
                TriggerFlash();
            }
            previousLegs = legsLeft;
            UpdateSpiderLegs(legsLeft);
        }
    }

    void UpdateSpiderLegs(int currentLegs)
    {
        for (int i = 0; i<LegOutlines.Length; i++)
        {
            if(LegOutlines[i]!= null)
            {
                LegOutlines[i].SetActive(i<currentLegs);
            }
        }
    }

    void TriggerFlash()
    {
        if (flash != null)
        {
            StopAllCoroutines();
            StartCoroutine(FlashRoutine());
        }
    }

    IEnumerator FlashRoutine()
    {
       for (int i = 0; i < 4; i++)
        {
            flash.SetActive(true);
            yield return new WaitForSeconds(0.1f);
            flash.SetActive(false);
            yield return new WaitForSeconds(0.1f);
        }

    }
}
