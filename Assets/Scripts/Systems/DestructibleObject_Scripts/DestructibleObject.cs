using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeMonkey.Utils;

public class DestructibleObject : MonoBehaviour, IDamageable 
{

    [SerializeField] private Transform pfObjectBroken;
    [SerializeField] private Transform vfxSmoke;
    //[SerializeField] private int vfxAmount = 5;
    [SerializeField] private int healthAmountMax = 100;
    [SerializeField] private float explosionForce = 200f;

    private HealthSystem healthSystem;
    private Vector3 lastDamagePosition;

    private void Awake() 
    {
        healthSystem = new HealthSystem(healthAmountMax);
        healthSystem.OnDead += HealthSystem_OnDead;
    }

    public virtual void HealthSystem_OnDead(object sender, System.EventArgs e) 
    {
        // Spawn Smoke VFX
        if (vfxSmoke != null) Instantiate(vfxSmoke, lastDamagePosition, Quaternion.identity);
        //for (int i = 1; i < vfxAmount; i++) {
        //    Instantiate(vfxSmoke, lastDamagePosition + UtilsClass.GetRandomDir() * 2f, Quaternion.identity);
        //}

        // Spawn Wall Broken and Apply Explosion
        Transform wallBrokenTransform = pfObjectBroken != null ? Instantiate(pfObjectBroken, transform.position, transform.rotation) : null;
        if (wallBrokenTransform != null) foreach (Transform child in wallBrokenTransform) {
            if (child.TryGetComponent<Rigidbody>(out Rigidbody childRigidbody)) {
                childRigidbody.AddExplosionForce(explosionForce, lastDamagePosition, 10f);
            }
        }

        // Destroy this game object
        Destroy(gameObject);
        Debug.Log("Destroyed!");
    }

    private void OnDestroy()
    {
        if (healthSystem != null) healthSystem.OnDead -= HealthSystem_OnDead;
    }

    public void Damage(int damageAmount, Vector3 damagePosition) {
        lastDamagePosition = damagePosition;
        healthSystem.Damage(damageAmount);
    }

}