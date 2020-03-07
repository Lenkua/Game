using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public class Tuple
{
    public Vector3 it;
    public float at;
    public bool baoji;
    public float time;
    public GameObject ui;
    public Transform text;
    public Vector3 offset;
    Tuple() { }
    public Tuple(Transform IT,float ATTACK,bool BAOJI,GameObject UI, Vector3 OFFSET,string COLOR)
    {
        it = IT.position;
        at = ATTACK;
        baoji = BAOJI;
        offset = OFFSET;
        ui = UI;
        if (baoji)
        {
            offset.x += 1;
            text = ui.transform.Find("BJText").transform;
            text.position = RectTransformUtility.WorldToScreenPoint(Camera.main, it+offset);
            ui.transform.Find("Text").gameObject.SetActive(false);
            ui.transform.Find("BJText").GetComponent<Text>().text=
            "<color="+ COLOR+ "><size=30>"+at.ToString()+ "</size></color>";
        }
        else
        {
            
            text = ui.transform.Find("Text").transform;
            text.position = RectTransformUtility.WorldToScreenPoint(Camera.main, it + offset);
            ui.transform.Find("Text").GetComponent<Text>().fontSize = 20;
            ui.transform.Find("Text").GetComponent<Text>().text =
            "<color=" + COLOR + ">" + at.ToString() + "</color>";
            ui.transform.Find("BJText").gameObject.SetActive(false);
        }
        time = 2f;
    }
}
public class UIContro : MonoBehaviour
{
    private GameObject UI_obj;
    //private GameObject Player;
    private Button button;
    private Button button1;
    private Button button2;
    private Button button3;
    private Button button4;
    private Button sjbutton;
    private Text declear;
    private Text kill;
    private Player player;
    private Warrior warrior;
    private GameObject store;
    private List<Tuple> shtz = new List<Tuple>();
    private float showtime = 0;
    private GameObject[] csm = new GameObject[2];
    //skill effect
    //login 
    private GameObject Login;
    private Button Login_Button;
    private Button Reges_Button;
    private InputField Login_ID_Text;
    private InputField Login_Password_Text;
    private InputField Login_Name_Text;
    private Text Message_Text;
    private float Message_Time;
    private bool IsLoadingCompleteed;

    //socket
    private Connect con;
    private void Awake()
    {
        con = transform.GetComponent<Connect>();
        Login = Instantiate(Resources.Load("UI/Login_UI", typeof(GameObject))) as GameObject;
        Login_ID_Text = Login.transform.Find("Panel/ID/InputField").GetComponent<InputField>();
        Login_Password_Text = Login.transform.Find("Panel/Password/InputField").GetComponent<InputField>();
        Login_Name_Text = Login.transform.Find("Panel/Name/InputField").GetComponent<InputField>();
        Message_Text = Login.transform.Find("Text").GetComponent<Text>();
        Message_Text.gameObject.SetActive(false);
        Login.transform.Find("Panel/Name").gameObject.SetActive(false);
        Login_Button = Login.transform.Find("Panel/DengLu").GetComponent<Button>();
        Login_Button.onClick.AddListener(OnLogin_ButtonClick);
        Reges_Button = Login.transform.Find("Panel/ZhuCe").GetComponent<Button>();
        Reges_Button.onClick.AddListener(OnReges_ButtonClick);
    }
    // Start is called before the first frame update

    void Start()
    {

    }

    public void check_Login_no(int num)
    {
        Message_Time = 2;
        if (num < 0) Message_Text.text = "网络错误";
        if (num == 0) Message_Text.text = "账号不存在或密码错误";
        if (num == 10)
        {
            Message_Text.text = "登录成功";
            Loading();
            Login.transform.Find("Panel").gameObject.SetActive(false);
        }
        if (num == 1) Message_Text.text = "账号或昵称已存在";
        if (num == 12) Message_Text.text = "账号或昵称不合法";
        if (num == 11) Message_Text.text = "注册成功";

    }
    // Update is called once per frame
    void Update()
    {
        Message_Time -= Time.fixedDeltaTime;
        if (Message_Time > 0) Message_Text.gameObject.SetActive(true);
        else Message_Text.gameObject.SetActive(false);

        if (IsLoadingCompleteed == false) return;

        csm[0].SetActive(false); csm[0].SetActive(true);
        csm[1].SetActive(false); csm[1].SetActive(true);

        kill.text = "杀敌数:" + player.kill.ToString();
        //伤害跳字
        for (int i = shtz.Count - 1; i >= 0; i--)
        {
            Tuple it = shtz[i];
            it.time -= Time.fixedDeltaTime;
            if (it.time < 0)
            {
                Destroy(it.ui);
                shtz.Remove(it);
            }
            else
            {
                Vector3 tpos = it.it + it.offset;
                tpos.y += (2 - it.time) * 1.5f;
                Vector2 npos = RectTransformUtility.WorldToScreenPoint(Camera.main, tpos);
                it.text.position = npos;
            }
        }
        showtime -= Time.fixedDeltaTime;
        if (showtime <= 0) UI_obj.transform.Find("Text1").gameObject.SetActive(false);
        if (warrior.getHP() < 40)
        {
            UI_obj.transform.Find("Tip").GetComponent<Text>().text = "雅典娜正在遭受攻击！！雅典娜血量剩余%" + warrior.getHP().ToString();
            UI_obj.transform.Find("Tip").gameObject.SetActive(true);
        }
        else UI_obj.transform.Find("Tip").gameObject.SetActive(false);
        update_declear();
        if (warrior.getHP() <= 1e-4)
        {
            UI_obj.transform.Find("defead").gameObject.SetActive(true);
            UI_obj.transform.Find("defead/Text").gameObject.SetActive(false);
            Color now = UI_obj.transform.Find("defead").GetComponent<Image>().color;
            now.a += 0.005f;
            UI_obj.transform.Find("defead").GetComponent<Image>().color = now;
            if (now.a >= 1)
            {
                UI_obj.transform.Find("defead/Text").gameObject.SetActive(true);
            }
        }
    }
    private void Loading()
    {
        if (IsLoadingCompleteed) return;
        Debug.Log("UI Loading");
        player = transform.Find("Player_archer(Clone)/player_archer_nerwin").GetComponent<Player>();
        warrior = transform.Find("Player_warrior_blademaster_master").GetComponent<Warrior>();
        csm[0] = Instantiate(Resources.Load("Player_warrior_attack_an_fuchoufengbao_Clip02", typeof(GameObject))) as GameObject;
        csm[1] = Instantiate(Resources.Load("Player_warrior_attack_an_fuchoufengbao_Clip02", typeof(GameObject))) as GameObject;
        csm[0].transform.position = new Vector3(68.78509f, 0, 168.816f);
        csm[1].transform.position = new Vector3(136.6014f, 2, 70.01092f);
        Object UI_prefab = Resources.Load("UI/UI", typeof(GameObject));
        UI_obj = Instantiate(UI_prefab) as GameObject;
        //UI_obj.transform.SetParent(player.transform);
        UI_obj.transform.Find("Tip").gameObject.SetActive(false);
        button = UI_obj.transform.Find("Image/Button").GetComponent<Button>();
        button1 = UI_obj.transform.Find("Image1/Button").GetComponent<Button>();
        button2 = UI_obj.transform.Find("Image2/Button").GetComponent<Button>();
        button3 = UI_obj.transform.Find("LianJi").GetComponent<Button>();
        button4 = UI_obj.transform.Find("ZhuCheng").GetComponent<Button>();
        store = UI_obj.transform.Find("Store").gameObject;
        sjbutton = store.transform.Find("Button").GetComponent<Button>();
        declear = store.transform.Find("Text").GetComponent<Text>();
        sjbutton.gameObject.SetActive(false);
        declear.gameObject.SetActive(false);
        UI_obj.transform.Find("Text1").gameObject.SetActive(false);
        UI_obj.transform.Find("defead").gameObject.SetActive(false);
        store.SetActive(false);
        button3.gameObject.SetActive(false);
        button4.gameObject.SetActive(false);
        kill = UI_obj.transform.Find("Text").GetComponent<Text>();

        button.onClick.AddListener(OnButtonClick);
        button1.onClick.AddListener(OnButtonClick1);
        button2.onClick.AddListener(OnButtonClick2);
        button3.onClick.AddListener(OnButtonClick3);
        button4.onClick.AddListener(OnButtonClick4);
        sjbutton.onClick.AddListener(OnsjButtonClick);
        //store.transform.GetChild(0).GetComponent<Button>().onClick.AddListener(OnButton0Click);
        store.transform.GetChild(1).GetComponent<Button>().onClick.AddListener(OnButton1Click);
        store.transform.GetChild(2).GetComponent<Button>().onClick.AddListener(OnButton2Click);
        store.transform.GetChild(3).GetComponent<Button>().onClick.AddListener(OnButton3Click);
        store.transform.GetChild(4).GetComponent<Button>().onClick.AddListener(OnButton4Click);
        IsLoadingCompleteed = true;
    }
    private void update_declear()
    {
        if (declear.gameObject.name == "xixue")
        {
            declear.text = "当前效果:" + "额外增加%" + (warrior.XiXue * 100).ToString() + "的吸血效果"
            + "\n下一级:额外增加%10吸血";
        }
        if (declear.gameObject.name == "mingling")
        {
            declear.text = "当前效果:" + "额外增加" + (warrior.Attack_Power - 1).ToString() + "点攻击力"
        + "\n下一级:额外增加1点攻击力";
        }
        if (declear.gameObject.name == "zhuanzhu")
        {
            declear.text = "当前效果:" + "额外增加" + (warrior.Defense).ToString() + "点护甲"
        + "\n下一级:额外增加1点护甲";
        }
    }
    private void OnButtonClick()
    {
        if (player.isDead() == true) return;
        player.GetComponent<Skill_Contro>().Jianyu();
    }
    private void OnButtonClick1()
    {
        if (player.isDead()) return;
        player.GetComponent<Skill_Contro>().Heavy_Attack();
    }
    private void OnButtonClick2()
    {
        if (player.isDead()) return;
        player.GetComponent<Skill_Contro>().luoxuanfengbao();
    }
    private void OnButtonClick3()
    {
        if (player.isDead()) return;
        string now = UI_obj.transform.Find("LianJi/Text").GetComponent<Text>().text;
        if (now == "进入")
        {
            UI_obj.transform.Find("LianJi/Text").GetComponent<Text>().text = "离开";

        }
        else UI_obj.transform.Find("LianJi/Text").GetComponent<Text>().text = "进入";
        transform.Find("LianGongFang(Clone)").GetComponent<LGF>().react();
        gameObject.GetComponent<Triger>().Act();
    }
    private void OnButtonClick4()
    {
        if (player.isDead()) return;
        if (store.activeSelf == false) store.SetActive(true);
        else store.SetActive(false);
    }
    public void ShangHaiTiaoZi(Transform it, float sh, bool baoji, Vector3 offset, string color)
    {
        GameObject ui = Instantiate(Resources.Load("UI/ShangHaiTiaoZi", typeof(GameObject))) as GameObject;
        ui.transform.SetParent(transform.Find("ShanghaiTiaoZi").transform);
        shtz.Add(new Tuple(it, sh, baoji, ui, offset, color));
    }
    public void InRegion()
    {
        if (IsLoadingCompleteed == false) return;
        button3.gameObject.SetActive(true);
    }
    public void OutRegion()
    {
        if (IsLoadingCompleteed == false) return;
        button3.gameObject.SetActive(false);
    }
    public void Shop(bool tp)
    {
        if (IsLoadingCompleteed == false) return;
        button4.gameObject.SetActive(tp);
        if (tp == false)
        {
            store.SetActive(false);
        }
    }
    private void OnButton1Click()
    {
        if (declear.gameObject.name == "xixue" && declear.gameObject.activeSelf == true)
        {
            declear.gameObject.SetActive(false);
            sjbutton.gameObject.SetActive(false);
            return;
        }
        declear.gameObject.name = "xixue";
        declear.text = "当前效果:" + "额外增加%" + (warrior.XiXue * 100).ToString() + "的吸血效果"
        + "\n下一级:额外增加%10吸血";
        declear.gameObject.SetActive(true);
        sjbutton.gameObject.SetActive(true);
        //warrior.XiXueGH();
    }
    private void OnButton2Click()
    {
        if (declear.gameObject.name == "zhuanzhu" && declear.gameObject.activeSelf == true)
        {
            declear.gameObject.SetActive(false);
            sjbutton.gameObject.SetActive(false);
            return;
        }
        declear.gameObject.name = "zhuanzhu";
        declear.text = "当前效果:" + "额外增加" + (warrior.Defense).ToString() + "点护甲"
        + "\n下一级:额外增加1点护甲";
        declear.gameObject.SetActive(true);
        sjbutton.gameObject.SetActive(true);
    }
    private void OnButton3Click()
    {
        warrior.ShenShengZhiGuang();
    }
    private void OnButton4Click()
    {
        if (declear.gameObject.name == "mingling" && declear.gameObject.activeSelf == true)
        {
            declear.gameObject.SetActive(false);
            sjbutton.gameObject.SetActive(false);
            return;
        }
        declear.gameObject.name = "mingling";
        declear.text = "当前效果:" + "额外增加" + (warrior.Attack_Power - 1).ToString() + "点攻击力"
        + "\n下一级:额外增加1点攻击力";
        declear.gameObject.SetActive(true);
        sjbutton.gameObject.SetActive(true);
    }
    private void OnsjButtonClick()
    {
        if (player.kill < 0)
        {
            UI_obj.transform.Find("Text1").gameObject.SetActive(true);
            showtime = 4;
            return;
        }
        player.kill -= 0;
        if (declear.gameObject.name == "xixue") warrior.XiXueGH();
        if (declear.gameObject.name == "mingling") warrior.MingLingGH();
        if (declear.gameObject.name == "zhuanzhu") warrior.ZhuanZhuGH();
    }
    private void OnLogin_ButtonClick()
    {
        if (Login.transform.Find("Panel/Name").gameObject.activeSelf == true)
        {
            Login.transform.Find("Panel/Name").gameObject.SetActive(false);
            return;
        }
        con.Login(Login_ID_Text.text, Login_Password_Text.text);

    }
    private void OnReges_ButtonClick()
    {
        if (Login.transform.Find("Panel/Name").gameObject.activeSelf == false)
        {
            Login.transform.Find("Panel/Name").gameObject.SetActive(true);
            return;
        }
        con.Reges(Login_ID_Text.text, Login_Password_Text.text, Login_Name_Text.text);
    }
    public bool Loading_Type() { return IsLoadingCompleteed; }
}
