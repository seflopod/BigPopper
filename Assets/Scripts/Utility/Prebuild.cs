using UnityEngine;
using System.Collections;

public class Prebuild : MonoBehaviour {
	
	GUITexture _progBar;
	// Use this for initialization
	void Start () {
		_progBar = GameObject.FindGameObjectWithTag("ProgBar").GetComponent<GUITexture>();
		
	}
	
	// Update is called once per frame
	void Update () {
#if UNITY_WEBPLAYER
		if(Application.CanStreamedLevelBeLoaded(1))
			Application.LoadLevel(1);
		else
		{
			float prog = Application.GetStreamProgressForLevel(1);
			Rect cur = _progBar.pixelInset;
			cur.width = prog * Screen.width;
			_progBar.pixelInset = cur;
		}
#elif UNITY_STANDALONE
		Application.LoadLevel(1);
#endif
	}
}