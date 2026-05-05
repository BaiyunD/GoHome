using System.Collections.Generic;
using UnityEngine;

public sealed class PlayerRuntime
{
    /// <summary>
    /// NewGame 时 <see cref="PlayerStateManager.NewGame"/> 对 <see cref="StartGameConfig.PlayerTemplate"/> 的 <c>Instantiate</c> 实例；读档路径为 null。
    /// 数值以 <see cref="CombatBase"/> / 存档为准，勿把此引用当作运行中权威面板。
    /// </summary>
    public PlayerData NewGameTemplate
    {
        get;
        private set;
    }

    /// <summary>角色自身战斗基础（存档持久化层）。</summary>
    public PlayerCombatStatBase CombatBase;

    /// <summary>道具被动层（运行时；不进存档，由背包事件重建）。</summary>
    public PlayerCombatStatItemPassive CombatItemPassive;

    public float CurrentHp { get; set; }

    /// <summary>以下为 Base + ItemPassive  flatten 后的最终战斗面板缓存（只应由 RefreshFlattenedCombatFromLayers 更新）。</summary>
    public float MaxHp { get; private set; }

    public float Attack { get; private set; }
    public float Defense { get; private set; }
    public float CriticalRate { get; private set; }
    public float CriticalDamage { get; private set; }
    public float BlockRate { get; private set; }
    public float DodgeRate { get; private set; }
    public float EscapeRate { get; private set; }

    public int MoneyCents { get; set; }
    public string DisplayName { get; set; }
    public IReadOnlyList<string> TraitIds { get; set; }

    private PlayerRuntime()
    {
    }

    /// <summary>读档用：不创建 <see cref="PlayerData"/> 空壳，由 <see cref="ApplyCombatBaseFromSaveSnapshot"/> 写入 <see cref="CombatBase"/>。</summary>
    public static PlayerRuntime CreateForLoadBeforeSnapshot()
    {
        PlayerRuntime rt = new PlayerRuntime();
        rt.NewGameTemplate = null;
        rt.CombatBase = default;
        rt.CombatItemPassive = default;
        rt.MoneyCents = 0;
        rt.DisplayName = string.Empty;
        rt.TraitIds = new List<string>();
        rt.CurrentHp = 0f;
        rt.RefreshFlattenedCombatFromLayers();
        return rt;
    }

    public PlayerRuntime(PlayerData templateInstance, StartResourceOverrides startResources)
    {
        if (templateInstance == null)
        {
            throw new System.ArgumentNullException(nameof(templateInstance), "PlayerRuntime 需要有效 PlayerData。");
        }

        NewGameTemplate = templateInstance;
        ResetFromStartConfigInternal(templateInstance, startResources);
    }

    public PlayerCombatStatFinal GetFinalCombatStats()
    {
        return PlayerCombatStatCalculator.Combine(CombatBase, CombatItemPassive);
    }

    /// <summary>将 CombatBase / CombatItemPassive 合成结果写回对外扁平字段，并钳制 CurrentHp。</summary>
    public void RefreshFlattenedCombatFromLayers()
    {
        PlayerCombatStatFinal f = GetFinalCombatStats();
        MaxHp = f.MaxHp;
        Attack = f.Attack;
        Defense = f.Defense;
        CriticalRate = f.CriticalRate;
        CriticalDamage = f.CriticalDamage;
        BlockRate = f.BlockRate;
        DodgeRate = f.DodgeRate;
        EscapeRate = f.EscapeRate;
        CurrentHp = Mathf.Clamp(CurrentHp, 0f, Mathf.Max(0f, MaxHp));
    }

    /// <summary>仅刷新道具被动层（不重算背包）；用于迁移或测试。</summary>
    public void SetCombatItemPassive(PlayerCombatStatItemPassive passive)
    {
        CombatItemPassive = passive;
        RefreshFlattenedCombatFromLayers();
    }

    public bool ResetFromStartConfig(StartGameConfig startGameConfig, out string errorMessage)
    {
        PlayerData data = startGameConfig != null ? startGameConfig.PlayerTemplate : null;
        if (data == null)
        {
            errorMessage = "PlayerRuntime.ResetFromStartConfig -> StartGameConfig.PlayerTemplate 为空。";
            return false;
        }

        StartResourceOverrides startResources = startGameConfig != null ? startGameConfig.StartResources : null;
        ResetFromStartConfigInternal(data, startResources);
        errorMessage = string.Empty;
        return true;
    }

    private void ResetFromStartConfigInternal(PlayerData data, StartResourceOverrides startResources)
    {
        CombatBase = PlayerCombatStatBase.FromPlayerData(data);
        CombatItemPassive = default;
        float startMoneyYuan = startResources != null ? startResources.money : 0f;
        MoneyCents = MoneyUtil.ClampNonNegativeCents(MoneyUtil.YuanToCents(startMoneyYuan));
        DisplayName = data.CharacterName;
        TraitIds = data.TraitIds ?? new List<string>();
        CurrentHp = CombatBase.MaxHp;
        RefreshFlattenedCombatFromLayers();
    }

    /// <summary>将存档中的基础战斗字段写入 CombatBase（不碰 ItemPassive）；随后应 Rebuild 被动并 Refresh。</summary>
    /// <param name="saveFormatVersion">≥ <see cref="PlayerCombatStatCalculator.SaveFormatCombatLayers"/> 时应用 EscapeRate/DisplayName 等 v4 基础身份字段，避免旧档缺字段被默认 0 覆盖。</param>
    public void ApplyCombatBaseFromSaveSnapshot(SavePlayerData snapshot, int saveFormatVersion)
    {
        if (snapshot == null)
        {
            return;
        }

        float safeMax = Mathf.Max(0f, snapshot.HPMax);
        CombatBase.MaxHp = safeMax;
        CombatBase.Attack = Mathf.Max(0f, snapshot.Attack);
        CombatBase.Defense = Mathf.Max(0f, snapshot.Defense);
        CombatBase.CriticalRate = snapshot.CriticalRate;
        CombatBase.CriticalDamage = Mathf.Max(0f, snapshot.CriticalDamage);
        CombatBase.BlockRate = snapshot.BlockRate;
        CombatBase.DodgeRate = snapshot.DodgeRate;
        if (saveFormatVersion >= PlayerCombatStatCalculator.SaveFormatCombatLayers)
        {
            CombatBase.EscapeRate = CharacterDataBase.ClampRate(snapshot.EscapeRate);
            DisplayName = snapshot.DisplayName != null ? snapshot.DisplayName : string.Empty;
        }

        MoneyCents = MoneyUtil.ClampNonNegativeCents(snapshot.MoneyCents);
        RefreshFlattenedCombatFromLayers();
        CurrentHp = Mathf.Clamp(snapshot.HPCurrent, 0f, Mathf.Max(0f, MaxHp));
    }

    /// <summary>v3 存档：快照里为「合并后面板」，在 ItemPassive 已由背包重算后，拆回 Base。</summary>
    public void MigrateCombatBaseFromLegacyCombinedSnapshot(SavePlayerData snapshot)
    {
        if (snapshot == null)
        {
            return;
        }

        PlayerCombatStatItemPassive p = CombatItemPassive;
        CombatBase.MaxHp = Mathf.Max(0f, snapshot.HPMax - p.MaxHpBonus);
        CombatBase.Attack = Mathf.Max(0f, snapshot.Attack - p.AttackBonus);
        CombatBase.Defense = Mathf.Max(0f, snapshot.Defense - p.DefenseBonus);
        CombatBase.CriticalDamage = Mathf.Max(0f, snapshot.CriticalDamage - p.CriticalDamageBonus);

        CombatBase.CriticalRate = DeclampRateForBase(snapshot.CriticalRate, p.CriticalRateBonus);
        CombatBase.BlockRate = DeclampRateForBase(snapshot.BlockRate, p.BlockRateBonus);
        CombatBase.DodgeRate = DeclampRateForBase(snapshot.DodgeRate, p.DodgeRateBonus);
    }

    private static float DeclampRateForBase(float combinedClamped, float passiveBonus)
    {
        float sum = combinedClamped - passiveBonus;
        return CharacterDataBase.ClampRate(sum);
    }

    /// <summary>事件系统：按「当前最终值」思路修改 Attack（写入 Base）。</summary>
    public void ApplyFinalAttackValueAfterOperation(float nextFinal)
    {
        CombatBase.Attack = Mathf.Max(0f, nextFinal - CombatItemPassive.AttackBonus);
        RefreshFlattenedCombatFromLayers();
    }

    public void ApplyFinalDefenseValueAfterOperation(float nextFinal)
    {
        CombatBase.Defense = Mathf.Max(0f, nextFinal - CombatItemPassive.DefenseBonus);
        RefreshFlattenedCombatFromLayers();
    }

    public void ApplyFinalCriticalRateValueAfterOperation(float nextFinal)
    {
        CombatBase.CriticalRate = DeclampRateForBase(
            CharacterDataBase.ClampRate(nextFinal),
            CombatItemPassive.CriticalRateBonus);
        RefreshFlattenedCombatFromLayers();
    }

    public void ApplyFinalCriticalDamageValueAfterOperation(float nextFinal)
    {
        CombatBase.CriticalDamage = Mathf.Max(0f, nextFinal - CombatItemPassive.CriticalDamageBonus);
        RefreshFlattenedCombatFromLayers();
    }

    public void ApplyFinalBlockRateValueAfterOperation(float nextFinal)
    {
        CombatBase.BlockRate = DeclampRateForBase(
            CharacterDataBase.ClampRate(nextFinal),
            CombatItemPassive.BlockRateBonus);
        RefreshFlattenedCombatFromLayers();
    }

    public void ApplyFinalDodgeRateValueAfterOperation(float nextFinal)
    {
        CombatBase.DodgeRate = DeclampRateForBase(
            CharacterDataBase.ClampRate(nextFinal),
            CombatItemPassive.DodgeRateBonus);
        RefreshFlattenedCombatFromLayers();
    }
}
