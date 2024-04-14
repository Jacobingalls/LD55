using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System;

[CreateAssetMenu(fileName = "DeckDefinition", menuName = "Card/Deck", order = 1)]
public class DeckDefinition : ScriptableObject
{
    public string Name = "Red Deck";
    public Sprite CardBack = null;
    public CardCount[] CardCounts;

    public List<CardActionDefinition> Cards
    {
        get
        {
            List<CardActionDefinition> cards = new List<CardActionDefinition>();
            foreach (var cardCount in CardCounts)
            {
                for (int i = 0; i < cardCount.Count; i++)
                {
                    cards.Add(cardCount.Card);
                }
            }

            // Shuffle deck :)
            return cards.OrderBy( _ => Guid.NewGuid()).ToList();
        }
    }
}

[System.Serializable]
public class CardCount
{
    public CardActionDefinition Card;
    public int Count;
}