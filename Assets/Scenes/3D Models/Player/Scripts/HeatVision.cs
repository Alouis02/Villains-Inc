using UnityEngine;

public class HeatVision : MonoBehaviour
{
    [Header("Heat Vision")]
    [SerializeField] private LineRenderer heatVision;
    [SerializeField] private Transform firePoint;

    [Header("Impact Effect")]
    [SerializeField] private ParticleSystem blastEffect;

    [Header("Settings")]
    [SerializeField] private float maxDistance = 1000f;

    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;

        heatVision.enabled = false;

        if (blastEffect != null)
        {
            blastEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }

    private void Update()
    {
        if (Input.GetMouseButton(1))
        {
            FireHeatVision();
        }
        else
        {
            StopHeatVision();
        }
    }

    private void FireHeatVision()
    {
        if (mainCamera == null || firePoint == null)
            return;

        // Create an aiming ray from the center of the camera.
        Ray cameraRay = mainCamera.ViewportPointToRay(
            new Vector3(0.5f, 0.5f, 0f)
        );

        Vector3 targetPoint;

        // Raycast from the camera to determine what the player is aiming at.
        if (Physics.Raycast(cameraRay, out RaycastHit hit, maxDistance))
        {
            targetPoint = hit.point;

            // Move impact particles to the hit location.
            if (blastEffect != null)
            {
                blastEffect.transform.position = hit.point;

                // Make the particles face the surface.
                blastEffect.transform.rotation =
                    Quaternion.LookRotation(hit.normal);

                if (!blastEffect.isPlaying)
                {
                    blastEffect.Play();
                }
            }
        }
        else
        {
            // Nothing was hit, so extend the beam forward.
            targetPoint = cameraRay.origin +
                          cameraRay.direction * maxDistance;

            // No impact particles when nothing is hit.
            if (blastEffect != null && blastEffect.isPlaying)
            {
                blastEffect.Stop(
                    true,
                    ParticleSystemStopBehavior.StopEmittingAndClear
                );
            }
        }

        // Draw the actual heat beam.
        heatVision.SetPosition(0, firePoint.position);
        heatVision.SetPosition(1, targetPoint);

        heatVision.enabled = true;
    }

    private void StopHeatVision()
    {
        heatVision.enabled = false;

        if (blastEffect != null && blastEffect.isPlaying)
        {
            blastEffect.Stop(
                true,
                ParticleSystemStopBehavior.StopEmittingAndClear
            );
        }
    }
}