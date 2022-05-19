using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Immobalize", menuName = "Effects/Immobalize", order = 0)]
public class EImmboalize : StatusEffect
{
    public float duration;
    public float tenacityInfluence = 1.0f;

    private StatModifier<float> rootMod;
    protected ChampionStats stats;
    protected List<Coroutine> coroutines = new List<Coroutine>();
    protected float stunTime;
    protected float stunDuration
    {
        get
        {
            return duration / (1.0f + ((stats.tenacity.value * tenacityInfluence) / 100.0f));
        }
    }
    protected float deltaStunTime { get
        {
            return stunTime / stunDuration;
        } }
    public override void Begin()
    {
        stats = owner.GetComponent<ChampionStats>();
        base.Begin();
        rootMod = new StatModifier<float>((val) =>
        {
            return 0;
        });
        stats.movementSpeed.AddModifier(EffectPriority.Last, rootMod);
        coroutines.Add(stats.StartCoroutine(WaitToRemove()));
        stunTime = 0.0f;
        SpawnVisualEffect();
    }

    IEnumerator WaitToRemove()
    {
        yield return WaitFunction();
        owner.RemoveStatusEffect(this);
    }

    protected virtual IEnumerator WaitFunction()
    {
        yield return new WaitForSeconds(duration / stunDuration);
    }

    public override void End()
    {
        base.End();
        StopRelatedCoroutines();
        Debug.Log("Remove Root");
        stats.movementSpeed.RemoveModifier(rootMod);
        RemoveVisualEffect();
    }

    public override void Tick()
    {
        base.Tick();
        stunTime += Time.deltaTime;
    }

    protected void StopRelatedCoroutines()
    {
        foreach(Coroutine c in coroutines)
        {
            stats.StopCoroutine(c);
        }
        coroutines.Clear();
    }
}
