using com.cyborgAssets.inspectorButtonPro;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TEst : MonoBehaviour
{
    public static int TestLink = 0;

    public bool someCondition;
    private float currentSpeed;
    private float sprintAcceleration;
    private float targetSpeed;


    public float aSpeed = 1;
    public float bSpeed = 10;
    public float cSpeed;
    public float speedTime = 0;

    public float Timer;

    public LayerMask testLayer;
    public LayerMask testLayers;

    private void Awake()
    {
        
    }

    [ProButton]
    public void TEstSetTestLink(int anwser)
    {
        TestLink = anwser;
        Debug.Log($"Set TestLink {gameObject.name} : {TestLink}");
    }

    [ProButton]
    public void TEstShowTestLink()
    {
        Debug.Log($"Show TestLink {gameObject.name} : {TestLink}");
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        string[] layersToInclude = new string[] { "Enemy", "Player" };
        testLayers = LayerMask.GetMask(layersToInclude);
        testLayer = LayerMask.GetMask("Enemy");
    }

    // Update is called once per frame
    void Update()
    {
        cSpeed = Mathf.Lerp(aSpeed, bSpeed, speedTime);
        //currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime * sprintAcceleration);
        Timer += Time.deltaTime;
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

    private IEnumerator CheckForCondition()
    {
        yield return new WaitForSeconds(1f);
        Debug.Log("กำลังทำงาน...");

        // ถ้าเงื่อนไขเป็นจริง ให้หยุด Coroutine ทันที
        if (someCondition == true)
        {
            yield break; // Coroutine นี้จะจบลงที่นี่
        }

        // โค้ดด้านล่างนี้จะไม่ถูกเรียกใช้ถ้า someCondition เป็นจริง
        Debug.Log("ทำงานต่อ... เพราะเงื่อนไขไม่เป็นจริง");
    }

    public void TestAnimationEvent(GameObject gameObject,int type)
    {

    }

    public void GetGameObject(GameObject gameObject)
    {

    }
    public void GetInt(int type)
    {

    }

    public void GetBool(bool isBool)
    {

    }
    [ProButton]
    public void SetTimeScal(float scall)
    {
        Time.timeScale = scall;
    }

}
