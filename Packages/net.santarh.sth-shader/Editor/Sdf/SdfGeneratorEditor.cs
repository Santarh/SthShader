using System;
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

        private Button _fixTextureImporterSettingButton;
        private Button _generateButton;

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

            _fixTextureImporterSettingButton = new Button(FixTextureImporterSetting) { text = "Fix Texture Importer Setting" };
            _generateButton = new Button(Generate) { text = "Generate" };
            textureField.RegisterValueChangedCallback(ev => UpdateUiEnabled());
            rootVisualElement.Add(_fixTextureImporterSettingButton);
            rootVisualElement.Add(_generateButton);
        }

        private void Update()
        {
            // NOTE: TextureImporter の設定が変更されたことを検知する手段が Update でのポーリングしか見当たらない
            UpdateUiEnabled();
        }

        private void UpdateUiEnabled()
        {
            var isValid = IsTextureValid(_texture);
            _fixTextureImporterSettingButton.SetEnabled(!isValid);
            _generateButton.SetEnabled(isValid);
        }

        private bool IsTextureValid(Texture2D texture)
        {
            if (AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(texture)) is TextureImporter textureImporter)
            {
                return textureImporter.isReadable &&
                       textureImporter.textureCompression == TextureImporterCompression.Uncompressed;
            }
            else
            {
                return false;
            }
        }

        private void FixTextureImporterSetting()
        {
            if (AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(_texture)) is TextureImporter textureImporter)
            {
                textureImporter.isReadable = true;
                textureImporter.textureCompression = TextureImporterCompression.Uncompressed;
                textureImporter.SaveAndReimport();
            }
        }

        private void Generate()
        {
            if (_texture == null)
            {
                throw new InvalidOperationException("Texture is not set");
            }

            var filePath = EditorUtility.SaveFilePanelInProject("Save SDF Texture", "SDFTexture", "png", "Save SDF Texture");
            if (string.IsNullOrEmpty(filePath))
            {
                Debug.LogWarning("Invalid file path");
                return;
            }

            var converter = new SdfConverter();
            var tex = converter.ConvertToSdfTexture(_texture, _spreadPixelCount);
            try
            {
                var bytes = tex.EncodeToPNG();
                File.WriteAllBytes(filePath, bytes);
                AssetDatabase.Refresh();
                var assetPath = Path.GetRelativePath(Directory.GetCurrentDirectory(), filePath);
                if (AssetImporter.GetAtPath(assetPath) is TextureImporter textureImporter)
                {
                    // NOTE: SDF テクスチャに記録された Code Value はリニアである
                    textureImporter.sRGBTexture = false;
                    // NOTE: SDF テクスチャは Block 圧縮に耐えられない
                    textureImporter.textureCompression = TextureImporterCompression.Uncompressed;
                    // NOTE: SDF テクスチャは何段階か小さくしておく
                    var sdfTextureWidth = Mathf.NextPowerOfTwo(Mathf.Max(tex.width, tex.height)) >> 2;
                    textureImporter.maxTextureSize = sdfTextureWidth;
                    textureImporter.SaveAndReimport();
                }
                else
                {
                    throw new System.InvalidOperationException("Failed to import the generated SDF texture");
                }
            }
            finally
            {
                DestroyImmediate(tex);
            }
        }
    }
}