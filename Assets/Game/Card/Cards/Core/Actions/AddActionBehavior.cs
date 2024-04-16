using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using info.jacobingalls.jamkit;

public class AddActionBehavior : CardActionBehavior
{
    public override bool CanExecute(CardExecutionContext context)
    {
        return true;
    }

    public override void Execute(CardExecutionContext context)
    {
        GameObject.FindFirstObjectByType<LevelManager>().ActiveLevel.AvailableActions += 1;
    }
}
