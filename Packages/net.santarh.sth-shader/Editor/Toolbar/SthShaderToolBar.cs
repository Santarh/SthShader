using SthShader.Editor.Sdf;
using UnityEditor;

namespace SthShader.Editor.Toolbar
{
    public static class SthShaderToolBar
    {
        public const string RootName = "Sth Shader";

        [MenuItem(RootName + "/SDF Generator")]
        public static void ShowSdfGeneratorWindow() => SdfGeneratorEditor.ShowWindow();
    }
}