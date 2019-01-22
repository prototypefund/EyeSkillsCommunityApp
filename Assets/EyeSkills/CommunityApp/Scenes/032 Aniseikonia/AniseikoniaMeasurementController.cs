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
    // http://www.opticaldiagnostics.com/info/aniseikonia.html

    /// <summary>
    /// Aniseikonia measurement controller. 
    /// </summary>
    public class AniseikoniaMeasurementController : MonoBehaviour
    {
        EyeSkillsCameraRig cameraRig;
        public BiocularObservables observables;

        protected StereoTargetEyeMask eye;
        protected EyeSkillsInput esInput;
        protected AudioManager audioManager;

        public float distanceInc = 0.2f;
        public int startingDistance = 2;

        void Start()
        {
            cameraRig = FindObjectOfType<EyeSkillsCameraRig>();

            audioManager = AudioManager.instance;
            NetworkManager.instance.RegisterScene("Detect Aniseikonia", "What scaling and position are needed to fuse?");
            NetworkManager.instance.RegisterButton("unlockRight", "Unlock right eye", "Unlock the right eye");
            NetworkManager.instance.RegisterButton("unlockLeft", "Unlock left eye", "Unlock the left eye");
            NetworkManager.instance.RegisterButton("lock", "Lock both", "Lock both eyes");
            NetworkManager.instance.RegisterButton("change", "Change observables", "Change Fixation Object");
            NetworkManager.instance.RegisterButton("enlarge", "Enlarge observable", "Enlarge the fixation object");
            NetworkManager.instance.RegisterButton("shrink", "Shrink observable", "Shrink the fixation object");
            NetworkManager.instance.RegisterButton("save", "Save misalignment", "Save misalignment angle");
            NetworkManager.instance.RegisterFloat("degree", -45f, 45f, 1f, "Misalignment", "Angle between the eyes.");

            //audioManager.Say("ChooseStrabismicEye");

            //if (EyeSkills.UserCalibrationManager.instance.userCalibration.leftEyeIsStrabismic)
            //    lockRightEye();
            //else if (EyeSkills.UserCalibrationManager.instance.userCalibration.rightEyeIsStrabismic)
            //lockLeftEye();

            esInput = EyeSkillsInput.instance;

            observables.AlterDistanceLeft((float) startingDistance, true);
            observables.AlterDistanceRight((float) startingDistance, true);

            StartCoroutine(followMisalignmentAngle());
        }


        private void unlockRightEye()
        {
            eye = StereoTargetEyeMask.Right;

            observables.ApplyCommandsToRightSide();

            cameraRig.SetRightEyeRotationAndPosition();
            cameraRig.StraightenLeftEye();
            cameraRig.SetLeftEyePositionOnly();
        }

        private void unlockLeftEye()
        {
            eye = StereoTargetEyeMask.Left;

            observables.ApplyCommandsToLeftSide();

            cameraRig.SetLeftEyeRotationAndPosition();
            cameraRig.StraightenRightEye();
            cameraRig.SetRightEyePositionOnly();
        }

        private void sayString(string text)
        {
            //Debug.Log("Want to say " + text);
            AudioManager.instance.Say(text);
        }

        private float sayDistance(float distance)
        {
            string fileKey = (int) Mathf.Round(distance) + "Distant";
            sayString(fileKey);
            return distance;
        }

        IEnumerator followMisalignmentAngle()
        {
            yield return new WaitForSeconds(2f); //Wait for system to settle.
            //Something at startup preventing initial view - so we hack it in here.
            observables.PickAlternateImage();
            while (true)
            {
                Vector3 euler = cameraRig.GetEyeAngle(eye);

                float degree = 0;
                if (euler.y >= 180)
                {
                    degree = euler.y - 360;
                }
                else
                {
                    degree = euler.y;
                }

                NetworkManager.instance.SetFloat("degree", degree);
                yield return new WaitForSeconds(0.5f);
            }
        }

        public void Update()
        {
            if (esInput.GetShortButtonPress("EyeSkills Confirm") || NetworkManager.instance.GetButton("save"))
            {
                Vector3 euler = cameraRig.GetEyeAngle(eye);

                if (eye == StereoTargetEyeMask.Left)
                    cameraRig.config.leftEyeMisalignmentAngle = euler;
                else
                    cameraRig.config.rightEyeMisalignmentAngle = euler;

                SceneManager.LoadScene("Menu");
            }
            else if (esInput.GetLongButtonPress("EyeSkills Left") || NetworkManager.instance.GetButton("unlockLeft"))
            {
                //audioManager.Say("LeftStrabismic");
                unlockLeftEye();
            }
            else if (esInput.GetLongButtonPress("EyeSkills Right") || NetworkManager.instance.GetButton("unlockRight"))
            {
                //audioManager.Say("RightStrabismic");
                unlockRightEye();
            }
            else if (esInput.GetShortButtonPress("EyeSkills Left") || NetworkManager.instance.GetButton("change"))
            {
                //audioManager.Say("LeftStrabismic");
                observables.PickAlternateImage();
            }
            else if (esInput.GetShortButtonPress("EyeSkills Right") || NetworkManager.instance.GetButton("change"))
            {
                //audioManager.Say("RightStrabismic");
                observables.PickAlternateImage();
            }
            else if (esInput.GetShortButtonPress("EyeSkills Up") || NetworkManager.instance.GetButton("shrink"))
            {
                Debug.Log("Distance is " + sayDistance(observables.AlterDistance(+distanceInc, false)));
            }
            else if (esInput.GetShortButtonPress("EyeSkills Down") || NetworkManager.instance.GetButton("enlarge"))
            {
                Debug.Log("Distance is " + sayDistance(observables.AlterDistance(-distanceInc, false)));
            }
            else if (NetworkManager.instance.GetButton("lock"))
            {
                cameraRig.SetLeftEyePositionOnly();
                cameraRig.SetRightEyePositionOnly();
                Debug.Log("Locking both eyes");
            }
        }
    }
}