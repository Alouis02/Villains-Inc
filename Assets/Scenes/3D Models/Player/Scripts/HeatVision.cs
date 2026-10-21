using UnityEngine;
using System;

public class HeatVision : MonoBehaviour
{
    [Header("Heat Vision")]
    [SerializeField] private LineRenderer heatVision;
    [SerializeField] private Transform firePoint;

    [Header("Impact Effect")]
    [SerializeField] private ParticleSystem blastEffect;
    [SerializeField] private float impactOffset = 0.02f;

    [Header("Raycast Settings")]
    [SerializeField] private float maxDistance = 1000f;
    [SerializeField] private LayerMask hitLayers = ~0;
    [SerializeField] private Transform playerRoot;

    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;

        if (heatVision != null)
        {
            heatVision.positionCount = 2;
            heatVision.useWorldSpace = true;
            heatVision.enabled = false;
        }

        StopBlastEffect();
    }

    private void Update()
    {
        if (!Input.GetMouseButton(1))
        {
            StopHeatVision();
            return;
        }

        if (mainCamera == null)
            mainCamera = Camera.main;

        if (mainCamera == null || firePoint == null)
        {
            StopHeatVision();
            return;
        }

        FireHeatVision();
    }

    private void FireHeatVision()
    {
        // Aim toward the center of the screen.
        Ray cameraRay = mainCamera.ViewportPointToRay(
            new Vector3(0.5f, 0.5f, 0f)
        );

        Vector3 targetPoint =
            cameraRay.origin + cameraRay.direction * maxDistance;

        bool hitSurface = false;
        RaycastHit hit = default;

        // Find the closest valid surface.
        RaycastHit[] hits = Physics.RaycastAll(
            cameraRay,
            maxDistance,
            hitLayers,
            QueryTriggerInteraction.Ignore
        );

        Array.Sort(
            hits,
            (a, b) => a.distance.CompareTo(b.distance)
        );

        foreach (RaycastHit candidate in hits)
        {
            if (playerRoot != null &&
                (candidate.transform == playerRoot ||
                 candidate.transform.IsChildOf(playerRoot)))
            {
                continue;
            }

            hit = candidate;
            targetPoint = hit.point;
            hitSurface = true;
            break;
        }

        // Draw the heat vision beam.
        if (heatVision != null)
        {
            heatVision.SetPosition(0, firePoint.position);
            heatVision.SetPosition(1, targetPoint);
            heatVision.enabled = true;
        }

        if (hitSurface)
        {
            UpdateImpactEffect(hit.point, hit.normal);
        }
        else
        {
            StopBlastEffect();
        }
    }

    private void UpdateImpactEffect(
        Vector3 hitPoint,
        Vector3 hitNormal)
    {
        if (blastEffect == null)
            return;

        blastEffect.transform.position =
            hitPoint + hitNormal * impactOffset;

        blastEffect.transform.rotation =
            Quaternion.LookRotation(hitNormal);

        if (!blastEffect.isPlaying)
            blastEffect.Play();
    }

    private void StopBlastEffect()
    {
        if (blastEffect == null)
            return;

        blastEffect.Stop(
            true,
            ParticleSystemStopBehavior.StopEmittingAndClear
        );
    }

    private void StopHeatVision()
    {
        if (heatVision != null)
            heatVision.enabled = false;

        StopBlastEffect();
    }

    private void OnDisable()
    {
        StopHeatVision();
    }
}