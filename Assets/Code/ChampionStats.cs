using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct DamageInfo
{
    public float trueDamage;
    public float physicalDamage;
    public float magicalDamage;
    public bool critical;

    public DamageInfo(float trueDamage, float physicalDamage, float magicalDamage, bool critical)
    {
        this.trueDamage = trueDamage;
        this.physicalDamage = physicalDamage;
        this.magicalDamage = magicalDamage;
        this.critical = critical;
    }
}

public class ChampionStats : MonoBehaviour
{
    [Header("Resources")]
    public StatValue<PoolValueFloat> health;
    public StatValue<PoolValueFloat> mana;
    public StatValue<PoolValueFloat> stamina;
    public StatValue<PoolValueFloat> rage;

    [Header("Resources/Currencies")]
    public StatValue<PoolValueInt> experience;
    public StatValue<int> gold;

    [Header("Combat")]
    public StatValue<int> range;
    public StatValue<float> size;

    [Header("Physical")]
    public StatValue<float> attackDamage;
    public StatValue<float> armorPenetration;
    public StatValue<float> armor;
    public StatValue<float> attackSpeed;

    [Header("Magical")]
    public StatValue<float> abilityPower;
    public StatValue<float> magicPenetration;
    public StatValue<float> magicResistence;
    public StatValue<float> abilityHaste;

    [Header("Regeneration")]
    public StatValue<float> lifeSteal;
    public StatValue<float> healPower;
    public StatValue<float> healthRegeneration;
    public StatValue<float> manaRegeneration;

    [Header("Movenent")]
    public StatValue<int> movementSpeed;
    public StatValue<float> tenacity;
    public StatValue<float> slowResistence;

    public class DamageModifier : StatModifier<PoolValueFloat>
    {
        public float damage;
        public ChampionStats instigator;

        public DamageModifier(ChampionStats instigator, float damage)
        {
            this.damage = damage;
            this.instigator = instigator;
            modification = (val) => 
            {
                val.current -= damage;
                return val;
            };
        }
    }
    public void Damage(ChampionStats instigator, DamageInfo damageInfo)
    {
        float dmg = damageInfo.trueDamage;

        float ar = armor.value - instigator.armorPenetration.value;
        float mr = magicResistence.value - instigator.magicPenetration.value;

        float phyDmgMult = 1;
        if(ar >= 0)
        {
            phyDmgMult = 100.0f / (100.0f + ar);
        }
        else
        {
            phyDmgMult = 2.0f - (100.0f / (100.0f - ar));
        }


        float magDmgMult = 1;
        if (mr >= 0)
        {
            magDmgMult = 100.0f / (100.0f + mr);
        }
        else
        {
            magDmgMult = 2.0f - (100.0f / (100.0f - mr));
        }


        dmg += damageInfo.physicalDamage * phyDmgMult + damageInfo.magicalDamage * magDmgMult;

        health.AddModifier(0, new DamageModifier(instigator, dmg));
        Debug.Log("Damage: " + dmg + " -> " + damageInfo);
    }
}
