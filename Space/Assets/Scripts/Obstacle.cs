using UnityEngine;
using SA;
using System.Linq;

public class Obstacle : BaseEntity {
	[SerializeField]
	private float damage;
	[SerializeField]
	private float knockForce;
	[SerializeField]
	private Vector2i[] lockedToTiles;

	public float Damage {
		get { return damage; }
	}
	public float KnockForce {
		get { return knockForce; }
	}
	public Vector2i[] LockedToTiles {
		get { return lockedToTiles; }
	}

	public void LockToTiles( params Vector2i[] tiles ) {
		lockedToTiles = tiles;
	}

	public override string DebugInfo {
		get {
			string lockedTo = lockedToTiles == null ? "none" : string.Join( ", ", lockedToTiles.Select( t => t.ToString() ).ToArray() );
			return string.Concat( base.DebugInfo, "\n- Locked to: {1}".Fmt( entityName, lockedTo ) );
		}
	}
}
