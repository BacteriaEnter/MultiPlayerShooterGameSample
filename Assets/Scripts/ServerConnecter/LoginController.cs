using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class LoginController : MonoBehaviour
{
    [SerializeField] GameObject connectPanel;
    [SerializeField] GameObject loginPanel;
    private void Start()
    {
        NetManager.AddMsgListener("MsgLogin", OnMsgLogin);
    }
    public void Login(string account,string pw)
    {
        MsgLogin msg = new MsgLogin();
        msg.account = account;
        msg.pw = pw;
        NetManager.Send(msg);
    }
    void OnMsgLogin(MsgBase msg)
    {
        MsgLogin tmpMsg = (MsgLogin)(msg);
        int result = tmpMsg.result;
        GameObject.FindGameObjectWithTag("TextInfo").
            GetComponent<TextMeshProUGUI>().text = result == 0 ? "Login Sucess" : "Login Failed";
        if (result == 0)
        {
            connectPanel.SetActive(true);
            loginPanel.SetActive(false);
            //PlayerManager.ID = tmpMsg.account;
            //print("callback-----------------------------");
            //SceneManager.LoadScene("GameScene");
        }
    }
}
