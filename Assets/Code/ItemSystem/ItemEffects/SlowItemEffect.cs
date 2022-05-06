using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using MyBox;

[Serializable]
[CreateAssetMenu(fileName = "SlowEffect", menuName = "Items/Create Item Effect/Slow Effect", order = 1)]
public class SlowItemEffect : ItemEffect
{
    [Foldout("Slow Amount")]
    public float baseSlowAmount;
    [Foldout("Slow Amount")]
    public List<ScaleInformation> slowAmountScaling;

    [Foldout("Slow Duration")]
    public float slowDuration;
    [Foldout("Slow Duration")]
    public List<ScaleInformation> slowDurationScaling;
}
