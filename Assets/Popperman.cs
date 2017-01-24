using System.Collections;
using System.Collections.Generic;
using UnityEngine;




public class PopperMan
{
	public enum Tile
	{
		//	map tiles
		Empty,
		Solid,
		Wall,

		//	meta tiles
		OutOfBounds,

		//	game tiles
		Bomb,
		Player,
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


