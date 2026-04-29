using UnityEngine;

[CreateAssetMenu(fileName = "NewTrait", menuName = "GoHome/Trait Definition")]
public class TraitDefinition : ScriptableObject
{
    [Header("标识")]
    [SerializeField] private string traitId;

    [Header("基础信息")]
    [SerializeField] private string displayName;
    [TextArea(2, 6)]
    [SerializeField] private string description;
    [SerializeField] private TraitSource source;

    [Header("可选")]
    [Tooltip("同组互斥等规则可后续在 TraitManager 中使用")]
    [SerializeField] private string traitGroup;

    public string TraitId => traitId;
    public string DisplayName => displayName;
    public string Description => description;
    public TraitSource Source => source;
    public string TraitGroup => traitGroup;
}
