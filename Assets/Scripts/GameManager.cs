using UnityEngine;
using MGUtilities;
using System.Collections.Generic;
using TMPro;
using System.IO;
public class GameManager : Singleton_template<GameManager>
{
    [SerializeField] private bool m_loop;
    [Header("Enemys")]
    [SerializeField] private int[] m_maxTotalEnemiesPerLevel;
    [SerializeField] private int[] m_maxActiveEnemiesPerLevel;
    [SerializeField] private Vector2 m_spawnDistance;
    [SerializeField] private Enemy m_enemyprefab;
    [Header("Text")]
    [SerializeField] private TextMeshProUGUI m_enemiesRemainingText;
    [SerializeField] private TextMeshProUGUI m_playerScoreText;
    [SerializeField] private TextMeshProUGUI m_levelText;
    [Space]
    [SerializeField] private TextMeshProUGUI m_penetrationText;
    [SerializeField] private TextMeshProUGUI m_bulletSpeedText;
    [SerializeField] private TextMeshProUGUI m_damageText;
    [SerializeField] private TextMeshProUGUI m_rotateSpeedText;
    [SerializeField] private TextMeshProUGUI m_fireRateText;
    [Space]
    [SerializeField] private TextMeshProUGUI m_penetrationStatsText;
    [SerializeField] private TextMeshProUGUI m_bulletSpeedStatsText;
    [SerializeField] private TextMeshProUGUI m_damageStatsText;
    [SerializeField] private TextMeshProUGUI m_rotateSpeedStatsText;
    [SerializeField] private TextMeshProUGUI m_fireRateStatsText;
    [Space]
    [SerializeField] private TextMeshProUGUI[] m_turretsText;
    [Header("Upgrades")]
    [SerializeField] private float m_penetrationCost;
    [SerializeField] private float m_bulletSpeedCost;
    [SerializeField] private float m_damageCost;
    [SerializeField] private float m_rotateSpeedCost;
    [SerializeField] private float m_fireRateCost;
    [Header("Turrets")]
    [SerializeField] private Turret[] m_turrets;
    [SerializeField] private int[] m_turretCosts;
    [Header("Menus")]
    [SerializeField] private GameObject m_gameOverScreen;
    [Header("SFX")]
    [SerializeField] private float m_enemyVolume;
    [SerializeField] protected AudioClip m_enemyClip;
    private float m_playerScore;
    private int m_level = 0;
    private int m_numEnemies;
    private int m_numEnemiesThisLevel;
    private Tower m_tower;
    private AudioPool m_audioPool;
    private List<Enemy> m_reserveEnemies = new();
    private List<Enemy> m_activeEnemies = new();
    private string m_path;
    void Start()
    {
        m_tower = Tower.Instance();
        m_audioPool = AudioPool.Instance();
        for (int i = 0; i < m_maxActiveEnemiesPerLevel[0]; i++)
        {
            SpawnEnemy();
        }

        m_path = Path.Combine(Application.dataPath, "Costs.txt");
        if (!File.Exists(m_path))
        {
            string defaultContent = $"{0},{0},{0},{0},{0},{0}";
            File.WriteAllText(m_path, defaultContent);
        }
        ReadStats();

        for (int i = 0; i < m_turretsText.Length; i++)
        {
            m_turretsText[i].text = $"Cost: {m_turretCosts[i]}";
        }

        if (m_penetrationText) m_penetrationText.text = $"Penetration Moddifier: {m_tower.Penetration}\nCost: {m_penetrationCost}";
        if (m_penetrationStatsText) m_penetrationStatsText.text = $"Penetration: {m_tower.CurrentTurret.Stats.Item1}";

        if (m_bulletSpeedText) m_bulletSpeedText.text = $"BulletSpeed Multi: {m_tower.BulletSpeed}\nCost: {m_bulletSpeedCost}";
        if (m_bulletSpeedStatsText) m_bulletSpeedStatsText.text = $"BulletSpeed: {m_tower.CurrentTurret.Stats.Item2}";

        if (m_damageText) m_damageText.text = $"Damage Multi: {m_tower.Damage}\nCost: {m_damageCost}";
        if (m_damageStatsText) m_damageStatsText.text = $"Damage: {m_tower.CurrentTurret.Stats.Item3}";

        if (m_rotateSpeedText) m_rotateSpeedText.text = $"RotateSpeed Multi: {m_tower.RotateSpeed }\nCost: {m_rotateSpeedCost}";
        if (m_rotateSpeedStatsText) m_rotateSpeedStatsText.text = $"RotateSpeed: {m_tower.CurrentTurret.Stats.Item4}";

        if (m_fireRateText) m_fireRateText.text = $"FireRate Multi: {m_tower.FireRate}\nCost: {m_fireRateCost}";
        if (m_fireRateStatsText) m_fireRateStatsText.text = $"FireRate: {m_tower.CurrentTurret.Stats.Item5}";
    }
    // UPGRADES //
    public void BuyTurret(int index)
    {
        if (m_playerScore < m_turretCosts[index]) return;
        m_playerScore -= m_turretCosts[index];
        m_tower.CurrentTurret = m_turrets[index];
    }
    public void UpgradePenetration(int ammount)
    {
        if (m_playerScore < m_penetrationCost || m_tower.CurrentTurret.MaxPenetration) return;
        m_playerScore -= m_penetrationCost;
        m_tower.Penetration += ammount;
        m_tower.UpdateTurretStats();
        m_penetrationCost *= 2;
        m_penetrationText.text = $"Penetration Moddifier: {m_tower.Penetration}\nCost: {m_penetrationCost}";
        m_penetrationStatsText.text = $"Penetration: {m_tower.CurrentTurret.Stats.Item1}";
    }
    public void UpgradeBulletSpeed(float ammount)
    {
        if (m_playerScore < m_bulletSpeedCost || m_tower.CurrentTurret.MaxBulletSpeed) return;
        m_playerScore -= m_bulletSpeedCost;
        m_tower.BulletSpeed += ammount;
        m_tower.UpdateTurretStats();
        m_bulletSpeedCost *= 2;
        m_bulletSpeedText.text = $"BulletSpeed Multi: {m_tower.BulletSpeed}\nCost: {m_bulletSpeedCost}";
        m_bulletSpeedStatsText.text = $"BulletSpeed: {m_tower.CurrentTurret.Stats.Item2}";
    }
    public void UpgradeDamage(float ammount)
    {
        if (m_playerScore < m_damageCost || m_tower.CurrentTurret.MaxDamage) return;
        m_playerScore -= m_damageCost;
        m_tower.Damage += ammount;
        m_tower.UpdateTurretStats();
        m_damageCost *= 2;
        m_damageText.text = $"Damage Multi: {m_tower.Damage}\nCost: {m_damageCost}";
        m_damageStatsText.text = $"Damage: {m_tower.CurrentTurret.Stats.Item3}";
    }
    public void UpgradeRotateSpeed(float ammount)
    {
        if (m_playerScore < m_rotateSpeedCost || m_tower.CurrentTurret.MaxRotateSpeed) return;
        m_playerScore -= m_rotateSpeedCost;
        m_tower.RotateSpeed += ammount;
        m_tower.UpdateTurretStats();
        m_rotateSpeedCost *= 2;
        m_rotateSpeedText.text = $"RotateSpeed Multi: {m_tower.RotateSpeed}\nCost: {m_rotateSpeedCost}";
        m_rotateSpeedStatsText.text = $"RotateSpeed: {m_tower.CurrentTurret.Stats.Item4}";
    }
    public void UpgradeFireRate(float ammount)
    {
        if (m_playerScore < m_fireRateCost || m_tower.CurrentTurret.MaxFireRate) return;
        m_playerScore -= m_fireRateCost;
        m_tower.FireRate += ammount;
        m_tower.UpdateTurretStats();
        m_fireRateCost *= 2;
        m_fireRateText.text = $"FireRate Multi: {m_tower.FireRate}\nCost: {m_fireRateCost}";
        m_fireRateStatsText.text = $"FireRate: {m_tower.CurrentTurret.Stats.Item5}";
    }
    // UPGRADES //
    private void GameOver()
    {
        m_gameOverScreen.SetActive(true);
        string content = $"{m_penetrationCost},{m_bulletSpeedCost},{m_damageCost},{m_rotateSpeedCost},{m_fireRateCost},{m_playerScore}";
        File.WriteAllText(m_path, content);
    }
    private void ReadStats()
    {
        string content = File.ReadAllText(m_path);
        string[] values = content.Split(',');
        if (values.Length >= 5)
        {
            m_penetrationCost = float.Parse(values[0]);
            m_bulletSpeedCost = float.Parse(values[1]);
            m_damageCost = float.Parse(values[2]);
            m_rotateSpeedCost = float.Parse(values[3]);
            m_fireRateCost = float.Parse(values[4]);
            m_playerScore = float.Parse(values[5]);
        }
        else Debug.LogWarning("Invalid data in file: not enough values.");
    }
    void Update()
    {
        if (m_tower.IsDead)
        {
            GameOver();
            return;
        }
        if (m_enemiesRemainingText) m_enemiesRemainingText.text = $"Enemies: {m_numEnemies}/{m_maxTotalEnemiesPerLevel[m_level] - (m_numEnemiesThisLevel - m_numEnemies)}";
        if (m_playerScoreText) m_playerScoreText.text = $"Score: {m_playerScore}";
        if (m_levelText) m_levelText.text = $"Level: {m_level + 1}";

        if (m_numEnemies < m_maxActiveEnemiesPerLevel[m_level] && m_numEnemiesThisLevel < m_maxTotalEnemiesPerLevel[m_level]) SpawnEnemy();
        if (m_numEnemies == 0 && m_numEnemiesThisLevel == m_maxTotalEnemiesPerLevel[m_level])
        {
            m_numEnemiesThisLevel = 0;
            if (!m_loop) m_level++;
        }

        foreach (Enemy e in new List<Enemy>(m_activeEnemies))
        {
            if (e == null) continue;
            if (e.IsDead)
            {
                ReturnEnemy(e);
                continue;
            }
            e.RunUpdate();
        }
    }
    private void SpawnEnemy()
    {
        if (m_reserveEnemies.Count <= 0) AddEnemy();
        Vector2 spawnPos = Random.Range(m_spawnDistance.x, m_spawnDistance.y) * new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
        Enemy e = m_reserveEnemies[^1];
        e.SetHP((m_level + 1) * 10f);
        e.transform.SetPositionAndRotation(spawnPos, Quaternion.identity);
        m_reserveEnemies.Remove(e);
        m_activeEnemies.Add(e);
        e.gameObject.SetActive(true);
        m_numEnemies++;
        m_numEnemiesThisLevel++;
    }
    private void AddEnemy()
    {
        m_reserveEnemies.Add(Instantiate(m_enemyprefab, transform));
    }
    private void ReturnEnemy(Enemy e)
    {
        m_activeEnemies.Remove(e);
        m_reserveEnemies.Add(e);
        e.gameObject.SetActive(false);
        m_numEnemies--;
        m_playerScore += e.ScoreOnDeath * (m_level + 1);
        ResetEnemy(e);
        m_audioPool.SpawnAudioSource(transform.position, m_enemyClip, m_enemyVolume);
    }
    private void ResetEnemy(Enemy e)
    {
        e.ResetEnemy();
    }
}