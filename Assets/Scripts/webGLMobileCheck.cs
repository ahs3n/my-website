using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

public class webGLMobileCheck : MonoBehaviour
{
    [DllImport("__Internal")]
    private static extern bool IsMobile();

    public bool isMobile()
    {
        Debug.Log("Starting WebGL platform check");
#if !UNITY_EDITOR && UNITY_WEBGL
             return IsMobile();
#endif

        return false;
    }
}
