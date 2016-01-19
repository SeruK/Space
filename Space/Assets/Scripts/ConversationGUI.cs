using UnityEngine;
using UE = UnityEngine;
using UnityEngine.UI;
using SA;
using System;

public class ConversationGUI : SA.Behaviour {
	[SerializeField]
	private TextDisplay textDisplay;
	[SerializeField]
	private Transform buttonsHolder;
	[SerializeField]
	private GameObject buttonPrefab;
	[SerializeField]
	private int buttonCount;

	private Button[] buttons;

	public Conversation CurrentConvo {
		get { return currentConvo; }
	}

	private Conversations conversations;
	private Conversation currentConvo;
	private int currentConvoEntryIndex;
	private Localization localization;

	public void Reinitialize( Localization localization, Conversations conversations ) {
		this.conversations = conversations;

		if( currentConvo != null ) {
			textDisplay.ResetText();
		}

		buttons = new Button[ buttonCount ];
        for( int i = 0; i < buttonCount; ++i ) {
			GameObject buttonGO = Instantiate( buttonPrefab );
			buttonGO.transform.SetParent( buttonsHolder, worldPositionStays: true );
			Button button = buttonGO.GetComponent<Button>();
			buttons[ i ] = button;
			int buttonIndex = i;
			button.onClick.AddListener( () => { OnAnswerSelected( buttonIndex ); } );
        }

		SetButtons( null );

		this.localization = localization;

		SetConvo( null );
	}

	private void OnDisable() {
		for( int i = buttonsHolder.childCount - 1; i >= 0; --i ) {
			Destroy( buttonsHolder.GetChild( i ).gameObject );
		}
	}

	public void SetConvo( Conversation convo, int entryIndex = -1 ) {
		currentConvo = convo;
		currentConvoEntryIndex = entryIndex;
		if( currentConvoEntryIndex == -1 ) {
			NextConvoEntry();
		} else {
			SetEntry( currentConvoEntryIndex );
		}
	}

	public void ForwardCurrentEntry() {
		if( currentConvo == null ) {
			return;
		}

		if( textDisplay.TextFullyWritten ) {
			NextConvoEntry();
		} else {
			textDisplay.ForceFinishCurrentText();
		}
	}

	public void EndConvo() {
		if( currentConvo == null ) {
			return;
		}

		DebugLog( "Conversation complete" );
		textDisplay.ResetText();
		SetConvo( null );
		SetButtons( null );
	}

	protected void Update() {
		if( currentConvo == null ) {
			return;
		}

		if( !currentConvo.Pauses ) {
			if( textDisplay.TextFinishedDisplaying ) {
				NextConvoEntry();
			}
		}
	}

	private void OnAnswerSelected( int index ) {
		ConversationAnswer[] answers = currentConvo.Entries[ currentConvoEntryIndex ].Answers;
		UE.Debug.Assert( index >= 0 && index < answers.Length, "Answer index {0} out of range", index );
		ConversationAnswer answer = answers[ index ];
		if( string.IsNullOrEmpty( answer.Destination ) ) {
			NextConvoEntry();
		} else {
			Conversation newConvo;
			int newEntryIndex;
			if( !conversations.ResolveTag( answer.Destination, out newConvo, out newEntryIndex ) ) {
				DebugLogError( "Unable to resolve tag {0} in entry {1}", answer.Destination, currentConvo.Entries[ currentConvoEntryIndex ].ContentId );
				return;
			}
			SetConvo( newConvo, newEntryIndex );
        }
	}

	private void NextConvoEntry() {
		if( currentConvo == null ) {
			return;
		}

		if( ++currentConvoEntryIndex >= currentConvo.Entries.Length ) {
			EndConvo();
			return;
		}

		SetEntry( currentConvoEntryIndex );
	}

	private void SetEntry( int index ) {
		ConversationEntry entry = currentConvo.Entries[ index ];
		string talker = localization.Get( entry.Talker.Id );
		string content = localization.Get( entry.ContentId );
		string text = string.Format( "{0}:\n{1}", talker, content );
		float displayFor = currentConvo.Pauses ? -1.0f : 3.0f;
		textDisplay.TypeTextThenDisplayFor( text, displayFor, entry.Talker.TextColor, syllabalize: true );
		SetButtons( entry.Answers );
	}

	private void SetButtons( ConversationAnswer[] answers ) {
		for( int i = 0; i < buttons.Length; ++i ) {
			if( answers != null && i < answers.Length ) {
				ConversationAnswer answer = answers[ i ];
				buttons[ i ].gameObject.SetActive( true );
				buttons[ i ].GetComponentInChildren<Text>().text = localization.Get( answer.ContentId );
			} else {
				buttons[ i ].gameObject.SetActive( false );
			}
		}
	}
}
