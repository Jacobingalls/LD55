using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

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

    // Update is called once per frame
    void Update()
    {
        CardCursor[] cards = gameObject.GetComponentsInChildren<CardCursor>();
        cards = cards.Where(c => !c.isSelected && c.cardDrop == null).ToArray();
        if (cards.Length == 0) { return; }

        float cardSize = unhoveredScale * fullWidthSize;
        float maxAllowedSpace = (maxFullWidthCards * cardSize) + ((maxFullWidthCards - 1) * fullWidthSpacing);
        float spaceNeeded = (cards.Length * cardSize) + ((cards.Length - 1) * fullWidthSpacing);
        float scaleNeeded = Mathf.Min(1f, maxAllowedSpace / spaceNeeded);
        float usedSpace = spaceNeeded * scaleNeeded;

        for (int i = 0; i < cards.Length; i++)
        {

            float pos = ((-1 * usedSpace) / 2f) + ((usedSpace / cards.Length) * i);
            pos += (cardSize * scaleNeeded) / 2f; // Account for origin of card being in the center.

            CardCursor card = cards[i];
            card.handPosition = new Vector3(pos, 0, 0);
            card.handScale = unhoveredScale * scaleNeeded;
        }
    }
}
