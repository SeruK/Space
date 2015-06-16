using UnityEngine;
using System.Collections;

public class Game : MonoBehaviour {
	[SerializeField]
	private GUIStyle inventoryStyle;

	public Entity Player;
	public SmoothFollow CameraController;
	public TextDisplay TextDisplay;
	public Inventory Inventory;
	private Localization localization;

	// TODO: This nicer
	public GameObject OrbPrefab;
	
	protected void OnEnable() {
		string tmxFilePath = System.IO.Path.Combine( Application.streamingAssetsPath, "test.tmx" );
		var tileMap = SA.TileMapTMXReader.ParseTMXFileAtPath( tmxFilePath );

		if( localization == null ) {
			localization = gameObject.AddComponent<Localization>();
		}
		localization.Load();

		if( Player != null ) {		
			Player.CharController.onControllerCollidedEvent += OnPlayerCollided;
			Player.CharController.onTriggerEnterEvent += OnPlayerEnteredTrigger;
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

	// Pickups

	private void DestroyPickup( Pickup pickup ) {
		Destroy( pickup.gameObject );
	}

	private class GUIState {
		public bool ShowInventory = false;
	}

	GUIState guiState = new GUIState();

	// TODO: Temp
	protected void OnGUI() {
		if( !guiState.ShowInventory ) {
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
