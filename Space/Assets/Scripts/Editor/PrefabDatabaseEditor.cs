using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

[CanEditMultipleObjects]
[CustomEditor( typeof( PrefabDatabase ) )]
public class PrefabDatabaseEditor : Editor {
	private bool expanded;
	private SerializedProperty keyValuePairs;
	private bool[] validFields;

	protected void OnEnable() {
		keyValuePairs = serializedObject.FindProperty( "keyValuePairs" );
	}

	public override void OnInspectorGUI() {
		EditorGUILayout.ObjectField( "Script", target, typeof(PrefabDatabase), allowSceneObjects:false );
		serializedObject.Update();

		if( true == ( expanded = EditorGUILayout.Foldout( expanded, "Prefabs" ) ) ) {
			DrawFields();
		}

		bool modified = serializedObject.ApplyModifiedProperties();

		if( modified ) {
			var nameValues = new string[ keyValuePairs.arraySize ];

			for( int i = 0; i < nameValues.Length; ++i ) {
				nameValues[ i ] = keyValuePairs.GetArrayElementAtIndex( i ).FindPropertyRelative( "Key" ).stringValue;
			}

			validFields = nameValues.Select( a => ( nameValues.Count( b => a == b ) == 1 ) ).ToArray();
		}
	}

	private void DrawFields() {
		if( keyValuePairs != null ) {
			for( int i = 0; i < keyValuePairs.arraySize; ++i ) {
				SerializedProperty prop = keyValuePairs.GetArrayElementAtIndex( i );
				DrawField( prop, i );
			}
			if( keyValuePairs.arraySize == 0 ) {
				if( GUILayout.Button( "+", GUILayout.Width( 40.0f ) ) ) {
					keyValuePairs.InsertArrayElementAtIndex( keyValuePairs.arraySize - 1 );
				}
			}
		}
	}

	private void DrawField( SerializedProperty prop, int index ) {
		EditorGUILayout.BeginHorizontal();

		if( validFields != null && index < validFields.Length && validFields[ index ] == false ) {
			GUI.color = Color.red;
		}
		SerializedProperty key = prop.FindPropertyRelative( "Key" );
		key.stringValue = EditorGUILayout.TextField( key.stringValue );
		GUI.color = Color.white;

		SerializedProperty value = prop.FindPropertyRelative( "Value" );
		value.objectReferenceValue = EditorGUILayout.ObjectField( value.objectReferenceValue, typeof(GameObject), allowSceneObjects:false );

		if( GUILayout.Button( "+", GUILayout.Width( 30.0f ) ) ) {
			keyValuePairs.InsertArrayElementAtIndex( index );
		}

		if( GUILayout.Button( "-", GUILayout.Width( 30.0f ) ) ) {
			keyValuePairs.DeleteArrayElementAtIndex( index );
		}

		if( GUILayout.Button( "▴", GUILayout.Width( 30.0f ) ) && index > 0) {
			keyValuePairs.MoveArrayElement( index, index - 1 );
		}

		if( GUILayout.Button( "▾", GUILayout.Width( 30.0f ) ) && index < keyValuePairs.arraySize - 1 ) {
			keyValuePairs.MoveArrayElement( index, index + 1 );
		}

		EditorGUILayout.EndHorizontal();
	}
}
