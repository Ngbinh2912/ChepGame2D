using TMPro;
using UnityEngine;

public class CombatText : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI combatText;

    public void OnInit(float damage)
    {
        combatText.text = damage.ToString();
        Invoke(nameof(OnDespawn), 1f);
    }

    public void OnInit(string textContent)
    {
        combatText.text = textContent;
        Invoke(nameof(OnDespawn), 1.5f);
    }

    public void OnDespawn()
    {
        Destroy(gameObject);
    }
}
