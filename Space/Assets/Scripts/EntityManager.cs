using UnityEngine;
using System.Collections.Generic;
using SA;

public class EntityManager {
	public delegate void EntityCollisionHandler( Entity collidingEntity, RaycastHit2D hit );

	public EntityCollisionHandler OnEntityCollided;
	public IEnumerable<Obstacle> Obstacles {
		get { return obstacles.Values; }
	}

	private PrefabDatabase prefabDatabase;

	private int currentEntityId;
	private Dictionary<int, BaseEntity> entities;
	private Dictionary<int, Obstacle> obstacles;

	protected void OnDisable() {
		RemoveAllEntities();
		prefabDatabase = null;
	}

	public void Reinitialize( PrefabDatabase prefabDatabase ) {
		this.prefabDatabase = prefabDatabase;
		RemoveAllEntities();
		currentEntityId = 0;
		entities = new Dictionary<int, BaseEntity>();
		obstacles = new Dictionary<int, Obstacle>();
	}

	public T Spawn<T>( string name ) where T : Component {
		GameObject prefab = prefabDatabase[ name ];

		T obj = InstantiateObject<T>( prefab );
		if( obj != null ) {
			RespawnObject( obj, name );
		}
		return obj;
	}

	private void RespawnObject( Component obj, string name=null ) {
		if( name != null ) {
			var ent = obj as BaseEntity;
			if( ent != null ) {
				ent.EntityName = name;
			}
		}
		
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
			GameObject.Destroy( entity.gameObject );
		}
	}

	// Instantiation

	private T InstantiateObject<T>( GameObject prefab ) where T : Component {
		var go = InstantiatePrefab( prefab );
		T comp = null;
		if( go != null ) {
			comp = go.GetComponent<T>();
			if( comp == null ) {
				GameObject.Destroy( go );
				return null;
			}
		}
		return comp;
	}
	
	private GameObject InstantiatePrefab( GameObject prefab ) {
		return prefab == null ? null : GameObject.Instantiate( prefab );
	}
}
