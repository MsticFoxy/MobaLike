using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using MyBox;

public enum SpawnRule
{
    Everywhere,
    NearPlayer,
    AtSpawnPoints
}

[Serializable]
public class SpawnInfo
{
    public GameObject spawnObject;
    public SpawnRule rule;
    public float probability;
    [ReadOnly]
    public Vector2 probabilityRange;
}

[CreateAssetMenu(fileName = "Wave", menuName = "Game Mode/Wave", order = 0)]
public class WaveInfo : ScriptableObject
{
    public List<SpawnInfo> spawnables = new List<SpawnInfo>();


    private void OnValidate()
    {
        float range = 0;
        foreach (SpawnInfo s in spawnables)
        {
            float start = range;
            range += s.probability;
            s.probabilityRange = new Vector2(start, range);
        }
    }
}
