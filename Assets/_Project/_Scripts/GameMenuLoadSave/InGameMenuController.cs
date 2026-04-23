using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class InGameMenuController : MonoBehaviour
{
    [Header("Settings")]
    public string mainMenuSceneName = "GameMenu"; // ชื่อฉากหน้าเมนูหลัก

    [Header("UI Buttons")]
    public Button saveGameButton;
    public Button saveAndBackToMenuButton;
    public Button saveAndQuitGameButton;
    public Button endGameButton;

    private void Start()
    {
        saveGameButton.onClick.AddListener(OnSaveGame);
        saveAndBackToMenuButton.onClick.AddListener(OnSaveAndBackToMenu);
        saveAndQuitGameButton.onClick.AddListener(OnSaveAndQuitDesktop);
        endGameButton.onClick.AddListener(OnEndGame);
    }

    public void OnSaveGame()
    {
        // 1. สั่ง Save ทันที
        if (SaveManager.Instance != null) SaveManager.Instance.SaveGame();
    }

    private void OnSaveAndBackToMenu()
    {
        // 1. สั่ง Save ทันที
        if (SaveManager.Instance != null) SaveManager.Instance.SaveGame();

        // 2. โหลดกลับไปหน้า Menu
        SceneManager.LoadScene(mainMenuSceneName);
    }

    private void OnSaveAndQuitDesktop()
    {
        // 1. สั่ง Save ทันที
        if (SaveManager.Instance != null) SaveManager.Instance.SaveGame();

        // 2. ปิดเกมออกไปเลย
        Debug.Log("บันทึกและออกจากเกม!");
        Application.Quit();
    }

    private void OnEndGame()
    {
        if (SaveManager.Instance != null) SaveManager.Instance.ResetGameToDefault();

        // 2. โหลดกลับไปหน้า Menu
        SceneManager.LoadScene(mainMenuSceneName);
    }
}