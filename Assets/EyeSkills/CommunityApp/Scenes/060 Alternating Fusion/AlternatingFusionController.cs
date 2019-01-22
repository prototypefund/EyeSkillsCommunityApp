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
    /// Alternating fusion controller.  
    /// Short left/right alters rate at which the objects switch eye.
    /// Short up/down places them in conflict or brings them out of conflict (vertically)
    /// 
    /// FUTURE:
    /// Long up/down takes misalignment into account (useful for non alternatingStrabismics)
    /// Long left/right might then apply/remove gradually migrating the strabismic eye to the center
    /// </summary>
    public class AlternatingFusionController : MonoBehaviour
    {
        public ConflictZoneModel conflictBackground;
        public AlternatorModel alternator;

        private AudioManager audioManager;

        private float freqInc = 1f;

        void Start()
        {
            NetworkManager.instance.RegisterScene("Alternating Fusion", "Can the participant switch eyes fast enough to start fusing?");
            NetworkManager.instance.RegisterButton("instructions", "Instructions", "Have the scene instructions read to you");
            NetworkManager.instance.RegisterButton("incFrequency", "Increase Frequency", "Increase object frequency");
            NetworkManager.instance.RegisterButton("decFrequency", "Decrease Frequency", "Decrease object frequency");
            NetworkManager.instance.RegisterButton("conflict", "In conflict", "Place the objects in conflict (occupying the same perceived space))");
            NetworkManager.instance.RegisterButton("noConflict", "No conflict", "Remove conflict (objects occupy different percieved spaces)");

            alternator.ShowRight(true);
            alternator.ShowLeft(true);
            conflict(false);

            audioManager = AudioManager.instance;
            sayString("ready");
            alternator.StartSwitchingFixationObjects();
        }

        private void sayString(string text)
        {
            Debug.Log("Want to say " + text);
            AudioManager.instance.Say(text);
        }

        private void sayFrequency(float freq)
        {
            //string fileKey = (1/freq).ToString().Substring(0,3) + "Hertz";
            string f = freq.ToString();
            if (f.Length > 4)
            {
                f = f.Substring(0, 4);
            }

            string fileKey = f + "Hertz";
            Debug.Log("Looking for file key " + fileKey);
            sayString(fileKey);
        }

        bool conflict(bool inConflict)
        {
            if (inConflict)
            {
                conflictBackground.Show(true);
            }
            else
            {
                conflictBackground.Show(false);
            }

            Debug.Log("Conflict Status " + inConflict);
            return inConflict;
        }

        void sayConflict(bool inConflict)
        {
            if (inConflict)
            {
                sayString("inConflict");
            }
            else
            {
                sayString("notInConflict");
            }
        }

        void Update()
        {
            if (EyeSkillsInput.instance.GetShortButtonPress("EyeSkills Left") || (NetworkManager.instance.GetButton("decFrequency")))
            {
                Debug.Log("Decreasing Freq - short button press left");
                sayFrequency((int) alternator.AlterFrequency(-freqInc));
            }
            else if (EyeSkillsInput.instance.GetShortButtonPress("EyeSkills Right") || (NetworkManager.instance.GetButton("incFrequency")))
            {
                Debug.Log("Increasing Freq - short button press right");
                sayFrequency((int) alternator.AlterFrequency(+freqInc));
            }
            else if (EyeSkillsInput.instance.GetShortButtonPress("EyeSkills Up") || (NetworkManager.instance.GetButton("conflict")))
            {
                Debug.Log("Conflict");
                sayConflict(conflict(true));
            }
            else if (NetworkManager.instance.GetButton("instructions"))
            {
                sayString("alternatingFusionInstructions");
            }
            else if (EyeSkillsInput.instance.GetShortButtonPress("EyeSkills Down") || (NetworkManager.instance.GetButton("noConflict")))
            {
                Debug.Log("Nonconflict");
                sayConflict(conflict(false));
            }
        }
    }
}