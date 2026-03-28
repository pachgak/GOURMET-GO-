using UnityEngine;

public static class VectorMath
{
    // ฟังก์ชันคำนวณการหมุน Vector จากอันนึงไปสู่อีกอันนึงตามความเร็วหมุน (เหมือน Vector3.RotateTowards แต่ควบคุมง่ายกว่า)
    public static Vector3 Steering(Vector3 currentVelocity, Vector3 desiredDirection, float turnSpeed, float deltaTime)
    {
        Vector3 desiredVelocity = desiredDirection.normalized * currentVelocity.magnitude;
        Vector3 steer = desiredVelocity - currentVelocity;

        // จำกัดแรงเลี้ยวเพื่อไม่ให้มันหักมุมเกินไป จนเกิดเป็นวงโค้งที่สวยงาม
        Vector3 finalVelocity = currentVelocity + (steer.normalized * turnSpeed * deltaTime);

        // รักษาความเร็วให้คงที่ตลอดวงโค้ง
        return finalVelocity.normalized * currentVelocity.magnitude;
    }
}