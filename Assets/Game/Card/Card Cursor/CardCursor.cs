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


    GridManager gridManager;

    [Range(0f, 1f)]
    public float progress;

    public bool isSelected = false;
    public bool isHovered = false;
    public CardExecutionContext? context;

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

        gridManager = GameObject.FindFirstObjectByType<GridManager>();
    }

    // Update is called once per frame
    void Update()
    {

        // Advance time
        progress = Mathf.Clamp01(progress + ( (context != null ? 1 : -1) * Time.deltaTime * speed));

        if (isSelected) {
            MoveWithCursor();
        } else {

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
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = 10f;
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(mousePos);
        gameObject.transform.position = worldPosition;

        CardExecutionContext context = new CardExecutionContext(card.actionDefinition, gridManager, gameObject.transform.position);
        if (context.Validate())
        {
            this.context = context;
            return;
        }
        this.context = null;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isSelected = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isSelected = false;

        // Check if we were dropped.
        if (context != null)
        {
            PlayOnCardDrop();
            return;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
    }

    public void PlayOnCardDrop()
    {
        if (context != null) {
            context?.Execute();
            GameObject.Destroy(gameObject); // We are all done!
        }
    }
}
