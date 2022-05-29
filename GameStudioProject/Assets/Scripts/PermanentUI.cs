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

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    public void LoseExperience()
    {
        exp -= 250;
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
}
