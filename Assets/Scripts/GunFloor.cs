using UnityEngine;

public class GunFloor : MonoBehaviour
{
    [Header("References")]
    public GameObject gun;     
    public GameObject gunPrefab;
    public GameObject gunHand; 
    public Transform gunPoint;

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("GunFloor collided with " + collision.collider.name);

        if (collision.collider.CompareTag("Player"))
        {
            // delee gun from floor
            Destroy(gun);
            Debug.Log("Gun picked up by Player!");

            // check if prefab already exists
            if (gunHand.transform.Find(gunPrefab.name + "(Clone)") != null)
            {
                Debug.Log("Player already has a gun!");
                return;
            }

            // spawn gun in player's hand
            GameObject gunInHand = Instantiate(
                gunPrefab,
                collision.collider.transform.position,
                Quaternion.identity
            );

            // add GunHand component to new instance, not prefab
            GunHand gunHandComponent = gunInHand.AddComponent<GunHand>();
            gunHandComponent.gunPoint = gunPoint;

            gunInHand.transform.position = gunHand.transform.position;
            gunInHand.transform.rotation = gunHand.transform.rotation;

            // (Optional) parent to player so it follows
            gunInHand.transform.SetParent(gunHand.transform, worldPositionStays: true);
        }
    }
}
