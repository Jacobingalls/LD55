using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using info.jacobingalls.jamkit;

[RequireComponent(typeof(PubSubSender))]
public class Shop : MonoBehaviour
{

    public DeckDefinition regularShopDeck, specialShopDeck;
    public List<CardActionDefinition> specialShopCardsLeft;

    public Dictionary<CardActionDefinition, int> regularShopStock;
    public Dictionary<CardActionDefinition, int> specialShopStock;

    public Deck playerGameDeck;

    public GameObject specialSection;
    public GameObject normalSection;

    public GameObject window;
    public GameObject CardForSalePrefab;

    // Start is called before the first frame update
    void Start()
    {
        playerGameDeck = GameObject.FindFirstObjectByType<Deck>();
        ResetStore();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ResetStore()
    {
        regularShopStock = new Dictionary<CardActionDefinition, int>();
        foreach (var card in regularShopDeck.CardCounts) { regularShopStock[card.Card] = card.Count; }

        specialShopCardsLeft = specialShopDeck.Cards;
        PutTopThreeSpecialCardsForSale();

        UpdateStore();
    }

    public void PutTopThreeSpecialCardsForSale()
    {
        specialShopStock = new Dictionary<CardActionDefinition, int>();
        PutTopSpecialCardForSale();
        PutTopSpecialCardForSale();
        PutTopSpecialCardForSale();
    }

    public void PutTopSpecialCardForSale()
    {
        if (specialShopCardsLeft.Count <= 0) { specialShopCardsLeft = specialShopDeck.Cards; } // Never run out... for now...
        CardActionDefinition first = specialShopCardsLeft[0];
        specialShopCardsLeft.RemoveAt(0);
        specialShopStock[first] = specialShopStock.GetValueOrDefault(first, 0) + 1;
    }

    public void UpdateStore()
    {
        UpdateSection(specialSection, true, specialShopStock);
        UpdateSection(normalSection, false, regularShopStock);
    }

    public void UpdateSection(GameObject section, bool isSpecial, Dictionary<CardActionDefinition, int> stock)
    {
        // Remove all children
        while (section.transform.childCount > 0) { DestroyImmediate(section.transform.GetChild(0).gameObject); }

        // Add back in what should be there...
        foreach (var (card, count) in stock)
        {
            if (count <= 0 ) { continue; }

            GameObject o = GameObject.Instantiate<GameObject>(CardForSalePrefab, section.transform);
            CardForSale cfs = o.GetComponent<CardForSale>();
            cfs.shop = this;
            cfs.available = count;
            cfs.cardActionDefinition = card;
            cfs.isSpecial = isSpecial;

        }
    }

    public bool CanBuy(CardActionDefinition card, bool isSpecial)
    {

        // Check if we are even selling the card
        if (isSpecial && specialShopStock.GetValueOrDefault(card, 0) <= 0) {
            return false;
        } else if (!isSpecial && regularShopStock.GetValueOrDefault(card, 0) <= 0) {
            return false;
        }

        return true;
    }

    public void Buy(CardActionDefinition card, bool isSpecial)
    {
        if(!CanBuy(card, isSpecial)) { return; }
        if (isSpecial) {
            specialShopStock[card] = specialShopStock.GetValueOrDefault(card, 0) - 1;
        } else {
            regularShopStock[card] = regularShopStock.GetValueOrDefault(card, 0) - 1;
        }

        playerGameDeck.AddCardToDiscard(card);
        UpdateStore();
    }

    public void OpenStore()
    {
        gameObject.GetComponent<PubSubSender>().Publish("store.wasOpened");
        PutTopThreeSpecialCardsForSale();
        UpdateStore();
        window.SetActive(true);
    }

    public void CloseStore()
    {
        gameObject.GetComponent<PubSubSender>().Publish("store.wasClosed");
        window.SetActive(false);
    }

    public void RefreshSpecial()
    {
        PutTopSpecialCardForSale();
        UpdateStore();
    }
}
