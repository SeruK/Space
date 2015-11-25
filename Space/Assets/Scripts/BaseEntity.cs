using UnityEngine;
using System.Collections;

public abstract class BaseEntity : MonoBehaviour {
	private int entityId = -1;
	protected string entityName;

	public int EntityId {
		get { return entityId; }
	}

	// Just to prevent accidents and maybe guard for errors later
	public void SetEntityId( int entityId ) {
		this.entityId = entityId;
	}

	public string EntityName {
		get {
			return entityName;
		}

		set {
			entityName = value;
		}
	}

	public virtual string DebugInfo {
		get { return "{0} ({1})\n- Id: {2}".Fmt( entityName, GetType().Name, entityId ); }
	}
}
