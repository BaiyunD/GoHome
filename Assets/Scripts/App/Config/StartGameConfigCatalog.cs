using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StartGameConfigCatalog", menuName = "GoHome/Start Game Config Catalog")]
public class StartGameConfigCatalog : ScriptableObject
{
    [SerializeField] private List<StartGameConfig> presets = new List<StartGameConfig>();

    public IReadOnlyList<StartGameConfig> Presets => presets;
}

