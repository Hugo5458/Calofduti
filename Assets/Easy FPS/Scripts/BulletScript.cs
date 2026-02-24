using UnityEngine;
using System.Collections;

public class BulletScript : MonoBehaviour {

	[Tooltip("Furthest distance bullet will look for target")]
	public float maxDistance = 1000000;
	RaycastHit hit;
	[Tooltip("Prefab of wall damange hit. The object needs 'LevelPart' tag to create decal on it.")]
	public GameObject decalHitWall;
	[Tooltip("Decal will need to be sligtly infront of the wall so it doesnt cause rendeing problems so for best feel put from 0.01-0.1.")]
	public float floatInfrontOfWall;
	[Tooltip("Damage this bullet will cause to enemies")]
	public float damage = 25f;
	[Tooltip("Blood prefab particle this bullet will create upoon hitting enemy")]
	public GameObject bloodEffect;
	[Tooltip("Put Weapon layer and Player layer to ignore bullet raycast.")]
	public LayerMask ignoreLayer;

	/*
	* Uppon bullet creation with this script attatched,
	* bullet creates a raycast which searches for corresponding tags.
	* If raycast finds somethig it will create a decal of corresponding tag.
	*/
	void Update () {
		
		// Detectar zombies en un radio alrededor de la bala (más fiable que raycast solo)
		Collider[] hitColliders = Physics.OverlapSphere(transform.position, 0.5f, ~ignoreLayer);
		bool zombieHit = false;
		
		foreach (Collider hitCollider in hitColliders)
		{
			// Priorizar detección de zombies
			ZombieHealth zombieHealth = hitCollider.GetComponent<ZombieHealth>();
			if (zombieHealth == null)
			{
				zombieHealth = hitCollider.GetComponentInParent<ZombieHealth>();
			}
			if (zombieHealth == null)
			{
				zombieHealth = hitCollider.GetComponentInChildren<ZombieHealth>();
			}
			
			if (zombieHealth != null && !zombieHealth.IsDead())
			{
				Debug.Log($"[BulletScript] Hit zombie: {hitCollider.name}, Health: {zombieHealth.currentHealth}/{zombieHealth.maxHealth}");
				zombieHealth.TakeDamage(damage);
				if (bloodEffect != null)
				{
					Instantiate(bloodEffect, transform.position, Quaternion.LookRotation(transform.forward));
				}
				GunScript.HitMarkerSound();
				Destroy(gameObject);
				zombieHit = true;
				break;
			}
			
			// Detectar otros objetos
			if (hitCollider.transform.tag == "LevelPart")
			{
				if (decalHitWall != null)
				{
					RaycastHit wallHit;
					if (Physics.Raycast(transform.position - transform.forward * 0.1f, transform.forward, out wallHit, 1f, ~ignoreLayer))
					{
						Instantiate(decalHitWall, wallHit.point + wallHit.normal * floatInfrontOfWall, Quaternion.LookRotation(wallHit.normal));
					}
				}
				Destroy(gameObject);
				zombieHit = true;
				break;
			}
			
			if (hitCollider.transform.tag == "Dummie")
			{
				if (bloodEffect != null)
				{
					Instantiate(bloodEffect, transform.position, Quaternion.LookRotation(transform.forward));
				}
				Destroy(gameObject);
				zombieHit = true;
				break;
			}
		}
		
		// Si no hit nada, mover la bala hacia adelante
		if (!zombieHit)
		{
			transform.position += transform.forward * 50f * Time.deltaTime;
		}
		
		// Destruir después de un tiempo o distancia
		Destroy(gameObject, 2f);
	}

}
