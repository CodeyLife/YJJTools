using UnityEngine;

public class OceanInput : MonoBehaviour
{
    void Update()
    {

        // Get mouse position in screen coordinates
        Vector3 mousePos = Input.mousePosition;
        mousePos.y = Screen.height - mousePos.y;  // Flip Y coordinate
        var vec = new Vector4(-mousePos.x, mousePos.y, 0, 0);
        Debug.Log(vec);
        Shader.SetGlobalVector("_MousePos", vec);
    }
}