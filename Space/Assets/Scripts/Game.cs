using UnityEngine;
using SA;
using System;
using System.Collections.Generic;
using OngoingQuest = Quests.OngoingQuest;
using ItemType = Item.ItemType;
using System.Linq;

[RequireComponent( typeof(TileMapGrid) )]
[RequireComponent( typeof(PrefabDatabase) )]
public class Game : SA.Behaviour {
	[SerializeField]
	private GUISkin guiSkin;
	[SerializeField]
	private GUIStyle inventoryStyle;
	[SerializeField]
	private GUIStyle debugStringStyle;
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
	private SpriteRenderer tileDamageSprite;
	
	public SmoothFollow CameraController;
	public TextDisplay TextDisplay;
	public Inventory Inventory;

	private ItemType equippedItemType {
		get { return Inventory.ItemAt( guiState.SelectedItem.x, guiState.SelectedItem.y ).ItemType;  }
	}

	private Localization localization;
	private Conversations conversations;
	private Quests quests;
	private Vector2 spawnPos;
	private float playerInvincibilityTimer;
	private Vector2i aimVector;
	private bool requestedDig;
	private bool requestedPlaceItem;
	private string displayedQuestId;
	private bool waitForSpaceUp;
	private Vector2i mouseTilePos;
	private float currentDigDamage;
	private Vector2i currentDigTile;

	private string currentDebugString;

	private EntityManager entityManager;
	private PrefabDatabase prefabDatabase;
	private Syllabificator syllabificator;
	private Entity player;
	private Unit playerUnit;

	private TilesetLookup tilesetLookup;

	protected void OnEnable() {
		try {
			if( debugStringStyle == null && guiSkin != null ) {
				debugStringStyle = guiSkin.label;
			}

			if( spaceRenderer != null ) {
				spaceRenderer.material.renderQueue = 1000;
			}

			if( localization == null ) {
				localization = new Localization();
			}
			localization.Load();

			if( quests == null ) {
				quests = new Quests();
			}
			quests.Load( localization );
			quests.OnQuestStarted += OnQuestStarted;
			quests.OnObjectiveCompleted += OnObjectiveCompleted;
			quests.OnQuestCompleted += OnQuestCompleted;

			if( conversations == null ) {
				conversations = new Conversations();
			}
			conversations.Load( localization );

			syllabificator = Syllabificator.CreateFromFile( Application.streamingAssetsPath + "/SyllableList.txt" );
			TextDisplay.Reinitialize( syllabificator );

			if( prefabDatabase == null ) {
				prefabDatabase = gameObject.GetComponent<PrefabDatabase>();
			}
			prefabDatabase.Reinitialize();

			if( entityManager == null ) {
				entityManager = new EntityManager();
			}
			entityManager.Reinitialize( prefabDatabase );
			entityManager.OnEntityCollided += OnEntityCollided;

			player = entityManager.Spawn<Entity>( "Player" );
			if( player != null ) {
				Camera.main.GetComponent<SmoothFollow>().target = player.CharController.transform;
				playerUnit = player.GetComponent<Unit>();
			}

			tileMapGrid.CreateGrid();
			tilesetLookup = new SA.TilesetLookup();
			var worldGen = new WorldGenerator( tileMapGrid, tilesetLookup, entityManager );
			worldGen.GenerateTileGrid();
			spawnPos = worldGen.SpawnPos;

			RespawnPlayer( spawnPos );

			if( convoGUI != null ) {
				convoGUI.Reinitialize( localization );
			}

			quests.StartQuest( "main_quest_01" );
		} catch( Exception exc ) {
			DebugLogException( exc );
			DebugLog( "Setup failed, disabling." );
			gameObject.SetActive( false );
		}
	}

	protected void OnDisable() {
		DebugLog( "Game.OnDisable()" );
		Shutdown();
	}

	protected void OnApplicationQuit() {
		DebugLog( "Game.OnApplicationQuit()" );
		Shutdown();
	}

	private void Shutdown() {
		DebugLog( "Game.Shutdown()" );

		if( Camera.main ) {
			var cameraScript = Camera.main.GetComponent<SmoothFollow>();
			if( cameraScript != null ) {
				cameraScript.target = null;
			}
		}
		if( player != null ) {
			player.CharController.onTriggerStayEvent -= OnPlayerStayTrigger;
		}
		player = null;
		playerUnit = null;
		entityManager.RemoveAllEntities();
		quests.OnQuestStarted -= OnQuestStarted;
		quests.OnObjectiveCompleted -= OnObjectiveCompleted;
		quests.OnQuestCompleted -= OnQuestCompleted;
	}

	protected void Update() {
		UpdateInput();
		tileMapGrid.DoGlobalLight();

		if( player != null && tileMapGrid != null ) {
			Vector2i lightPos = EntityTilePos( player ) + new Vector2i( 0, 1 );

			tileMapGrid.DoLightSource( lightPos, lightRadius, Color.white, Easing.Mode.In, lightAlgo );
		}

		tileMapGrid.ApplyLightMap();

		Vector2i playerTilePos = EntityTilePos( player );
		Vector2i aimPos = playerTilePos + aimVector;

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

		if( requestedDig && equippedItemType == ItemType.Drill ) {
			if( !TryDig( aimPos ) ) {
				if( aimVector.x == 0 && aimVector.y != 0 ) {
					Vector2 tileCenterPos = TilePosToPos( playerTilePos ) + new Vector2( 0.5f, 0.5f );
					Vector2 playerPos = EntityPos( player );
					float diff = playerPos.x - tileCenterPos.x;
					aimPos += new Vector2i( diff < 0.0f ? -1 : 1, 0 );
					TryDig( aimPos );
				}
			}
		} else if( requestedPlaceItem ) {
			InventoryItem item = Inventory.ItemAt( guiState.SelectedItem.x, guiState.SelectedItem.y );
			System.UInt32 uuid = Item.TileUUIDFromItem( item.ItemType, 0u );
			if( uuid != 0u ) {
				Inventory.RemoveSingleItem( guiState.SelectedItem );
				TryPlaceTile( aimPos, uuid );
			}
		}

		int numSteps = 2;
		int step = Mathf.CeilToInt( ( currentDigDamage / ( 1.0f / (float)numSteps ) ) );
		float alpha = step * ( 1.0f / (float)numSteps );

		Vector3 damagePos = tileMapGrid.TilePosToWorldPos( currentDigTile ) + new Vector2( Constants.TILE_SIZE_UNITS, Constants.TILE_SIZE_UNITS ) / 2.0f;
		tileDamageSprite.transform.parent.position = damagePos;


		tileDamageSprite.color = new Color( 1, 1, 1, currentDigDamage > 0.0f ? 1.0f : 0.0f );
		tileDamageSprite.transform.parent.localScale = new Vector2( 1.0f, 1.0f ) * Mathf.SmoothStep( 0.3f, 1.0f, alpha );
		
	}

	private bool TryDig( Vector2i digPos ) {
		System.UInt32 tileAtDigPos = Tile.UUID( tileMapGrid.TileAtTilePos( digPos ) );
		if( tileAtDigPos == 0u ) {
			return false;
		}

		var tileMapVisual = tileMapGrid.TileMapVisualAtTilePos( digPos.x, digPos.y );
		if( tileMapVisual != null ) {
			if( currentDigTile != digPos ) {
				currentDigTile = digPos;
				currentDigDamage = 0.0f;
			}

			// TODO: This properly
			currentDigDamage += Time.deltaTime * 3.0f;
			
			if( currentDigDamage < 1.0f ) {
				return true;
			}

			SetTile( tileMapVisual, digPos, 0u );
			ItemType tileDrop = Item.TileDrops( tileAtDigPos );
			if( tileDrop != ItemType.None ) {
				var pickup = entityManager.Spawn<Pickup>( "Pickup" );
				pickup.GetComponentInChildren<SpriteRenderer>().sprite = tilesetLookup.Tiles[ (int)tileAtDigPos ].TileSprite;
				pickup.ItemType = tileDrop;
				pickup.TileUUID = Item.IsTile( tileDrop ) ? Item.TileUUIDFromItem( tileDrop ) : 0u;
				pickup.noPickupTimer = 0.5f;
				pickup.transform.position = TilePosToPos( digPos ) + new Vector2( 0.0f, 0.5f );
				pickup.transform.localScale = new Vector2( 0.7f, 0.7f );
			}

			currentDigDamage = 0.0f;

			return true;
		}
		return false;
	}

	private bool TryPlaceTile( Vector2i tilePos, System.UInt32 tile ) {
		if( Tile.UUID( tileMapGrid.TileAtTilePos( tilePos ) ) != 0u ) {
			return false;
		}
		return TrySetTile( tilePos, tile );
	}

	private bool TrySetTile( Vector2i tilePos, System.UInt32 tile ) {
		var tileMapVisual = tileMapGrid.TileMapVisualAtTilePos( tilePos.x, tilePos.y );
		if( tileMapVisual == null ) {
			return false;
		}
		SetTile( tileMapVisual, tilePos, tile );
		return true;
	} 

	private void SetTile( TileMapVisual tileMapVisual, Vector2i tilePos, System.UInt32 tile ) {
		if( tile == 0u ) {
			var obstaclesToRemove = ( from Obstacle obstacle in entityManager.Obstacles
			                          where obstacle.LockedToTiles.Contains( tilePos )
			                          select obstacle ).ToArray();

			foreach( Obstacle obstacle in obstaclesToRemove ) {
				entityManager.RemoveEntity( obstacle );
			}
		}

		var gridPos = tileMapGrid.TileMapTileBounds( tileMapVisual.TileMap ).origin;
		int localX = tilePos.x - gridPos.x;
		int localY = tilePos.y - gridPos.y;
		tileMapVisual.UpdateTile( localX, localY, tile );
	}

	protected void UpdateInput() {
		mouseTilePos = tileMapGrid.WorldPosToTilePos( Camera.main.ScreenToWorldPoint( Input.mousePosition ) );

		{
			currentDebugString = null;
			RaycastHit2D hit = Camera.main.ScreenPointToRay2D( Input.mousePosition );
			if( hit.collider != null ) {
				var ent = hit.collider.GetComponent<BaseEntity>();
				if( ent != null ) {
					currentDebugString = ent.DebugInfo;
				}
			}
		}

		if( Input.GetKeyDown( KeyCode.Tab ) ) {
			guiState.Show = !guiState.Show;
		}

		if( Input.GetKeyDown( KeyCode.Home ) ) {
			guiState.ShowDebug = !guiState.ShowDebug;
		}

		bool standStill = Input.GetKey( KeyCode.LeftShift );
		aimVector.Set( 0, 0 );
		requestedDig = false;
		requestedPlaceItem = false;

		if( player != null ) {
			player.RequestedHorizontalSpeed = 0.0f;
			player.RequestedJump = false;
		}

		if( convoGUI != null ) {
			if( convoGUI.CurrentConvo != null && convoGUI.CurrentConvo.Pauses ) {
				if( Input.GetKeyDown( KeyCode.Space ) ) {
					waitForSpaceUp = true;
					convoGUI.ForwardCurrentEntry();
				}
				else if( Input.GetKeyDown( KeyCode.Escape ) ) {
					convoGUI.EndConvo();
				}
				return;
			}
		}

		if( Input.GetKeyDown( KeyCode.I ) ) {
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

		bool left = Input.GetKey( KeyCode.LeftArrow );
		bool right = Input.GetKey( KeyCode.RightArrow );
		bool up = Input.GetKey( KeyCode.UpArrow );
		bool down = Input.GetKey( KeyCode.DownArrow );
		requestedDig = Input.GetKey( KeyCode.W );

		aimVector.Set( left ? -1 : right ? 1 : 0, down ? -1 : up ? 1 : 0 );

		if( aimVector.Magnitude > 0 ) {
			requestedPlaceItem = Input.GetKeyDown( KeyCode.Q );
		}

		Vector2i aimTilePos = EntityTilePos( player ) + aimVector;

		bool aimingAtTile = Tile.UUID( tileMapGrid.TileAtTilePos( aimTilePos ) ) != 0u;

		player.RequestedHorizontalSpeed = aimingAtTile || standStill ? 0.0f : left ? -1.0f : right ? 1.0f : 0.0f;
		bool spacePressed = Input.GetKey( KeyCode.Space );
		if( waitForSpaceUp ) {
			if( !spacePressed ) {
				waitForSpaceUp = false;
			}
		}
		else {
			player.RequestedJump = spacePressed;
		}
		if( CameraController != null ) {
			CameraController.extraCameraOffset = new Vector3( 0.0f, 2.0f * -aimVector.y );
		}

		for( int i = 0; i < Inventory.Width; ++i ) {
			var keycode = (KeyCode)System.Enum.Parse( typeof( KeyCode ), "Alpha" + ( i + 1 ) );
			if( Input.GetKeyDown( keycode ) ) {
				guiState.SelectedItem = new Vector2i( i, 0 );
			}
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
			TypeTextThenDisplayFor( "Collided with " + obstacle.name, 3.0f );
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
				TypeTextThenDisplayFor( "Attacked by " + name, 3.0f );
				a.Invincible = true;
				playerInvincibilityTimer = 2.0f;
			}
		}
	}

	protected void OnPlayerStayTrigger( Collider2D collider ) {
		var pickup = collider.GetComponentInChildren<Pickup>();
		if( pickup != null && pickup.Pickupable ) {
			ItemType itemType = pickup.ItemType;
			if( itemType == ItemType.TILE ) {
				itemType = Item.ItemFromTileUUID( pickup.TileUUID );
			}

			if( Inventory.AddItem( itemType, 1 ) ) {
				if( TextDisplay != null ) {
					string lineToDisplay = localization.Get( pickup.LocalizedLineId );

					if( lineToDisplay == null && itemType != ItemType.None ) {
						lineToDisplay = "Picked up a " + GetItemName( itemType );
					}

					if( !string.IsNullOrEmpty( lineToDisplay ) ) {
						TypeTextThenDisplayFor( lineToDisplay, 3.0f );
					}
				}
				entityManager.RemoveEntity( pickup );
				quests.AquiredItem( itemType );
			}
		}
	}

	private void OnQuestStarted( OngoingQuest quest ) {
		displayedQuestId = quest.QuestId;

		if( !string.IsNullOrEmpty( quest.Quest.StartConvoId ) ) {
			Conversation convo = conversations.Convos[ quest.Quest.StartConvoId ];
			convoGUI.SetConvo( convo );
		} else {
			string title = localization.Get( quest.Quest.TitleId );
			TypeTextThenDisplayFor( "New quest:\n" + title, 3.0f );
		}
	}

	private void OnObjectiveCompleted( OngoingQuest quest, Objective objective ) {
		if( !string.IsNullOrEmpty( objective.EndConvoId ) ) {
			Conversation convo = conversations.Convos[ objective.EndConvoId ];
			convoGUI.SetConvo( convo );
		} else {
			string title = localization.Get( objective.TitleId );
			TypeTextThenDisplayFor( "Completed Objective:\n" + title, 3.0f );
		}
	}

	private void OnQuestCompleted( OngoingQuest quest ) {
		string title = localization.Get( quest.Quest.TitleId );
		TypeTextThenDisplayFor( "Completed Quest:\n" + title, 3.0f );
	}

	private void TypeTextThenDisplayFor( string text, float displayFor ) {
		if( convoGUI != null && convoGUI.CurrentConvo != null ) {
			return;
		}
		TextDisplay.TypeTextThenDisplayFor( text, displayFor );
	}

	// TODO: Temp
	private class GUIState {
		public bool Show = true;
		public bool ShowInventory;
		public Vector2i SelectedItem;
		public bool ShowDebug = true;
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
		player.CharController.onTriggerStayEvent += OnPlayerStayTrigger;
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

	private string GetItemName( Item.ItemType item ) {
		string itemName = localization.Get( Item.LocalizedNameId( item ) );
		if( itemName == null ) {
			itemName = Item.FallbackName( item );
		}
		return itemName;
	}

	// TODO: Temp
	protected void OnGUI() {
		if( guiSkin != null ) {
			GUI.skin = guiSkin;
		}

		if( convoGUI != null ) {
			if( convoGUI.CurrentConvo != null ) {
				return;
			}
		}

		if( !string.IsNullOrEmpty( currentDebugString ) ) {
			float w = 200.0f;
			float h = GUI.skin.label.CalcHeight( new GUIContent( currentDebugString ), w );
			Vector3 offset = new Vector3( 10.0f, 0.0f );
			Rect rect = new Rect( Util.ScreenPointToGUI( Input.mousePosition ) + offset, new Vector2( w, h ) );
			if( !Util.ScreenRect.Contains( rect.max ) ) {
				rect.position -= new Vector2( offset.x * 2.0f + rect.width, 0.0f );
			}
			GUI.Label( rect, currentDebugString, debugStringStyle );
		}

		if( !guiState.Show ) {
			return;
		}

		if( GUILayout.Button( "Inventory" ) ) {
			guiState.ShowInventory = !guiState.ShowInventory;
		}

		if( player != null && guiState.ShowDebug ) {
			GUILayout.Label( "Mousepos: " + mouseTilePos );
			GUILayout.Label( "Playerpos: " + EntityTilePos( player ) );
			GUILayout.Label( "Dig force: " + currentDigDamage );
			GUILayout.Label( "Dig tile: " + currentDigTile );
//			GUILayout.Label( "PlayerTileMap: " + tileMapGrid.TileMapAtWorldPos( player.transform.position ) );
		}

		if( !guiState.ShowInventory ) {
			if( playerUnit != null ) {
				float hpFrac = playerUnit.HealthPoints / playerUnit.MaxHealthPoints;
				float barWidth = 100.0f;
				float barHeight = 20.0f;
				GUI.DrawTexture( new Rect( Screen.width - 10.0f - barWidth, barHeight - 10.0f, barWidth, barHeight ), Texture2D.whiteTexture );
				GUI.color = Color.red;
				GUI.DrawTexture( new Rect( Screen.width - 10.0f - barWidth, barHeight - 10.0f, barWidth * hpFrac, barHeight ), Texture2D.whiteTexture );
				GUI.color = Color.white;
			}
		}

		GUILayout.BeginArea( new Rect( 0, 0, Screen.width, Screen.height ) );
		DrawCurrentQuest();
		DrawEquipmentSlots();
		GUILayout.EndArea();

		if( guiState.ShowInventory ) {
			DrawInventory();
		}
	}

	private void DrawCurrentQuest() {
		if( quests.CurrentQuests == null || string.IsNullOrEmpty( displayedQuestId ) ) {
			return;
		}

		OngoingQuest displayedQuest = quests.CurrentQuests.Find( ( OngoingQuest q ) => {
			return q.QuestId == displayedQuestId;
		} );

		if( displayedQuest == null ) {
			return;
		}

		GUILayout.BeginVertical();
		GUILayout.FlexibleSpace();

		GUILayout.Label( "Current Quest" );

		string questTitle = localization.Get( displayedQuest.Quest.TitleId );

		GUILayout.Label( questTitle );

		var incompleteObjectives = new List<Objective>();
		quests.AllIncompleteObjectives( displayedQuest, displayedQuest.Quest.Objectives, ref incompleteObjectives );
		foreach( var objective in incompleteObjectives ) {
			GUILayout.BeginHorizontal();
			GUILayout.Space( 10.0f );
			string title = localization.Get( objective.TitleId );
			GUILayout.Label( string.Format( "- {0}", title ) );
			GUILayout.EndHorizontal();
		}

		GUILayout.EndVertical();
	}

	private void DrawEquipmentSlots() {
		GUILayout.BeginVertical();
		GUILayout.Space( 10.0f );
		DrawInventoryRow( 0 );
		GUILayout.EndVertical();
	}

	private void DrawInventory() {
		GUILayout.BeginVertical();
		GUILayout.Space( 10.0f );
		for( int y = 0; y < Inventory.Height; ++y ) {
			DrawInventoryRow( y );
		}
		GUILayout.EndVertical();
	}

	private void DrawInventoryRow( int y ) {
		GUILayout.BeginHorizontal();
		GUILayout.Space( 10.0f );
		for( int x = 0; x < Inventory.Width; ++x ) {
			GUILayout.BeginVertical();
			
			InventoryItem invItem = Inventory.ItemAt( x, y );
			Item.ItemType itemType = invItem.ItemType;
			
			string buttonText = "";
			Sprite sprite = Inventory.GetItemSprite( itemType, tilesetLookup );
			bool toggled = guiState.SelectedItem == new Vector2i( x, y );
			toggled = GUILayout.Toggle( toggled, buttonText, GUILayout.Width( 48.0f ), GUILayout.Height( 48.0f ) );
			if( toggled ) {
				guiState.SelectedItem = new Vector2i( x, y );
			}
			
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
			
			string itemName = "";
			if( invItem.ItemType != ItemType.None ) {
				if( Item.StackAmount( invItem.ItemType ) == 1 ) {
					itemName = GetItemName( invItem.ItemType );
				} else {
					itemName = string.Format( "{0} x {1}", invItem.Amount, GetItemName( invItem.ItemType ) );
				}
			}
			GUILayout.Label( itemName, inventoryStyle, GUILayout.Width( lastRect.width ) );
			
			GUILayout.EndVertical();
		}
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
	}
}
