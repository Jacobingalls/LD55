using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RectTransformAnimator : MonoBehaviour
{

    RectTransform rectTransform;

    public AnimationCurve yPosAnimation;
    float startingYPos;

    float currentTime = 0f;
    public float animationTime = 1f;


    // Start is called before the first frame update
    void Start()
    {
        rectTransform = gameObject.GetComponent<RectTransform>();
        startingYPos = rectTransform.position.y;
    }

    // Update is called once per frame
    void Update()
    {
        currentTime += Time.deltaTime;
        while (currentTime > animationTime) { currentTime -= animationTime; }
        float progress = currentTime / animationTime;

        rectTransform.position = new Vector3(
            rectTransform.position.x,
            startingYPos + yPosAnimation.Evaluate(progress),
            rectTransform.position.z
        );
    }
}
