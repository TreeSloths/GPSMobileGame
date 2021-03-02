using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    // Start is called before the first frame update
    private List<GameObject> RenderedBases;
    public string username;
    void Start()
    {
        //magic string, needs to be fetched from server after login
        username = "Emil";
    }

    public void AddBase(GameObject Base)
    {
        RenderedBases.Add(Base);
    }
    

    // Update is called once per frame
    void Update()
    {
        
    }
}
