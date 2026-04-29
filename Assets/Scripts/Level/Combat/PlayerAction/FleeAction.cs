using UnityEngine;

public sealed class FleeAction : IPlayerAction
{
    public CombatActionResult Execute(PlayerActionContext context)
    {
        CombatActionResult result = new CombatActionResult();
        if (context == null || context.Player == null)
        {
            return result;
        }

        float roll = Random.Range(0f, 100f);
        bool success = roll <= context.Player.EscapeRate;
        if (success)
        {
            result.EndIntent = BattleResult.Escape;
            return result;
        }

        result.SettlementLogs.Add(CombatSettlementLog.FromHint("逃跑失败！"));
        return result;
    }
}
