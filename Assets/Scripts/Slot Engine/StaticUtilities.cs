
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public static class StaticUtilities
{
    [Serializable]
    public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        [SerializeField]
        private List<TKey> keys = new List<TKey>();

        [SerializeField]
        private List<TValue> values = new List<TValue>();

        // save the dictionary to lists
        public void OnBeforeSerialize()
        {
            keys.Clear();
            values.Clear();
            foreach (KeyValuePair<TKey, TValue> pair in this)
            {
                keys.Add(pair.Key);
                values.Add(pair.Value);
            }
        }

        // load dictionary from lists
        public void OnAfterDeserialize()
        {
            this.Clear();

            if (keys.Count != values.Count)
                throw new System.Exception(string.Format("there are {0} keys and {1} values after deserialization. Make sure that both key and value types are serializable."));

            for (int i = 0; i < keys.Count; i++)
                this.Add(keys[i], values[i]);
        }
    }
    /// <summary>
    /// Create a Gameobject with Components and return component from within components added
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="components_to_add">components to add to gameobject created</param>
    /// <param name="name">Name of the Gamobject created</param>
    /// <param name="parent">parent transform</param>
    /// <returns>any componented added to gameobject in components_to_add[]</returns>
    public static T CreateGameobject<T>(Type[] components_to_add,string name,Transform parent)
    {
        GameObject created_gameobject = new GameObject(name, components_to_add);
        T output_component = created_gameobject.GetComponent<T>();
        if (output_component == null)
            DebugLogWarning("Component returned from CreateGameObject was not in list of components_to_add");
        created_gameobject.transform.parent = parent;
        return output_component;
    }

    public static T[] RemoveAt<T>(this T[] source, int index)
    {
        T[] dest = new T[source.Length - 1];
        if (index > 0)
            Array.Copy(source, 0, dest, 0, index);

        if (index < source.Length - 1)
            Array.Copy(source, index + 1, dest, index, source.Length - index - 1);

        return dest;
    }

    public static string PrintElements<T>(this List<T> source)
    {
        return String.Join("|", source);
    }

    public static T Pop<T>(this List<T> source)
    {
        T output = source[0];
        source.RemoveAt(0);
        return output;
    }

    public static T[] AddAt<T>(this T[] source, int index, T value)
    {
        T[] dest = new T[source.Length + 1];
        //Copy the first part of the array until index to insert
        if (index > 0)
            Array.Copy(source, 0, dest, 0, index);
        
        //insert arr
        if (index < dest.Length)
        {
            dest[index] = value;
        }
        if (index < source.Length)
            Array.Copy(source, index, dest, index+1, source.Length - index);
        return dest;
    }


    public static int findIndex<T>(this T[] array, T item)
    {
        return Array.IndexOf(array, item);
    }
    public static void DebugLog(string message)
    {
#if DebugLogOn
        Debug.Log(message);
#endif
    }

    internal static void DebugLogWarning(string message)
    {
        Debug.LogWarning(message);
    }

    internal static void DebugLogError(string message)
    {
        Debug.LogError(message);
    }

}

public static class AnimatorStaticUtilites
{
    public static void SetTriggerTo(ref Animator animator, supported_triggers to_trigger)
    {
        animator.SetTrigger(to_trigger.ToString());
    }

    internal static void InitializeAnimator(ref Animator animator)
    {
        ResetAllBools(ref animator);
        ResetAllTriggers(ref animator);
    }

    internal static void ResetAllBools(ref Animator animator)
    {
        for (int bool_to_check = 0; bool_to_check < (int)supported_bools.End; bool_to_check++) //Don't change spin resolve yet. will need to reset on spin idleidle
        {
            Debug.Log(String.Format("Resetting bool {0}", ((supported_bools)bool_to_check).ToString()));
            SetBoolTo(ref animator, (supported_bools)bool_to_check, false);
        }
    }

    internal static void ResetAllTriggers(ref Animator animator)
    {
        for (int trigger_to_check = 0; trigger_to_check < (int)supported_triggers.End; trigger_to_check++) //Don't change spin resolve yet. will need to reset on spin idleidle
        {
            Debug.Log(String.Format("Resetting trigger {0}", ((supported_triggers)trigger_to_check).ToString()));
            ResetTrigger(ref animator, (supported_triggers)trigger_to_check);
        }
    }

    internal static void ResetTrigger(ref Animator animator, supported_triggers trigger)
    {
        animator.ResetTrigger(trigger.ToString());
    }

    internal static void SetBoolTo(ref Animator animator, supported_bools bool_name, bool value)
    {
        animator.SetBool(bool_name.ToString(), value);
    }

    internal static void SetFloatTo(ref Animator animator, supported_floats float_to_set, float value)
    {
        animator.SetFloat(float_to_set.ToString(),value);
    }
}