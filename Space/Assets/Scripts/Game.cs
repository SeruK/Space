using UnityEngine;
using SA;

[RequireComponent( typeof(EntityManager) )]
public class Game : MonoBehaviour {
	[SerializeField]
	private GUIStyle inventoryStyle;
	[SerializeField]
	private float lightRadius; //TODO: TEMP
	[SerializeField]
	private Easing.Algorithm lightAlgo;
	[SerializeField]
	private Material tileMaterial;

	[SerializeField]
	private GameObject TileMapVisualPrefab;
	
	public SmoothFollow CameraController;
	public TextDisplay TextDisplay;
	public Inventory Inventory;
	private Localization localization;
	private Vector2 spawnPos;
	private float playerInvincibilityTimer;

	private EntityManager entityManager;
	private Entity player;
	private Unit playerUnit;

	private TilesetLookup tilesetLookup;
	private TileMap tileMap;
	private TileMapVisual tileMapVisual;

	protected void OnEnable() {
		string tmxFilePath = System.IO.Path.Combine( Util.ResourcesPath, "test.tmx" );
		tilesetLookup = new SA.TilesetLookup();
		tileMap = SA.TileMapTMXReader.ParseTMXFileAtPath( tmxFilePath, tilesetLookup );
		DebugUtil.Log( "tilemap: " + tileMap );

		if( tileMapVisual != null ) {
			Destroy( tileMapVisual.gameObject );
		}
		var tileMapGO = GameObject.Instantiate( TileMapVisualPrefab );
		tileMapVisual = tileMapGO.GetComponent<TileMapVisual>();
		tileMapVisual.TileMaterial = tileMaterial;
		tileMapVisual.CreateWithTileMap( tileMap, tilesetLookup );

		if( localization == null ) {
			localization = gameObject.AddComponent<Localization>();
		}
		localization.Load();

		if( entityManager == null ) {
			entityManager = gameObject.GetComponent<EntityManager>();
		}
		entityManager.OnEntityCollided += OnEntityCollided;

		foreach( var objectLayer in tileMap.ObjectLayers ) {
			foreach( var layerObject in objectLayer.Objects ) {
				Vector2 worldPos = LayerToWorldPos( layerObject.Position );

				if( layerObject.ObjectType == "SpawnPoint" ) {
					spawnPos = worldPos;
				}

				if( layerObject.ObjectType == "Goomba" ) {
					var goomba = entityManager.Spawn<Unit>( "Goomba" );
					goomba.transform.position = worldPos;
				}

				if( layerObject.ObjectType == "Item" ) {
					var pickup = entityManager.Spawn<Pickup>( "Pickup" );
					pickup.transform.position = worldPos;
					if( layerObject.Properties.ContainsKey( "ItemType" ) ) {
						pickup.ItemType = Item.ItemTypeFromString( layerObject.Properties[ "ItemType" ] );
					}
				}

				if( layerObject.ObjectType == "Spike" ) {
					var obstacle = entityManager.Spawn<Obstacle>( "Spike" );
					obstacle.transform.position = worldPos;
				}
			}
		}

		player = entityManager.Spawn<Entity>( "Player" );
		if( player != null ) {
			Camera.main.GetComponent<SmoothFollow>().target = player.CharController.transform;
			playerUnit = player.GetComponent<Unit>();
			RespawnPlayer( spawnPos );
			player.CharController.onTriggerEnterEvent += OnPlayerEnteredTrigger;
		}
	}

	protected void OnDisable() {
		Camera.main.GetComponent<SmoothFollow>().target = null;
		if( player != null ) {
			player.CharController.onTriggerEnterEvent -= OnPlayerEnteredTrigger;
		}
		player = null;
		playerUnit = null;
		entityManager.RemoveAllEntities();
	}

	protected void Update() {
		UpdateInput();
		if( player != null && tileMapVisual != null ) {
			tileMapVisual.DoLightSource( EntityPos( player ) + new Vector2i( 0, -1 ), lightRadius, Color.white, Easing.Mode.In, lightAlgo );
		}
		if( playerUnit != null && playerInvincibilityTimer > 0.0f ) {
			playerInvincibilityTimer -= Time.deltaTime;
			if( playerInvincibilityTimer <= 0.0f ) {
				playerUnit.Invincible = false;
				player.Visual.color = Color.white;
			} else {
				playerUnit.Invincible = true;
				player.Visual.color = new Color( 0.8f, 0.0f, 0.0f );
			}
		}
	}

	protected void UpdateInput() {
		if( Input.GetKeyDown( KeyCode.Escape ) ) {
			guiState.ShowInventory = !guiState.ShowInventory;
		}

		if( player == null ) {
			return;
		}

		if( Input.GetKeyDown( KeyCode.R ) ) {
			RespawnPlayer( spawnPos );
		}
		if( playerUnit != null && playerUnit.Dead ) {
			player.RequestedHorizontalSpeed = 0.0f;
			player.RequestedJump = false;
			return;
		}

		player.RequestedHorizontalSpeed = Input.GetKey( KeyCode.LeftArrow ) ? -1.0f :
			Input.GetKey( KeyCode.RightArrow ) ? 1.0f : 0.0f;
		player.RequestedJump = Input.GetKey( KeyCode.UpArrow );
		if( Input.GetKey( KeyCode.DownArrow ) && CameraController != null ) {
			CameraController.extraCameraOffset = new Vector3( 0.0f, 2.0f, 0.0f );
		} else {
			CameraController.extraCameraOffset = Vector3.zero;
		}
	}

	private void OnEntityCollided( Entity collidingEntity, RaycastHit2D hit ) {
		var collidingUnit = collidingEntity.GetComponent<Unit>();

		if( collidingUnit == null ) {
			return;
		}

		var obstacle = hit.collider.GetComponentInChildren<Obstacle>();
		if( obstacle != null ) {
			HandleUnitObstacleCollision( collidingUnit, obstacle, hit.normal );
		}

		var unit = hit.collider.GetComponentInChildren<Unit>();
		// Handle all Unit collisions from both ends
		if( unit != null ) {
			HandleUnitUnitCollision( collidingUnit, unit, hit.normal );
			HandleUnitUnitCollision( unit, collidingUnit, -hit.normal );
		}
	}

	private void HandleUnitObstacleCollision( Unit unit, Obstacle obstacle, Vector2 normal ) {
		if( Vector2.Dot( normal, Vector2.up ) < 0.3f ) {
			return;
		}

		if( unit == playerUnit ) {
			TextDisplay.TypeTextThenDisplayFor( "Collided with " + obstacle.name, 3.0f );
		}

		if( obstacle.KnockForce > 0.0f ) {
			Vector3 force = normal * obstacle.KnockForce;
			unit.GetComponent<Entity>().CharController.move( force );
		}

		// TODO: Make the invulnerability thing per-unit
		if( unit == playerUnit && obstacle.Damage > 0.0f ) {
			bool wasDamaged = entityManager.DamageUnit( unit, obstacle.Damage );
			if( wasDamaged && unit == playerUnit ) {
				unit.Invincible = true;
				playerInvincibilityTimer = 2.0f;
			}
		}
	}

	private void HandleUnitUnitCollision( Unit a, Unit b, Vector2 normal ) {
		bool wasDamaged = entityManager.DamageUnit( a, b.Damage );
		if( wasDamaged ) {
			Vector3 force = normal * 0.05f + Vector2.up * 0.1f;
			a.GetComponent<Entity>().CharController.move( force );
			if( a == playerUnit ) {
				string name = localization.Get( b.LocalizedNameId );
				if( name == null ) {
					name = b.LocalizedNameId;
				}
				TextDisplay.TypeTextThenDisplayFor( "Attacked by " + name, 3.0f );
				a.Invincible = true;
				playerInvincibilityTimer = 2.0f;
			}
		}
	}

	protected void OnPlayerEnteredTrigger( Collider2D collider ) {
		var pickup = collider.GetComponentInChildren<Pickup>();
		if( pickup != null ) {
			if( TextDisplay != null ) {
				string lineToDisplay = localization.Get( pickup.LocalizedLineId );

				if( lineToDisplay == null && pickup.ItemType != Item.ItemType.None ) {
					string localizedLine = localization.Get( Item.LocalizedNameId( pickup.ItemType ) );
					lineToDisplay = "Picked up a " + ( localizedLine != null ? localizedLine : Item.FallbackName( pickup.ItemType ) );
				}

				if( !string.IsNullOrEmpty( lineToDisplay ) ) {
					TextDisplay.TypeTextThenDisplayFor( lineToDisplay, 3.0f );
				}
			}

			if( Inventory != null ) {
				Inventory.AddItem( pickup.ItemType );
			}

			entityManager.RemoveEntity( pickup );
		}
	}
	
	private class GUIState {
		public bool ShowInventory = false;
	}

	GUIState guiState = new GUIState();

	private Vector2i PosToTilePos( Vector2 pos ) {
		float pixelsPerUnit = 20.0f;
		return new Vector2i( Mathf.FloorToInt( ( pos.x * pixelsPerUnit ) / tileMap.TileSize.width ),
		                     Mathf.FloorToInt( ( pos.y * pixelsPerUnit ) / tileMap.TileSize.height ) );
	}

	private Vector2i EntityPos( Entity entity ) {
		return PosToTilePos( entity.transform.position );
	}

	private Vector2 LayerToWorldPos( Vector2 layerPos ) {
		float pixelsPerUnit = 20.0f;
		// Flipperoo
		layerPos.y = ( tileMap.Size.height * tileMap.TileSize.height ) - layerPos.y;
		layerPos /= pixelsPerUnit;
		return layerPos;
	}

	private void RespawnPlayer( Vector2 pos ) {
		if( player == null ) {
			return;
		}
		player.CharController.GetComponent<Transform>().position = pos;
		player.CharController.onTriggerEnterEvent += OnPlayerEnteredTrigger;
		if( playerUnit != null ) {
			entityManager.RespawnUnit( playerUnit );
		}
	}

	// TODO: Temp
	protected void OnGUI() {
		if( !guiState.ShowInventory ) {
			if( player != null ) {
				GUILayout.Label( "Playerpos: " + EntityPos( player ) );
			}

			if( playerUnit != null ) {
				float hpFrac = playerUnit.HealthPoints / playerUnit.MaxHealthPoints;
				float barWidth = 100.0f;
				float barHeight = 20.0f;
				GUI.DrawTexture( new Rect( Screen.width - 10.0f - barWidth, barHeight - 10.0f, barWidth, barHeight ), Texture2D.whiteTexture );
				GUI.color = Color.red;
				GUI.DrawTexture( new Rect( Screen.width - 10.0f - barWidth, barHeight - 10.0f, barWidth * hpFrac, barHeight ), Texture2D.whiteTexture );
				GUI.color = Color.white;
			}

			guiState.ShowInventory = GUILayout.Button( "Inventory" );
			return;
		}

		GUILayout.BeginVertical();
		GUILayout.Space( 10.0f );
		for( int y = 0; y < Inventory.Items.Rank; ++y ) {
			var items = Inventory.Items;
			int w = items.GetLength( y );

			GUILayout.BeginHorizontal();
			GUILayout.Space( 10.0f );
			for( int x = 0; x < w; ++x ) {
				GUILayout.BeginVertical();

				string buttonText = "";
				Sprite sprite = Inventory.GetItemSprite( items[ y, x ] );
				GUILayout.Button( buttonText, GUILayout.Width( 48.0f ), GUILayout.Height( 48.0f ) );

				var lastRect = GUILayoutUtility.GetLastRect();	

				if( sprite != null ) {
					var lastRectRel = lastRect;
					lastRectRel.xMin += 5.0f;
					lastRectRel.xMax -= 5.0f;
					lastRectRel.yMin += 5.0f;
					lastRectRel.yMax -= 5.0f;
					var textRect = sprite.textureRect;
					textRect.x /= sprite.texture.width;
					textRect.y /= sprite.texture.height;
					textRect.width /= sprite.texture.width;
					textRect.height /= sprite.texture.height;
					GUI.DrawTextureWithTexCoords( lastRectRel, sprite.texture, textRect );
				}
				var itemType = items[ y, x ];
				string itemName = itemType == Item.ItemType.None ? "" :
					System.Enum.GetName( typeof(Item.ItemType), itemType );
				GUILayout.Label( itemName, inventoryStyle, GUILayout.Width( lastRect.width ) );

				GUILayout.EndVertical();
			}
			GUILayout.EndHorizontal();
		}
		GUILayout.EndVertical();
	}
}
