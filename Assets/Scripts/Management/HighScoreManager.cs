using UnityEngine;
using System.Collections.Generic;

public class HighScoreManager
{
	public static readonly int MAX_HIGH_SCORES = 5;

	public static void SaveHighScores(HighScoreManager highScores)
	{
		for(int i=MAX_HIGH_SCORES-1;i>=MAX_HIGH_SCORES-highScores.NumberOfScores;--i)
		{
			ScoreData sd = highScores.Scores[i];
			PlayerPrefs.SetString("name"+i.ToString(), sd.name);
			PlayerPrefs.SetInt("score"+i.ToString(), sd.score);
		}
		PlayerPrefs.Save();
	}

	public static HighScoreManager LoadHighScores()
	{
		HighScoreManager ret = new HighScoreManager();
		for(int i=MAX_HIGH_SCORES-1;i>=0;--i)
		{
			string nameKey = "name"+i.ToString();
			string scoreKey = "score"+i.ToString();
			if(PlayerPrefs.HasKey(nameKey) && PlayerPrefs.HasKey(scoreKey))
			{
				ScoreData sd;
				sd.name = PlayerPrefs.GetString(nameKey);
				sd.score = PlayerPrefs.GetInt(scoreKey);
				ret.AddScore(sd);
			}
			else
			{
				break;
			}
		}
		return ret;
	}
	
	private ScoreData[] _scores = new ScoreData[MAX_HIGH_SCORES];
	private int _numScores = 0;

	private HighScoreManager()
	{
	}

	public bool AddScore(ScoreData info)
	{
		if(isHighScore(info))
		{
			shiftScoresLeft();
			_scores[MAX_HIGH_SCORES-1] = info;
			System.Array.Sort(_scores);
			_numScores++;
			return true;
		}
		return false;
	}

	private void shiftScoresLeft()
	{
		for(int i=0;i<MAX_HIGH_SCORES-1;++i)
		{
			_scores[i] = _scores[i+1];
		}
	}

	private bool isHighScore(ScoreData info)
	{
		if(_numScores < MAX_HIGH_SCORES)
		{
			return true;
		}

		return (System.Array.BinarySearch(_scores, info) == ~MAX_HIGH_SCORES);
	}

	public override string ToString ()
	{
		string ret = "";
		for(int i=MAX_HIGH_SCORES-1;i>=MAX_HIGH_SCORES-_numScores;--i)
		{
			ret += string.Format("{0,-16}\t{1,-10}", _scores[i].name, _scores[i].score);
			if(i != MAX_HIGH_SCORES-_numScores)
			{
				ret+="\n";
			}
		}
		return ret;
	}

	public ScoreData[] Scores { get { return _scores; } }
	public int NumberOfScores { get { return _numScores; } }
}
