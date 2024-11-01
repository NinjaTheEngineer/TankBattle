using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Health : MonoBehaviour
{
    [SerializeField] Image imageFill;
    [SerializeField] int maxHealth = 100;
    int currentHealth;
    public int CurrentHealth => currentHealth;
    public void OnEnable()
    {
        currentHealth = maxHealth;
    }
    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        float fillAmount = (1f* currentHealth) / maxHealth;
        imageFill.fillAmount = fillAmount;
    }
}
