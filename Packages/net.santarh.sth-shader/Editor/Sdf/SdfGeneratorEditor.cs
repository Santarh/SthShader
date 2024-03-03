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

            rootVisualElement.Add(new Label("Input Texture"));

            var textureField = new TextureObjectFieldWithPreview();
            textureField.BindProperty(textureProperty);
            rootVisualElement.Add(textureField);

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
            var tex = converter.ConvertToSdfTexture(_texture, 128);
            var bytes = tex.EncodeToPNG();
            System.IO.File.WriteAllBytes(filePath, bytes);
            AssetDatabase.Refresh();
        }
    }
}