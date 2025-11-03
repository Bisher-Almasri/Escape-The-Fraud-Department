using UnityEngine;

public class GunHand : MonoBehaviour
{
    [Header("Gun Settings")]
    public Transform gunPoint;
    public float gunRange = 100f;
    public float bulletSpeed = 50f;
    public GameObject bulletPrefab;
    public float fireRate = 0.1f; 

    private PlayerControl controls;
    private float lastShotTime;

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
        if (Time.time - lastShotTime < fireRate)
            return;
            
        lastShotTime = Time.time;
        Debug.Log("Pew Pew!");

        if (gunPoint == null)
        {
            Debug.LogWarning("GunPoint not assigned!");
            return;
        }

        if (bulletPrefab != null)
        {
            GameObject bullet = Instantiate(bulletPrefab, gunPoint.position, gunPoint.rotation * Quaternion.Euler(180,0,0));

            Rigidbody rb = bullet.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = gunPoint.forward * bulletSpeed;
            }
        }

        if (Physics.Raycast(gunPoint.position, gunPoint.forward, out RaycastHit hit, gunRange))
        {
            Debug.Log($"Shot fired at {hit.collider.name}");
            Debug.DrawLine(gunPoint.position, hit.point, Color.red, 1f);
        }
        else
        {
            Debug.Log("Shot fired, nothing hit.");
        }
    }
}
