using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using MoreMountains.Tools;
using MoreMountains.TopDownEngine;
using Random = UnityEngine.Random;

[System.Serializable]
public class EnemyWave
{
    public string waveName;
    public float duration; // Duración de la wave en segundos
    public int enemiesToKill; // Cantidad de enemigos a matar
    public bool isBoss;

    public List<EnemyPool> enemyPools;

    [System.Serializable]
    public class EnemyPool
    {
        [SerializeField] private MMSimpleObjectPooler enemyPooler; // Pool de enemigo específico
        public int enemiesPerSecond; // Cantidad de enemigos spawneados por segundo

        [MinMaxSlider(0, 1)]
        public Vector2
            spawnDurationRange; // Rango de duración del spawneo de este pool en porcentaje de la duración total de la wave

        public MMSimpleObjectPooler EnemyPooler => enemyPooler;
    }
}

public struct LevelCompleted
{
    public static LevelCompleted e;

    public static void Trigger()
    {
        MMEventManager.TriggerEvent(e);
    }
}

public struct LevelFailed
{
    public static LevelFailed e;

    public static void Trigger()
    {
        MMEventManager.TriggerEvent(e);
    }
}

public class EnemySpawner : MonoBehaviour, MMEventListener<EnemyDeathEvent>
{
    public List<EnemyWave> waves; // Lista de waves
    public Transform spawnCenter; // Centro del área de spawneo
    public float spawnRadius = 10f; // Radio del área de spawneo
    public List<Enemy> activeEnemies = new List<Enemy>(); // Lista de scripts Enemy activos
    public int maxEnemies = 120; // Número máximo de enemigos activos permitidos

    private int currentWaveIndex = -1;
    private float waveTimer;
    private bool isSpawning;
    private int enemiesKilledInWave;

    private int currentEnemiesToKill;
    private string formattedTime;
    private List<Coroutine> activeCoroutines = new List<Coroutine>();

    private bool levelFinished;

    void Start()
    {
        StartNextWave();
    }

    void Update()
    {
        if (!isSpawning || levelFinished) return;

        waveTimer -= Time.deltaTime;
        formattedTime = FormatTime(waveTimer);
        UpdateWaveDurationUI();

        // Si el tiempo de la oleada se acaba y quedan muchos enemigos por matar, fallar el nivel
        if (waveTimer <= 0)
        {
            if (enemiesKilledInWave < waves[currentWaveIndex].enemiesToKill)
            {
                OnLevelFailed();
            }
            else
            {
                StartNextWave();
            }
        }
    }

    private void LateUpdate()
    {
        if (activeEnemies.Count > 0)
        {
            foreach (Enemy enemy in activeEnemies)
            {
               // enemy.FollowTarget();
            }
        }
    }

    void StartNextWave()
    {
        // Detener y limpiar todas las corrutinas activas de la oleada anterior
        foreach (var coroutine in activeCoroutines)
        {
            StopCoroutine(coroutine);
        }

        activeCoroutines.Clear();

        currentWaveIndex++;
        if (currentWaveIndex >= waves.Count)
        {
            Debug.Log("All waves completed!");
            OnLevelCompleted();
            isSpawning = false;
            return;
        }

        enemiesKilledInWave = 0;
        EnemyWave currentWave = waves[currentWaveIndex];
        waveTimer = currentWave.duration;
        currentEnemiesToKill = waves[currentWaveIndex].enemiesToKill;
        activeCoroutines.Add(StartCoroutine(SpawnEnemies(currentWave)));
        isSpawning = true;
        UpdateWaveNumberUI();
        UpdateWaveProgressionUI();
    }

    IEnumerator SpawnEnemies(EnemyWave wave)
    {
        List<IEnumerator> spawningCoroutines = new List<IEnumerator>();

        foreach (var pool in wave.enemyPools)
        {
            float spawnStartTime = wave.duration * pool.spawnDurationRange.x;
            float spawnEndTime = wave.duration * pool.spawnDurationRange.y;

            spawningCoroutines.Add(SpawnEnemiesFromPool(pool, spawnStartTime, spawnEndTime, wave.duration));
        }

        foreach (var coroutine in spawningCoroutines)
        {
            activeCoroutines.Add(StartCoroutine(coroutine));
        }

        yield return new WaitForSeconds(wave.duration);
    }

    IEnumerator SpawnEnemiesFromPool(EnemyWave.EnemyPool pool, float startTime, float endTime, float waveDuration)
    {
        yield return new WaitForSeconds(startTime);

        float spawnInterval = 1f / pool.enemiesPerSecond;
        float spawnTimer = endTime - startTime;
        float remainingWaveTime = waveDuration - startTime; // Tiempo restante de la wave desde el comienzo del spawn

        while (spawnTimer > 0)
        {
            if (activeEnemies.Count < maxEnemies)
            {
                spawnTimer -= spawnInterval;
                GameObject enemyObject = pool.EnemyPooler.GetPooledGameObject();
                if (enemyObject != null)
                {
                    Vector3 spawnPosition = GetRandomSpawnPosition();
                    enemyObject.transform.position = spawnPosition;

                    // Ajustar la rotación del enemigo para que mire hacia el centro del spawner
                    Vector3 direction = (spawnCenter.position - spawnPosition).normalized;
                    enemyObject.transform.rotation = Quaternion.LookRotation(direction);

                    enemyObject.SetActive(true);

                    Enemy enemyScript = enemyObject.GetComponent<Enemy>();
                    if (enemyScript != null)
                    {
                        activeEnemies.Add(enemyScript); // Añadir el script Enemy a la lista de enemigos activos
                        // Desactivar el enemigo después del tiempo restante
                        StartCoroutine(DeactivateAfterTime(enemyScript, remainingWaveTime));
                    }
                }

                yield return new WaitForSeconds(spawnInterval);
            }
            else
            {
                // Si se supera el máximo de enemigos, espera un poco antes de intentar spawnear de nuevo
                yield return new WaitForSeconds(1f);
            }
        }
    }

    Vector3 GetRandomSpawnPosition()
    {
        // Calcular la posición en el perímetro del radio alrededor del centro de spawneo
        float angle = Random.Range(0f, 2f * Mathf.PI);
        float x = Mathf.Cos(angle) * spawnRadius;
        float z = Mathf.Sin(angle) * spawnRadius;

        return new Vector3(spawnCenter.position.x + x, spawnCenter.position.y, spawnCenter.position.z + z);
    }

    IEnumerator DeactivateAfterTime(Enemy enemyScript, float time)
    {
        yield return new WaitForSeconds(time);
        if (enemyScript != null)
        {
            activeEnemies.Remove(enemyScript); // Quitar enemigo de la lista de enemigos activos
            enemyScript.gameObject.SetActive(false);
        }
    }

    public void OnMMEvent(EnemyDeathEvent eventType)
    {
        UpdateWaveProgression(eventType);
        UpdateWaveProgressionUI();
    }

    private void UpdateWaveProgression(EnemyDeathEvent eventType)
    {
        activeEnemies.Remove(eventType.Enemy);
        enemiesKilledInWave++;
        currentEnemiesToKill--;

        if (currentEnemiesToKill <= 0)
        {
            isSpawning = false;
            Debug.Log("Wave completed!");
            StartNextWave();
        }
    }

    private int GetWaveProgression()
    {
        return currentEnemiesToKill;
    }

    private void UpdateWaveProgressionUI()
    {
        if(levelFinished) return;
        if (GUIManager.Instance != null)
            GUIManager.Instance.UpdateWaveProgression(GetWaveProgression());
    }

    private void UpdateWaveNumberUI()
    {
        if(levelFinished) return;
        if (GUIManager.Instance != null)
            GUIManager.Instance.UpdateWaveNumber(currentWaveIndex + 1);
    }

    private void UpdateWaveDurationUI()
    {
        if(levelFinished) return;
        if (GUIManager.Instance != null)
            GUIManager.Instance.UpdateWaveTime(formattedTime);
    }

    private string FormatTime(float time)
    {
        if (time < 0) return "TIME OVER";
        int minutes = Mathf.FloorToInt(time / 60);
        int seconds = Mathf.FloorToInt(time % 60);
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    private void OnEnable()
    {
        this.MMEventStartListening<EnemyDeathEvent>();
    }

    private void OnDisable()
    {
        this.MMEventStopListening<EnemyDeathEvent>();
    }

    private void OnLevelCompleted()
    {
        Debug.Log("Level Completed!");
        LevelCompleted.Trigger();
        if(GUIManager.Instance is not null)
            GUIManager.Instance.ShowWinPanel();
    }

    private void OnLevelFailed()
    {
        
        isSpawning = false;
        levelFinished = true;
        Debug.Log("Level Failed!");
        LevelFailed.Trigger();
        if(GUIManager.Instance is not null)
            GUIManager.Instance.ShowLosePanel();
 
    }
}