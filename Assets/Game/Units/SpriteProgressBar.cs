using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SpriteProgressBar : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _backgroundSprite;
    [SerializeField] private SpriteRenderer _progressSprite;

    public float CurrentProgress
    {
        get
        {
            return _currentProgress;
        }
        set
        {
            if (_currentProgress == value)
            {
                return;
            }

            _currentProgress = Mathf.Clamp01(_currentProgress);
            CalculateProgressBar(_currentProgress);
        }
    }
    private float _currentProgress;

    void CalculateProgressBar(float progress)
    {
        float xScale = progress;
        float xOffset = 1.0f - ((1.0f - progress) / 2.0f);

        _progressSprite.transform.position = new Vector3(xOffset, _progressSprite.transform.position.y, _progressSprite.transform.position.z);
        _progressSprite.transform.localScale = new Vector3(xScale, _progressSprite.transform.localScale.y, _progressSprite.transform.localScale.z);
    }
}
