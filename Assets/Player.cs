using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

	public bool		Input_Up = false;
	public bool		Input_Down = false;
	public bool		Input_Left = false;
	public bool		Input_Right = false;
	public bool		Input_Bomb = false;

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

	public PopperMan.Direction	Direction
	{
		get
		{
			if ( Input_Up )		return PopperMan.Direction.Up;
			if ( Input_Down )	return PopperMan.Direction.Down;
			if ( Input_Left )	return PopperMan.Direction.Left;
			if ( Input_Right )	return PopperMan.Direction.Right;
			return PopperMan.Direction.None;
		}
	}

	public void Move(Map map)
	{
		if ( Direction == PopperMan.Direction.None )
			return;

		var NewPos = PopperMan.Move( new int2(x,y), Direction );
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
