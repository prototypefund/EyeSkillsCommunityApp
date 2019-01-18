/*
Copyright 2019 Dr. Thomas Benjamin Senior, Michael Zoeller 

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EyeSkills.Calibrations
{
    /// <summary>
    /// Ten tests with increasingly subtle depth indicators of a random circle.
    /// The Controller selects which is not flat.
    /// 
    /// </summary>
    public class FakeDepthController : MonoBehaviour
    {
        private EyeSkillsInput esInput;
        public List<GameObject> parts;
        public GameObject depthParts;
        private GameObject currentPart, chosenPart;
        private DepthSimulator depthSimulator;
        private IEnumerator coroutine;
        private bool userFeedback;


        /// <summary>
        /// The VR Headset controller for choosing a direction by moving the head
        /// </summary>
        public EyeSkillsVRHeadsetSelectByCross selectByCross;

        /// <summary>
        /// As coroutines get suspended/killed by disabling a gameobject, we need to preserve state for post-microcontroller activation.
        /// Perhaps we need to move the microcontroller logic back to disabling scripts.
        /// </summary>
        private int currentIndex;

        private bool practitionerMode = false;

        private float numberOfCorrectUserChoices = 0, choicesMade = 0;
        private AudioManager audioManager;

        float step = 0.05f;

        public int trials = 10;
        public float maxSeparation = 1f, minSeparation = 0.1f;

        public void Start()
        {
            esInput = EyeSkillsInput.instance;

            audioManager = AudioManager.instance;

            practitionerMode = (PlayerPrefs.GetString("EyeSkills.practitionerMode") == "1") ? true : false;
            //setCurrentPart(parts[0]);
        }

        IHorizontalFakeDepthControl setCurrentPart(GameObject go)
        {
            //currentPart = go as IHorizontalFakeDepthControl;
            currentPart = go;
            depthSimulator = currentPart.GetComponent<DepthSimulator>();
            return depthSimulator;
        }

        /// <summary>
        /// If a microcontroller disabled the gameController, it also stops coroutines, so we need to restart them.
        /// </summary>
        private void OnEnable()
        {
            Debug.Log("Restarting listener");
            StartCoroutine(OfferChoices());
        }

        /// <summary>
        /// Manual control of the depth perception elements. Useful for debugging or tighter control.
        /// </summary>
        void ManualControl()
        {
            // Choose which circle to move
            if (esInput.GetLongButtonPress("EyeSkills Up"))
            {
                setCurrentPart(parts[0]);
            }
            else if (esInput.GetLongButtonPress("EyeSkills Down"))
            {
                setCurrentPart(parts[1]);
            }
            else if (esInput.GetLongButtonPress("EyeSkills Right"))
            {
                setCurrentPart(parts[2]);
            }
            else if (esInput.GetLongButtonPress("EyeSkills Left"))
            {
                setCurrentPart(parts[3]);
            }
            else if (esInput.GetShortButtonPress("EyeSkills Up"))
            {
            }
            else if (esInput.GetShortButtonPress("EyeSkills Down"))
            {
                depthSimulator.resetHorizontalSeparation();
            }
            else if (esInput.GetShortButtonPress("EyeSkills Right")) //Now move that part
            {
                depthSimulator.stepIncreaseHorizontalSeparation(step);
            }
            else if (esInput.GetShortButtonPress("EyeSkills Left"))
            {
                depthSimulator.stepDecreaseHorizontalSeparation(step);
            }
        }

        private GameObject pickRandomPart()
        {
            return parts[(int) Mathf.RoundToInt(Random.Range(0, parts.Count - 1))];
        }

        private IEnumerator OfferChoices()
        {
            Debug.Log("Starting to offer choices");
            yield return 0;

            for (currentIndex = 1; currentIndex <= trials; currentIndex++)
            {
                // Visual/auditory feedback that we are trying another one (a similar U.I. display element to depth perception?)
                AudioManager.instance.Say("Next");

                // Pick one of the depth parts at random
                setCurrentPart(pickRandomPart());

                // Elevate it (reduce the distance) in steps 
                float separation = minSeparation + ((maxSeparation - minSeparation) / currentIndex);
                Debug.Log("Separation is " + separation + " for trial " + currentIndex + " where minSeparation is " + minSeparation + " and maxSeparation is " + maxSeparation);

                depthSimulator.setHorizontalSeparation(separation);

                depthParts.SetActive(true);
                /*
                Vector3 currentPosition = currentPart.transform.position;
                Vector3 previousPosition = new Vector3(currentPosition.x,currentPosition.y,currentPosition.z);
                Vector3 newPosition = new Vector3(currentPosition.x, currentPosition.y, currentPosition.z + separation);
                currentPart.transform.position = newPosition;
                */

                // Wait for user feedback
                userFeedback = false;
                chosenPart = null;

                //only start sensing again when we're ready with a new object.  
                selectByCross.ClearQueues(); 

                while (!userFeedback)
                {
                    if (EyeSkillsInput.instance.GetShortButtonPress("EyeSkills Up") || ((practitionerMode!=true) && selectByCross.isLookingUp()))
                    {
                        Debug.Log("Chose UP");
                        chosenPart = parts[0];
                    }
                    else if (EyeSkillsInput.instance.GetShortButtonPress("EyeSkills Down") || ((practitionerMode != true) && selectByCross.isLookingDown()))
                    {
                        Debug.Log("Chose DOWN");
                        chosenPart = parts[1];
                    }
                    else if (EyeSkillsInput.instance.GetShortButtonPress("EyeSkills Right") || ((practitionerMode != true) && selectByCross.isLookingRight()))
                    {
                        Debug.Log("Chose RIGHT");
                        chosenPart = parts[2];
                    }
                    else if (EyeSkillsInput.instance.GetShortButtonPress("EyeSkills Left") || ((practitionerMode != true) && selectByCross.isLookingLeft()))
                    {
                        Debug.Log("Chose LEFT");
                        chosenPart = parts[3];
                    }

                    //TODO: in the case of a headshake we need to explicitly recognise that they couldn't tell the answer

                    if (chosenPart != null)
                    {
                        Debug.Log("User made a selection");
                        userFeedback = true;
                        break;
                    }

                    yield return 0;
                }

                // Compare success
                choicesMade += 1.0f;
                if (chosenPart == currentPart)
                {
                    // Keep running total
                    numberOfCorrectUserChoices += 1.0f;
                    audioManager.Say("Correct");
                    Debug.Log("Correct Choice");
                }
                else
                {
                    Debug.Log("Incorrect Choice");
                    audioManager.Say("False");
                }

                // Reset everything
                depthSimulator.resetHorizontalSeparation();

                // Need to yield twice because the ESInput manager needs two passes.
                // Could I handle the input in the UpdateFunction where this wouldn't be necessary? - by stopping and starting the coroutine?
                depthParts.SetActive(false);

                yield return new WaitForSeconds(1);
                //yield return 0; 
            }

            audioManager.Say(numberOfCorrectUserChoices + "in10Correct");
            yield return new WaitForSeconds(1);

            // store the calibration and return to the menu
            FindObjectOfType<EyeSkillsCameraRig>().config.depthPerceptionAccuracy = numberOfCorrectUserChoices / choicesMade;
            SceneManager.LoadScene("Menu");
        }
    }
}