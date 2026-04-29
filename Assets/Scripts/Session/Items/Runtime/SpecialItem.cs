using System.Collections.Generic;
using UnityEngine;

public class SpecialItem : ItemBase
{
    [Header("剧情标签")]
    [SerializeField] private List<string> tags = new List<string>();

    public IReadOnlyList<string> Tags => tags;
}

