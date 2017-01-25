Shader "PoppperMan/MapRenderer"
{
	Properties
	{
		Width("Width",Range(1,100) ) = 20
		Height("Height",Range(1,100) ) = 20
		TileEmpty("TileEmpty",COLOR) = (0,1,0,1)
		TileSolid("TileSolid",COLOR) = (1,1,1,1)
		TileWall("TileWall",COLOR) = (0,0,1,1)
		TileInvalid("TileInvalid",COLOR) = (1,0,1,1)
		TilePlayer("TilePlayer",COLOR) = (0,1,1,1)
		TileBomb("TileBomb",COLOR) = (0,0,0,1)
		TileFlame("TileFlame", COLOR) = (1,0,0,1)
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

		#define TILE_EMPTY	0
		#define TILE_SOLID	1
		#define TILE_WALL	2
		#define TILE_BOMB	4
		#define TILE_PLAYER	5
		#define TILE_FLAME	6

			float4 TileEmpty;
			float4 TileSolid;
			float4 TileWall;
			float4 TileInvalid;
			float4 TilePlayer;
			float4 TileBomb;
			float4 TileFlame;

		#define MAX_WIDTH	20
		#define MAX_HEIGHT	20
			int Width;
			int Height;

			//	gr: on OSX int Tiles[] renders everything as 0 or <invalid>
			float Tiles[MAX_WIDTH*MAX_HEIGHT];

			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				o.uv.y = 1 - o.uv.y;
				return o;
			}

			float4 GetTileColour(int Tile)
			{
				switch( Tile )
				{
					case TILE_EMPTY:	return TileEmpty;
					case TILE_SOLID:	return TileSolid;
					case TILE_WALL:		return TileWall;
					case TILE_BOMB:		return TileBomb;
					case TILE_PLAYER:	return TilePlayer;
					case TILE_FLAME:	return TileFlame;
					default:			return TileInvalid;
				}
			}
		
			
			fixed4 frag (v2f Frag) : SV_Target
			{
				int x = (int)(Frag.uv.x * Width);
				int y = (int)(Frag.uv.y * Height);
				int i = x + (y*Width);
				
				return GetTileColour( Tiles[i] );
			}
			ENDCG
		}
	}
}
