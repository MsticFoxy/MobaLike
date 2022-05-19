using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;




public class StatModifier<T>
{
    protected Func<T, T> modification;

    /// <summary>
    /// Creates a modifier with the given modification.
    /// </summary>
    /// <param name="modification">The modification that gets the value previous to this 
    /// modification and returns the modified value.</param>
    public StatModifier(Func<T, T> modification)
    {
        this.modification = modification;
    }

    protected StatModifier()
    {

    }

    /// <summary>
    /// Applies the modifier to the given stat.
    /// </summary>
    /// <param name="stat">The stat that gets modified.</param>
    /// <returns>Returns the modified stat.</returns>
    public T Apply(T stat)
    {
        return modification(stat);
    }
}

[Serializable]
public class StatValue<T> : StatBase
{
    #region Stat Value Properties
    [SerializeField]
    protected T _baseValue;
    public T baseValue
    {
        get
        {
            return _baseValue;
        }
        private set
        {
            _baseValue = value;
            Invalidate();
        }
    }

    protected T _value;
    public T value
    {
        get
        {
            if (_isDirty)
            {
                _value = CalculateValue();
            }
            return _value;
        }
        private set
        {
            _value = value;
        }
    }

    protected Dictionary<int, List<StatModifier<T>>> modifiers = new Dictionary<int, List<StatModifier<T>>>();
    #endregion

    #region Stat State Properties
    private bool _isDirty;
    #endregion

    public StatValue()
    {
        _isDirty = true;
    }


    /// <summary>
    /// Adds a modifier to this statvalue wich will be applied with the given priority.
    /// </summary>
    /// <param name="priority">The priority with wich the modifier will be applied.</param>
    /// <param name="modifier">The modifier that will be applied to this stat.</param>
    public void AddModifier(EffectPriority priority, StatModifier<T> modifier)
    {
        AddModifier((int)priority, modifier);
    }

    /// <summary>
    /// Adds a modifier to this statvalue wich will be applied with the given priority.
    /// </summary>
    /// <param name="priority">The priority with wich the modifier will be applied.</param>
    /// <param name="modifier">The modifier that will be applied to this stat.</param>
    public void AddModifier(int priority, StatModifier<T> modifier)
    {
        if (modifier != null)
        {
            if (modifiers.ContainsKey(priority))
            {
                List<StatModifier<T>> prioMods;
                modifiers.TryGetValue(priority, out prioMods);
                prioMods.Add(modifier);
            }
            else
            {
                List<StatModifier<T>> newPrio = new List<StatModifier<T>>();
                newPrio.Add(modifier);
                modifiers.Add(priority, newPrio);
            }
            Invalidate();
        }
    }

    /// <summary>
    /// Removes the given modifier from this stat.
    /// </summary>
    /// <param name="modifier">The modifier that will be removed</param>
    public void RemoveModifier(StatModifier<T> modifier)
    {
        if (modifier != null)
        {
            foreach (KeyValuePair<int, List<StatModifier<T>>> entry in modifiers)
            {
                if (entry.Value.Contains(modifier))
                {
                    entry.Value.Remove(modifier);
                    if (entry.Value.Count == 0)
                    {
                        modifiers.Remove(entry.Key);
                        Invalidate();
                        return;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Marks the stat as dirty so it has to recalculate the next time it is read.
    /// </summary>
    public void Invalidate()
    {
        _isDirty = true;
        if(OnStatChanged != null)
        {
            OnStatChanged.Invoke();
        }
    }

    /// <summary>
    /// Calculates the stat and applies all modifiers.
    /// </summary>
    /// <returns>Returns the modified stat.</returns>
    protected virtual T CalculateValue()
    {
        T valueCopy = _baseValue;
        if (typeof(ICloneable).IsAssignableFrom(typeof(T)))
        {
            valueCopy = (T)((ICloneable)_baseValue).Clone();
        }

        foreach (KeyValuePair<int, List<StatModifier<T>>> entry in modifiers)
        {
            if(entry.Value != null)
            {
                foreach(StatModifier<T> mod in entry.Value)
                {
                    valueCopy = mod.Apply(valueCopy);
                }
            }
        }

        return valueCopy;
    }
}


[Serializable]
public class CombineStat<T> : StatValue<T>
{

    private List<StatBase> stats = new List<StatBase>();
    private Func<StatBlock, T, T> combinationFunction;

    public CombineStat(Func<StatBlock, T, T> combinationRule) : base()
    {
        combinationFunction = combinationRule;
        OnAddedToStatBlock += () =>
        {
            owner.OnStatAdded += AddCombinationStat;
        };
    }

    private void AddCombinationStat(StatBase stat)
    {
        if(stat != null)
        {
            stats.Add(stat);
            stat.OnStatChanged += Invalidate;
            Invalidate();
        }
    }

    /// <summary>
    /// Calculates the stat and applies all modifiers.
    /// </summary>
    /// <returns>Returns the modified stat.</returns>
    protected override T CalculateValue()
    {
        T valueCopy = _baseValue;
        if (typeof(ICloneable).IsAssignableFrom(typeof(T)))
        {
            valueCopy = (T)((ICloneable)_baseValue).Clone();
        }
        valueCopy = combinationFunction.Invoke(owner, valueCopy);
        foreach (KeyValuePair<int, List<StatModifier<T>>> entry in modifiers)
        {
            if (entry.Value != null)
            {
                foreach (StatModifier<T> mod in entry.Value)
                {
                    valueCopy = mod.Apply(valueCopy);
                }
            }
        }

        return valueCopy;
    }
}
