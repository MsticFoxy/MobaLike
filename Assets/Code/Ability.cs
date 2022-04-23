using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum AbilityType
{
    Passive,
    Active
}

[RequireComponent(typeof(StatBlock))]
public class Ability : MonoBehaviour
{
    public AbilityType type;
    private float currentCooldown;
    public StatValue<float> cooldown;
    public CharacterController characterController;

    public Action AbilityDown;
    public Action AbilityHold;
    public Action AbilityUp;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (type == AbilityType.Passive)
        {
            AbilityButtonHold();
        }
    }

    public virtual void AbilityButtonDown()
    {
        if(AbilityDown != null)
        {
            AbilityDown.Invoke();
        }
    }

    public virtual void AbilityButtonHold()
    {
        if (AbilityHold != null)
        {
            AbilityHold.Invoke();
        }
    }

    public virtual void AbilityButtonUp()
    {
        if (AbilityUp != null)
        {
            AbilityUp.Invoke();
        }
    }

    public void Attach(AbilitySlot slot, CharacterController controller)
    {
        characterController = controller;
        switch (slot)
        {
            case AbilitySlot.Passive:
                characterController.PassiveAbility = this;
                break;
            case AbilitySlot.Q:
                characterController.QAbility = this;
                break;
            case AbilitySlot.W:
                characterController.WAbility = this;
                break;
            case AbilitySlot.E:
                characterController.EAbility = this;
                break;
            case AbilitySlot.R:
                characterController.RAbility = this;
                break;
        }

        if(type == AbilityType.Passive)
        {
            AbilityButtonDown();
        }
    }

    protected void AttachedToParent()
    {

    }

    public void RemoveFromParent()
    {
        characterController = null;
        RemovedFromParent();
    }

    protected virtual void RemovedFromParent()
    {

    }
}
