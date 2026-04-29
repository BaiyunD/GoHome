using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewEnding", menuName = "�����ռ�/EndingData")]
public class EndingData : ScriptableObject
{
    public string endingName;               // �ڲ���ʶ
    public string conditionType;            // ����չ������ "distanceZero", "healthZero", "moneyHigh" ��
    public List<string> textSegments;       // �ֶ��ı��б�
    public bool isWin;                      // �Ƿ�ʤ����֣�����UI��ʽ���֣�

    [Header("��ѡ����ֵ������Ԥ����")]
    public int winDistance = -1;           // �� isWin=true ʱ������þ��봥����-1��ʾ�����ƣ�
    public float loseHealth = -1;          // �� isWin=false ʱ��Ѫ��<=��ֵ������-1��ʾ�����ƣ�
    public int requiredRegion = -1;       // �������ƣ�-1��ʾ�����ƣ�
}
