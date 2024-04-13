using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ActionDefinition", menuName = "Action/Definition", order = 1)]
public class ActionDefinition : MonoBehaviour
{
    public string Name = "Action";

    public List<ActionDefinition> Behaviors;

    [Range(1, 50)]
    public int BaseDamage = 1;

    [Range(1.0f, 10.0f)]
    public float BaseProjectileSpeed = 1; // units per second

    [Range(0.05f, 10.0f)]
    public float BaseAttackCooldown = 0.5f; // seconds

    [Range(1.0f, 25.0f)]
    public float BaseRange = 5.0f; // units
}