using UnityEngine;
using System.Collections;

public class Bullet : MonoBehaviour
{
	//Explosion Effect
	public GameObject Explosion;
	
	public float speed = 300.0f;
	public float lifeTime = 3.0f;
	public int damage = 50;
	
	private Vector3 newPos;
	
	void Start()
	{
		Destroy(gameObject, lifeTime);
	}
	
	void Update()
	{
		// future position if bullet doesn't hit any colliders
		newPos = transform.position + transform.forward * speed * Time.deltaTime;
		
		// see if bullet hits a collider
		RaycastHit hit;
		if (Physics.Linecast(transform.position, newPos, out hit))
		{
			if (hit.collider)
			{
				// create explosion and destroy bullet
				transform.position = hit.point;
				if (Explosion)
					Instantiate(Explosion, hit.point, Quaternion.identity);
				Destroy(gameObject);
				
				// apply damage to object
				GameObject obj = hit.collider.gameObject;
				if ((obj.tag == "Player") | (obj.tag == "EnemyTank"))
					obj.SendMessage("ApplyDamage", damage);
			}
		}
		else
		{
			// didn't hit - move to newPos
			transform.position = newPos;
		}     
	}
	
}