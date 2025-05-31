using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEditor;

public class PopUpGenerator : MonoBehaviour
{
    public static PopUpGenerator currentIntance;

    [SerializeField] GameObject prefab;
    [SerializeField] float secondsForDestruction = 1f;
    // Start is called before the first frame update
    void Awake()
    {
        currentIntance = this;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void CreatePopUp(Vector3 position, string text, Color color)
    {
        var popup = Instantiate(prefab, position, Quaternion.identity);
        TextMeshProUGUI temp = popup.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        temp.faceColor = color;
        temp.text = text;
        

        Destroy(popup, secondsForDestruction);
    }
}
