using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;
using static UnityEngine.RuleTile.TilingRuleOutput;

public interface IDamageable
{
    void DealDamage(int damageDealt);
}

public interface IActor
{
    bool CanAffordAction(Action a);
    bool PayCostForAction(Action a);
}

public class ActionBehavior : MonoBehaviour
{
    public virtual bool CanExecute(Action.ExecutionContext context)
    {
        throw new NotImplementedException("ActionBehavior.CanExecute must be implemented in a subclass!");
    }

    public virtual void Execute(Action.ExecutionContext context)
    {
        throw new NotImplementedException("ActionBehavior.Execute must be implemented in a subclass!");
    }
}

public class Action
{
    public struct ExecutionContext
    {
        public Action Action;
        public IActor source;
        public IDamageable target;
        public bool ignoringCost;

        public override string ToString() {
            return $"<ExecutionContext: action={Action.Name} source={source}, target={target}>";
        }
    }

    public delegate void ActionCompleted(Action a);

    public ActionDefinition Definition;

    public string Name
    {
        get
        {
            return Definition.Name;
        }
    }

    public int Damage
    {
        get
        {
            return Definition.BaseDamage;
        }
    }

    public float ProjectileSpeed
    {
        get
        {
            return Definition.BaseProjectileSpeed;
        }
    }

    public float ActionCooldown
    {
        get
        {
            return Definition.BaseActionCooldown;
        }
    }

    public float Range
    {
        get
        {
            return Definition.BaseRange;
        }
    }

    public List<ActionBehavior> BehaviorRecipes
    {
        get
        {
            return Definition.Behaviors;
        }
    }

    public void BeginAction(IActor actor, IDamageable target)
    {
        ExecutionContext context = new()
        {
            Action = this,
            source = actor,
            target = target,
            ignoringCost = false
        };
        ExecuteAction(this, context);
    }

    private void ExecuteAction(Action a, Action.ExecutionContext context)
    {
        Debug.Log($"Executing {a}...");

        var Actioner = context.source;

        if (!context.ignoringCost && !Actioner.CanAffordAction(a))
        {
            Debug.LogError($"Unable to execute {a} for {this} - cannot afford it.");
        }

        var recipes = a.BehaviorRecipes.GroupBy(r => r.gameObject)
            .Select(y => y.First())
            .ToList();
        List<ActionBehavior> behaviors = new();

        foreach (var recipe in recipes)
        {
            var go = GameObject.Instantiate(recipe);
            var ActionsGo = GameObject.Find("Actions");
            if (ActionsGo == null)
            {
                ActionsGo = new GameObject("Actions");
            }
            go.transform.parent = ActionsGo.transform;
            go.transform.name = $"{a.Name} Behavior";
            foreach (var b in go.GetComponents<ActionBehavior>())
            {
                behaviors.Add(b);
            }
        }

        var canExecuteAllBehaviors = behaviors.All(b => b.CanExecute(context));

        if (!context.ignoringCost)
        {
            Actioner.PayCostForAction(a);
        }

        foreach (var behavior in behaviors)
        {
            behavior.Execute(context);
        }
    }
}
