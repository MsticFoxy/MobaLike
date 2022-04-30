using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangedAttack : MonoBehaviour, AttackInstance
{
    private Vector3 _target;
    private DamageInfo _damageInfo;
    private ChampionStats _instigator;
    public float flyingSpeed = 2;
    public float radius = 0.25f;
    public LayerMask hitMask;

    private float hitDistance = 0.01f;

    public Vector3 target => _target;

    public DamageInfo damageInfo => _damageInfo;

    public ChampionStats instigator => _instigator;

    public void Initialize(ChampionStats instigator, Vector3 target, DamageInfo damageInfo)
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
        if((target + Vector3.up - transform.position).magnitude > hitDistance)
        {
            Vector3 start = transform.position;
            transform.rotation = Quaternion.LookRotation(target + Vector3.up - transform.position);
            transform.position = Vector3.MoveTowards(transform.position, 
                target + Vector3.up, flyingSpeed * Time.deltaTime);
            Vector3 end = transform.position;

            if(Physics.SphereCast(start,radius, end - start, out RaycastHit hit, (end - start).magnitude, hitMask))
            {
                ChampionStats stats = hit.collider.GetComponent<ChampionStats>();
                if(stats != null)
                {
                    stats.Damage(instigator, damageInfo);
                    Destroy(gameObject);
                }
            }

        }
        else
        {
            Destroy(gameObject);
        }
    }
}
