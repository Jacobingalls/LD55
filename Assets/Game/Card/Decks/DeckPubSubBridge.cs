using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using info.jacobingalls.jamkit;

[RequireComponent(typeof(PubSubSender))]
public class DeckPubSubBridge : MonoBehaviour
{
    public void DrawCards(int number)
    {
        gameObject.GetComponent<PubSubSender>().Publish("deck.draw.number", number);
    }
}
