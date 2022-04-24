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

public enum AbilitySlot
{
    Passive,
    Q,
    W,
    E,
    R,
    D,
    F
}

[RequireComponent(typeof(NavMeshAgent), typeof(ChampionStats))]
public class CharacterController : MonoBehaviour, IInteractable
{
    public NavMeshAgent agent { get; private set; }
    public ChampionStats stats { get; private set; }

    public bool isControlledLocally { get; set; }

    public IInteractable attackTarget { get; private set; }
    public float radius => stats.size.value;
    public Vector3 position => transform.position;
    private bool blockTargetFollow;
    protected float maxAttackRange => stats.range.value * 0.01f;

    public AttackInfo attackInfo;

    public bool inAttack { get; private set; }
    private float attackCooldown = 0;

    ChampionStats IInteractable.stats => stats;

    private int attackIndex;
    public Action<int> OnAttackExecuted;
    public Action<int> OnAttackFired;

    private bool restartAttackIfInRange;
    public StatValue<bool> canAttack;
    private bool previousCanAttackState;
    public StatValue<bool> canManualMove;
    private Vector3 pendingMovePosition;
    private bool previousCanManualMoveState;
    public StatValue<bool> rotateTowardsDestination;
    public StatValue<bool> untargetable;

    private List<Coroutine> attackCoroutines = new List<Coroutine>();

    public Ability PassiveAbility;
    public Ability QAbility;
    public Ability WAbility;
    public Ability EAbility;
    public Ability RAbility;
    public Ability DUtilityAbility;
    public Ability FUtilityAbility;

    public Action<AbilitySlot> OnAbilityChanged;
    public Action OnDied;
    public float afterDeathDestructionDelay = 5;
    public bool dead { get; private set; }

    public bool interactable => !untargetable.value;

    [Header("Attack Information")]
    public GameObject attackGameObject;
    public Transform attackSpawnLocation;

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        stats = GetComponent<ChampionStats>();

        agent.updateRotation = false;
        

        stats.movementSpeed.OnStatChanged += () => {
            agent.speed = stats.movementSpeed.value * 0.01f; };
        stats.movementSpeed.OnStatChanged.Invoke();

        previousCanAttackState = canAttack.value;
        canAttack.OnStatChanged += () => 
        {
            if (canAttack.value != previousCanAttackState)
            {
                previousCanAttackState = canAttack.value;
                if(canAttack.value)
                {
                    restartAttackIfInRange = true;
                }
                else
                {
                    StopAttackCoroutines();
                }
            }
        };

        canManualMove.OnStatChanged += () =>
        {
            if (canManualMove.value != previousCanManualMoveState)
            {
                previousCanManualMoveState = canManualMove.value;
                if (canManualMove.value)
                {
                    agent.destination = pendingMovePosition;
                }
            }
        };

        stats.health.OnStatChanged += () => 
        {
            if(stats.health.value.current <= 0)
            {
                Die();
            }
        };

        SetAbility(AbilitySlot.Passive, PassiveAbility);
        SetAbility(AbilitySlot.Q, QAbility);
        SetAbility(AbilitySlot.W, WAbility);
        SetAbility(AbilitySlot.E, EAbility);
        SetAbility(AbilitySlot.R, RAbility);
        SetAbility(AbilitySlot.D, DUtilityAbility);
        SetAbility(AbilitySlot.F, FUtilityAbility);

    }

    // Update is called once per frame
    void Update()
    {
        if (!dead)
        {
            if (attackCooldown > 0)
            {
                attackCooldown -= Time.deltaTime;
            }

            HandleAbilityInput();

            if (attackTarget != null && canAttack.value)
            {
                if ((attackTarget.position - transform.position).magnitude <= attackTarget.radius + maxAttackRange)
                {
                    //Is Attacking
                    if (rotateTowardsDestination.value)
                    {
                        if (attackCooldown <= 0)
                        {
                            agent.transform.rotation = Quaternion.RotateTowards(agent.transform.rotation,
                                    Quaternion.LookRotation(attackTarget.position - agent.transform.position),
                                    agent.angularSpeed * Time.deltaTime);
                        }
                    }
                    agent.destination = transform.position;
                    if (restartAttackIfInRange)
                    {
                        attackCoroutines.Add(StartCoroutine(AttackCoroutine()));
                        restartAttackIfInRange = false;
                    }
                }
                else if (!blockTargetFollow)
                {
                    // follows target to Attack
                    agent.destination = attackTarget.position;
                    attackIndex = 0;
                }
            }
        }
    }

    public void HandleAbilityInput()
    {
        if (isControlledLocally)
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                if (QAbility != null)
                {
                    QAbility.AbilityButtonDown();
                }
            }
            else if (Input.GetKey(KeyCode.Q))
            {
                if (QAbility != null)
                {
                    QAbility.AbilityButtonHold();
                }
            }
            else if (Input.GetKeyUp(KeyCode.Q))
            {
                if (QAbility != null)
                {
                    QAbility.AbilityButtonUp();
                }
            }

            if (Input.GetKeyDown(KeyCode.W))
            {
                if (WAbility != null)
                {
                    WAbility.AbilityButtonDown();
                }
            }
            else if (Input.GetKey(KeyCode.W))
            {
                if (WAbility != null)
                {
                    WAbility.AbilityButtonHold();
                }
            }
            else if (Input.GetKeyUp(KeyCode.W))
            {
                if (WAbility != null)
                {
                    WAbility.AbilityButtonUp();
                }
            }

            if (Input.GetKeyDown(KeyCode.E))
            {
                if (EAbility != null)
                {
                    EAbility.AbilityButtonDown();
                }
            }
            else if (Input.GetKey(KeyCode.E))
            {
                if (EAbility != null)
                {
                    EAbility.AbilityButtonHold();
                }
            }
            else if (Input.GetKeyUp(KeyCode.E))
            {
                if (EAbility != null)
                {
                    EAbility.AbilityButtonUp();
                }
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                if (RAbility != null)
                {
                    RAbility.AbilityButtonDown();
                }
            }
            else if (Input.GetKey(KeyCode.R))
            {
                if (RAbility != null)
                {
                    RAbility.AbilityButtonHold();
                }
            }
            else if (Input.GetKeyUp(KeyCode.R))
            {
                if (RAbility != null)
                {
                    RAbility.AbilityButtonUp();
                }
            }

            // Utility
            if (Input.GetKeyDown(KeyCode.D))
            {
                if (DUtilityAbility != null)
                {
                    DUtilityAbility.AbilityButtonDown();
                }
            }
            else if (Input.GetKey(KeyCode.D))
            {
                if (DUtilityAbility != null)
                {
                    DUtilityAbility.AbilityButtonHold();
                }
            }
            else if (Input.GetKeyUp(KeyCode.D))
            {
                if (DUtilityAbility != null)
                {
                    DUtilityAbility.AbilityButtonUp();
                }
            }


            if (Input.GetKeyDown(KeyCode.F))
            {
                if (FUtilityAbility != null)
                {
                    FUtilityAbility.AbilityButtonDown();
                }
            }
            else if (Input.GetKey(KeyCode.F))
            {
                if (FUtilityAbility != null)
                {
                    FUtilityAbility.AbilityButtonHold();
                }
            }
            else if (Input.GetKeyUp(KeyCode.F))
            {
                if (FUtilityAbility != null)
                {
                    FUtilityAbility.AbilityButtonUp();
                }
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
                if (OnAttackFired != null)
                {
                    OnAttackFired.Invoke(attackIndex);
                }
                attackCooldown = attackInfo.attackTriggerTimes[attackIndex].attackTime - executionTime;
            }
        }
        
    }

    IEnumerator AttackCoroutine()
    {

        if (OnAttackExecuted != null)
        {
            yield return new WaitUntil( () => 
            {
                return (attackTarget.position - transform.position).magnitude <= attackTarget.radius + maxAttackRange;
            });

            yield return new WaitUntil( () =>
            {
                return attackCooldown <= 0;
            });

            inAttack = true;
            while (inAttack)
            {
                if (attackIndex < attackInfo.attackTriggerTimes.Count)
                {
                    blockTargetFollow = true;
                    OnAttackExecuted.Invoke(attackIndex);
                    attackCoroutines.Add(StartCoroutine(AttackExecution(attackInfo.attackTriggerTimes[attackIndex].executionTime 
                        / stats.attackSpeed.value, attackIndex)));
                    yield return new WaitForSeconds(attackInfo.attackTriggerTimes[attackIndex].attackTime 
                        / stats.attackSpeed.value);
                    if ((attackTarget.position - transform.position).magnitude >= attackTarget.radius + maxAttackRange)
                    {
                        blockTargetFollow = false;
                        inAttack = false;
                        restartAttackIfInRange = true;
                        StopAttackCoroutines();
                        Debug.Log("OutOfRange");
                    }
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
                if (rotateTowardsDestination.value)
                {
                    agent.transform.rotation = Quaternion.RotateTowards(agent.transform.rotation,
                        Quaternion.LookRotation(agent.velocity.normalized),
                        agent.angularSpeed * Time.deltaTime);
                }
            }

        }
    }

    public void SetAttackTarget(IInteractable target)
    {
        if (attackTarget != null)
        {
            if (attackTarget is CharacterController)
            {
                CharacterController charCont = attackTarget as CharacterController;
                charCont.untargetable.OnStatChanged -= AttackTargetUntargetabilityChanged;
            }
        }
        if (target != null && target != attackTarget)
        {

            if (target is CharacterController)
            {
                CharacterController charCont = target as CharacterController;
                if(charCont.untargetable.value)
                {
                    return;
                }
            }

            StopAttackCoroutines();
            SetDestination(target.position, target.radius + stats.range.value * 0.01f);
            attackTarget = target;
            
            if(attackTarget is CharacterController)
            {
                CharacterController charCont = attackTarget as CharacterController;
                charCont.untargetable.OnStatChanged += AttackTargetUntargetabilityChanged;
            }

            // Make Char attack target
            //inAttack = true;
            if (canAttack.value)
            {
                attackCoroutines.Add(StartCoroutine(AttackCoroutine()));
            }
        }
        if(target == null)
        {
            attackTarget = null;
            StopAllCoroutines();
            SetDestination(transform.position);
        }
    }

    public void StopAttack()
    {
        StopAttackCoroutines();
        SetAttackTarget(null);
        attackIndex = 0;
        inAttack=false;
        restartAttackIfInRange = false;
        blockTargetFollow = false;
    }

    public void ResetAttack()
    {
        attackCooldown = 0;
        int attackCo = attackCoroutines.Count;
        StopAttackCoroutines();
        if (attackTarget != null && attackCo > 0 && attackTarget.interactable)
        {
            attackCoroutines.Add(StartCoroutine(AttackCoroutine()));
        }
    }

    public void StopAttackCoroutines()
    {
        foreach(Coroutine c in attackCoroutines)
        {
            StopCoroutine(c);
        }
        attackCoroutines.Clear();
        inAttack=false;
    }

    public Vector3 SetDestination(Vector3 position, float stoppingDistance = 0)
    {
        if (agent != null)
        {
            agent.stoppingDistance = stoppingDistance;
            if (NavMesh.SamplePosition(position, out NavMeshHit hit, 100, agent.areaMask))
            {
                if (canManualMove.value)
                {
                    agent.destination = hit.position;
                }
                pendingMovePosition = hit.position;

                return hit.position;
            }
        }
        return position;
    }

    public void ForceDestination(Vector3 position, bool keepOldDestination = true)
    {
        if (agent != null)
        {
            agent.stoppingDistance = 0;
            if (NavMesh.SamplePosition(position, out NavMeshHit hit, 100, agent.areaMask))
            {
                if (keepOldDestination)
                {
                    pendingMovePosition = agent.destination;
                }
                agent.destination = hit.position;
            }
        }
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
        GameObject attackObject = Instantiate(attackGameObject);
        attackObject.transform.position = attackSpawnLocation.position;
        attackObject.GetComponent<RangedAttack>().Initialize(stats, target,
            new DamageInfo(0, stats.attackDamage.value, 0, false));
    }
    public void Attack(ChampionStats target, int attackIndex, DamageInfo damageInfo)
    {
        GameObject attackObject = Instantiate(attackGameObject);
        attackObject.transform.position = attackSpawnLocation.position;
        attackObject.GetComponent<RangedAttack>().Initialize(stats,target, damageInfo);
    }

    public void SetAbility(AbilitySlot slot, Ability ability)
    {
        switch (slot)
        {
            case AbilitySlot.Passive:
                RemoveAbility(PassiveAbility);
                PassiveAbility = null;
                break;
            case AbilitySlot.Q:
                RemoveAbility(QAbility);
                QAbility = null;
                break;
            case AbilitySlot.W:
                RemoveAbility(WAbility);
                WAbility = null;
                break;
            case AbilitySlot.E:
                RemoveAbility(EAbility);
                EAbility = null;
                break;
            case AbilitySlot.R:
                RemoveAbility(RAbility);
                RAbility = null;
                break;
        }
        if (ability != null)
        {
            ability.Attach(slot, this);
            if (OnAbilityChanged != null)
            {
                OnAbilityChanged.Invoke(slot);
            }
        }
    }

    public void RemoveAbility(Ability ability)
    {
        if(ability != null)
        {
            ability.RemoveFromParent();
        }
    }

    public void AttackTargetUntargetabilityChanged()
    {
        if (attackTarget != null)
        {
            if (attackTarget is CharacterController)
            {
                CharacterController charCont = attackTarget as CharacterController;
                if(charCont.untargetable.value)
                {
                    SetAttackTarget(null);
                }
            }
        }
    }

    public void Die()
    {
        if(OnDied != null)
        {
            OnDied.Invoke();
        }
        untargetable.AddModifier(10000, new StatModifier<bool>((val) => { return true; }));
        StopAllCoroutines();
        dead = true;
        StartCoroutine(DeathSinkIntoGround());
    }

    IEnumerator DeathSinkIntoGround()
    {
        yield return new WaitForSeconds(afterDeathDestructionDelay);

        Debug.Log("Sink");
        float sunken = 0;
        agent.updatePosition = false;
        while (sunken < 1)
        {
            yield return new WaitForFixedUpdate();
            transform.position -= Vector3.up * Time.deltaTime;
            sunken += Time.deltaTime;
        }
        Destroy(gameObject);
    }
}
