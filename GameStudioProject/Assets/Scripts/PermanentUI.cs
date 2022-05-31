using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PermanentUI : MonoBehaviour
{
    public GameObject[] potions;
    public int healthPotion = 0;
    public int maxHealthPotion;
    public int maxHealth = 100;
    public int currentHealth;
    public int maxStamina = 100;
    public int currentStamina;
    public int exp = 0;
    public TextMeshProUGUI expText;

    private static PermanentUI perm;

    private PermanentUI()
    {
        if (perm == null)
        {
            perm = this;
        }
    }

    public static PermanentUI GetInstance()
    {
        return perm;
    }

    public void LoseExperience(int lostExp)
    {
        exp -= lostExp;
        if(exp < 0)
        {
            exp = 0;
        }
        expText.text = exp.ToString();
    }

    public void GainExperience()
    {
        expText.text = exp.ToString();
    }

    public void Respawn()
    {
        currentHealth = maxHealth;
        currentStamina = maxStamina;
        PlayerController.GetInstance().healthBar.SetHealth(currentHealth);
        PlayerController.GetInstance().staminaBar.SetStamina(currentStamina);

        while (PermanentUI.GetInstance().healthPotion < PermanentUI.GetInstance().maxHealthPotion)
        {
            PermanentUI.GetInstance().potions[PermanentUI.GetInstance().healthPotion].gameObject.SetActive(true);
            PermanentUI.GetInstance().healthPotion += 1;
        }
        PlayerController.GetInstance().anim.SetBool("isDead", false);
        PlayerController.GetInstance().anim.Play("Idle");
    }
}
