using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PieceUI : MonoBehaviour
{
    [SerializeField] Text damageText;
    [SerializeField] Text healthText;

    public void SetDamage(int damage)
    {
        damageText.text = "Dmg: " + damage.ToString();
    }

    public void SetHealth(int currentHealth, int maxHealth)
    {
        healthText.text = $"Hp: {currentHealth}/{maxHealth}";
    }

}
