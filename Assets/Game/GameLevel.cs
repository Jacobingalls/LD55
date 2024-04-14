using info.jacobingalls.jamkit;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

[RequireComponent(typeof(PubSubSender))]
[RequireComponent(typeof(PubSubListener))]
public class GameLevel : MonoBehaviour
{
    public GridManager GridManager
    {
        get
        {
            if (_gridManager == null)
            {
                _gridManager = GetComponentInChildren<GridManager>();
            }
            return _gridManager;
        }

        private set
        {
            _gridManager = value;
        }
    }
    private GridManager _gridManager;

    private List<Unit> _units = new();

    [SerializeField][Range(1, 40)] private int _startingLife = 10;
    private int _currentLife;

    public Vector3 GetCameraStartingPosition()
    {
        var wizardTowerGO = transform.Find("WizardTower");
        if (wizardTowerGO != null)
        {
            return wizardTowerGO.transform.position;
        }
        else
        {
            return transform.position;
        }
    }

    public void Awake()
    {
    }

    public void Start()
    {
        _currentLife = _startingLife;
    }

    public void UnitHasReachedTheEnd()
    {
        _currentLife -= 1;

        if (_currentLife == 0)
        {
            transform.GetComponentInParent<LevelManager>().StartNextLevel();
        }
    }

    public void RegisterUnit(Unit unit)
    {
        if (_units.Contains(unit))
        {
            Debug.LogError("Trying to register unit that has already been registered.");
            return;
        }

        _units.Add(unit);
    }

    public void UnregisterUnit(Unit unit)
    {
        if (!_units.Contains(unit))
        {
            Debug.LogError("Trying to unregister unit that has not been registered.");
            return;
        }

        _units.Remove(unit);
    }

    public ReadOnlyCollection<Unit> Units
    {
        get
        {
            return _units.AsReadOnly();
        }
    }


}
