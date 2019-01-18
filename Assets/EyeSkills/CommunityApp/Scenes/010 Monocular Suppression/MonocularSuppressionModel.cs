/*
Copyright 2019 Dr. Thomas Benjamin Senior, Michael Zoeller 

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using UnityEngine;

namespace EyeSkills.Calibrations
{
    /// <summary>
    /// Monocular suppression controller.
    /// Short Up/Down alters luminosity.
    /// Long Up/Down alters distance from camera.
    /// Long Left/Right alters currently active eye.
    /// </summary>
    public class MonocularSuppressionModel : MonoBehaviour
    {
        private EyeSkillsInput esInput;

        public MultiFixationSprite rightOccluder, leftOccluder;
        private MultiFixationSprite currentOccluder;

        public MultiSprite PickRandomOccluder()
        {
            return currentOccluder.PickRandom();
        }

        public void SwitchToRightEye()
        {
            leftOccluder.Hide();
            rightOccluder.Show();
            currentOccluder = rightOccluder;
        }

        public void SwitchToLeftEye()
        {
            rightOccluder.Hide();
            leftOccluder.Show();
            currentOccluder = leftOccluder;
        }

        public MultiFixationSprite GetCurrentOccluder()
        {
            return currentOccluder;
        }

        public void LogState()
        {
            Debug.Log("Current fixation object is " + currentOccluder.GetCurrentKey() + " with luminosity " + currentOccluder.GetCurrentLuminosity() + " at distance " + currentOccluder.GetCurrentDistance());
        }
    }
}