using UnityEngine;
using System.Collections;

public class Laser : Projectile
{
	
	// Use this for initialization
	void Start () {

		//Launch Effect
		Instantiate(hitEffect, transform.position, transform.rotation);

		//Launch Audio
		GameObject launchSFX = Instantiate(launchSound, transform.position, transform.rotation) as GameObject;
		launchSFX.transform.parent = this.transform;

		//Set object kill time
		Destroy (this.gameObject, lifeTime);
	}
	
	// Update is called once per frame
	void Update () {
		//Projectile Movement
		transform.position += Time.deltaTime * projectileSpeed * transform.forward;
	}


	void OnTriggerEnter(Collider otherObject)
	{
		if (otherObject.CompareTag("Enemy"))
		{
			otherObject.GetComponent<Enemy>().TakeDamage(10);
			Instantiate(hitEffect, transform.position, transform.rotation);
			Instantiate(hitSound, transform.position, transform.rotation);
			Destroy (gameObject);
		}
	}
}
