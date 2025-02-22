﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LightspeedModLoader
{
    public class GameOptimizations : MonoBehaviour
    {
        GameObject satsuma;
        GameObject hayosiko;

        GameObject Player;

        public static bool optimizing = false;

        public static bool SuckSatsumasExhaustPipeForFPS = true;
        public static bool optimizeHayosiko = true;
        public static bool optimizeGifu = true;
        public static bool optimizeJonnez = true;
        public static bool optimizeKekmet = true;
        public static bool optimizeFerndale = true;
        public static bool optimizeRuscko = true;
        public static bool optimizeFlatbed = true;

        #region satsumaShit
        List<Transform> kidsToDisable = new List<Transform>();
        bool deactivated = false;

        Transform doorLeft;
        Transform doorRight;
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

                foreach (Transform transform in satsuma.transform)
                {
                    if (transform.gameObject.activeSelf)
                    {
                        if (transform.gameObject.name != "Chassis" && transform.gameObject.name != "Colliders")
                        {
                            kidsToDisable.Add(transform);
                        }
                    }
                }

                if (satsuma.transform.Find("Body").Find("pivot_door_left").childCount > 0)
                {
                    doorLeft = satsuma.transform.Find("Body").Find("pivot_door_left").GetChild(0);
                }
                if (satsuma.transform.Find("Body").Find("pivot_door_right").childCount > 0)
                {
                    doorRight = satsuma.transform.Find("Body").Find("pivot_door_right").GetChild(0);
                }
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
                        if (!deactivated)
                        {
                            satsuma.GetComponent<Rigidbody>().isKinematic = true;

                            foreach (Transform transform in kidsToDisable)
                            {
                                transform.gameObject.SetActive(false);
                            }
                            deactivated = true;
                        }
                    }
                    else
                    {
                        if (deactivated)
                        {
                            foreach (Transform transform in kidsToDisable)
                            {
                                transform.gameObject.SetActive(true);
                            }

                            yield return new WaitForSeconds(1f);

                            if (doorLeft)
                            {
                                doorLeft.GetComponents<PlayMakerFSM>()[0].FsmVariables.GetFsmBool("Detach").Value = true;
                                yield return new WaitForSeconds(1f);
                                doorLeft.GetComponents<PlayMakerFSM>()[0].FsmVariables.GetFsmBool("Detach").Value = false;
                                Destroy(doorLeft.GetComponents<FixedJoint>()[0]);
                                Destroy(doorLeft.GetComponents<FixedJoint>()[1]);
                                yield return new WaitForSeconds(1f);
                                try
                                {
                                    satsuma.transform.Find("Body").Find("trigger_door_left").GetComponent<PlayMakerFSM>().SendEvent("ASSEMBLE");
                                    doorLeft.gameObject.tag = "";
                                }
                                catch { }
                                yield return new WaitForSeconds(1f);
                                doorLeft.GetComponents<PlayMakerFSM>()[1].FsmVariables.GetFsmFloat("Moment").Value = 32000f;
                                doorLeft.GetComponents<PlayMakerFSM>()[1].FsmVariables.GetFsmFloat("Tightness").Value = 32f;
                            }

                            if (doorRight)
                            {
                                doorRight.GetComponent<PlayMakerFSM>().FsmVariables.GetFsmBool("Detach").Value = true;
                                yield return new WaitForSeconds(1f);
                                doorRight.GetComponent<PlayMakerFSM>().FsmVariables.GetFsmBool("Detach").Value = false;
                                Destroy(doorRight.GetComponents<FixedJoint>()[0]);
                                Destroy(doorRight.GetComponents<FixedJoint>()[1]);
                                yield return new WaitForSeconds(1f);
                                try
                                {
                                    satsuma.transform.Find("Body").Find("trigger_door_right").GetComponent<PlayMakerFSM>().SendEvent("ASSEMBLE");
                                    doorRight.gameObject.tag = "";
                                }
                                catch { }
                                yield return new WaitForSeconds(1f);
                                doorRight.GetComponents<PlayMakerFSM>()[1].FsmVariables.GetFsmFloat("Moment").Value = 32000f;
                                doorRight.GetComponents<PlayMakerFSM>()[1].FsmVariables.GetFsmFloat("Tightness").Value = 32f;
                            }

                            yield return new WaitForSeconds(3f);

                            satsuma.GetComponent<Rigidbody>().isKinematic = false;

                            deactivated = false;
                        }
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