/*
Copyright 2019 Dr. Thomas Benjamin Senior, Michael Zoeller 

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

 using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EyeSkills;


namespace EyeSkills.Calibrations
{
    public class EyeStraighteningController : MonoBehaviour
    {
        [System.Serializable]
        public class EyeStraighteningConfig : ConfigBase
        {
            public float leastMisalignmentBeforeFusionLost;
        }

        public EyeStraighteningConfig config;
        public float turningRate = 2f;

        EyeSkillsCameraRig cameraRig;

        private StereoTargetEyeMask eyeMask;
        private Vector3 misalignmentRotation;
        private float currentAngle;
        private EyeSkillsInput esInput;
        private bool straightening = false;

        private void FetchEyeCalibration()
        {
            if (cameraRig.config.rightEyeIsStrabismic)
            {
                eyeMask = StereoTargetEyeMask.Right;
                misalignmentRotation = cameraRig.config.rightEyeMisalignmentAngle;
            }
            else
            {
                //default to left eye
                eyeMask = StereoTargetEyeMask.Left;
                misalignmentRotation = cameraRig.config.leftEyeMisalignmentAngle;
            }
        }

        private void ResetEyeMisalignment()
        {
            Debug.Log("Resetting eye misalignment");
            cameraRig.StraightenLeftEye();
            cameraRig.StraightenRightEye();
            cameraRig.Rotate(eyeMask, misalignmentRotation);
            //misalignedEye.transform.rotation = Quaternion.identity;
            //misalignedEye.transform.Rotate(misalignmentRotation);
        }

        void Start()
        {
            cameraRig = FindObjectOfType<EyeSkillsCameraRig>();

            NetworkManager.instance.RegisterScene("Eye Straightening", "How long can the participant hold fusion as we straighten up their world?");
            NetworkManager.instance.RegisterButton("start", "Start/Re-start straightening", "Start/Re-start straightening the eye from the misaligned position");
            NetworkManager.instance.RegisterButton("stop", "Stop straightening", "Pause the straightening at the point fusion was lost");
            NetworkManager.instance.RegisterFloat("degree", -45f, 45f, 1f, "Misalignment", "Angle between the eyes.");

            FetchEyeCalibration();
            ResetEyeMisalignment();
        }

        void Update()
        {
            // One press to start. Second to stop. Third to (re-)start
            if (Input.GetButtonDown("EyeSkills Confirm") || NetworkManager.instance.GetButton("start") || NetworkManager.instance.GetButton("stop"))
            {
                if (!straightening)
                {
                    ResetEyeMisalignment();
                    straightening = true;
                }
                else if (straightening)
                {
                    straightening = false;
                    // Time to log our new time to the calibration object.
                    // We store the best (least) misalignment the participant achieved as an absolute angle
                    Debug.Log("smallest angle in calibration " + config.leastMisalignmentBeforeFusionLost);
                    Debug.Log("current angle" + currentAngle);

                    if (config.leastMisalignmentBeforeFusionLost > currentAngle)
                    {
                        Debug.Log("Storing least misalignment before fusion lost");
                        config.leastMisalignmentBeforeFusionLost = Mathf.Abs(currentAngle);
                        AudioManager.instance.Say("SavingBestAngle");
                    }
                }
            }

            if (straightening)
            {
                //Debug.Log("Straightening");
                cameraRig.RotateToStraightenEye(eyeMask, turningRate * Time.deltaTime);

                // The least distance we were away from being straight.
                currentAngle = cameraRig.GetEyeAngle(eyeMask).y; //misalignedEye.transform.rotation.eulerAngles.y;
                if (currentAngle > 180) currentAngle = 360 - currentAngle; //HACK. Need to think this through more thoroughly.

                //currentAngle = Vector3.Angle(misalignedEye.transform.rotation.eulerAngles, nonMisalignedEye.transform.rotation.eulerAngles).y;
                NetworkManager.instance.SetFloat("degree", currentAngle);
            }
        }
    }
}