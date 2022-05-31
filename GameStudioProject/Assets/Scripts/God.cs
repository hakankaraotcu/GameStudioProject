using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class God : MonoBehaviour, IInteractable
{

    public void Interact()
    {
        if(PlayerController.GetInstance().extraJumps != 2)
        {
            if (PermanentUI.GetInstance().exp >= 3000)
            {
                PlayerController.GetInstance().extraJumps++;
                PermanentUI.GetInstance().LoseExperience(3000);
            }
        }
        else
        {
            StopInteract();
        }
    }

    public void StopInteract()
    {
    }
}
