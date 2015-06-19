﻿using UnityEngine;
using SA;

public class Game : MonoBehaviour {
	[SerializeField]
	private GUIStyle inventoryStyle;
	[SerializeField]
	private float lightRadius; //TODO: TEMP
	[SerializeField]
	private Easing.Algorithm lightAlgo;
	[SerializeField]
	private Material tileMaterial;

	public Entity Player;
	public SmoothFollow CameraController;
	public TextDisplay TextDisplay;
	public Inventory Inventory;
	private Localization localization;

	private TilesetLookup tilesetLookup;
	private TileMap tileMap;
	private TileMapVisual tileMapVisual;

	// TODO: This nicer
	[SerializeField]
	private GameObject PickupPrefab;
	[SerializeField]
	private GameObject TileMapVisualPrefab;
	[SerializeField]
	private GameObject GoombaPrefab;
	[SerializeField]
	private GameObject SpikePrefab;

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

		if( Player != null ) {		
			Player.CharController.onControllerCollidedEvent += OnPlayerCollided;
			Player.CharController.onTriggerEnterEvent += OnPlayerEnteredTrigger;
			foreach( var objectLayer in tileMap.ObjectLayers ) {
				foreach( var layerObject in objectLayer.Objects ) {
					if( layerObject.ObjectType == "SpawnPoint" ) {
						Player.CharController.transform.position = LayerToWorldPos( layerObject.Position );
					}
					if( layerObject.ObjectType == "Goomba" ) {
						InstantiateTileMapObject( GoombaPrefab, layerObject );
					}
					if( layerObject.ObjectType == "Item" ) {
						InstantiateTileMapObject<Pickup>( PickupPrefab, layerObject, ( pickup ) => {
							foreach( var prop in layerObject.Properties ) {
								if( prop.Name == "ItemType" ) {
									pickup.ItemType = Item.ItemTypeFromString( prop.Value );
								}
							}
						} );
					}
					if( layerObject.ObjectType == "Spike" ) {
						InstantiateTileMapObject( SpikePrefab, layerObject );
					}
				}
			}
		}
	}

	protected void OnDisable() {
		if( Player != null ) {
			Player.CharController.onControllerCollidedEvent -= OnPlayerCollided;
			Player.CharController.onTriggerEnterEvent -= OnPlayerEnteredTrigger;
		}
	}

	protected void Update() {
		UpdateInput();
		if( Player != null && tileMapVisual != null ) {
			tileMapVisual.DoLightSource( EntityPos( Player ) + new Vector2i( 0, -1 ), lightRadius, Color.white, Easing.Mode.In, lightAlgo );
		} 
	}

	protected void UpdateInput() {
		if( Input.GetKeyDown( KeyCode.Escape ) ) {
			guiState.ShowInventory = !guiState.ShowInventory;
		}

		if( Player == null ) {
			return;
		}

		Player.RequestedHorizontalSpeed = Input.GetKey( KeyCode.LeftArrow ) ? -1.0f :
			Input.GetKey( KeyCode.RightArrow ) ? 1.0f : 0.0f;
		Player.RequestedJump = Input.GetKey( KeyCode.UpArrow );
		if( Input.GetKey( KeyCode.DownArrow ) && CameraController != null ) {
			CameraController.extraCameraOffset = new Vector3( 0.0f, 2.0f, 0.0f );
		} else {
			CameraController.extraCameraOffset = Vector3.zero;
		}
	}

	protected void OnPlayerCollided( RaycastHit2D hit ) {
		var obstacle = hit.collider.GetComponentInChildren<Obstacle>();
		if( obstacle != null ) {
			if( obstacle.KnockForce > 0.0f ) {
				Vector3 force = hit.normal * obstacle.KnockForce;
				Player.CharController.move( force );
				TextDisplay.TypeTextThenDisplayFor( "Collided with " + obstacle.name, 3.0f );
			}
		}

		var unit = hit.collider.GetComponentInChildren<Unit>();
		if( unit != null ) {
			TextDisplay.TypeTextThenDisplayFor( "Collided with " + unit.DisplayName, 3.0f );
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

			DestroyPickup( pickup );
		}
	}

	private T InstantiateTileMapObject<T>( GameObject prefab, SA.TileMapObject layerObject, System.Action<T> setup ) where T : Component {
		var go = InstantiateTileMapObject( prefab, layerObject );
		T comp = null;
		if( go != null ) {
			comp = go.GetComponent<T>();
			if( comp == null ) {
				Destroy( go );
				return null;
			}
			if( setup != null ) {
				setup( comp );
			}
		}
		return comp;
	}

	private GameObject InstantiateTileMapObject( GameObject prefab, SA.TileMapObject layerObject ) {
		if( prefab == null ) {
			return null;
		}
		
		var go = GameObject.Instantiate( prefab );
		go.transform.position = LayerToWorldPos( layerObject.Position );
		return go;
	}
	
	// Pickups

	private void DestroyPickup( Pickup pickup ) {
		Destroy( pickup.gameObject );
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
	
	// TODO: Temp
	protected void OnGUI() {
		if( !guiState.ShowInventory ) {
			if( Player != null ) {
				GUILayout.Label( "Playerpos: " + EntityPos( Player ) );
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
