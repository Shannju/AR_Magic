using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LightningStrikeEffect
{

    public class RandomSpawn : MonoBehaviour
    {
        [Header("Spawn")]
        public GameObject[] spawnObjectPrefabs;
        public float distanceXZRange = 10.0f;
        public float distanceYRange = 10.0f;
        public float spwanTimeGap = 1.0f;

        [Header("CameraShake")]
        public Camera shakeCamera;
        public float shakeDuration = 0.5f;
        public float shakeMagnitude = 0.2f;
        public float dampingSpeed = 1.0f;
        private Vector3 initialPosition;
        private float currentShakeDuration = 0f;








        void Start()
        {
            StartCoroutine(Spawn());
            initialPosition = shakeCamera.transform.localPosition;
        }

        // Update is called once per frame
        void Update()
        {
            CalculateCameraShake();
        }

        private IEnumerator Spawn()
        {
            while(true)
            {

                Vector3 randomPos = transform.position + new Vector3(Random.Range(-distanceXZRange, distanceXZRange), Random.Range(-distanceYRange, distanceYRange), Random.Range(-distanceXZRange, distanceXZRange));
                int prefabIndex = Random.Range(0, spawnObjectPrefabs.Length);
                GameObject effect = Instantiate(spawnObjectPrefabs[prefabIndex], randomPos, Quaternion.identity);
                TriggerCameraShake();
                yield return new WaitForSeconds(spwanTimeGap);
            }
        }



        private void CalculateCameraShake()
        {
            if (currentShakeDuration > 0)
            {
                shakeCamera.transform.localPosition = initialPosition + Random.insideUnitSphere * shakeMagnitude;

                currentShakeDuration -= Time.deltaTime * dampingSpeed;
            }
            else
            {
                currentShakeDuration = 0f;
                shakeCamera.transform.localPosition = initialPosition;
            }
        }
        private void TriggerCameraShake()
        {
            currentShakeDuration = shakeDuration;
        }


    }
}
