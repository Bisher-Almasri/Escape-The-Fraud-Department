using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float lifeTime = 3f;

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Enemy"))
        {
            Debug.Log("Bullet hit an enemy!");
            EnemyHealth enemyHealth = collision.collider.TryGetComponent<EnemyHealth>(out var healthComponent) ? healthComponent : null;
            if (enemyHealth == null)
            {
                collision.collider.gameObject.AddComponent<EnemyHealth>();
                enemyHealth = collision.collider.GetComponent<EnemyHealth>();
            }
            enemyHealth.TakeDamage(10);
        }

        Destroy(gameObject);
    }
}
