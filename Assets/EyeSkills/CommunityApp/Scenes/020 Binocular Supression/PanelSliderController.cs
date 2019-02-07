using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EyeSkills
{

    /***
     * This class name needs refactoring.
     * The horizontal tracking doesn't seem to work properly. WHY?!?!? DEBUG
     * We should be calculating a % or fraction of the starting position of the luminance panel. Think it through better.
     * 
     */

    //[ExecuteInEditMode]
    public class PanelSliderController : MonoBehaviour
    {

        public GameObject leftPanel, rightPanel;
        public EyeSkillsVRHeadsetInput headsetInput;
        public float panelMovementIncrement = 0.2f;
        public float maxExtension = 20f;
        //[Range(-1, 1)]
        private float direction = 0f;

        void OnDisable()
        {

        }

        void OnEnable()
        {

        }

       
        Vector3 updatePosition(GameObject go, float direction)
        {
            Vector3 pos = go.transform.localPosition;
            //An acceleration based approach.
            //float newX = Mathf.Clamp(pos.x + direction*panelMovementIncrement, -maxExtension,maxExtension);
            //An absolute position based approach.
            float newX = Mathf.Clamp(maxExtension * direction, -maxExtension, maxExtension);
            //Debug.Log("Old position " + go.transform.localPosition.x);
            pos = new Vector3(newX, pos.y, pos.z);
            //Debug.Log("New position " + pos.x);
            return pos;
        }

        void Update()
        {
            direction = headsetInput.getHorizontalDirection();
            //if looking right, bring them together, if left apart
            //Debug.Log("Direction " + direction);
            leftPanel.transform.localPosition = updatePosition(leftPanel, direction);
            rightPanel.transform.localPosition = updatePosition(rightPanel, -direction);

        }
    }
}
