using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Controlador centralizado de animaciones para zombies.
/// Usa animator.Play() directamente para forzar las animaciones,
/// sin depender de parámetros o transiciones del Animator Controller.
/// 
/// Esto es MÁS FIABLE que usar triggers porque no depende de la configuración
/// del Animator Controller.
/// </summary>
public class GhoulAnimationController : MonoBehaviour
{
    public enum ZombieAnimState
    {
        Idle,
        Walking,
        Attacking,
        Dead
    }
    
    private Animator animator;
    private ZombieAnimState currentState = ZombieAnimState.Idle;
    private bool hasDied = false;
    private bool isInitialized = false;
    
    // Nombres de estados encontrados (cacheados)
    private int idleHash = -1;
    private int walkHash = -1;
    private int attackHash = -1;
    private int deathHash = -1;
    
    // También guardamos los parámetros por si los necesitamos
    private string speedParam = null;
    private string deadBoolParam = null;
    
    void Start()
    {
        Initialize();
    }
    
    /// <summary>
    /// Inicializa el controlador. Se puede llamar múltiples veces de forma segura.
    /// </summary>
    public void Initialize()
    {
        if (isInitialized) return;
        
        animator = GetComponent<Animator>();
        
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
        
        if (animator == null)
        {
            Debug.LogWarning($"[AnimController] {gameObject.name}: No se encontró Animator.");
            return;
        }
        
        animator.applyRootMotion = false;
        
        // Detectar parámetros (para Speed y Dead)
        DetectParameters();
        
        // Detectar estados del Animator por nombre
        DetectStates();
        
        isInitialized = true;
        
        Debug.Log($"[AnimController] {gameObject.name}: Init OK. Idle={idleHash != -1}, Walk={walkHash != -1}, Attack={attackHash != -1}, Death={deathHash != -1}, SpeedParam={speedParam}");
        
        // Empezar en Idle
        ForceIdle();
    }
    
    void DetectParameters()
    {
        if (animator == null || animator.runtimeAnimatorController == null) return;
        
        foreach (AnimatorControllerParameter param in animator.parameters)
        {
            string nameLower = param.name.ToLower();
            
            if (speedParam == null && param.type == AnimatorControllerParameterType.Float)
            {
                if (nameLower == "speed" || nameLower == "walkspeed")
                    speedParam = param.name;
            }
            
            if (deadBoolParam == null && param.type == AnimatorControllerParameterType.Bool)
            {
                if (nameLower == "dead" || nameLower == "isdead")
                    deadBoolParam = param.name;
            }
        }
    }
    
    /// <summary>
    /// Detecta los estados disponibles en el Animator Controller probando nombres comunes.
    /// Usa animator.HasState() para verificar que existen.
    /// </summary>
    void DetectStates()
    {
        if (animator == null || animator.runtimeAnimatorController == null) return;
        
        // Nombres comunes para cada tipo de animación
        string[] idleNames = { "Idle", "Z_Idle", "idle", "Standing", "Z_Idle_InPlace" };
        string[] walkNames = { "Walk", "Z_Walk", "walk", "Run", "Z_Run", "Z_Walk_InPlace", "Walking", "Running" };
        string[] attackNames = { "Attack", "Z_Attack", "attack", "Attacking", "Hit", "Punch", "Bite" };
        string[] deathNames = { "Death", "Z_Death", "death", "Die", "Dead", "Z_FallingBack", "Z_FallingForward", "FallingBack", "Dying" };
        
        idleHash = FindStateHash(idleNames);
        walkHash = FindStateHash(walkNames);
        attackHash = FindStateHash(attackNames);
        deathHash = FindStateHash(deathNames);
        
        // Si no encontramos estados por nombre, buscar por clips de animación
        if (attackHash == -1 || idleHash == -1)
        {
            Debug.Log($"[AnimController] {gameObject.name}: Buscando estados por clips...");
            FindStatesByClipNames();
        }
    }
    
    int FindStateHash(string[] possibleNames)
    {
        foreach (string name in possibleNames)
        {
            int hash = Animator.StringToHash(name);
            if (animator.HasState(0, hash))
            {
                Debug.Log($"[AnimController] {gameObject.name}: Estado encontrado: '{name}'");
                return hash;
            }
        }
        return -1;
    }
    
    /// <summary>
    /// Busca estados usando los nombres de los clips de animación del controller.
    /// A veces el estado tiene el mismo nombre que el clip.
    /// </summary>
    void FindStatesByClipNames()
    {
        if (animator.runtimeAnimatorController == null) return;
        
        AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;
        
        foreach (AnimationClip clip in clips)
        {
            if (clip == null) continue;
            
            string clipName = clip.name;
            string clipNameLower = clipName.ToLower();
            int hash = Animator.StringToHash(clipName);
            
            bool stateExists = animator.HasState(0, hash);
            
            if (!stateExists) continue;
            
            if (idleHash == -1 && (clipNameLower.Contains("idle") || clipNameLower.Contains("stand")))
            {
                idleHash = hash;
                Debug.Log($"[AnimController] {gameObject.name}: Idle encontrado por clip: '{clipName}'");
            }
            if (walkHash == -1 && (clipNameLower.Contains("walk") || clipNameLower.Contains("run")))
            {
                walkHash = hash;
                Debug.Log($"[AnimController] {gameObject.name}: Walk encontrado por clip: '{clipName}'");
            }
            if (attackHash == -1 && clipNameLower.Contains("attack"))
            {
                attackHash = hash;
                Debug.Log($"[AnimController] {gameObject.name}: Attack encontrado por clip: '{clipName}'");
            }
            if (deathHash == -1 && (clipNameLower.Contains("death") || clipNameLower.Contains("die") || clipNameLower.Contains("falling")))
            {
                deathHash = hash;
                Debug.Log($"[AnimController] {gameObject.name}: Death encontrado por clip: '{clipName}'");
            }
        }
    }
    
    // ==========================================
    // MÉTODOS PÚBLICOS
    // ==========================================
    
    public void SetWalking()
    {
        if (!EnsureInit()) return;
        if (hasDied) return;
        if (currentState == ZombieAnimState.Walking) return;
        if (currentState == ZombieAnimState.Attacking) return;
        
        currentState = ZombieAnimState.Walking;
        
        if (speedParam != null)
        {
            animator.SetFloat(speedParam, 1f);
        }
        
        if (walkHash != -1)
        {
            animator.CrossFadeInFixedTime(walkHash, 0.15f, 0);
        }
    }
    
    public void SetIdle()
    {
        if (!EnsureInit()) return;
        if (hasDied) return;
        if (currentState == ZombieAnimState.Idle) return;
        if (currentState == ZombieAnimState.Attacking) return;
        
        ForceIdle();
    }
    
    void ForceIdle()
    {
        currentState = ZombieAnimState.Idle;
        
        if (speedParam != null && animator != null)
        {
            animator.SetFloat(speedParam, 0f);
        }
        
        if (idleHash != -1 && animator != null)
        {
            animator.CrossFadeInFixedTime(idleHash, 0.15f, 0);
        }
    }
    
    /// <summary>
    /// Reproduce la animación de ataque UNA SOLA VEZ usando animator.Play().
    /// No depende de triggers ni transiciones del controller.
    /// </summary>
    public void PlayAttackAnimation()
    {
        if (!EnsureInit()) return;
        if (hasDied) return;
        if (currentState == ZombieAnimState.Attacking) return;
        
        currentState = ZombieAnimState.Attacking;
        
        if (speedParam != null)
        {
            animator.SetFloat(speedParam, 0f);
        }
        
        if (attackHash != -1)
        {
            // Forzar reproducción directa - esto es lo más fiable
            animator.Play(attackHash, 0, 0f); // layer 0, desde el inicio (0f)
            Debug.Log($"[AnimController] {gameObject.name}: ► ATTACK via Play(hash)");
        }
        else
        {
            // Último recurso: probar animator.Play con strings
            string[] tryNames = { "Attack", "Z_Attack", "attack" };
            foreach (string name in tryNames)
            {
                try
                {
                    animator.Play(name, 0, 0f);
                    Debug.Log($"[AnimController] {gameObject.name}: ► ATTACK via Play('{name}')");
                    break;
                }
                catch { }
            }
        }
    }
    
    public void OnAttackFinished()
    {
        if (hasDied) return;
        
        currentState = ZombieAnimState.Idle;
        
        // Volver a Idle
        if (animator != null)
        {
            if (speedParam != null) animator.SetFloat(speedParam, 0f);
            
            if (idleHash != -1)
            {
                animator.CrossFadeInFixedTime(idleHash, 0.2f, 0);
            }
        }
    }
    
    public void PlayDeathAnimation()
    {
        if (!EnsureInit()) return;
        if (hasDied) return;
        
        hasDied = true;
        currentState = ZombieAnimState.Dead;
        
        if (speedParam != null) animator.SetFloat(speedParam, 0f);
        if (deadBoolParam != null) animator.SetBool(deadBoolParam, true);
        
        if (deathHash != -1)
        {
            animator.Play(deathHash, 0, 0f);
        }
        else
        {
            // Fallback
            string[] tryNames = { "Death", "Z_Death", "Z_FallingBack", "Die" };
            foreach (string name in tryNames)
            {
                try { animator.Play(name, 0, 0f); break; } catch { }
            }
        }
    }
    
    public void PlayHitAnimation()
    {
        // Hit es opcional - no todos los controllers lo tienen
        // No cambiar estado por un hit
        if (!EnsureInit()) return;
        if (hasDied) return;
    }
    
    public bool IsAttacking()
    {
        return currentState == ZombieAnimState.Attacking;
    }
    
    public ZombieAnimState GetCurrentState()
    {
        return currentState;
    }
    
    // ==========================================
    // HELPERS
    // ==========================================
    
    bool EnsureInit()
    {
        if (!isInitialized)
        {
            Initialize();
        }
        return animator != null;
    }
}
