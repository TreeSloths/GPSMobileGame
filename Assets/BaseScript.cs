using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseScript : MonoBehaviour
{
    // Start is called before the first frame update
    public string owner;
    public int gold;
    public GameObject player;
    
    void Start()
    {
        player.GetComponent<PlayerScript>().locationChanged += StartTakeOver;
    }


    public void StartTakeOver()
    {
        if (owner == player.GetComponent<PlayerScript>().username)
        {
            gold += player.GetComponent<PlayerScript>().gold;
            player.GetComponent<PlayerScript>().gold = 0;
            
            // send to database
            return;     
        }
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
        GetComponent<SpriteRenderer>().color = new Color(1f, 0.64f, 0f, 0.7f);
        TakeOverRunning = false;
    }
    
    // Update is called once per frame
    void Update()
    {
        
    }
}
