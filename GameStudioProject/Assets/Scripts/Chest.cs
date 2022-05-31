using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chest : MonoBehaviour, IInteractable
{
    [SerializeField] private Animator anim;
    private bool isOpen;

    private void Start()
    {
        anim = GetComponent<Animator>();
    }

    public void Interact()
    {
        if (isOpen)
        {
            StopInteract();
        }
        else
        {
            isOpen = true;
            anim.SetTrigger("open");
            TextManager.GetInstance().CreateText(transform.position, "500");
            PermanentUI.GetInstance().exp += 500;
            PermanentUI.GetInstance().GainExperience();
        }
    }

    public void StopInteract()
    {
    }
}
