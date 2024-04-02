using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Steps : MonoBehaviour
{
    public float stepRangeRadius;

    public AudioClip[] stepSounds;
    public AudioSource source;

    public void PlayStepNoise()
    {
        // check sphere of radius stepradius 
        // for each enemy in the overlapsphere, raycast to it from player
        // if no wall blocking, alert them

        source.PlayOneShot(stepSounds[Random.Range(0, stepSounds.Length)]);

        Collider[] hits = Physics.OverlapSphere(transform.position, stepRangeRadius);
        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                if (Physics.Raycast(transform.position, hit.transform.position, stepRangeRadius))
                {
                    hit.GetComponent<EnemyMove>().NoiseHeard(transform);
                }
            }
        }

    }
}
