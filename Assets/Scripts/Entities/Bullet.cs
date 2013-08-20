using UnityEngine;
using System.Collections;

public class Bullet : MonoBehaviour
{
	private bool _dying;
	
	private void Start()
	{
		_dying = false;
		this.NumberHit = 0;
	}
	
	private void Update()
	{
		if(!_dying && transform.position.z - transform.localScale.z > GameManager.MainCamera.farClipPlane)
		{
			_dying = true;
			GameManager.Instance.DestroyBullet(gameObject);
		}
	}

	public bool Fired { get; set; }
	public int NumberHit { get; set; }
	public bool Dying { get { return _dying; }}
}
