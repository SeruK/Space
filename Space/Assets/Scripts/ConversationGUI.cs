using UnityEngine;
using System.Collections;
using SA;

public class ConversationGUI : MonoBehaviour {
	[SerializeField]
	private TextDisplay textDisplay;

	public Conversation CurrentConvo {
		get { return currentConvo; }
	}

	private Conversation currentConvo;
	private int currentConvoEntryIndex;
	private Localization localization;

	public void Reinitialize( Localization localization ) {
		if( currentConvo != null ) {
			textDisplay.ResetText();
		}

		this.localization = localization;

		SetConvo( null );
	}

	public void SetConvo( Conversation convo ) {
		currentConvo = convo;
		currentConvoEntryIndex = -1;
		NextConvoEntry();
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

	private void NextConvoEntry() {
		if( currentConvo == null ) {
			return;
		}

		if( ++currentConvoEntryIndex >= currentConvo.Entries.Length ) {
			DebugUtil.Log( "Conversation complete" );
			textDisplay.ResetText();
			SetConvo( null );
			return;
		}

		ConversationEntry entry = currentConvo.Entries[ currentConvoEntryIndex ];
		string talker = localization.Get( entry.Talker.Id );
		string content = localization.Get( entry.ContentId );
		string text = string.Format( "{0}:\n{1}", talker, content );
		float displayFor = currentConvo.Pauses ? -1.0f : 3.0f;
		textDisplay.TypeTextThenDisplayFor( text, displayFor, entry.Talker.TextColor );
	}
}
