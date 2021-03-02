using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseScript : MonoBehaviour
{
    // Start is called before the first frame update
    public string owner;
    
    public 
    void Start()
    {
        var player = GameObject.Find("PlayerTarget");
        player.GetComponent<Collisions>().AddBase(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
