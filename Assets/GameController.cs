using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour {

	[Range(0,20)]
	public float		StartDelay = 20;

	float				Startup = 0;

	public UnityEngine.Events.UnityEvent	OnStartGame;

	void Start()
	{
		ResetGame ();
	}

	public void ResetGame()
	{
		Startup = 0;
		this.enabled = true;
	}

	void Update () {

		Startup += Time.deltaTime;

		if (Startup < StartDelay)
			return;

		OnStartGame.Invoke ();
		this.enabled = false;		
	}
}
