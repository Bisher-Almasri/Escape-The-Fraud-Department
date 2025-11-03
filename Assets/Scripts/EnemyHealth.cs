using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class EnemyHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    private float currentHealth;
    private Renderer rend;
    private Color originalColor;

    void Start()
    {
        currentHealth = maxHealth;

        GameObject plane = transform.Find("Plane").gameObject;

        rend = plane.GetComponent<Renderer>();
        if (rend != null)
            originalColor = rend.material.color;
    }

    public void TakeDamage(int damage)
    {
        Debug.Log($"{gameObject.name} took {damage} damage!");

        currentHealth -= damage;

        if (rend != null)
            StartCoroutine(TintRed());

        if (currentHealth <= 0)
            Die();
    }

    private IEnumerator TintRed()
    {
        float duration = 0.3f;
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            rend.material.color = Color.Lerp(originalColor, Color.red, t / duration);
            yield return null;
        }

        t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            rend.material.color = Color.Lerp(Color.red, originalColor, t / duration);
            yield return null;
        }

        rend.material.color = originalColor;
    }

    private void Die()
    {
        Debug.Log($"{gameObject.name} has died.");
        Destroy(gameObject.transform.parent.gameObject);
    }
}
