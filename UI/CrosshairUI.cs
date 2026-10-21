using UnityEngine;

public class CrosshairUI : MonoBehaviour
{
    public RectTransform up, down, left, right;

    [Header("Gap Settings")]
    public float minGap = 10f;
    public float maxGap = 40f;
    [HideInInspector] public float currentGap;

    void Awake()
    {
        if (currentGap <= 0f) currentGap = minGap;
        Apply();
    }

    public void Apply()
    {
        up.anchoredPosition    = new Vector2(0,  currentGap);
        down.anchoredPosition  = new Vector2(0, -currentGap);
        left.anchoredPosition  = new Vector2(-currentGap, 0);
        right.anchoredPosition = new Vector2( currentGap, 0);
    }
    public void SetMinGap(float newMinGap)
    {
        minGap = newMinGap;
        currentGap = Mathf.Max(currentGap, minGap);
        Apply();
    }
}
