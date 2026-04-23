using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "BuffDatabase", menuName = "Buff System/BuffDatabase")]
public class BuffDatabaseSO : ScriptableObject
{
    public List<BuffSO> allBuffs = new List<BuffSO>();

    public BuffSO GetBuffByID(string id)
    {
        return allBuffs.Find(buff => buff != null && buff.ID == id);
    }
}