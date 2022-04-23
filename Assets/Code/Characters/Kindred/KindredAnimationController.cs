using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Animator))]
public class KindredAnimationController : MonoBehaviour
{
    public Animator animator;
    public NavMeshAgent agent;
    public ChampionStats stats;
    public CharacterController characterController;

    private bool attackAnimation = false;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        characterController.OnAttackExecuted += (id) =>
        {
            animator.SetFloat("walkingSpeed", 0);
            animator.SetInteger("attackIndex", id);
            animator.SetBool("attacking", characterController.inAttack);
            animator.SetFloat("attackSpeed", stats.attackSpeed.value);
            animator.SetTrigger("triggerAttack");
        };
    }

    // Update is called once per frame
    void Update()
    {
        if (agent != null)
        {
            if (!characterController.inAttack)
            {
                animator.SetFloat("walkingSpeed", agent.velocity.magnitude * 100.0f);
            }
            animator.SetFloat("walkSpeedMulti", agent.velocity.magnitude * 0.25f);
            animator.SetFloat("healthPercent", (stats.health.value.current / stats.health.value.max)*100);
        }
    }
}
