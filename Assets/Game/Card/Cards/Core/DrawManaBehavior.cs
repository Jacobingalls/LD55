using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawManaBehavior : CardActionBehavior
{
    public override bool IsManaCard()
    {
        return true;
    }

    public override bool CanExecute(CardExecutionContext context)
    {
        return true;
    }

    public override void Execute(CardExecutionContext context)
    {
        Debug.Log("Add that mana");
        context.activeLevel.AvailableMana += 1;
    }
}
