using UnityEngine;

public class PlayerAim : MonoBehaviour
{
    private Transform playerCamera; // Reference to the player's camera

    void Start()
    {
        // Find the player's camera by looking for the "MainCamera" tag
        var camera = Camera.main;
        playerCamera = camera != null ? camera.transform : null;
    }

    void LateUpdate()
    {
        Move();
    }

    void Move()
    {
        // Set the aim object's position to be the same as the player's camera
        //transform.position = playerCamera.position;

        // Move the aim object back by the desired distance
        //transform.Translate(Vector3.back * - 14f);

        // Smoothly rotate the aim object to face the same direction as the player's camera
        if (playerCamera == null) return;
        transform.rotation = Quaternion.Slerp(transform.rotation, playerCamera.rotation, 10f * Time.deltaTime);
    
    }
}
