using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;



public class Map : MonoBehaviour {
			
	[Range(3,50)]
	public int		Width = 10;

	[Range(3,50)]
	public int		Height = 10;

	[InspectorButton("ResetMap")]
	public bool		_ResetMap;

	public List<PopperMan.Tile>	Tiles;
	
	public UnityEvent		OnMapChanged;

	public int GetMapIndex(int x, int y)
	{
		return x + (y*Width);
	}

	public int2 GetMapXy(int Index)
	{
		return new int2( Index % Width, Index / Width );
	}

	public PopperMan.Tile this[int x,int y]
	{
		get
		{
			if ( x < 0 || x >= Width || y < 0 || y >= Height )
				return PopperMan.Tile.OutOfBounds;

		    return Tiles[GetMapIndex(x,y)];
		}
		set
		{
			Tiles[GetMapIndex(x,y)] = value;
		}
	}

	public PopperMan.Tile this[int2 xy]
	{
		get
		{
		    return this[xy.x,xy.y];
		}
		set
		{
			this[xy.x,xy.y] = value;
		}
	}


	public void ResetMap()
	{
		Tiles = new List<PopperMan.Tile>();
		for (int i = 0; i < Width * Height; i++)
		{
			var Tile = PopperMan.Tile.Empty;
			var xy = GetMapXy( i );
			var x = xy.x;
			var y = xy.y;

			if ( x == 0 || y == 0 || x == Width-1 || y == Height-1 )
				Tile = PopperMan.Tile.Solid;

			Tiles.Add( Tile );
		}

		OnMapChanged.Invoke();
	}



}
