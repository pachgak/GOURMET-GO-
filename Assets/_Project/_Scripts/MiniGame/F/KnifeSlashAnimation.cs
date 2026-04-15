using UnityEngine;
using DG.Tweening; // อย่าลืมใส่บรรทัดนี้

public class KnifeSlashAnimation : MonoBehaviour
{
    [Header("References")]
    [Tooltip("ลาก RectTransform ของรูปมีดมาใส่ตรงนี้")]
    public RectTransform knife;
    private MiniGameFManager _miniGameManager;

    [Header("Rotation Angles")]
    public float startRotationZ = 111.0f; // จังหวะง้างรอ
    public float endRotationZ = 0.0f;     // จังหวะฟันลงมาสุด

    [Header("Animation Timings")]
    [Tooltip("เวลาตอนสับลงมา (ยิ่งน้อยยิ่งฟันเร็ว แนะนำ 0.05 - 0.1)")]
    public float strikeDuration = 0.08f;

    [Tooltip("เวลาตอนดึงมีดกลับไปง้างใหม่ (ควรช้ากว่าตอนฟัน แนะนำ 0.15 - 0.25)")]
    public float returnDuration = 0.2f;

    [Header("Ease Settings")]
    [Tooltip("กราฟตอนฟันลง: OutExpo คือพุ่งแรงตอนแรกแล้วหนืดตอนปลาย (ดูสมจริง)")]
    public Ease strikeEase = Ease.OutExpo;

    [Tooltip("กราฟตอนดึงกลับ: InOutSine ให้ความรู้สึกสมูทเป็นธรรมชาติ")]
    public Ease returnEase = Ease.InOutSine;

    private void Awake()
    {
        // หา Manager ถ้าลืมลากใส่ Inspector
        if (_miniGameManager == null)
        {
            _miniGameManager = GetComponent<MiniGameFManager>();
        }

        // เซ็ตให้มีดอยู่ในท่าง้างรอตั้งแต่เริ่มเกม
        if (knife != null)
        {
            knife.localRotation = Quaternion.Euler(0, 0, startRotationZ);
        }
    }

    private void OnEnable()
    {
        if (_miniGameManager != null)
        {
            _miniGameManager.OnSlashTriggered += PlaySlashAnimation;
        }
    }

    private void OnDisable()
    {
        if (_miniGameManager != null)
        {
            _miniGameManager.OnSlashTriggered -= PlaySlashAnimation;
        }
    }

    public void PlaySlashAnimation()
    {
        if (knife == null) return;

        // 1. หยุดแอนิเมชันเดิมทันที (สำคัญมาก! เผื่อผู้เล่นรัวปุ่ม แอนิเมชันจะได้ไม่บั๊กตีกันเอง)
        knife.DOKill();

        // 2. รีเซ็ตให้กลับไปท่าง้างทันที (111 องศา) ตามที่คุณต้องการ
        knife.localRotation = Quaternion.Euler(0, 0, startRotationZ);

        // 3. สร้าง Sequence เพื่อร้อยเรียงคิวแอนิเมชัน
        Sequence slashSeq = DOTween.Sequence();

        // คิวที่ 1: ฟันลงมาที่ 0 องศา อย่างรวดเร็ว
        slashSeq.Append(knife.DOLocalRotate(new Vector3(0, 0, endRotationZ), strikeDuration).SetEase(strikeEase));

        // คิวที่ 2: ดึงกลับไปที่ 111 องศา อย่างนุ่มนวล
        slashSeq.Append(knife.DOLocalRotate(new Vector3(0, 0, startRotationZ), returnDuration).SetEase(returnEase));
    }
}