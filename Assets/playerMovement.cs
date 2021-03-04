using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerMovement : MonoBehaviour
{
    // Start is called before the first frame updat
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey("w"))
        {
            transform.position += transform.forward * 0.1f;
        }
        
        if (Input.GetKey("s"))
        {
            transform.position -= transform.forward * 0.1f;
        }
        if (Input.GetKey("a"))
            
        {
            transform.position -= transform.right * 0.1f;
        }
        if (Input.GetKey("d"))
        {
            transform.position += transform.right * 0.1f;
        }
            
    }
}
