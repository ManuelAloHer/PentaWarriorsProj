using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PopUpAnimation : MonoBehaviour
{
    public AnimationCurve opacityCurve; //Further investigation needed for creative uses
    public AnimationCurve scaleCurve;
    public AnimationCurve heightCurve;

    private TextMeshProUGUI tmp;
    private float timer = 0;
    private Vector3 origin;

    // Start is called before the first frame update
    void Awake()
    {
        tmp = transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        origin = transform.position;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        tmp.color = new Color(1, 1, 1, opacityCurve.Evaluate(timer));
        transform.localScale = Vector3.one * scaleCurve.Evaluate(timer);
        transform.position = origin + new Vector3(0, 1 + heightCurve.Evaluate(timer),0);
        timer += Time.deltaTime;
    }
}
