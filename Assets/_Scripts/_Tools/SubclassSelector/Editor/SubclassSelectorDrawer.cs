using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(SubclassSelectorAttribute))]
public class SubclassSelectorDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // ถ้าตัวแปรไม่ใช่ [SerializeReference] ให้วาดแบบปกติแล้วจบเลย
        if (property.propertyType != SerializedPropertyType.ManagedReference)
        {
            EditorGUI.PropertyField(position, property, label, true);
            return;
        }

        EditorGUI.BeginProperty(position, label, property);

        // วาดพื้นที่ปุ่ม Dropdown (ทับตำแหน่ง Label เดิม)
        Rect buttonRect = new Rect(position.x + EditorGUIUtility.labelWidth, position.y, position.width - EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight);

        // ดึงชื่อ Class ปัจจุบันมาโชว์
        string typeName = "Null (Click to Select)";
        if (!string.IsNullOrEmpty(property.managedReferenceFullTypename))
        {
            // ตัดชื่อให้สั้นลงเหลือแค่ชื่อ Class
            typeName = property.managedReferenceFullTypename.Split(' ').Last().Split('.').Last();
        }

        // วาดปุ่ม
        if (GUI.Button(buttonRect, typeName, EditorStyles.layerMaskField))
        {
            // ดึง Type ของตัวแปร (เช่น SkillActionTest) จาก fieldInfo
            Type baseType = fieldInfo.FieldType;

            // ถ้าเป็น List (เช่น List<SkillActionTest>) ต้องดึงไส้ใน (Generic Argument) ออกมา
            if (baseType.IsGenericType && baseType.GetGenericTypeDefinition() == typeof(System.Collections.Generic.List<>))
            {
                baseType = baseType.GetGenericArguments()[0];
            }
            // ถ้าเป็น Array (เช่น SkillActionTest[])
            else if (baseType.IsArray)
            {
                baseType = baseType.GetElementType();
            }

            // ค้นหา Class ลูกทั้งหมดของ baseType
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => baseType.IsAssignableFrom(p) && !p.IsAbstract && p.IsClass);

            GenericMenu menu = new GenericMenu();

            // เมนูสำหรับ Reset ค่าเป็น Null
            menu.AddItem(new GUIContent("None"), false, () => {
                property.managedReferenceValue = null;
                property.serializedObject.ApplyModifiedProperties();
            });

            // วนลูปสร้างเมนูตาม Class ลูกที่หาเจอ
            foreach (var type in types)
            {
                menu.AddItem(new GUIContent(type.Name), false, () => {
                    property.managedReferenceValue = Activator.CreateInstance(type);
                    property.serializedObject.ApplyModifiedProperties();
                });
            }
            menu.ShowAsContext();
        }

        // วาด Property ปกติ (เพื่อให้เห็นไส้ในของ Class ที่เลือก)
        EditorGUI.PropertyField(position, property, label, true);
        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, true);
    }
}