using UnityEngine;
using UnityEngine.Events;

public class MonkeyBombManager : MonoBehaviour
{
    [Header("Monkey Bomb Counter")]
    [SerializeField] private int monkeyCount = 0;
    [SerializeField] private int maxMonkeys = 3;
    
    [Header("Events")]
    public UnityEvent<int> OnMonkeyCountChanged; // Evento che passa il nuovo count
    
    private void Start()
    {
        // Notifica il count iniziale
        NotifyCountChanged();
    }
    
    /// <summary>
    /// Aggiunge una Monkey Bomb al counter (chiamato dalla MysteryBox)
    /// </summary>
    public void AddMonkeyBomb()
    {
        if (monkeyCount < maxMonkeys)
        {
            monkeyCount++;
            Debug.Log($"Monkey Bomb ricevuta! Count: {monkeyCount}/{maxMonkeys}");
            NotifyCountChanged();
        }
        else
        {
            Debug.LogWarning($"Monkey Bomb massime raggiunto! ({maxMonkeys})");
        }
    }
    
    /// <summary>
    /// Usa una Monkey Bomb (chiamato quando il player lancia la monkey)
    /// </summary>
    public bool TryUseMonkeyBomb()
    {
        if (monkeyCount > 0)
        {
            monkeyCount--;
            Debug.Log($"Monkey Bomb usata! Rimanenti: {monkeyCount}");
            NotifyCountChanged();
            return true;
        }
        
        Debug.LogWarning("Nessuna Monkey Bomb disponibile!");
        return false;
    }
    
    /// <summary>
    /// Imposta il numero di monkey bomb (utile per debug o cheat)
    /// </summary>
    public void SetMonkeyCount(int count)
    {
        monkeyCount = Mathf.Clamp(count, 0, maxMonkeys);
        NotifyCountChanged();
    }
    
    /// <summary>
    /// Ottiene il numero corrente di monkey bombs
    /// </summary>
    public int GetMonkeyCount() => monkeyCount;
    
    /// <summary>
    /// Controlla se il player ha almeno una monkey bomb
    /// </summary>
    public bool HasMonkeyBomb() => monkeyCount > 0;
    
    private void NotifyCountChanged()
    {
        OnMonkeyCountChanged?.Invoke(monkeyCount);
    }
}
