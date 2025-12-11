using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace LightningStrikeEffect
{
    public class DelayedDestroy : MonoBehaviour
    {
        public float lifeTime = 5.0f;
        // Start is called before the first frame update
        void Start()
        {
            StartCoroutine(DelayedDeath());
        }

        private IEnumerator DelayedDeath()
        {
            yield return new WaitForSeconds(lifeTime);
            Destroy(gameObject);
        }
    }
}
