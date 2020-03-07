using System;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using Google.Protobuf;
using System.Threading;

public class Connect : MonoBehaviour
{
    private UIContro ui;


    private Socket client_fd;
    private EndPoint client_addr;

    //Data
    private Account.Account acc=new Account.Account();
    private IMessage Iacc = new Account.Account();
    private byte[] recv=new byte[5000];
    private byte[] tmp = new byte[5000];
    private List<byte> vec = new List<byte>();
    private List<Account.Account> Sever_Message=new List<Account.Account>();

    //Player Data
    private Dictionary<string, GameObject> PlayerList = new Dictionary<string, GameObject>();
    private GameObject LocalPlayer;
    private bool IsLoadingCompleteed;
    // Start is called before the first frame update
    void Start()
    {
        ui = transform.GetComponent<UIContro>();
        acc.ID = "1";acc.Name = "1";acc.Password = "1";acc.Type = "1";
        acc.X = "1";acc.Y ="1";acc.Z ="1";
	    acc.EulerAnglesX="1";
	    acc.EulerAnglesY = "1";
	    acc.EulerAnglesZ="1";
	    acc.Ani="Player_archer_nerwin_idle";
        client_fd = new Socket(AddressFamily.InterNetwork,SocketType.Stream,ProtocolType.Tcp);
        client_addr = new IPEndPoint(IPAddress.Parse("116.62.161.150"),4000);
        client_fd.Connect(client_addr);
        IsLoadingCompleteed = false;
    }

    // Update is called once per frame
    void Update()
    {
        ReceiveByteData();
        if (IsLoadingCompleteed == false) return;
        SendLocalPosition();
        for(int i=0;i<Sever_Message.Count;i++)
        {
            HandleSeverMessage();
        }

    }

    private void AddPlayer(string name,string hero)
    {
        GameObject it = Instantiate(Resources.Load("OtherPlayer", typeof(GameObject))) as GameObject;
        it.transform.SetParent(transform.Find("PlayerList"));
        it.transform.GetComponent<Player>().SetName(name);
        it.transform.GetComponent<Player>().SetIsLocal(false);
        PlayerList.Add(name,it);
    }
    private void MovePlayer(ref Account.Account now)
    {
        if (PlayerList.ContainsKey(now.Name) == false) AddPlayer(now.Name, "");
        GameObject it = PlayerList[now.Name];
        Vector3 pos = new Vector3(float.Parse(now.X),float.Parse(now.Y),float.Parse(now.Z));
        Vector3 ro=new Vector3(float.Parse(now.EulerAnglesX),float.Parse(now.EulerAnglesY),float.Parse(now.EulerAnglesZ));
        it.transform.eulerAngles = ro;
        it.transform.position = pos;
        it.GetComponent<Player>().Play(now.Ani);
    }
    private void HandleSeverMessage()
    {
        if (Sever_Message.Count <= 0) return;
        Account.Account now = Sever_Message[0];
        Sever_Message.RemoveAt(0);
        Debug.Log(now.Type);
        if (now.Type == "Login")
        {
            AddPlayer(now.Name, "");
        }
        else if (now.Type == "Move")
        {
            MovePlayer(ref now);
        }
        else if (now.Type == "1")ui.check_Login_no(10);
        else if (now.Type == "0") ui.check_Login_no(0);
        else if (now.Type == "2") ui.check_Login_no(1);
        else if (now.Type == "3") ui.check_Login_no(12);
        else if (now.Type == "4") ui.check_Login_no(11);
    }
    private void SendLocalPosition()
    {
        acc.Type = "Move";
        acc.Name = LocalPlayer.transform.GetComponent<Player>().GetName();
        acc.X = LocalPlayer.transform.position.x.ToString();
        acc.Y = LocalPlayer.transform.position.y.ToString();
        acc.Z = LocalPlayer.transform.position.z.ToString();
        acc.EulerAnglesX = LocalPlayer.transform.eulerAngles.x.ToString();
        acc.EulerAnglesY = LocalPlayer.transform.eulerAngles.y.ToString();
        acc.EulerAnglesZ = LocalPlayer.transform.eulerAngles.z.ToString();
        if (LocalPlayer.GetComponent<Animator>().GetBool("IsRun") == true) acc.Ani = "Player_archer_run_normal";
        else acc.Ani = "Player_archer_nerwin_idle";
        SendToSeverMessage(acc.ToByteArray());
    }
    private void Loading(string name)
    {
        GameObject it = Instantiate(Resources.Load("Player_archer", typeof(GameObject))) as GameObject;//玩家
        it.transform.SetParent(transform);
        LocalPlayer = it.transform.Find("player_archer_nerwin").gameObject;
        LocalPlayer.transform.GetComponent<Player>().SetName(name);
        LocalPlayer.transform.GetComponent<Player>().SetIsLocal(true);
        it = Instantiate(Resources.Load("LianGongFang", typeof(GameObject))) as GameObject;//练功房
        it.transform.SetParent(transform);
        it = Instantiate(Resources.Load("Monster", typeof(GameObject))) as GameObject;//关卡
        it.name = "Monster";
        it.transform.SetParent(transform);
        IsLoadingCompleteed = true;
    }
    public bool Loading_Type() { return IsLoadingCompleteed; }
    public bool IsConnected() { return client_fd.Connected; }

    /*
    private string ByteToString()
    {
        string ans=System.Text.Encoding.UTF8.GetString(recv);
        string ANS = "";
        for (int i = 0; i < ans.Length && ans[i] != '\0'; i++)
            ANS += ans[i];
        return ANS;
    }
    */
    public void Login(string ID,string Password)
    {
        if (IsLoadingCompleteed) return ;
        if (client_fd.Connected == false) return ;
        acc.ID =ID;
        acc.Name = "nerwin";
        acc.Password = Password;
        acc.Type = "Login";
        SendToSeverMessage(acc.ToByteArray());
    }
    public void Reges(string ID,string Password,string nick)
    {
        if (client_fd.Connected == false) return ;
        acc.ID = ID;
        acc.Password = Password;
        acc.Name = nick;
        acc.Type = "Reges";
        SendToSeverMessage(acc.ToByteArray());
    }
    private void ReceiveByteData()
    {
        Debug.Log("???");
        int count = client_fd.Receive(recv);
        for (int i = 0; i < count; i++) vec.Add(recv[i]);
        while (vec.Count >= 3)
        {
            int MessageTag = (int)vec[0];
            int length = (int)((int)vec[1] - 1) * 127 + (int)vec[2] - 1;
            Debug.Log("length=" + length);
            if (length + 3 < vec.Count) break;
            vec.RemoveAt(0);
            vec.RemoveAt(1);
            vec.RemoveAt(2);
            for (int i = 0; i < length; i++)tmp[i] = vec[i];
            Sever_Message.Add((Account.Account)Iacc.Descriptor.Parser.ParseFrom(tmp, 0, length));
            vec.RemoveRange(0,length);
        }
    }
    private void SendToSeverMessage(byte[] msg)
    {
        byte[] vs = new byte[3 + msg.Length];
        vs[0] = (byte)1;
        vs[1] =(byte)( msg.Length / 127 + 1);
        vs[2] = (byte)(msg.Length % 127 + 1);
        for (int i = 3; i < 3 + msg.Length; i++) vs[i]= msg[i-3];
        client_fd.Send(vs,vs.Length,0);
    }
}
