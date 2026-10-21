using UnityEngine;

public class TirggerGroundSlam : MonoBehaviour
{
    [SerializeField] ParticleSystem triggerGroundSlam = null;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Trigger();
        }
    }

    public void Trigger()
    {
        if (triggerGroundSlam == null)
        {
            Debug.LogError("Ground slam particle system is not assigned.", this);
            return;
        }

        triggerGroundSlam.Play();
    }
}