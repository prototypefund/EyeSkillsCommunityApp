using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EyeSkills
{

    //[ExecuteInEditMode]
    public class PanelSliderController : MonoBehaviour
    {

        public GameObject leftPanel, rightPanel;
        public EyeSkillsVRHeadsetInput headsetInput;
        //public float panelMovementIncrement = 0.2f;
        public float maxExtension = 20f;
        //[Range(-1, 1)]
        private float direction = 0f;
       
        Vector3 updatePosition(GameObject go, float direction)
        {
            Vector3 pos = go.transform.localPosition;

            //An acceleration based approach.
            //float newX = Mathf.Clamp(pos.x + direction*panelMovementIncrement, -maxExtension,maxExtension);

            //An absolute position based approach.
            float newX = maxExtension * direction;

            pos = new Vector3(newX, pos.y, pos.z);

            return pos;
        }

        void Update()
        {
            direction = headsetInput.getHorizontalDirection();

            //bring them together/apart based on head direction

            leftPanel.transform.localPosition = updatePosition(leftPanel, direction);
            rightPanel.transform.localPosition = updatePosition(rightPanel, -direction);

        }
    }
}
