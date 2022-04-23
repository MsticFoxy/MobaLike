using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class WalkingTarget : MonoBehaviour
{
    public LayerMask layerMaskGround;
    public LayerMask layerMaskInteractable;
    public CharacterController controller;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButtonDown(1))
        {
            if (Camera.main != null)
            {
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                LayerMask saveLayer = controller.gameObject.layer;
                controller.gameObject.layer = 0;
                if (Physics.SphereCast(ray, 0.25f, out hit, 100, layerMaskInteractable))
                {
                    IInteractable interactable = hit.collider.GetComponent<IInteractable>();
                    if(interactable != null)
                    {
                        interactable.Interact(controller.gameObject);
                    }
                }
                else if (Physics.Raycast(ray, out hit, 100, layerMaskGround))
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
    }

    

    private void OnTargetPositionChanged(Vector3 position)
    {
        transform.position = controller.SetDestination(position);
    }
}
