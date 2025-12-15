using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 농작물 & 자랄 수 있는 나무들을 관리하는 작물 매니저
/// </summary>
public class CropManager : MonoBehaviour
{
    GameTimeManager _timeManager;
    readonly List<FarmTile> _tiles = new List<FarmTile>();

    public void Initialize(GameTimeManager timeManager, GridManager gridManager)
    {
        _timeManager = timeManager;

        _timeManager.OnDateChanged += HandleNewDay;

        _tiles.Clear();

        var tiles = gridManager.Tiles;
        if(tiles!=null)
        {
            int width  = tiles.GetLength(0);
            int height = tiles.GetLength(1);
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    _tiles.Add(tiles[x, y]);
        }
    }

    void HandleNewDay(int year, Season season, int day)
    {
        foreach(var tile in _tiles)
        {
            if (tile != null)
                tile.AdvancedGrowthOneDay();
        }
    }
}
