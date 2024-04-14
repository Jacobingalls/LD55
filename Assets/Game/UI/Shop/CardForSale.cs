using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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

    // Start is called before the first frame update
    void Start()
    {
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
        buyButton.interactable = CanBuy();
        buyButtonLabel.text = CostString();

        availableLabel.gameObject.SetActive(available != 1);
        availableLabel.text = available + " available";
        specialLabel.gameObject.SetActive(isSpecial);
        buyAnnotationsView.SetActive(isSpecial || available != 1);
    }

    public string CostString()
    {
        return "Free";
    }

    public bool CanBuy()
    {
        if (shop == null) { return false; }
        return shop.CanBuy(cardActionDefinition, isSpecial);
    }

    public void Buy()
    {
        if (shop == null) { return;  }
        shop.Buy(cardActionDefinition, isSpecial);
    }
}
