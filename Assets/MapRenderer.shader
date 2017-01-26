Shader "PoppperMan/MapRenderer"
{
	Properties
	{
		Width("Width",Range(1,100) ) = 20
		Height("Height",Range(1,100) ) = 20
		
		TileColour_Floor("TileColour_Floor",COLOR) = (0,1,0,1)
		TileColour_Solid("TileColour_Solid",COLOR) = (1,1,1,1)
		TileColour_Wall("TileColour_Wall",COLOR) = (0.5,0.5,0.5,1)
		TileColour_Bomb("TileColour_Bomb",COLOR) = (0,0,0,1)
		TileColour_Player("TileColour_Player",COLOR) = (0,0,1,1)
		TileColour_Ghost("TileColour_Ghost",COLOR) = (0,0,1,0.4)
		TileColour_Flame("TileColour_Flame",COLOR) = (1,0.3,0,1)

		BombRadius("BombRadius", Range(0,1) ) = 0.6
		PlayerRadius("PlayerRadius", Range(0,1) ) = 0.8
		FlameRadius("FlameRadius", Range(0,1) ) = 1.0
		GlyphRadius("GlyphRadius", Range(0,1) ) = 0.6
		
		IdentTexture("IdentTexture", 2D ) = "white" {}
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

		
			#define TILE_INVALID -1
		#define TILE_NONE		0
		#define TILE_FLOOR		1
		#define TILE_SOLID			2
		#define TILE_WALL		3
		#define TILE_BOMB		4
		#define TILE_PLAYER0	 5
		#define TILE_PLAYER1	6
		#define TILE_PLAYER2	7
		#define TILE_PLAYER3	8
		#define TILE_PLAYER4	 9
		#define TILE_PLAYER5		10
		#define TILE_PLAYER6	11
		#define TILE_PLAYER7	12
		#define TILE_GHOST0		13
		#define TILE_GHOST1		14
		#define TILE_GHOST2		15
		#define TILE_GHOST3		16
		#define TILE_GHOST4		17
		#define TILE_GHOST5		 18
		#define TILE_GHOST6		 19
		#define TILE_GHOST7		20
		#define TILE_FLAME		21

		#define TileColour_Invalid float4(1,0,1,1)
		#define TileColour_None float4(0,0,0,0)
			float4 TileColour_Floor;
			float4 TileColour_Solid;
			float4 TileColour_Wall;
			float4 TileColour_Bomb;
			float4 TileColour_Player;
			float4 TileColour_Ghost;
			float4 TileColour_Flame;

			float BombRadius;
			float PlayerRadius;
			float FlameRadius;
			float GlyphRadius;

		#define MAX_WIDTH	20
		#define MAX_HEIGHT	20
			int Width;
			int Height;

			//	gr: on OSX int Tiles[] renders everything as 0 or <invalid>
			float MapTiles[MAX_WIDTH*MAX_HEIGHT];
			float GameTiles[MAX_WIDTH*MAX_HEIGHT];
			float AnimTiles[MAX_WIDTH*MAX_HEIGHT];

		#define MAX_PLAYERS	8
			float4 IdentUvs[MAX_PLAYERS];
			sampler2D IdentTexture;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				o.uv.y = 1 - o.uv.y;
				return o;
			}

			float4 GetCircle(float2 uv,float Radius,float4 Colour)
			{
				//	0..1 to -1...1
				uv -= 0.5f;
				uv *= 2;

				if ( length( uv ) <= Radius )
					return Colour;
				else
					return float4( Colour.xyz, 0 );
			}

			float4 GetPlayerGlyph(int Player,float2 uv,float Radius,float4 Colour)
			{
				Colour = GetCircle( uv, Radius, Colour );

				//	add the font too
				uv.x = 1-uv.x;

				float GlyphScale = 1 / GlyphRadius;
				float Step = (1/Radius) * GlyphScale;

				float2 Glyphuv = uv;
				Glyphuv -= 0.5f;
				Glyphuv *= Step;
				Glyphuv += 0.5f;
				Glyphuv = min( 1, max(0,Glyphuv));
				
				float u = lerp( IdentUvs[Player].x, IdentUvs[Player].z, 1-Glyphuv.x );
				float v = lerp( IdentUvs[Player].y, IdentUvs[Player].w, Glyphuv.y );
				float4 Glyph = tex2D( IdentTexture, float2(u,v) );

				//Colour.xyz = lerp( Colour, float3(1,1,1), Glyph.a );
				//Colour.a = max( Colour.x, Glyph.a );
				Colour.a *= 1 - Glyph.a;
				//Colour += Glyph;
				//if ( Glyph.a > 0.5f )
				{
					//Colour *= Glyph;
					//Colour.xy = float2(u,v);
					//Colour.xy = uv;
				}

				return Colour;
			}


			float4 GetTileColour(int Tile,float2 uv)
			{
				switch( Tile )
				{
					default:
					case TILE_INVALID:	return TileColour_Invalid;

					case TILE_NONE:		return TileColour_None;
					case TILE_FLOOR:	return TileColour_Floor;
					case TILE_SOLID:	return TileColour_Solid;
					case TILE_WALL:		return TileColour_Wall;
					case TILE_BOMB:		return GetCircle( uv, BombRadius, TileColour_Bomb );
					case TILE_PLAYER0:
					case TILE_PLAYER1:
					case TILE_PLAYER2:
					case TILE_PLAYER3:
					case TILE_PLAYER4:
					case TILE_PLAYER5:
					case TILE_PLAYER6:
					case TILE_PLAYER7:
						return GetPlayerGlyph( Tile-TILE_PLAYER0, uv, PlayerRadius, TileColour_Player);

					case TILE_GHOST0:
					case TILE_GHOST1:
					case TILE_GHOST2:
					case TILE_GHOST3:
					case TILE_GHOST4:
					case TILE_GHOST5:
					case TILE_GHOST6:
					case TILE_GHOST7:
						return GetPlayerGlyph( Tile-TILE_GHOST0, uv, PlayerRadius, TileColour_Ghost);

					case TILE_FLAME:	return GetCircle( uv, FlameRadius, TileColour_Flame);
				}
			}
		
			float3 BlendColour(float3 Bottom,float4 Top)
			{
				float3 Rgb = lerp( Bottom.xyz, Top.xyz, Top.w );
				return Rgb;
			}

			float3 GetTileColour(float2 Tileuv,int MapTile,int GameTile,int AnimTile)
			{
				float4 Colour = GetTileColour( MapTile, Tileuv );
				Colour.xyz = BlendColour( Colour, GetTileColour(GameTile,Tileuv) );
				Colour.xyz = BlendColour( Colour, GetTileColour(AnimTile,Tileuv) );
				return Colour;
			}
			
			fixed4 frag (v2f Frag) : SV_Target
			{
				float2 uv = Frag.uv * float2(Width,Height);
				int x = floor( uv.x );
				int y = floor( uv.y );
				uv = frac(uv);
				int i = x + (y*Width);
				
				float3 Colour = GetTileColour( uv, MapTiles[i], GameTiles[i], AnimTiles[i] );
				return float4( Colour, 1 );
			}
			ENDCG
		}
	}
}
