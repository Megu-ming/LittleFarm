using UnityEngine;

[CreateAssetMenu(fileName = "BD_", menuName = "Game/BuildingData")]
public class BuildingData : ScriptableObject
{
    [Header("Prefab")]
    [SerializeField] GameObject _prefab;
    public GameObject Prefab => _prefab;

    [Header("Footprint(tiles)")]
    [Min(1)][SerializeField] int _width = 1;
    [Min(1)][SerializeField] int _height = 1;
    public int Width => _width;
    public int Height => _height;

    [Header("Placement")]
    [SerializeField] Vector3 _extraOffset = Vector3.zero;
    public Vector3 ExtraOffset => _extraOffset;
}
