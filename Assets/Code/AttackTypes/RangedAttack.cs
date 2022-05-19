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

            foreach(RaycastHit hit in Physics.SphereCastAll(start,radius, end - start, (end - start).magnitude, hitMask))
            {
                if (hit.collider.gameObject != instigator.gameObject)
                {
                    ChampionStats stats = hit.collider.GetComponent<ChampionStats>();

                    if (stats != null && stats.characterController.team != instigator.characterController.team)
                    {
                        stats.Damage(instigator, damageInfo);
                        if (instigator.characterController.OnAutoAttackHit != null)
                        {
                            instigator.characterController.OnAutoAttackHit(stats);
                        }
                        if (instigator.characterController.OnHit != null)
                        {
                            instigator.characterController.OnHit.Invoke(stats);
                        }
                        Destroy(gameObject);
                        break;
                    }
                    
                }
            }

        }
        else
        {
            Destroy(gameObject);
        }
    }
}
