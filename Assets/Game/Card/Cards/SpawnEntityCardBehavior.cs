using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnEntityCardBehavior : CardActionBehavior
{

    public GameObject SpawnGameObject;

    public override bool CanExecute(CardExecutionContext context)
    {
        return context.ValidPlacementIgnoringExistingEntities(false);
    }

    public override void Execute(CardExecutionContext context)
    {
        GameObject o = GameObject.Instantiate<GameObject>(SpawnGameObject);
        o.transform.position = context.target?.WorldPosition ?? Vector3.zero;
    }
}
