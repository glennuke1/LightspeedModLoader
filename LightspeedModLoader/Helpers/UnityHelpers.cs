using UnityEngine;

namespace LightspeedModLoader
{
    public static class UnityHelpers
    {

        /// <summary>
        /// Returns a GameObject with the specified name. Make sure to cache this GameObject and not run this every frame!!!
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
