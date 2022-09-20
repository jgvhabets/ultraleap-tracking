using UnityEngine;
using System;
using System.Collections;

public class globalTimer : MonoBehaviour {

	public static double paradigmTime;	    // static public variable that is used across all other scripts as a timeStamp
	DateTime beginTime;						// public variable to keep track when the paradigm started

	// Use this for initialization
	void Awake () {
		beginTime = DateTime.Now;
	}
	
	// FixedUpdate is called at a constant rate, independent of frame rate
	void Update () {
		DateTime newTime = DateTime.Now;

		TimeSpan elapsedTime = newTime.Subtract(beginTime) ;

		paradigmTime = elapsedTime.TotalSeconds;
	}
}
