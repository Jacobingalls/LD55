using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using info.jacobingalls.jamkit;

[RequireComponent(typeof(PubSubSender))]
public class DrawOneCardBehavior : CardActionBehavior
{
    public override bool CanExecute(CardExecutionContext context)
    {
        return true;
    }

    public override void Execute(CardExecutionContext context)
    {
        gameObject.GetComponent<PubSubSender>().Publish("deck.draw.number", 1);
    }
}
