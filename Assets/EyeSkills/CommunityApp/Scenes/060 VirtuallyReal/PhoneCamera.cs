/*
Copyright 2019 Dr. Thomas Benjamin Senior, Michael Zoeller 

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace EyeSkills.Experiences
{
    public class PhoneCamera : MonoBehaviour
    {
        private bool camAvailable;
        private WebCamTexture backCam;

        public RawImage background;
        public AspectRatioFitter fit;

        void OnEnable()
        {
            SceneManager.sceneUnloaded += OnSceneUnloaded;
        }

        void OnSceneUnloaded(Scene scene)
        {
            if ((background != null) && (background.texture != null)){
                background.texture = null;
            }
            backCam.Stop();
            camAvailable = false;
        }

        void Start()
        {
            Debug.Log("Starting PhoneCamera script");
            WebCamDevice[] devices = WebCamTexture.devices;

            if (devices.Length == 0)
            {
                Debug.Log("No camera detected");
                camAvailable = false;
                return;
            }

            for (int i = 0; i < devices.Length; i++){
                Debug.Log("Found cam " + devices[i].name);
                //if (!devices[i].isFrontFacing){
                //    backCam = new WebCamTexture(devices[i].name,Screen.width, Screen.height);
                //    Debug.Log("Set cam to camera " + i);
                //}

            }
            //0 is main camera with auto focus
            //2 is wide-angle
            Debug.Log("Trying to set Backcamera");
            backCam = new WebCamTexture(devices[0].name, Screen.width, Screen.height);
            Debug.Log("Back camera set");
            if (backCam == null)
            {
                Debug.LogWarning("Could not find backward facing camera.");
                return;
            }
            Debug.Log("Setting texture");
            background.texture = backCam;
            Debug.Log("Background texture set");
            Debug.Log("About to play back camera " + backCam.deviceName + " name: " + backCam.name);
            backCam.Play();
            camAvailable = true;
        }

        void Update()
        {
            if (!camAvailable)
                return;

            float ratio = (float) backCam.width / (float) backCam.height;
            fit.aspectRatio = ratio;

            float scaleY = backCam.videoVerticallyMirrored ? -1f : 1f;
            background.rectTransform.localScale = new Vector3(1f, scaleY, 1f);

            int orient = -backCam.videoRotationAngle;
            background.rectTransform.localEulerAngles = new Vector3(0, 0, orient);
        }
    }
}
