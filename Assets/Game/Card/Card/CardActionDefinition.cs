using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum CardActionKind
{
    Attack,
    Build,
    Modifiy,
    Other
}

public enum CardActionTarget
{
    None,
    EmptyBuildableArea,
    GameState
}


[CreateAssetMenu(fileName = "CardActionDefinition", menuName = "Card/Action", order = 1)]
public class CardActionDefinition : ScriptableObject
{

    public string Name = "Action";
    public string Description = "Description of the action.";
    public string FlavorText = "\"Flavor text for the action.\"";

    public Sprite Icon = null;
    public Sprite PlacementIcon = null;
    public Color Color = Color.blue;

    public CardActionKind Kind = CardActionKind.Other;
    public CardActionTarget Target = CardActionTarget.None;
    public CardType Type = CardType.Generic;
    public List<CardActionBehavior> Behaviors;

    [Range(0, 5)]
    public int ActionPointCost = 1;

    [Range(0, 50)]
    public int PurchaseCost = 0;
}