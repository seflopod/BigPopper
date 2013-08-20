using UnityEngine;

public class Timer
{
	private float _startTime;
	private float _timerLength;
	
	public Timer()
	{
		SetTimer (0.0f);
	}
	
	public Timer(float timerLength)
	{
		SetTimer (timerLength);
	}
	
	public void SetTimer(float timerLength)
	{
		_timerLength = timerLength;
	}
	
	public void StartTimer()
	{
		_startTime = Time.time;
	}

	public float TimeRemaining
	{
		get { return _timerLength - Time.time + _startTime; }
	}
	
	public float Length
	{
		get { return _timerLength; }
	}
	
	public bool Expired
	{
		get { return Time.time - _startTime >= _timerLength; }
	}
}