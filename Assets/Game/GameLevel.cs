using info.jacobingalls.jamkit;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using static Unity.Collections.Unicode;

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
    public WaveManager WaveManager { get; private set; }

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

    public int AvailableBuys
    {
        get
        {
            return Mathf.Max(_availableBuys, 0);
        }
        set
        {
            if (_availableBuys == value)
            {
                return;
            }

            _availableBuys = value;
            GetComponent<PubSubSender>().Publish("resource.buys.changed", _availableBuys);
        }
    }
    private int _availableBuys = -1;

    public int AvailableMana
    {
        get
        {
            return Mathf.Max(_availableMana, 0);
        }
        set
        {
            if (_availableMana == value)
            {
                return;
            }

            _availableMana = value;
            GetComponent<PubSubSender>().Publish("resource.mana.changed", _availableMana);
        }
    }
    private int _availableMana = -1;

    public int AvailableActions
    {
        get
        {
            return Mathf.Max(_availableActions, 0);
        }
        set
        {
            if (_availableActions == value)
            {
                return;
            }

            _availableActions = value;
            GetComponent<PubSubSender>().Publish("resource.actions.changed", _availableActions);
        }
    }
    private int _startingActions = 2;
    private int _availableActions = -1;

    public void Awake()
    {
        _summonsParent = new GameObject("Summons");
        _summonsParent.transform.parent = transform;
    }

    public void Start()
    {
        InitializeRound();
    }

    public void InitializeRound()
    {
        AvailableMana = 0;
        AvailableBuys = 1;
        AvailableActions = _startingActions;

        var round = WaveManager.CurrentWave + 1;
        if (WaveManager.CurrentWaveCompleted())
        {
            round += 1;
        }

        GetComponent<PubSubSender>().Publish("round.started", round);

        DrawPlayerHand();
    }

    public void DrawPlayerHand()
    {
        GetComponent<PubSubSender>().Publish("hand.discard.all");
        const int cardsPerHand = 5;
        GetComponent<PubSubSender>().Publish("deck.draw.number", cardsPerHand);
    }

    public void OnWaveComplete(PubSubListenerEvent e)
    {
        var waveManager = e.value as WaveManager;
        if (waveManager != null && waveManager != WaveManager)
        {
            return;
        }

        if (WaveManager.AllWavesCompleted())
        {
            LevelManager.StartNextLevel();
            return;
        }

        InitializeRound();
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

    public void RegisterWaveManager(WaveManager waveManager)
    {
        if (WaveManager != null)
        {
            Debug.LogError("Trying to register wave manager for a game level that already has one registered.");
            return;
        }

        WaveManager = waveManager;
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
