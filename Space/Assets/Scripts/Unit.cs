using UnityEngine;
using System.Collections;

[RequireComponent( typeof(Entity) )]
public class Unit : MonoBehaviour {
	[SerializeField]
	private string localizedNameId;
	[SerializeField]
	private int faction;
	[SerializeField]
	private float maxHealthPoints;
	[SerializeField]
	private float damage;

	public string LocalizedNameId {
		get { return localizedNameId; }
	}
	public int Faction {
		get { return faction; }
	}
	public float MaxHealthPoints {
		get { return maxHealthPoints; }
	}
	public float Damage {
		get { return damage; }
	}
	public float HealthPoints {
		get { return healthPoints; }
		set { healthPoints = value; }
	}
	public bool Dead {
		get { return dead; }
		set { dead = value; }
	}
	public bool Invincible {
		get { return invincible; }
		set { invincible = value; }
	}

	private float healthPoints;
	private bool dead;
	private bool invincible;
}
