using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using info.jacobingalls.jamkit;

[RequireComponent(typeof(PubSubSender))]
public class LevelManager : MonoBehaviour
{
    private List<GameLevel> _levels = new();

    public int StartingLevel = 0;

    public GameLevel ActiveLevel
    {
        get
        {
            return _levels[_currentLevelIndex];
        }
    }

    public int CurrentLevelNumber
    {
        get
        {
            return Mathf.Max(_currentLevelIndex, 0);
        }
    }

    public int LevelCount
    {
        get
        {
            return _levels.Count;
        }
    }


    private int _currentLevelIndex = -1;

    private void Awake()
    {
        for (var i = 0; i < transform.childCount; i++)
        {
            var childTransform = transform.GetChild(i);
            if (childTransform.TryGetComponent<GameLevel>(out var gameLevel))
            {
                _levels.Add(gameLevel);
                gameLevel.RegisterLevelManager(this);
            }

            gameLevel.gameObject.SetActive(false);
        }

        StartCoroutine(StartNextLevelWithDelay(0.050f));
    }

    public IEnumerator StartNextLevelWithDelay(float seconds)
    {
        yield return new WaitForSecondsRealtime(seconds);

        ActivateLevel(StartingLevel);
    }

    public void StartNextLevel()
    {
        if (_currentLevelIndex == _levels.Count - 1)
        {
            var sender = GetComponent<PubSubSender>();
            sender.Publish("gameManager.showWin");
        }

        ActivateLevel(_currentLevelIndex + 1);
    }

    private void ActivateLevel(int index)
    {
        if (!(index >= 0 && index < _levels.Count))
        {
            Debug.LogError($"Unable to ActivateLevel {index}");
            return;
        }

        if (_currentLevelIndex != -1)
        {
            var oldLevel = _levels[_currentLevelIndex];
            oldLevel.gameObject.SetActive(false);
        }

        _currentLevelIndex = index;

        var currentLevel = _levels[_currentLevelIndex];
        currentLevel.gameObject.SetActive(true);

        var cameraStartingPosition = currentLevel.GetCameraStartingPosition();
        Camera.main.transform.position = new Vector3(cameraStartingPosition.x, cameraStartingPosition.y, Camera.main.transform.position.z);
    }
}
