using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 包含更低一层扇区的索引列表
/// </summary>
[System.Serializable]
public class SectorIndexList 
{
    public List<ushort> lowSectorIndex = new List<ushort>();
    public SectorIndexList() { }

    public void AddLowSectorIndex(ushort index)
    {
        lowSectorIndex.Add(index);
    }
}
