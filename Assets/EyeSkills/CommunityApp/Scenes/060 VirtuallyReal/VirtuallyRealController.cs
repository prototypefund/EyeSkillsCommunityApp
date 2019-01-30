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

namespace EyeSkills.Experiences
{
    public class VirtuallyRealController : MonoBehaviour
    {
        // TODO: Why is the binocular suppression ratio not being correctly applied?
        // TODO: Getting audio working in android : https://developer.vuforia.com/forum/unity/video-playback-no-sound

        private StereoTargetEyeMask eyeMask;

        private Vector3 misalignmentRotation;
        private float currentAngle;
        private EyeSkillsInput esInput;
        public PhoneCamera phoneCamera;
        public GameObject blinker1, blinker2;
        private int currentCam=0;
        private bool userWantsStraightening = false;
        private bool areRemovingSuppression = false;
        public float suppressionReductionRate = 0.1f;
        private float originalSuppressionRatio, currentSuppressionRatio;
        public EyeSkillsVRHeadsetSelectByShake chooseCancel;
        private bool practitionerMode = false;
        public float turningRate = 2f;

        EyeSkillsCameraRig cameraRig;

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

        /// <summary>
        /// Resets the eye misalignment - does not start the rotation
        /// </summary>
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

            blinker1.SetActive(false);
            blinker2.SetActive(false);

            Debug.Log("Starting Virtually Real Realignment");
            cameraRig = FindObjectOfType<EyeSkillsCameraRig>();

            NetworkManager.instance.RegisterScene("Eye Straightening", "How long can the participant hold fusion as we straighten up their world?");
            NetworkManager.instance.RegisterButton("start", "Start/Re-start straightening", "Start/Re-start straightening the eye from the misaligned position");
            NetworkManager.instance.RegisterButton("store", "Store best fusion loss angle", "Store the greatest angle at which fusion was lost");
            //NetworkManager.instance.RegisterButton("stop", "Stop straightening", "Pause the straightening at the point fusion was lost");
            NetworkManager.instance.RegisterFloat("degree", -45f, 45f, 1f, "Misalignment", "Angle between the eyes.");

            practitionerMode = (PlayerPrefs.GetString("EyeSkills.practitionerMode") == "1") ? true : false;

            //Pick up user calibration
            esInput = EyeSkillsInput.instance;
            FetchEyeCalibration();

            originalSuppressionRatio = cameraRig.config.binocularSuppressionRatio;
            cameraRig.config.leastMisalignmentBeforeFusionLost = 180f; //Make our initial "best" as bad as can be.
            ResetEyeMisalignment();
            userWantsStraightening = true; //This will case the camera to roate in the Update phase - ought to be a coroutine really.

        }

        void Update()
        {
            //TODO : Do not forget these experiences started as a quick experimental hack. They must be completely refactored.

            //Debug.Log("Time still " + still);
            if (!practitionerMode){
                if (chooseCancel.IsCancelled())
                {
                    ResetEyeMisalignment();
                    userWantsStraightening = true;
                }
            }

            //TODO: Confirm (or waiting a specified time, or resetting a given number of times) should exit, but restart/shaking the head should reset the camera - we ought to record each reset within the specified time.
            if (esInput.GetShortButtonPress("EyeSkills Confirm") || NetworkManager.instance.GetButton("store") || NetworkManager.instance.GetButton("start") || chooseCancel.IsCancelled()) //TODO : or SHAKE
            {
                // Time to log our new time to the calibration object.
                // We store the best (least) misalignment the participant achieved as an absolute angle
                if (cameraRig.config.leastMisalignmentBeforeFusionLost > currentAngle)
                {
                    Debug.Log("Storing least misalignment before fusion lost");
                    cameraRig.config.leastMisalignmentBeforeFusionLost = Mathf.Abs(currentAngle);
                    AudioManager.instance.Say("SavingBestAngle");
                }
                Debug.Log("Resetting eye");
                ResetEyeMisalignment();
                userWantsStraightening = true;
            }

            //if (esInput.GetShortButtonPress("EyeSkills Up"))
            //{
            //    AudioManager.instance.Say("BlinkersOn");
            //    blinker1.SetActive(true);
            //    blinker2.SetActive(true);
            //}
            //else if (esInput.GetShortButtonPress("EyeSkills Down"))
            //{
            //    AudioManager.instance.Say("BlinkersOff");
            //    blinker1.SetActive(false);
            //    blinker2.SetActive(false);
            //}

            if (userWantsStraightening)
            {
                //Debug.Log("Straightening");
                cameraRig.RotateToStraightenEye(eyeMask, turningRate * Time.deltaTime);

                // The least distance we were away from being straight.
                currentAngle = cameraRig.GetEyeAngle(eyeMask).y; //misalignedEye.transform.rotation.eulerAngles.y;
                if (currentAngle > 180)
                    currentAngle = 360 - currentAngle; //HACK. Need to think this through more thoroughly.

                //currentAngle = Vector3.Angle(misalignedEye.transform.rotation.eulerAngles, nonMisalignedEye.transform.rotation.eulerAngles).y;
                NetworkManager.instance.SetFloat("degree", currentAngle);
            }


            // Lets just hack the binocular suppression de-escalation to implicitly detect when it's needed
            //Debug.Log("current angle : " + currentAngle);
            if (Mathf.Approximately(currentAngle, 0) && !areRemovingSuppression)
            {
                AudioManager.instance.Say("Straightened");
                Debug.Log("Starting to remove suppression");
                areRemovingSuppression = true;
                currentSuppressionRatio = originalSuppressionRatio;
            }
            else if (!Mathf.Approximately(currentAngle, 0) && (areRemovingSuppression))
            {
                //the cameras are no longer straight, but we had been manipulating the suppression ratio, so reset it
                Debug.Log("Resetting suppression");
                AudioManager.instance.Say("Resetting");
                cameraRig.SetBinocularSuppressionRatio(originalSuppressionRatio);
                areRemovingSuppression = false;
            }

            if (areRemovingSuppression)
            {
                // Now we need to gradually straighten up the binocular suppression ratio
                if (!Mathf.Approximately(currentSuppressionRatio, 0f))
                {
                    currentSuppressionRatio = currentSuppressionRatio -
                                              (currentSuppressionRatio * suppressionReductionRate * Time.deltaTime);
                    //Debug.Log("Current suppression ratio " + currentSuppressionRatio);
                    cameraRig.SetBinocularSuppressionRatio(currentSuppressionRatio);

                    if (Mathf.Approximately(currentSuppressionRatio, 0f))
                        AudioManager.instance.Say("SuppressionCompensationRemoved");
                }
            }
            if (esInput.GetShortButtonPress("EyeSkills Up")){ //Cycle through the available cameras
                int camID = currentCam % phoneCamera.getNumberOfCameras();
                Debug.Log("Trying to choose camera number " + camID);
                phoneCamera.startCamera(camID);
                currentCam += 1;
            }
        }
    }
}