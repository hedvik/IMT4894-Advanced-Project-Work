using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShield : MonoBehaviour {
    public PlayerManager _playerManager;

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("projectile"))
        {
            var projectile = other.gameObject.GetComponent<Projectile>();
            _playerManager.AddCharge(projectile._chargeValue);
            Destroy(other.gameObject);
        }
    }
}
