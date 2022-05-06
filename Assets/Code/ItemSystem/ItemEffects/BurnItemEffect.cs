using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using MyBox;

[Serializable]
[CreateAssetMenu(fileName = "BurnEffect", menuName = "Items/Create Item Effect/Burn Effect", order = 1)]
public class BurnItemEffect : ItemEffect
{
    [Foldout("Burn Damage")]
    public float baseDamagePerSecond;
    [Foldout("Burn Damage")]
    public List<ScaleInformation> damagePerSecondScaling;

    [Foldout("Burn Duration")]
    public float burnDuration;
    [Foldout("Burn Duration")]
    public List<ScaleInformation> burnDurationScaling;
}
