using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ZombieSpawner : MonoBehaviour
{
    [Header("Configuración de Spawn")]
    public GameObject zombiePrefab;
    public Transform[] spawnPoints;
    public float initialSpawnDelay = 3f;
    public float spawnInterval = 5f;
    public int maxZombiesAlive = 10;
    
    [Header("Dificultad Progresiva")]
    public bool increaseDifficulty = true;
    public float difficultyIncreaseRate = 0.1f;
    public int zombiesPerWave = 3;
    public int maxZombiesPerWave = 15;
    
    [Header("Sistema de Oleadas")]
    public bool useWaveSystem = true;
    public float timeBetweenWaves = 10f;
    
    private int currentWave = 0;
    private int zombiesSpawnedThisWave = 0;
    private int zombiesAlive = 0;
    private bool isSpawning = false;
    private GameManager gameManager;
    
    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        
        if (spawnPoints.Length == 0)
        {
            Debug.LogWarning("No hay puntos de spawn asignados. Usando la posición del spawner.");
            spawnPoints = new Transform[] { transform };
        }
        
        StartCoroutine(SpawnRoutine());
    }
    
    IEnumerator SpawnRoutine()
    {
        yield return new WaitForSeconds(initialSpawnDelay);
        
        while (true)
        {
            if (useWaveSystem)
            {
                yield return StartCoroutine(SpawnWave());
                yield return new WaitForSeconds(timeBetweenWaves);
            }
            else
            {
                if (zombiesAlive < maxZombiesAlive)
                {
                    SpawnZombie();
                }
                yield return new WaitForSeconds(spawnInterval);
            }
        }
    }
    
    IEnumerator SpawnWave()
    {
        currentWave++;
        int zombiesToSpawn = Mathf.Min(zombiesPerWave + (currentWave - 1), maxZombiesPerWave);
        zombiesSpawnedThisWave = 0;
        
        if (gameManager != null)
        {
            gameManager.NewWave(currentWave);
        }
        
        Debug.Log("¡Oleada " + currentWave + "! Zombies: " + zombiesToSpawn);
        
        for (int i = 0; i < zombiesToSpawn; i++)
        {
            if (zombiesAlive < maxZombiesAlive)
            {
                SpawnZombie();
                zombiesSpawnedThisWave++;
                yield return new WaitForSeconds(0.5f);
            }
        }
        
        // Esperar a que mueran todos los zombies de la oleada
        while (zombiesAlive > 0)
        {
            yield return new WaitForSeconds(1f);
        }
    }
    
    void SpawnZombie()
    {
        if (zombiePrefab == null)
        {
            Debug.LogError("No hay prefab de zombie asignado.");
            return;
        }
        
        // Elegir punto de spawn aleatorio
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        
        // Crear zombie
        GameObject zombie = Instantiate(zombiePrefab, spawnPoint.position, spawnPoint.rotation);
        
        // Aumentar dificultad
        if (increaseDifficulty && currentWave > 1)
        {
            ZombieHealth health = zombie.GetComponent<ZombieHealth>();
            if (health != null)
            {
                health.maxHealth *= 1 + (currentWave * difficultyIncreaseRate);
                health.currentHealth = health.maxHealth;
            }
            
            ZombieAI ai = zombie.GetComponent<ZombieAI>();
            if (ai != null)
            {
                ai.damage *= 1 + (currentWave * difficultyIncreaseRate * 0.5f);
                ai.moveSpeed *= 1 + (currentWave * difficultyIncreaseRate * 0.3f);
            }
        }
        
        zombiesAlive++;
        
        // Suscribirse al evento de muerte
        ZombieHealth zombieHealth = zombie.GetComponent<ZombieHealth>();
        // Nota: La muerte del zombie se maneja en ZombieHealth llamando a GameManager.ZombieKilled()
    }
    
    public void ZombieDied()
    {
        zombiesAlive = Mathf.Max(0, zombiesAlive - 1);
    }
    
    public int GetCurrentWave()
    {
        return currentWave;
    }
    
    public int GetZombiesAlive()
    {
        return zombiesAlive;
    }
}
