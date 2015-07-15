using UnityEngine;
using SA;

[RequireComponent( typeof(EntityManager) )]
[RequireComponent( typeof(TileMapGrid) )]
public class Game : MonoBehaviour {
	[SerializeField]
	private GUIStyle inventoryStyle;
	[SerializeField]
	private float lightRadius; //TODO: TEMP
	[SerializeField]
	private Easing.Algorithm lightAlgo;
	[SerializeField]
	private TileMapGrid tileMapGrid;
	[SerializeField]
	private SpriteRenderer spaceRenderer;
	[SerializeField]
	private ConversationGUI convoGUI;

	[SerializeField]
	private GameObject TileMapVisualPrefab;
	
	public SmoothFollow CameraController;
	public TextDisplay TextDisplay;
	public Inventory Inventory;

	private Localization localization;
	private Conversations conversations;
	private Quests quests;
	private Vector2 spawnPos;
	private float playerInvincibilityTimer;
	private Vector2i aimVector;
	private bool requestedDig;

	private EntityManager entityManager;
	private Entity player;
	private Unit playerUnit;

	private TilesetLookup tilesetLookup;

	protected void OnEnable() {
		if( spaceRenderer != null ) {
			spaceRenderer.material.renderQueue = 1000;
		}

		string tmxFilePath = System.IO.Path.Combine( Util.ResourcesPath, "test.tmx" );
		tilesetLookup = new SA.TilesetLookup();
		TileMap tileMap = SA.TileMapTMXReader.ParseTMXFileAtPath( tmxFilePath, tilesetLookup );
		DebugUtil.Log( "tilemap: " + tileMap );

		if( localization == null ) {
			localization = new Localization();
		}
		localization.Load();

		if( quests == null ) {
			quests = new Quests();
		}
		quests.Load( localization );

		if( conversations == null ) {
			conversations = new Conversations();
		}
		conversations.Load( localization );

		if( entityManager == null ) {
			entityManager = gameObject.GetComponent<EntityManager>();
		}
		entityManager.Reinitialize();
		entityManager.OnEntityCollided += OnEntityCollided;

		player = entityManager.Spawn<Entity>( "Player" );
		if( player != null ) {
			Camera.main.GetComponent<SmoothFollow>().target = player.CharController.transform;
			playerUnit = player.GetComponent<Unit>();
			player.CharController.onTriggerEnterEvent += OnPlayerEnteredTrigger;
		}

		tileMapGrid.CreateGrid();
		var midgroundTiles = tileMap.MidgroundLayer.Tiles;
		GenerateMountainTiles( ref midgroundTiles, 3, 3, 30, 30 );
		SetTileMapAt( tileMap, 3, 3 );
		GenerateTileMapAt( 1, 3 );
		GenerateTileMapAt( 2, 3 );
		GenerateTileMapAt( 4, 3 );
		GenerateTileMapAt( 5, 3 );
		// Dungeons
		GenerateTileMapAt( 1, 2 );
		GenerateTileMapAt( 2, 2 );
		GenerateTileMapAt( 3, 2 );
		GenerateTileMapAt( 4, 2 );
		GenerateTileMapAt( 5, 2 );

		RespawnPlayer( spawnPos );

		quests.StartQuest( "main_quest_01" );

		if( convoGUI != null ) {
			convoGUI.Reinitialize( localization );
			convoGUI.SetConvo( conversations.Convos[ "conv_test" ] );
		}
	}

	private void SetTileMapAt( TileMap tileMap, int x, int y ) {
		tileMapGrid.SetTileMapAt( tileMap, tilesetLookup, x, y );
		CreateTileMapObjects( tileMap );
	}

	private void GenerateTileMapAt( int gridX, int gridY ) {
		int w = 30; int h = 30;
		var tiles = new System.UInt32[ w * h ];

		if( gridY < 3 ) {
			GenerateDungeonTiles( ref tiles, gridX, gridY, w, h );
		} else {
			GenerateMountainTiles( ref tiles, gridX, gridY, w, h );
		}
		var tileLayer = new TileLayer( "Midground", 1.0f, true, null, tiles );
		var tileMap = new TileMap( new Size2i( w, h ), new Size2i( 20, 20 ), Color.clear, null, null, new TileLayer[] { tileLayer }, null, 0 );
		SetTileMapAt( tileMap, gridX, gridY );
	}

	private void GenerateMountainTiles( ref System.UInt32[] tiles, int gridX, int gridY, int w, int h ) {
		float freq = 0.1f;
		int octaves = 5;
		float lacunarity = 2.0f;//range(2.0f, 3.0f);
		float gain = 0.6f;//range(0.6f, 0.7f); // increases "noise", helps decrease the blockiness of it all
		float amplitude = 2.0f;//range(6.0f, 8.0f); // higher number decrease "thickness" of the paths created

		// Start from bottom, generate upwards until we hit something
		for( int x = 0; x < w; ++x ) {
			float offsetX = gridX * w + x;
			float a = Simplex.GenerateOne1D( offsetX, freq, octaves, lacunarity, gain, amplitude );
			int yHeight = Mathf.FloorToInt( ( h / 2 ) + ( h / 2 ) * a );
			yHeight = Mathf.Min( yHeight, h - 1 );
			for( int y = 0; y < yHeight; ++y ) {
				int tileIndex = x + y * w;
				if( Tile.UUID( tiles[ tileIndex ] ) != 0u ) {
					break;
				}
				tiles[ tileIndex ] = 16u;
			}
		}
	}

	private void GenerateDungeonTiles( ref System.UInt32[] tiles, int gridX, int gridY, int w, int h ) {
		float freq = 0.1f;
		int octaves = 5;
		float lacunarity = 2.0f;//range(2.0f, 3.0f);
		float gain = 0.8f;//range(0.6f, 0.7f); // increases "noise", helps decrease the blockiness of it all
		float amplitude = 10.0f;//range(6.0f, 8.0f); // higher number decrease "thickness" of the paths created

		Vector2i offset = new Vector2i( gridX * w, gridY * h );

		// Start from bottom, generate upwards until we hit something
		for( int x = 0; x < w; ++x ) {
			for( int y = 0; y < h; ++y ) {
				int tileIndex = x + y * w;
				if( Tile.UUID( tiles[ tileIndex ] ) != 0u ) {
					continue;
				}

				float a = Simplex.GenerateOne2D( offset.x + x, offset.y + y, freq, octaves, lacunarity, gain, amplitude );
				a = ( 1.0f + a ) / 2.0f;
				tiles[ tileIndex ] = a < 0.5f ? 0u : 16u;
			}
		}
	}

	private void CreateTileMapObjects( TileMap tileMap ) {
		if( tileMap.ObjectLayers == null ) {
			return;
		}

		foreach( var objectLayer in tileMap.ObjectLayers ) {
			foreach( var layerObject in objectLayer.Objects ) {
				Vector2 worldPos = tileMapGrid.LayerToWorldPos( tileMap, objectLayer, layerObject.Position );
				
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
	}

	protected void OnDisable() {
		if( Camera.main ) {
			var cameraScript = Camera.main.GetComponent<SmoothFollow>();
			if( cameraScript != null ) {
				cameraScript.target = null;
			}
		}
		if( player != null ) {
			player.CharController.onTriggerEnterEvent -= OnPlayerEnteredTrigger;
		}
		player = null;
		playerUnit = null;
		entityManager.RemoveAllEntities();
	}

	protected void Update() {
		UpdateInput();
		tileMapGrid.DoGlobalLight();

		if( player != null && tileMapGrid != null ) {
			Vector2i lightPos = EntityTilePos( player ) + new Vector2i( 0, 1 );

			tileMapGrid.DoLightSource( lightPos, lightRadius, Color.white, Easing.Mode.In, lightAlgo );
		}

		tileMapGrid.ApplyLightMap();

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

		if( requestedDig ) {
			Vector2i playerTilePos = EntityTilePos( player );
			Vector2i digPos = playerTilePos + aimVector;
			if( !TryDig( digPos ) ) {
				if( aimVector.x == 0 && aimVector.y != 0 ) {
					Vector2 tileCenterPos = TilePosToPos( playerTilePos ) + new Vector2( 0.5f, 0.5f );
					Vector2 playerPos = EntityPos( player );
					float diff = playerPos.x - tileCenterPos.x;
					digPos += new Vector2i( diff < 0.0f ? -1 : 1, 0 );
					TryDig( digPos );
				}
			}
		}
	}

	private bool TryDig( Vector2i digPos ) {
		if( Tile.UUID( tileMapGrid.TileAtTilePos( digPos ) ) == 0u ) {
			return false;
		}

		var tileMapVisual = tileMapGrid.TileMapVisualAtTilePos( digPos.x, digPos.y );
		if( tileMapVisual != null ) {
			var gridPos = tileMapGrid.TileMapTileBounds( tileMapVisual.TileMap ).origin;
			int localX = digPos.x - gridPos.x;
			int localY = digPos.y - gridPos.y;
			
			DebugUtil.Log( "Destroying: " + digPos + " | " + new Vector2i( localX, localY ) );
			tileMapVisual.UpdateTile( localX, localY, 0u );
			return true;
		}
		return false;
	}

	protected void UpdateInput() {
		aimVector.Set( 0, 0 );
		requestedDig = false;

		if( convoGUI != null ) {
			if( convoGUI.CurrentConvo != null && convoGUI.CurrentConvo.Pauses ) {
				if( Input.GetKeyDown( KeyCode.Space ) ) {
					convoGUI.ForwardCurrentEntry();
				}
				return;
			}
		}

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

		bool left  = Input.GetKey( KeyCode.LeftArrow );
		bool right = Input.GetKey( KeyCode.RightArrow );
		bool up    = Input.GetKey( KeyCode.UpArrow );
		bool down  = Input.GetKey( KeyCode.DownArrow );
		requestedDig = Input.GetKeyDown( KeyCode.W );

		aimVector.Set( left ? -1 : right ? 1 : 0, down ? -1 : up ? 1 : 0 );

		Vector2i aimTilePos = EntityTilePos( player ) + aimVector;

		bool aimingAtTile = Tile.UUID( tileMapGrid.TileAtTilePos( aimTilePos ) ) != 0u;

		player.RequestedHorizontalSpeed = aimingAtTile ? 0.0f : left ? -1.0f : right ? 1.0f : 0.0f;
		player.RequestedJump = Input.GetKey( KeyCode.Space );
		if( CameraController != null ) {
			CameraController.extraCameraOffset = new Vector3( 0.0f, 2.0f * -aimVector.y );
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
				Inventory.AddItem( pickup.ItemType, 1 );
				quests.AquiredItem( pickup.ItemType );
			}

			entityManager.RemoveEntity( pickup );
		}
	}
	
	private class GUIState {
		public bool ShowInventory = false;
		public bool DestroyRandomTile = false;
	}

	GUIState guiState = new GUIState();

	private Vector2 TilePosToPos( Vector2i tilePos ) {
		return new Vector2( ( tilePos.x * Constants.TILE_SIZE ) / Constants.PIXELS_PER_UNIT,
		                    ( tilePos.y * Constants.TILE_SIZE ) / Constants.PIXELS_PER_UNIT );
	}

	private Vector2i PosToTilePos( Vector2 pos ) {
		return new Vector2i( Mathf.FloorToInt( ( pos.x * Constants.PIXELS_PER_UNIT ) / Constants.TILE_SIZE ),
		                     Mathf.FloorToInt( ( pos.y * Constants.PIXELS_PER_UNIT ) / Constants.TILE_SIZE ) );
	}

	private Vector2i EntityTilePos( Entity entity ) {
		return PosToTilePos( EntityPos( entity ) );
	}

	private Vector2 EntityPos( Entity entity ) {
		return entity.transform.position + new Vector3( 0.0f, 0.5f );
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

	protected void OnDrawGizmos() {
		if( player == null ) {
			return;
		}

		Vector2i tilePos = EntityTilePos( player );

		Gizmos.color = Color.yellow;
		Vector2i aimTilePos = tilePos + aimVector;
		Vector2  aimPos = new Vector2( 0.5f, 0.5f ) + (Vector2)aimTilePos;
		Gizmos.DrawWireCube( aimPos, new Vector3( 1.0f, 1.0f ) );

		Gizmos.color = Color.white;
		Vector2  pos = new Vector2( 0.5f, 0.5f ) + (Vector2)tilePos;
		Gizmos.DrawWireCube( pos, new Vector3( 1.0f, 1.0f ) );
	}

	// TODO: Temp
	protected void OnGUI() {
		if( convoGUI != null ) {
			if( convoGUI.CurrentConvo != null ) {
				return;
			}
		}

		if( !guiState.ShowInventory ) {
			guiState.ShowInventory = GUILayout.Button( "Inventory" );
			if( player != null ) {
				GUILayout.Label( "Playerpos: " + EntityTilePos( player ) );
				GUILayout.Label( "PlayerTileMap: " + tileMapGrid.TileMapAtWorldPos( player.transform.position ) );
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
			guiState.DestroyRandomTile = GUILayout.Button( "Destroy random tile" );
			return;
		}

		GUILayout.BeginVertical();
		GUILayout.Space( 10.0f );
		for( int y = 0; y < Inventory.Height; ++y ) {
			GUILayout.BeginHorizontal();
			GUILayout.Space( 10.0f );
			for( int x = 0; x < Inventory.Width; ++x ) {
				GUILayout.BeginVertical();

				InventoryItem invItem = Inventory.ItemAt( x, y );
				Item.ItemType itemType = invItem.ItemType;

				string buttonText = "";
				Sprite sprite = Inventory.GetItemSprite( itemType, tilesetLookup );
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
