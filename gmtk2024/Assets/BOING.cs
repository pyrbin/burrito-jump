using FMODUnity;
using UnityEngine;

public class BOING : MonoBehaviour
{
    public EventReference BOINGSOUND;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameManager.Instance.Player.HealthZero += () => PlayBOING();
    }

    public void PlayBOING() {
        FMODUtil.PlayOneShot(BOINGSOUND);
    }
}
