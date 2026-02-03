using UnityEngine;

[System.Serializable]
public class PlayerSaveData
{
    Vector3 _position = Vector3.zero;
    Quaternion _rotation = Quaternion.identity;

    int _currentHandIndex = -1;
    int _currentHandItemId = -1;

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
