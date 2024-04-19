using info.jacobingalls.jamkit;
using UnityEngine;

public class RangeIndicator : MonoBehaviour
{
    [SerializeField] private GameObject _visuals;

    private GameObject _cardCursorTarget;
    private bool _showRangeIndicator = false;
    private bool _trackGridPosition = true;
    private Vector3 _mousePosition = Vector3.zero;
    private Vector2Int _mouseGridPosition = Vector2Int.zero;

    private LevelManager _levelManager;

    private void Start()
    {
        _levelManager = FindAnyObjectByType<LevelManager>();
    }

    void Update()
    {

        EvaluateTrackingState();
        EvaluateShouldShowRangeIndicator();

        if (_trackGridPosition)
        {
            transform.position = new Vector3(_mouseGridPosition.x + 0.5f, _mouseGridPosition.y + 0.5f, transform.position.z);
        }
        else
        {
            var positionLerp = 10.0f;
            if (_cardCursorTarget.GetComponent<CardCursor>().card.actionDefinition.Target == CardActionTarget.EmptyBuildableArea) // snaps to grid copy paste hack
            {
                var pos = new Vector3(_mouseGridPosition.x + 0.5f, _mouseGridPosition.y + 0.5f, transform.position.z);
                transform.position = Vector3.Lerp(gameObject.transform.position, pos, 2 * positionLerp * Time.deltaTime);
            }
            else
            {
                var pos = new Vector3(_mousePosition.x, _mousePosition.y, transform.position.z);
                transform.position = Vector3.Lerp(gameObject.transform.position, pos, 2 * positionLerp * Time.deltaTime);
            }
        }
    }

    public void EvaluateTrackingState()
    {
        _mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        _trackGridPosition = _cardCursorTarget == null;

        var mouseGridPosition = new Vector2Int(Mathf.FloorToInt(_mousePosition.x), Mathf.FloorToInt(_mousePosition.y));
        if (mouseGridPosition == _mouseGridPosition)
        {
            return;
        }
        _mouseGridPosition = mouseGridPosition;
    }

    private void EvaluateShouldShowRangeIndicator()
    {
        if (_trackGridPosition)
        {
            _showRangeIndicator = _levelManager.ActiveLevel.GridManager.HasSummon(_mouseGridPosition);
        }
        else
        {
            _showRangeIndicator = _cardCursorTarget != null && _cardCursorTarget.GetComponent<CardCursor>().card.actionDefinition.Target == CardActionTarget.EmptyBuildableArea;
        }

        _visuals.SetActive(_showRangeIndicator);
    }

    public void SetCardCursorSelected(PubSubListenerEvent e)
    {
        _cardCursorTarget = e.sender;
        EvaluateTrackingState();
    }

    public void SetCardCursorUnselected(PubSubListenerEvent e)
    {
        _cardCursorTarget = null;
        EvaluateTrackingState();
    }
}
