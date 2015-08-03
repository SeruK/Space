using UnityEngine;
using System.Collections.Generic;
using SA;

public class EntityManager : MonoBehaviour {
	public delegate void EntityCollisionHandler( Entity collidingEntity, RaycastHit2D hit );

	[SerializeField]
	private GameObject PlayerPrefab;
	[SerializeField]
	private GameObject PickupPrefab;
	[SerializeField]
	private GameObject GoombaPrefab;
	[SerializeField]
	private GameObject SpikePrefab;

	public EntityCollisionHandler OnEntityCollided;
	public Dictionary<int, Obstacle>.ValueCollection Obstacles {
		get { return obstacles.Values; }
	}

	private int currentEntityId;
	private Dictionary<int, BaseEntity> entities;
	private Dictionary<int, Obstacle> obstacles;

	protected void OnDisable() {
		RemoveAllEntities();
	}

	public void Reinitialize() {
		RemoveAllEntities();
		currentEntityId = 0;
		entities = new Dictionary<int, BaseEntity>();
		obstacles = new Dictionary<int, Obstacle>();
	}

	public T Spawn<T>( string name ) where T : Component {
		GameObject prefab =
			name == "Player" ? PlayerPrefab :
			name == "Goomba" ? GoombaPrefab :
			name == "Pickup" ? PickupPrefab : 
			name == "Spike" ? SpikePrefab : null;

		T obj = InstantiateObject<T>( prefab );
		if( obj != null ) {
			RespawnObject( obj );
		}
		return obj;
	}

	private void RespawnObject( Component obj ) {
		var obstacle = obj as Obstacle;
		if( obstacle != null ) {
			RespawnObstacle( obstacle );
			return;
		}

		var unit = obj as Unit;
		if( unit != null ) {
			RespawnUnit( unit );
			return;
		}
		var entity = obj as Entity;
		if( entity != null ) {
			RespawnEntity( entity );
			return;
		}
		var baseEntity = obj as BaseEntity;
		if( baseEntity != null ) {
			RespawnBaseEntity( baseEntity );
		}
	}

	public void RespawnObstacle( Obstacle obstacle ) {
		RespawnBaseEntity( obstacle );
		obstacles[ obstacle.EntityId ] = obstacle;
	}

	public void RespawnUnit( Unit unit ) {
		var entity = unit.GetComponent<Entity>();
		RespawnEntity( entity );
		unit.HealthPoints = unit.MaxHealthPoints;
		unit.Dead = false;
	}

	public void RespawnEntity( Entity entity ) {
		RespawnBaseEntity( entity );
		entity.Visual.enabled = true;
		
		var charController = entity.GetComponentInChildren<CharacterController2D>();
		
		var weakEntity = new System.WeakReference( entity as object );
		charController.onControllerCollidedEvent += ( hit ) => {
			if( weakEntity.IsAlive ) {
				if( OnEntityCollided != null ) {
					OnEntityCollided( weakEntity.Target as Entity, hit );
				}
			}
		};
	}

	private void RespawnBaseEntity( BaseEntity entity ) {
		if( entity.EntityId == -1 ) {
			++currentEntityId;
			entity.SetEntityId( currentEntityId );
			entities[ currentEntityId ] = entity;
		}
	}
	
	public bool DamageUnit( Unit damagedUnit, float damageAmount ) {
		if( damagedUnit.Invincible ) {
			return false;
		}
		damagedUnit.HealthPoints -= damageAmount;
		if( damagedUnit.HealthPoints < 0.0f || Mathf.Approximately( damagedUnit.HealthPoints, 0.0f ) ) {
			KillUnit( damagedUnit );
		}
		return !Mathf.Approximately( damageAmount, 0.0f );
	}
	
	public void KillUnit( Unit dyingUnit ) {
		var entity = dyingUnit.GetComponent<Entity>();
		entity.Visual.enabled = false;
		dyingUnit.HealthPoints = 0.0f;
		dyingUnit.Dead = true;
	}

	// Removal

	public void RemoveEntity( BaseEntity entity ) {
		if( obstacles.ContainsKey( entity.EntityId ) ) {
			obstacles.Remove( entity.EntityId );
		}
		if( entities.ContainsKey( entity.EntityId ) ) {
			entities.Remove( entity.EntityId );
		}
		RemoveEntityInternal( entity );
	}

	public void RemoveAllEntities() {
		if( entities == null ) {
			return;
		}

		obstacles.Clear();

		foreach( var entity in entities.Values ) {
			RemoveEntityInternal( entity );
		}
		entities.Clear();
	}

	private void RemoveEntityInternal( BaseEntity entity ) {
		if( entity == null ) {
			DebugUtil.LogWarn( "Entity was already null when attempting to remove it." );
			return;
		}
		entity.SetEntityId( -1 );
		if( entity ) {
			Destroy( entity.gameObject );
		}
	}

	// Instantiation

	private T InstantiateObject<T>( GameObject prefab ) where T : Component {
		var go = InstantiatePrefab( prefab );
		T comp = null;
		if( go != null ) {
			comp = go.GetComponent<T>();
			if( comp == null ) {
				Destroy( go );
				return null;
			}
		}
		return comp;
	}
	
	private GameObject InstantiatePrefab( GameObject prefab ) {
		return prefab == null ? null : GameObject.Instantiate( prefab );
	}
}
