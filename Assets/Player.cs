﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[ExecuteInEditMode]
public class Player : MonoBehaviour {

	public int		JoystickIndex = 0;
	public bool		EnableKeyboardInput = true;

	public PopperMan.Direction	Input_Direction = PopperMan.Direction.None;
	public bool					Input_Bomb = false;

	[Range(0,20)]
	public int		x = 1;

	[Range(0,20)]
	public int		y = 1;

	public int2 xy
	{
		get
		{
			return new int2(x,y);
		}
	}


	[Range(0,4)]
	public int		BombCount = 1;
	[Range(1,10)]
	public int		BombDuration = 10;
	[Range(1,10)]
	public int		BombRadius = 2;

	[Header("Gameplay sounds etc")]
	public UnityEngine.Events.UnityEvent	OnBump;
	public UnityEngine.Events.UnityEvent	OnPlaceBomb;
	public UnityEngine.Events.UnityEvent	OnPlaceBombFailed;
	public UnityEngine.Events.UnityEvent	OnPlayerMoved;


	void SetDirectionIfKey(PopperMan.Direction Direction,PopperMan.NesPadJoystickButton KeyName)
	{
		SetDirectionIfKey (Direction, PopperMan.GetJoystickButtonName (JoystickIndex, KeyName));
	}

	void SetDirectionIfKey(PopperMan.Direction Direction,string KeyName)
	{
		if (!Input.GetKey (KeyName))
			return;
		Input_Direction = Direction;
	}

	void SetDirectionIfKey(PopperMan.Direction Direction,KeyCode KeyName)
	{
		if (!Input.GetKey (KeyName))
			return;
		Input_Direction = Direction;
	}

	public void ClearInput()
	{
		Input_Direction = PopperMan.Direction.None;
		Input_Bomb = false;
	}

	void Update()
	{
		//	we OR the inputs, as they're only used on a tick, we store it until
		SetDirectionIfKey( PopperMan.Direction.Up, PopperMan.NesPadJoystickButton.Up );
		SetDirectionIfKey( PopperMan.Direction.Up, PopperMan.NesPadJoystickButton.Up );
		SetDirectionIfKey( PopperMan.Direction.Up, PopperMan.NesPadJoystickButton.Up );
		SetDirectionIfKey( PopperMan.Direction.Up, PopperMan.NesPadJoystickButton.Up );

		Input_Bomb |= Input.GetKey (PopperMan.GetJoystickButtonName (JoystickIndex, PopperMan.NesPadJoystickButton.A));

		if ( EnableKeyboardInput ) 
		{
			SetDirectionIfKey( PopperMan.Direction.Up, KeyCode.UpArrow );
			SetDirectionIfKey( PopperMan.Direction.Down, KeyCode.DownArrow );
			SetDirectionIfKey( PopperMan.Direction.Left, KeyCode.LeftArrow );
			SetDirectionIfKey( PopperMan.Direction.Right, KeyCode.RightArrow );

			Input_Bomb |= Input.GetKey (KeyCode.Space);
		}
	}

	public void Move(Map map)
	{
		if ( Input_Direction == PopperMan.Direction.None )
			return;

		var NewPos = PopperMan.Move( new int2(x,y), Input_Direction );
		var NewTile = map[NewPos];
		if ( NewTile != PopperMan.Tile.Empty )
		{
			OnBump.Invoke();
			return;
		}

		x = NewPos.x;
		y = NewPos.y;
		OnPlayerMoved.Invoke();
	}


	public void PlaceBomb(Map map,Game game,System.Action<int2> PlaceBomb)
	{
		if ( !Input_Bomb )
			return;

		if ( BombCount == 0 )
		{
			OnPlaceBombFailed.Invoke();
			return;
		}

		var Tile = game[x,y];
		switch ( Tile )
		{
			//	assuming player must be us.. maybe double check that
			case PopperMan.Tile.Empty:
			case PopperMan.Tile.Player:
				break;
			default:
				OnPlaceBombFailed.Invoke();
				return;
		}

		//	place bomb!
		PlaceBomb.Invoke(xy);
		BombCount--;
		Input_Bomb = false;
		OnPlaceBomb.Invoke();
	}

}
