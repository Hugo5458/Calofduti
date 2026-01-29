using UnityEngine;

/// <summary>
/// Script de ayuda para configurar la escena del juego.
/// Añádelo a un GameObject vacío llamado "GameSetup" en tu escena.
/// </summary>
public class GameSetup : MonoBehaviour
{
    [Header("=== GUÍA DE CONFIGURACIÓN ===")]
    [TextArea(10, 20)]
    public string instructions = @"
PASOS PARA CONFIGURAR TU JUEGO FPS ZOMBIES:

1. JUGADOR:
   - Arrastra el prefab 'FPSController' desde:
     Standard Assets/Characters/FirstPersonCharacter/Prefabs/
   - Añádele el script 'PlayerHealth'
   - Añádele el script 'Crosshair'
   - Añádele el tag 'Player'

2. ARMA:
   - Crea un objeto vacío como hijo de la cámara del FPSController
   - Añádele el modelo de tu arma
   - Añádele el script 'Weapon'
   - Configura los parámetros del arma

3. ZOMBIE:
   - Crea un prefab de zombie con:
     - Un modelo 3D (puede ser un cubo temporalmente)
     - Collider (CapsuleCollider recomendado)
     - NavMeshAgent (para IA con pathfinding)
     - Script 'ZombieHealth'
     - Script 'ZombieAI'
   - O usa 'SimpleZombie' si no quieres NavMesh

4. SPAWNER:
   - Crea un GameObject vacío
   - Añádele el script 'ZombieSpawner'
   - Crea puntos de spawn (objetos vacíos)
   - Asigna el prefab del zombie y los spawn points

5. GAME MANAGER:
   - Crea un GameObject vacío llamado 'GameManager'
   - Añádele el script 'GameManager'
   - Configura la UI

6. UI (Canvas):
   - Crea un Canvas
   - Añade textos para: Puntuación, Oleada, Munición, Vida
   - Crea un panel de pausa
   - Crea un panel de Game Over

7. NAVMESH (para ZombieAI):
   - Window > AI > Navigation
   - Selecciona el terreno
   - En la pestaña 'Bake', haz clic en 'Bake'

8. PICKUPS (opcional):
   - Crea objetos con Collider (IsTrigger = true)
   - Añádeles el script 'PickupItem'
";

    [Header("Referencias Rápidas")]
    public GameObject playerPrefab;
    public GameObject zombiePrefab;
    public Transform[] spawnPoints;
    
    void OnDrawGizmos()
    {
        // Dibujar spawn points en el editor
        if (spawnPoints != null)
        {
            Gizmos.color = Color.red;
            foreach (Transform point in spawnPoints)
            {
                if (point != null)
                {
                    Gizmos.DrawWireSphere(point.position, 1f);
                    Gizmos.DrawLine(point.position, point.position + point.forward * 2f);
                }
            }
        }
    }
}
