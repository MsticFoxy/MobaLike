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

    public LayerMask layerMaskGround;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        characterController.OnAttackExecuted += (id, crit) =>
        {
            animator.SetFloat("walkingSpeed", 0);
            animator.SetInteger("attackIndex", id);
            animator.SetBool("attacking", characterController.inAttack);
            animator.SetFloat("attackSpeed", stats.attackSpeed.value);
            animator.SetTrigger("triggerAttack");
            if(crit)
            {
                animator.SetTrigger("critical");
            }
        };
        characterController.OnAbilityChanged += (slot) => {
            if (slot == AbilitySlot.Q)
            {
                characterController.QAbility.AbilityDown += () =>
                {
                    Vector3 targetPoint = characterController.transform.forward;
                    RaycastHit hit;
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    if (Physics.Raycast(ray, out hit, 100, layerMaskGround))
                    {
                        targetPoint = hit.point - characterController.transform.position;
                    }

                    float angle = Vector3.SignedAngle(characterController.transform.forward, targetPoint, Vector3.up);
                    if(Mathf.Abs(angle) < 45)
                    {
                        animator.SetTrigger("Q_Forward");
                    }
                    else if(Mathf.Abs(angle) < 135)
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
