using UnityEngine;

public class SpeedColaPerkMachine : PerkMachine
{
    public int SpeedBonus { get; private set; }

    public SpeedColaPerkMachine()
        : base(2000, "SpeedCola", "Move faster!")
    {
        SpeedBonus = 2;
    }

    protected override void Apply(PlayerCharacter player)
    {
        if (player.boughtSpeedColaStatus() == false)
        {
            // Applica boost alla velocità di movimento
            player.SetSpeed(SpeedBonus);
            player.SetBoughtSpeedCola(true);
            
            // Applica boost alla velocità di ricarica (animazione)
            WeaponAnimator wp = player.GetWeaponAnimator();
            if (wp != null)
            {
                wp.speedUpReload(2f);
            }
            
            // Applica boost alla velocità di ricarica (logica)
            AmmoManager ammoManager = player.GetComponent<AmmoManager>();
            if (ammoManager != null)
            {
                ammoManager.SetReloadSpeedMultiplier(2f);
            }
            
            Debug.Log("SpeedCola applied: movement speed +2, reload speed x2!");
        }
        else
        {
            Debug.LogWarning("SpeedCola: Player has already bought this perk!");
        }
    }
}
