/*
Copyright 2019 Dr. Thomas Benjamin Senior, Michael Zoeller 

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EyeSkills.Calibrations;

namespace EyeSkills
{
    /// <summary>
    /// DDP Master controller. This might be where we, eventually, also include some expert system logic/workflow to even automate in scene processes
    /// We would have to rewrite the controllers to use onEnable and onDisable though, or add hooks to rerun start when they are called.
    /// </summary>
    public class DDPMasterController : MonoBehaviour
    {
        private FakeDepthController depthController;

        private void InitDepth()
        {
            gameObject.SetActive(true);
            depthController = gameObject.GetComponentInChildren<FakeDepthController>();
            depthController.enabled = true;
            depthController.Start();
        }

        void Start()
        {
            InitDepth();
        }
    }
}