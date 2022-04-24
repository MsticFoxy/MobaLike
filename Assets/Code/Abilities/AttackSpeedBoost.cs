using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackSpeedBoost : Ability
{
    public float attackSpeedMultiplyer = 2;
    public float duration = 2;
    public int maxAttacks = 3;
    int currentAttack = 0;
    StatModifier<float> mod;

    // Start is called before the first frame update
    void Start()
    {
        mod = new StatModifier<float>((val) => { return val * attackSpeedMultiplyer; });
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator StatModification()
    {
        characterController.stats.attackSpeed.AddModifier(0, mod);
        yield return new WaitForSeconds(duration);
        characterController.stats.attackSpeed.RemoveModifier(mod);
        characterController.OnAttackFired -= CountAttack;
    }

    public override void AbilityButtonDown()
    {
        base.AbilityButtonDown();
        currentAttack = 0;
        characterController.OnAttackFired += CountAttack;
        characterController.ResetAttack();
        StartCoroutine(StatModification());
    }

    public void CountAttack(int id)
    {
        currentAttack++;
        if(currentAttack == maxAttacks)
        {
            characterController.stats.attackSpeed.RemoveModifier(mod);
            characterController.OnAttackFired -= CountAttack;
        }
    }
}
