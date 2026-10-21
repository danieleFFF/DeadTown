using UnityEngine;
using System.Collections;

public class ReactiveTarget : MonoBehaviour {
    //Chiamato dallo script di sparo
    public void ReactToHit(WeaponManager weapon, bool isHeadShot) {
        ZombieAISimple zombieAI = GetComponent<ZombieAISimple>();
        if (zombieAI != null) {
            if (isHeadShot)
                zombieAI.TakeDamage(weapon.getCurrentWeaponDmg() * 2);
            else
            zombieAI.TakeDamage(weapon.getCurrentWeaponDmg());
        }   
    }

}