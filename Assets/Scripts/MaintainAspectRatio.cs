//using UnityEngine;

//[RequireComponent(typeof(Camera))]
//public class MaintainAspectRatio : MonoBehaviour
//{
//    private Camera cam;

//    void Start()
//    {
//        cam = GetComponent<Camera>();
//        SetAspectRatio();
//    }

//    void SetAspectRatio()
//    {
//        // The target aspect ratio (4:3)
//        float targetAspect = 4.0f / 3.0f;
//        float windowAspect = (float)Screen.width / (float)Screen.height;
//        float scaleHeight = windowAspect / targetAspect;

//        // If the current aspect ratio is wider than 4:3
//        if (scaleHeight < 1.0f)
//        {
//            Rect rect = cam.rect;

//            rect.width = 1.0f;
//            rect.height = scaleHeight;
//            rect.x = 0;
//            rect.y = (1.0f - scaleHeight) / 2.0f;

//            cam.rect = rect;
//        }
//        else // If the current aspect ratio is narrower than 4:3
//        {
//            float scaleWidth = 1.0f / scaleHeight;

//            Rect rect = cam.rect;

//            rect.width = scaleWidth;
//            rect.height = 1.0f;
//            rect.x = (1.0f - scaleWidth) / 2.0f;
//            rect.y = 0;

//            cam.rect = rect;
//        }
//    }

//    void OnPreCull()
//    {
//        // Make the background black to create the pillarbox effect
//        GL.Clear(true, true, Color.black);
//    }

//    void OnApplicationFocus(bool hasFocus)
//    {
//        // Reapply the aspect ratio when the application regains focus
//        if (hasFocus)
//        {
//            SetAspectRatio();
//        }
//    }

//    void OnRectTransformDimensionsChange()
//    {
//        // Reapply the aspect ratio when the screen dimensions change
//        SetAspectRatio();
//    }
//}

using UnityEngine;

[RequireComponent(typeof(Camera))]
public class AspectRatioController : MonoBehaviour
{
    public float targetAspect = 4.0f / 3.0f;

    void Start()
    {
        Camera camera = GetComponent<Camera>();

        // Calculate the target aspect ratio
        float windowAspect = (float)Screen.width / (float)Screen.height;
        float scaleHeight = windowAspect / targetAspect;

        // Adjust the camera viewport to fit the target aspect ratio
        if (scaleHeight < 1.0f)
        {
            Rect rect = camera.rect;

            rect.width = 1.0f;
            rect.height = scaleHeight;
            rect.x = 0;
            rect.y = (1.0f - scaleHeight) / 2.0f;

            camera.rect = rect;
        }
        else
        {
            float scaleWidth = 1.0f / scaleHeight;

            Rect rect = camera.rect;

            rect.width = scaleWidth;
            rect.height = 1.0f;
            rect.x = (1.0f - scaleWidth) / 2.0f;
            rect.y = 0;

            camera.rect = rect;
        }
    }
}
