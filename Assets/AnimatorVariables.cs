using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;
using static Unity.Burst.Intrinsics.X86.Avx;
using static UnityEngine.UI.Image;

public class AnimatorVariables : MonoBehaviour
{
    [SerializeField] Transform sword;
    [SerializeField] RectTransform mainMenuImage;
    [SerializeField] RectTransform title;
    [SerializeField] TextMeshProUGUI tmp;
    [SerializeField] AnimationCurve zoomAnimationCurve;
    [SerializeField] AnimationCurve zoomSwordCurve;
    [SerializeField] AnimationCurve opacityCurve;
    [SerializeField] GameObject Panel;
    private float timer = 0;
    // Start is called before the first frame update
    void Start()
    {
        tmp.alpha = 0f;
        Panel.SetActive(false);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (timer >= 1f) 
        {
            Panel.SetActive(true);
            return; 
        }
        title.localScale = Vector3.one * zoomAnimationCurve.Evaluate(timer);
        tmp.alpha = 1f * opacityCurve.Evaluate(timer);
        mainMenuImage.localScale = Vector3.one * zoomAnimationCurve.Evaluate(timer);
        sword.localPosition = new Vector3 (sword.position.x, sword.position.y, sword.position.z * zoomSwordCurve.Evaluate(timer));
        timer += Time.deltaTime;
    }
}
