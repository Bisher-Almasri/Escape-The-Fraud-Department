
using UnityEngine;

public class SprintItem : MonoBehaviour
{
    public float sprintDuration = 5f;
    public float sprintCooldown = 10f;

    private PlayerMovement playerMovement;
    private bool isSprinting;
    private void Update()
    {
        if (isSprinting)
        {
            sprintDuration -= Time.deltaTime;
            if (sprintDuration <= 0)
            {
                StopSprinting();
            }
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            playerMovement = other.gameObject.GetComponent<PlayerMovement>();
            StartSprinting();
            Destroy(gameObject);
        }
    }

    private void StartSprinting()
    {
        if (playerMovement != null)
        {
            playerMovement.canSprint = true;
            isSprinting = true;
        }
    }

    private void StopSprinting()
    {
        if (playerMovement != null)
        {
            playerMovement.canSprint = false;
            isSprinting = false;
            sprintDuration = 5f;
            sprintCooldown = 10f;
        }
    }
}
