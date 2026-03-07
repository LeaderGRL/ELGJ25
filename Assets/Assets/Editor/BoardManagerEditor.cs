using UnityEditor;
using UnityEngine;
using Crossatro.Board;

/// <summary>
/// Custom editor for BoardManager that shows relevant fields
/// based on the selected GenerationMode.
///
/// - Random mode: shows width, height, black ratio
/// - Custom mode: shows mask field
/// </summary>
[CustomEditor(typeof(BoardManager))]
public class BoardManagerEditor : Editor
{
    // Serialized properties
    private SerializedProperty _board;
    private SerializedProperty _inputHandler;
    private SerializedProperty _boardController;
    private SerializedProperty _tileFactory;
    private SerializedProperty _enemyManager;
    private SerializedProperty _database;
    private SerializedProperty _generationMode;
    private SerializedProperty _mask;
    private SerializedProperty _gridWidth;
    private SerializedProperty _gridHeight;
    private SerializedProperty _blackRatio;
    private SerializedProperty _minDifficulty;
    private SerializedProperty _maxDifficulty;
    private SerializedProperty _theme;
    private SerializedProperty _inputField;
    private SerializedProperty _verboseLogging;
    private SerializedProperty _seed;

    private void OnEnable()
    {
        _board = serializedObject.FindProperty("_board");
        _inputHandler = serializedObject.FindProperty("_inputHandler");
        _boardController = serializedObject.FindProperty("_boardController");
        _tileFactory = serializedObject.FindProperty("_tileFactory");
        _enemyManager = serializedObject.FindProperty("_enemyManager");
        _database = serializedObject.FindProperty("_wordDatabase");
        _generationMode = serializedObject.FindProperty("_generationMode");
        _mask = serializedObject.FindProperty("_mask");
        _gridWidth = serializedObject.FindProperty("_gridWidth");
        _gridHeight = serializedObject.FindProperty("_gridHeight");
        _blackRatio = serializedObject.FindProperty("_blackRatio");
        _minDifficulty = serializedObject.FindProperty("_minDifficulty");
        _maxDifficulty = serializedObject.FindProperty("_maxDifficulty");
        _theme = serializedObject.FindProperty("_theme");
        _inputField = serializedObject.FindProperty("_inputField");
        _verboseLogging = serializedObject.FindProperty("_verboseLogging");
        _seed = serializedObject.FindProperty("_seed");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // Board Components
        EditorGUILayout.LabelField("Board Components", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(_board);
        EditorGUILayout.PropertyField(_inputHandler);
        EditorGUILayout.PropertyField(_boardController);
        EditorGUILayout.PropertyField(_tileFactory);
        EditorGUILayout.PropertyField(_enemyManager);

        EditorGUILayout.Space(10);

        // Crossword Generation
        EditorGUILayout.LabelField("Crossword Generation", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(_database);

        EditorGUILayout.Space(5);

        // Generation mode with colored label
        EditorGUILayout.PropertyField(_generationMode);
        bool isRandom = _generationMode.enumValueIndex == (int)GenerationMode.Random;

        EditorGUILayout.Space(5);

        if (isRandom)
        {
            // Random mode fields
            EditorGUILayout.HelpBox(
                "A new random mask is generated each play session.",
                MessageType.Info);

            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(_gridWidth, new GUIContent("Width"));
            EditorGUILayout.PropertyField(_gridHeight, new GUIContent("Height"));
            EditorGUILayout.PropertyField(_blackRatio, new GUIContent("Black Cell Ratio"));
            EditorGUI.indentLevel--;
        }
        else
        {
            // Custom mode fields
            EditorGUILayout.PropertyField(_mask, new GUIContent("Custom Mask"));

            if (_mask.objectReferenceValue == null)
            {
                EditorGUILayout.HelpBox(
                    "No mask assigned! Will fall back to random generation.",
                    MessageType.Warning);
            }
            else
            {
                EditorGUILayout.HelpBox(
                    "Using the assigned GridMask asset. " +
                    "Edit it by selecting it in the Project window.",
                    MessageType.Info);
            }
        }

        EditorGUILayout.Space(10);

        // Difficulty
        EditorGUILayout.LabelField("Difficulty", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(_minDifficulty);
        EditorGUILayout.PropertyField(_maxDifficulty);
        EditorGUILayout.PropertyField(_theme);

        EditorGUILayout.Space(10);

        // UI
        EditorGUILayout.LabelField("UI", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(_inputField);

        EditorGUILayout.Space(10);

        // Debug
        EditorGUILayout.LabelField("Debug", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(_verboseLogging);
        EditorGUILayout.PropertyField(_seed);

        serializedObject.ApplyModifiedProperties();
    }
}
