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
    /// <summary>
    /// Monocular suppression controller.
    /// Short Up/Down alters luminosity.
    /// Long Up/Down alters distance from camera.
    /// Long Left/Right alters currently active eye.
    /// </summary>
    public class MonocularSuppressionController : MonoBehaviour
    {
        /// <summary>
        /// The model. We may inject this on the basis of "Mode"
        /// </summary>
        public MonocularSuppressionModel model;

        private EyeSkillsInput esInput;
        private AudioManager audioManager;

        private float colorInc = 0.05f;
        private float distanceInc = 2f;

        /// <summary>
        /// The starting distance. Units are meters. We will convert this to a float, but we want to force the Editor U.I. to only offer integers.
        /// </summary>
        public int startingDistance = 15;

        public int initialBrightnessInPercent = 10;

        void Start()
        {
            // TODO : Specify a CSS sheet to use then give class names. Can use some default CSS or make custom layouts.
            NetworkManager.instance.RegisterScene("Monocular Suppression", "Can the participant see with both eyes?");
            NetworkManager.instance.RegisterButton("instructions", "Instructions", "Have the scene instructions read to you");
            NetworkManager.instance.RegisterButton("leftEye", "Left Eye", "Switch to the left eye");
            NetworkManager.instance.RegisterButton("rightEye", "Right Eye", "Switch to the right eye");
            NetworkManager.instance.RegisterButton("incBrightness", "Increase Brightness", "Increase object brightness");
            NetworkManager.instance.RegisterButton("decBrightness", "Decrease Brightness", "Decrease object brightness");
            NetworkManager.instance.RegisterButton("incDistance", "Increase Distance", "Increase object distance (make the object smaller)");
            NetworkManager.instance.RegisterButton("decDistance", "Decrease Distance", "Decrease object distance (make the object larger)");

            esInput = EyeSkillsInput.instance;

            Debug.Log("Start - fetch esInput " + esInput);
            audioManager = AudioManager.instance;

            //Setup our initial occluders
            model.leftOccluder.PickRandom();
            model.rightOccluder.PickRandom();

            //Now lets setup those occluders with their initial settings.
            model.leftOccluder.alterLuminance((float) initialBrightnessInPercent / 100, true);
            model.leftOccluder.AlterDistance((float) startingDistance, true);

            model.rightOccluder.alterLuminance((float) initialBrightnessInPercent / 100, true);
            model.rightOccluder.AlterDistance((float) startingDistance, true);

            model.SwitchToRightEye();
            sayString("rightEyeActive");
            model.PickRandomOccluder();
        }

        private void sayBrightness(float brightness)
        {
            string fileKey = (int) Mathf.Round(brightness * 100) + "PercentBright";
            sayString(fileKey);
        }

        private void sayDistance(float distance)
        {
            string fileKey = (int) Mathf.Round(distance) + "Distant";
            sayString(fileKey);
        }

        private void sayString(string text)
        {
            Debug.Log("Want to say " + text);
            audioManager.Say(text);
        }

        void Update()
        {
            if (esInput.GetShortButtonPress("EyeSkills Up") || NetworkManager.instance.GetButton("incBrightness"))
            {
                //Debug.Log("Increasing Luminance - short button press up");
                sayBrightness(model.GetCurrentOccluder().alterLuminance(+colorInc, false));
            }
            else if (esInput.GetShortButtonPress("EyeSkills Down") || NetworkManager.instance.GetButton("decBrightness"))
            {
                //Debug.Log("Decreasing Luminance - short button press down");
                sayBrightness(model.GetCurrentOccluder().alterLuminance(-colorInc, false));
            }
            else if (esInput.GetLongButtonPress("EyeSkills Up") || NetworkManager.instance.GetButton("incDistance"))
            {
                Debug.Log("Increasing Distance - long button press up");
                sayDistance(model.GetCurrentOccluder().AlterDistance(+distanceInc, false));
            }
            else if (esInput.GetLongButtonPress("EyeSkills Down") || NetworkManager.instance.GetButton("decDistance"))
            {
                Debug.Log("Decreasing Distance - long button press down");
                sayDistance(model.GetCurrentOccluder().AlterDistance(-distanceInc, false));
            }
            else if (esInput.GetLongButtonPress("EyeSkills Left") || NetworkManager.instance.GetButton("leftEye"))
            {
                model.SwitchToLeftEye();
                sayString("leftEyeActive");
                model.GetCurrentOccluder().PickRandom();
                model.LogState();
            }
            else if (esInput.GetLongButtonPress("EyeSkills Right") || NetworkManager.instance.GetButton("rightEye"))
            {
                model.SwitchToRightEye();
                sayString("rightEyeActive");
                model.PickRandomOccluder();
                model.LogState();
            }
            else if (NetworkManager.instance.GetButton("instructions"))
            {
                sayString("monocularSuppressionInstructions");
            }
        }
    }
}