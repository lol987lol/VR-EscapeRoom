using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections.Generic;
using System;

//TODO Хотелось бы сделать красивее инспектор, только не знаю что именно красить то..)

[CustomEditor(typeof(Chain))]
[CanEditMultipleObjects]
public class ChainEditor : Editor
{
    Chain t;
    public GUISkin skin;

    SerializedProperty prefabsList;
    SerializedProperty scaleFactorXY;
    SerializedProperty scaleFactorX;
    SerializedProperty scaleFactorY;
    SerializedProperty autoAspectRatio;
    SerializedProperty anchorOffset;
    SerializedProperty anchorOffsetDummy;
    SerializedProperty prefabId;
    SerializedProperty queueOffset;
    SerializedProperty chainMode;
    SerializedProperty snapToA;
    SerializedProperty snapToB;

    // Contents для свойств, чтобы не создавать заново постоянно
    GUIContent coilASpeed_content;
    GUIContent coilBSpeed_content;
    GUIContent fromA_content;
    GUIContent toB_content;
    GUIContent snapToA_content;
    GUIContent snapToB_content;
    GUIContent offsetA_content;
    GUIContent offsetB_content;
    GUIContent anchorOffset_content;
    GUIContent autoAspectRatio_content;
    GUIContent chainLinksSize_content;
    GUIContent sizeX_content;
    GUIContent sizeY_content;
    GUIContent chainMode_content;
    GUIContent prefabsList_content;
    GUIContent reloadPrefabsButton_content;
    GUIContent mass_content;
    GUIContent linearDrag_content;
    GUIContent angularDrag_content;
    GUIContent gravityScale_content;
    GUIContent enableBreak_content;
    GUIContent breakDistance_content;
    GUIContent updateIntervalBreak_content;
    GUIContent snapOffAButton_content;
    GUIContent snapOffBButton_content;


    [MenuItem("GameObject/2D Object/Chain2D")]
    // создаёт цепь на сцене.
    public static void CreateRope()
    {
        GameObject chain = new GameObject("chain");

        Chain chainScript = chain.AddComponent<Chain>();
        chainScript.init();

        EditorUtility.SetDirty(chainScript);
    }

    void OnEnable()
    {
        t = (Chain)target;

        prefabsList = serializedObject.FindProperty("prefabsList");
        scaleFactorXY = serializedObject.FindProperty("scaleFactorXY");
        scaleFactorX = serializedObject.FindProperty("scaleFactorX");
        scaleFactorY = serializedObject.FindProperty("scaleFactorY");
        autoAspectRatio = serializedObject.FindProperty("autoAspectRatio");
        anchorOffset = serializedObject.FindProperty("anchorOffset");
        anchorOffsetDummy = serializedObject.FindProperty("anchorOffsetDummy");
        prefabId = serializedObject.FindProperty("prefabId");
        queueOffset = serializedObject.FindProperty("queueOffset");
        chainMode = serializedObject.FindProperty("chainMode");
        snapToA = serializedObject.FindProperty("snapToA");
        snapToB = serializedObject.FindProperty("snapToB");

        skin = Resources.Load<GUISkin>("ChainSkinDefault");

        // Contents для свойств
        coilASpeed_content = new GUIContent("coil A speed", "Speed of coil A.");
        coilBSpeed_content = new GUIContent("coil B speed", "Speed of coil B.");
        fromA_content = new GUIContent("from A", "Starting point for attaching the chain.");
        toB_content = new GUIContent("to B", "Endpoint for attaching the chain.");
        snapToA_content = new GUIContent("snap to A", "Snap the beginning of the chain to the object A, with physics simulation.");
        snapToB_content = new GUIContent("snap to B", "Tie the end of the chain to the object B, with physics simulation.");
        offsetA_content = new GUIContent("offset A", "Offset from object A");
        offsetB_content = new GUIContent("offset B", "Offset from object B");
        anchorOffset_content = new GUIContent("anchor offset", "The offset between the links along the chain.");
        autoAspectRatio_content = new GUIContent("auto aspect ratio", "Aspect ratio for X and Y scale.");
        chainLinksSize_content = new GUIContent("chain links size", "Scale X and Y for every chains link.");
        sizeX_content = new GUIContent("scale X", "Scale X for every chains link.");
        sizeY_content = new GUIContent("scale Y", "Scale Y for every chains link.");
        chainMode_content = new GUIContent("Chain Mode", "Modes create different chains.");
        prefabsList_content = new GUIContent("Prefab list", "Prefab chain links, which will consist of a chain.");
        reloadPrefabsButton_content = new GUIContent("reload prefabs and rebuild chain", "Reload prefabs if they have been modified and rebuilt chain.");
        mass_content = new GUIContent("Mass", "The mass of the body. [0.0001, 1000000]");
        linearDrag_content = new GUIContent("Linear Drag", "The linear drag coefficient, 0 means no damping. [0, 1000000]");
        angularDrag_content = new GUIContent("Angular Drag", "The angular drag coefficient, 0 means no damping. [0, 1000000]");
        gravityScale_content = new GUIContent("Gravity Scale", "How much gravity affects this body. [-1000000, 1000000]");
        enableBreak_content = new GUIContent("enable chain break", "It enables checking the possibility of breaking the circuit, depending on the distance between the links of the chain.");
        breakDistance_content = new GUIContent("Break distance", "The maximum distance between the attaching chain links, after which the open circuit occurs.");
        updateIntervalBreak_content = new GUIContent("Update interval (sec)", "Refresh rate in seconds chain checking the possibility of rupture.");
        snapOffAButton_content = new GUIContent("snap off A", "Snap off chain from A in game mode.");
        snapOffBButton_content = new GUIContent("snap off B", "Snap off chain from B in game mode.");
    }


    public override void OnInspectorGUI()
    {
        //DrawDefaultInspector();

        serializedObject.Update();

        //EditorGUILayout.LabelField("--------- PLAY SETTINGS ---------", EditorStyles.centeredGreyMiniLabel);
        t.showPlaySettings = EditorGUILayout.Foldout(t.showPlaySettings, new GUIContent("GAME MODE SETTINGS"));

        if (t.showPlaySettings)
        {
            EditorGUILayout.LabelField("Coils", EditorStyles.boldLabel);
            t.coilASpeed = EditorGUILayout.FloatField(coilASpeed_content, t.coilASpeed);
            t.coilBSpeed = EditorGUILayout.FloatField(coilBSpeed_content, t.coilBSpeed);
            EditorGUILayout.Space();

            // отсоединение от привязки к А или В (в игре понадобится)
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(snapOffAButton_content)) t.snapOffA();
            if (GUILayout.Button(snapOffBButton_content)) t.snapOffB();
            EditorGUILayout.EndHorizontal();

        }
        drawLine();
        drawLine();
        //EditorGUILayout.LabelField("--------- DEBUG ---------", EditorStyles.centeredGreyMiniLabel);


        // EDIT MODE те свойства, который доступны только в режиме редактирования
        t.showCreateSettings = EditorGUILayout.Foldout(t.showCreateSettings, "CREATE SETTINGS");

        bool isPlaying = Application.isPlaying;
        if (isPlaying)
        {
            EditorGUILayout.HelpBox("Some creating settings are not available in play mode", MessageType.Info);
        }

        if (t.showCreateSettings)
        {
            if (isPlaying == false)
            {

                //EditorGUILayout.LabelField("--------- CREATE SETTINGS ---------", EditorStyles.centeredGreyMiniLabel);

                //----------------------------------------------------------
                //                  ОБЪЕКТЫ А И В

                EditorGUILayout.LabelField("Objects", EditorStyles.boldLabel);

                EditorGUI.BeginChangeCheck();//---------------------------------- 1 BeginChangeCheck

                EditorGUILayout.BeginHorizontal();

                t.A = (GameObject)EditorGUILayout.ObjectField(fromA_content, t.A, typeof(GameObject), true);
                if (GUILayout.Button("reset", GUILayout.ExpandWidth(false))) t.resetA();

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();

                t.B = (GameObject)EditorGUILayout.ObjectField(toB_content, t.B, typeof(GameObject), true);
                if (GUILayout.Button("reset", GUILayout.ExpandWidth(false))) t.resetB();

                EditorGUILayout.EndHorizontal();

                //                     SNAPING
                EditorGUILayout.PropertyField(snapToA, snapToA_content);
                EditorGUILayout.PropertyField(snapToB, snapToB_content);

                if (EditorGUI.EndChangeCheck())//---------------------------------- 1 EndChangeCheck
                {
                    serializedObject.ApplyModifiedProperties();
                    t.checkABObjects();
                    t.mainEditorUpdate();
                    EditorUtility.SetDirty(t);
                }

                //----------------------------------------------------------
                //                      OFFSET 
                drawLine();

                EditorGUILayout.LabelField("Offsets", EditorStyles.boldLabel);
                EditorGUI.BeginChangeCheck();//---------------------------------- 2 BeginChangeCheck

                EditorGUILayout.BeginHorizontal();
                t.offsetA = EditorGUILayout.Vector2Field(offsetA_content, t.offsetA);
                if (GUILayout.Button("reset", GUILayout.ExpandWidth(false))) t.offsetA = Vector2.zero;
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                t.offsetB = EditorGUILayout.Vector2Field(offsetB_content, t.offsetB);
                if (GUILayout.Button("reset", GUILayout.ExpandWidth(false))) t.offsetB = Vector2.zero;
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space();


                //                  ANCHOR OFFSET
                //------------------------------------------------------------
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(anchorOffsetDummy, anchorOffset_content);
                if (GUILayout.Button("reset")) anchorOffsetDummy.floatValue = 0f;
                anchorOffset.floatValue = anchorOffsetDummy.floatValue * 0.01f;
                EditorGUILayout.EndHorizontal();

                if (EditorGUI.EndChangeCheck())//---------------------------------- 2 EndChangeCheck
                {
                    serializedObject.ApplyModifiedProperties();

                    t.mainEditorUpdate();
                    EditorUtility.SetDirty(t); // надо вызвать, когда меняешь параметры выше вручную в инспекторе
                }

                drawLine();

                //----------------------------------------------------------
                //                  SCALE FACTOR
                EditorGUILayout.LabelField("Scale", EditorStyles.boldLabel);

                EditorGUI.BeginChangeCheck();//---------------------------------- 3 BeginChangeCheck

                EditorGUILayout.PropertyField(autoAspectRatio, autoAspectRatio_content);
                serializedObject.ApplyModifiedProperties();

                if (autoAspectRatio.boolValue == true)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(scaleFactorXY, chainLinksSize_content);
                    if (GUILayout.Button("reset"))
                    {
                        scaleFactorXY.floatValue = 1;
                        scaleFactorX.floatValue = scaleFactorXY.floatValue;
                        scaleFactorY.floatValue = scaleFactorXY.floatValue;
                    }
                    EditorGUILayout.EndHorizontal();

                    if (scaleFactorXY.floatValue < 0.1f) scaleFactorXY.floatValue = 0.1f;
                    scaleFactorX.floatValue = scaleFactorXY.floatValue;
                    scaleFactorY.floatValue = scaleFactorXY.floatValue;
                }
                else
                {
                    EditorGUILayout.PropertyField(scaleFactorX, sizeX_content);
                    EditorGUILayout.PropertyField(scaleFactorY, sizeY_content);

                    if (scaleFactorX.floatValue < 0.1f) scaleFactorX.floatValue = 0.1f;
                    if (scaleFactorY.floatValue < 0.1f) scaleFactorY.floatValue = 0.1f;

                    if (GUILayout.Button("reset"))
                    {
                        scaleFactorXY.floatValue = 1;
                        scaleFactorX.floatValue = scaleFactorXY.floatValue;
                        scaleFactorY.floatValue = scaleFactorXY.floatValue;
                    }
                }

                if (EditorGUI.EndChangeCheck())//---------------------------------- 3 EndChangeCheck
                {
                    serializedObject.ApplyModifiedProperties();
                    t.mainEditorUpdate();
                }

                drawLine();

                //----------------------------------------------------------
                //                  ВИД ЦЕПИ CHAIN MODE

                EditorGUILayout.LabelField("Chain Mode", EditorStyles.boldLabel);

                EditorGUI.BeginChangeCheck();//---------------------------------- 4 BeginChangeCheck
                EditorGUILayout.PropertyField(chainMode, chainMode_content);
                serializedObject.ApplyModifiedProperties();

                if (chainMode.enumValueIndex == (int)ChainMode.None)
                {
                    if (prefabsList.arraySize < 2) GUI.enabled = false;
                    EditorGUILayout.IntSlider(prefabId, 0, prefabsList.arraySize - 1, new GUIContent("prefab id"));
                    GUI.enabled = true;
                }
                else if (chainMode.enumValueIndex == (int)ChainMode.Queue)
                {
                    if (prefabsList.arraySize < 2) GUI.enabled = false;
                    EditorGUILayout.IntSlider(queueOffset, 0, prefabsList.arraySize - 1, "queue offset");
                    GUI.enabled = true;
                }
                else if (chainMode.enumValueIndex == (int)ChainMode.Random)
                {
                    if (GUILayout.Button("random seed")) { serializedObject.ApplyModifiedProperties(); t.rebuildRope(); }
                }

                if (EditorGUI.EndChangeCheck())//---------------------------------- 4 EndChangeCheck
                {
                    serializedObject.ApplyModifiedProperties();
                    t.rebuildRope();
                }

                drawLine();
                //----------------------------------------------------------
                //                  МАССИВ С ПРЕФАБАМИ ЗВЕНЬЕВ

                EditorGUILayout.LabelField("Prefabs chain links .  .  .  .  .  .  .", EditorStyles.boldLabel);

                EditorGUI.BeginChangeCheck();//---------------------------------- 5 BeginChangeCheck

                EditorGUILayout.PropertyField(prefabsList, prefabsList_content, true);
                serializedObject.ApplyModifiedProperties(); // ОБЯЗАТЕЛЬНО нужно, после редактирования каждого сериализуемого свойства, если от этого свойства зависит дальнейшая логика в скрипте

                bool reload = GUILayout.Button(reloadPrefabsButton_content);

                if (EditorGUI.EndChangeCheck())//---------------------------------- 5 EndChangeCheck
                {
                    anchorOffset.floatValue = anchorOffsetDummy.floatValue = 0f; // нужно сбросить смещение звеньев, так как зависает, если новые звенья стали маленькими
                    serializedObject.ApplyModifiedProperties();

                    // если в массиве были реальные изменения, то перестроить цепь и запомнить текущий состав массива
                    if (t.prefabListAndCloneIsDifferent() || reload == true)
                    {
                        serializedObject.ApplyModifiedProperties();
                        t.rebuildRope();
                    }

                }

                //если у префаба нет коллайдера, то написать предупреждение
                if (Event.current.type != EventType.DragPerform) // иначе ошибка... ArgumentException: GUILayout: Mismatched LayoutGroup.DragPerform  БАГ?
                {
                    string prefabNames = t.namePrefabsWitoutColliders();
                    if (prefabNames.Length != 0)
                        EditorGUILayout.HelpBox("Some prefabs of chain link do not have colliders2d" + prefabNames, MessageType.Warning);
                }
                //----------------------------------------------------------
                EditorGUILayout.Space();
                drawLine();
            }

            //----------------------------------------------------------
            //                          RIGID BODY SETTINGS

            EditorGUI.BeginChangeCheck();//---------------------------------- 0 BeginChangeCheck

            EditorGUILayout.LabelField("Chain Physics Settings", EditorStyles.boldLabel);

            t.rbSettings.mass = EditorGUILayout.FloatField(mass_content, t.rbSettings.mass);
            if (t.rbSettings.mass < 0.0001f) t.rbSettings.mass = 0.0001f;

            t.rbSettings.linearDrag = EditorGUILayout.FloatField(linearDrag_content, t.rbSettings.linearDrag);
            if (t.rbSettings.linearDrag < 0) t.rbSettings.linearDrag = 0;

            t.rbSettings.angularDrag = EditorGUILayout.FloatField(angularDrag_content, t.rbSettings.angularDrag);
            if (t.rbSettings.angularDrag < 0) t.rbSettings.angularDrag = 0;

            t.rbSettings.grav = EditorGUILayout.FloatField(gravityScale_content, t.rbSettings.grav);

            EditorGUILayout.Space();

            t.enableBreakCheck = EditorGUILayout.Toggle(enableBreak_content, t.enableBreakCheck);
            if (t.enableBreakCheck)
            {
                t.rbSettings.breakDistance = EditorGUILayout.FloatField(breakDistance_content, t.rbSettings.breakDistance);
                if (t.rbSettings.breakDistance < 0) t.rbSettings.breakDistance = 0;

                t.updateBreakTick = EditorGUILayout.FloatField(updateIntervalBreak_content, t.updateBreakTick);
                if (t.updateBreakTick < 0) t.updateBreakTick = 0;
            }
            if (GUILayout.Button("reset"))
            {
                t.setSettingsRBDefault();
            }
            if (EditorGUI.EndChangeCheck()) //---------------------------------- 0 EndChangeCheck
            {
                serializedObject.ApplyModifiedProperties();
                t.updateChainSettingsRB2D();
            }

            drawLine();

            if (!isPlaying)
            {
                EditorGUILayout.LabelField("Misk", EditorStyles.boldLabel);
                EditorGUI.BeginChangeCheck();               //---------------------------------- 6 BeginChangeCheck
                t.handleSize = EditorGUILayout.FloatField("handleSize", t.handleSize);
                if (t.handleSize < 0.1) t.handleSize = 0.1f;
                if (EditorGUI.EndChangeCheck()) EditorUtility.SetDirty(t);//---------------------------------- 6 EndChangeCheck

                drawLine();
            }

        }
    }


    void OnInspectorUpdate()
    {
        Repaint();
    }

    void OnSceneGUI()
    {
        if (Application.isPlaying) return;

        //это нужно для ситуации, когда цепь была привязана к чему-то, но это что-то удалили 
        t.checkABObjects();


        Vector3 posA = t.A.transform.position;
        Vector3 posB = t.B.transform.position;

        Vector3 offsetAGlob = t.A.transform.TransformPoint(t.offsetA);
        Vector3 offsetBGlob = t.B.transform.TransformPoint(t.offsetB);

        // круги на месте А и В
        Handles.color = new Color(1, 1, 0, 0.1f);
        Handles.DrawSolidDisc(posA, Vector3.forward, 0.5f * t.handleSize);
        Handles.DrawSolidDisc(posB, Vector3.forward, 0.5f * t.handleSize);


        // перемещение для А и В
        Handles.color = Color.green;
        posA = Handles.FreeMoveHandle(posA, Quaternion.identity, 0.5f * t.handleSize, Vector3.zero, Handles.CircleCap);
        posB = Handles.FreeMoveHandle(posB, Quaternion.identity, 0.5f * t.handleSize, Vector3.zero, Handles.CircleCap);

        Vector3 dirAtoB = (posA - posB).normalized;

        if (t.isSnapToA || t.isSnapToB)
        {
            skin.label.fontSize = 10;
            skin.label.fontStyle = FontStyle.Normal;
            skin.label.normal.textColor = Color.white;
            skin.label.alignment = TextAnchor.LowerLeft;

            string snapMessA = t.A.name == "tempA" ? "    (snap)" : "    (snapped to " + t.A.name + ")";
            string snapMessB = t.B.name == "tempB" ? "    (snap)" : "    (snapped to " + t.B.name + ")";
            if (t.isSnapToA) Handles.Label(posA + dirAtoB * (0.8f * t.handleSize), snapMessA, skin.label);
            if (t.isSnapToB) Handles.Label(posB - dirAtoB * (0.8f * t.handleSize), snapMessB, skin.label);
        }

        // перемещение для offsetA и offsetВ
        Handles.color = Color.green;
        offsetAGlob = Handles.FreeMoveHandle(offsetAGlob, Quaternion.identity, 0.1f * t.handleSize, Vector3.zero, Handles.CircleCap);
        offsetBGlob = Handles.FreeMoveHandle(offsetBGlob, Quaternion.identity, 0.1f * t.handleSize, Vector3.zero, Handles.CircleCap);

        // круг и крестик на смещениях
        Handles.color = new Color(1, 1, 1, 0.2f);
        Handles.DrawSolidDisc(offsetAGlob, Vector3.forward, 0.1f * t.handleSize);
        Handles.DrawSolidDisc(offsetBGlob, Vector3.forward, 0.1f * t.handleSize);
        drawCross(offsetAGlob, 0.2f * t.handleSize, Color.white);
        drawCross(offsetBGlob, 0.2f * t.handleSize, Color.white);

        t.offsetA = t.A.transform.InverseTransformPoint(offsetAGlob);
        t.offsetB = t.B.transform.InverseTransformPoint(offsetBGlob);

        skin.label.fontSize = 10;
        skin.label.fontStyle = FontStyle.Normal;
        skin.label.alignment = TextAnchor.MiddleLeft;

        Handles.color = Color.white;
        if (t.offsetA != Vector2.zero)
        {
            Handles.Label(offsetAGlob + Vector3.right * 0.2f, "offset A", skin.label);
            Handles.DrawDottedLine(posA, offsetAGlob, 2f);
        }

        if (t.offsetB != Vector2.zero)
        {
            Handles.Label(offsetBGlob + Vector3.right * 0.2f, "offset B", skin.label);
            Handles.DrawDottedLine(posB, offsetBGlob, 2f);
        }

        //рисуем пунктир от А до В, просто для наглядности, если звеньев ещё нет
        if (t.linksCount == 0) Handles.DrawDottedLine(offsetAGlob, offsetBGlob, 5f);


        t.A.transform.position = posA;
        t.B.transform.position = posB;

        //новая фишка - быстрая привязка. Нажимает на цепь и любой объект и возле А и В появляются маркеры, кликнув по которым А или В перемещается в позицию второго выделенного объекта. Но маркер какой-то не красивый, надо бы заменить на что-то более наглядное, может кнопку.
        if (Selection.transforms.Length == 2)
        {
            int chainInd = Array.IndexOf(Selection.transforms, t.gameObject.transform);
            int secondInd;
            if (chainInd != -1)
            {
                secondInd = chainInd == 0 ? 1 : 0;

                float circleSize = 0.2f * t.handleSize;
                Vector3 rightNormalAtoB = new Vector3(-dirAtoB.y, dirAtoB.x, dirAtoB.z);
                Vector3 centerDiskA = t.A.transform.position + rightNormalAtoB * -0.8f * t.handleSize;
                Vector3 centerDiskB = t.B.transform.position + rightNormalAtoB * -0.8f * t.handleSize;

                //рисуем кружочки привязки возле А и В
                Handles.color = Color.red;

                Handles.DrawSolidDisc(centerDiskA, Vector3.forward, circleSize);
                Handles.DrawSolidDisc(centerDiskB, Vector3.forward, circleSize);

                Handles.color = Color.black;

                Handles.DrawSolidDisc(centerDiskA, Vector3.forward, circleSize * 0.5f);
                Handles.DrawSolidDisc(centerDiskB, Vector3.forward, circleSize * 0.5f);

                // кнопки привязки возле А и В
                bool butA = Handles.Button(centerDiskA, Quaternion.identity, circleSize, circleSize, Handles.CircleCap);
                bool butB = Handles.Button(centerDiskB, Quaternion.identity, circleSize, circleSize, Handles.CircleCap);

                if (butA)
                {
                    t.defineABObjects(Selection.transforms[secondInd].gameObject, t.B);
                    Selection.activeTransform = t.gameObject.transform;
                    EditorUtility.SetDirty(t);
                }

                if (butB)
                {
                    t.defineABObjects(t.A, Selection.transforms[secondInd].gameObject);
                    Selection.activeTransform = t.gameObject.transform;
                    EditorUtility.SetDirty(t);
                }

            }

        }

        // отрисовка лейбов для А и В, чтобы они были поверх всех хендлов
        skin.label.fontSize = 20;
        skin.label.fontStyle = FontStyle.Bold;
        skin.label.normal.textColor = Color.green;
        skin.label.alignment = TextAnchor.MiddleCenter;

        Handles.Label(posA + dirAtoB * (0.8f * t.handleSize), "A", skin.label);
        Handles.Label(posB - dirAtoB * (0.8f * t.handleSize), "B", skin.label);

        t.mainEditorUpdate();
    }


    // вспомогательные методы

    // рисует крестик с попомью  Handles.DrawLine
    void drawCross(Vector3 pos, float radius, Color color)
    {
        Color currentColor = Handles.color;
        Handles.color = color;

        Handles.DrawLine(pos + Vector3.left * radius, pos + Vector3.right * radius);
        Handles.DrawLine(pos + Vector3.up * radius, pos + Vector3.down * radius);

        Handles.color = currentColor;
    }
    // рисует линию используя GUI box
    void drawLine()
    {
        GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(2) });
    }

}
