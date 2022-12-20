using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class hpBar : MonoBehaviour
{

    public static hpBar instance;
    private float MHP;
    private float CHP;
    private float PHP;

    public float FillSpeed;
    public Image[] HpBars;
    public Image[] BackGrounds;
    public Image[] MidGrounds;

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogError("MORE THAN ONE HPBAR INSTANCES AT " + instance.transform.name + " AND " + transform.name);
            return;
        }
        instance = this;
    }

    public void SetMHP(float mhp)
    { 
        MHP = mhp; 
    }
    public void SetCHP(float chp) 
    {
        PHP = CHP;
        CHP = chp;
    }
    private void TakeDamage() 
    {
        foreach (Image i in HpBars)
            i.fillAmount = CHP / MHP;
        foreach (Image i in MidGrounds)
            i.fillAmount = CHP / MHP;
    }
    private void HealDamage()
    {
        foreach (Image i in MidGrounds)
            i.fillAmount = CHP / MHP;
    }

    private void FillBG() 
    {
        float current = BackGrounds[0].fillAmount;
        float target = HpBars[0].fillAmount;
        float fill = Mathf.Clamp(current - Time.deltaTime * FillSpeed, target, current);
        //fill = current - target < 0.0125f ? target : fill;
        //fill = target > current ? target : fill;

        foreach (Image i in BackGrounds)
            i.fillAmount = fill;
    }
    private void FillHP() 
    {
        float current = HpBars[0].fillAmount;
        float target = MidGrounds[0].fillAmount;
        float fill = Mathf.Clamp(current + Time.deltaTime * FillSpeed, current, target);
        //fill = current - target < 0.0125f ? target : fill;
        //fill = target > current ? fill : target;

        foreach (Image i in HpBars)
            i.fillAmount = fill;
    }
    private void FollowHP() 
    {

        float cMG = MidGrounds[0].fillAmount;
        float cBG = BackGrounds[0].fillAmount;
        float cHP = HpBars[0].fillAmount;        

        //foreach (Image i in MidGrounds)
//            i.fillAmount = cMG < cHP ? cHP : cMG;
        foreach (Image i in BackGrounds)
            i.fillAmount = cBG < cHP ? cHP : cBG;

    }

    private void Update()
    {

        FillSpeed = FillSpeed <= 0 ? 1 : FillSpeed;

        if (CHP > PHP)
            HealDamage();
        else if (CHP < PHP)
            TakeDamage();

        FillHP();
        FillBG();
        FollowHP();

    }
}
