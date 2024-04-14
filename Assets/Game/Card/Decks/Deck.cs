using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using info.jacobingalls.jamkit;
using System.Linq;
using System;

public class Deck : MonoBehaviour
{

    public DeckDefinition deckDefinition;
    public List<CardActionDefinition> cardsInDeck, cardsInDiscard = new List<CardActionDefinition>();

    public GameObject cardCursorPrefab;
    Hand hand;

    int cardNumber = 0;
    float timeUntilNextSpawn = 0f;
    float minTimeBetweenSpawns = 0.1f;
    public int numberOfCardsWantingToBeSpawned = 0;

    // Start is called before the first frame update
    void Start()
    {
        hand = GameObject.FindFirstObjectByType<Hand>();
        cardsInDeck = deckDefinition.Cards;
    }

    // Update is called once per frame
    void Update()
    {
        timeUntilNextSpawn = Mathf.Max(0, timeUntilNextSpawn - Time.deltaTime);
        if (timeUntilNextSpawn == 0 && numberOfCardsWantingToBeSpawned > 0)
        {
            numberOfCardsWantingToBeSpawned -= 1;
            SpawnCard();
            timeUntilNextSpawn = minTimeBetweenSpawns;
        }
    }

    public void DrawCard()
    {
        numberOfCardsWantingToBeSpawned += 1;
    }

    public void DrawCards(int number)
    {
        numberOfCardsWantingToBeSpawned += Mathf.Max(0, number);
    }

    public void DrawCardsEvent(PubSubListenerEvent e)
    {
        DrawCards((int)e.value);
    }

    public GameObject SpawnCard()
    {
        if (cardsInDeck.Count <= 0 && cardsInDiscard.Count > 0)
        {
            cardsInDeck = cardsInDiscard.OrderBy(_ => Guid.NewGuid()).ToList();
            cardsInDiscard = new List<CardActionDefinition>();
        }

        if (cardsInDeck.Count <= 0) { return null; }
        CardActionDefinition topCard = cardsInDeck.First();
        cardsInDeck.RemoveAt(0);

        GameObject o = GameObject.Instantiate<GameObject>(cardCursorPrefab, hand.gameObject.transform);
        o.transform.position = gameObject.transform.position; // Set world pos, but keep transform in hand so it gets animated to the right place.
        o.transform.rotation = Quaternion.Euler(0, -180, 0); // Spawn it upside down.

        CardCursor card = o.GetComponent<CardCursor>();
        if(card != null) {
            card.order = cardNumber;
            cardNumber += 1;
            card.card.actionDefinition = topCard;
            card.card.deck = this;
        }

        return o;
    }

    public void AddCardToDiscard(CardActionDefinition card)
    {
        cardsInDiscard.Add(card);
    }
}
