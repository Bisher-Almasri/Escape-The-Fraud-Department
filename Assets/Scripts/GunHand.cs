using UnityEngine;

public class GunHand : MonoBehaviour
{
    [Header("Gun Settings")]
    public Transform gunPoint;
    public float gunRange = 100f;

    private PlayerControl controls;

    private void Awake()
    {
        controls = new PlayerControl();
    }

    private void OnEnable()
    {
        controls.Enable();
        controls.Player.Attack.performed += ctx => Shoot();
    }

    private void OnDisable()
    {
        controls.Player.Attack.performed -= ctx => Shoot();
        controls.Disable();
    }

    private void Shoot()
    {
        Debug.Log("Pew Pew!");
        if (gunPoint == null)
        {
            Debug.LogWarning("GunPoint not assigned!");
            return;
        }

        if (Physics.Raycast(gunPoint.position, gunPoint.forward, out RaycastHit hit, gunRange))
        {
            Debug.Log($"Shot fired at {hit.collider.name}");
            Debug.DrawLine(gunPoint.position, hit.point, Color.red, 1f);
            if (hit.collider.CompareTag("Enemy"))
            {
                hit.collider.GetComponent<EnemyHealth>()?.TakeDamage(10);
            }
        }
        else
        {
            Debug.Log("Shot fired, nothing hit.");
        }

    }
}
