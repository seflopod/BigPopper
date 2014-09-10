using UnityEngine;
using System.Collections.Generic;

public struct ScoreData : System.IComparable<ScoreData>
{
	public string name;
	public int score;

	#region IComparable implementation

	public int CompareTo (ScoreData other)
	{
		int tmp = score.CompareTo(other.score);
		if(tmp == 0 && name != null)
		{
			tmp = name.CompareTo(other.name);
		}
		return tmp;
	}

	#endregion
}

