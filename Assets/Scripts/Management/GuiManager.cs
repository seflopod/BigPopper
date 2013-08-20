using UnityEngine;
using System.Collections;

public class GuiManager : Singleton<GuiManager>
{
	private GUITexture _reticle;
	public Texture reloadBarBackground;
	public Texture reloadBarForeground;
	public GUISkin skin;
	
	private string[] _menuOptions;
	
	private float _rldBarWidth;
	private float _rldBarHeight;
	
	private void Start()
	{
		_reticle = (GUITexture) gameObject.GetComponentInChildren<GUITexture>();
		
		_menuOptions = new string[] {"Play Game", "Quit"};
		
		//_rldBarWidth = reloadBarBackground.width;
		_rldBarWidth = Screen.width;
		_rldBarHeight = reloadBarBackground.height;
		
		_reticle.pixelInset = new Rect(Screen.width/2-16, Screen.height/2-16, 32.0f, 32.0f);
	}
	
	private void OnGUI()
	{
		GUI.skin = skin;
		switch(GameManager.GameState)
		{
		case GameStates.MainMenu:
			Screen.showCursor = true;
			DisplayMainMenu();
			break;
		case GameStates.Playing:
			Screen.showCursor = false;
			DisplayHUD();
			break;
		case GameStates.GameOver:
			Screen.showCursor = true;
			DisplayGameOver();
			break;
		default:
			break;
		}
	}
	
	private void DisplayMainMenu()
	{
		_menuOptions = new string[] {"Play Game", "Quit"};
		int choice = GUI.SelectionGrid(CenteredBox(Screen.width*0.4f, Screen.height*0.4f), -1, _menuOptions, 1);
		GameManager.Instance.MenuSelect(choice);
		
	}
	
	private void DisplayHUD()
	{
		string hitStr = string.Format("Balloons Hit: {0}", GameManager.Instance.BalloonsHit);
		string scoreStr = string.Format("Score: {0}", GameManager.Instance.Score);
		string ammoStr = string.Format ("Bullets: {0}", GameManager.Instance.BulletsLeft);
		
		float h = 60.0f;
		float w = Screen.width*0.25f;
		GUI.Box((new Rect(0.0f,20.0f,w,h)), hitStr);
		GUI.Box((new Rect(Screen.width*0.33f,20.0f,w,h)), scoreStr);
		GUI.Box((new Rect(Screen.width*0.67f,20.0f,w,h)), ammoStr);
		DrawReloadTimer();
	}
	
	private void DrawReloadTimer()
	{
		float pct = 1 -
			GameManager.Instance.ReloadTimer.TimeRemaining/GameManager.Instance.ReloadTimer.Length;
		pct = (GameManager.Instance.ReloadTimer.Expired)?1.0f:pct;
		
		GUI.BeginGroup(new Rect(0.0f, 0.0f, _rldBarWidth, _rldBarHeight));
			GUI.DrawTexture(new Rect(0,0,_rldBarWidth,_rldBarHeight), reloadBarBackground);
			GUI.BeginGroup(new Rect(0,0,pct*_rldBarWidth,_rldBarHeight));
				GUI.DrawTexture(new Rect(0,0,_rldBarWidth,_rldBarHeight), reloadBarForeground);
			GUI.EndGroup();
		GUI.EndGroup();
	}
	
	
	private void DisplayGameOver()
	{
		_menuOptions = new string[] {"Play Again", "Quit"};
		Rect textArea = CenteredBox (Screen.width*0.4f, Screen.height*0.2f);
		Rect inertArea = new Rect(textArea.x, textArea.y, textArea.width, textArea.height*0.2f);
		Rect selectionArea = new Rect(inertArea.x, inertArea.y+textArea.height*0.25f,
										textArea.width, textArea.height*0.75f);
		GUI.Box(inertArea, string.Format("Game Over\nScore: {0}", GameManager.Instance.Score));
		int choice = GUI.SelectionGrid(selectionArea, -1, _menuOptions, 1);
		GameManager.Instance.MenuSelect(choice);		
	}
	
	private Rect CenteredBox(float sideMargin, float topMargin)
	{
		float width = Screen.width - 2*sideMargin;
		float height = Screen.height - 2*topMargin;
		float x = sideMargin;
		float y = topMargin;
		return new Rect(x, y, width, height);
	}
	
	public void UpdateReticle(Vector3 newPosition)
	{
		//need to offset by half texture size to center on actual position
		if(GameManager.GameState == GameStates.Playing)
			_reticle.pixelInset = new Rect(newPosition.x-16.0f, newPosition.y-16.0f, 32.0f, 32.0f);
	}
}
