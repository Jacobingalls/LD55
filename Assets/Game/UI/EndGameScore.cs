using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class EndGameScore : MonoBehaviour
{

    public TextMeshProUGUI wavesCompleted;
    public TextMeshProUGUI enemiesKilled;
    public LD55GameManager gameManager;

    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameObject.FindFirstObjectByType<LD55GameManager>();
    }

    // Update is called once per frame
    void Update()
    {
        wavesCompleted.text = "" + gameManager.wavesCompleted;
        enemiesKilled.text = "" + gameManager.enemiesKilled;
    }
}
