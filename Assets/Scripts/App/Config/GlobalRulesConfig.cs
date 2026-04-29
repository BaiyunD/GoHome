using UnityEngine;

[CreateAssetMenu(fileName = "GlobalRulesConfig", menuName = "GoHome/Global Rules Config")]
public class GlobalRulesConfig : ScriptableObject
{
    [Header("休息结算规则")]
    public float restEnergyPercent = 1f;
    public int restHPDelta = 30;
    public int restHungerDelta = -20;

    [Header("全局路线目标")]
    public int homeDistance = 300;
}

