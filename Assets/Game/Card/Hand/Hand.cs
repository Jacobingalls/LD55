using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using info.jacobingalls.jamkit;

[RequireComponent(typeof(PubSubListener))]
public class Hand : MonoBehaviour
{

    public int maxFullWidthCards = 7;
    public float fullWidthSize = 3f;
    public float fullWidthSpacing = 0.5f;
    public float unhoveredScale = 0.5f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    CardCursor[] GetCards()
    {
        CardCursor[] cards = gameObject.GetComponentsInChildren<CardCursor>();

        return cards;
    }

    CardCursor[] GetCardsInHand()
    {
        CardCursor[] cards = gameObject.GetComponentsInChildren<CardCursor>();

        return cards.Where(c => !c.isSelected && c.context == null).OrderBy(c => c.order).ToArray(); ;
    }

    // Update is called once per frame
    void Update()
    {
        var cards = GetCardsInHand();
        if (cards.Length == 0) { return; }

        float cardSize = unhoveredScale * fullWidthSize;
        float maxAllowedSpace = (maxFullWidthCards * cardSize) + ((maxFullWidthCards - 1) * fullWidthSpacing);
        float spaceNeeded = (cards.Length * cardSize) + ((cards.Length - 1) * fullWidthSpacing);
        float scaleNeeded = Mathf.Min(1f, maxAllowedSpace / spaceNeeded);
        float usedSpace = spaceNeeded * scaleNeeded;

        var manaCards = new List<CardCursor>();
        for (int i = 0; i < cards.Length; i++)
        {

            float pos = ((-1 * usedSpace) / 2f) + ((usedSpace / cards.Length) * i);
            pos += (cardSize * scaleNeeded) / 2f; // Account for origin of card being in the center.

            CardCursor card = cards[i];
            card.handPosition = new Vector3(pos, 0, 0);
            card.handScale = unhoveredScale * scaleNeeded;

            if (card.IsManaCard())
            {
                manaCards.Add(card);
            }
        }

        foreach(var manaCard in manaCards)
        {
            manaCard.handPosition = new Vector3(-1000, 1000.0f, manaCard.transform.position.z);
            manaCard.executeOnceHandPositionReached = true;
            manaCard.transform.SetParent(manaCard.transform.parent.parent);
        }
    }

    public void DiscardHand()
    {
        var cards = GetCards();

        Debug.Log("DiscardHand");

        for (int i = 0; i < cards.Length; i++)
        {
            Debug.Log("Return to Deck");
            var card = cards[i];
            card.card.ReturnToDeck();
            Destroy(card.gameObject);
        }
    }
}
