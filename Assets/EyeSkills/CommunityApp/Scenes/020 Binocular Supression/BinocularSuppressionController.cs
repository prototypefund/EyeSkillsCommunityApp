/*
Copyright 2019 Dr. Thomas Benjamin Senior, Michael Zoeller 

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using UnityEngine;
using UnityEngine.SceneManagement;

namespace EyeSkills.Calibrations
{
    /// <summary>
    /// Binocular suppression controller.  This alters the relative brightness of the scene perceived by both eyes by asking the cameraRig to apply filters
    /// to the virtual cameras feeding each eye with input.
    /// </summary>
    [System.Serializable]
    public class BinocularSuppressionController : MonoBehaviour
    {
        public GameObject trackedCamera;
        public GameObject cameraRigObject;
        public ConflictZoneModel model;

        public bool ignoreStillnessSensor = false;

        private float brightnessRatio;
        private float previousBrightnessRatio = -2;
        private EyeSkillsVRHeadsetInput ratioController;
        private EyeSkillsInput esInput;
        private EyeSkillsCameraRig cameraRig;
        public float secondsOfStillnessForSelect = 10f;



        /// <summary>
        /// The selection indicator.
        /// </summary>
        public SelectionIndicatorScaler indicator;


        /// <summary>
        /// Our headset stillness detection controller
        /// </summary>
        public EyeSkillsVRHeadsetSelectByStillness stillness;

        void Start()
        {
            NetworkManager.instance.RegisterButton("inConflict", "Add Conflict", "Put the eyes into conflict");
            NetworkManager.instance.RegisterButton("outOfConflict", "Remove Conflict", "Remove conflict");
            NetworkManager.instance.RegisterButton("save", "Save ratio", "Save the luminance ratio between the eyes");
            NetworkManager.instance.RegisterFloat("brightnessRatio", -1f, 1f, 0.05f, "Luminance ratio", "Brightness ratio between the eyes.");
                                                             
            ignoreStillnessSensor = (PlayerPrefs.GetString("EyeSkills.practitionerMode") == "1") ? true : false;

            ratioController = new EyeSkillsVRHeadsetInput(trackedCamera);
            esInput = EyeSkillsInput.instance;
            cameraRig = cameraRigObject.GetComponent<EyeSkillsCameraRig>();
            brightnessRatio = 0;
            model.IntoConflict();
            AudioManager.instance.Say("inConflict");
            model.Show(true);
        }

        private void StoreSuppressionRatioAndQuit(){
            Debug.Log("Setting binocular suppression ratio to " + brightnessRatio);
            cameraRig.config.binocularSuppressionRatio = brightnessRatio;
            //cameraRig.config.Sync();
            SceneManager.LoadScene("Menu");
        }

        void Update()
        {
            // TODO: Turns out this has some issues. It's -1 at the upper extent, and 1 at the lower. It seems to also extend beyond 1/-1 which is shouldn't.
            // TODO : Really, we want this to be a non-linear control, it should be possible at the extremes to spend longer with more control
            brightnessRatio = Mathf.Clamp(ratioController.getDirection(), -1, 1);

            // Don't over communicate :-)
            if (Mathf.Abs(previousBrightnessRatio - brightnessRatio) > 0.01)
            {
                NetworkManager.instance.SetFloat("brightnessRatio", brightnessRatio);
                Debug.Log("Brightness Ratio " + brightnessRatio);
                previousBrightnessRatio = brightnessRatio;
            }

            cameraRig.SetBinocularSuppressionRatio(brightnessRatio);

            //Now lets check to see if the headset if being held still
            if (!ignoreStillnessSensor)
            {
                float still = stillness.getTimeStill();
                Debug.Log("Time still " + still);
                if (still > secondsOfStillnessForSelect)
                {
                    //Selected!
                    indicator.Reset();
                    StoreSuppressionRatioAndQuit();
                }
                else
                {
                    //redraw the indicator and play a rising tone?!?
                    float percentage = (still / secondsOfStillnessForSelect);
                    //Debug.Log("Percentage still " + percentage);
                    indicator.SetIndicatorPercentage(percentage);
                }
            }

            if (esInput.GetShortButtonPress("EyeSkills Confirm") || NetworkManager.instance.GetButton("save"))
            {
                StoreSuppressionRatioAndQuit();
            }
            else if (esInput.GetShortButtonPress("EyeSkills Up") || NetworkManager.instance.GetButton("inConflict"))
            {
                AudioManager.instance.Say("inConflict");
                model.IntoConflict();
            }
            else if (esInput.GetShortButtonPress("EyeSkills Down") || NetworkManager.instance.GetButton("outOfConflict"))
            {
                AudioManager.instance.Say("notInConflict");
                model.OutOfConflict();
            }
        }
    }
}
