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

	[InspectorButton("MapChanged")]
	public bool		_MapChanged;

	[Multiline(20)]
	public string	TilesAsNumbers = "00000000000";

	public List<PopperMan.Tile> Tiles
	{
		get
		{
			var TheTiles = new List<PopperMan.Tile>();
			foreach ( var TileChar in TilesAsNumbers )
			{
				if ( TileChar < '0' || TileChar > '9' )
					continue;

				var TileValue = (int)TileChar - '0';
				var Tile = (PopperMan.Tile)TileValue;
				TheTiles.Add( Tile );
			}
			return TheTiles;
		}

	}

	
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
			var Index = GetTilesAsNumbersIndex(x,y);

			char[] TilesAsNumberChars = TilesAsNumbers.ToCharArray();
			TilesAsNumberChars[Index] = GetChar( value );
			TilesAsNumbers = new string(TilesAsNumberChars);
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

	int GetTilesAsNumbersIndex(int x,int y)
	{
		int MapIndex = GetMapIndex(x,y);
		int TileIndex = 0;
		for ( int i=0;	i<TilesAsNumbers.Length;	i++ )
		{
			var TileChar = TilesAsNumbers[i];
			if ( TileChar < '0' || TileChar > '9' )
				continue;

			if ( MapIndex == TileIndex )
				return i;

			TileIndex++;
		}
		
		return -1;
	}

	char GetChar(PopperMan.Tile Tile)
	{
		char Char = '0';
		Char += (char)Tile;
		return Char;
	}

	public void ResetMap()
	{
		var CurrentTiles = Tiles;
		var Size = Width*Height;

		while ( CurrentTiles.Count < Size )
			CurrentTiles.Add( PopperMan.Tile.Floor );

		TilesAsNumbers = "";
		for (int i = 0; i < Size; i++)
		{
			TilesAsNumbers += GetChar(CurrentTiles[i]);
			if ( (i % Width) == (Width-1) )
				TilesAsNumbers += '\n';
		}


		/*
		Tiles = new List<PopperMan.Tile>();
		for (int i = 0; i < Width * Height; i++)
		{
			var Tile = PopperMan.Tile.Floor;
			var xy = GetMapXy( i );
			var x = xy.x;
			var y = xy.y;

			if ( x == 0 || y == 0 || x == Width-1 || y == Height-1 )
				Tile = PopperMan.Tile.Solid;

			Tiles.Add( Tile );
		}
		*/
		OnMapChanged.Invoke();
	}


	void MapChanged()
	{
		OnMapChanged.Invoke();
	}




}
