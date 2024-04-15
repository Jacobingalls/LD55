using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LD55GameManager : MonoBehaviour
{

    public int enemiesKilled = 0;
    public int wavesCompleted = 0;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ResetScore()
    {
        enemiesKilled = 0;
        wavesCompleted = 0;
    }

    public void EnemyKilled()
    {
        enemiesKilled += 1;
    }

    public void WaveCompleted()
    {
        wavesCompleted += 1;
    }
}
