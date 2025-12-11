using System.Collections;
using System.Collections.Generic;
using UnityEngine;




namespace LightningStrikeEffect
{
    [ExecuteInEditMode]
    public class DepthTexture : MonoBehaviour
    {
        private Camera currentCam;
        void Start()
        {
            currentCam = GetComponent<Camera>();
            currentCam.depthTextureMode = DepthTextureMode.Depth;
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
