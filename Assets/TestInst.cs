using UnityEngine;

public class TestInst : EnemyMovement
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected override void Start()
    {
        base.Start();
        //GetKnockedBack(Vector3.forward, 20);
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
    }
}
