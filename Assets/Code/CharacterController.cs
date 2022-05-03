using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using MyBox;


[Serializable]
public struct AttackTriggerInfo
{
    public float attackTime;
    public float executionTime;
}

[Serializable]
public struct AttackInfo
{
    public AttackTriggerInfo critTriggerTime;
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

public enum Team
{
    Player,
    Neutral,
    Enemy
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

    

    public bool inAttack { get; private set; }
    private float attackCooldown = 0;

    ChampionStats IInteractable.stats => stats;

    private int attackIndex;
    public Action<int, bool> OnAttackExecuted;
    public Action<int> OnAttackFired;

    [Foldout("Interaction")]
    public AttackInfo attackInfo;
    [Foldout("Interaction")]
    public List<GameObject> ignoreInteractObjects;
    [Foldout("Interaction")]
    public Team team;


    private bool restartAttackIfInRange;
    [Foldout("Attack Behavior")]
    public StatValue<bool> canAttack;
    private bool previousCanAttackState;
    [Foldout("Attack Behavior")]
    public StatValue<bool> canManualMove;
    private Vector3 pendingMovePosition;
    private bool previousCanManualMoveState;
    [Foldout("Attack Behavior")]
    public StatValue<bool> rotateTowardsDestination;

    [Foldout("Attack Information")]
    public GameObject attackGameObject;
    [Foldout("Attack Information")]
    public Transform attackSpawnLocation;
    [Foldout("Attack Information")]
    public bool canDieAnywhere = true;
    [Foldout("Attack Information")]
    public bool canRotateAfterAttackStarted;
    private Vector3 attackLocationIfCanNotRotate;


    [Foldout("Character State")]
    public StatValue<bool> untargetable;
    [Foldout("Character State")]
    public float afterDeathDestructionDelay = 5;

    private List<Coroutine> attackCoroutines = new List<Coroutine>();

    [Foldout("Abilities")]
    public Ability PassiveAbility;
    [Foldout("Abilities")]
    public Ability QAbility;
    [Foldout("Abilities")]
    public Ability WAbility;
    [Foldout("Abilities")]
    public Ability EAbility;
    [Foldout("Abilities")]
    public Ability RAbility;
    [Foldout("Abilities")]
    public Ability DUtilityAbility;
    [Foldout("Abilities")]
    public Ability FUtilityAbility;

    public Action<AbilitySlot> OnAbilityChanged;
    public Action OnDied;

    public bool dead { get; private set; }
    private int collidingCharacters = 0;
    public bool interactable => !untargetable.value;

    

    #region CombatEvents
    /// <summary>
    /// ChampionStats -> the stats that the OnHit is applied to.
    /// </summary>
    public Action<ChampionStats> OnHit;

    /// <summary>
    /// ChampionStats -> the stats that the auto attack hit.
    /// </summary>
    public Action<ChampionStats> OnAutoAttackHit;
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        stats = GetComponent<ChampionStats>();

        agent.updateRotation = false;
        NavMesh.avoidancePredictionTime = 0.5f;
        NavMesh.pathfindingIterationsPerFrame = 100;

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

        if (canDieAnywhere)
        {
            stats.health.OnStatChanged += () =>
            {
                if (stats.health.value.current <= 0)
                {
                    Die();
                }
            };
        }

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
                    if (canRotateAfterAttackStarted)
                    {
                        //Is Attacking
                        if (rotateTowardsDestination.value)
                        {
                            //if (attackCooldown <= 0)
                            //{
                            agent.transform.rotation = Quaternion.RotateTowards(agent.transform.rotation,
                                    Quaternion.LookRotation(attackTarget.position - agent.transform.position),
                                    agent.angularSpeed * Time.deltaTime);
                            //}
                        }
                        
                    }
                    else
                    {
                        agent.transform.rotation = Quaternion.RotateTowards(agent.transform.rotation,
                                    Quaternion.LookRotation(attackLocationIfCanNotRotate - agent.transform.position),
                                    agent.angularSpeed * Time.deltaTime);
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

    public void OnTriggerEnter(Collider other)
    {
        IInteractable interactable = other.GetComponent<IInteractable>();
        if(interactable != null && interactable.interactable)
        {
            if (!dead)
            {
                collidingCharacters++;
                //agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
            }
        }
    }

    public void OnTriggerExit(Collider other)
    {
        IInteractable interactable = other.GetComponent<IInteractable>();
        if (interactable != null)
        {
            collidingCharacters--;
            if (collidingCharacters <= 0)
            {
                //agent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
            }
        }
    }

    public void HandleAbilityInput()
    {
        if (isControlledLocally)
        {
            if (QAbility != null)
            {
                if (QAbility.IsCastable())
                {
                    if (Input.GetKeyDown(KeyCode.Q))
                    {
                        QAbility.AbilityButtonDown();
                    }
                    else if (Input.GetKey(KeyCode.Q))
                    {
                        QAbility.AbilityButtonHold();
                    }
                    else if (Input.GetKeyUp(KeyCode.Q))
                    {
                        QAbility.AbilityButtonUp();
                    }
                }
            }

            if (WAbility != null)
            {
                if (WAbility.IsCastable())
                {
                    if (Input.GetKeyDown(KeyCode.W))
                    {
                        WAbility.AbilityButtonDown();
                    }
                    else if (Input.GetKey(KeyCode.W))
                    {
                        WAbility.AbilityButtonHold();
                    }
                    else if (Input.GetKeyUp(KeyCode.W))
                    {
                        WAbility.AbilityButtonUp();
                    }
                }
            }

            if (EAbility != null)
            {
                if (EAbility.IsCastable())
                {
                    if (Input.GetKeyDown(KeyCode.E))
                    {
                        EAbility.AbilityButtonDown();
                    }
                    else if (Input.GetKey(KeyCode.E))
                    {
                        EAbility.AbilityButtonHold();
                    }
                    else if (Input.GetKeyUp(KeyCode.E))
                    {
                        EAbility.AbilityButtonUp();
                    }
                }
            }
            if (RAbility != null)
            {
                if (RAbility.IsCastable())
                {
                    if (Input.GetKeyDown(KeyCode.R))
                    {
                        RAbility.AbilityButtonDown();
                    }
                    else if (Input.GetKey(KeyCode.R))
                    {
                        RAbility.AbilityButtonHold();
                    }
                    else if (Input.GetKeyUp(KeyCode.R))
                    {
                        RAbility.AbilityButtonUp();
                    }
                }
            }

            // Utility
            if (DUtilityAbility != null)
            {
                if (DUtilityAbility.IsCastable())
                {
                    if (Input.GetKeyDown(KeyCode.D))
                    {
                        DUtilityAbility.AbilityButtonDown();
                    }
                    else if (Input.GetKey(KeyCode.D))
                    {
                        DUtilityAbility.AbilityButtonHold();
                    }
                    else if (Input.GetKeyUp(KeyCode.D))
                    {
                        DUtilityAbility.AbilityButtonUp();
                    }
                }
            }

            if (FUtilityAbility != null)
            {
                if (FUtilityAbility.IsCastable())
                {
                    if (Input.GetKeyDown(KeyCode.F))
                    {
                        FUtilityAbility.AbilityButtonDown();
                    }
                    else if (Input.GetKey(KeyCode.F))
                    {
                        FUtilityAbility.AbilityButtonHold();
                    }
                    else if (Input.GetKeyUp(KeyCode.F))
                    {
                        FUtilityAbility.AbilityButtonUp();
                    }
                }
            }
        }
    }

    IEnumerator AttackExecution(float executionTime, int attackIndex, bool crit)
    {
        yield return new WaitForSeconds(executionTime);
        if(attackTarget != null)
        {
            if (attackTarget.stats != null)
            {
                if (!canRotateAfterAttackStarted)
                {
                    Attack(attackLocationIfCanNotRotate, attackIndex, crit);
                }
                else
                {
                    Attack(attackTarget.position, attackIndex, crit);
                }
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
                    bool isCritAttack = CheckKrit();

                    if(!canRotateAfterAttackStarted)
                    {
                        attackLocationIfCanNotRotate = attackTarget.position;
                    }

                    blockTargetFollow = true;

                    OnAttackExecuted.Invoke(attackIndex, isCritAttack);
                    if (!isCritAttack)
                    {
                        attackCoroutines.Add(StartCoroutine(AttackExecution(attackInfo.attackTriggerTimes[attackIndex].executionTime
                            / stats.attackSpeed.value, attackIndex, isCritAttack)));
                        yield return new WaitForSeconds(attackInfo.attackTriggerTimes[attackIndex].attackTime
                            / stats.attackSpeed.value);
                    }
                    else
                    {
                        attackCoroutines.Add(StartCoroutine(AttackExecution(attackInfo.critTriggerTime.executionTime
                            / stats.attackSpeed.value, attackIndex, isCritAttack)));
                        yield return new WaitForSeconds(attackInfo.critTriggerTime.attackTime
                            / stats.attackSpeed.value);
                    }
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

    public virtual bool CheckKrit()
    {
        if(UnityEngine.Random.Range(1,100) <= stats.critChance.value)
        {
            return true;
        }
        return false;
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

    public void Attack(Vector3 target, int attackIndex, bool crit)
    {
        Vector3 attackPos = (target - transform.position).normalized * 0.01f 
            * stats.range.value + transform.position;
        GameObject attackObject = Instantiate(attackGameObject);
        attackObject.transform.position = attackSpawnLocation.position;
        attackObject.GetComponent<RangedAttack>().Initialize(stats, attackPos,
            new DamageInfo(0, stats.attackDamage.value, 0, crit));
    }
    public void Attack(Vector3 target, int attackIndex, DamageInfo damageInfo)
    {
        Vector3 attackPos = (target - transform.position).normalized * 0.01f
            * stats.range.value + transform.position;
        GameObject attackObject = Instantiate(attackGameObject);
        attackObject.transform.position = attackSpawnLocation.position;
        attackObject.GetComponent<RangedAttack>().Initialize(stats,attackPos, damageInfo);
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

    public Ability GetAbilityBySlot(AbilitySlot slot)
    {
        switch(slot)
        {
            case AbilitySlot.Passive: return PassiveAbility;
            case AbilitySlot.Q: return QAbility;
            case AbilitySlot.W: return WAbility;
            case AbilitySlot.E: return EAbility;
            case AbilitySlot.R: return RAbility;
            case AbilitySlot.D: return DUtilityAbility;
            case AbilitySlot.F: return FUtilityAbility;
        }
        return null;
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
        if(GetComponent<CapsuleCollider>())
        {
            GetComponent<CapsuleCollider>().enabled = false;
        }
        untargetable.AddModifier(10000, new StatModifier<bool>((val) => { return true; }));
        StopAllCoroutines();
        dead = true;
        collidingCharacters = 0;
        agent.enabled = false;
        StartCoroutine(DeathSinkIntoGround());
    }

    IEnumerator DeathSinkIntoGround()
    {
        yield return new WaitForSeconds(afterDeathDestructionDelay);

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

    public bool IsInteractable(GameObject instigator)
    {
        return !ignoreInteractObjects.Contains(instigator) && interactable && canDieAnywhere && !untargetable.value;
    }
}
