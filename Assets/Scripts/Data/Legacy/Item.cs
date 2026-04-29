using UnityEngine;

// Legacy item asset type kept for backward compatibility.
[CreateAssetMenu(fileName = "LegacyItem", menuName = "GoHome/Legacy Item")]
public class Item : ScriptableObject
{
    public int itemId;
    public string itemName;
}
