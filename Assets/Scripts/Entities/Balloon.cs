using UnityEngine;
using System.Collections;

public class Balloon : MonoBehaviour
{
	private bool _dying;
	
	private void Start()
	{
		_dying = false;
	}
	
	private void Update()
	{
		if(!_dying && GameManager.MainCamera.WorldToScreenPoint(transform.position-Vector3.up*transform.localScale.y).y > Screen.height)
		{
			_dying = true;
			GameManager.Instance.DestroyBalloon(gameObject);
		}
	}
	
	private void OnCollisionEnter(Collision collision)
	{
		if(collision.gameObject.CompareTag("bullet"))
		{
			GameManager.Instance.BalloonHit(gameObject, transform.rigidbody.velocity, collision.gameObject.GetComponent<Bullet>());
		}
	}
}
