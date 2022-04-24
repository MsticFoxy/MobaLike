using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface AttackInstance
{
    public ChampionStats target { get;}
    public DamageInfo damageInfo { get;}
    public ChampionStats instigator { get;}
    public void Initialize(ChampionStats instigator, ChampionStats target, DamageInfo damageInfo);
}
