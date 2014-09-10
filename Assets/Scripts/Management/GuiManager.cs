using UnityEngine;
using System.Collections;

public class GuiManager : Singleton<GuiManager>
{
	public Texture reloadBarBackground;
	public Texture reloadBarForeground;
	public GUISkin skin;
	
	private string[] _menuOptions;
	
	private float _rldBarWidth;
	private float _rldBarHeight;
	
	private void Start()
	{
		_menuOptions = new string[] {"Play Game", "Quit"};
		
		//_rldBarWidth = reloadBarBackground.width;
		_rldBarWidth = Screen.width;
		_rldBarHeight = reloadBarBackground.height;
	}
	
	private void OnGUI()
	{
		GUI.skin = skin;
		switch(GameManager.GameState)
		{
		case GameStates.MainMenu:
			if(GameManager.IS_WEB)
			{
				_menuOptions = new string[] {"Play Game", "Quit"};
			}
			else
			{
				_menuOptions = new string[] {"Play Game", "High Scores", "Quit"};
			}
			displayMainMenu();
			break;
		case GameStates.Playing:
			displayHUD();
			break;
		case GameStates.GameOver:
			if(GameManager.IS_WEB)
			{
				_menuOptions = new string[] {"Play Again", "Main Menu"};
			}
			else
			{
				_menuOptions = new string[] {"Play Again", "High Scores", "Main Menu"};
			}
			displayGameOver();
			break;
		case GameStates.HighScores:
			_menuOptions = new string[] {"Main Menu"};
			displayHighScores();
			break;
		default:
			break;
		}
	}
	
	private void displayMainMenu()
	{
		Rect gridArea = new Rect(Screen.width*0.2f, Screen.height*0.4f, Screen.width*0.6f, Screen.height*0.4f);
		int choice = GUI.SelectionGrid(gridArea, -1, _menuOptions, 1);
		GameManager.Instance.MenuSelect(choice);
		
	}
	
	private void displayHUD()
	{
		string hitStr = string.Format("Balloons Hit: {0}", GameManager.Instance.BalloonsHit);
		string scoreStr = string.Format("Score: {0}", GameManager.Instance.Score);
		string ammoStr = string.Format ("Bullets: {0}", GameManager.Instance.BulletsLeft);
		
		float h = 60.0f;
		float w = Screen.width*0.25f;
		GUI.Box((new Rect(0.0f,20.0f,w,h)), hitStr);
		GUI.Box((new Rect(Screen.width*0.33f,20.0f,w,h)), scoreStr);
		GUI.Box((new Rect(Screen.width*0.67f,20.0f,w,h)), ammoStr);
		drawReloadTimer();
	}
	
	private void drawReloadTimer()
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
	
	
	private void displayGameOver()
	{
		Rect textArea = centeredBox (Screen.width*0.2f, Screen.height*0.2f);
		Rect inertArea = new Rect(textArea.x, textArea.y, textArea.width, textArea.height*0.25f);
		Rect selectionArea = new Rect(inertArea.x, inertArea.y+textArea.height*0.3f,
										textArea.width, textArea.height*0.7f);
		GUI.Box(inertArea, string.Format("Game Over\nScore: {0}", GameManager.Instance.Score));
		int choice = GUI.SelectionGrid(selectionArea, -1, _menuOptions, 1);
		GameManager.Instance.MenuSelect(choice);		
	}

	private void displayHighScores()
	{

		Rect fullArea = centeredBox(Screen.width*1f/9f, Screen.height*0.2f);
		Rect inertArea = new Rect(fullArea.x, fullArea.y, fullArea.width, fullArea.height*0.75f);
		Rect selectionArea = new Rect(inertArea.x, inertArea.y+fullArea.height*0.8f,
		                              fullArea.width, fullArea.height*0.2f);
		GUI.Box(inertArea, GameManager.Instance.HighScores.ToString());
		int choice = GUI.SelectionGrid(selectionArea, -1, _menuOptions, 1);
		GameManager.Instance.MenuSelect(choice);
	}
	
	private Rect centeredBox(float sideMargin, float topMargin)
	{
		float width = Screen.width - 2*sideMargin;
		float height = Screen.height - 2*topMargin;
		float x = sideMargin;
		float y = topMargin;
		return new Rect(x, y, width, height);
	}
}
