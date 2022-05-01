using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using MyBox;

public enum AbilityType
{
    Passive,
    Active
}

[RequireComponent(typeof(StatBlock))]
public class Ability : MonoBehaviour
{
    public AbilityType type;

    [Foldout("Information")]
    public Sprite sprite;
    [Foldout("Information")]
    public string abilityName;
    [Foldout("Information")]
    public string description;

    public float currentCooldown { get; private set; }
    public bool inCast { get; private set; }
    public StatValue<float> cooldown;
    [HideInInspector]
    public CharacterController characterController;

    public Action AbilityDown;
    public Action AbilityHold;
    public Action AbilityUp;

    public void StartCast()
    {
        inCast = true;
    }

    public void StartCooldown()
    {
        inCast = false;
        currentCooldown = GetCooldown();
    }

    public float GetCooldown()
    {
        if(characterController != null)
        {
            return cooldown.value;
        }
        float cooldownMult = 1.0f + characterController.stats.abilityHaste.value * 0.01f;
        return cooldown.value * cooldownMult;
    }

    public virtual bool IsCastable()
    {
        if(currentCooldown <= 0 && !inCast)
        {
            return true;
        }
        return false;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        BaseUpdate();
    }

    protected virtual void BaseUpdate()
    {
        Debug.Log("update");
        if (!inCast)
        {
            if (currentCooldown > 0)
            {

                currentCooldown -= Time.deltaTime;
            }
        }
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
