using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace LightspeedModLoader
{
    public static class MSCExtensions
    {
        public static void MakePickable(this GameObject gameObject)
        {
            gameObject.layer = LayerMask.NameToLayer("Parts");
            gameObject.tag = "PART";
        }
    }
}
