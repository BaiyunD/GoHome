using System;

public readonly struct RegionActionContext
{
    public RegionActionContext(int mainRegionIndex, int subRegionIndex, int playerEnergy, int playerHunger)
    {
        MainRegionIndex = mainRegionIndex;
        SubRegionIndex = subRegionIndex;
        PlayerEnergy = playerEnergy;
        PlayerHunger = playerHunger;
    }

    public int MainRegionIndex { get; }
    public int SubRegionIndex { get; }
    public int PlayerEnergy { get; }
    public int PlayerHunger { get; }
}

public readonly struct RegionNodeContract : IEquatable<RegionNodeContract>
{
    public RegionNodeContract(string mainRegionId, int mainRegionIndex, string subRegionId, int subRegionIndex)
    {
        if (string.IsNullOrWhiteSpace(mainRegionId))
        {
            throw new ArgumentException("mainRegionId 不能为空", nameof(mainRegionId));
        }

        if (string.IsNullOrWhiteSpace(subRegionId))
        {
            throw new ArgumentException("subRegionId 不能为空", nameof(subRegionId));
        }

        MainRegionId = mainRegionId.Trim();
        MainRegionIndex = mainRegionIndex;
        SubRegionId = subRegionId.Trim();
        SubRegionIndex = subRegionIndex;
        RegionCode = $"{MainRegionIndex}_{SubRegionIndex}";
    }

    public string MainRegionId { get; }
    public int MainRegionIndex { get; }
    public string SubRegionId { get; }
    public int SubRegionIndex { get; }
    public string RegionCode { get; }

    public bool Equals(RegionNodeContract other)
    {
        return string.Equals(RegionCode, other.RegionCode, StringComparison.Ordinal);
    }

    public override bool Equals(object obj)
    {
        return obj is RegionNodeContract other && Equals(other);
    }

    public override int GetHashCode()
    {
        return RegionCode != null ? StringComparer.Ordinal.GetHashCode(RegionCode) : 0;
    }
}
