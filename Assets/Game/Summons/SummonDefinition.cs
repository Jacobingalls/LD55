using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SummonDefinition", menuName = "Summon/Definition", order = 1)]
public class SummonDefinition : ScriptableObject
{
    public enum TargetPriority
    {
        First,
        Closest,
        Strongest,
        Fastest
    }

    public string Name = "Summon";
    public string Description = "Description of the unit.";
    public string FlavorText = "\"Flavor text for the unit.\"";
    public Sprite Icon;
    public Color Color = Color.blue;

    public TargetPriority Priority = TargetPriority.First;
}
