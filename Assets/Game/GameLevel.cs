using info.jacobingalls.jamkit;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.Rendering;

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
    private List<Summon> _summons = new();
    private EndPortal _endPortal;
    public LevelManager LevelManager { get; private set; }

    private GameObject _summonsParent;

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
        _summonsParent = new GameObject("Summons");
        _summonsParent.transform.parent = transform;
    }

    public void RegisterLevelManager(LevelManager levelManager)
    {
        if (LevelManager != null)
        {
            Debug.LogError("Trying to register level manager for a game level that already has one registered.");
            return;
        }

        LevelManager = levelManager;
    }

    public void RegisterEndPortal(EndPortal endPortal)
    {
        if (_endPortal != null)
        {
            Debug.LogError("Trying to register end portal for a game level that already has one registered.");
            return;
        }

        _endPortal = endPortal;
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

    public void RegisterSummon(Summon summon)
    {
        if (_summons.Contains(summon))
        {
            Debug.LogError("Trying to register summon that has already been registered.");
            return;
        }

        _summons.Add(summon);
        summon.transform.parent = _summonsParent.transform;
    }

    public void UnregisterSummon(Summon summon)
    {
        if (!_summons.Contains(summon))
        {
            Debug.LogError("Trying to unregister summon that has not been registered.");
            return;
        }

        _summons.Remove(summon);
    }

    public ReadOnlyCollection<Summon> Summons
    {
        get
        {
            return _summons.AsReadOnly();
        }
    }


}
