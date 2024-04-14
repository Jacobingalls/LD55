using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    private int _currentLevelIndex = -1;

    private void Awake()
    {
        for (var i = 0; i < transform.childCount; i++)
        {
            var childTransform = transform.GetChild(i);
            if (childTransform.TryGetComponent<GameLevel>(out var gameLevel))
            {
                _levels.Add(gameLevel);
            }

            gameLevel.gameObject.SetActive(false);
        }

        ActivateLevel(StartingLevel);
    }

    public void StartNextLevel()
    {
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
            _levels[_currentLevelIndex].gameObject.SetActive(false);
        }

        _currentLevelIndex = index;

        var currentLevel = _levels[_currentLevelIndex];
        currentLevel.gameObject.SetActive(true);

        var cameraStartingPosition = currentLevel.GetCameraStartingPosition();
        Camera.main.transform.position = new Vector3(cameraStartingPosition.x, cameraStartingPosition.y, Camera.main.transform.position.z);
    }
}
