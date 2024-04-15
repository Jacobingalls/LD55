using info.jacobingalls.jamkit;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(PubSubListener))]
public class GameMenubar : MonoBehaviour
{
    public GameObject ManaIconPrefab;
    public GameObject ActionIconPrefab;

    public GameObject ManaIcons;
    public GameObject ActionIcons;
    public TextMeshProUGUI RoundLabel;

    public void SetRound(PubSubListenerEvent e)
    {
        var round = (int)e.value;

        RoundLabel.text = $"Round {round}";
    }

    public void SetMana(PubSubListenerEvent e)
    {
        var mana = (int)e.value;

        UpdateIconList(ManaIcons, ManaIconPrefab, mana);
    }

    public void SetActions(PubSubListenerEvent e)
    {
        var actions = (int)e.value;

        UpdateIconList(ActionIcons, ActionIconPrefab, actions);
    }

    public void UpdateIconList(GameObject parentGO, GameObject iconPrefab, int iconQuantity)
    {
        // Destroy existing icons if needed
        for (var i = iconQuantity; i < parentGO.transform.childCount; i++)
        {
            var existingChild = parentGO.transform.GetChild(i);
            existingChild.SetParent(null);
            Destroy(existingChild.gameObject);
        }

        // Create new icons if needed
        for (var i = parentGO.transform.childCount; i < iconQuantity; i++)
        {
            Instantiate(iconPrefab, parentGO.transform);
        }
    }
}
