using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class CardActionBehavior : MonoBehaviour
{

    public virtual bool IsManaCard()
    {
        return false;
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
