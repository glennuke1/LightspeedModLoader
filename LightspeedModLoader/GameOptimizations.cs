using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LightspeedModLoader
{
    public class GameOptimizations : MonoBehaviour
    {
        GameObject satsuma;
        GameObject Player;

        public static bool optimizingSatsuma = true;

        #region satsuma

        GameObject chassis;
        GameObject miscParts;
        GameObject interior;
        GameObject body;

        List<PlayMakerFSM> assembleFSMS = new List<PlayMakerFSM>();
        List<PlayMakerFSM> removalFSMS = new List<PlayMakerFSM>();

        bool switchedBackOn = true;

        #endregion

        #region bus

        #endregion

        private void Start()
        {
            Player = Camera.main.gameObject;

            StartCoroutine(optimizeSatsuma());
        }

        IEnumerator optimizeSatsuma()
        {
            while (satsuma == null)
            {
                yield return null;
                satsuma = GameObject.Find("SATSUMA(557kg, 248)");

                chassis = satsuma.transform.Find("Chassis").gameObject;
                miscParts = satsuma.transform.Find("MiscParts").gameObject;
                interior = satsuma.transform.Find("Interior").gameObject;
                body = satsuma.transform.Find("Body").gameObject;

                foreach (PlayMakerFSM fsm in satsuma.GetComponentsInChildren<PlayMakerFSM>(true))
                {
                    if (fsm.FsmName.Contains("Assembly"))
                    {
                        assembleFSMS.Add(fsm);
                    }
                    if (fsm.FsmName.Contains("Removal"))
                    {
                        assembleFSMS.Add(fsm);
                    }
                }
            }

            while (optimizingSatsuma)
            {
                yield return new WaitForSeconds(3f);
                if (Vector3.Distance(satsuma.transform.position, Player.transform.position) > 100)
                {
                    switchedBackOn = false;
                    foreach (PlayMakerFSM fsm in assembleFSMS)
                    {
                        yield return null;
                        if (fsm.enabled)
                            fsm.enabled = false;
                    }

                    foreach (PlayMakerFSM fsm in removalFSMS)
                    {
                        yield return null;
                        if (fsm.enabled)
                            fsm.enabled = false;
                    }

                    /*chassis.SetActive(false);
                    miscParts.SetActive(false);
                    interior.SetActive(false);
                    body.SetActive(false);*/
                }
                else
                {
                    if (!switchedBackOn)
                    {
                        foreach (PlayMakerFSM fsm in assembleFSMS)
                        {
                            if (!fsm.enabled)
                                fsm.enabled = true;
                        }

                        foreach (PlayMakerFSM fsm in removalFSMS)
                        {
                            if (!fsm.enabled)
                                fsm.enabled = true;
                        }
                    }
                    switchedBackOn = true;

                    /*if (!chassis.activeSelf)
                    {
                        chassis.SetActive(true);
                        miscParts.SetActive(true);
                        interior.SetActive(true);
                        body.SetActive(true);
                    }*/
                }
            }
        }
    }
}