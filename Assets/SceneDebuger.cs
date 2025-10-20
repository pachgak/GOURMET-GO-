using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class SceneDebuger : MonoBehaviour
{
    public static SceneDebuger instance;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        DontDestroyOnLoad(this.gameObject);
    }

    private bool isMoveScene = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        MoveSceneMode(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F5)) ReloadCurrentScene();
        if (Input.GetKeyDown(KeyCode.F6)) MoveSceneMode(!isMoveScene);

        if (isMoveScene && Input.inputString != null)
        {
            bool isNumber = int.TryParse(Input.inputString, out int number);
            if (isNumber)
            {
                HeadleMoveToScene(number);
            }

        }

    }

        public void ReloadCurrentScene()
    {
        // 1. รับ Scene ปัจจุบัน
        Scene currentScene = SceneManager.GetActiveScene();

        // 2. สั่งโหลด Scene นั้นซ้ำ โดยใช้ชื่อ (แนะนำ)
        SceneManager.LoadScene(currentScene.name);

        // หรือใช้ดัชนี (Build Index):
        // SceneManager.LoadScene(currentScene.buildIndex);
    }

    public void MoveSceneMode(bool isSet)
    {
        isMoveScene = isSet;
    }

    public void HeadleMoveToScene(int indexScene)
    {
        if(indexScene <= 0) return;
        if(indexScene <= SceneManager.sceneCountInBuildSettings) SceneManager.LoadScene(indexScene-1);

        isMoveScene = false;
    }
}
