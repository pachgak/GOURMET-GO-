using System; // <-- อย่าลืม Using System สำหรับ Action
using System.Collections.Generic;
using UnityEngine;

public class PlayerBuffManager : MonoBehaviour
{
    // *** 1. สร้าง Event ส่งไปให้ UI ***
    public Action<BuffSO, float, int> OnBuffAdded;   // ส่ง (ข้อมูลบัพ, เวลา, จำนวน Stack)
    public Action<BuffSO, float, int> OnBuffUpdated; // ส่งเวลาที่ลดลงทุกเฟรม
    public Action<BuffSO> OnBuffRemoved;             // ส่งข้อมูลบัพที่ถูกลบออกไป

    public class ActiveBuff
    {
        public BuffSO data;
        public float durationTimer;
        public float tickTimer;
        public int currentStacks = 1;
    }

    private List<ActiveBuff> _activeBuffs = new List<ActiveBuff>();

    public void AddBuff(BuffSO buffData)
    {
        foreach (ActiveBuff activeBuff in _activeBuffs)
        {
            if (activeBuff.data.buffName == buffData.buffName)
            {
                activeBuff.durationTimer = buffData.duration;
                activeBuff.tickTimer = buffData.tickInterval;

                if (buffData.isStackable && activeBuff.currentStacks < buffData.maxStacks)
                {
                    activeBuff.currentStacks++;
                    foreach (var effect in buffData.effects) effect.ApplyEffect(gameObject);
                }

                // *** 2. เรียก Event แจ้งเตือนว่าบัพนี้ถูกอัปเดต (เช่น เวลาเด้งกลับไปเต็ม หรือ Stack เพิ่ม) ***
                OnBuffUpdated?.Invoke(activeBuff.data, activeBuff.durationTimer, activeBuff.currentStacks);
                return;
            }
        }

        foreach (var effect in buffData.effects) effect.ApplyEffect(gameObject);

        _activeBuffs.Add(new ActiveBuff
        {
            data = buffData,
            durationTimer = buffData.duration,
            tickTimer = buffData.tickInterval,
            currentStacks = 1
        });

        // *** 3. เรียก Event แจ้งเตือนว่ามีบัพใหม่เข้ามา! ***
        OnBuffAdded?.Invoke(buffData, buffData.duration, 1);
    }

    private void Update()
    {
        for (int i = _activeBuffs.Count - 1; i >= 0; i--)
        {
            ActiveBuff buff = _activeBuffs[i];
            buff.durationTimer -= Time.deltaTime;

            // *** 4. ส่ง Event อัปเดตเวลาที่ลดลงให้ UI ***
            OnBuffUpdated?.Invoke(buff.data, buff.durationTimer, buff.currentStacks);

            if (buff.data.hasTickEffect)
            {
                buff.tickTimer -= Time.deltaTime;
                if (buff.tickTimer <= 0)
                {
                    foreach (var effect in buff.data.effects) effect.TickEffect(gameObject);
                    buff.tickTimer = buff.data.tickInterval;
                }
            }

            if (buff.durationTimer <= 0)
            {
                for (int j = 0; j < buff.currentStacks; j++)
                {
                    foreach (var effect in buff.data.effects) effect.RemoveEffect(gameObject);
                }

                // *** 5. แจ้ง UI ว่าบัพหมดเวลาแล้ว ให้ลบไอคอนทิ้งด้วย! ***
                OnBuffRemoved?.Invoke(buff.data);

                _activeBuffs.RemoveAt(i);
            }
        }
    }

    // 1. ดึงข้อมูลบัฟทั้งหมดส่งให้ SaveManager
    public List<ActiveBuff> GetActiveBuffs() => _activeBuffs;

    // 2. เคลียร์บัฟทั้งหมด (ใช้ตอนกำลังจะโหลดเกม)
    public void ClearAllBuffs()
    {
        for (int i = _activeBuffs.Count - 1; i >= 0; i--)
        {
            ActiveBuff buff = _activeBuffs[i];
            for (int j = 0; j < buff.currentStacks; j++)
            {
                foreach (var effect in buff.data.effects) effect.RemoveEffect(gameObject);
            }
            OnBuffRemoved?.Invoke(buff.data); // แจ้ง UI ให้ลบไอคอน
        }
        _activeBuffs.Clear();
    }

    // 3. เสกบัฟกลับคืนมาตามข้อมูลที่โหลดได้
    public void RestoreBuff(BuffSO buffData, float remainingTime, int stacks)
    {
        // ใส่ Effect เข้าตัว Player ตามจำนวน Stack
        for (int i = 0; i < stacks; i++)
        {
            foreach (var effect in buffData.effects) effect.ApplyEffect(gameObject);
        }

        _activeBuffs.Add(new ActiveBuff
        {
            data = buffData,
            durationTimer = remainingTime,
            tickTimer = buffData.tickInterval,
            currentStacks = stacks
        });

        // แจ้ง UI ให้สร้างไอคอน
        OnBuffAdded?.Invoke(buffData, remainingTime, stacks);
    }
}