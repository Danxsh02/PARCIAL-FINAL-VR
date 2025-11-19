using UnityEngine;

public class TrashChuteTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Bolsa"))
        {
            GameManager.Instance.RegistrarBolsaDepositada();

            Destroy(other.gameObject); // o animación cayendo
        }
    }
}
