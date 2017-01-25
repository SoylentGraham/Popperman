using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class UnityEvent_Bomb : UnityEngine.Events.UnityEvent <Game.Bomb> {}

[System.Serializable]
public class UnityEvent_Player : UnityEngine.Events.UnityEvent <Player> {}



public class Game : MonoBehaviour {

	public class Bomb
	{
		public int		x {	get {return xy.x; } }
		public int		y {	get {return xy.y; } }
		public int2		xy;
		public int		StartFrame;
		public int		Duration;
		public Player	Player;
		public int		Radius;

		//	store flames upon explosion for anim
		public List<int2>	Flames = new List<int2>();
	};

	[InspectorButton("Tick")]
	public bool	_Tick;
	int Frame = 0;


	[Range(1,60)]
	public int		TicksPerSec = 5;
	float	TickCountdown = 0;
	float	TickMs {	get {	return 1000 / (float)TicksPerSec; } }

	public List<Player>	_Players;
	public List<Bomb>	Bombs = new List<Bomb>();
	public List<Player>	Players
	{
		get
		{
			if ( _Players == null || _Players.Count == 0 )
				_Players = new List<Player>( GameObject.FindObjectsOfType<Player>() );
			return _Players;
		}
	}

	[Header("Game events - eg sound")]
	public UnityEvent_Bomb					OnBombExplode;
	public UnityEvent_Player				OnPlayerDeathExplode;
	public UnityEvent_Player				OnPlayerJoin;
	public UnityEngine.Events.UnityEvent	OnGameFinished;
	public UnityEngine.Events.UnityEvent	OnTickEnd;

	public PopperMan.Tile this[int x,int y]
	{
		get
		{
			return GetTile(new int2(x,y));
		}
	}

	//	gr: this can return multiple things for a tile (player, bomb), handle this
	public PopperMan.Tile GetTile(int2 xy)
	{
		//	check for a bomb
		foreach ( var bomb in Bombs )
		{
			if ( bomb.xy == xy )
				return PopperMan.Tile.Bomb;
		}

		//	see if there's a player here
		foreach ( var player in Players )
		{
			if ( player.xy == xy )
				return PopperMan.Tile.Player;
		}

		var map = GameObject.FindObjectOfType<Map>();
		return map[xy];
	}


	private void Update()
	{
		TickCountdown -= Time.deltaTime;
		if ( TickCountdown < 0 )
		{
			Tick();
			TickCountdown = TickMs / 1000.0f;
		}
	}

	bool WithinDirectionRadius(int2 a,int2 b,int Radius)
	{
		var Deltax = Mathf.Abs( a.x - b.x );
		var Deltay = Mathf.Abs( a.y - b.y );

		if ( Deltay == 0 && Deltax <= Radius )
			return true;

		if ( Deltax == 0 && Deltay <= Radius )
			return true;

		return false;
	}
	
	Bomb GetBombAt(int2 xy)
	{
		foreach (var Bomb in Bombs) {
			if (Bomb.xy == xy)
				return Bomb;
		}
		return null;
	}

	Player GetPlayerAt(int2 xy)
	{
		foreach (var Player in Players) {
			if (Player.xy == xy)
				return Player;
		}
		return null;
	}


	PopperMan.Tile GetMapTileAt(int2 xy)
	{
		var map = GameObject.FindObjectOfType<Map>();
		return map [xy];
	}

	void PreExplodeBomb(Bomb bomb,System.Action<Player,Bomb> KillPlayer)
	{
		bomb.Duration = 0;

		System.Func<int2,bool> ProcessExplosionAndContinue = (xy) =>
		{
			//	explode bomb
			var HitBomb = GetBombAt (xy);
			if ( HitBomb!=null )
			{
				//	not already exploded (stop recursion)
				if ( HitBomb.Duration != 0)
				{
					PreExplodeBomb (HitBomb, KillPlayer);
				}
			}

			//	kill any players we hit
			var HitPlayer = GetPlayerAt (xy);
			if ( HitPlayer )
			{
				KillPlayer( HitPlayer, bomb );
			}

			//	check if we're blocked by the map
			var Tile = GetMapTileAt(xy);
			if ( Tile == PopperMan.Tile.Solid )
				return false;

			bomb.Flames.Add(xy);

			return true;
		};

		bomb.Flames.Add(bomb.xy);

		//	change of logic. walk explosion to detect different objects, halt the blast on solids
		for (int x = 1;	x <= bomb.Radius;	x++) {
			if (!ProcessExplosionAndContinue (new int2 (bomb.x + x, bomb.y)))
				break;
		}
		for (int x = 1;	x <= bomb.Radius;	x++) {
			if (!ProcessExplosionAndContinue (new int2 (bomb.x - x, bomb.y)))
				break;
		}
		for (int y = 1;	y <= bomb.Radius;	y++) {
			if (!ProcessExplosionAndContinue (new int2 (bomb.x, bomb.y+y)))
				break;
		}
		for (int y = 1;	y <= bomb.Radius;	y++) {
			if (!ProcessExplosionAndContinue (new int2 (bomb.x, bomb.y-y)))
				break;
		}

	}

	bool CanPlayerMoveTo(int2 xy)
	{
		var map = GameObject.FindObjectOfType<Map>();

		if ( GetBombAt(xy) != null )
			return false;

		if ( GetPlayerAt(xy) )
			return false;

		var MapTile = map[xy];
		if ( MapTile != PopperMan.Tile.Empty )
			return false;

		return true;
	}

	public void Tick()
	{
		Frame++;

		//	move each player
		var map = GameObject.FindObjectOfType<Map>();

		foreach ( var player in Players )
		{
			System.Action<int2> OnPlaceBomb = (xy) =>
			{
				var bomb = new Bomb();
				bomb.xy = xy;
				bomb.Player = player;
				bomb.StartFrame = Frame;
				bomb.Duration = player.BombDuration;
				bomb.Radius = player.BombRadius;
				Bombs.Add( bomb );
			};
			
			if ( !player.Alive )
			{
				if ( player.Input_JoinGame )
				{
					//	todo: be clever at placing them
					player.Alive = true;
					OnPlayerJoin.Invoke(player);
				}
			}
			else
			{
				player.PlaceBomb(map,this,OnPlaceBomb);
				player.Move( CanPlayerMoveTo );
			}

			player.ClearInput ();
		}

		System.Action<Player, Bomb> KillPlayer = (p, b) =>
		{
			p.Alive = false;
			OnPlayerDeathExplode.Invoke(p);
		};

		//	update bombs
		//	decrease all bomb times. if it pre-explodes, pre-explode others
		foreach ( var bomb in Bombs )
		{
			bomb.Duration--;
			if ( bomb.Duration > 0 )
				continue;

			PreExplodeBomb( bomb, KillPlayer );
		}

		var KilledPlayers = new List<Player>();

		//	now actually "explode" all those that were triggered. 
		//	restore to players bomb count
		//	kill players
		for (int i = Bombs.Count - 1; i >= 0; i--)
		{ 
			var bomb = Bombs[i];
			if ( bomb.Duration > 0 )
				continue;

			bomb.Player.BombCount++;
			if ( OnBombExplode != null )
				OnBombExplode.Invoke(bomb);
			Bombs.RemoveAt(i);
		}

		//	kill some players! end the game etc

		OnTickEnd.Invoke ();
	}
}
