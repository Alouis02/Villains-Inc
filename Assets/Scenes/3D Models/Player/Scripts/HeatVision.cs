using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HeatVision : MonoBehaviour
{
    [SerializeField] private LineRenderer heatvision;
    [SerializeField] private Transform firePoint;
    [SerializeField] private Transform target;
    [SerializeField] private ParticleSystem blastEffect;

    void Start()
    {
        heatvision.enabled = false;
    }

    void Update()
    {
        if (Input.GetMouseButton(1) || Input.GetMouseButtonDown(1)) // Hold Right mouse to shoot
        {
            Camera cam = Camera.main;
            if (cam == null)
                return;

            Ray ray = cam.ScreenPointToRay(new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0f));
            ray.origin = firePoint.position;

            Vector3 endpoint = ray.origin + ray.direction * 1000f; // Default endpoint if no hit occurs
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 1000f))
            {
                endpoint = hit.point;

                heatvision.SetPosition(0, firePoint.position);
                heatvision.SetPosition(1, endpoint);
                heatvision.enabled = true;

                Invoke("TurnOffHeatVision", .3f);

                blastEffect.transform.position = hit.point;
                blastEffect.Play();
            }

            else
            {
                heatvision.SetPosition(0, firePoint.position);
                heatvision.SetPosition(1, endpoint);
                heatvision.enabled = true;
                Invoke("TurnOffHeatVision", .3f);
            }

        }
    }

    private void TurnOffHeatVision()
    {
        heatvision.enabled = false;
    }
}