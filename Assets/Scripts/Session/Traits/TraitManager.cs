using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TraitManager : MonoBehaviour
{
    public static TraitManager Instance { get; private set; }

    [SerializeField] private TraitDatabase traitDatabase;

    private readonly Dictionary<string, TraitDefinition> _definitionDict = new Dictionary<string, TraitDefinition>();
    private readonly List<string> _playerTraitIds = new List<string>();
    private readonly List<string> _enemyTraitIds = new List<string>();
    private readonly Dictionary<string, TraitEffectBase> _attachedEffects = new Dictionary<string, TraitEffectBase>();
    private readonly Dictionary<string, Func<TraitRuntimeContext, TraitEffectBase>> _effectFactories =
        new Dictionary<string, Func<TraitRuntimeContext, TraitEffectBase>>();

    private void Awake()
    {
        Instance = this;
        RebuildDefinitionDictionary();
    }

    /// <summary>
    /// 注册某特性编号对应的效果工厂（无注册则无运行时效果，仅保留定义与列表）。
    /// </summary>
    public void RegisterEffectFactory(string traitId, Func<TraitRuntimeContext, TraitEffectBase> factory)
    {
        if (string.IsNullOrEmpty(traitId) || factory == null)
        {
            return;
        }

        _effectFactories[traitId] = factory;
    }

    /// <summary>
    /// 从当前 TraitDatabase 重建定义字典（不清空已激活列表与效果）。
    /// </summary>
    public void RebuildDefinitionDictionary()
    {
        _definitionDict.Clear();
        if (traitDatabase == null || traitDatabase.Traits == null)
        {
            return;
        }

        foreach (TraitDefinition def in traitDatabase.Traits)
        {
            if (def == null || string.IsNullOrEmpty(def.TraitId))
            {
                continue;
            }

            if (_definitionDict.ContainsKey(def.TraitId))
            {
                Debug.LogWarning($"TraitManager.RebuildDefinitionDictionary -> 重复的特性编号已跳过：{def.TraitId}");
                continue;
            }

            _definitionDict.Add(def.TraitId, def);
        }
    }

    public void AddTrait(string traitId, TraitOwner owner)
    {
        if (string.IsNullOrEmpty(traitId))
        {
            Debug.LogWarning("TraitManager.AddTrait -> traitId 为空");
            return;
        }

        if (!_definitionDict.ContainsKey(traitId))
        {
            Debug.LogWarning($"TraitManager.AddTrait -> 未找到特性定义：{traitId}");
            return;
        }

        List<string> list = GetTraitIdList(owner);
        if (list.Contains(traitId))
        {
            Debug.Log($"添加特性失败【{traitId}】，该特性已存在");
            return;
        }

        list.Add(traitId);
        Debug.Log($"添加特性成功【{traitId}】");

        string effectKey = MakeEffectInstanceKey(owner, traitId);
        if (_effectFactories.TryGetValue(traitId, out Func<TraitRuntimeContext, TraitEffectBase> factory))
        {
            var ctx = new TraitRuntimeContext(owner, traitId, this);
            TraitEffectBase effect = factory(ctx);
            if (effect != null)
            {
                if (_attachedEffects.ContainsKey(effectKey))
                {
                    _attachedEffects[effectKey].Detach();
                    _attachedEffects.Remove(effectKey);
                }

                _attachedEffects[effectKey] = effect;
                effect.Attach(ctx);
            }
        }
    }

    public void RemoveTrait(string traitId, TraitOwner owner)
    {
        if (string.IsNullOrEmpty(traitId))
        {
            Debug.LogWarning("TraitManager.RemoveTrait -> traitId 为空");
            return;
        }

        List<string> list = GetTraitIdList(owner);
        if (!list.Contains(traitId))
        {
            Debug.Log($"删除特性失败【{traitId}】，该特性不存在");
            return;
        }

        string effectKey = MakeEffectInstanceKey(owner, traitId);
        if (_attachedEffects.TryGetValue(effectKey, out TraitEffectBase effect))
        {
            effect.Detach();
            _attachedEffects.Remove(effectKey);
        }

        list.Remove(traitId);
        Debug.Log($"删除特性成功【{traitId}】");
    }

    public bool ContainsDefinition(string traitId)
    {
        return !string.IsNullOrEmpty(traitId) && _definitionDict.ContainsKey(traitId);
    }

    /// <summary>
    /// 清空定义字典，并移除双方所有已激活特性及其效果。
    /// </summary>
    public void ClearDefinitions()
    {
        DetachAllEffects();
        _playerTraitIds.Clear();
        _enemyTraitIds.Clear();
        _definitionDict.Clear();
    }

    public IEnumerable<TraitDefinition> GetAllDefinitionsSorted()
    {
        return _definitionDict.Values.OrderBy(d => d.TraitId);
    }

    public bool TryGetDefinition(string traitId, out TraitDefinition definition)
    {
        return _definitionDict.TryGetValue(traitId, out definition);
    }

    public void ClearOwnerTraits(TraitOwner owner)
    {
        List<string> list = GetTraitIdList(owner);
        if (list == null || list.Count == 0)
        {
            return;
        }

        string[] snapshot = list.ToArray();
        for (int i = 0; i < snapshot.Length; i++)
        {
            RemoveTrait(snapshot[i], owner);
        }
    }

    public List<string> ExportPlayerTraitIds()
    {
        return new List<string>(_playerTraitIds);
    }

    public void ApplyRestSettlement(RestContext context)
    {
        DispatchPlayerTraitEffects(
            PlayerTraitTrigger.RestSettlement,
            effect => effect.OnRestSettlement(context)
        );
    }

    public void ApplyDayStart(RestContext context)
    {
        DispatchPlayerTraitEffects(
            PlayerTraitTrigger.DayStart,
            effect => effect.OnDayStart(context)
        );
    }

    public void ReplacePlayerTraits(IEnumerable<string> traitIds)
    {
        ClearOwnerTraits(TraitOwner.Player);
        if (traitIds != null)
        {
            foreach (string traitId in traitIds)
            {
                if (string.IsNullOrEmpty(traitId))
                {
                    continue;
                }

                AddTrait(traitId, TraitOwner.Player);
            }
        }

        PushPlayerTraitsToRuntime();
    }

    private void PushPlayerTraitsToRuntime()
    {
        if (PlayerStateManager.Instance == null || PlayerStateManager.Instance.Current == null)
        {
            return;
        }

        PlayerStateManager.Instance.Current.TraitIds = ExportPlayerTraitIds();
    }

    private List<string> GetTraitIdList(TraitOwner owner)
    {
        switch (owner)
        {
            case TraitOwner.Player:
                return _playerTraitIds;
            case TraitOwner.Enemy:
                return _enemyTraitIds;
            default:
                return _playerTraitIds;
        }
    }

    private static string MakeEffectInstanceKey(TraitOwner owner, string traitId)
    {
        return $"{owner}_{traitId}";
    }

    private void DispatchPlayerTraitEffects(PlayerTraitTrigger trigger, Action<PlayerTraitEffectBase> apply)
    {
        if (apply == null)
        {
            return;
        }

        foreach (KeyValuePair<string, TraitEffectBase> pair in _attachedEffects)
        {
            PlayerTraitEffectBase effect = pair.Value as PlayerTraitEffectBase;
            if (effect == null || effect.TriggerKind != trigger)
            {
                continue;
            }

            apply(effect);
        }
    }

    private void DetachAllEffects()
    {
        foreach (KeyValuePair<string, TraitEffectBase> pair in _attachedEffects)
        {
            pair.Value?.Detach();
        }

        _attachedEffects.Clear();
    }

    private void OnDestroy()
    {
        DetachAllEffects();
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
