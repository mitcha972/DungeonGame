/* 
    ------------------- Code Monkey -------------------

    Thank you for downloading this package
    I hope you find it useful in your projects
    If you have any questions let me know
    Cheers!

               unitycodemonkey.com
    --------------------------------------------------
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialTintColor : MonoBehaviour {
    
    private Material material;
    private Color materialTintColor;
    private float tintFadeSpeed;
    private Material ownedMaterial;
    private Material originalMaterial;
    private MeshRenderer sourceRenderer;

    private void Awake() {
        materialTintColor = new Color(1, 0, 0, 0);
        Transform body = transform.Find("Body");
        sourceRenderer = body != null ? body.GetComponent<MeshRenderer>() : null;
        if (sourceRenderer != null && sourceRenderer.sharedMaterial != null) {
            originalMaterial = sourceRenderer.sharedMaterial;
            ownedMaterial = new Material(originalMaterial);
            sourceRenderer.sharedMaterial = ownedMaterial;
            material = ownedMaterial;
        }
        tintFadeSpeed = 6f;
    }

    private void Update() {
        if (material != null && materialTintColor.a > 0) {
            materialTintColor.a = Mathf.Clamp01(materialTintColor.a - tintFadeSpeed * Time.deltaTime);
            material.SetColor("_Tint", materialTintColor);
        }
    }

    public void SetMaterial(Material material) {
        if (ownedMaterial != null && material != ownedMaterial) ReleaseOwnedMaterial();
        this.material = material;
    }

    public void SetTintColor(Color color) {
        materialTintColor = color;
        if (material != null) material.SetColor("_Tint", materialTintColor);
    }

    private void OnDestroy() => ReleaseOwnedMaterial();

    private void ReleaseOwnedMaterial() {
        if (ownedMaterial == null) return;
        if (sourceRenderer != null && sourceRenderer.sharedMaterial == ownedMaterial)
            sourceRenderer.sharedMaterial = originalMaterial;
        Destroy(ownedMaterial);
        ownedMaterial = null;
    }

    public void SetTintFadeSpeed(float tintFadeSpeed) {
        this.tintFadeSpeed = tintFadeSpeed;
    }

}
