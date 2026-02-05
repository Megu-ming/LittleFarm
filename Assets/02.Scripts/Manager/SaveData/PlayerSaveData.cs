using UnityEngine;

[System.Serializable]
public class PlayerSaveData
{
    [SerializeField] Vector3 _position = Vector3.zero;
    [SerializeField] Quaternion _rotation = Quaternion.identity;

    [SerializeField] int _currentHandIndex = -1;
    [SerializeField] int _currentHandItemId = -1;

    public Vector3 Position => _position;
    public Quaternion Rotation => _rotation;
    public int CurrentHandIndex => _currentHandIndex;
    public int CurrentHandItemId => _currentHandItemId;

    public void SetPlayerTransform(Vector3 position, Quaternion rotation)
    {
        _position = position;
        _rotation = rotation;
    }
}
