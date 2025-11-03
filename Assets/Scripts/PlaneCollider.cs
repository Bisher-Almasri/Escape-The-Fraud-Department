using UnityEngine;
using UnityEngine.SceneManagement;

public class PlaneCollider : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            Debug.Log("Plane collided with Player!");
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
