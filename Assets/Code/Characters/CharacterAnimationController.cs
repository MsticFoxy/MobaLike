using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;

[RequireComponent(typeof(Animator))]
public class CharacterAnimationController : MonoBehaviour
{
    public Animator animator { get; private set; }
    [Foldout("References")]
    public UnityEngine.AI.NavMeshAgent agent;
    [Foldout("References")]
    public ChampionStats stats;
    [Foldout("References")]
    public CharacterController characterController;

    public AnimatorOverrideController overrideAnimationController;
    public LayerMask layerMaskGround;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        animator.runtimeAnimatorController = overrideAnimationController;

        characterController.OnAttackExecuted += (id, crit) =>
        {
            animator.SetFloat("walkingSpeed", 0);
            animator.SetInteger("attackIndex", id);
            animator.SetBool("attacking", characterController.inAttack);
            animator.SetFloat("attackSpeed", stats.attackSpeed.value);
            animator.SetTrigger("triggerAttack");
            if (crit)
            {
                animator.SetTrigger("critical");
            }
        };
        characterController.OnAbilityChanged += (slot) => {
            if (slot == AbilitySlot.Q)
            {
                characterController.QAbility.AbilityDown += () =>
                {
                    if (characterController != null)
                    {
                        Vector3 targetPoint = characterController.transform.forward;
                        RaycastHit hit;
                        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                        if (Physics.Raycast(ray, out hit, 100, layerMaskGround))
                        {
                            targetPoint = hit.point - characterController.transform.position;
                        }

                        float angle = Vector3.SignedAngle(characterController.transform.forward, targetPoint, Vector3.up);
                        if (Mathf.Abs(angle) < 45)
                        {
                            animator.SetTrigger("Q_Forward");
                        }
                        else if (Mathf.Abs(angle) < 135)
                        {
                            StopAllCoroutines();

                            if (angle < 0)
                            {
                                StartCoroutine(rotate(-90, 0.64f));
                                animator.SetTrigger("Q_Right");
                            }
                            else
                            {
                                StartCoroutine(rotate(90, 0.64f));
                                animator.SetTrigger("Q_Left");
                            }
                        }
                        else
                        {
                            animator.SetTrigger("Q_Backward");
                        }
                    }
                };
            }
        };

        characterController.OnDied += () =>
        {
            animator.SetTrigger("Die");
        };
    }

    IEnumerator rotate(float angle, float time)
    {
        transform.localRotation = Quaternion.Euler(0, angle, 0);
        yield return new WaitForSeconds(time);
        transform.localRotation = new Quaternion();
    }

    float prevVel;
    // Update is called once per frame
    void Update()
    {
        if (agent != null)
        {
            if (!characterController.inAttack)
            {
                animator.SetFloat("walkingSpeed", Mathf.Max(agent.velocity.magnitude, prevVel) * 100.0f);
            }
            animator.SetFloat("walkSpeedMulti", Mathf.Max(agent.velocity.magnitude, prevVel) * 0.25f);
            prevVel = agent.velocity.magnitude;
            animator.SetFloat("healthPercent", (stats.health.value.current / stats.health.value.max) * 100);
        }
    }
}
