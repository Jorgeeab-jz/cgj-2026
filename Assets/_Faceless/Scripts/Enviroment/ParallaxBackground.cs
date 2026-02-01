using UnityEngine;

namespace Faceless
{
    public class ParallaxBackground : MonoBehaviour
    {
        [SerializeField] private Vector2 _parallaxEffectMultiplier;
        private Transform _cameraTransform;
        private Vector3 _lastCameraPosition;

        private void Start()
        {
            if (Camera.main != null) 
                _cameraTransform = Camera.main.transform;
            
            if (_cameraTransform == null)
            {
                Debug.LogError("Main Camera not found! Parallax effect will not work.");
                enabled = false;
                return;
            }

            _lastCameraPosition = _cameraTransform.position;
        }

        private void LateUpdate()
        {
            Vector3 deltaMovement = _cameraTransform.position - _lastCameraPosition;
            transform.position += new Vector3(deltaMovement.x * _parallaxEffectMultiplier.x, deltaMovement.y * _parallaxEffectMultiplier.y, 0);
            _lastCameraPosition = _cameraTransform.position;
        }
    }
}
