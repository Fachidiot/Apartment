using GameAIfySDK;
using UnityEditor;

public class Alerts : EditorWindow
{
    public static bool FailDialog(string message)
    {
        return EditorUtility.DisplayDialog("Fail", message, "Ok");
    }
    public static bool SuccessDialog(string message)
    {
        return EditorUtility.DisplayDialog("Success!", message, "Ok");
    }
    public static bool CheckDialog(string message)
    {
        return EditorUtility.DisplayDialog("Warning", message, "Ok", "Cancel");
    }
}
