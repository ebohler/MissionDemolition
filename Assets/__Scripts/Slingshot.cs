using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slingshot : MonoBehaviour
{
    [Header("Inscribed")]
    public GameObject projectilePrefab;
    public float velocityMult = 10f;
    public GameObject projLinePrefab;

    [Header("Dynamic")]
    public GameObject launchPoint;
    public Vector3 launchPos;
    public GameObject projectile;
    public bool aimingMode;

    private LineRenderer lineRenderer;  // LineRenderer for drawing lines
    private AudioSource audioSource;    // AudioSource for sound effects
    public AudioClip fireSound;         // Sound effect for firing the slingshot

    void Awake() {
        Transform launchPointTrans = transform.Find("LaunchPoint");
        launchPoint = launchPointTrans.gameObject;
        launchPoint.SetActive(false);
        launchPos = launchPointTrans.position;

        // Get the LineRenderer component
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 0;  // Initialize to no positions
        
        // Get the AudioSource component
        audioSource = GetComponent<AudioSource>();
    }

    void OnMouseEnter() {
        launchPoint.SetActive(true);
        // Prepare the LineRenderer for drawing
        lineRenderer.positionCount = 2;  // We want to draw 2 lines
    }

    void OnMouseExit() {
        launchPoint.SetActive(false);
        // Clear the LineRenderer when not aiming
        lineRenderer.positionCount = 0;  
    }

    void OnMouseDown() {
        if (MissionDemolition.S.shotsLeft <= 0) return;

        // The player has pressed the mouse button while over Slingshot
        aimingMode = true;
        // Instantiate a projectile
        projectile = Instantiate(projectilePrefab) as GameObject;
        // Start it at the launchPoint
        projectile.transform.position = launchPos;
        // Set it to isKinematic for now
        projectile.GetComponent<Rigidbody>().isKinematic = true;
    }

    void Update() {
        // If slingshot not in aiming mode don't run this code
        if (!aimingMode) return;

        // Get the current mouse pos in 2D screen coords
        Vector3 mousePos2D = Input.mousePosition;
        mousePos2D.z = -Camera.main.transform.position.z; // Set this to be in front of the camera
        Vector3 mousePos3D = Camera.main.ScreenToWorldPoint(mousePos2D);

        // Find the delta from the launchPos to the mousePos3D
        Vector3 mouseDelta = mousePos3D - launchPos;
        // Limit mouseDelta to the radius of the Slingshot SphereCollider
        float maxMagnitude = this.GetComponent<SphereCollider>().radius;
        if (mouseDelta.magnitude > maxMagnitude) {
            mouseDelta.Normalize();
            mouseDelta *= maxMagnitude;
        }
        // Move the projectile to this new position
        Vector3 projPos = launchPos + mouseDelta;
        projectile.transform.position = projPos;

        // Update the LineRenderer positions to draw lines to the mouse cursor
        lineRenderer.SetPosition(0, launchPos);      // First line from launch position
        lineRenderer.SetPosition(1, projPos);        // Second line to the projectile position

        if (Input.GetMouseButtonUp(0)) {
            // The mouse has been released
            aimingMode = false;
            Rigidbody projRB = projectile.GetComponent<Rigidbody>();
            projRB.isKinematic = false;
            projRB.collisionDetectionMode = CollisionDetectionMode.Continuous;
            projRB.velocity = -mouseDelta * velocityMult;

            FollowCam.SWITCH_VIEW(FollowCam.eView.slingshot);

            FollowCam.POI = projectile; // Set the _MainCamera POI
            Instantiate<GameObject>(projLinePrefab, projectile.transform);
            projectile = null;

            // Play the firing sound effect
            if (audioSource && fireSound) {
                audioSource.PlayOneShot(fireSound);  // Play sound when firing
            }

            // Clear the LineRenderer after firing
            lineRenderer.positionCount = 0;  
            MissionDemolition.SHOT_FIRED();
        }
    }
}
