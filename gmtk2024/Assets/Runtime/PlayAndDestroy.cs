using System.Collections;
using UnityEngine;

public class PlayAndDestroy : MonoBehaviour
{
    public Duration Duration;

    void Start()
    {
        GetComponent<ParticleSystem>().Play();
        StartCoroutine(DestroyObjectAfterDelay(this.gameObject, Duration.Seconds));
    }

    IEnumerator DestroyObjectAfterDelay(GameObject objectToDestroy, float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(objectToDestroy);
    }
}
