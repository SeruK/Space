using UnityEngine;
using System.Collections;

public class Obstacle : BaseEntity {
	[SerializeField]
	private float damage;
	[SerializeField]
	private float knockForce;

	public float Damage {
		get { return damage; }
	}
	public float KnockForce {
		get { return knockForce; }
	}
}
