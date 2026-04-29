using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TraitDatabase", menuName = "GoHome/Trait Database")]
public class TraitDatabase : ScriptableObject
{
    [SerializeField] private List<TraitDefinition> traits = new List<TraitDefinition>();

    public IReadOnlyList<TraitDefinition> Traits => traits;
}
