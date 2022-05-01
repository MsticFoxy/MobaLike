using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInteractable
{
    public float radius{ get;}
    public Vector3 position { get;}
    public ChampionStats stats { get;}
    public bool interactable { get;}
    public void Interact(GameObject instigator);

    public bool IsInteractable(GameObject instigator);
}
