using UnityEngine;
using System;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
	protected static T _instance;
	
	/// <summary>
	/// Implementation of MonoBehaviour method Awake
	/// </summary>
	/// <description>
	/// <para>The Awake method for this generic is a little special.  Since the pattern I'm trying to
	/// enforce is a singleton, there has to be means of making sure only one instance of a class
	/// exists.  Unity makes this a little more difficult than normal because of how instantiation
	/// works.  So this Awake method forces the variable checks to see if there is more than one
	/// instance of the class that inherits from Singleton and if there is it destroys all but one
	/// GameObject that has the child attached.  This is a little harsh, but I couldn't find a
	/// decent object disable method in 3 seconds.  One other note, which objects are destroyed may
	/// be predictable, but depending on execution order it might not be.
	/// </para>
	/// <para>As always, be sure to run base.Awake() in any class that inherits from this.
	/// </para>
	/// </description>
	protected virtual void Awake()
	{
		//T dummy = MySingleton<T>.Instance;
		T[] allT = (T[]) MonoBehaviour.FindObjectsOfType(typeof(T));
		if(allT.Length > 1)
		{
			for(int i=1;i<allT.Length;++i)
				GameObject.Destroy(allT[i].gameObject);
			Debug.LogError("Only one instance of " + typeof(T) + " is allowed.  Random offending objects have been destroyed.");
		}
	}
	
	/// <summary>
	/// Returns the instance of the singleton
	/// </summary>
	public static T Instance
	{
		get
		{
			if(_instance == null)
			{
				_instance = (T) MonoBehaviour.FindObjectOfType(typeof(T));
				if(_instance == null)
				{
					Debug.LogError("An instance of " + typeof(T) + " is required.");
				}
			}
			return _instance;
		}
	}
}
