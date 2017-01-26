using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class Render : MonoBehaviour {

	[System.Serializable]
	public class TickAnimation
	{
		public int				Duration;	//	life in ticks
		public PopperMan.Tile	Tile;
		public int2				xy;
	}


	public Material	MapShader;
	public Font		IdentFont;
	public Texture	IdentFontTexture;

	[InspectorButton("UpdateMapShader")]
	public bool		_UpdateMapShader;

	public List<TickAnimation>	Animations = new List<TickAnimation>();

	public Vector2 GetCanvasTileUv(int2 xy)
	{
		var map = GameObject.FindObjectOfType<Map>();
		var uv = new Vector2();
		uv.x = xy.x / (float)map.Width;
		uv.y = xy.y / (float)map.Height;
		return uv;
	}

	void Update()
	{
		var game = GameObject.FindObjectOfType<Game>();
		MapShader.SetFloat("FrameDelta", game.FrameDelta );
		
		UpdateMapShader();
	}

	public void AddBombExplodeAnim(Game.Bomb Bomb)
	{
		System.Action<int2> MakeBombAnim = (xy) => {
			var Anim = new TickAnimation ();
			Anim.xy = xy;
			Anim.Tile = PopperMan.Tile.Flame;
			Anim.Duration = 2;
			Animations.Add( Anim );
		};

		foreach (var FlamePos in Bomb.Flames)
		{
			MakeBombAnim ( FlamePos );
		}

	}

	public void AddWallDestroyAnim(int2 xy)
	{
		var Anim = new TickAnimation ();
		Anim.xy = xy;
		Anim.Tile = PopperMan.Tile.WallCrumble;
		Anim.Duration = 1;
		Animations.Add( Anim );
	}

	public void UpdateAnimationTick()
	{
		for ( int a=Animations.Count-1;	a>=0;	a-- )
		{
			var Anim = Animations [a];
			Anim.Duration--;

			//	gr: this may have started on this frame, and this end of tick is triggered at start, so let it go to 0
			if (Anim.Duration >= 0)
				continue;

			Animations.RemoveAt(a);
		}
	}
				

	public void UpdateMapShader()
	{
		var game = GameObject.FindObjectOfType<Game>();
		var map = GameObject.FindObjectOfType<Map>();

		MapShader.SetInt("Width", map.Width );
		MapShader.SetInt("Height", map.Height );
		
		var MapTiles = new List<float>();
		var GameTiles = new List<float>();
		var AnimTiles = new List<float>();

		for ( int i=0;	i<map.Width*map.Height;	i++ )
		{
			var xy = map.GetMapXy(i);
			var MapTile = map[xy];
			var GameTile = PopperMan.Tile.None;
			var AnimTile = PopperMan.Tile.None;

			//	check for anims
			foreach (var Anim in Animations) {
				if (Anim.xy != xy)
					continue;
				AnimTile = Anim.Tile;
			}

			if ( game.GetBombAt(xy) != null )
				GameTile = PopperMan.Tile.Bomb;

			{
				var Player = game.GetPlayerAt(xy);
				if ( Player )
				{
					var PlayerIndex = game.Players.IndexOf(Player);
					GameTile = Player.Alive ? PopperMan.GetPlayerTile(PlayerIndex) : PopperMan.GetGhostTile(PlayerIndex);
				}
			}

			MapTiles.Add( (float)MapTile );
			GameTiles.Add( (float)GameTile );
			AnimTiles.Add( (float)AnimTile );
		}

		MapShader.SetFloatArray("MapTiles", MapTiles );
		MapShader.SetFloatArray("GameTiles", GameTiles );
		MapShader.SetFloatArray("AnimTiles", AnimTiles );

		MapShader.SetTexture("IdentTexture", IdentFontTexture );
		var PlayerGlyphs = new List<Vector4>();
		for ( int p=0;	p<game.Players.Count;	p++ )
		{
			var player = game.Players[p];
			var Glyphuv = new Vector4();
			var CharInfo = new CharacterInfo();
			if ( IdentFont.GetCharacterInfo( player.Ident[0], out CharInfo ) )
			{
				Glyphuv.x = CharInfo.uvTopLeft.x;
				Glyphuv.y = CharInfo.uvTopLeft.y;
				Glyphuv.z = CharInfo.uvBottomRight.x;
				Glyphuv.w = CharInfo.uvBottomRight.y;
			}
			PlayerGlyphs.Add(Glyphuv);
		}
		MapShader.SetVectorArray("IdentUvs", PlayerGlyphs );
		

		UnityEditor.SceneView.RepaintAll();
	}
}
