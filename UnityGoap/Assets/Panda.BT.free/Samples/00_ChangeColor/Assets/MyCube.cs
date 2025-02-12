﻿using UnityEngine;

namespace Panda.Examples.ChangeColor
{
    public class MyCube : MonoBehaviour
    {
        /*
         * Set the color to the specified rgb value and succeed.
         */
        [Task] // <-- Attribute used to tag a class member as a task implementation.
        void SetColor(float r, float g, float b)
        {
            this.GetComponent<Renderer>().material.color = new Color(r, g, b);
            ThisTask.Succeed(); // <-- ThisTask gives access to the run-time task bind to this method.
        }
    }
}
