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

		if(Physics.Raycast(transform.position, transform.forward,out hit, maxDistance, ~ignoreLayer)){
			if(decalHitWall){
				if(hit.transform.tag == "LevelPart"){
					Instantiate(decalHitWall, hit.point + hit.normal * floatInfrontOfWall, Quaternion.LookRotation(hit.normal));
					Destroy(gameObject);
					return;
				}
				if(hit.transform.tag == "Dummie"){
					Instantiate(bloodEffect, hit.point, Quaternion.LookRotation(hit.normal));
					Destroy(gameObject);
					return;
				}
				
				// Detectar zombies y aplicar daño (buscar en el objeto golpeado y sus padres)
				ZombieHealth zombieHealth = hit.transform.GetComponent<ZombieHealth>();
				if(zombieHealth == null){
					zombieHealth = hit.transform.GetComponentInParent<ZombieHealth>();
				}
				if(zombieHealth == null){
					zombieHealth = hit.transform.GetComponentInChildren<ZombieHealth>();
				}
				
				if(zombieHealth != null){
					Debug.Log($"[BulletScript] Hit zombie: {hit.transform.name}, Health: {zombieHealth.currentHealth}/{zombieHealth.maxHealth}");
					zombieHealth.TakeDamage(damage);
					if(bloodEffect != null){
						Instantiate(bloodEffect, hit.point, Quaternion.LookRotation(hit.normal));
					}
					GunScript.HitMarkerSound();
					Destroy(gameObject);
					return;
				}
				
				// Debug para ver qué está golpeando la bala
				Debug.Log($"[BulletScript] Hit object: {hit.transform.name}, Tag: {hit.transform.tag}, Layer: {hit.transform.gameObject.layer}");
			}		
			Destroy(gameObject);
		}
		Destroy(gameObject, 0.1f);
	}

}
