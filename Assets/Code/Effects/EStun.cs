using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Stun", menuName = "Effects/Stun", order = 0)]
public class EStun : EImmboalize
{
    private StatModifier<bool> stunMod;
    public override void Begin()
    {
        coroutines = new List<Coroutine>();
        stunMod = new StatModifier<bool>((val) =>
        {
            return true;
        });
        base.Begin();
        stats.isStunned.AddModifier(EffectPriority.Last, stunMod);
    }

    protected override IEnumerator WaitFunction()
    {
        yield return new WaitForSeconds(duration / (1.0f + ((stats.tenacity.value * tenacityInfluence) / 100.0f)));
    }

    public override void End()
    {
        base.End();
        Debug.Log("Remove Stun");
        stats.isStunned.RemoveModifier(stunMod);
    }
}
