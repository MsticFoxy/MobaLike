using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target;

    private void Update()
    {
        if(target)
        {
            if(Input.GetKey(KeyCode.Space))
            {
                transform.position = target.position;
            }
        }
    }
}
