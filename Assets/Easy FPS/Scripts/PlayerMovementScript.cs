using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovementScript : MonoBehaviour {
	Rigidbody rb;

	[Header("Current Stats (Read Only)")]
	public float currentSpeed;
	public bool grounded;
    public string debugInfo; // See in Inspector what's happening

	[Header("Settings")]
	public float jumpForce = 500;
	public float maxSpeed = 5;
	public float accelerationSpeed = 50000.0f;
    public float deaccelerationSpeed = 15.0f; // Damping time

	[Header("References")]
	[HideInInspector]public Transform cameraMain;
	[HideInInspector]public Vector3 cameraPosition;
	public Transform bulletSpawn;
    public GameObject bloodEffect;

	// Audio vars
	[Header("Player SOUNDS")]
	public AudioSource _jumpSound;
	public AudioSource _freakingZombiesSound;
	public AudioSource _hitSound;
	public AudioSource _walkSound;
	public AudioSource _runSound;

	// Internal vars
	private Vector3 slowdownV;
	private Vector2 horizontalMovement;
	private LayerMask ignoreLayer;
	private RaycastHit hitInfo;
	private float meleeAttack_cooldown;
	private string currentWeapo;
	public bool been_to_meele_anim = false;
	Ray ray1, ray2, ray3, ray4, ray5, ray6, ray7, ray8, ray9;
	//private float rayDetectorMeeleSpace = 0.15f;
	//private float offsetStart = 0.05f;

	void Awake(){
        Debug.Log("PlayerMovementScript: Script Initialized (Awake)!");
		rb = GetComponent<Rigidbody>();
		
		// 1. Resolve Component Conflicts (AGGRESSIVE)
        // Disable CharacterController
        var cc = GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        // Disable Root Motion on ALL Animators
        Animator[] anims = GetComponentsInChildren<Animator>();
        foreach(var anim in anims) {
            anim.applyRootMotion = false;
            Debug.LogWarning("Disabled Root Motion on Animator: " + anim.gameObject.name);
        }

		// 2. Setup Rigidbody
		if (rb == null) {
			Debug.LogError("PlayerMovementScript: No Rigidbody attached!");
		} else {
			rb.isKinematic = false;
			rb.freezeRotation = true;
            rb.useGravity = true;
            rb.drag = 0f;
            rb.angularDrag = 0.05f;
		}

        // 3. Unstuck
        transform.position += Vector3.up * 0.2f;

		// 4. Robust Camera Finding
		Transform cameraTransform = transform.Find("Main Camera");
		if (cameraTransform != null) {
			cameraMain = cameraTransform;
		} else {
			cameraMain = GetComponentInChildren<Camera>()?.transform;
			if (cameraMain == null && Camera.main != null) {
				cameraMain = Camera.main.transform;
			}
		}

		// 5. Robust BulletSpawn
		if (cameraMain != null) {
			Transform spawnTransform = cameraMain.Find("BulletSpawn");
			if (spawnTransform != null) {
				bulletSpawn = spawnTransform;
			} else {
				GameObject newSpawn = new GameObject("BulletSpawn");
				newSpawn.transform.parent = cameraMain;
				newSpawn.transform.localPosition = Vector3.forward;
				bulletSpawn = newSpawn.transform;
			}
		}

        int playerLayer = LayerMask.NameToLayer("Player");
        if (playerLayer != -1) ignoreLayer = 1 << playerLayer;
        else ignoreLayer = 0; 
        
		Time.timeScale = 1f;
        StartCoroutine(DebugStatusRoutine());
	}

	void PlayerMovementLogic(){
		if (rb == null) return;

        // --- DIAGNOSTIC MOVEMENT ---
        
        // 1. Get Input with Explicit Fallbacks
		float h = Input.GetAxis("Horizontal");
		float v = Input.GetAxis("Vertical");
        bool keyW = Input.GetKey(KeyCode.W);
        bool keyS = Input.GetKey(KeyCode.S);

        if (h == 0 && v == 0) {
            if (keyW) v = 1;
            if (keyS) v = -1;
            if (Input.GetKey(KeyCode.A)) h = -1;
            if (Input.GetKey(KeyCode.D)) h = 1;
        }

        // 2. Determine Speed
        float speed = 5.0f; // Hardcode speed to ensure it's not 0

        // 3. Move
        Vector3 moveDir = (transform.forward * v + transform.right * h).normalized;
        Vector3 movement = moveDir * speed * Time.deltaTime;

        // 4. Force Position
        if (moveDir.magnitude > 0) {
            transform.position += movement;
            // Debug every frame we *try* to move
            Debug.Log(string.Format("TRYING TO MOVE: KeyW={0} V={1} MoveVec={2} TimeDelta={3} Static={4}", 
                keyW, v, movement, Time.deltaTime, gameObject.isStatic));
        }

        currentSpeed = rb.velocity.magnitude;
        grounded = RayCastGrounded();
        
        debugInfo = string.Format("DIAG: In({0},{1}) Sta:{2}", h, v, gameObject.isStatic);
	}

	/* 
       CRITICAL FIX: 
       The previous version accidentally deleted FixedUpdate(), so PlayerMovementLogic() was NEVER CALLED.
       Restoring it now. 
    */
    void FixedUpdate(){
        RaycastForMeleeAttacks(); // Keep this if it exists, or just call movement
        PlayerMovementLogic();
    }

	void Update(){
		Jumping();
		Crouching();
		WalkingSound();
        // Fallback key detection for simple debug
        if (Input.GetKeyDown(KeyCode.P)) Debug.Break();
        
        // Also call Movement here just in case FixedUpdate is paused? No, Logic uses Time.deltaTime, so Update is fine too.
        // Actually, for Transform movement, Update might be smoother. But let's stick to restoring the call first.
	}

    // Restore DebugStatusRoutine
    IEnumerator DebugStatusRoutine() {
        while (true) {
            Debug.Log(string.Format("TRANSFORM: Pos={0} || Input=({1:F2},{2:F2}) || Speed={3:F2}", 
                transform.position, Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), currentSpeed));
            yield return new WaitForSeconds(1.0f);
        }
    }

	void Jumping(){
		if (Input.GetKeyDown(KeyCode.Space)) {
            if (grounded) {
			    rb.AddRelativeForce(Vector3.up * jumpForce);
			    if (_jumpSound) _jumpSound.Play();
			    if (_walkSound) _walkSound.Stop();
			    if (_runSound) _runSound.Stop();
            }
		}
	}

	void Crouching(){
		if(Input.GetKey(KeyCode.C)){
			transform.localScale = Vector3.Lerp(transform.localScale, new Vector3(1,0.6f,1), Time.deltaTime * 15);
		} else {
			transform.localScale = Vector3.Lerp(transform.localScale, new Vector3(1,1,1), Time.deltaTime * 15);
		}
	}

	void WalkingSound(){
		if (_walkSound && _runSound) {
			if (grounded) {
				if (currentSpeed > 1) {
					if (maxSpeed == 3) {
						if (!_walkSound.isPlaying) {
							_walkSound.Play();
							_runSound.Stop();
						}					
					} else if (maxSpeed == 5) {
						if (!_runSound.isPlaying) {
							_walkSound.Stop();
							_runSound.Play();
						}
					}
				} else {
					_walkSound.Stop();
					_runSound.Stop();
				}
			} else {
				// In air
				_walkSound.Stop();
				_runSound.Stop();
			}
		}
	}

	private bool RayCastGrounded(){
		RaycastHit groundedInfo;
        // Cast a ray downwards 1.1 units (slightly more than standard 1 unit height) from center
		if(Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, out groundedInfo, 1.2f, ~ignoreLayer)){
			return true;
		}
		return false;
	}

	void OnCollisionStay(Collision other){
		foreach(ContactPoint contact in other.contacts){
			if(Vector2.Angle(contact.normal,Vector3.up) < 60){
				grounded = true;
			}
		}
	}

	void OnCollisionExit(){
		grounded = false;
	}

	private void RaycastForMeleeAttacks(){
		if (bulletSpawn == null) return;
        // Melee logicplaceholder
	}

	public void InstantiateBlood (RaycastHit _hitPos, bool swordHitWithGunOrNot) {
		if (bloodEffect) Instantiate (bloodEffect, _hitPos.point, Quaternion.identity);
	}
}
