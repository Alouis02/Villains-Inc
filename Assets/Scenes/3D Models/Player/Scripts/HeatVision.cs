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
        // Cache the camera used to aim the heat vision.
        mainCamera = Camera.main;

        // Keep the beam hidden until firing.
        if (heatVision != null)
        {
            heatVision.enabled = false;
        }

        if (blastEffect != null)
        {
            // Force the particle effect to loop.
            var main = blastEffect.main;
            main.loop = true;

            // Make sure the effect starts turned off.
            blastEffect.Stop(
                true,
                ParticleSystemStopBehavior.StopEmittingAndClear
            );
        }
    }

    private void Update()
    {
        // Hold Right Mouse Button to fire.
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

        // Shoot an aiming ray from the center of the camera.
        Ray cameraRay = mainCamera.ViewportPointToRay(
            new Vector3(0.5f, 0.5f, 0f)
        );

        Vector3 targetPoint;

        // Check what the player is aiming at.
        if (Physics.Raycast(
            cameraRay,
            out RaycastHit hit,
            maxDistance
        ))
        {
            targetPoint = hit.point;

            // ------------------------------------------------
            // IMPACT EFFECT
            // ------------------------------------------------

            if (blastEffect != null)
            {
                // Move the effect to the hit location.
                blastEffect.transform.position = hit.point;

                // Make the effect face the surface.
                blastEffect.transform.rotation =
                    Quaternion.LookRotation(hit.normal);

                // Start the looping effect.
                if (!blastEffect.isPlaying)
                {
                    blastEffect.Play();
                }
            }
        }
        else
        {
            // No surface was hit.
            targetPoint =
                cameraRay.origin +
                cameraRay.direction * maxDistance;

            // Stop the impact effect because there is
            // nothing for the heat vision to hit.
            StopBlastEffect();
        }

        // ------------------------------------------------
        // HEAT VISION BEAM
        // ------------------------------------------------

        if (heatVision != null)
        {
            // Beam starts at the player's eyes/fire point.
            heatVision.SetPosition(
                0,
                firePoint.position
            );

            // Beam ends at whatever the camera is aiming at.
            heatVision.SetPosition(
                1,
                targetPoint
            );

            heatVision.enabled = true;
        }
    }

    private void StopHeatVision()
    {
        // Hide the beam.
        if (heatVision != null)
        {
            heatVision.enabled = false;
        }

        // Stop the impact effect.
        StopBlastEffect();
    }

    private void StopBlastEffect()
    {
        if (blastEffect != null && blastEffect.isPlaying)
        {
            blastEffect.Stop(
                true,
                ParticleSystemStopBehavior.StopEmittingAndClear
            );
        }
    }
}