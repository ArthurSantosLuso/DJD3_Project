using UnityEditor;
using UnityEngine;

public class ScreenshotHelper
{
    [MenuItem("Tools/Take 7K Screenshot")]
    public static void TakeScreenshot()
    {
        // Change these to your exact numbers if needed
        ScreenCapture.CaptureScreenshot("Prerender_7K.png", 1);
        Debug.Log("Screenshot saved to project root folder!");
    }
}