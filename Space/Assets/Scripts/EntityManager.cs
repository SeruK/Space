﻿using UnityEngine;
using System.Collections.Generic;
using SA;

public class EntityManager : MonoBehaviour {
	public delegate void EntityCollisionHandler( MovingEntity collidingEntity, RaycastHit2D hit );

	[SerializeField]
	private GameObject PlayerPrefab;
	[SerializeField]
	private GameObject PickupPrefab;
	[SerializeField]
	private GameObject GoombaPrefab;
	[SerializeField]
	private GameObject SpikePrefab;

	public EntityCollisionHandler OnEntityCollided;

	private int currentEntityId;
	private Dictionary<int, MovingEntity> entities;

	protected void OnEnable() {
		currentEntityId = 0;
		entities = new Dictionary<int, MovingEntity>();
	}

	protected void OnDisable() {
		RemoveAllEntities();
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
		var unit = obj as Unit;
		if( unit != null ) {
			RespawnUnit( unit );
		} else {
			var entity = obj as MovingEntity;
			if( entity != null ) {
				RespawnEntity( entity );
			}
		}
	}

	public void RespawnEntity( MovingEntity entity ) {
		if( entity.EntityId != -1 ) {
			++currentEntityId;
			entity.SetEntityId( currentEntityId );
			entities[ currentEntityId ] = entity;
		}
		entity.Visual.enabled = true;
		
		var charController = entity.GetComponentInChildren<CharacterController2D>();
		
		var weakEntity = new System.WeakReference( entity as object );
		charController.onControllerCollidedEvent += ( hit ) => {
			if( weakEntity.IsAlive ) {
				if( OnEntityCollided != null ) {
					OnEntityCollided( weakEntity.Target as MovingEntity, hit );
				}
			}
		};
	}
	
	public void RespawnUnit( Unit unit ) {
		var entity = unit.GetComponent<MovingEntity>();
		RespawnEntity( entity );
		unit.HealthPoints = unit.MaxHealthPoints;
		unit.Dead = false;
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
		var entity = dyingUnit.GetComponent<MovingEntity>();
		entity.Visual.enabled = false;
		dyingUnit.HealthPoints = 0.0f;
		dyingUnit.Dead = true;
	}

	// Removal

	public void RemoveEntity( MovingEntity entity ) {
		if( entities.ContainsKey( entity.EntityId ) ) {
			entities.Remove( entity.EntityId );
		}
		RemoveEntityInternal( entity );
	}

	public void RemoveAllEntities() {
		var entitiesList = entities.Values;
		entities.Clear();
		foreach( var entity in entitiesList ) {
			RemoveEntityInternal( entity );
		}
	}

	private void RemoveEntityInternal( MovingEntity entity ) {
		entity.SetEntityId( -1 );
		Destroy( entity.gameObject );
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
