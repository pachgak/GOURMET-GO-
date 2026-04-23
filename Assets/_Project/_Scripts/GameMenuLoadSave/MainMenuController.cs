using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.IO;

public class MainMenuController : MonoBehaviour
{
    [Header("Settings")]
    public string mainGameSceneName = "MainGame"; // ชื่อฉากเกมหลักของคุณ (พิมพ์ให้ตรงเป๊ะๆ)

    [Header("UI Buttons")]
    public Button continueButton;
    public Button newGameButton;
    public Button quitButton;

    private string _saveFilePath;

    private void Start()
    {
        // 1. ผูก Event ให้ปุ่มกด
        continueButton.onClick.AddListener(OnContinueClicked);
        newGameButton.onClick.AddListener(OnNewGameClicked);
        quitButton.onClick.AddListener(OnQuitClicked);

        // 2. เช็คว่ามีไฟล์เซฟเก่าไหม ถ้าไม่มีให้ปุ่ม Continue กดไม่ได้!
        _saveFilePath = Path.Combine(Application.persistentDataPath, "SaveGame.json");
        continueButton.interactable = File.Exists(_saveFilePath);
    }

    private void OnContinueClicked()
    {
        // ส่งจดหมายน้อยบอกฉากหน้าว่า "โหลดเซฟนะ" (0 = Continue)
        PlayerPrefs.SetInt("IsNewGame", 0);
        SceneManager.LoadScene(mainGameSceneName);
    }

    private void OnNewGameClicked()
    {
        // ส่งจดหมายน้อยบอกฉากหน้าว่า "เริ่มใหม่ทั้งหมด" (1 = New Game)
        PlayerPrefs.SetInt("IsNewGame", 1);
        SceneManager.LoadScene(mainGameSceneName);
    }

    private void OnQuitClicked()
    {
        Debug.Log("ออกจากเกม!");
        Application.Quit();
    }
}