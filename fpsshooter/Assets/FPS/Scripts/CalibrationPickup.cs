using UnityEngine;
using UnityEngine.Events;

public class CalibrationPickup : MonoBehaviour
{
    Pickup m_Pickup;
    public UnityEvent onPickup;
    public UnityEventGameObject onPickupGameObject;

    void Start()
    {
        m_Pickup = GetComponent<Pickup>();
        DebugUtility.HandleErrorIfNullGetComponent<Pickup, HealthPickup>(m_Pickup, this, gameObject);

        // Subscribe to pickup action
        m_Pickup.onPick += OnPicked;
    }

    void OnPicked(PlayerCharacterController player)
    {
        onPickup.Invoke();
        onPickupGameObject.Invoke(this.gameObject);

        // remove the calibration object -- so don't call multiple times.
        Destroy(gameObject);
    }
}
