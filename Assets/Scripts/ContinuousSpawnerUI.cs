using UnityEngine;
using TMPro;

public class ContinuousSpawnerUI : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI enemiesAliveText;
    public TextMeshProUGUI enemiesKilledText;
    public TextMeshProUGUI difficultyText;
    
    private ContinuousEnemySpawner spawner;

    void Start()
    {
        spawner = FindObjectOfType<ContinuousEnemySpawner>();
        
        if (spawner == null)
        {
            Debug.LogWarning("ContinuousSpawnerUI: No ContinuousEnemySpawner found in scene!");
        }
    }

    void Update()
    {
        if (spawner == null) return;
        
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (enemiesAliveText != null)
        {
            enemiesAliveText.text = $"Enemies Alive: {spawner.ActiveEnemyCount}";
        }
        
        if (enemiesKilledText != null)
        {
            enemiesKilledText.text = $"Enemies Killed: {spawner.EnemiesKilled}";
        }
        
        if (difficultyText != null)
        {
            difficultyText.text = $"Difficulty: {spawner.HealthMultiplier:F1}x";
        }
    }
}