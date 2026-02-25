using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerMovementScript : MonoBehaviour {

	[Header("Current Stats (Read Only)")]
	public float currentSpeed;
	public bool grounded;

	[Header("Settings")]
	public float jumpForce = 12f;
	public float maxSpeed = 18;
	public float gravity = 20f;
	public float accelerationSpeed = 50.0f;
    public float deaccelerationSpeed = 15.0f;
    public float speedMultiplier = 2.0f;

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
	private CharacterController controller;
	private float verticalSpeed = 0f;
	public bool been_to_meele_anim = false;

	void Awake(){
		// ===========================================
		// STEP 0: FIX PHYSICS LAYER COLLISION MATRIX
		// Ensure ALL layers can collide with each other.
		// Without this, the CharacterController may be on a layer
		// that doesn't collide with the terrain, causing infinite fall.
		// ===========================================
		for (int i = 0; i < 32; i++) {
			for (int j = 0; j < 32; j++) {
				Physics.IgnoreLayerCollision(i, j, false);
			}
		}

		// Force player to Player layer (8) if exists, else Default (0)
		int playerLayer = LayerMask.NameToLayer("Player");
		gameObject.layer = playerLayer != -1 ? playerLayer : 0;
		Debug.Log($"[PlayerMovementScript] Player en capa: {gameObject.layer} ({LayerMask.LayerToName(gameObject.layer)})");

		// Force terrain to Default layer (0)
		Terrain terrain = FindObjectOfType<Terrain>();
		if (terrain != null) {
			terrain.gameObject.layer = 0;
		}

		// ===========================================
		// STEP 1: Remove conflicting scripts INSTANTLY
		// ===========================================
		MonoBehaviour[] allScripts = GetComponentsInChildren<MonoBehaviour>(true);
		foreach(var script in allScripts) {
			if (script == null || script == this) continue;
			string typeName = script.GetType().Name;
			if (typeName.Contains("FirstPerson") || 
				typeName.Contains("FPSController") ||
				typeName.Contains("RigidbodyFirstPerson")) {
				script.enabled = false;
				DestroyImmediate(script);
			}
		}

		// Remove Rigidbody
		Rigidbody rb = GetComponent<Rigidbody>();
		if (rb != null) DestroyImmediate(rb);

		// STEP 2: Setup CharacterController
		controller = GetComponent<CharacterController>();
		if (controller == null) {
			controller = gameObject.AddComponent<CharacterController>();
		}
		controller.height = 1.8f;
		controller.radius = 0.4f;
		controller.center = new Vector3(0, 0.9f, 0);
		controller.skinWidth = 0.001f;
		controller.minMoveDistance = 0.001f;
		controller.stepOffset = 0.3f;

		// STEP 3: Setup camera and bullet spawn
		SetupCameraAndBulletSpawn();
	}

	void SetupCameraAndBulletSpawn() {
		Transform cameraTransform = transform.Find("Main Camera");
		if (cameraTransform != null) {
			cameraMain = cameraTransform;
		} else {
			cameraMain = GetComponentInChildren<Camera>()?.transform;
			if (cameraMain == null && Camera.main != null) {
				cameraMain = Camera.main.transform;
			}
		}

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
	}

	void Update(){
		if (controller == null) return;

		// 1. Input
		float h = Input.GetAxis("Horizontal");
		float v = Input.GetAxis("Vertical");

		if (h == 0 && v == 0) {
			if (Input.GetKey(KeyCode.W)) v = 1;
			if (Input.GetKey(KeyCode.S)) v = -1;
			if (Input.GetKey(KeyCode.A)) h = -1;
			if (Input.GetKey(KeyCode.D)) h = 1;
		}

		// 2. Horizontal movement - sistema ultra rápido
		Vector3 moveDir = (transform.forward * v + transform.right * h);
		moveDir.y = 0;
		if (moveDir.magnitude > 1f) moveDir.Normalize();
		Vector3 horizontalMove = moveDir * maxSpeed * speedMultiplier * 2f; // Multiplicador reducido

		// 3. Ground check - simply use CharacterController.isGrounded
		grounded = controller.isGrounded;

		// 4. Gravity and Jump
		if (grounded) {
			// Small downward force to keep grounded (standard Unity practice)
			verticalSpeed = -2f;

			if (Input.GetKeyDown(KeyCode.Space)) {
				verticalSpeed = jumpForce;
				if (_jumpSound) _jumpSound.Play();
				if (_walkSound) _walkSound.Stop();
				if (_runSound) _runSound.Stop();
			}
		} else {
			verticalSpeed -= gravity * Time.deltaTime;
			if (verticalSpeed < -30f) verticalSpeed = -30f;
		}

		// 5. Move
		Vector3 finalMove = horizontalMove + Vector3.up * verticalSpeed;
		controller.Move(finalMove * Time.deltaTime);

		// 6. Speed output
		currentSpeed = new Vector3(horizontalMove.x, 0, horizontalMove.z).magnitude;

		Crouching();
		WalkingSound();
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
				_walkSound.Stop();
				_runSound.Stop();
			}
		}
	}

	private void RaycastForMeleeAttacks(){
		if (bulletSpawn == null) return;
	}

	public void InstantiateBlood (RaycastHit _hitPos, bool swordHitWithGunOrNot) {
		if (bloodEffect) Instantiate (bloodEffect, _hitPos.point, Quaternion.identity);
	}
}
