using System;
using System.Collections;
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
}