using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class SettingPlayerControllerManager : MonoBehaviour
{
    public static SettingPlayerControllerManager instance;

    public AttackDiractionType meleeAttackDiraction;
    public TMP_Dropdown meleeAttackDropdown;
    public AttackDiractionType skillDiraction;
    public TMP_Dropdown skillDropdown;
    public AttackDiractionType dashDiraction;
    public TMP_Dropdown dashDropdown;

    public enum AttackDiractionType
    {
        mouse, movement
    }

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        SetupDropdown();

        LoadSetting();

    }

    private void SetupDropdown()
    {
        // 1. ล้างรายการเดิมออกทั้งหมดก่อน
        meleeAttackDropdown.ClearOptions();
        skillDropdown.ClearOptions();
        dashDropdown.ClearOptions();

        // 2. ดึงชื่อสมาชิกทั้งหมดของ AttackDiractionType ออกมาเป็น string[]
        // จะได้ {"mouse", "movement"}
        string[] enumNames = Enum.GetNames(typeof(AttackDiractionType));

        string[] drowDownNames = new string[enumNames.Length];

        for (int i = 0; i < enumNames.Length; i++)
        {
            drowDownNames[i] = ToTitleCase(enumNames[i]);
        }

        // 3. เพิ่มรายการชื่อ enum เข้าไปใน Dropdown
        // List<string> ถูกสร้างจาก array ของชื่อ enum
        meleeAttackDropdown.AddOptions(new System.Collections.Generic.List<string>(drowDownNames));
        skillDropdown.AddOptions(new System.Collections.Generic.List<string>(drowDownNames));
        dashDropdown.AddOptions(new System.Collections.Generic.List<string>(drowDownNames));

        meleeAttackDropdown.onValueChanged.AddListener(meleeAttackDiractionChang);
        skillDropdown.onValueChanged.AddListener(skillDiractionChang);
        dashDropdown.onValueChanged.AddListener(dashDiractionChang);

        // 4. (ทางเลือก) ตั้งค่าเริ่มต้น
        // directionDropdown.value = (int)AttackDiractionType.mouse; 
        // directionDropdown.RefreshShownValue();
    }

    private string ToTitleCase(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return text;
        }
        // ตัวอักษรตัวแรกให้เป็นตัวใหญ่ (ToUpper()) แล้วนำไปต่อกับส่วนที่เหลือของ string (Substring(1))
        return char.ToUpper(text[0]) + text.Substring(1);
    }

    private void LoadSetting()
    {
        int rebinds = -1;

        rebinds = PlayerPrefs.GetInt("meleeAttackDiraction",-1);
        if (rebinds > -1) meleeAttackDiraction = (AttackDiractionType)rebinds;
        meleeAttackDropdown.value = rebinds;

        rebinds = PlayerPrefs.GetInt("skillDiraction", -1);
        if (rebinds > -1) meleeAttackDiraction = (AttackDiractionType)rebinds;
        skillDropdown.value = rebinds;

        rebinds = PlayerPrefs.GetInt("dashDiraction", -1);
        if (rebinds > -1) meleeAttackDiraction = (AttackDiractionType)rebinds;
        dashDropdown.value = rebinds;

    }

    public void meleeAttackDiractionChang(int chang)
    {
        meleeAttackDiraction = (AttackDiractionType)chang;

        PlayerPrefs.SetInt("meleeAttackDiraction", (int)meleeAttackDiraction);
    }

    public void skillDiractionChang(int chang)
    {
        skillDiraction = (AttackDiractionType)chang;

        PlayerPrefs.SetInt("skillDiraction", (int)skillDiraction);
    }
    public void dashDiractionChang(int chang)
    {
        dashDiraction = (AttackDiractionType)chang;

        PlayerPrefs.SetInt("dashDiraction", (int)dashDiraction);
    }

}


