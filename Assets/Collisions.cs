using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collisions : MonoBehaviour
{
    // Start is called before the first frame update
    public List<GameObject> RenderedBases;
    void Start()
    {
        
    }

    public void AddBase(GameObject Base)
    {
        RenderedBases.Add(Base);
    }
    

    // Update is called once per frame
    void Update()
    {
        /*foreach (var Base in RenderedBases)
        {
            Debug.Log("Hello");
            
            //10 below is a magic number, should be fixed
            if (Vector3.Distance(transform.position, Base.transform.position) < 10)
            {
                base.GetComponent<BaseScript>().StartTakeOver(GetComponent<PlayerScript>().username);
            }    
        }*/
    }
}
