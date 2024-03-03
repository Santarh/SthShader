using System.IO;
using SthShader.Editor.UIToolkit;
using SthShader.Sdf;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SthShader.Editor.Sdf
{
    public sealed class SdfGeneratorEditor : EditorWindow
    {
        [SerializeField] private Texture2D _texture;
        [SerializeField] private int _spreadPixelCount = 127;

        public static void ShowWindow()
        {
            var window = GetWindow<SdfGeneratorEditor>();
            window.titleContent = new GUIContent("SDF Generator");
            window.Show();
        }

        public void CreateGUI()
        {
            var serializedObject = new SerializedObject(this);
            var textureProperty = serializedObject.FindProperty(nameof(_texture));
            var spreadPixelCountProperty = serializedObject.FindProperty(nameof(_spreadPixelCount));

            rootVisualElement.Add(new Label("Input Texture"));

            var textureField = new TextureObjectFieldWithPreview();
            textureField.BindProperty(textureProperty);
            rootVisualElement.Add(textureField);

            var spreadPixelCountField = new IntegerField(label: spreadPixelCountProperty.displayName);
            spreadPixelCountField.BindProperty(spreadPixelCountProperty);
            rootVisualElement.Add(spreadPixelCountField);

            rootVisualElement.Add(new Button(Generate) { text = "Generate" });
        }

        private void Generate()
        {
            var filePath = EditorUtility.SaveFilePanelInProject("Save SDF Texture", "SDFTexture", "png", "Save SDF Texture");
            if (string.IsNullOrEmpty(filePath))
            {
                Debug.LogWarning("Invalid file path");
                return;
            }

            var converter = new SdfConverter();
            var tex = converter.ConvertToSdfTexture(_texture, _spreadPixelCount);
            var bytes = tex.EncodeToPNG();
            File.WriteAllBytes(filePath, bytes);
            AssetDatabase.Refresh();
            var assetPath = Path.GetRelativePath(Directory.GetCurrentDirectory(), filePath);
            if (AssetImporter.GetAtPath(assetPath) is TextureImporter textureImporter)
            {
                // NOTE: SDF テクスチャに記録された Code Value はリニアである
                textureImporter.sRGBTexture = false;
            }
            else
            {
                throw new System.InvalidOperationException("Failed to import the generated SDF texture");
            }
        }
    }
}