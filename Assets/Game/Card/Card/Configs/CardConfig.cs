using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CardType
{
    Generic,
    CardDraw,
    Mana,
    Dog,
    Electricity,
    Legal,
    Ink,
    Explosive
}

[CreateAssetMenu(fileName = "CardConfig", menuName = "Card/Config", order = 2)]
public class CardConfig : ScriptableObject
{
    public CardType Type;
    public Sprite TypeIcon;

    public Color HeaderBackgroundColor = Color.black;
    public Color ContentBackgroundColor = Color.gray;
    public Color OutlineColor = Color.black;

    public Color HeaderTextColor = Color.white;
    public Color DescriptionTextColor = Color.white;
}
