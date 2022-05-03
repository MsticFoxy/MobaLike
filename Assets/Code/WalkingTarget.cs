using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class WalkingTarget : MonoBehaviour
{
    public LayerMask layerMaskGround;
    public LayerMask layerMaskInteractable;
    public CharacterController controller;
    public CharacterController attackTarget;

    // Start is called before the first frame update
    void Start()
    {
        controller.isControlledLocally = true;
        attackTarget.GetComponent<NavMeshAgent>().updatePosition = false;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButton(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 100, layerMaskGround))
            {
                hit.point = new Vector3(hit.point.x, controller.position.y, hit.point.z);
                attackTarget.transform.position = (hit.point - controller.position).normalized 
                    * controller.stats.range.value * 0.005f + controller.position;
            }
        }
        if(Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 100, layerMaskGround))
            {
                hit.point = new Vector3(hit.point.x, controller.position.y, hit.point.z);
                attackTarget.transform.position = (hit.point - controller.position).normalized
                    * controller.stats.range.value * 0.005f + controller.position;
            }
            attackTarget.Interact(controller.gameObject);
        }
        else if(Input.GetMouseButtonDown(1))
        {
            if (Camera.main != null)
            {
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                LayerMask saveLayer = controller.gameObject.layer;
                controller.gameObject.layer = 0;
                /*if (Physics.SphereCast(ray, 0.25f, out hit, 100, layerMaskInteractable))
                {
                    IInteractable interactable = hit.collider.GetComponent<IInteractable>();
                    if(interactable != null)
                    {
                        if (interactable.interactable)
                        {
                            interactable.Interact(controller.gameObject);
                        }
                        else
                        {
                            if (Physics.Raycast(ray, out hit, 100, layerMaskGround))
                            {
                                controller.StopAttack();
                                OnTargetPositionChanged(hit.point);
                            }
                        }
                    }
                }
                else*/ if (Physics.Raycast(ray, out hit, 100, layerMaskGround))
                {
                    controller.StopAttack();
                    OnTargetPositionChanged(hit.point);
                }
                controller.gameObject.layer = saveLayer;
            }
            else
            {
                Debug.LogWarning("Current Camera is Null");
            }
        }

        if ((attackTarget.position - controller.position).magnitude >= controller.stats.range.value * 0.009f)
        {
            attackTarget.transform.position = (attackTarget.position
                - controller.position).normalized * 0.009f * controller.stats.range.value 
                + controller.position;
        }
    }

    

    private void OnTargetPositionChanged(Vector3 position)
    {
        transform.position = controller.SetDestination(position);
    }
}
