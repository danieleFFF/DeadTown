using UnityEngine;
public abstract class PerkMachine
{
    public int Cost { get; protected set; }
    public string PerkName { get; protected set; }
    public string TextPrompt { get; protected set; }
    public bool IsPurchased { get; protected set; }
    public PerkMachine(int cost, string perkName, string textPrompt)
    {
        Cost = cost;
        PerkName = perkName;
        TextPrompt = textPrompt;
        IsPurchased = false;
    }

    public virtual void Purchase(PlayerCharacter player)
    {

        Apply(player);

    }
    protected abstract void Apply(PlayerCharacter player);
}
