using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Knock", menuName = "Effects/Knock", order = 0)]
public class EKnock : EStun
{
    public Vector3 targetPosition;
    private Vector3 startPosition;
    private Transform visualKnockupTransform;
    private float visualYOffset;
    public override void Begin()
    {
        base.Begin();
        startPosition = owner.transform.position;
        visualKnockupTransform = owner.transform.GetChild(0);
        visualYOffset = visualKnockupTransform.localPosition.y;
    }

    public override void Tick()
    {
        base.Tick();
        float x = (deltaStunTime*2.0f - 1.0f);
        float yPos = (-(x*x)+1) * 2;
        owner.transform.position = new Vector3(Mathf.Lerp(startPosition.x, targetPosition.x, deltaStunTime)
            , startPosition.y, Mathf.Lerp(startPosition.z, targetPosition.z, deltaStunTime));
        visualKnockupTransform.localPosition = new Vector3(0, yPos * yPos + targetPosition.y + 
            visualYOffset * (1.0f- deltaStunTime), 0);
    }

    public override void End()
    {
        base.End();
        Debug.Log("Remove Knock");
        visualKnockupTransform.localPosition = Vector3.zero;
    }
}
