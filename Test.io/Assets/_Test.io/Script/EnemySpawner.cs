using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using UnityEngine;
using System.Collections.Generic;
using MoreMountains.Tools;
using Random = UnityEngine.Random;

[System.Serializable]
public class EnemyWave
{
    public string waveName;
    public float duration; // Duración de la wave en segundos
    
    public List<EnemyPool> enemyPools;

    [System.Serializable]
    public class EnemyPool
    {
        [SerializeField] private MMSimpleObjectPooler enemyPooler; // Pool de enemigo específico
        public int enemiesPerSecond; // Cantidad de enemigos spawneados por segundo

        [MinMaxSlider(0, 1)]
        public Vector2 spawnDurationRange; // Rango de duración del spawneo de este pool en porcentaje de la duración total de la wave

        public MMSimpleObjectPooler EnemyPooler => enemyPooler;
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

    void Start()
    {
        StartNextWave();
    }

    void Update()
    {
        if (!isSpawning) return;

        waveTimer -= Time.deltaTime;

        if (waveTimer <= 0)
        {
            StartNextWave();
        }
    }

    private void LateUpdate()
    {
        if (activeEnemies.Count > 0)
        {
            foreach (Enemy enemy in activeEnemies)
            {
                enemy.FollowTarget();
            }
        }
    }

    void StartNextWave()
    {
        currentWaveIndex++;
        if (currentWaveIndex >= waves.Count)
        {
            Debug.Log("All waves completed!");
            isSpawning = false;
            return;
        }

        EnemyWave currentWave = waves[currentWaveIndex];
        waveTimer = currentWave.duration;
        StartCoroutine(SpawnEnemies(currentWave));
        isSpawning = true;
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
            StartCoroutine(coroutine);
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
                    enemyObject.transform.position = GetRandomSpawnPosition();
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
        activeEnemies.Remove(eventType.Enemy);
    }

    private void OnEnable()
    {
        this.MMEventStartListening<EnemyDeathEvent>();
    }

    private void OnDisable()
    {
        this.MMEventStopListening<EnemyDeathEvent>();
    }
}
