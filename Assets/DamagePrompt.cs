using UnityEngine;
using TMPro; // ���������Ҥس��ͧ�� TextMeshPro ���ਡ�����

public class DamagePrompt : MonoBehaviour
{
    // ������Ҹ�óз��س����ö��駤����� Unity Inspector
    public float destroyTime = 2f; // �������ҷ���ͤ�������������¨����躹˹�Ҩ�
    public float risingSpeed = 1f; // �������Ǿ�鹰ҹ����ͤ�������������¨���¢��
    public float randomXRange = 1f; // ��ǧ��������᡹ X
    public float randomYRange = 1f; // ��ǧ��������᡹ Y
    public Color damageColor = Color.red; // �բͧ��ͤ��������������

    private Vector3 _offSet;
    private Transform _targetPos;
    public TMP_Text damageText;
    private Vector3 _randomDirection;

    void Awake()
    {
        // ��駤�����������
        damageText.color = damageColor;

        // ������ȷҧ�������͹���
        _randomDirection = new Vector3(Random.Range(-randomXRange, randomXRange), Random.Range(0, randomYRange), 0);

        // ������ѵ�آ�ͤ������ѵ��ѵ���ѧ�ҡ��ҹ仵�����ҷ���˹�
        Destroy(gameObject, destroyTime);
    }

    void Update()
    {
        // ������ͤ�����¢��仵��������Ҵ��·�ȷҧ����������
        transform.position = _targetPos.position + _offSet;
        damageText.transform.position += (_randomDirection.normalized * risingSpeed + Vector3.up) * Time.deltaTime;

        // ����� ������ͤ����ҧ����
        damageText.color = new Color(damageText.color.r, damageText.color.g, damageText.color.b, damageText.color.a - (Time.deltaTime / destroyTime));
    }

    // ���ʹ�Ҹ�ó�����Ѻ��駤�Ң�ͤ��������������
    public void SetDamageText(float damageAmount, Vector3 offSet, Transform targetPos)
    {
        damageText.text = damageAmount.ToString();
        _offSet = offSet;
        _targetPos = targetPos;
    }
}