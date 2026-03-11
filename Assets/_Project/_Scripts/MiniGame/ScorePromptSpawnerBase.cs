using UnityEngine;

// สังเกตว่าเราใช้ class ธรรมดา ไม่ต้องมี Update หรือดัก Event อะไรเลย
public class ScorePromptSpawnerBase : MonoBehaviour
{
    public ScorePromptUI prefabScorePrompt;

    // ฟังก์ชันนี้ตั้งเป็น protected เพื่อให้เฉพาะคลาสลูก (ที่สืบทอดไป) เรียกใช้งานได้
    protected void SpawnPrompt(string text, Color color, RectTransform spawnPos, float randomRange = 0f)
    {
        // โค้ดเสก Prefab และสุ่มตำแหน่ง (ที่คุณเขียนไว้) จะถูกรวมไว้ที่นี่ที่เดียว
        GameObject promptObj = Instantiate(prefabScorePrompt.gameObject, spawnPos.transform);

        RectTransform promptRect = promptObj.GetComponent<RectTransform>();
        promptRect.anchoredPosition = new Vector2(
            Random.Range(-randomRange, randomRange),
            Random.Range(-randomRange, randomRange)
        );

        ScorePromptUI promptScript = promptObj.GetComponent<ScorePromptUI>();
        if (promptScript != null)
        {
            promptScript.Setup(text, color);
        }
    }
}