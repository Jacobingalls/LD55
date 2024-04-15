using info.jacobingalls.jamkit;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(PubSubListener))]
public class ShopButton : MonoBehaviour
{
    public TextMeshProUGUI BuyActionsLabel;

    public void SetNumberOfBuyActions(PubSubListenerEvent e)
    {
        var buyActions = e.value;

        BuyActionsLabel.text = $"{buyActions}";
    }
}
