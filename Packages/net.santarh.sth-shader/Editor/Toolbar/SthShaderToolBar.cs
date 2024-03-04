using SthShader.Editor.ShadowThresholdMap;
using SthShader.Editor.SignedDistanceField;
using UnityEditor;

namespace SthShader.Editor.Toolbar
{
    public static class SthShaderToolBar
    {
        public const string RootName = "Sth Shader";

        [MenuItem(RootName + "/SDF Generator")]
        public static void ShowSignedDistanceFieldGeneratorEditorWindow() => SignedDistanceFieldGeneratorEditor.ShowWindow();

        [MenuItem(RootName + "/Shadow Threshold Map Generator")]
        public static void ShowShadowThresholdMapGeneratorWindow() => ShadowThresholdMapGeneratorEditor.ShowWindow();
    }
}