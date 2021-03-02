using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseScript : MonoBehaviour
{
    // Start is called before the first frame update
    public string owner;
    public GameObject player;
    
    void Start()
    {
        player.GetComponent<PlayerScript>().locationChanged += StartTakeOver;
    }

    
    
    

    public void StartTakeOver()
    {
        Debug.Log("Hello");
        if (Vector3.Distance(transform.position, player.transform.position) < 10 && !TakeOverRunning) 
        {
            StartCoroutine(TakeOver());
        }
        

    }


    private bool TakeOverRunning = false;
    
    private IEnumerator TakeOver()
    {
        TakeOverRunning = true;
        yield return new WaitForSeconds(10);
        owner = player.GetComponent<PlayerScript>().username;
        TakeOverRunning = false;
    }
    
    // Update is called once per frame
    void Update()
    {
        
    }
}
