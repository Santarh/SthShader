using System;
using System.IO;
using SthShader.Editor.UIToolkit;
using SthShader.SignedDistanceField;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SthShader.Editor.SignedDistanceField
{
    public sealed class SignedDistanceFieldGeneratorEditor : EditorWindow
    {
        [SerializeField] private Texture2D _texture;
        [SerializeField] private float _thresholdRed = 0f;
        [SerializeField] private float _thresholdGreen = 0f;
        [SerializeField] private float _thresholdBlue = 0f;
        [SerializeField] private bool _advancedSettingsFoldoutValue = false;

        private Button _fixTextureImporterSettingButton;
        private Button _generateButton;

        public static void ShowWindow()
        {
            var window = GetWindow<SignedDistanceFieldGeneratorEditor>();
            window.titleContent = new GUIContent("SDF Generator");
            var pos = window.position;
            pos.width = 420;
            pos.height = 420;
            window.position = pos;
            window.Show();
        }

        public void CreateGUI()
        {
            rootVisualElement.Add(Uxml.GenerateTree());

            var serializedObject = new SerializedObject(this);
            var textureProperty = serializedObject.FindProperty(nameof(_texture));
            var thresholdRedProperty = serializedObject.FindProperty(nameof(_thresholdRed));
            var thresholdGreenProperty = serializedObject.FindProperty(nameof(_thresholdGreen));
            var thresholdBlueProperty = serializedObject.FindProperty(nameof(_thresholdBlue));
            var advancedSettingsFoldoutProperty = serializedObject.FindProperty(nameof(_advancedSettingsFoldoutValue));

            _fixTextureImporterSettingButton = rootVisualElement.Q<Button>("sth-sdf-fix-texture-button");
            _fixTextureImporterSettingButton.clickable = new Clickable(FixTextureImporterSetting);
            _generateButton = rootVisualElement.Q<Button>("sth-sdf-generate-button");
            _generateButton.clickable = new Clickable(Generate);
            var textureField = rootVisualElement.Q<TextureObjectFieldWithPreview>("sth-sdf-input-texture");
            textureField.BindProperty(textureProperty);
            textureField.RegisterValueChangedCallback(ev => UpdateUiEnabled());
            rootVisualElement.Q<BindableElement>("sth-sdf-advanced-settings-foldout").BindProperty(advancedSettingsFoldoutProperty);
            rootVisualElement.Q<BindableElement>("sth-sdf-mask-threshold-red").BindProperty(thresholdRedProperty);
            rootVisualElement.Q<BindableElement>("sth-sdf-mask-threshold-green").BindProperty(thresholdGreenProperty);
            rootVisualElement.Q<BindableElement>("sth-sdf-mask-threshold-blue").BindProperty(thresholdBlueProperty);
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

            var tex = SignedDistanceFieldGenerator.Generate(_texture);
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

        private static class Uxml
        {
            public static VisualElement GenerateTree() => new VisualElement()
                {
                    name = "sth-sdf-root",
                }
                .WithStyleSheet(Resources.Load<StyleSheet>("SthShader/SthFieldStyle"))
                .WithStyleSheet(Resources.Load<StyleSheet>("SthShader/SignedDistanceFieldGeneratorEditorStyle"))
                .WithChild(new ScrollView(ScrollViewMode.VerticalAndHorizontal)
                    {
                        name = "sth-sdf-settings",
                    }
                    .WithChild(new TextureObjectFieldWithPreview("Input Texture")
                        {
                            name = "sth-sdf-input-texture",
                        }
                    )
                    .WithChild(new Button()
                        {
                            name = "sth-sdf-fix-texture-button",
                            text = "Fix Texture Importer Setting",
                        }
                    )
                    .WithChild(new Foldout()
                        {
                            name = "sth-sdf-advanced-settings-foldout",
                            text = "Advanced Settings",
                        }
                        .WithChild(new FloatFieldWithSlider("Inner Mask Threshold [Red]", 0, 1)
                            {
                                name = "sth-sdf-mask-threshold-red",
                            }
                        )
                        .WithChild(new FloatFieldWithSlider("Inner Mask Threshold [Green]", 0, 1)
                            {
                                name = "sth-sdf-mask-threshold-green",
                            }
                        )
                        .WithChild(new FloatFieldWithSlider("Inner Mask Threshold [Blue]", 0, 1)
                            {
                                name = "sth-sdf-mask-threshold-blue",
                            }
                        )
                    )
                )
                .WithChild(new VisualElement()
                    {
                        name = "sth-sdf-spacer"
                    }
                )
                .WithChild(new Button()
                    {
                        name = "sth-sdf-generate-button",
                        text = "Generate",
                    }
                );
        }
    }
}