using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.VisualScripting;
using UnityEngine;

public class SummonAbility : MonoBehaviour
{
    public Targetable.TargetType TargetType = Targetable.TargetType.Unit;

    [Range(0.0f, 10.0f)]
    public float Cooldown = 2.5f;

    protected bool _busy;
    protected float _currentCooldown;

    public virtual void Update()
    {
        if(OnCooldown())
        {
            _currentCooldown -= Time.deltaTime;
        }
    }

    public class ExecutionContext {
        public Vector3 originPosition;
        public Summon source;
        public Targetable target;
        public GameLevel world;

        public override string ToString() => $"<ExecutionContext: source={source.Name}, target={target}>";
    }

    public bool OnCooldown()
    {
        return _currentCooldown > 0.0001f;
    }

    public virtual bool CanExecute(ExecutionContext context)
    {
        throw new NotImplementedException("SummonAbility.CanExecute must be implemented in a subclass!");
    }

    public virtual bool Execute(ExecutionContext context)
    {
        throw new NotImplementedException("SummonAbility.CanExecute must be implemented in a subclass!");
    }
}
