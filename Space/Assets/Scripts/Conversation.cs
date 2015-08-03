using UnityEngine;
using SimpleJSON;
using System.Collections.Generic;
using System.IO;

public class ConversationCharacter {
	public readonly string Id;
	public readonly Color TextColor;

	public ConversationCharacter( string id, UnityEngine.Color textColor ) {
		this.Id = id;
		this.TextColor = textColor;
	}
}

public class ConversationEntry {
	public readonly ConversationCharacter Talker;
	public readonly string ContentId;

	public ConversationEntry( ConversationCharacter talker, string contentId ) {
		this.Talker = talker;
		this.ContentId = contentId;
	}
}

public class Conversation {
	public readonly ConversationEntry[] Entries;
	public readonly bool Pauses;

	public Conversation( ConversationEntry[] entries, bool pauses ) {
		this.Entries = entries;
		this.Pauses = pauses;
	}
}

public class Conversations {
	private static readonly string UNKNOWN_TALKER_ID = "char_unknown";

	private Dictionary<string, ConversationCharacter> characters;
	public Dictionary<string, Conversation> Convos;

	public void Load( Localization localization ) {
		characters = new Dictionary<string, ConversationCharacter>();

		characters.Add( UNKNOWN_TALKER_ID, new ConversationCharacter( UNKNOWN_TALKER_ID, Color.white ) );

		Convos = new Dictionary<string, Conversation>();

		var filePath = Path.Combine( Application.streamingAssetsPath, "conversations.json" );
		using( var file = new StreamReader( filePath ) ) {
			JSONNode root = JSON.Parse( file.ReadToEnd() );
			JSONClass jsonCharacters = root[ "characters" ].AsObject;

			foreach( KeyValuePair<string, JSONNode> kvp in jsonCharacters ) {
				string id = kvp.Key;
				JSONNode jsonCharacter = kvp.Value;
				string colorString = jsonCharacter[ "text_color" ];
				Color textColor = Util.ParseColorString( colorString, Color.white );
				characters[ id ] = new ConversationCharacter( id, textColor );
			}

			var aliases = new Dictionary<string, ConversationCharacter>();
			JSONClass jsonAliases = root[ "aliases" ].AsObject;
			foreach( KeyValuePair<string, JSONNode> aliasKvp in jsonAliases ) {
				aliases[ aliasKvp.Value ] = characters[ aliasKvp.Key ];
			}

			JSONClass conversations = root[ "conversations" ].AsObject;

			foreach( KeyValuePair<string, JSONNode> kvp in conversations ) {
				string convoId = kvp.Key;
				JSONClass jsonConvo = kvp.Value.AsObject;

				JSONNode jsonPause = jsonConvo[ "pause" ];
				bool pauses = jsonPause == null ? false : jsonPause.AsBool;

				var entriesList = new List<ConversationEntry>();
				JSONArray jsonEntries = jsonConvo[ "entries" ].AsArray;
				int entryCounter = 0;
				foreach( JSONNode jsonEntry in jsonEntries ) {
					foreach( KeyValuePair<string, JSONNode> entryKvp in jsonEntry.AsObject ) {
						ConversationCharacter talker = null;
						if( aliases.ContainsKey( entryKvp.Key ) ) {
							talker = aliases[ entryKvp.Key ];
						} else if( characters.ContainsKey( entryKvp.Key ) ) {
							talker = characters[ entryKvp.Key ];
						} else {
							DebugUtil.LogWarn( "Unknown conversation talker @" + convoId + "[" + entryCounter + "] : " + entryKvp.Key );
							talker = characters[ UNKNOWN_TALKER_ID ];
						}

						string generatedId = string.Format( "conv_{0}_{1}",  convoId, entryCounter.ToString().PadLeft( 4, '0' ) );

						localization.Set( generatedId, entryKvp.Value );

						entriesList.Add( new ConversationEntry( talker, generatedId ) );
						++entryCounter;
					}
				}

				Convos.Add( convoId, new Conversation( entriesList.ToArray(), pauses ) );
			}
		}
	}
}
