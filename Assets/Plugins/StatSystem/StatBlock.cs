using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;



public class StatBlock : MonoBehaviour
{
    /// <summary>
    /// Determines if the component is initialized.
    /// </summary>
    private bool initialized;

    public Action<StatBase> OnStatAdded;

    private Dictionary<string, StatBase> stats = new Dictionary<string, StatBase>();
    public Dictionary<int, List<StatusEffect>> effects = new Dictionary<int, List<StatusEffect>>();

    // Start is called before the first frame update
    void Start()
    {
        Initialize();
    }

    // Update is called once per frame
    void Update()
    {
        TickEffects();
    }

    protected void TickEffects()
    {
        foreach (KeyValuePair<int, List<StatusEffect>> entry in effects)
        {
            foreach (StatusEffect effect in entry.Value)
            {
                effect.Tick();
            }
        }
    }

    /// <summary>
    /// Initializes the statblock by writing the stat properties into the stats dictionary.
    /// </summary>
    public void Initialize()
    {
        stats.Clear();
        initialized = true;
        foreach (Component c in gameObject.GetComponents(typeof(Component)))
        {
            foreach (var prop in c.GetType().GetFields())
            {
                try
                {
                    if (prop.GetValue(c) is StatBase)
                    {
                        if (!stats.ContainsKey(prop.Name))
                        {
                            AddStat(prop.Name, (StatBase)prop.GetValue(c));
                        }
                        else
                        {
                            Debug.LogWarning("The stat name " + prop.Name + " from component " + c.GetType() 
                                + " of gameobject " + gameObject.name + " has duplicate names colliding with another component. " +
                                "The statblock will only contain the first stat with this name.");
                        }
                    }
                }
                catch (Exception e) { Console.WriteLine(e.ToString()); }
            }
        }
    }

    /// <summary>
    /// Adds the given stat to this statblock if its name is unique in this statblock.
    /// </summary>
    /// <param name="name">The name of the Stat.</param>
    /// <param name="stat">The Stat thats is going to be added.</param>
    /// <returns>Returns false if there is already a Stat with this name.</returns>
    public bool AddStat(string name, StatBase stat)
    {
        if (!initialized)
        {
            Initialize();
        }

        if (stat != null)
        {
            if (!stats.ContainsKey(name))
            {
                stat.owner = this;
                stats.Add(name, stat);
                if(stat.OnAddedToStatBlock != null)
                {
                    stat.OnAddedToStatBlock.Invoke();
                }
                if (OnStatAdded != null)
                {
                    OnStatAdded.Invoke(stat);
                }
                return true;
            }
        }
        return false;
    }


    /// <summary>
    /// Checks if the given name matches a stat of this statblock.
    /// </summary>
    /// <typeparam name="T">The type that the stat type has to match.</typeparam>
    /// <param name="name">The name of the stat.</param>
    /// <returns>Returns if this statblock contains a stat with the given name and type.</returns>
    public bool ContainsStat<T>(string name) where T : StatBase
    {
        if (!initialized)
        {
            Initialize();
        }
        StatBase stat;
        if (stats.TryGetValue(name, out stat))
        {
            if (stat is T)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Gets the stat with the given name if the types match.
    /// </summary>
    /// <typeparam name="T">The type that the stat type has to match.</typeparam>
    /// <param name="name">The name of the stat.</param>
    /// <returns>Return the stat or a default value of the type if there is no matching stat.</returns>
    public T GetStat<T>(string name) where T : StatBase
    {
        if (!initialized)
        {
            Initialize();
        }

        StatBase stat;
        if(stats.TryGetValue(name, out stat))
        {
            if(stat is T)
            {
                return (T)stat;
            }
        }
        return default;
    }

    /// <summary>
    /// Gets the type of the given stat.
    /// </summary>
    /// <param name="name">The name of the stat</param>
    /// <returns>Returns the type of the stat and null if there is no matching stat.</returns>
    public Type GetTypeOfStat(string name)
    {
        if (!initialized)
        {
            Initialize();
        }

        StatBase stat;
        if (stats.TryGetValue(name, out stat))
        {
            return stat.GetType();
        }
        return null;
    }


    /// <summary>
    /// Adds the given effect to this statblock and calls its Begin function.
    /// </summary>
    /// <param name="priority">The priority with wich the effects Tick function is called.</param>
    /// <param name="effect">The effect that is added to this statblock.</param>
    public void AddStatusEffect(int priority, StatusEffect effect)
    {
        if (!initialized)
        {
            Initialize();
        }
        if (effect != null)
        {
            foreach (KeyValuePair<int, List<StatusEffect>> entry in effects)
            {
                foreach(StatusEffect e in entry.Value)
                {
                    if(e.BlockStatusEffectOnAdditionToOwner(effect))
                    {
                        return;
                    }
                }
            }
            if (effect.AttachToStatBlock(this))
            {
                if (effects.ContainsKey(priority))
                {
                    List<StatusEffect> prioMods;
                    effects.TryGetValue(priority, out prioMods);
                    prioMods.Add(effect);
                }
                else
                {
                    List<StatusEffect> newPrio = new List<StatusEffect>();
                    newPrio.Add(effect);
                    effects.Add(priority, newPrio);
                }
                effect.Begin();
            }
        }
    }

    /// <summary>
    /// removes the given effect from this statblock and calls its End function.
    /// </summary>
    /// <param name="effect">The effect that will be removed.</param>
    public void RemoveStatusEffect(StatusEffect effect)
    {
        if (!initialized)
        {
            Initialize();
        }
        if (effect != null)
        {
            if (effect.RemoveFromOwner(this))
            {
                foreach (KeyValuePair<int, List<StatusEffect>> entry in effects)
                {
                    Debug.Log(entry.Value);
                    if (entry.Value.Contains(effect))
                    {
                        entry.Value.Remove(effect);
                        if (entry.Value.Count == 0)
                        {
                            effects.Remove(entry.Key);
                            effect.End();
                            return;
                        }
                    }
                }
            }
        }
    }
}


