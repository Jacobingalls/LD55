using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using info.jacobingalls.jamkit;

[RequireComponent(typeof(PubSubSender))]
public class CardCursor : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{

    public int order = 0;

    public float handScale = 1.0f;
    public Vector3 handPosition;
    public bool executeOnceHandPositionReached;


    public float handHoverScale = 1.0f;
    public Vector3 handHoverOffset = new Vector3(0, 20f, -1f);

    public Card card;
    CanvasGroup cardCanvasGroup;
    public AnimationCurve cardProgressCurve;

    public GameObject placementIndicator;
    Image placementIndicatorImage;
    public AnimationCurve placementIndicatorCurve;
    Vector3 originalPlacementIndicatorScale;
    public Color placementGoodColor, placementBadColor;

    public GameObject cardFront;
    public GameObject cardBack;

    Vector3 targetPosition;
    Vector3 targetScale;
    Quaternion targetRotation = Quaternion.Euler(0, 0, 0);
    float positionLerp = 10f;
    float scaleLerp = 10f;
    float rotationLerp = 10f;

    LevelManager levelManager;
    CameraControls cameraControls;

    [Range(0f, 1f)]
    public float progress;

    public bool isSelected = false;
    public bool isHovered = false;
    public CardExecutionContext? context;

    [Range(1f, 5f)]
    public float speed = 1f; 

    public bool IsManaCard()
    {
        foreach (var behavior in card.actionDefinition.Behaviors)
        {
            if (behavior.IsManaCard())
            {
                return true;
            }
        }
        return false;
    }

    // Start is called before the first frame update
    void Start()
    {
        cardCanvasGroup = card.GetComponent<CanvasGroup>();
        placementIndicatorImage = placementIndicator.GetComponent<Image>();
        originalPlacementIndicatorScale = placementIndicator.transform.localScale;
        handPosition = gameObject.transform.position;
        placementIndicatorImage.sprite = card.actionDefinition.PlacementIcon;


        targetPosition = gameObject.transform.localPosition;
        targetScale = gameObject.transform.localScale;

        levelManager = GameObject.FindFirstObjectByType<LevelManager>();
        cameraControls = GameObject.FindFirstObjectByType<CameraControls>();
    }

    private bool IsCloseEnough(Vector3 target)
    {
        return Vector3.Distance(gameObject.transform.localPosition, target) < 10.0f;
    }

    // Update is called once per frame
    void Update()
    {

        // Advance time
        progress = Mathf.Clamp01(progress + ( (context != null ? 1 : -1) * Time.deltaTime * speed));

        if (isSelected) {
            targetScale = new Vector3(1f, 1f, 1f);

            Vector3 pos = MoveWithCursor();
            if (context?.clicksToGrid == true) {
                pos = (context?.target?.WorldPosition) ?? Vector3.zero;
                pos = Camera.main.WorldToScreenPoint(pos);
                pos += new Vector3(0, 10f, 0); // Seems to want a slight poke up to align.
                gameObject.transform.position = Vector3.Lerp(gameObject.transform.position, pos, 2 * positionLerp * Time.deltaTime);
            } else {
                gameObject.transform.position = pos; // Don't lerp if we aren't clicking to grid
            }
            
        } else {

            if (isHovered) {
                targetPosition = handPosition + handHoverOffset;
                targetScale = new Vector3(handHoverScale, handHoverScale, handHoverScale);
                gameObject.transform.SetAsLastSibling();
            } else {
                targetPosition = handPosition;
                targetScale = new Vector3(handScale, handScale, handScale);
            }
            gameObject.transform.localPosition = Vector3.Lerp(gameObject.transform.localPosition, targetPosition, positionLerp * Time.deltaTime);

            if (IsCloseEnough(targetPosition) && executeOnceHandPositionReached)
            {
                context = new CardExecutionContext(card.actionDefinition,
                    levelManager,
                    levelManager.ActiveLevel,
                    levelManager.ActiveLevel.GridManager,
                    Vector3.zero);
                executeOnceHandPositionReached = false;
                PlayOnCardDrop(forced: true);
            }
        }

        gameObject.transform.localScale = Vector3.Lerp(gameObject.transform.localScale, targetScale, scaleLerp * Time.deltaTime);
        gameObject.transform.localRotation = Quaternion.Lerp(gameObject.transform.localRotation, targetRotation, rotationLerp * Time.deltaTime);

        bool showFront = Vector3.Dot(gameObject.transform.forward, Camera.main.transform.forward) > 0;
        cardFront.SetActive(true);
        cardBack.SetActive(!showFront);

        float cardScale = Mathf.Max(cardProgressCurve.Evaluate(progress), 0.0001f);
        card.transform.localScale = new Vector3(cardScale, cardScale, cardScale);

        float cardAlpha = Mathf.Clamp01(cardProgressCurve.Evaluate(progress));
        cardCanvasGroup.alpha = cardAlpha;

        
        

        // Hack needed to get the image to scale with camera.
        float xCameraScale = (Camera.main.ViewportToWorldPoint(new Vector3(1f, 0f, 0)) - Camera.main.ViewportToWorldPoint(new Vector3(0, 0, 0))).x;
        xCameraScale = 20f / xCameraScale;
        float placementIndicatorScale = Mathf.Max(placementIndicatorCurve.Evaluate(progress), 0.0001f);
        placementIndicator.transform.localScale = Vector3.Scale(new Vector3(xCameraScale, xCameraScale, xCameraScale), Vector3.Scale(originalPlacementIndicatorScale, new Vector3(placementIndicatorScale, placementIndicatorScale, placementIndicatorScale)));

        float placementIndicatorAlpha = Mathf.Clamp01(placementIndicatorCurve.Evaluate(progress));
        Color color = placementBadColor;
        if (context is CardExecutionContext con && con.Validate()){ color = placementGoodColor; }
        color.a *= placementIndicatorAlpha;
        placementIndicatorImage.color = color;
    }

    Vector3 MoveWithCursor()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = 10f;
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(mousePos);
        CardExecutionContext context = new CardExecutionContext(card.actionDefinition, levelManager, levelManager.ActiveLevel, levelManager.ActiveLevel.GridManager, worldPosition);
        if (context.ValidPlacementIgnoringExistingEntities(true)) {
            this.context = context;
        } else {
            this.context = null;
        }
        return mousePos;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right) { isSelected = false; context = null; return; }
        if (eventData.button != PointerEventData.InputButton.Left) { return; }
        isSelected = true;

        PlayCardPlayAudio();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right) { isSelected = false; context = null; return; }
        if (eventData.button != PointerEventData.InputButton.Left) { return; }

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
        isHovered = !cameraControls.CameraIsPanning; // Don't hover when moving the cursor due to panning.
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
    }

    public void PlayCardPlayAudio()
    {
        AudioManager.Instance.Play("Card/Play",
        pitchMin: 0.8f, pitchMax: 1.2f,
        volumeMin: 0.25f, volumeMax: 0.25f,
        position: Camera.main.transform.position,
        minDistance: 10, maxDistance: 20);
    }

    public void PlayInsufficentResourcesAudio()
    {
        AudioManager.Instance.Play("Card/CantPlayNoResources",
        pitchMin: 0.8f, pitchMax: 1.2f,
        volumeMin: 0.3f, volumeMax: 0.3f,
        position: Camera.main.transform.position,
        minDistance: 10, maxDistance: 20);
    }

    public void PlayOnCardDrop(bool forced = false)
    {
        Debug.Log("PlayOnCardDrop " + this);
        if (context != null) {
            if (forced || context?.Validate() == true)
            {
                Debug.Log("Execute " + this);
                Debug.Log(context?.actionDefinition);
                Debug.Log(context?.target);
                context?.Execute();
                card.ReturnToDeck();
                GameObject.Destroy(gameObject);
            }
            else
            {
                if (context?.CanAfford() == false)
                {
                    PlayInsufficentResourcesAudio();
                }
                context = null;
            }
        } else
        {
            context = null;
        }
    }
}
