using UnityEngine;
using System.Collections.Generic;

public sealed class ConfigRegistry
{
    public GlobalRulesConfig GlobalRules
    {
        get;
    }

    public StartGameConfigCatalog StartGameCatalog
    {
        get;
    }

    public ConfigRegistry()
    {
        GlobalRules = Resources.Load<GlobalRulesConfig>("Configs/GlobalRulesConfig");
        StartGameCatalog = Resources.Load<StartGameConfigCatalog>("Configs/StartGameConfigCatalog");

        if (GlobalRules == null)
        {
            Debug.LogError(
                "ConfigRegistry -> 未加载到 GlobalRulesConfig，请确认资源位于 Resources/Configs/GlobalRulesConfig。"
            );
        }

        if (StartGameCatalog == null)
        {
            Debug.LogWarning(
                "ConfigRegistry -> 未加载到 StartGameConfigCatalog，请确认资源位于 Resources/Configs/StartGameConfigCatalog。"
            );
        }
    }

    public IReadOnlyList<StartGameConfig> GetStartGameConfigs()
    {
        if (StartGameCatalog == null || StartGameCatalog.Presets == null)
        {
            return new List<StartGameConfig>();
        }

        return StartGameCatalog.Presets;
    }

    public StartGameConfig FindStartGameConfigById(string presetId)
    {
        if (string.IsNullOrWhiteSpace(presetId))
        {
            return null;
        }

        IReadOnlyList<StartGameConfig> presets = GetStartGameConfigs();
        for (int i = 0; i < presets.Count; i++)
        {
            StartGameConfig preset = presets[i];
            if (preset != null && preset.PresetId == presetId)
            {
                return preset;
            }
        }

        return null;
    }
}

