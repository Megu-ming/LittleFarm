using UnityEngine;

public static class SpawnResolver
{
    public static void ApplyPendingSpawn(Transform playertransform)
    {
        var id = SceneTransitionData.PendingSpawnId;
        if (string.IsNullOrEmpty(id))
            return;

        var points = Object.FindObjectsByType<SpawnPoint>(FindObjectsSortMode.None);
        for(int i = 0; i < points.Length; i++)
        {
            if (points[i].Id == id)
            {
                playertransform.SetPositionAndRotation(points[i].transform.position, points[i].transform.rotation);
                SceneTransitionData.PendingSpawnId = null;
                return;
            }
        }

        Debug.LogWarning($"[SpawnResolver] SpawnPoint with id '{id}' not found.");
        SceneTransitionData.PendingSpawnId = null;
    }
}
