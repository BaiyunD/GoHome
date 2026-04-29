public sealed class NotInBattleGuard : IGuardRule
{
    public bool CanExecute(ActionId actionId, ActionContext context, out string reason)
    {
        BattleManager battleManager = BattleManager.Instance;
        if (battleManager == null)
        {
            reason = null;
            return true;
        }

        bool inBattle = battleManager.Phase != BattlePhase.None && battleManager.Phase != BattlePhase.Ended;
        if (!inBattle)
        {
            reason = null;
            return true;
        }

        reason = $"In battle (Phase: {battleManager.Phase}, SubPhase: {battleManager.TurnSubPhase}).";
        return false;
    }
}

