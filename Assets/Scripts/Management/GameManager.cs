using UnityEngine;
using System.Collections.Generic;

public enum GameStates
{
	Preload,
	MainMenu,
	Playing,
	GameOver,
	HighScores,
	Quit,
	Restart
};

[RequireComponent(typeof(InputController))]
[RequireComponent(typeof(GuiManager))]
public class GameManager : Singleton<GameManager>
{
#if UNITY_WEBPLAYER
	public static readonly bool IS_WEB = true;
#else
	public static readonly bool IS_WEB = false;
#endif

	#region public_variables
	public int balloonsAtStart = 5;
	public int maxBalloons = 10;
	public float balloonsPerSecond = 1.0f;
	public GameObject balloonPrefab;
	public GameObject balloonParticlePrefab;
	public List<Material> balloonMaterials;
	public List<Material> particleMaterials;
	
	public int maxBullets = 10;
	public float pointsForBonus = 40000.0f;
	public GameObject bulletPrefab;
	
	public RailGunData railGunData = new RailGunData();
	#endregion
	
	#region private_variables
	private GameStates _state;
	private GUITexture _progBar;
	
	private Timer _reloadTimer;
	private Timer _balloonSpawnTimer;
	private Camera _mainCamera;
	
	private Bullet _bullet;
	private int _remainingAmmo;
	private int _bonusShots;
	
	private GameObject _railGunObj;
	
	private int _numBalloons;
	private GameObject _balloonParticleObj;
	private ParticleSystem _curParticles;
	
	
	private int _balloonsHit;
	private float _score;
	private int _blnMatIdx;
	private float[] _freq;

	private HighScoreManager _highScores;
	#endregion
	
	#region unity_funcs
	protected override void Awake()
	{
		base.Awake();
		_state = GameStates.Preload;
		DontDestroyOnLoad(gameObject);
	}
	
	private void Start()
	{
		_progBar = GameObject.FindGameObjectWithTag("ProgBar").GetComponent<GUITexture>();
		if(!IS_WEB)
		{
			_highScores = HighScoreManager.LoadHighScores();
		}
	}
	
	private void Update()
	{
		switch(_state)
		{
		case GameStates.Preload:
			preloadUpdate();
			break;
		case GameStates.MainMenu:
			if(Application.loadedLevel != 1)
			{
				if(IS_WEB && Application.CanStreamedLevelBeLoaded(1))
				{
					Application.LoadLevel(1);
				}
				else
				{
					Application.LoadLevel(1);
				}
			}
			break;
		case GameStates.Playing:
			if(IS_WEB)
			{
				if(Application.loadedLevel == 1 && Application.CanStreamedLevelBeLoaded(2))
				{
					Application.LoadLevel(2);
					_state = GameStates.MainMenu;
				}
				else if(Application.loadedLevel == 2)
				{
					playingUpdate();
				}
			}
			else
			{
				if(Application.loadedLevel == 1)
				{
					Application.LoadLevel(2);
					_state = GameStates.MainMenu;
				}
				else
				{
					playingUpdate();
				}
			}
			break;
		case GameStates.GameOver:
			break;
		case GameStates.Quit:
			Application.Quit();
			break;
		case GameStates.Restart:
			if(IS_WEB && Application.CanStreamedLevelBeLoaded(2))
			{
				Application.LoadLevel(2);
				_state = GameStates.Playing;
			}
			else
			{
				Application.LoadLevel(2);
				_state = GameStates.Playing;
			}
			break;
		}
	}
	
	private void LateUpdate()
	{
		switch(_state)
		{
		case GameStates.MainMenu:
			break;
		case GameStates.Playing:
			if(Application.loadedLevel == 2)
			{
				playingLateUpdate();
			}
			break;
		case GameStates.GameOver:
			break;
		}
	}
	
	private void FixedUpdate()
	{
		if(Application.loadedLevel == 2 && _state == GameStates.Playing && _remainingAmmo <= 0 && !_bullet.Dying)
		{
			if(!IS_WEB)
			{
				ScoreData sd;
				sd.score = Mathf.FloorToInt(_score);
				sd.name = "Player";
				_highScores.AddScore(sd);
				HighScoreManager.SaveHighScores(_highScores);
			}
			_state = GameStates.GameOver;
		}
	}
	
	private void OnLevelWasLoaded(int level)
	{
		switch(level)
		{
		case 1:
			initMainMenu();
			break;
		case 2:
			initGallery();
			_state = GameStates.Playing;
			break;
		default:
			break;
		}
	}
	#endregion


	#region case_specific_updates
	private void preloadUpdate()
	{
		if(IS_WEB)
		{
			if(Application.CanStreamedLevelBeLoaded(1))
			{
				Application.LoadLevel(1);
			}
			else
			{
				float prog = Application.GetStreamProgressForLevel(1);
				Rect cur = _progBar.pixelInset;
				cur.width = prog * Screen.width;
				_progBar.pixelInset = cur;
			}
		}
		else
		{
			Application.LoadLevel(1);
		}
	}
	
	private void playingUpdate()
	{
		if(_bullet.Fired && _reloadTimer.Expired && _remainingAmmo > 0)
			loadBullet();
	}
	
	private void playingLateUpdate()
	{
		if(_balloonSpawnTimer.Expired)
		{
			if(_numBalloons < maxBalloons)
				instantiateBalloon();
			
			_balloonSpawnTimer.StartTimer();
		}
		
		if(_curParticles != null)
		{
			if(!_curParticles.IsAlive())
			{
				GameObject.Destroy(_curParticles.gameObject);
				_curParticles = null;
			}
		}
	}
	#endregion
	
	#region level_management
	private void initMainMenu()
	{
		_state = GameStates.MainMenu;
		_mainCamera = Camera.main;
		_freq = GenerateFreqTable(balloonMaterials.Count);
		for(int i=0;i<25;++i)
		{
			instantiateBalloon();
		}
	}

	private void initGallery()
	{
		_remainingAmmo = maxBullets;
		_railGunObj = (GameObject) GameObject.Instantiate(railGunData.railGunPrefab,
															0.5f*Vector3.forward,
															Quaternion.identity);
		
		for(int i=0;i<balloonsAtStart;++i)
			instantiateBalloon();
		
		_numBalloons = balloonsAtStart;
		_curParticles = null;
		_reloadTimer = new Timer();
		_balloonSpawnTimer = new Timer(1/balloonsPerSecond);
		_balloonSpawnTimer.StartTimer();
		_mainCamera = Camera.main;
		_score = 0.0f;
		_balloonsHit = 0;
		_bonusShots = 0;
		loadBullet();
	}
	#endregion

	#region gui_proc
	public void MenuSelect(int choice)
	{
		if(choice == -1)
			return;
		
		switch(_state)
		{
		case GameStates.MainMenu:
			switch(choice)
			{
			case 0:
				_state = GameStates.Playing;
				break;
			case 1:
				_state = GameStates.HighScores;
				break;
			case 2:
				_state = GameStates.Quit;
				break;
			default:
				break;
			}
			break;
		case GameStates.HighScores:
			switch(choice)
			{
			case 0:
				_state = GameStates.MainMenu;
				break;
			default:
				break;
			}
			break;
		case GameStates.GameOver:
			switch(choice)
			{
			case 0:
				_state = GameStates.Restart;
				break;
			case 1:
				_state = GameStates.HighScores;
				break;
			case 2:
				_state = GameStates.MainMenu;
				break;
			default:
				break;
			}
			break;
		default:
			break;
		}
	}
	#endregion
	
	#region bullet_stuff
	private void loadBullet()
	{
		GameObject tmpBullet = (GameObject) GameObject.Instantiate(bulletPrefab,
																	Vector3.up,
																	Quaternion.identity);			
		_bullet = tmpBullet.GetComponent<Bullet>();
		tmpBullet.transform.parent = _railGunObj.transform;
		//tmpBullet.transform.localScale = new Vector3(0.75f, 0.15f, 0.1875f);
		tmpBullet.transform.localPosition = new Vector3(0.0f, 0.01805623f, -0.4f);
		tmpBullet.transform.localRotation = Quaternion.identity;
		_bullet.Fired = false;
		--_remainingAmmo;
	}
	
	public void FireBullet()
	{
		if(_remainingAmmo > 0)
		{
			_bullet.transform.rigidbody.useGravity = true;
			_bullet.transform.rigidbody.isKinematic = false;
			_bullet.transform.rigidbody.constraints = RigidbodyConstraints.None;
			_bullet.transform.rigidbody.AddRelativeForce(railGunData.fireForce * Vector3.forward,
															ForceMode.Impulse);
			_bullet.transform.parent = null;
			_bullet.Fired = true;
			_reloadTimer.SetTimer(railGunData.reloadTime);
			_reloadTimer.StartTimer();
		}
	}
	
	public void DestroyBullet(GameObject blt)
	{
		GameObject.Destroy(blt);
	}
	#endregion
	
	#region balloon_stuff
	private void instantiateBalloon()
	{
		GameObject tmpBalloon = (GameObject) GameObject.Instantiate(balloonPrefab,
																	new Vector3(
																			Random.Range(-32,32),
																			Random.Range(-24,0),
																			Random.Range (40,80)),
																	Quaternion.identity);
		tmpBalloon.transform.rigidbody.AddForce(Vector3.up+Random.Range (-1.0f,1.0f)*Vector3.right, ForceMode.VelocityChange);
		MeshRenderer blnMsh = (MeshRenderer) tmpBalloon.GetComponent<MeshRenderer>();
		//blnMsh.material.color = RandomBalloonColor();
		_blnMatIdx = RandomFromTable(balloonMaterials.Count);		
		blnMsh.material = balloonMaterials[_blnMatIdx];
		_balloonParticleObj = balloonParticlePrefab;
		++_numBalloons;
	}
	
	/// <summary>
	/// Generates a random color for a balloon.
	/// </summary>
	/// <returns>
	/// The balloon color.
	/// </returns>
	/// <description>
	/// Rather than generate any possible color, this function sticks to the bright colors the tend
	/// to be used for balloons.
	/// </description>
	private Color randBalloonColor()
	{
		float[] rndFlt = {1.0f/Random.Range(1,3),
							(Random.Range(1,3)==2)?1.0f/Random.Range(1,3):0.0f,
							0.0f};
		
		if(rndFlt[0] == 0.5f && rndFlt[1] == 0.0f)
			rndFlt[0] = 1.0f;
		else if(rndFlt[0] == 0.0f && rndFlt[1] == 0.5f)
			rndFlt[1] = 1.0f;
		
		float tmp;
		int j;
		for(int i=2;i>=0;--i)
		{
			tmp = rndFlt[i];
			j = Random.Range(0,3);
			rndFlt[i] = rndFlt[j];
			rndFlt[j] = tmp;
		}
		return new Color(rndFlt[0], rndFlt[1], rndFlt[2]);
			
	}
	
	public void BalloonHit(GameObject bln, Vector3 blnVeclocity, Bullet hitBy)
	{
		float baseScore = bln.transform.rigidbody.velocity.sqrMagnitude;
		int scoreMult = ++hitBy.NumberHit;
		//Color blnClr = bln.gameObject.GetComponent<MeshRenderer>().material.color;
		//float colorBonus = (blnClr.r*3+blnClr.g*2+blnClr.b) * 1000;
		float colorBonus = Mathf.Pow(2.0f, _blnMatIdx) * 300;
		_score+=baseScore*scoreMult+colorBonus;
		if(_score-_bonusShots*pointsForBonus>=pointsForBonus)
		{
			++_bonusShots;
			++_remainingAmmo;
		}
		++_balloonsHit;
		
		Vector3 pos = bln.transform.position;
		DestroyBalloon(bln);
		_curParticles = ((GameObject) GameObject.Instantiate(_balloonParticleObj, pos, Quaternion.identity)).GetComponent<ParticleSystem>();
		//_curParticles.startColor = blnClr;
		Color curMat = bln.gameObject.GetComponent<MeshRenderer>().material.color;
		int idx = 0;
		foreach(Material mat in balloonMaterials)
		{
			if(mat.color == curMat)
				break;
			else
				++idx;
		}
		
		_curParticles.renderer.material = particleMaterials[idx];
	}
	
	public void DestroyBalloon(GameObject bln)
	{
		--_numBalloons;
		GameObject.Destroy(bln);
	}
	#endregion
	
	
	#region railgun_stuff
	public void Aim(Vector3 cursorPos)
	{
		//if we don't have an object to aim, don't try to aim it
		if(_railGunObj == null)
		{
			return;
		}

		cursorPos.x = Mathf.Min(Screen.width, Mathf.Max(cursorPos.x, 0.0f));
		cursorPos.y = Mathf.Min(Screen.height, Mathf.Max(cursorPos.y, 0.0f));
		cursorPos.z = 64.0f;
		Vector3 worldPos = _mainCamera.ScreenToWorldPoint(cursorPos);

		_railGunObj.transform.LookAt(worldPos);
		
		//180 is arbitrary here. B/c of the 360/0 issue, there's no good way to determine if
		//an object has over-rotated other than by defining an arbitrary cut-off.
		if(_railGunObj.transform.rotation.eulerAngles.x < -90.0f)
		{
			Vector3 rot = _railGunObj.transform.rotation.eulerAngles;
			rot.x = 0.0f;
			_railGunObj.transform.rotation = Quaternion.Euler(rot);
		}
	}
	#endregion

	/**
	 * This is used to create a roll list with a descending chance
	 * index	probability		to roll
	 * 0		0.5				0.5
	 * 1		0.25			0.75
	 * 2		0.125			0.875
	 * ...
	 * ele		1/2^(ele+1)		last to roll + 1/2^(ele+1)
	 * 
	 */
	public float[] GenerateFreqTable(int ele)
	{
		float[] ret = new float[ele];
		for(int i=0;i<ele;++i)
		{
			ret[i] = 1/Mathf.Pow(2, i+1) + ((i==0)?0:ret[i-1]);
		}
		return ret;
	}

	/**
	 * Grabs a random int in the range of [0,ele) based on a frequency table
	 * 
	 */
	public int RandomFromTable(int ele)
	{
		float n = Random.value;
		int result = 0;
		while(result < ele && n > _freq[result])
		{
			result++;
		}
		return (result==ele)?ele-1:result; 
	}
	
	#region properties
	public int Score { get { return Mathf.RoundToInt(_score); } }
	public int BalloonsHit { get { return _balloonsHit; } }
	public int BulletsLeft { get { return _remainingAmmo; } }
	public Timer ReloadTimer { get { return _reloadTimer; } }
	public HighScoreManager HighScores
	{
		get { return _highScores; }
		set { _highScores = value; }
	}
	
	public static Camera MainCamera { get { return Instance._mainCamera; } }
	public static GameStates GameState { get { return Instance._state; } }
	#endregion
	
}
