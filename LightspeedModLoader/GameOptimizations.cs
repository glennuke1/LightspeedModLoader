using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LightspeedModLoader
{
    public class GameOptimizations : MonoBehaviour
    {
        GameObject satsuma;
        GameObject hayosiko;

        GameObject Player;

        public static bool optimizing = true;

        public static bool SuckSatsumasExhaustPipeForFPS = true;
        public static bool optimizeHayosiko = true;
        public static bool optimizeGifu = true;
        public static bool optimizeJonnez = true;
        public static bool optimizeKekmet = true;
        public static bool optimizeFerndale = true;
        public static bool optimizeRuscko = true;
        public static bool optimizeFlatbed = true;

        #region satsumaShit

        #endregion

        private void Start()
        {
            Player = Camera.main.gameObject;

            StartCoroutine(optimize());
        }

        IEnumerator optimize()
        {
            while (satsuma == null)
            {
                yield return null;
                satsuma = GameObject.Find("SATSUMA(557kg, 248)");
            }

            while (hayosiko == null)
            {
                yield return null;
                hayosiko = GameObject.Find("HAYOSIKO(1500kg, 250)");
            }

            while (optimizing)
            {
                yield return new WaitForSeconds(3f);
                if (SuckSatsumasExhaustPipeForFPS)
                {
                    if (Vector3.Distance(satsuma.transform.position, Player.transform.position) > 100)
                    {
                        foreach (Transform transform in satsuma.GetComponentsInChildren(typeof(Transform)))
                        {
                            if (!transform.gameObject.name.ToLower().Contains("chassis") && !transform.gameObject.name.ToLower().Contains("door"))
                            {

                            }
                        }
                    }
                    else
                    {
                        satsuma.SetActive(true);
                    }
                }

                if (optimizeHayosiko)
                {
                    if (Vector3.Distance(hayosiko.transform.position, Player.transform.position) > 100)
                    {
                        hayosiko.SetActive(false);
                    }
                    else
                    {
                        hayosiko.SetActive(true);
                    }
                }
            }
        }
    }
}