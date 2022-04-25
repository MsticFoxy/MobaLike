using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class KindredAI : MonoBehaviour
{
    public CharacterController target { get; private set; }

    // Start is called before the first frame update
    void Start()
    {
        WalkingTarget wt = FindObjectOfType<WalkingTarget>();
        if(wt != null)
        {
            target = wt.controller;
        }

        if(target != null)
        {
            GetComponent<CharacterController>().SetAttackTarget(target);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
