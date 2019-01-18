/*
Copyright 2019 Dr. Thomas Benjamin Senior, Michael Zoeller 

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EyeSkills
{
    public class AlternatorModel : MonoBehaviour
    {
        private float freq = 0f;

        public GameObject rightObj, leftObj;

        public void ShowRight(bool show)
        {
            rightObj.SetActive(show);
        }

        public void ShowLeft(bool show)
        {
            leftObj.SetActive(show);
        }

        public float AlterFrequency(float _freq)
        {
            if ((freq + _freq > -1f) && (freq + _freq <= 30f))
            {
                freq += _freq;
            }

            return freq;
        }

        public void StartSwitchingFixationObjects()
        {
            StartCoroutine(SwitchFixationObjects());
        }

        IEnumerator SwitchFixationObjects()
        {
            bool s = false;
            while (true)
            {
                if (freq > 0)
                {
                    ShowRight(s);
                    ShowLeft(!s);
                    s = !s;
                }

                float secondsToWait = (freq == 0) ? 0 : 1 / freq;
                yield return new WaitForSeconds(secondsToWait);
            }
        }
    }
}