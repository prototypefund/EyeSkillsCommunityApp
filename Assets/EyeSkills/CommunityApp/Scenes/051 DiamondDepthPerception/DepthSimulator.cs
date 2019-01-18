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

namespace EyeSkills
{
    public class DepthSimulator : MonoBehaviour, IHorizontalFakeDepthControl
    {
        public GameObject part1, part2;
        private float horizontalSeparation, previousSeparation;
        Vector3 centerPoint;

        //float rate = 0.2f;
        //float step = 0.01f;

        void Start()
        {
            centerPoint = part1.transform.position;
        }

        void redrawPositions(float _horizontalSeparation)
        {
            Mathf.Clamp(_horizontalSeparation, 0, 40);

            if (!Mathf.Approximately(_horizontalSeparation, previousSeparation))
            {
                part1.transform.position = new Vector3(centerPoint.x + _horizontalSeparation, centerPoint.y, centerPoint.z);
                part2.transform.position = new Vector3(centerPoint.x - _horizontalSeparation, centerPoint.y, centerPoint.z);
                previousSeparation = _horizontalSeparation;
            }
        }

        public void resetHorizontalSeparation()
        {
            redrawPositions(0f);
        }

        public void setHorizontalSeparation(float separation)
        {
            redrawPositions(separation);
        }

        public void smoothlyIncreaseHorizontalSeparation(float rate)
        {
            horizontalSeparation += Time.deltaTime * rate;
            redrawPositions(horizontalSeparation);
        }

        public void smoothlyDecreaseHorizontalSeparation(float rate)
        {
            horizontalSeparation -= Time.deltaTime * rate;
            redrawPositions(horizontalSeparation);
        }

        public void stepIncreaseHorizontalSeparation(float step)
        {
            horizontalSeparation += step;
            redrawPositions(horizontalSeparation);
        }

        public void stepDecreaseHorizontalSeparation(float step)
        {
            horizontalSeparation -= step;
            redrawPositions(horizontalSeparation);
        }
    }
}