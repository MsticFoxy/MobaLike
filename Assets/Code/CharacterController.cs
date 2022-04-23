using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


[Serializable]
public struct AttackTriggerInfo
{
    public float attackTime;
    public float executionTime;
}

[Serializable]
public struct AttackInfo
{
    public List<AttackTriggerInfo> attackTriggerTimes;
}

[RequireComponent(typeof(NavMeshAgent), typeof(ChampionStats))]
public class CharacterController : MonoBehaviour, IInteractable
{
    private NavMeshAgent agent;
    private ChampionStats stats;

    private IInteractable attackTarget;
    public float radius => stats.size.value;
    public Vector3 position => transform.position;
    private bool blockTargetFollow;
    protected float maxAttackRange => stats.range.value * 0.01f;

    public AttackInfo attackInfo;

    public bool inAttack { get; private set; }

    ChampionStats IInteractable.stats => stats;

    private int attackIndex;
    public Action<int> OnAttackExecuted;

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        stats = GetComponent<ChampionStats>();

        agent.updateRotation = false;

        stats.movementSpeed.OnStatChanged += () => { agent.speed = stats.movementSpeed.value * 0.01f; };
        stats.movementSpeed.OnStatChanged.Invoke();
    }

    // Update is called once per frame
    void Update()
    {
        if(attackTarget != null)
        {
            if ((attackTarget.position - transform.position).magnitude <= attackTarget.radius + maxAttackRange)
            {
                //Is Attacking
                agent.transform.rotation = Quaternion.RotateTowards(agent.transform.rotation,
                        Quaternion.LookRotation(attackTarget.position - agent.transform.position),
                        agent.angularSpeed * Time.deltaTime);
                
            }
            else if(!blockTargetFollow)
            {
                // follows target to Attack
                agent.destination = attackTarget.position;
                attackIndex = 0;
            }
        }
    }

    IEnumerator AttackExecution(float executionTime, int attackIndex)
    {
        yield return new WaitForSeconds(executionTime);
        if(attackTarget != null)
        {
            if (attackTarget.stats != null)
            {
                Attack(attackTarget.stats, attackIndex);
            }
        }
        
    }

    IEnumerator AttackCoroutine()
    {
        inAttack = true;
        if (OnAttackExecuted != null)
        {
            yield return new WaitUntil( () => 
            {
                return (attackTarget.position - transform.position).magnitude <= attackTarget.radius + maxAttackRange;
            });
            while (inAttack)
            {
                if (attackIndex < attackInfo.attackTriggerTimes.Count)
                {
                    OnAttackExecuted.Invoke(attackIndex);
                    StartCoroutine(AttackExecution(attackInfo.attackTriggerTimes[attackIndex].executionTime 
                        / stats.attackSpeed.value, attackIndex));
                    yield return new WaitForSeconds(attackInfo.attackTriggerTimes[attackIndex].attackTime 
                        / stats.attackSpeed.value);

                    attackIndex++;
                    if (attackInfo.attackTriggerTimes.Count <= attackIndex)
                    {
                        attackIndex = 0;
                    }
                }
            }
        }
        yield return null;
    }

    private void LateUpdate()
    {
        if (agent != null)
        {
            if (agent.velocity.sqrMagnitude > Mathf.Epsilon)
            {
                agent.transform.rotation = Quaternion.RotateTowards(agent.transform.rotation,
                    Quaternion.LookRotation(agent.velocity.normalized),
                    agent.angularSpeed * Time.deltaTime);
            }

        }
    }

    public void SetAttackTarget(IInteractable target)
    {
        if (target != null && target != attackTarget)
        {
            SetDestination(target.position, target.radius + stats.range.value * 0.01f);
            attackTarget = target;
            // Make Char attack target
            inAttack = true;
            StartCoroutine(AttackCoroutine());
        }
    }

    public void StopAttack()
    {
        StopAllCoroutines();
        attackTarget = null;
        attackIndex = 0;
        inAttack=false;
    }

    public Vector3 SetDestination(Vector3 position, float stoppingDistance = 0)
    {
        if (agent != null)
        {
            agent.stoppingDistance = stoppingDistance;
            if (NavMesh.SamplePosition(position, out NavMeshHit hit, 100, agent.areaMask))
            {
                agent.destination = hit.position;
                return hit.position;
            }
        }
        return position;
    }

    public void Interact(GameObject instigator)
    {
        if(instigator != null)
        {
            CharacterController controller = instigator.GetComponent<CharacterController>();
            if (controller != null)
            {
                controller.SetAttackTarget(this);
            }
        }
    }

    public void Attack(ChampionStats target, int attackIndex)
    {
        target.Damage(stats, new DamageInfo(0, stats.attackDamage.value, 0, false));
    }
}
