using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public Vector3 target;
    public float speed;
    public float radius;
    public LayerMask layerMask;
    public CharacterController owner { get; set; }

    public Action<CharacterController> AllyHit;
    public Action<CharacterController> EnemyHit;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        foreach(RaycastHit hit in Physics.SphereCastAll(transform.position, radius, target-transform.position, 
            speed * Time.deltaTime, layerMask))
        {
            if(hit.collider.gameObject != owner.gameObject)
            {
                if(hit.collider.GetComponent<CharacterController>() != null)
                {
                    CharacterController controller = hit.collider.GetComponent<CharacterController>();
                    if(controller.team == owner.team)
                    {
                        OnHitAlly(controller);
                        if(AllyHit != null)
                        {
                            AllyHit.Invoke(controller);
                        }
                    }
                    else
                    {
                        OnHitEnemy(controller);
                        if(EnemyHit != null)
                        {
                            EnemyHit.Invoke(controller);
                        }
                    }
                }
            }
        }
        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
        if ((transform.position - target).magnitude < 0.01f)
        {
            Destroy(gameObject);
        }
    }

    public virtual void OnHitAlly(CharacterController controller)
    {

    }

    public virtual void OnHitEnemy(CharacterController controller)
    {

    }
}
