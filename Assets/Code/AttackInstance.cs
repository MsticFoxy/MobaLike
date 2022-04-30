using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface AttackInstance
{
    public Vector3 target { get;}
    public DamageInfo damageInfo { get;}
    public ChampionStats instigator { get;}
    public void Initialize(ChampionStats instigator, Vector3 target, DamageInfo damageInfo);
}
