using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;

public class UIAbilityManager : MonoBehaviour
{
    public CharacterController characterController;

    [Foldout("Abilities")]
    public UIAbilitySlot PassiveAbility;
    [Foldout("Abilities")]
    public UIAbilitySlot QAbility;
    [Foldout("Abilities")]
    public UIAbilitySlot WAbility;
    [Foldout("Abilities")]
    public UIAbilitySlot EAbility;
    [Foldout("Abilities")]
    public UIAbilitySlot RAbility;
    [Foldout("Abilities")]
    public UIAbilitySlot DUtilityAbility;
    [Foldout("Abilities")]
    public UIAbilitySlot FUtilityAbility;

    // Start is called before the first frame update
    void Start()
    {
        characterController.OnAbilityChanged += (slot) =>
        {
            SetAbilitySlot(slot, characterController.GetAbilityBySlot(slot));
        };
    }

    public void SetAbilitySlot(AbilitySlot slot, Ability ability)
    {
        GetUIAbilitySlotBySlot(slot).SetAbility(ability);
    }

    public UIAbilitySlot GetUIAbilitySlotBySlot(AbilitySlot slot)
    {
        switch (slot)
        {
            case AbilitySlot.Passive: return PassiveAbility;
            case AbilitySlot.Q: return QAbility;
            case AbilitySlot.W: return WAbility;
            case AbilitySlot.E: return EAbility;
            case AbilitySlot.R: return RAbility;
            case AbilitySlot.D: return DUtilityAbility;
            case AbilitySlot.F: return FUtilityAbility;
        }
        return null;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
