using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shop : MonoBehaviour, IInteractable
{
    public void Interact()
    {
        if (PermanentUI.GetInstance().exp >= 1500)
        {
            PermanentUI.GetInstance().maxHealthPotion += 1;
            if (PermanentUI.GetInstance().maxHealthPotion > PermanentUI.GetInstance().potions.Length)
            {
                PermanentUI.GetInstance().maxHealthPotion = PermanentUI.GetInstance().potions.Length;
            }

            PermanentUI.GetInstance().potions[PermanentUI.GetInstance().healthPotion].gameObject.SetActive(true);
            PermanentUI.GetInstance().healthPotion += 1;
            PermanentUI.GetInstance().LoseExperience(1500);
        }
    }

    public void StopInteract()
    {
    }
}
