using System.Collections.Generic;

public sealed class PlayerRuntime
{
    public PlayerData RuntimeData
    {
        get;
    }

    public float CurrentHp { get; set; }
    public float MaxHp { get; set; }
    public float Attack { get; set; }
    public float Defense { get; set; }
    public float CriticalRate { get; set; }
    public float CriticalDamage { get; set; }
    public float BlockRate { get; set; }
    public float DodgeRate { get; set; }
    public float EscapeRate { get; set; }
    public float Money { get; set; }
    public string DisplayName { get; set; }
    public IReadOnlyList<string> TraitIds { get; set; }

    public PlayerRuntime(PlayerData runtimeData, StartResourceOverrides startResources)
    {
        if (runtimeData == null)
        {
            throw new System.ArgumentNullException(nameof(runtimeData), "PlayerRuntime 需要有效 PlayerData。");
        }

        RuntimeData = runtimeData;
        ResetFromStartConfigInternal(runtimeData, startResources);
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
        MaxHp = data.HP;
        CurrentHp = MaxHp;
        Attack = data.Attack;
        Defense = data.Defense;
        CriticalRate = data.CriticalRate;
        CriticalDamage = data.CriticalDamage;
        BlockRate = data.BlockRate;
        DodgeRate = data.DodgeRate;
        EscapeRate = data.EscapeRate;
        Money = startResources != null ? startResources.money : 0f;
        DisplayName = data.CharacterName;
        TraitIds = data.TraitIds ?? new List<string>();
    }
}

