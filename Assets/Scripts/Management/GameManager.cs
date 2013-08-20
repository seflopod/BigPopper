using UnityEngine;
using System.Collections.Generic;

public enum GameStates
{
	MainMenu = 0x01,
	Playing = 0x02,
	GameOver = 0x04,
	Quit = 0x08,
	Restart = 0xF0
};

[RequireComponent(typeof(InputController))]
[RequireComponent(typeof(GuiManager))]
public class GameManager : Singleton<GameManager>
{
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
	#endregion
	
	#region unity_funcs
	protected override void Awake()
	{
		base.Awake();
		DontDestroyOnLoad(gameObject);
	}
	
	private void Start()
	{
		_state = GameStates.MainMenu;
		_mainCamera = Camera.main;
		_freq = GenerateFreqTable(balloonMaterials.Count);
		for(int i=0;i<25;++i)
			InstantiateBalloon();
	}
	
	private void Update()
	{
		switch(_state)
		{
		case GameStates.MainMenu:
			break;
		case GameStates.Playing:
			if(Application.loadedLevel==0)
			{
				Application.LoadLevel(1);
				_state = GameStates.MainMenu;
			}
			else
				PlayingUpdate();
			break;
		case GameStates.GameOver:
			break;
		case GameStates.Quit:
			Application.Quit();
			break;
		case GameStates.Restart:
			Application.LoadLevel (1);
			_state = GameStates.Playing;
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
			if(Application.loadedLevel == 1)
				PlayingLateUpdate();
			break;
		case GameStates.GameOver:
			break;
		}
	}
	
	private void FixedUpdate()
	{
		//Vector3.Cross(_bullet.transform.rigidbody.velocity, Vector3.forward
		if(Application.loadedLevel == 1)
			if(_state==GameStates.Playing && _remainingAmmo <= 0 && !_bullet.Dying)
				_state = GameStates.GameOver;
	}
	
	private void OnLevelWasLoaded(int level)
	{
		if(level==1)
		{
			GalleryStart();
			_state = GameStates.Playing;
		}
	}
	#endregion
	
	#region case_specific_updates
	private void MainMenuUpdate()
	{
	}
	
	private void PlayingUpdate()
	{
		if(_bullet.Fired && _reloadTimer.Expired && _remainingAmmo > 0)
			LoadBullet();
	}
	
	private void GameOverUpdate()
	{
	}
	
	private void PlayingLateUpdate()
	{
		if(_balloonSpawnTimer.Expired)
		{
			if(_numBalloons < maxBalloons)
				InstantiateBalloon();
			
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
	private void GalleryStart()
	{
		_remainingAmmo = maxBullets;
		_railGunObj = (GameObject) GameObject.Instantiate(railGunData.railGunPrefab,
															0.5f*Vector3.forward,
															Quaternion.identity);
		
		for(int i=0;i<balloonsAtStart;++i)
			InstantiateBalloon();
		
		_numBalloons = balloonsAtStart;
		_curParticles = null;
		_reloadTimer = new Timer();
		_balloonSpawnTimer = new Timer(1/balloonsPerSecond);
		_balloonSpawnTimer.StartTimer();
		_mainCamera = Camera.main;
		_score = 0.0f;
		_balloonsHit = 0;
		_bonusShots = 0;
		LoadBullet();
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
			if(choice == 0)
				_state = GameStates.Playing;
			else
				_state = GameStates.Quit;
			break;
		case GameStates.GameOver:
			if(choice == 0)
				_state = GameStates.Restart;
			else
				_state = GameStates.Quit;
			break;
		default:
			break;
		}
	}
	#endregion
	
	#region bullet_stuff
	private void LoadBullet()
	{
		GameObject tmpBullet = (GameObject) GameObject.Instantiate(bulletPrefab,
																	Vector3.up,
																	Quaternion.identity);			
		_bullet = tmpBullet.GetComponent<Bullet>();
		tmpBullet.transform.parent = _railGunObj.transform;
		tmpBullet.transform.localScale = new Vector3(0.75f, 0.15f, 0.1875f);
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
	private void InstantiateBalloon()
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
	private Color RandomBalloonColor()
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
	public void Aim(Vector3 mousePos)
	{
		mousePos.x = Mathf.Min(Screen.width, Mathf.Max(mousePos.x, 0.0f));
		mousePos.y = Mathf.Min(Screen.height, Mathf.Max(mousePos.y, 0.0f));
		mousePos.z = 64.0f;
		GuiManager.Instance.UpdateReticle(mousePos);
		Vector3 worldPos = _mainCamera.ScreenToWorldPoint(mousePos);
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
	
	public float[] GenerateFreqTable(int ele)
	{
		float[] ret = new float[ele];
		for(int i=0;i<ele;++i)
			ret[i] = 1/Mathf.Pow(2, i+1) + ((i==0)?0:ret[i-1]);
		return ret;
	}
	
	public int RandomFromTable(int ele)
	{
		float n = Random.value;
		int result = 0;
		while(result < ele && n > _freq[result])
			result++;
		return (result==ele)?ele-1:result; 
	}
	
	#region properties
	public int Score { get { return Mathf.RoundToInt(_score); } }
	public int BalloonsHit { get { return _balloonsHit; } }
	public int BulletsLeft { get { return _remainingAmmo; } }
	public Timer ReloadTimer { get { return _reloadTimer; } }
	
	public static Camera MainCamera { get { return Instance._mainCamera; } }
	public static GameStates GameState { get { return Instance._state; } }
	#endregion
	
}
