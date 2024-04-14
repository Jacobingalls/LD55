using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DamageType
{
    Piercing,
    Crushing,
    Fire,
    Lightning,
}

public interface IDamageSource
{
    DamageType GetDamageType();
    int GetDamageAmount();
}

public interface IDamageable
{
    void Damage();
}