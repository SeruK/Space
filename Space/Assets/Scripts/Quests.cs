using UnityEngine;
using System.Collections.Generic;
using ItemType = Item.ItemType;
using SimpleJSON;
using System.IO;

public class Objective {
	public enum ObjType {
		GetItem,
		KillEntity
	}
	
	public readonly ObjType  ObjectiveType;

	public readonly string   TitleId;

	public readonly ItemType ItemType;
	public readonly string   EntityType;

	public readonly int      Amount;

	public readonly Objective[] SubObjectives;

	public Objective( string titleId, ItemType itemType, int amount, Objective[] subObjectives ) {
		this.TitleId = titleId;
		this.ObjectiveType = ObjType.GetItem;
		this.ItemType = itemType;
		this.Amount = amount;
		this.SubObjectives = subObjectives;
	}

	public Objective( string titleId, string entityType, int amount, Objective[] subObjectives ) {
		this.TitleId = titleId;
		this.ObjectiveType = ObjType.KillEntity;
		this.EntityType = entityType;
		this.Amount = amount;
		this.SubObjectives = subObjectives;
	}
}

public class Quest {
	public readonly string TitleId;
	public readonly string DescriptionId;
	public readonly Objective[] Objectives;
	public readonly string NextQuestId;

	public Quest( string titleId, string descriptionId, Objective[] objectives, string nextQuestId ) {
		this.TitleId = titleId;
		this.DescriptionId = descriptionId;
		this.Objectives = objectives;
		this.NextQuestId = nextQuestId;
	}
}

public class Quests {
	private static readonly string GET_ITEM_FORMAT_ID = "objective_get_item_format";
	private static readonly string KILL_ENTITY_FORMAT_ID = "objective_kill_entity_format";

	private Dictionary<string, Quest> quests;

	public void Load( Localization localization ) {
		quests = new Dictionary<string, Quest>();

		var filePath = Path.Combine( Application.streamingAssetsPath, "quests.json" );
		using( var file = new StreamReader( filePath ) ) {
			JSONClass root = JSON.Parse( file.ReadToEnd() ).AsObject;

			foreach( KeyValuePair<string, JSONNode> kvp in root ) {
				string questId = kvp.Key;

				if( quests.ContainsKey( questId ) ) {
					DebugUtil.LogWarn( "Quest " + questId + " already existed" );
					continue;
				}

				JSONClass jsonQuest = kvp.Value.AsObject;

				string generatedTitleId = string.Format( "quest_{0}_title", questId );
				string generatedDescId  = string.Format( "quest_{0}_desc", questId );

				string title = jsonQuest[ "title" ] ?? generatedTitleId;
				string desc  = jsonQuest[ "desc" ] ?? generatedDescId;

				localization.Set( generatedTitleId, title );
				localization.Set( generatedDescId, desc );

				Objective[] objectives = ReadObjectives( localization, jsonQuest[ "objectives" ].AsArray, questId );

				string nextQuestId = jsonQuest[ "next_quest" ];

				quests[ questId ] = new Quest( generatedTitleId, generatedDescId, objectives, nextQuestId );
			}
		}
	}

	private Objective[] ReadObjectives( Localization localization, JSONArray jsonObjectives, string parentId ) {
		var objectivesList = new List<Objective>();

		int objectiveIdCounter = 0;
		foreach( JSONNode objectiveNode in jsonObjectives ) {
			string objectiveId = string.Format( "{0}_obj{1}", parentId, objectiveIdCounter.ToString().PadLeft( 2, '0' ) );

			JSONClass jsonObjective = objectiveNode.AsObject;
			string objectiveType = jsonObjective[ "objective_type" ];

			JSONNode jsonAmount = jsonObjective[ "amount" ];
			int amount = jsonAmount == null ? 0 : jsonAmount.AsInt;

			JSONNode jsonSubs = jsonObjective[ "sub_objectives" ];
			Objective[] subObjectives = null;
			if( jsonSubs != null ) {
				subObjectives = ReadObjectives( localization, jsonSubs.AsArray, objectiveId );
			}

			string titleId = string.Format( "{0}_title", objectiveId );
			string title = jsonObjective[ "title" ];

			if( objectiveType == "GetItem" ) {
				ItemType itemType = Item.ItemTypeFromString( jsonObjective[ "item_type" ] );
				string format = localization.Get( GET_ITEM_FORMAT_ID );
				title = title ?? string.Format( format, amount, GetItemName( localization, itemType ) );
				objectivesList.Add( new Objective( titleId, itemType, amount, subObjectives ) );
			} else if( objectiveType == "KillEntity" ) {
				// TODO: Get proper name
				string entityType = jsonObjective[ "entity_type" ];
				string entityName = localization.Get( entityType );
				string format = localization.Get( KILL_ENTITY_FORMAT_ID );
				title = title ?? string.Format( format, amount, entityName );
				objectivesList.Add( new Objective( titleId, entityType, amount, subObjectives ) );
			}

			localization.Set( titleId, title ?? "[OBJECTIVE_TITLE]" );

			++objectiveIdCounter;
		}

		return objectivesList.ToArray();
	}

	private string GetItemName( Localization localization, ItemType itemType ) {
		string name = Item.LocalizedNameId( itemType );
		name = localization.Get( name );
		if( name == null ) {
			name = Item.FallbackName( itemType );
		}
		return name ?? "[UNKNOWN_ITEM]";
	}
}
