using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
/// <summary>
/// 
/// </summary>
public class MsgBase
{
    public string protoName = "";//协议名
    public static byte[] Encode(MsgBase msg)
    {
        string s = JsonUtility.ToJson(msg);
        return System.Text.Encoding.UTF8.GetBytes(s);
    }
    public static MsgBase Decode(string protoName, byte[] bytes, int offset, int count)
    {
        string s = System.Text.Encoding.UTF8.GetString(bytes, offset, count);
        Debug.Log("Decode Msg As String:" + s);
        try
        {
            MsgBase msg = (MsgBase)JsonUtility.FromJson(s, Type.GetType(protoName));
            return msg;
        }
        catch (Exception e)
        {
            Debug.Log(e.ToString());
        }
        return null;
    }
    public static byte[] EncodeName(MsgBase msg)
    {
        byte[] nameBytes = System.Text.Encoding.UTF8.GetBytes(msg.protoName);
        Int16 nameBtyesLen = (Int16)nameBytes.Length;
        byte[] lenBytes = BitConverter.GetBytes(nameBtyesLen);
        byte[] bytes = lenBytes.Concat(nameBytes).ToArray();
        return bytes;
    }
    public static string DecodeName(byte[] bytes, int offset, out int count)
    {
        string protoName = "";
        count = 0;
        if (offset + 2 > bytes.Length)
        {
            return protoName;
        }
        Int16 len = BitConverter.ToInt16(bytes, offset);
        if (len < 0)
        {
            return protoName;
        }
        if (offset + 2 + len > bytes.Length)
        {
            return protoName;
        }
        count = 2 + len;
        protoName = System.Text.Encoding.UTF8.GetString(bytes, offset + 2, len);
        return protoName;
    }
}
