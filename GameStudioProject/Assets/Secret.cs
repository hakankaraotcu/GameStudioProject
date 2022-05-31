using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Secret : MonoBehaviour, IInteractable
{
    [SerializeField] private Transform targetArea;
    public void Interact()
    {
        PlayerController.GetInstance().transform.position = targetArea.position;
        if(PlayerController.GetInstance().transform.position.x < targetArea.position.x)
        {
            PlayerController.GetInstance().transform.localScale = new Vector2(3, 3);
        }
        else
        {
            PlayerController.GetInstance().transform.localScale = new Vector2(-3, 3);
        }
    }

    public void StopInteract()
    {
    }
}
