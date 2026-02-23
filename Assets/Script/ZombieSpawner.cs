using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class WaveEnemyConfig
{
    [Tooltip("Identificador opcional para organizar (ej: 'Zombies Rápidos')")]
    public string note = "Grupo";
    [Tooltip("El prefab del enemigo a generar")]
    public GameObject enemyPrefab; 
    [Tooltip("Cantidad de enemigos de este tipo en este grupo")]
    public int count = 5;
    [Tooltip("Tiempo de espera (segundos) ANTES de empezar a generar este grupo")]
    public float initialDelay = 2f; 
    [Tooltip("Tiempo de espera (segundos) entre cada enemigo de este grupo")]
    public float timeBetweenSpawns = 1f;
}

[System.Serializable]
public class Wave
{
    public string name = "Oleada";
    [Tooltip("Lista de grupos de enemigos que aparecerán en orden")]
    public List<WaveEnemyConfig> enemies = new List<WaveEnemyConfig>(); // Renombrado de vuelta a 'enemies' para recuperar datos
    public float difficultyMultiplier = 1.0f;
}

public class ZombieSpawner : MonoBehaviour
{
    [Header("Configuración Global")]
    public GameObject defaultZombiePrefab;
    public Transform[] spawnPoints;
    public float initialSpawnDelay = 3f;
    public float timeBetweenWaves = 5f;
    public int maxZombiesAlive = 20;

    [Header("Configuración de Oleadas")]
    public List<Wave> waves = new List<Wave>();
    public bool loopWaves = true;
    public float endlessDifficultyMultiplier = 0.2f;

    private int currentWaveIndex = 0;
    private int zombiesAlive = 0;
    private GameManager gameManager;
    private int endlessWaveCount = 0;

    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();

        if (spawnPoints.Length == 0)
        {
            Debug.LogWarning("No hay puntos de spawn asignados. Usando la posición del spawner.");
            spawnPoints = new Transform[] { transform };
        }

        if (maxZombiesAlive <= 0) maxZombiesAlive = 20;

        // Validación de seguridad para evitar estancamiento
        if (waves.Count == 0 && defaultZombiePrefab != null)
        {
            Debug.Log("No hay oleadas configuradas. Creando oleada por defecto.");
            Wave defaultWave = new Wave();
            defaultWave.name = "Oleada Inicial";
            defaultWave.enemies.Add(new WaveEnemyConfig { 
                enemyPrefab = defaultZombiePrefab, 
                count = 5, 
                initialDelay = 1f, 
                timeBetweenSpawns = 2f 
            });
            waves.Add(defaultWave);
        }
        else if (waves.Count > 0)
        {
            // Validar que la primera oleada tenga enemigos
            if (waves[0].enemies.Count == 0)
            {
                Debug.LogError("¡La Oleada 0 no tiene enemigos configurados! Añade grupos a la lista 'Enemies'.");
                if (defaultZombiePrefab != null)
                {
                    waves[0].enemies.Add(new WaveEnemyConfig { enemyPrefab = defaultZombiePrefab, count = 5 });
                }
            }
        }

        StartCoroutine(StartGameRoutine());
    }

    IEnumerator StartGameRoutine()
    {
        Debug.Log($"Iniciando juego en {initialSpawnDelay} segundos...");
        yield return new WaitForSeconds(initialSpawnDelay);

        while (true)
        {
            yield return StartCoroutine(SpawnWaveRoutine());
            yield return new WaitForSeconds(timeBetweenWaves);
        }
    }

    IEnumerator SpawnWaveRoutine()
    {
        List<WaveEnemyConfig> groupsToSpawn = new List<WaveEnemyConfig>();
        float currentDifficulty = 1.0f;
        int waveNumberDisplay = currentWaveIndex + 1;
        string waveNameDisplay = "";

        // 1. CONFIGURAR OLEADA
        if (currentWaveIndex < waves.Count)
        {
            Wave waveConfig = waves[currentWaveIndex];
            waveNameDisplay = waveConfig.name;
            currentDifficulty = waveConfig.difficultyMultiplier;
            groupsToSpawn = waveConfig.enemies; // Usando 'enemies'
            
            if (groupsToSpawn.Count == 0) Debug.LogWarning($"La oleada {waveNameDisplay} no tiene grupos de enemigos.");
        }
        else if (loopWaves)
        {
            // OLEADA INFINITA
            waveNameDisplay = "Oleada Infinita " + (endlessWaveCount + 1);
            waveNumberDisplay = waves.Count + endlessWaveCount + 1;
            currentDifficulty = 1.0f + (endlessWaveCount * endlessDifficultyMultiplier);

            if (waves.Count > 0)
            {
                Wave lastWave = waves[waves.Count - 1];
                foreach (var group in lastWave.enemies)
                {
                    WaveEnemyConfig newGroup = new WaveEnemyConfig();
                    newGroup.enemyPrefab = group.enemyPrefab;
                    newGroup.note = group.note + " (Inf)";
                    newGroup.count = Mathf.CeilToInt(group.count * (1 + (endlessWaveCount * 0.2f))); 
                    newGroup.initialDelay = group.initialDelay;
                    newGroup.timeBetweenSpawns = Mathf.Max(0.2f, group.timeBetweenSpawns * 0.9f); 
                    
                    groupsToSpawn.Add(newGroup);
                }
            }
            else if (defaultZombiePrefab != null)
            {
                groupsToSpawn.Add(new WaveEnemyConfig {
                    enemyPrefab = defaultZombiePrefab,
                    count = 10 + (endlessWaveCount * 2),
                    initialDelay = 1f,
                    timeBetweenSpawns = 1f
                });
            }
        }
        else
        {
            Debug.Log("Juego Completado.");
            yield break;
        }

        if (gameManager != null) gameManager.NewWave(waveNumberDisplay);

        Debug.Log($"Iniciando {waveNameDisplay} con {groupsToSpawn.Count} grupos.");

        // 2. SPAWNEAR GRUPOS SECUENCIALMENTE
        foreach (WaveEnemyConfig group in groupsToSpawn)
        {
            if (group.enemyPrefab == null)
            {
                Debug.LogWarning("Grupo con Prefab nulo ignorado.");
                continue;
            }

            // Espera inicial del grupo
            if (group.initialDelay > 0) yield return new WaitForSeconds(group.initialDelay);

            for (int i = 0; i < group.count; i++)
            {
                // Esperar si hay demasiados zombies
                if (zombiesAlive >= maxZombiesAlive)
                {
                    // Debug.Log("Esperando espacio para spawnear...");
                    while (zombiesAlive >= maxZombiesAlive) yield return new WaitForSeconds(0.5f);
                }

                SpawnEnemy(group.enemyPrefab, currentDifficulty);

                if (group.timeBetweenSpawns > 0) yield return new WaitForSeconds(group.timeBetweenSpawns);
            }
        }

        // 3. ESPERAR A QUE TERMINE LA OLEADA (TODOS MUERTOS)
        while (zombiesAlive > 0)
        {
            yield return new WaitForSeconds(1f);
        }

        // Avanzar índices
        if (currentWaveIndex < waves.Count)
        {
            currentWaveIndex++;
        }
        else
        {
            endlessWaveCount++;
        }
    }

    void SpawnEnemy(GameObject prefab, float difficultyMultiplier)
    {
        if (prefab == null) return;

        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        GameObject enemy = Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);

        // Debug.Log($"Spawning {enemy.name} (Dif: {difficultyMultiplier})");

        // Ajustar Salud
        ZombieHealth health = enemy.GetComponent<ZombieHealth>();
        if (health != null)
        {
            health.maxHealth *= difficultyMultiplier;
            health.currentHealth = health.maxHealth;
        }

        // Ajustar Daño
        ZombieAI ai = enemy.GetComponent<ZombieAI>();
        if (ai != null)
        {
            ai.damage *= difficultyMultiplier;
        }
        
        zombiesAlive++;
    }

    public void ZombieDied()
    {
        zombiesAlive = Mathf.Max(0, zombiesAlive - 1);
    }

    public int GetZombiesAlive()
    {
        return zombiesAlive;
    }
}
