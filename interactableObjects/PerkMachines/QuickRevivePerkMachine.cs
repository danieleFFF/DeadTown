using UnityEngine;

public class QuickRevivePerkMachine : PerkMachine
{
    public bool AutoReviveBonus { get; private set; }

    public QuickRevivePerkMachine()
        : base(500, "Quick Revive", "Respawn after you die!")
    {
        AutoReviveBonus = true;
    }

    protected override void Apply(PlayerCharacter player)
    {
        if(player.boughtQuickReviveStatus()==false){
            player.SetAutoRevive(AutoReviveBonus);
            Debug.Log("Second life bonus applied!");
        }
        else
        {
            Debug.LogWarning("Quick Revive: Player has already bought this perk!");
        }
    }
}
