using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CardCursor : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{

    public float handScale = 1.0f;
    public Vector3 handPosition;
    public float handHoverScale = 1.0f;
    public Vector3 handHoverOffset = new Vector3(0, 0.8f, -1f);

    public Card card;
    CanvasGroup cardCanvasGroup;
    public AnimationCurve cardProgressCurve;

    public GameObject placementIndicator;
    SpriteRenderer placementIndicatorSpriteRenderer;
    public AnimationCurve placementIndicatorCurve;
    Vector3 originalPlacementIndicatorScale;

    Vector3 targetPosition;
    Vector3 targetScale;
    Quaternion targetRotation = Quaternion.Euler(0, 0, 0);
    float positionLerp = 10f;
    float scaleLerp = 10f;
    float rotationLerp = 10f;




    [Range(0f, 1f)]
    public float progress;

    public bool isSelected = false;
    public bool isHovered = false;
    public CardDrop cardDrop;

    [Range(1f, 5f)]
    public float speed = 1f; 

    // Start is called before the first frame update
    void Start()
    {
        cardCanvasGroup = card.GetComponent<CanvasGroup>();
        placementIndicatorSpriteRenderer = placementIndicator.GetComponent<SpriteRenderer>();
        originalPlacementIndicatorScale = placementIndicator.transform.localScale;
        handPosition = gameObject.transform.position;

        targetPosition = gameObject.transform.localPosition;
        targetScale = gameObject.transform.localScale;
    }

    // Update is called once per frame
    void Update()
    {

        // Advance time
        progress = Mathf.Clamp01(progress + ( (cardDrop != null ? 1 : -1) * Time.deltaTime * speed));

        if (isSelected) {
            MoveWithCursor();
        } else {

            // Check if we were dropped.
            if (cardDrop != null) {
                GameObject.Destroy(gameObject); // We are all done!
                return;
            }

            if (isHovered) {
                targetPosition = handPosition + handHoverOffset;
                targetScale = new Vector3(handHoverScale, handHoverScale, handHoverScale);
            } else {
                targetPosition = handPosition;
                targetScale = new Vector3(handScale, handScale, handScale);
            }

            gameObject.transform.localPosition = Vector3.Lerp(gameObject.transform.localPosition, targetPosition, positionLerp * Time.deltaTime);
            gameObject.transform.localScale = Vector3.Lerp(gameObject.transform.localScale, targetScale, scaleLerp * Time.deltaTime);
            gameObject.transform.localRotation = Quaternion.Lerp(gameObject.transform.localRotation, targetRotation, rotationLerp * Time.deltaTime);
        }

        float cardScale = Mathf.Max(cardProgressCurve.Evaluate(progress), 0.0001f);
        card.transform.localScale = new Vector3(cardScale, cardScale, cardScale);

        float cardAlpha = Mathf.Clamp01(cardProgressCurve.Evaluate(progress));
        cardCanvasGroup.alpha = cardAlpha;

        float placementIndicatorScale = Mathf.Max(placementIndicatorCurve.Evaluate(progress), 0.0001f);
        placementIndicator.transform.localScale = Vector3.Scale(originalPlacementIndicatorScale, new Vector3(placementIndicatorScale, placementIndicatorScale, placementIndicatorScale));

        float placementIndicatorAlpha = Mathf.Clamp01(placementIndicatorCurve.Evaluate(progress));
        placementIndicatorSpriteRenderer.color = new Color(placementIndicatorSpriteRenderer.color.r, placementIndicatorSpriteRenderer.color.g, placementIndicatorSpriteRenderer.color.b, placementIndicatorAlpha);
    }

    void MoveWithCursor()
    {
        

        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit[] hits = Physics.RaycastAll(ray);
        foreach (var hit in hits)
        {
            if (hit.collider == null) { continue; }
            CardDrop drop = hit.transform.gameObject.GetComponent<CardDrop>();
            if (drop == null) { continue; }
            Vector3 realHit = hit.point;
            realHit.z = gameObject.transform.position.z;
            gameObject.transform.position = realHit;
            if (drop.isValidDrop == false)
            {
                cardDrop = null;
                continue;
            } else
            {
                cardDrop = drop;
            }
            break; // Don't do it again in the case there is more than one!
        }


        //if (Physics.Raycast(ray, out var hit))
        //{
        //    //Debug.Log(Input.mousePosition);
        //    //gameObject.transform.position = hit.point - new Vector3(0, 0, Camera.main.nearClipPlane);
        //    gameObject.transform.position = hit.point - Camera.main.transform.forward;
        //    //gameObject.transform.position = new Vector3(hit.point.x, hit.point.y, hit.transform.position.z - 1f);
        //    isHoveringMap = hit.transform.gameObject.GetComponent<CardDrop>() != null;
        //}

        //RaycastHit2D[] hits = Physics2D.GetRayIntersectionAll(Camera.main.ScreenPointToRay(Input.mousePosition));
        //foreach (var hit in hits)
        //{
        //    if (hit.collider == null) { continue; }
        //    CardDrop drop = hit.transform.gameObject.GetComponent<CardDrop>();
        //    if (drop == null) { continue; }

        //    Vector3 mousePos = Input.mousePosition;
        //    mousePos.z = 10f;
        //    Vector3 worldPosition = Camera.main.ScreenToWorldPoint(mousePos);
        //    gameObject.transform.position = worldPosition;

        //    if (drop.isValidDrop == false) { continue; }
        //    isHoveringMap = hit.transform.gameObject.GetComponent<CardDrop>() != null;
        //    break; // Don't do it again in the case there is more than one!
        //}
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isSelected = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isSelected = false;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
    }
}
