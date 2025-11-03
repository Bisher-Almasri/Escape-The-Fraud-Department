using UnityEngine;

public class EnemyDeathTracker : MonoBehaviour
{
    [HideInInspector]
    public ContinuousEnemySpawner spawner;

    void OnDestroy()
    {
        if (spawner != null)
        {
            spawner.OnEnemyKilled();
        }
    }
}