using UnityEngine;
using System.Collections.Generic;

namespace EZCameraShake
{
    [DefaultExecutionOrder(10000)]
    [AddComponentMenu("EZ Camera Shake/Camera Shaker")]
    public class CameraShaker : MonoBehaviour
    {

        //public Vector3 DefaultPosInfluence = new Vector3(0.15f, 0.15f, 0.15f);
        //public Vector3 DefaultRotInfluence = new Vector3(1, 1, 1);
        public Vector3 RestPositionOffset = new Vector3(0, 0, 0);
        public Vector3 RestRotationOffset = new Vector3(0, 0, 0);

        public CameraShakeInstance shake;

        private Quaternion baseRotation, appliedRotation;
        private bool rotationApplied;

        private void RestoreRotation()
        {
            if (!rotationApplied) return;
            // Absolute camera controllers may already have replaced our previous pose.
            if (Quaternion.Angle(transform.localRotation, appliedRotation) < 0.001f)
                transform.localRotation = baseRotation;
            rotationApplied = false;
        }

        private void LateUpdate()
        {
            RestoreRotation();
            if (shake == null) return;
            if (shake.CurrentState == CameraShakeState.Inactive)
            {
                shake = null;
                return;
            }
            Vector3 offset = shake.UpdateShake();
            Vector3 rotation = CameraUtilities.MultiplyVectors(offset, shake.RotationInfluence);
            baseRotation = transform.localRotation;
            appliedRotation = baseRotation * Quaternion.Euler(rotation + RestRotationOffset);
            transform.localRotation = appliedRotation;
            rotationApplied = true;
        }

        private void OnDisable() => RestoreRotation();

        public CameraShakeInstance ShakeOnce(float magnitude, float roughness, float fadeInTime, float fadeOutTime, Vector3 posInfluence, Vector3 rotInfluence)
        {
            //Debug.Log("Shaking uPPPP");
            shake = new CameraShakeInstance(magnitude, roughness, fadeInTime, fadeOutTime);
            shake.PositionInfluence = posInfluence;
            shake.RotationInfluence = rotInfluence;

            return shake;
        }

        public CameraShakeInstance StartShake(float magnitude, float roughness, float fadeInTime, Vector3 posInfluence, Vector3 rotInfluence)
        {
            shake = new CameraShakeInstance(magnitude, roughness);
            shake.PositionInfluence = posInfluence;
            shake.RotationInfluence = rotInfluence;
            shake.StartFadeIn(fadeInTime);
            return shake;
        }
    }
}