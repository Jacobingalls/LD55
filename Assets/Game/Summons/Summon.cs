using info.jacobingalls.jamkit;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using static Attack;

[RequireComponent(typeof(PubSubSender))]
public class Summon : MonoBehaviour
{
    public SummonDefinition Definition;

    [HideInInspector]
    public GridManager GridManager;

    public string Name
    {
        get
        {
            return Definition.Name;
        }
    }

    private PubSubSender _pubSubSender;

    // Start is called before the first frame update
    void Awake()
    {
        _pubSubSender = GetComponent<PubSubSender>();
        if (GridManager == null)
        {
            // PANIK, try to find one
            GridManager = GameObject.FindObjectOfType<GridManager>();
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void BeginAttack(AttackCompleted completionHandler)
    {

    }

    private void ExecuteAction(Attack a, Attack.ExecutionContext context, AttackCompleted completionHandler)
    {
        Debug.Log($"Executing {a}...");

        var attacker = context.source;

        if (!context.ignoringCost && !attacker.CanAffordAttack(a))
        {
            Debug.LogError($"Unable to execute {a} for {this} - cannot afford it.");
        }

        var recipes = a.BehaviorRecipes.GroupBy(r => r.gameObject)
            .Select(y => y.First())
            .ToList();
        List<AttackBehavior> behaviors = new();

        foreach (var recipe in recipes)
        {
            var go = Instantiate(recipe);
            go.transform.parent = transform;
            go.transform.name = $"{a.Name} Behavior";
            foreach (var b in go.GetComponents<AttackBehavior>())
            {
                behaviors.Add(b);
            }
        }

        var canExecuteAllBehaviors = behaviors.All(b => b.CanExecute(context));

        if (!context.ignoringCost)
        {
            attacker.PayCostForAttack(a);
        }

        foreach (var behavior in behaviors)
        {
            behavior.Execute(context);
        }
    }
}
