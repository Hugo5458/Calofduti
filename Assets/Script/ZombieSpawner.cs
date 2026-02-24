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
    public List<WaveEnemyConfig> enemies = new List<WaveEnemyConfig>();
}

public class ZombieSpawner : MonoBehaviour
{
    [Header("Configuración Global")]
    public GameObject defaultZombiePrefab;
    public Transform[] spawnPoints;
    public float initialSpawnDelay = 3f;
    public float timeBetweenWaves = 5f;
    public int maxZombiesAlive = 20;

    [Header("Configuración de Spawn Aleatorio")]
    [Tooltip("Radio máximo alrededor de cada punto de spawn donde pueden aparecer los enemigos")]
    public float spawnRadius = 5f;
    [Tooltip("Radio mínimo de separación entre zombies al spawnear (para evitar solapamiento)")]
    public float zombieSeparation = 1.5f;
    [Tooltip("Intentos máximos para encontrar una posición sin solapamiento")]
    public int maxSpawnAttempts = 15;
    [Tooltip("Altura máxima sobre el terreno para considerar válido (evita techos)")]
    public float maxHeightAboveTerrain = 2f;

    [Header("Configuración de Oleadas")]
    public List<Wave> waves = new List<Wave>();
    public bool loopWaves = true;

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
        int waveNumberDisplay = currentWaveIndex + 1;
        string waveNameDisplay = "";

        // 1. CONFIGURAR OLEADA
        if (currentWaveIndex < waves.Count)
        {
            Wave waveConfig = waves[currentWaveIndex];
            waveNameDisplay = waveConfig.name;
            groupsToSpawn = waveConfig.enemies;
            
            if (groupsToSpawn.Count == 0) Debug.LogWarning($"La oleada {waveNameDisplay} no tiene grupos de enemigos.");
        }
        else if (loopWaves)
        {
            // OLEADA INFINITA - repite la última oleada con más enemigos
            waveNameDisplay = "Oleada Infinita " + (endlessWaveCount + 1);
            waveNumberDisplay = waves.Count + endlessWaveCount + 1;

            if (waves.Count > 0)
            {
                Wave lastWave = waves[waves.Count - 1];
                foreach (var group in lastWave.enemies)
                {
                    WaveEnemyConfig newGroup = new WaveEnemyConfig();
                    newGroup.enemyPrefab = group.enemyPrefab;
                    newGroup.note = group.note + " (Inf)";
                    newGroup.count = group.count + (endlessWaveCount * 2);
                    newGroup.initialDelay = group.initialDelay;
                    newGroup.timeBetweenSpawns = group.timeBetweenSpawns;
                    
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

                SpawnEnemy(group.enemyPrefab);

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

    void SpawnEnemy(GameObject prefab)
    {
        if (prefab == null) return;

        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        
        // Buscar una posición aleatoria válida alrededor del punto de spawn
        Vector3 spawnPosition = FindValidSpawnPosition(spawnPoint.position);
        
        GameObject enemy = Instantiate(prefab, spawnPosition, spawnPoint.rotation);

        // Asegurar que el zombie tiene un Collider para no atravesarse con otros
        EnsureZombieCollision(enemy);
        
        zombiesAlive++;
    }

    /// <summary>
    /// Busca una posición aleatoria alrededor del centro del spawn que esté en terreno válido.
    /// Evita casas, pozos, lagos y otros objetos.
    /// </summary>
    Vector3 FindValidSpawnPosition(Vector3 center)
    {
        // Obtener la altura del terreno en el centro como referencia
        float terrainHeightAtCenter = GetTerrainHeight(center);

        for (int attempt = 0; attempt < maxSpawnAttempts; attempt++)
        {
            // Generar posición aleatoria en un círculo alrededor del spawn point
            Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
            Vector3 candidatePos = center + new Vector3(randomCircle.x, 0f, randomCircle.y);

            // Verificar que la posición es válida (terreno, no agua, no edificio, no ocupada)
            if (IsValidSpawnPosition(candidatePos, terrainHeightAtCenter) && !IsPositionOccupied(candidatePos))
            {
                // Colocar exactamente sobre el terreno
                float terrainY = GetTerrainHeight(candidatePos);
                candidatePos.y = terrainY;
                return candidatePos;
            }
        }

        // Fallback: usar el centro sobre el terreno
        center.y = terrainHeightAtCenter;
        return center;
    }

    /// <summary>
    /// Verifica que una posición es válida para spawnear:
    /// - Debe estar sobre terreno (no sobre casas, pozos, etc.)
    /// - No debe estar en agua
    /// - No debe estar a una altura anormal (encima de un edificio)
    /// </summary>
    bool IsValidSpawnPosition(Vector3 position, float referenceTerrainHeight)
    {
        // 1. Verificar que hay terreno debajo
        float terrainHeight = GetTerrainHeight(position);
        if (terrainHeight < -1000f) return false; // No hay terreno aquí

        // 2. Hacer raycast desde arriba para ver qué hay
        RaycastHit hit;
        if (Physics.Raycast(position + Vector3.up * 50f, Vector3.down, out hit, 100f))
        {
            // Rechazar si cae en capa de agua
            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Water"))
            {
                return false;
            }

            // Rechazar si el objeto golpeado NO es terreno y está por encima del terreno
            // (significa que hay un edificio, pozo, etc.)
            if (!(hit.collider is TerrainCollider))
            {
                // Si el raycast pega en algo que no es terreno y está significativamente
                // por encima del terreno, hay una estructura
                if (hit.point.y > terrainHeight + maxHeightAboveTerrain)
                {
                    return false;
                }
            }
        }

        // 3. Rechazar si la altura del terreno es muy diferente al centro
        // (podría ser un barranco o zona de agua baja)
        if (Mathf.Abs(terrainHeight - referenceTerrainHeight) > 5f)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Obtiene la altura del Terrain en una posición XZ.
    /// Retorna -9999 si no hay terreno.
    /// </summary>
    float GetTerrainHeight(Vector3 position)
    {
        Terrain terrain = Terrain.activeTerrain;
        if (terrain != null)
        {
            return terrain.SampleHeight(position) + terrain.transform.position.y;
        }

        // Fallback: usar raycast si no hay Terrain activo
        RaycastHit hit;
        if (Physics.Raycast(position + Vector3.up * 50f, Vector3.down, out hit, 100f))
        {
            if (hit.collider is TerrainCollider)
            {
                return hit.point.y;
            }
        }
        return -9999f;
    }

    /// <summary>
    /// Comprueba si ya hay otro zombie demasiado cerca de la posición candidata.
    /// </summary>
    bool IsPositionOccupied(Vector3 position)
    {
        Collider[] colliders = Physics.OverlapSphere(position, zombieSeparation);
        foreach (Collider col in colliders)
        {
            // Verificar si es un zombie (tiene ZombieAI, ZombieHealth o SimpleZombie)
            if (col.GetComponent<ZombieAI>() != null ||
                col.GetComponent<ZombieHealth>() != null ||
                col.GetComponent<SimpleZombie>() != null)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Asegura que el zombie tiene un Collider para detección.
    /// La separación entre zombies la maneja el script ZombieAI.
    /// </summary>
    void EnsureZombieCollision(GameObject enemy)
    {
        // Asegurar que tiene un Collider para detección
        Collider col = enemy.GetComponent<Collider>();
        if (col == null)
        {
            CapsuleCollider capsule = enemy.AddComponent<CapsuleCollider>();
            capsule.center = new Vector3(0f, 1f, 0f);
            capsule.radius = 0.4f;
            capsule.height = 2f;
        }
    }

    public void ZombieDied()
    {
        zombiesAlive = Mathf.Max(0, zombiesAlive - 1);
    }
    
    /// <summary>
    /// Aumenta la cantidad de zombies para las próximas oleadas
    /// </summary>
    public void IncreaseWaveDifficulty(int additionalZombies)
    {
        // Aumentar la cantidad de zombies en las oleadas infinitas
        if (currentWaveIndex >= waves.Count)
        {
            endlessWaveCount += additionalZombies;
        }
        else
        {
            // Aumentar zombies en oleadas configuradas
            for (int i = 0; i < waves.Count; i++)
            {
                foreach (var group in waves[i].enemies)
                {
                    group.count += additionalZombies;
                }
            }
        }
        
        Debug.Log($"Cantidad de zombies por oleada aumentada en +{additionalZombies}");
    }

    public int GetZombiesAlive()
    {
        return zombiesAlive;
    }

    void OnDrawGizmosSelected()
    {
        if (spawnPoints == null) return;
        
        Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
        foreach (Transform point in spawnPoints)
        {
            if (point != null)
            {
                Gizmos.DrawWireSphere(point.position, spawnRadius);
                Gizmos.DrawSphere(point.position, 0.3f);
            }
        }
    }
}
