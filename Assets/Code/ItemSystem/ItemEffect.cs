using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using MyBox;
using UnityEditor;
using System.Linq;

[Serializable]
public struct ScaleInformation
{
    public StatIndicator stat;
    [Tooltip("scaling in %")]
    public float scaling;
}

[Serializable]
// [CreateAssetMenu(fileName = "Item", menuName = "Items/Create Item Effect/", order = 1)]
public class ItemEffect : ScriptableObject
{
    public static bool itemEffecListIsDirty = true;
    private static List<ItemEffect> _globalItemEffecs;
    public static List<ItemEffect> globalItemEffecs
    {
        get
        {
            if (itemEffecListIsDirty)
            {
                _globalItemEffecs = GetListOfItemEffects();
            }
            return _globalItemEffecs;
        }
        private set
        {
            _globalItemEffecs = value;
        }
    }
    private static List<ItemEffect> GetListOfItemEffects()
    {
        List<ItemEffect> ret = new List<ItemEffect>();

        string[] guids = AssetDatabase.FindAssets("t:" + typeof(ItemEffect).Name);
        ItemEffect[] a = new ItemEffect[guids.Length];
        for (int i = 0; i < guids.Length; i++)         //probably could get optimized 
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            a[i] = AssetDatabase.LoadAssetAtPath<ItemEffect>(path);
        }
        return a.ToList();
    }


    [Foldout("Information")]
    public string effectName;
    [Foldout("Information")]
    public string description;

    [ReadOnly]
    [NonReorderable]
    [Foldout("Information")]
    public List<Item> effectOf;

    public ItemEffect()
    {
        #if UNITY_EDITOR
        itemEffecListIsDirty = true;
        #endif
    }
}
