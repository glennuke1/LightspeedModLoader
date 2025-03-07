using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace LightspeedModLoader
{
    public static class UnityHelpers
    {

        /// <summary>
        /// Returns a GameObject with the specified name
        /// </summary>
        /// <param name="name"></param>
        /// <returns>The GameObject</returns>
        public static GameObject FindGameObject(string name)
        {
            foreach (GameObject go in Resources.FindObjectsOfTypeAll<GameObject>())
            {
                if (go.name == name)
                {
                    return go;
                }
            }

            return null;
        }
    }
}
