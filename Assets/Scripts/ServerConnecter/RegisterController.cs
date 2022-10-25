using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// 
/// </summary>
public class RegisterController : MonoBehaviour
{
    private void Start()
    {
        NetManager.AddMsgListener("MsgRegister", OnMsgRegister);
    }
    public void Register(string account,string pw)
    {
        MsgRegister msg = new MsgRegister();
        msg.account = account;
        msg.pw = pw;
        NetManager.Send(msg);
    }
    void OnMsgRegister(MsgBase msg)
    {
        MsgRegister tmpMsg = (MsgRegister)(msg);
        int result = tmpMsg.result;
        GameObject.FindGameObjectWithTag("TextInfo").GetComponent<TextMeshProUGUI>().text = result == 0 ? "sign in successfully" : " sign in faild";
    }
}
