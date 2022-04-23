using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class DashShot : Ability
{
    public float dashDistance = 2;
    public float dashTime = 0.4f;
    public LayerMask layerMaskGround;
    private bool inDash;
    private Vector3 destination;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(inDash)
        {
            if(characterController != null)
            {
                characterController.agent.updatePosition = false;
                characterController.transform.position = Vector3.MoveTowards(characterController.transform.position, 
                    destination, dashDistance * Time.deltaTime / dashTime);
                if((characterController.transform.position - destination).magnitude < 0.1f)
                {
                    characterController.agent.updatePosition = true;
                    characterController.agent.Warp(destination);
                    characterController.agent.destination = destination;
                    inDash = false;
                }
            }
        }
    }

    IEnumerator statModification()
    {
        inDash = true;
        StatModifier<bool> mod = new StatModifier<bool>((val) => { return false; });
        characterController.canAttack.AddModifier(0, mod);
        characterController.canManualMove.AddModifier(0, mod);
        characterController.rotateTowardsDestination.AddModifier(0, mod);
        yield return new WaitForSeconds(0.6f);
        characterController.canAttack.RemoveModifier(mod);
        characterController.canManualMove.RemoveModifier(mod);
        characterController.rotateTowardsDestination.RemoveModifier(mod);
    }

    public override void AbilityButtonDown()
    {
        base.AbilityButtonDown();
        characterController.StopAttackCoroutines();

        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit, 100, layerMaskGround))
        {
            destination = hit.point;
        }

        if(NavMesh.SamplePosition( characterController.position + 
            Vector3.ClampMagnitude(destination - characterController.position, dashDistance),
            out NavMeshHit navHit, 100, characterController.agent.areaMask))
        {
            destination = navHit.position;
        }
        if (characterController.attackTarget == null)
        {
            characterController.SetDestination(destination);
        }
        
        Debug.DrawLine(destination, destination + Vector3.up, Color.red, 10);

        StartCoroutine(statModification());
    }

    protected override void RemovedFromParent()
    {
        base.RemovedFromParent();
        inDash = false;
    }

}
