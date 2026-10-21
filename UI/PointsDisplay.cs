using UnityEngine;
using TMPro;
public class PointsDisplay : MonoBehaviour
{
    [Header("Points Settings")]
    [SerializeField] private int currentPoints = 1000;
    
    [Header("UI Reference")]
    [SerializeField] private TextMeshProUGUI pointsText;

    private void Start()
    {
        UpdatePointsDisplay();
        
        if (pointsText == null)
        {
            Debug.LogWarning("PointsText not assigned! Please assign the TextMeshProUGUI from your UI in Inspector.");
        }
    }
    private void UpdatePointsDisplay()
    {
        if (pointsText != null){
            pointsText.text = $"{currentPoints}";
        }
    }

    public void AddPoints(int amount)
    {
        currentPoints += amount;
        UpdatePointsDisplay();
        Debug.Log($"+{amount} points. Total: {currentPoints}");
    }
    public bool RemovePoints(int amount)
    {
        if (currentPoints >= amount)
        {
            currentPoints -= amount;
            UpdatePointsDisplay();
            Debug.Log($"-{amount} points. Remaining: {currentPoints}");
            return true;
        } else {
            Debug.Log("Not enough points.");
            return false;
        }
    }
    public int GetPoints() {
        return currentPoints;
    }
    public void SetPoints(int amount)
    {
        currentPoints = amount;
        UpdatePointsDisplay();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            AddPoints(100);
        }

        if (Input.GetKeyDown(KeyCode.O))
        {
            RemovePoints(50);
        }
    }
}
