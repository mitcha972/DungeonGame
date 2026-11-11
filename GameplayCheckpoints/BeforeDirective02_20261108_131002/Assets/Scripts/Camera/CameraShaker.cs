using UnityEngine;
using System.Collections.Generic;

namespace EZCameraShake
{
    [AddComponentMenu("EZ Camera Shake/Camera Shaker")]
    public class CameraShaker : MonoBehaviour
    {

        //public Vector3 DefaultPosInfluence = new Vector3(0.15f, 0.15f, 0.15f);
        //public Vector3 DefaultRotInfluence = new Vector3(1, 1, 1);
        public Vector3 RestPositionOffset = new Vector3(0, 0, 0);
        public Vector3 RestRotationOffset = new Vector3(0, 0, 0);

        public CameraShakeInstance shake;

        Vector3 posAddShake, rotAddShake;

        void Update()
        {
            if(shake != null)
            {

                if (shake.CurrentState == CameraShakeState.Inactive)
                {
                    shake = null;
                    return;
                }
                
                posAddShake = Vector3.zero;
                rotAddShake = Vector3.zero;

                posAddShake += CameraUtilities.MultiplyVectors(shake.UpdateShake(), shake.PositionInfluence);
                rotAddShake += CameraUtilities.MultiplyVectors(shake.UpdateShake(), shake.RotationInfluence);
                
                transform.localEulerAngles += rotAddShake + RestRotationOffset;
            }
        }

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
            CameraShakeInstance shake = new CameraShakeInstance(magnitude, roughness);
            shake.PositionInfluence = posInfluence;
            shake.RotationInfluence = rotInfluence;
            shake.StartFadeIn(fadeInTime);
            return shake;
        }
    }
}