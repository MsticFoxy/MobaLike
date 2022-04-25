using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangedAttack : MonoBehaviour, AttackInstance
{
    private ChampionStats _target;
    private DamageInfo _damageInfo;
    private ChampionStats _instigator;
    public float flyingSpeed = 2;

    public float hitDistance = 0.01f;

    public ChampionStats target => _target;

    public DamageInfo damageInfo => _damageInfo;

    public ChampionStats instigator => _instigator;

    public void Initialize(ChampionStats instigator, ChampionStats target, DamageInfo damageInfo)
    {
        _instigator = instigator;
        _target = target;
        _damageInfo = damageInfo;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if((target.transform.position + Vector3.up - transform.position).magnitude > hitDistance)
        {
            transform.rotation = Quaternion.LookRotation(target.transform.position + Vector3.up - transform.position);
            transform.position = Vector3.MoveTowards(transform.position, 
                target.transform.position + Vector3.up, flyingSpeed * Time.deltaTime);
        }
        else
        {
            target.Damage(instigator, damageInfo);
            Destroy(gameObject);
        }
    }
}
