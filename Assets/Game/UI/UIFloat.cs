using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIFloat : MonoBehaviour
{
    public Vector3 offset;

    [Range(0.01f, 25.0f)]
    public float timeScale = 1.0f;

    private RectTransform _rect;
    private Vector2 _startingAnchoredPosition;
    void Start()
    {
        _rect = GetComponent<RectTransform>();
        _startingAnchoredPosition = _rect.anchoredPosition;
    }

    // Update is called once per frame
    void Update()
    {
        float xOffset = Mathf.Sin(Time.time * timeScale) * offset.x;
        float yOffset = Mathf.Cos(Time.time * timeScale) * offset.y;
        _rect.anchoredPosition = new Vector2(_startingAnchoredPosition.x + xOffset, _startingAnchoredPosition.y + yOffset);
    }
}
