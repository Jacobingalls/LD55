using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "UnitDefinition", menuName = "Unit/Definition", order = 1)]
public class UnitDefinition : ScriptableObject
{
    public string Name = "Unit";
    public string ClassName = "";
    public string Description = "Description of the unit.";
    public string FlavorText = "\"Flavor text for the unit.\"";
    public Sprite Icon;
    public Color Color = Color.blue;

    [Range(1, 100)]
    public int BaseMaxHealth = 10;

    [Range(0.1f, 10.0f)]
    public float MoveSpeed = 5.0f;
}
