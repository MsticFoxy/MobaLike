using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusEffect
{
    public StatBlock owner { get; private set; }

    public virtual void Tick()
    {

    }

    public virtual void Begin()
    {

    }

    public virtual void End()
    {

    }

    /// <summary>
    /// Gets called when a new statuseffect is added to the owner. The addition can be blocked if the function returns true.
    /// Furthermore, additional operations can be done in this function like reacting to the given effect.
    /// </summary>
    /// <param name="effect">The status effect that is being added.</param>
    /// <returns>Returns if the addition of the given statuseffect should be stopped.</returns>
    public virtual bool BlockStatusEffectOnAdditionToOwner(StatusEffect effect)
    {
        return false;
    }

    /// <summary>
    /// Attaches this statuseffect to a statblock.
    /// </summary>
    /// <param name="owner">The statblock this statuseffect will be attached to.</param>
    /// <returns>Returns if the statuseffect could be attached.</returns>
    public bool AttachToStatBlock(StatBlock owner)
    {
        if(this.owner == null)
        {
            this.owner = owner;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Removes this statuseffect from its owner.
    /// </summary>
    /// <param name="owner">The statblock that has to match this statuseffects owner.</param>
    /// <returns>Returns if the statuseffect could be removed.</returns>
    public bool RemoveFromOwner(StatBlock owner)
    {
        if(this.owner != null && this.owner == owner)
        {
            this.owner = null;
            return true;
        }
        return false;
    }
}
