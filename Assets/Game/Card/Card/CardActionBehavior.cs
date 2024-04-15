using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class CardActionBehavior : MonoBehaviour
{
    [Range(0, 5)]
    public int ActionCost = 1;

    public virtual bool IsManaCard()
    {
        return false;
    }

    public bool CanAfford(CardExecutionContext context)
    {
        return ActionCost <= FindObjectOfType<LevelManager>().ActiveLevel.AvailableActions;
    }

    public void PayCost(CardExecutionContext context)
    {
        FindObjectOfType<LevelManager>().ActiveLevel.AvailableActions -= ActionCost;
    }

    public virtual bool CanExecute(CardExecutionContext context)
    {
        throw new NotImplementedException("CardActionBehavior.CanExecute must be implemented in a subclass!");
    }

    public virtual void Execute(CardExecutionContext context)
    {
        throw new NotImplementedException("CardActionBehavior.Execute must be implemented in a subclass!");
    }
}
