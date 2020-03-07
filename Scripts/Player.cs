using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class Player : MonoBehaviour
{
    // Start is called before the first frame update
    private bool IsLocal;
    private Animator ani;
    //UI
    private GameObject UI_player_xinxi;
    private GameObject hp;
    private Slider HP;
    private string NAME;

    //skill editor
    private GameObject UI_skill_editor;
    private skill_config config;

    //skill play
    private bool Press_Down;
    private bool Press_Up;

    //ARPG
    private Transform m_camera;

    //state
    private bool IsDead;

    //技能间隔
    private float CD_Time;

    //杀敌数
    public int kill;
    //复活时间
    private float fuhuotime;
    private void Awake()
    {
        //GameObject pp=transform.Find("EasyTouchControlsCanvas").Find("New Joystick").GetComponent<ETCJoystick>

        m_camera = Camera.main.transform;
        UI_player_xinxi = Instantiate(Resources.Load("UI/Player_xinxi", typeof(GameObject))) as GameObject;
        UI_skill_editor = Instantiate(Resources.Load("UI/Skill_Editor", typeof(GameObject))) as GameObject;
        config = Resources.Load("skill_config_file") as skill_config;
        if(config)
        {
            Transform skill_Editor_panel = UI_skill_editor.transform.Find("Panel");
            for(int i=0;i<config.skill_config_list.Count;i++)
            {
                skill_data data = config.skill_config_list[i];
                //Debug.LogWarning(i);
                skill_Editor_panel.GetChild(i * 5).GetComponent<InputField>().text=data.skill_id.ToString();
                skill_Editor_panel.GetChild(i * 5 + 1).GetComponent<InputField>().text=data.skill_name;
                skill_Editor_panel.GetChild(i * 5 + 2).GetComponent<InputField>().text = data.skill_anim;
                skill_Editor_panel.GetChild(i * 5 +3).GetComponent<InputField>().text = data.skill_effect;
                skill_Editor_panel.GetChild(i * 5 + 4).GetComponent<InputField>().text = data.skill_sound;
            }
        }
        UI_player_xinxi.SetActive(false);
        UI_skill_editor.SetActive(false);
    }
    void Start()
    {
        hp = Instantiate(Resources.Load("UI/HP", typeof(GameObject))) as GameObject;
        hp.transform.SetParent(transform.root.Find("ShanghaiTiaoZi"));
        HP = hp.transform.Find("HP_Slider").GetComponent<Slider>();
        HP.transform.Find("ID").GetComponent<Text>().text=NAME;
        HP.value = HP.maxValue;
        ani = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 sca = transform.localScale;
        sca += new Vector3(5f, 5f, 5f);

        transform.localScale.Set(sca.x, sca.y, sca.z);
        HP_update();
        if (IsLocal == false) return;
        CD_Time += Time.fixedDeltaTime;
        if (IsDead)
        {
            fuhuotime -= Time.fixedDeltaTime;
            transform.parent.Find("EasyTouchControlsCanvas/New Joystick").GetComponent<ETCJoystick>().activated = false;
            transform.parent.Find("EasyTouchControlsCanvas/New Joystick").GetComponent<ETCJoystick>().activated = true;
            transform.parent.Find("EasyTouchControlsCanvas/New Joystick").GetComponent<ETCJoystick>().activated = false;
            if (fuhuotime <= 0) FuHuo();
        }
        else
        {
            transform.parent.Find("EasyTouchControlsCanvas/New Joystick").GetComponent<ETCJoystick>().activated = true;
        }
        UI_player_xinxi.transform.Find("Panel").Find("HP").Find("Text").GetComponent<Text>().text =HP.value.ToString();
        Press();
        if (IsDead) return;
        Follow();
        if (Press_Down && Press_Up == false && ani.GetBool("Attack") == true)
        {
            transform.parent.Find("EasyTouchControlsCanvas/New Joystick").GetComponent<ETCJoystick>().activated = false;
            transform.parent.Find("EasyTouchControlsCanvas/New Joystick").GetComponent<ETCJoystick>().activated = true;
        }
        
    }
    private void FuHuo()
    {
        IsDead = false;
        ani.applyRootMotion = false;
        transform.position = new Vector3(58.77835f, 0.01062775f, 164.153f);
        ani.Play("Player_archer_nerwin_idle");
        HP.value = HP.maxValue;
    }
    private void HP_update()
    {
        Vector3 tarpos = transform.Find("Bip01_archer").transform.position;
        Vector2 pos = RectTransformUtility.WorldToScreenPoint(Camera.main, tarpos);
        HP.GetComponent<RectTransform>().position = pos + new Vector2(0, 25f);
    }
    private void Skill_data_update()
    {
        if(config)
        {
            Transform skill_Editor_panel = UI_skill_editor.transform.Find("Panel");
            for (int i = 0; i < config.skill_config_list.Count; i++)
            {
                //Debug.LogWarning(i);
                config.skill_config_list[i].skill_id=skill_Editor_panel.GetChild(i * 5).GetComponent<InputField>().text[0]-'0';
                config.skill_config_list[i].skill_name=skill_Editor_panel.GetChild(i * 5 + 1).GetComponent<InputField>().text;
                config.skill_config_list[i].skill_anim=skill_Editor_panel.GetChild(i * 5 + 2).GetComponent<InputField>().text;
                config.skill_config_list[i].skill_effect=skill_Editor_panel.GetChild(i * 5 + 3).GetComponent<InputField>().text ;
                config.skill_config_list[i].skill_sound=skill_Editor_panel.GetChild(i * 5 + 4).GetComponent<InputField>().text;
            }
        }
    }
    private void Press()
    {
        if (Input.GetKeyDown(KeyCode.Y))
        {
            if (UI_player_xinxi.activeSelf== false) UI_player_xinxi.SetActive(true);
            else UI_player_xinxi.SetActive(false);
        }
        if(Input.GetKeyDown(KeyCode.K))
        {
            if (UI_skill_editor.activeSelf == false) UI_skill_editor.SetActive(true);
            else UI_skill_editor.SetActive(false);
        }
        if (IsDead) return;
        if(Input.GetKeyDown(KeyCode.Q))
        {
            if (CD_Time <= 0.25) return;
            CD_Time = 0;
            ani.SetBool("IsRun", false);
            GetComponent<Skill_Contro>().Attack();
           
        }

    }
    private void Follow()
    {
        Vector3 Pos = transform.position;
        Pos.z -= 8f;
        Pos.y += 8f;
        if (m_camera.GetComponent<Camera_shake>().IsShake()) return;
        m_camera.position = Pos;
    }
    public void  RunAnimation()
    {
        if (GetComponent<Skill_Contro>().Stop() == false) return;
        Press_Down = true;
        Press_Up = false;
        ani.SetBool("IsRun",true);
    }
    public void StandAnimation()
    {
        Press_Up = true;
        ani.SetBool("IsRun",false);
    }
    public bool isDead(){return IsDead;}
    public void Dead()
    {
        if (IsDead) return;
        ani.SetTrigger("Dead");
        fuhuotime = 5f;
        IsDead = true;
        if (IsLocal == false) return;
        transform.parent.Find("EasyTouchControlsCanvas/New Joystick").GetComponent<ETCJoystick>().activated = false;
    }
    public void BeAttack(float val)
    {
        transform.root.GetComponent<UIContro>().ShangHaiTiaoZi(transform, val, false, new Vector3(0.5f, 1f, 0),"red");
        HP.value-= val;
        if (HP.value <= 0)
        {
            HP.value = 0;
            Dead();
        }
    }
    public void SetName(string name){NAME = name;}
    public string GetName() { return NAME; }
    public void Play(string animation)
    {
        ani.Play(animation);
    }
    public void SetIsLocal(bool FFFF) { IsLocal = FFFF; }
}
