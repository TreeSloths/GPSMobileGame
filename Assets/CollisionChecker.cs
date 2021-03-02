using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionChecker : MonoBehaviour
{
    private List<GameObject> RenderedBases;
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
        
    }
}
