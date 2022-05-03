using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;


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

[RequireComponent(typeof(ChampionStats))]
public class ChampionStats : MonoBehaviour
{
    public CharacterController characterController { get; private set; }

    [Foldout("Resources")]
    public StatValue<PoolValueFloat> health;
    [Foldout("Resources")]
    public StatValue<PoolValueFloat> mana;
    [Foldout("Resources")]
    public StatValue<PoolValueFloat> stamina;
    [Foldout("Resources")]
    public StatValue<PoolValueFloat> rage;

    [Foldout("Currencies")]
    public StatValue<PoolValueInt> experience;
    [Foldout("Currencies")]
    public StatValue<int> gold;

    [Foldout("Combat")]
    public StatValue<float> range;
    [Foldout("Combat")]
    public StatValue<float> size;

    [Foldout("Critical")]
    public StatValue<float> critChance;
    [Foldout("Critical")]
    public StatValue<float> critDamage;
    [Foldout("Critical")]
    public StatValue<float> critResistence;

    [Foldout("Physical")]
    public StatValue<float> attackDamage;
    [Foldout("Physical")]
    public StatValue<float> armorPenetration;
    [Foldout("Physical")]
    public StatValue<float> armor;
    [Foldout("Physical")]
    public StatValue<float> attackSpeed;

    [Foldout("Magical")]
    public StatValue<float> abilityPower;
    [Foldout("Magical")]
    public StatValue<float> abilityHaste;

    [Foldout("Regeneration")]
    public StatValue<float> lifeSteal;
    [Foldout("Regeneration")]
    public StatValue<float> healPower;
    [Foldout("Regeneration")]
    public StatValue<float> healthRegeneration;
    [Foldout("Regeneration")]
    public StatValue<float> manaRegeneration;

    [Foldout("Movement")]
    public StatValue<float> movementSpeed;
    [Foldout("Movement")]
    public StatValue<float> tenacity;
    [Foldout("Movement")]
    public StatValue<float> slowResistence;

    private void Start()
    {
        characterController = GetComponent<CharacterController>();
    }

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

        float def = armor.value - instigator.armorPenetration.value;

        float phyDmgMult;
        if(def >= 0)
        {
            phyDmgMult = 100.0f / (100.0f + def);
        }
        else
        {
            phyDmgMult = 2.0f - (100.0f / (100.0f - def));
        }


        float magDmgMult;
        if (def >= 0)
        {
            magDmgMult = 100.0f / (100.0f + def);
        }
        else
        {
            magDmgMult = 2.0f - (100.0f / (100.0f - def));
        }


        dmg += damageInfo.physicalDamage * phyDmgMult + damageInfo.magicalDamage * magDmgMult;

        if(damageInfo.critical)
        {
            float critDmgMult = Mathf.Clamp(1 - critResistence.value * 0.01f, 0, 1);
            dmg *=  1 + ((instigator.critDamage.value - 100.0f)*0.01f) * critDmgMult;
        }

        health.AddModifier(0, new DamageModifier(instigator, dmg));
        Debug.Log("Damage: " + dmg + " -> " + damageInfo);
    }
}
