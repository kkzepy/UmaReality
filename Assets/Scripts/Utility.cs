using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class Utility
{
    public static byte[] HexStringToBytes(string hex)
    {
        int len = hex.Length;
        byte[] result = new byte[len / 2];
        for (int i = 0; i < len; i += 2)
            result[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
        return result;
    }

    public static Dictionary<string, T> MergeDictionaries<T>(Dictionary<string, T> dict1, Dictionary<string, T> dict2)
    {
        Dictionary<string, T> mergedDict = new Dictionary<string, T>(dict1);
        foreach (var kvp in dict2)
        {
            if (!mergedDict.ContainsKey(kvp.Key))
            {
                mergedDict.Add(kvp.Key, kvp.Value);
            }
        }
        return mergedDict;
    }
}