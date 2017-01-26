using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class PopperMan
{
	//	same as shader
	const int TILE_INVALID	= -1;
	const int TILE_NONE		= 0;
	const int TILE_FLOOR	= 1;
	const int TILE_SOLID	= 2;
	const int TILE_WALL		= 3;
	const int TILE_BOMB		= 4;
	const int TILE_PLAYER0	= 5;
	const int TILE_PLAYER1	= 6;
	const int TILE_PLAYER2	= 7;
	const int TILE_PLAYER3	= 8;
	const int TILE_PLAYER4	= 9;
	const int TILE_PLAYER5	= 10;
	const int TILE_PLAYER6	= 11;
	const int TILE_PLAYER7	= 12;
	const int TILE_GHOST0	= 13;
	const int TILE_GHOST1	= 14;
	const int TILE_GHOST2	= 15;
	const int TILE_GHOST3	= 16;
	const int TILE_GHOST4	= 17;
	const int TILE_GHOST5	= 18;
	const int TILE_GHOST6	= 19;
	const int TILE_GHOST7	= 20;
	const int TILE_FLAME	= 21;
	const int TILE_WALLCRUMBLE	= 22;


	public enum Tile
	{
		//	map tiles
		Floor		= TILE_FLOOR,
		Solid		= TILE_SOLID,
		Wall		= TILE_WALL,
		WallCrumble	= TILE_WALLCRUMBLE,

		//	meta tiles
		OutOfBounds	= TILE_INVALID,
		Invalid		= TILE_INVALID,
		None		= TILE_NONE,

		//	game tiles
		Bomb		= TILE_BOMB,
		Player0		= TILE_PLAYER0,
		Player1		= TILE_PLAYER1,
		Player2		= TILE_PLAYER2,
		Player3		= TILE_PLAYER3,
		Player4		= TILE_PLAYER4,
		Player5		= TILE_PLAYER5,
		Player6		= TILE_PLAYER6,
		Player7		= TILE_PLAYER7,
		Ghost0		= TILE_GHOST0,
		Ghost1		= TILE_GHOST1,
		Ghost2		= TILE_GHOST2,
		Ghost3		= TILE_GHOST3,
		Ghost4		= TILE_GHOST4,
		Ghost5		= TILE_GHOST5,
		Ghost6		= TILE_GHOST6,
		Ghost7		= TILE_GHOST7,
		Flame		= TILE_FLAME,
		
	}

	public static Tile	GetPlayerTile(int Index) {	return (Tile)( (int)TILE_PLAYER0 + Index ); }
	public static Tile	GetGhostTile(int Index) {	return (Tile)( (int)TILE_GHOST0 + Index ); }

	public enum Direction
	{
		None,
		Up,
		Down,
		Left,
		Right
	}
	
	public static int2	GetDelta(Direction direction)
	{
		switch ( direction )
		{
			case Direction.Up:		return new int2( 0, -1 );
			case Direction.Down:	return new int2( 0, 1 );
			case Direction.Left:	return new int2( -1, 0 );
			case Direction.Right:	return new int2( 1, 0 );
			case Direction.None:	return new int2( 0, 0 );
			default:				throw new System.Exception("Invalid direction " + direction);
		}
	}

	public static int2	Move(int2 xy,Direction direction)
	{
		var Delta = GetDelta( direction );
		xy.x += Delta.x;
		xy.y += Delta.y;
		return xy;
	}

	public enum NesPadJoystickButton
	{
		A,
		B,
		Select,
		Start,
		Up,
		Down,
		Left,
		Right,
	};


}

//	generic funcs!
public class Pop
{
	static public string GetJoystickButtonName(int Joystick,int Button)
	{
		return "joystick " + (Joystick+1) + " button " + Button;
	}

	static public void AddUnique<T>(List<T> Array,T Value) where T : class
	{
		if ( Array.Exists( (v) => { return v==Value;	}	) )
			return;

		Array.Add( Value );
	}

}



public class int2
{
	public int x;
	public int y;

	public int2(int _x,int _y)
	{
		x = _x;
		y = _y;
	}

	public static bool operator== (int2 a, int2 b)
    {
        return ( a.x == b.x && a.y == b.y );
    }

	public static bool operator!= (int2 a, int2 b)
    {
         return !(a == b);
    }
	
    public override bool Equals(object b)
    {
         return b.GetType() == GetType() && (this == (int2)b );
    }
	

}


