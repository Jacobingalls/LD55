using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Targetable : MonoBehaviour, IExplode
{
    public enum TargetType
    {
        Unit,
        Summon,
        Location
    }

    public void Explode() => Destroy(gameObject);
}