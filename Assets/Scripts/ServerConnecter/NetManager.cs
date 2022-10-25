using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System;
using System.Linq;
using UnityEngine.UI;
/// <summary>
/// 
/// </summary>
public class NetManager
{
    static Socket conn;
    public delegate void EventListener(string str);
    public delegate void MsgListener(MsgBase msg);
    static Dictionary<string, MsgListener> msgListeners = new Dictionary<string, MsgListener>();
    static Dictionary<eNetEvent, EventListener> eventListeners = new Dictionary<eNetEvent, EventListener>();
    static List<MsgBase> msgList = new List<MsgBase>();
    const int RECVBUFLEN = 1024;
    static byte[] recvBuf = new byte[RECVBUFLEN];
    static int buffCount = 0;
    public static bool updateScrollBar;
    public static string strShow = "等待连接服务器";
    public static string s;
    public static void AddMsgListener(string msgName, MsgListener listener)
    {
        msgListeners[msgName] = listener;
    }
    public static void AddEventListener(eNetEvent netEvent, EventListener listener)
    {
        if (eventListeners.ContainsKey(netEvent))
        {
            eventListeners[netEvent] += listener;
        }
        else
        {
            eventListeners[netEvent] = listener;
        }
    }
    public static void RemoveEventListener(eNetEvent netEvent, EventListener listener)
    {
        if (eventListeners.ContainsKey(netEvent))
        {
            eventListeners[netEvent] -= listener;
        }
        if (eventListeners[netEvent] == null)
        {
            eventListeners.Remove(netEvent);
        }
    }
    static void FireEvent(eNetEvent netEvent, string err)
    {
        if (eventListeners.ContainsKey(netEvent))
        {
            eventListeners[netEvent](err);
        }
    }
    public static void MsgUpdate()
    {
        if (msgList.Count <= 0)
        {
            return;
        }
        MsgBase msg = null;
        {
            msg = msgList[0];
            msgList.RemoveAt(0);
        }
        if (msgListeners.ContainsKey(msg.protoName))
        {
            if (msgListeners[msg.protoName] != null)
            {
                msgListeners[msg.protoName](msg);
            }
            else
            {
                Debug.LogError("MSG Listener:" + msg.protoName + "Is NULL");
            }
        }
    }
    public static void Connect(string ip, int port)
    {
        conn = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        IPAddress ipAdd = IPAddress.Parse(ip);
        IPEndPoint ipEP = new IPEndPoint(ipAdd, port);
        conn.BeginConnect(ipEP, OnConnectCallBack, conn);
    }
    static void OnConnectCallBack(IAsyncResult ar)
    {
        try
        {
            Socket socket = (Socket)ar.AsyncState;
            socket.EndConnect(ar);
            FireEvent(eNetEvent.ConnectSuccess, "成功连接服务器.....");
            socket.BeginReceive(recvBuf, buffCount, recvBuf.Length - buffCount, 0, OnReceiveCallBack, socket);
            //strShow = "成功连接服务器......";
        }
        catch
        {
            FireEvent(eNetEvent.ConnectFail, "连接服务器失败");
        }
    }
    static void OnReceiveCallBack(IAsyncResult ar)
    {
        try
        {
            Socket socket = (Socket)ar.AsyncState;
            int count = socket.EndReceive(ar);
            buffCount += count;
            MsgBase msg = OnReceiveData();
            while (msg != null)
            {
                msgList.Add(msg);
                msg = OnReceiveData();
            }
            socket.BeginReceive(recvBuf, buffCount, recvBuf.Length - buffCount, 0, OnReceiveCallBack, socket);
        }
        catch
        {
            FireEvent(eNetEvent.Close, "与服务器断开连接");
        }
    }
    static MsgBase OnReceiveData()
    {
        // Debug.Log("缓冲区数据长度：buffCount = " + buffCount);
        // Debug.Log("缓冲区数据 recvBuf = " + BitConverter.ToString(recvBuf));
        if (buffCount <= 2)
        {
            return null;
        }
        Int16 bodyLength = BitConverter.ToInt16(recvBuf, 0);
        if (buffCount < 2 + bodyLength)
        {
            return null;
        }
        s = System.Text.Encoding.UTF8.GetString(recvBuf, 4, bodyLength);
        Debug.Log("有效数据内容s=" + s);
        int nameCount = 0;
        string protoName = MsgBase.DecodeName(recvBuf, 2, out nameCount);
        if (protoName == "")
        {
            return null;
        }
        MsgBase msg = MsgBase.Decode(protoName, recvBuf, nameCount + 2, bodyLength - nameCount);
        if (msg != null)
        {
            Debug.Log("解析：" + protoName + "recvBytes:" + bodyLength + "nameLen:" + nameCount + "bodyLen:" + (bodyLength - nameCount));
        }
        // strShow = s;
        int start = 2 + bodyLength;
        int count = buffCount - start;
        Array.Copy(recvBuf, start, recvBuf, 0, count);
        buffCount -= start;
        // Debug.Log("当前缓冲区数据长度 buffCount = " + buffCount);
        // Debug.Log("当前缓冲区数据 recvBuf = " + BitConverter.ToString(recvBuf));
        return msg;
    }
    public static void Send(MsgBase msg)
    {
        byte[] nameBytes = MsgBase.EncodeName(msg);
        byte[] bodyBytes = MsgBase.Encode(msg);
        byte[] msgBytes = nameBytes.Concat(bodyBytes).ToArray();
        Int16 len = (Int16)(msgBytes.Length);
        byte[] lenBytes = BitConverter.GetBytes(len);
        byte[] sendBytes = lenBytes.Concat(msgBytes).ToArray();
        conn.BeginSend(sendBytes, 0, sendBytes.Length, 0, OnSendCallBack, conn);
    }
    public static void Send(string msg)
    {
        byte[] bodyBytes = System.Text.Encoding.UTF8.GetBytes(msg);
        Int16 len = (Int16)(bodyBytes.Length);
        byte[] lenBytes = BitConverter.GetBytes(len);
        byte[] sendBytes = lenBytes.Concat(bodyBytes).ToArray();
        conn.BeginSend(sendBytes, 0, sendBytes.Length, 0, OnSendCallBack, conn);
    }
    static void OnSendCallBack(IAsyncResult ar)
    {
        Socket socket = (Socket)ar.AsyncState;
        int count = socket.EndSend(ar);
        Debug.Log("异步发送数据长度:" + count);
    }
}
