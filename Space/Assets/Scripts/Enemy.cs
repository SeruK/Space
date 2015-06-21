using UnityEngine;
using System.Collections;

[RequireComponent( typeof(MovingEntity) )]
[RequireComponent( typeof(Unit) )]
public class Enemy : MonoBehaviour {
	public enum Behavior {
		None,
		Goomba
	}

	[SerializeField]
	private Behavior  behavior;
	[SerializeField]
	private float     sightRange = 2.0f;
	[SerializeField]
	private float     chaseRange = 2.0f;
	[SerializeField]
	private LayerMask sightMask = 0;
	
	private MovingEntity thisEntity;
	private Unit   thisUnit;

	private float   moveAroundTimer;
	private float   flipTimer;
	private Unit    targetUnit;
	private Vector2 targetPosition;

	protected void OnEnable() {
		if( thisEntity == null ) {
			thisEntity = GetComponent<MovingEntity>();
		}
		if( thisUnit == null ) {
			thisUnit = GetComponent<Unit>();
		}

		targetPosition = transform.position;
	}

	protected void OnDisable() {
		targetUnit = null;
	}

	protected void Update() {
		if( thisEntity == null || thisUnit == null || thisUnit.Dead ) {
			return;
		}

		if( behavior == Behavior.Goomba ) {
			UpdateGoomba();
		}
	}

	private void UpdateGoomba() {
		// If has no target, try to find a new one
		if( targetUnit == null ) {
			targetUnit = FindTarget();
		}

		Vector2 pos = transform.position;

		if( targetUnit == null ) {
			moveAroundTimer -= Time.deltaTime;
			flipTimer -= Time.deltaTime;

			if( moveAroundTimer <= 0.0f ) {
				moveAroundTimer = Random.Range( 2.0f, 10.0f );
				targetPosition = pos + Vector2.right * Random.Range( 0.5f, 2.0f );
			} else if( flipTimer <= 0.0f ) {
				flipTimer = Random.Range( 2.0f, 4.0f );
				thisEntity.Direction = Random.Range( -1, 1 );
			}
		} else {
			Vector2 targetUnitPos = targetUnit.transform.position;
			if( targetUnit.Dead || Vector2.Distance( pos, targetUnitPos ) > chaseRange ) {
				targetPosition = pos;
				targetUnit = null;
			} else {
				targetPosition = targetUnitPos;
			}
		}

		float distToTargetPos = Mathf.Abs( targetPosition.x - pos.x );
		if( distToTargetPos > 0.1f ) {
			thisEntity.RequestedHorizontalSpeed = Mathf.Sign( targetPosition.x - pos.x );
		} else {
			thisEntity.RequestedHorizontalSpeed = 0.0f;
			targetPosition = pos;
		}
	}

	private Unit FindTarget() {
		var hits = Physics2D.CircleCastAll( transform.position, sightRange, Vector2.up, 0.0f, sightMask );
		if( hits == null ) {
			return null;
		}

		foreach( var hit in hits ) {
			var foundUnit = hit.collider.GetComponentInChildren<Unit>();
			if( foundUnit != null ) {
				// Don't wanna chase ourselves
				if( foundUnit == thisUnit ) {
					continue;
				}
				if( foundUnit.Faction != thisUnit.Faction && !foundUnit.Dead  ) {
					return foundUnit;
				}
			}
		}

		return null;
	}

	protected void OnDrawGizmos() {
		Gizmos.color = targetUnit == null ? Color.white : Color.red;
		Gizmos.DrawWireSphere( transform.position, sightRange );
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireSphere( transform.position, chaseRange );
	}
}
