using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;

public class CardForSale : MonoBehaviour
{

    public Shop shop;

    public CardActionDefinition cardActionDefinition;
    public bool isSpecial = false;
    public int available = 0;

    public Card card;
    public Button buyButton;
    public TextMeshProUGUI buyButtonLabel;

    public GameObject buyAnnotationsView;
    public TextMeshProUGUI specialLabel;
    public TextMeshProUGUI availableLabel;

    public GameObject manaCostIcon;
    public GameObject manaCostIconContainer;
    public TextMeshProUGUI manaCostLabel;

    private LevelManager _levelManger;

    // Start is called before the first frame update
    void Start()
    {
        _levelManger = FindObjectOfType<LevelManager>();
        card.actionDefinition = cardActionDefinition;
        UpdateUI();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateUI();
    }

    public void UpdateUI()
    {
        var evaluatedPurchase = EvaluatePurchase();
        buyButton.interactable = evaluatedPurchase.canBuy;
        buyButtonLabel.text = evaluatedPurchase.rejectionReason ?? "Buy";

        availableLabel.gameObject.SetActive(available != 1);
        availableLabel.text = available + " available";
        specialLabel.gameObject.SetActive(isSpecial);
        buyAnnotationsView.SetActive(isSpecial || available != 1);

        var cost = card.actionDefinition.PurchaseCost;
        if (cost == 0)
        {
            manaCostIcon.SetActive(false);
            manaCostIconContainer.SetActive(false);
        }
        else
        {
            manaCostIcon.SetActive(true);
            manaCostIconContainer.SetActive(true);
            manaCostLabel.text = $"{cost}";
        }
    }

    public struct EvaluatedPurchaseDecision
    {
        public bool canBuy;
        public string rejectionReason;
    }

    public EvaluatedPurchaseDecision EvaluatePurchase()
    {
        if (shop == null) {
            return new EvaluatedPurchaseDecision()
            {
                canBuy = false,
                rejectionReason = "ERROR"
            };
        }
        if (!shop.CanBuy(cardActionDefinition, isSpecial))
        {
            return new EvaluatedPurchaseDecision()
            {
                canBuy = false,
                rejectionReason = "Out of Stock"
            };
        }
        var playerBuyActions = _levelManger.ActiveLevel.AvailableBuys;
        if (playerBuyActions == 0)
        {
            return new EvaluatedPurchaseDecision()
            {
                canBuy = false,
                rejectionReason = "No Buys Left"
            };
        }

        var playerMana = _levelManger.ActiveLevel.AvailableMana;
        if (playerMana < card.actionDefinition.PurchaseCost)
        {
            return new EvaluatedPurchaseDecision()
            {
                canBuy = false,
                rejectionReason = "Can't Afford"
            };
        }

        return new EvaluatedPurchaseDecision()
        {
            canBuy = true,
        };
    }

    public void Buy()
    {
        if (shop == null) { return;  }

        shop.Buy(cardActionDefinition, isSpecial);
        Debug.Log("Buy cardActionDefinition.PurchaseCost " + cardActionDefinition.PurchaseCost);
        _levelManger.ActiveLevel.AvailableMana -= cardActionDefinition.PurchaseCost;
        _levelManger.ActiveLevel.AvailableBuys -= 1;
    }
}
