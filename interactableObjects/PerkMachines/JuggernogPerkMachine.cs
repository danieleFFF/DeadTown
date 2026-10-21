using UnityEngine;

public class JuggernogPerkMachine : PerkMachine
{
    public int HealthBonus { get; private set; }

    public JuggernogPerkMachine()
        : base(1500, "Juggernog!", "Stronger skin! Max health increases.")
    {
        HealthBonus = 130;
    }


    protected override void Apply(PlayerCharacter player)
    {
        if (player.boughtJuggernogStatus() == false)
        {
            player.SetBoughtJuggernog(true);
            player.SetHealth(HealthBonus);
            Debug.Log("Applied Juggernog: max health increased!");
        }
        else
        {
            Debug.LogWarning("Juggernog: Player has already bought this perk!");
            return;
        }
    }
}
