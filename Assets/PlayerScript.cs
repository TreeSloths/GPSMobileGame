using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    // Start is called before the first frame update
    private List<GameObject> RenderedBases;
    public string username;
    public int gold = 100;
    void Start()
    {
        //magic string, needs to be fetched from server after login
        username = "Emil";
    }
    public void AddBase(GameObject Base)
    {
        RenderedBases.Add(Base);
    }

    public delegate void LocationChangedDelegate();
    public event LocationChangedDelegate locationChanged;



    // Update is called once per frame
    void Update()
    {
        if (transform.hasChanged)
        {
            //Debug.Log("Hello");
            locationChanged.Invoke();
        }
    }
}
