//
//	Code repository for GPU noise development blog
//	http://briansharpe.wordpress.com
//	https://github.com/BrianSharpe
//
//	I'm not one for copyrights.  Use the code however you wish.
//	All I ask is that credit be given back to the blog or myself when appropriate.
//	And also to let me know if you come up with any changes, improvements, thoughts or interesting uses for this stuff. :)
//	Thanks!
//
//	Brian Sharpe
//	brisharpe CIRCLE_A yahoo DOT com
//	http://briansharpe.wordpress.com
//	https://github.com/BrianSharpe
//
//===============================================================================
//  Scape Software License
//===============================================================================
//
//Copyright (c) 2007-2012, Giliam de Carpentier
//All rights reserved.
//
//Redistribution and use in source and binary forms, with or without
//modification, are permitted provided that the following conditions are met: 
//
//1. Redistributions of source code must retain the above copyright notice, this
//   list of conditions and the following disclaimer. 
//2. Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution. 
//
//THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
//ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
//WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
//DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNERS OR CONTRIBUTORS BE LIABLE 
//FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL 
//DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR 
//SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER 
//CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, 
//OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE 
//OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.;

Shader "PerlinTurbulence"
{
	Properties 
	{
		_Octaves ("Octaves", Range(0,10) ) = 8.0
		_Frequency ("Frequency", Range(0,20) ) = 1.0
		_Amplitude ("Amplitude", Range(0,10) ) = 1.0
		_Lacunarity ("Lacunarity", Range(0,10) ) = 1.92
		_Persistence ("Persistence", Range(0,10) ) = 0.8
		_Offset ("Offset", Vector) = (0.0, 0.0, 0.0, 0.0)
		_AnimSpeed("Animation Speed", Range(0,10)) = 1.0
	}


	SubShader 
	{
		Pass
		{

		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag
		#pragma glsl
		#pragma target 3.0

		#include "UnityCG.cginc"

		//	speed up by hardcoding values at compile time
		//#define _Octaves		8
		//#define _Frequency		1.0f
		//#define _Amplitude		1.0f
		//#define _Offset			float3(0,0,0)
		//#define _Lacunarity		1.92f
		//#define _Persistence	0.8f
		fixed _Octaves;
		float _Frequency;
		float _Amplitude;
		float3 _Offset;
		float _Lacunarity;
		float _Persistence;
		float _AnimSpeed;


//
		//	FAST32_hash
		//	A very fast hashing function.  Requires 32bit support.
		//	http://briansharpe.wordpress.com/2011/11/15/a-fast-and-simple-32bit-floating-point-hash-function/
		//
		//	The hash formula takes the form....
		//	hash = mod( coord.x * coord.x * coord.y * coord.y, SOMELARGEFLOAT ) / SOMELARGEFLOAT
		//	We truncate and offset the domain to the most interesting part of the noise.
		//	SOMELARGEFLOAT should be in the range of 400.0->1000.0 and needs to be hand picked.  Only some give good results.
		//	3D Noise is achieved by offsetting the SOMELARGEFLOAT value by the Z coordinate
		//
		   void FAST32_hash_3D( 	float3 gridcell,
								out float4 lowz_hash_0,
								out float4 lowz_hash_1,
								out float4 lowz_hash_2,
								out float4 highz_hash_0,
								out float4 highz_hash_1,
								out float4 highz_hash_2	)		//	generates 3 random numbers for each of the 8 cell corners
		{
			//    gridcell is assumed to be an integer coordinate
		
			//	TODO: 	these constants need tweaked to find the best possible noise.
			//			probably requires some kind of brute force computational searching or something....
			const float2 OFFSET = float2( 50.0, 161.0 );
			const float DOMAIN = 69.0;
			const float3 SOMELARGEFLOATS = float3( 635.298681, 682.357502, 668.926525 );
			const float3 ZINC = float3( 48.500388, 65.294118, 63.934599 );
		
			//	truncate the domain
			gridcell.xyz = gridcell.xyz - floor(gridcell.xyz * ( 1.0 / DOMAIN )) * DOMAIN;
			float3 gridcell_inc1 = step( gridcell, float3( DOMAIN - 1.5, DOMAIN - 1.5, DOMAIN - 1.5 ) ) * ( gridcell + 1.0 );
		
			//	calculate the noise
			float4 P = float4( gridcell.xy, gridcell_inc1.xy ) + OFFSET.xyxy;
			P *= P;
			P = P.xzxz * P.yyww;
			float3 lowz_mod = float3( 1.0 / ( SOMELARGEFLOATS.xyz + gridcell.zzz * ZINC.xyz ) );
			float3 highz_mod = float3( 1.0 / ( SOMELARGEFLOATS.xyz + gridcell_inc1.zzz * ZINC.xyz ) );
			lowz_hash_0 = frac( P * lowz_mod.xxxx );
			highz_hash_0 = frac( P * highz_mod.xxxx );
			lowz_hash_1 = frac( P * lowz_mod.yyyy );
			highz_hash_1 = frac( P * highz_mod.yyyy );
			lowz_hash_2 = frac( P * lowz_mod.zzzz );
			highz_hash_2 = frac( P * highz_mod.zzzz );
		}
		//
		//	Interpolation functions
		//	( smoothly increase from 0.0 to 1.0 as x increases linearly from 0.0 to 1.0 )
		//	http://briansharpe.wordpress.com/2011/11/14/two-useful-interpolation-functions-for-noise-development/
		//
		float3 Interpolation_C2( float3 x ) { return x * x * x * (x * (x * 6.0 - 15.0) + 10.0); }
		//
		//	Perlin Noise 3D  ( gradient noise )
		//	Return value range of -1.0->1.0
		//	http://briansharpe.files.wordpress.com/2011/11/perlinsample.jpg
		//
		float Perlin3D( float3 P )
		{
			//	establish our grid cell and unit position
			float3 Pi = floor(P);
			float3 Pf = P - Pi;
			float3 Pf_min1 = Pf - 1.0;
		
			//
			//	classic noise.
			//	requires 3 random values per point.  with an efficent hash function will run faster than improved noise
			//
		
			//	calculate the hash.
			//	( various hashing methods listed in order of speed )
			float4 hashx0, hashy0, hashz0, hashx1, hashy1, hashz1;
			FAST32_hash_3D( Pi, hashx0, hashy0, hashz0, hashx1, hashy1, hashz1 );
		
			//	calculate the gradients
			float4 grad_x0 = hashx0 - 0.49999;
			float4 grad_y0 = hashy0 - 0.49999;
			float4 grad_z0 = hashz0 - 0.49999;
			float4 grad_x1 = hashx1 - 0.49999;
			float4 grad_y1 = hashy1 - 0.49999;
			float4 grad_z1 = hashz1 - 0.49999;
			float4 grad_results_0 = rsqrt( grad_x0 * grad_x0 + grad_y0 * grad_y0 + grad_z0 * grad_z0 ) * ( float2( Pf.x, Pf_min1.x ).xyxy * grad_x0 + float2( Pf.y, Pf_min1.y ).xxyy * grad_y0 + Pf.zzzz * grad_z0 );
			float4 grad_results_1 = rsqrt( grad_x1 * grad_x1 + grad_y1 * grad_y1 + grad_z1 * grad_z1 ) * ( float2( Pf.x, Pf_min1.x ).xyxy * grad_x1 + float2( Pf.y, Pf_min1.y ).xxyy * grad_y1 + Pf_min1.zzzz * grad_z1 );
		
			//	Classic Perlin Interpolation
			float3 blend = Interpolation_C2( Pf );
			float4 res0 = lerp( grad_results_0, grad_results_1, blend.z );
			float2 res1 = lerp( res0.xy, res0.zw, blend.y );
			float final = lerp( res1.x, res1.y, blend.x );
			final *= 1.1547005383792515290182975610039;		//	(optionally) scale things to a strict -1.0->1.0 range    *= 1.0/sqrt(0.75)
			return final;
		}
		float PerlinNormal(float3 p, int octaves, float3 offset, float frequency, float amplitude, float lacunarity, float persistence)
		{
			float sum = 0;
			for (int i = 0; i < octaves; i++)
			{
				float h = 0;
				h = Perlin3D((p + offset) * frequency);
				sum += h*amplitude;
				frequency *= lacunarity;
				amplitude *= persistence;
			}
			return sum;
		}

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

		v2f vert (appdata v)
		{
			v2f o;
			o.vertex = UnityObjectToClipPos(v.vertex);
			o.uv = v.uv;
			return o;
		}

		fixed4 frag (v2f i) : SV_Target
		{
			//float3 Seed = float3( i.uv,  _Time.y * _AnimSpeed );

			float t = _Time.x * _AnimSpeed;
			float u = i.uv.x;
			float v = i.uv.y;

			float3 Seedx = float3( t, u, v );
			float3 Seedy = float3( u, v, t );
			float3 Seedz = float3( v, t, u );

			float3 h;
			h.x = PerlinNormal(Seedx, _Octaves, _Offset, _Frequency, _Amplitude, _Lacunarity, _Persistence);
			h.y = PerlinNormal(Seedy, _Octaves, _Offset, _Frequency, _Amplitude, _Lacunarity, _Persistence);
			h.z = PerlinNormal(Seedz, _Octaves, _Offset, _Frequency, _Amplitude, _Lacunarity, _Persistence);
			
			h = h * 0.5 + 0.5;
			
			return float4(h, 1);

		}
		ENDCG
		}
	}
}
