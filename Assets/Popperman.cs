﻿using System.Collections;
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
	const int TILE_PLAYER	= 5;
	const int TILE_GHOST	= 6;
	const int TILE_FLAME	= 7;


	public enum Tile
	{
		//	map tiles
		Floor		= TILE_FLOOR,
		Solid		= TILE_SOLID,
		Wall		= TILE_WALL,

		//	meta tiles
		OutOfBounds	= TILE_INVALID,
		Invalid		= TILE_INVALID,
		None		= TILE_NONE,

		//	game tiles
		Bomb		= TILE_BOMB,
		Player		= TILE_PLAYER,
		Ghost		= TILE_GHOST,
		Flame		= TILE_FLAME,
	}

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

	static public string GetJoystickButtonName(int Joystick,int Button)
	{
		return "joystick " + (Joystick+1) + " button " + Button;
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


