using UnityEngine;

public class GunFloor : MonoBehaviour
{
    [Header("References")]
    public GameObject gunPrefab;
    public GameObject bulletPrefab;
    
    [Header("Auto-Setup")]
    public bool autoFindReferences = true;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("GunFloor triggered by " + other.name);

        if (other.CompareTag("Player"))
        {
            PickupGun(other.gameObject);
        }
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("GunFloor collided with " + collision.collider.name);

        if (collision.collider.CompareTag("Player"))
        {
            PickupGun(collision.collider.gameObject);
        }
    }
    
    private void PickupGun(GameObject player)
    {
        Transform gunHand = FindGunHandInPlayer(player);
        Transform gunPoint = FindGunPointInPlayer(player);
        
        if (gunHand == null)
        {
            Debug.LogWarning("Could not find GunHand in player hierarchy!");
            return;
        }
        
        if (gunPoint == null)
        {
            Debug.LogWarning("Could not find GunPoint in player hierarchy!");
            return;
        }

        if (gunHand.childCount > 0)
        {
            Debug.Log("Player already has a gun!");
            return;
        }

        Destroy(gameObject);
        Debug.Log("Gun picked up by Player!");

        GameObject gunInHand = Instantiate(gunPrefab, gunHand.position, gunHand.rotation);

        GunHand gunHandComponent = gunInHand.GetComponent<GunHand>();
        if (gunHandComponent == null)
        {
            gunHandComponent = gunInHand.AddComponent<GunHand>();
        }
        
        gunHandComponent.gunPoint = gunPoint;
        gunHandComponent.bulletPrefab = bulletPrefab;

        gunInHand.transform.SetParent(gunHand, false);
    }
    
    private Transform FindGunHandInPlayer(GameObject player)
    {
        Transform gunHand = player.transform.Find("GunPos");
        if (gunHand != null) return gunHand;
        
        gunHand = player.transform.Find("GunHand");
        if (gunHand != null) return gunHand;
        
        gunHand = FindChildByName(player.transform, "GunPos");
        if (gunHand != null) return gunHand;
        
        return FindChildByName(player.transform, "GunHand");
    }
    
    private Transform FindGunPointInPlayer(GameObject player)
    {
        Transform gunPoint = player.transform.Find("GunPoint");
        if (gunPoint != null) return gunPoint;
        
        return FindChildByName(player.transform, "GunPoint");
    }
    
    private Transform FindChildByName(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name)
                return child;
                
            Transform found = FindChildByName(child, name);
            if (found != null)
                return found;
        }
        return null;
    }
}
