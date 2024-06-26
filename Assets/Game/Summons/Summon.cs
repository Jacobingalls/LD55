using info.jacobingalls.jamkit;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(PubSubSender))]
public class Summon : MonoBehaviour
{
    public enum TargetPriority
    {
        First,
        Closest,
        Strongest,
        Fastest,
        Vulnerable,
    }

    [HideInInspector]
    public LevelManager LevelManager;

    public string Name = "Summon";

    [Header("Stats")]
    [Range(0.1f, 50.0f)]
    [SerializeField] private float _range = 10.0f;
    [SerializeField] private TargetPriority _targetPriority = TargetPriority.First;

    [Header("Abilities")]
    [SerializeField] private List<SummonAbility> _abilityPrefabs = new();
    [SerializeField] private float _cooldownBetweenAbilities = 0.0f;
    [SerializeField] private Transform _abilityOriginTransform;
    
    private List<SummonAbility> _abilities = new();
    private int _currentAbilityIndex = 0;
    private float _timeSinceLastAbilityCast;

    private Dictionary<Targetable.TargetType, Targetable> _currentTargets = new();
    
    private PubSubSender _pubSubSender;

    // Start is called before the first frame update
    void Awake()
    {
        _pubSubSender = GetComponent<PubSubSender>();
        if (LevelManager == null)
        {
            // PANIK, try to find one
            LevelManager = GameObject.FindObjectOfType<LevelManager>();
        }

        var abilitiesGO = new GameObject("Abilities");
        abilitiesGO.transform.parent = transform;
        foreach(var abilityPrefab in _abilityPrefabs)
        {
            var abilityGO = Instantiate(abilityPrefab);
            abilityGO.transform.parent = abilitiesGO.transform;
            _abilities.Add(abilityGO.GetComponent<SummonAbility>());
        }
    }

    private GameLevel _myLevel;

    private void Start()
    {
        _myLevel = LevelManager.ActiveLevel;
        _myLevel.RegisterSummon(this);
    }


    // Update is called once per frame
    void Update()
    {
        CastAbilityIfAble();
    }

    void CastAbilityIfAble()
    {
        _timeSinceLastAbilityCast += Time.deltaTime;

        if (_timeSinceLastAbilityCast < _cooldownBetweenAbilities)
        {
            return;
        }

        // Find next available ability
        var startingIndex = _currentAbilityIndex;
        var currentIndex = _currentAbilityIndex;
        do
        {
            var candidateAbility = _abilities[startingIndex];
            
            if (!candidateAbility.OnCooldown())
            {
                var context = ExecutionContextForAbility(candidateAbility);
                if (context.target != null && candidateAbility.CanExecute(context))
                {
                    candidateAbility.Execute(context);
                    break;
                }
            }

            currentIndex = (currentIndex + 1) % _abilities.Count;
        } while (startingIndex != currentIndex);
    }

    Targetable TargetForUnit(SummonAbility ability)
    {
        if (_myLevel.Units.Count == 0)
        {
            return null;
        }

        Targetable newTarget = null;
        var bestUnit = _myLevel.Units.First();
        switch (_targetPriority)
        {
            case TargetPriority.Closest:
                var bestUnitDistance = Vector3.Distance(transform.position, bestUnit.transform.position);
                foreach(var candidateUnit in _myLevel.Units)
                {
                    var candidateUnitDistance = Vector3.Distance(transform.position, candidateUnit.transform.position);
                    if (candidateUnitDistance < bestUnitDistance)
                    {
                        bestUnitDistance = candidateUnitDistance;
                        bestUnit = candidateUnit;
                    }
                    if (bestUnitDistance <= _range)
                    {
                        newTarget = bestUnit.GetComponent<Targetable>();
                    }
                }
                break;
            default:
                break;
        }

        if (newTarget != null)
        {
            _currentTargets[Targetable.TargetType.Unit] = newTarget;
        }
        else
        {
            _currentTargets.Remove(Targetable.TargetType.Unit);
        }

        return newTarget;
    }

    Targetable TargetForAbility(SummonAbility ability)
    {
        switch (ability.TargetType)
        {
            case Targetable.TargetType.Unit:
                return TargetForUnit(ability);
            case Targetable.TargetType.Summon:
            case Targetable.TargetType.Location:
            default:
                return null;
        }
    }

    SummonAbility.ExecutionContext ExecutionContextForAbility(SummonAbility ability)
    {
        SummonAbility.ExecutionContext context = new()
        {
            originPosition = _abilityOriginTransform.position,
            source = this,
            target = TargetForAbility(ability),
            level = _myLevel
        };

        return context;
    }
}
