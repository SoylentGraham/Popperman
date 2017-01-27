Shader "PoppperMan/MapRenderer"
{
	Properties
	{
		FrameDelta("FrameDelta", Range(0,1) ) = 0
		Width("Width",Range(1,100) ) = 20
		Height("Height",Range(1,100) ) = 20
		
		TileColour_Floor("TileColour_Floor",COLOR) = (0,1,0,1)
		TileColour_Solid("TileColour_Solid",COLOR) = (1,1,1,1)
		TileColour_Wall("TileColour_Wall",COLOR) = (0.5,0.5,0.5,1)
		TileColour_Bomb("TileColour_Bomb",COLOR) = (0,0,0,1)
		TileColour_Player("TileColour_Player",COLOR) = (0,0,1,1)
		TileColour_Ghost("TileColour_Ghost",COLOR) = (0,0,1,0.4)
		TileColour_Flame("TileColour_Flame",COLOR) = (1,0.3,0,1)
		TileColour_Ident("TileColour_Ident",COLOR) = (1,1,1,1)

		FloorColourMult("FloorColourMult", COLOR ) = (0.1,1,1,1)
		FloorColourMin("FloorColourMin", COLOR ) = (0.0,0.1,0.1,1)
		ForcedAlpha("ForcedAlpha", Range(0.5,1) ) = 1
		IdentDropShadowOffset("IdentDropShadowOffset", range(0,0.10) ) = 0.05

		BombRadius("BombRadius", Range(0,1) ) = 0.6
		PlayerRadius("PlayerRadius", Range(0,1) ) = 0.8
		FlameRadius("FlameRadius", Range(0,1) ) = 1.0
		GlyphRadius("GlyphRadius", Range(0,1) ) = 0.6
		
		IdentTexture("IdentTexture", 2D ) = "white" {}
		NoiseTexture("NoiseTexture", 2D ) = "white" {}
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
		#define TILE_WALLCRUMBLE		22

		#define TileColour_Invalid float4(1,0,1,1)
		#define TileColour_None float4(0,0,0,0)
			float4 TileColour_Floor;
			float4 TileColour_Solid;
			float4 TileColour_Wall;
			float4 TileColour_Bomb;
			float4 TileColour_Player;
			float4 TileColour_Ghost;
			float4 TileColour_Flame;
			float ForcedAlpha;
			float3 FloorColourMult;
			float3 FloorColourMin;
			float4 TileColour_Ident;
			float IdentDropShadowOffset;

			float BombRadius;
			float PlayerRadius;
			float FlameRadius;
			float GlyphRadius;

		#define MAX_WIDTH	20
		#define MAX_HEIGHT	20
			int Width;
			int Height;
			float FrameDelta;
			float Frame;

			//	gr: on OSX int Tiles[] renders everything as 0 or <invalid>
			float MapTiles[MAX_WIDTH*MAX_HEIGHT];
			float GameTiles[MAX_WIDTH*MAX_HEIGHT];
			float4 AnimTiles[MAX_WIDTH*MAX_HEIGHT];

		#define MAX_PLAYERS	8
			float4 IdentUvs[MAX_PLAYERS];
			sampler2D IdentTexture;
			sampler2D NoiseTexture;
			
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

			float GetGlyphAlpha(int Player,float2 uv,float Scale)
			{
				//	add the font too
				uv.x = 1-uv.x;

				float GlyphScale = 1 / GlyphRadius;
				float Step = (1/Scale) * GlyphScale;

				float2 Glyphuv = uv;
				Glyphuv -= 0.5f;
				Glyphuv *= Step;
				Glyphuv += 0.5f;
				Glyphuv = min( 1, max(0,Glyphuv));

				float u = lerp( IdentUvs[Player].x, IdentUvs[Player].z, 1-Glyphuv.x );
				float v = lerp( IdentUvs[Player].y, IdentUvs[Player].w, Glyphuv.y );
				float4 Glyph = tex2D( IdentTexture, float2(u,v) );
				return Glyph.a;
			}

			float4 GetPlayerGlyph(int Player,float2 uv,float Radius,float4 Colour)
			{
				Colour = GetCircle( uv, Radius, Colour );

				
				float Glyph = GetGlyphAlpha( Player, uv, Radius );
				float GlyphShadow = GetGlyphAlpha( Player, uv-IdentDropShadowOffset, Radius );

				//Colour.xyz = lerp( Colour, float3(1,1,1), Glyph.a );
				//Colour.a = max( Colour.x, Glyph.a );

				//	colour
				if ( TileColour_Ident.a > 0 )
				{
					if ( GlyphShadow > Glyph )
						Colour.xyz = lerp( Colour, float3(0,0,0), GlyphShadow );

					Colour.xyz = lerp( Colour, TileColour_Ident, Glyph );
					Colour.a += Glyph;

					
				}
				else
				{
					//	hole
					Colour.a *= 1 - Glyph;
				}

				//Colour += Glyph;
				//if ( Glyph.a > 0.5f )
				{
					//Colour *= Glyph;
					//Colour.xy = float2(u,v);
					//Colour.xy = uv;
				}

				return Colour;
			}

			float4 GetNoise(float2 Tilexy,float2 uv,float Time,float4 Colour)
			{
				Tilexy /= float2(Width,Height);
				uv *= Tilexy;

				float Noise = tex2D( NoiseTexture, uv ).y;
				if ( Noise >= Time )
					return Colour;
				return float4( Colour.xyz, 0 );
			}

			float GetPulse()
			{
				float Step = 3.0f;
				float Time = frac( Frame / Step ) + ( FrameDelta/Step);
				return Time;
			}

			float BombAnimRadius()
			{
				float Pulse = GetPulse();

				float Min = 0.7f;
				float Max = 1.0f;
				float Range = Max - Min;

				float Time = abs(lerp( -1, 1, Pulse ));
				float Scale = Max + ( Range  * Time );

				return Scale * BombRadius;
				/*
				return FrameDelta;
				
				float Loops = 5;
				float Time = frac(FrameDelta);
				float TimeRad = lerp( -2*UNITY_PI, 2*UNITY_PI, Time );
				return lerp( Min, Max, abs( cos( TimeRad ) ) );
				*/
			}

			float4 GetFloorColour(int2 Tilexy,float2 uv)
			{
				float u = Tilexy.x / (float)Width;
				float v = Tilexy.y / (float)Height;
				u += uv.x * ( 1/(float)Width );
				v += uv.y * ( 1/(float)Height );

				float3 Perlin = tex2D( NoiseTexture, float2(u,v) ).xyz;
				//Perlin = lerp( float3(1,0,0), float3(0,1,0), Perlin.x );
				Perlin *= FloorColourMult;
				Perlin = max( Perlin, FloorColourMin );
				return float4( Perlin, 1 );
			}

			float4 GetWallColour(float2 uv)
			{
				float Cols = 4;
				float Rows = 4;
				float Border = 0.1f;
				float BorderTop = 1 - (Border);

				int Row = (int)(uv.y * Rows);
				uv = frac( uv * float2( Cols, Rows ) );
				
				if ( fmod(Row, 2.0) == 0.0 )
				{
					//	even
				}
				else
				{
					//	odd
					uv.x = frac( uv.x + 0.5f );
				}
				
				if ( uv.x < Border || uv.y < Border )
					return float4(1,0,0,0);
				if ( uv.x > BorderTop || uv.y > BorderTop )
					return float4(0,0,0,1);

				float Blend = _CosTime.w;
				return float4( lerp(TileColour_Wall.xyz,float3(uv,1), Blend ), TileColour_Wall.a );
				//return float4( uv, 0, 1 );
				return TileColour_Wall;
			}

			float4 GetSolidColour(float2 uv)
			{
				float Border = 0.05f;
				float BorderTop = 1 - Border;
				float Change = 0.1f;
				if ( uv.x < Border || uv.y < Border )
					return TileColour_Solid + float4(Change,Change,Change,0);
				if ( uv.x > BorderTop || uv.y > BorderTop )
					return TileColour_Solid - float4(Change,Change,Change,0);
				return TileColour_Solid;
			}

			float4 GetTileColour(int2 Tilexy,int Tile,float2 uv,float AnimTime)
			{
				switch( Tile )
				{
					default:
					case TILE_INVALID:	return TileColour_Invalid;

					case TILE_NONE:			return TileColour_None;
					case TILE_FLOOR:		return GetFloorColour( Tilexy, uv );
					case TILE_SOLID:		return GetSolidColour(uv);
					case TILE_WALL:			return GetWallColour(uv);
					case TILE_WALLCRUMBLE:	return GetNoise( Tilexy, uv, AnimTime, GetWallColour(uv) );
					case TILE_BOMB:			return GetCircle( uv, BombAnimRadius(), TileColour_Bomb );
					case TILE_PLAYER0:
					case TILE_PLAYER1:
					case TILE_PLAYER2:
					case TILE_PLAYER3:
					case TILE_PLAYER4:
					case TILE_PLAYER5:
					case TILE_PLAYER6:
					case TILE_PLAYER7:
						return GetPlayerGlyph( Tile-TILE_PLAYER0, uv, lerp( PlayerRadius*0.9f, PlayerRadius, FrameDelta), TileColour_Player);

					case TILE_GHOST0:
					case TILE_GHOST1:
					case TILE_GHOST2:
					case TILE_GHOST3:
					case TILE_GHOST4:
					case TILE_GHOST5:
					case TILE_GHOST6:
					case TILE_GHOST7:
						return GetPlayerGlyph( Tile-TILE_GHOST0, uv, PlayerRadius, TileColour_Ghost);

					case TILE_FLAME:	return GetCircle( uv, lerp(FlameRadius,0,AnimTime), TileColour_Flame);
				}
			}
		
			float3 BlendColour(float3 Bottom,float4 Top,float AlphaMult)
			{
				float3 Rgb = lerp( Bottom.xyz, Top.xyz, Top.w * AlphaMult );
				return Rgb;
			}

			float3 GetTileColour(int2 Tilexy,float2 Tileuv,int MapTile,int GameTile,int AnimTile,float AnimTime)
			{
				float4 Colour = GetFloorColour( Tilexy, Tileuv );
				Colour.xyz = BlendColour( Colour, GetTileColour(Tilexy, MapTile,Tileuv,AnimTime), ForcedAlpha );
				Colour.xyz = BlendColour( Colour, GetTileColour(Tilexy, GameTile,Tileuv,AnimTime), 1 );
				Colour.xyz = BlendColour( Colour, GetTileColour(Tilexy, AnimTile,Tileuv,AnimTime), 1 );
				return Colour;
			}
			
			fixed4 frag (v2f Frag) : SV_Target
			{
				float2 uv = Frag.uv * float2(Width,Height);
				int x = floor( uv.x );
				int y = floor( uv.y );
				uv = frac(uv);
				int i = x + (y*Width);
				
				float AnimTile = AnimTiles[i].x;

				float Frame = AnimTiles[i].y;
				float FrameCount = AnimTiles[i].z;
				float AnimTime = (Frame / FrameCount) + ( FrameDelta * (1/FrameCount) );
				
				float3 Colour = GetTileColour( int2(x,y), uv, MapTiles[i], GameTiles[i], AnimTile, AnimTime );

				//	debug framedelta
				//Colour = lerp( Colour, float3(uv,0), FrameDelta );

				return float4( Colour, 1 );
			}
			ENDCG
		}
	}
}
