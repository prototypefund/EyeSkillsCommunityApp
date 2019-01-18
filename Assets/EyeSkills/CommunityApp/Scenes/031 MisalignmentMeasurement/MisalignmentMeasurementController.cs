/*
Copyright 2019 Dr. Thomas Benjamin Senior, Michael Zoeller 

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System.Collections;
using UnityEngine; 
using UnityEngine.SceneManagement;

namespace EyeSkills.Calibrations
{
    /// <summary>
    /// Detect eye rotation by having the user align two images. There is a model in here for tracking eye misalignment via the head. What we do with that data, or how we present the options,
    /// are to do with a controller, so we can and should split the two parts.
    /// </summary>
    public class MisalignmentMeasurementController : MonoBehaviour
    {
        protected StereoTargetEyeMask eye = StereoTargetEyeMask.None;
        private EyeSkillsInput esInput;
        protected AudioManager audioManager;
        protected bool rightEyeIsStrabismic = false;

        public float secondsOfStillnessForSelect = 10f;
        /// <summary>
        /// Our headset stillness detection controller
        /// </summary>
        public EyeSkillsVRHeadsetSelectByStillness stillness;

        /// <summary>
        /// The selection indicator.
        /// </summary>
        public SelectionIndicatorScaler indicator;

        private EyeSkillsCameraRig cameraRig;

        private bool practitionerMode = false;

        public void Start()
        {
            cameraRig = FindObjectOfType<EyeSkillsCameraRig>();
            esInput = EyeSkillsInput.instance;

            audioManager = AudioManager.instance;
            //audioManager.Say("ChooseStrabismicEye");

            if( cameraRig.config.leftEyeIsStrabismic )
            {
                audioManager.Say("LeftStrabismic");
                unlockLeftEye();
            } else
            {
                audioManager.Say("RightStrabismic");
                unlockRightEye();
            }

            practitionerMode = (PlayerPrefs.GetString("EyeSkills.practitionerMode") == "1") ? true : false;

            NetworkManager.instance.RegisterScene("Detect Eye Misalignment", "To what degree are the participants eyes misaligned?");
            NetworkManager.instance.RegisterButton("unlockRight", "Unlock right eye", "Right eye is strabismic");
            NetworkManager.instance.RegisterButton("unlockLeft", "Unlock left eye", "Left eye is strabismic");
            NetworkManager.instance.RegisterButton("save", "Save misalignment", "Save misalignment angle");
            NetworkManager.instance.RegisterFloat("degree", -45f, 45f, 1f, "Misalignment", "Angle between the eyes.");

            StartCoroutine(trackAndReportEyeMisalignment());
        }

        protected void unlockRightEye()
        {
            eye = StereoTargetEyeMask.Right;

            cameraRig.SetRightEyeRotationAndPosition();

            cameraRig.config.leftEyeIsStrabismic = false;
            cameraRig.config.rightEyeIsStrabismic = true;
            cameraRig.config.Sync();

            rightEyeIsStrabismic = true;

            // relock left eye
            cameraRig.StraightenLeftEye();
            cameraRig.SetLeftEyePositionOnly();
        }

        protected void unlockLeftEye()
        {
            eye = StereoTargetEyeMask.Left;

            cameraRig.SetLeftEyeRotationAndPosition();

            cameraRig.config.leftEyeIsStrabismic = true;
            cameraRig.config.rightEyeIsStrabismic = false;
            cameraRig.config.Sync();

            rightEyeIsStrabismic = false;

            // relock right eye
            cameraRig.StraightenRightEye();
            cameraRig.SetRightEyePositionOnly();
        }

        /// <summary>
        /// Reports the misalignment angle to the web interface.
        /// </summary>
        /// <returns>The and log misalignment angle.</returns>
        IEnumerator trackAndReportEyeMisalignment()
        {
            //Give the system time to settle before reporting. null references crash the coroutine.
            yield return new WaitForSeconds(2f);

            while (true)
            {
                if (eye != StereoTargetEyeMask.None)
                    NetworkManager.instance.SetFloat("degree", cameraRig.HorizontalEyeAngleHumanReadable(eye));

                yield return new WaitForSeconds(.5f);
            }
        }

        private void StoreAndQuit(){
            Vector3 euler = cameraRig.GetEyeAngle(eye);
            Debug.Log("Eye angle (euler) is " + euler + " degrees.");

            if (eye == StereoTargetEyeMask.Left)
                cameraRig.config.leftEyeMisalignmentAngle = euler;
            else
                cameraRig.config.rightEyeMisalignmentAngle = euler;

            //cameraRig.config.Sync();
            SceneManager.LoadScene("Menu");
        }

        public void Update()
        {

            if (!practitionerMode)
            {   //Now lets check to see if the headset if being held still
                float still = stillness.getTimeStill();
                Debug.Log("Time still " + still);
                if (still > secondsOfStillnessForSelect)
                {
                    //Selected!
                    indicator.Reset();
                    StoreAndQuit();
                }
                else
                {
                    //redraw the indicator and play a rising tone?!?
                    float percentage = (still / secondsOfStillnessForSelect);
                    Debug.Log("Percentage still " + percentage);
                    indicator.SetIndicatorPercentage(percentage);
                }
            }


            if (Input.GetButton("EyeSkills Confirm") || NetworkManager.instance.GetButton("save"))
            {
                StoreAndQuit();
            }
            else if (esInput.GetLongButtonPress("EyeSkills Left") || NetworkManager.instance.GetButton("unlockLeft"))
            {
                audioManager.Say("LeftStrabismic");
                unlockLeftEye();
            }
            else if (esInput.GetLongButtonPress("EyeSkills Right") || NetworkManager.instance.GetButton("unlockRight"))
            {
                audioManager.Say("RightStrabismic");
                unlockRightEye();
            }
        }
    }
}
