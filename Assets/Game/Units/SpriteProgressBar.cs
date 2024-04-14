using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SpriteProgressBar : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _backgroundSprite;
    [SerializeField] private SpriteRenderer _progressSprite;

    public bool HideOnZeroProgress = true;

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

            _currentProgress = Mathf.Clamp01(value);
            CalculateProgressBar(_currentProgress);
        }
    }
    private float _currentProgress;

    void CalculateProgressBar(float progress)
    {
        float xScale = progress;
        float xOffset = (1.0f - progress) / 2.0f;

        _progressSprite.transform.localPosition = new Vector3(-xOffset, _progressSprite.transform.localPosition.y, _progressSprite.transform.localPosition.z);
        _progressSprite.transform.localScale = new Vector3(xScale, _progressSprite.transform.localScale.y, _progressSprite.transform.localScale.z);

        if (HideOnZeroProgress && progress <= 0.0001f)
        {
            _backgroundSprite.gameObject.SetActive(false);
            _progressSprite.gameObject.SetActive(false);
        }
        else
        {
            _backgroundSprite.gameObject.SetActive(true);
            _progressSprite.gameObject.SetActive(true);
        }
    }
}
