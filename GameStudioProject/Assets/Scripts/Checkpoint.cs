using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour, IInteractable
{
    public void Interact()
    {
        PermanentUI.GetInstance().Respawn();
    }

    public void StopInteract()
    {
    }
}
