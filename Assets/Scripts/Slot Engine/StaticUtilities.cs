
using System;
using System.Collections;
using UnityEngine;
public static class StaticUtilities
{

    public static T[] RemoveAt<T>(this T[] source, int index)
    {
        T[] dest = new T[source.Length - 1];
        if (index > 0)
            Array.Copy(source, 0, dest, 0, index);

        if (index < source.Length - 1)
            Array.Copy(source, index + 1, dest, index, source.Length - index - 1);

        return dest;
    }
    public static T[] AddAt<T>(this T[] source, int index)
    {
        T[] dest = new T[source.Length + 1];
        if (index > 0)
            Array.Copy(source, 0, dest, 0, index);

        if (index < source.Length - 1)
            Array.Copy(source, index + 1, dest, index, source.Length + 1);

        return dest;
    }
    public static int findIndex<T>(this T[] array, T item)
    {
        return Array.IndexOf(array, item);
    }
}