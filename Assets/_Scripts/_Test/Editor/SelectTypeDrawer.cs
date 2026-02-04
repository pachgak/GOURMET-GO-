//using System;
//using System.Linq;
//using UnityEditor;
//using UnityEngine;

//[CustomPropertyDrawer(typeof(SkillActionTest), true)]
//public class SelectTypeDrawer : PropertyDrawer
//{
//    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
//    {
//        // ---------------------------------------------------------
//        // เพิ่มส่วนนี้: เช็คก่อนว่าเป็น SerializeReference หรือไม่?
//        // ---------------------------------------------------------
//        if (property.propertyType != SerializedPropertyType.ManagedReference)
//        {
//            // ถ้าเป็นตัวแปรธรรมดา (เช่น List<DashAction>) ให้วาดแบบปกติแล้วจบการทำงานเลย
//            EditorGUI.PropertyField(position, property, label, true);
//            return;
//        }
//        // ---------------------------------------------------------

//        EditorGUI.BeginProperty(position, label, property);

//        Rect buttonRect = new Rect(position.x + EditorGUIUtility.labelWidth, position.y, position.width - EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight);

//        string typeName = "Null (Click to Select)";

//        // ตรงนี้แหละที่เคย Error เพราะมันพยายามดึงค่าจากตัวแปรธรรมดา
//        if (!string.IsNullOrEmpty(property.managedReferenceFullTypename))
//        {
//            typeName = property.managedReferenceFullTypename.Split(' ').LastOrDefault();
//            typeName = typeName.Split('.').LastOrDefault();
//        }

//        if (GUI.Button(buttonRect, typeName, EditorStyles.layerMaskField))
//        {
//            var targetType = typeof(SkillActionTest);
//            var types = AppDomain.CurrentDomain.GetAssemblies()
//                .SelectMany(s => s.GetTypes())
//                .Where(p => targetType.IsAssignableFrom(p) && !p.IsAbstract && p.IsClass);

//            GenericMenu menu = new GenericMenu();

//            menu.AddItem(new GUIContent("None"), false, () =>
//            {
//                property.managedReferenceValue = null;
//                property.serializedObject.ApplyModifiedProperties();
//            });

//            foreach (var type in types)
//            {
//                menu.AddItem(new GUIContent(type.Name), false, () =>
//                {
//                    property.managedReferenceValue = Activator.CreateInstance(type);
//                    property.serializedObject.ApplyModifiedProperties();
//                });
//            }
//            menu.ShowAsContext();
//        }

//        EditorGUI.PropertyField(position, property, label, true);
//        EditorGUI.EndProperty();
//    }

//    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
//    {
//        return EditorGUI.GetPropertyHeight(property, true);
//    }
//}