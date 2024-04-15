using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CardsConfig", menuName = "Card/CardsConfig", order = 3)]
public class CardsConfig : ScriptableObject
{
    public List<CardConfig> CardConfigs;

    public CardConfig GetConfigForCardType(CardType type)
    {
        foreach (var config in CardConfigs)
        {
            if (config.Type == type)
            {
                return config;
            }
        }
        return null;
    }
}
