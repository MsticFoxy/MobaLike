using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Flash : Ability
{
    public float dashDistance = 2;
    public LayerMask layerMaskGround;
    private Vector3 destination;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void AbilityButtonDown()
    {
        base.AbilityButtonDown();
        

        destination = characterController.transform.position;
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit, 100, layerMaskGround))
        {
            destination = hit.point;
        }

        if (NavMesh.SamplePosition(characterController.position +
            Vector3.ClampMagnitude(destination - characterController.position, dashDistance),
            out NavMeshHit navHit, 100, characterController.agent.areaMask))
        {
            destination = navHit.position;
        }
        if (characterController.attackTarget == null)
        {
            characterController.SetDestination(destination);
        }

        characterController.agent.Warp(destination);
    }
}
