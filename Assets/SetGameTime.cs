using UnityEngine;

public class SetGameTime : MonoBehaviour
{
    public float slowTimeScale = 1f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(slowTimeScale != Time.timeScale) Time.timeScale = slowTimeScale;
    }
}
