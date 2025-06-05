using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AreaControl : MonoBehaviour
{
    List<Entity> entities;
    public GameObject[] areaHighLights;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AreaHighlightersEnable()
    {
        for (int i = 0; i < areaHighLights.Length; i++) 
        {
            areaHighLights[i].SetActive(true);
        }
    }
    public void AreaHighlightersDisable() 
    {
        for (int i = 0; i < areaHighLights.Length; i++)
        {
            areaHighLights[i].SetActive(false);
        }
    }
}
