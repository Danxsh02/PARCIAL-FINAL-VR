using UnityEngine;

public class WaterBucket : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Sponge sponge = other.GetComponent<Sponge>();
        if (sponge != null)
        {
            sponge.SetState(SpongeState.Wet);
        }
    }
}
