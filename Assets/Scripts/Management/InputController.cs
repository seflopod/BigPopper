using UnityEngine;
using System.Collections;

public class InputController : MonoBehaviour
{
	private void Update ()
	{
		switch(GameManager.GameState)
		{
		case GameStates.MainMenu:
			break;
		case GameStates.Playing:
			GameManager.Instance.Aim(Input.mousePosition);
			if(GameManager.Instance.ReloadTimer != null && GameManager.Instance.ReloadTimer.Expired && Input.GetButtonDown("Fire1"))
			{
				GameManager.Instance.FireBullet();
			}
			break;
		case GameStates.GameOver:
			break;
		default:
			break;
		}
	}
}
