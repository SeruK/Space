using UnityEngine;
using System.Collections;

public class Entity : MonoBehaviour {
	private int entityId = -1;

	public int EntityId {
		get { return entityId; }
	}

	// Just to prevent accidents and maybe guard for errors later
	public void SetEntityId( int entityId ) {
		this.entityId = entityId;
	}
}
