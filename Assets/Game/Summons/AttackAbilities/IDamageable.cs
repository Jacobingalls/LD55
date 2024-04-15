using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DamageType
{
    Dog,
    Ink,
    Electricity,
    Explosive,
    Legal
}

public interface IDamageSource
{
    DamageType GetDamageType();
    int GetDamageAmount();
}

public interface IDamageable
{
    void Damage(IDamageSource damageSource);
}