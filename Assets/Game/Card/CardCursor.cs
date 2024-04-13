using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CardCursor : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{

    public Card card;
    CanvasGroup cardCanvasGroup;
    public AnimationCurve cardProgressCurve;
    Vector3 originalPosition;

    public GameObject placementIndicator;
    SpriteRenderer placementIndicatorSpriteRenderer;
    public AnimationCurve placementIndicatorCurve;
    Vector3 originalPlacementIndicatorScale;

    [Range(0f, 1f)]
    public float progress;

    public bool isSelected = false;
    public bool isHoveringMap = false;

    [Range(1f, 5f)]
    public float speed = 1f; 

    // Start is called before the first frame update
    void Start()
    {
        Physics2DRaycaster physicsRaycaster = FindObjectOfType<Physics2DRaycaster>();
        Camera.main.gameObject.AddComponent<Physics2DRaycaster>();

        cardCanvasGroup = card.GetComponent<CanvasGroup>();
        placementIndicatorSpriteRenderer = placementIndicator.GetComponent<SpriteRenderer>();
        originalPlacementIndicatorScale = placementIndicator.transform.localScale;
        originalPosition = gameObject.transform.position;
    }

    // Update is called once per frame
    void Update()
    {

        // Advance time
        progress = Mathf.Clamp01(progress + ( (isHoveringMap ? 1 : -1) * Time.deltaTime * speed));

        if (isSelected)
        {
            MoveWithCursor();
        } else
        {
            isHoveringMap = false;
            gameObject.transform.position = Vector3.Lerp(gameObject.transform.position, originalPosition, 0.1f);
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
            gameObject.transform.position = hit.point - Camera.main.transform.forward;
            if (drop.isValidDrop == false)
            {
                isHoveringMap = false;
                continue;
            } else
            {
                isHoveringMap = true;
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
        originalPosition = gameObject.transform.position;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isSelected = false;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        
    }
}
