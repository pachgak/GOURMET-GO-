using UnityEngine;

public class DelayActive : MonoBehaviour
{
    public float timeDely;
    public float timer;
    public GameObject activeObj;
    
    private void OnEnable()
    {
        timer = timeDely;
        activeObj.SetActive(false);
        GetComponent<ControllerVFX>().enabled = false;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (timer > 0)
        {
            timer -= Time.deltaTime;
            if (timer <= 0)
            {

                activeObj.SetActive(true);
                GetComponent<ControllerVFX>().enabled = true;
            }
        }
    }
}
