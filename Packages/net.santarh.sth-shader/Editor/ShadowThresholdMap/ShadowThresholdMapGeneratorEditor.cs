using System.Collections.Generic;
using System.IO;
using SthShader.Editor.UIToolkit;
using SthShader.ShadowThresholdMap;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SthShader.Editor.ShadowThresholdMap
{
    public sealed class ShadowThresholdMapGeneratorEditor : EditorWindow
    {
        [SerializeField] private List<Texture2D> _textures;

        public static void ShowWindow()
        {
            var window = GetWindow<ShadowThresholdMapGeneratorEditor>();
            window.titleContent = new GUIContent("Shadow Threshold Map Generator");
            window.Show();
        }

        public void CreateGUI()
        {
            var serializedObject = new SerializedObject(this);
            var textureListProperty = serializedObject.FindProperty(nameof(_textures));

            var scrollView = new ScrollView(ScrollViewMode.Vertical);
            rootVisualElement.Add(scrollView);

            var listView = new ListView
            {
                headerTitle = textureListProperty.displayName,
                focusable = true,
                virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight,
                reorderable = true,
                reorderMode = ListViewReorderMode.Animated,
                showBorder = true,
                showAddRemoveFooter = true,
                showFoldoutHeader = true,
                makeItem = () => new TextureObjectFieldWithPreview(),
            };
            listView.BindProperty(textureListProperty);
            scrollView.Add(listView);

            rootVisualElement.Add(new Button(Generate)
            {
                text = "Generate",
            });
        }

        private void Generate()
        {
            var filePath = EditorUtility.SaveFilePanelInProject("Save Shadow Threshold Map", "ShadowThresholdMap", "png", "Save Shadow Threshold Map");
            if (string.IsNullOrEmpty(filePath)) return;

            var tex = ShadowThresholdMapGenerator.Generate(_textures);
            try
            {
                var bytes = tex.EncodeToPNG();
                File.WriteAllBytes(filePath, bytes);
                AssetDatabase.Refresh();
                var assetPath = Path.GetRelativePath(Directory.GetCurrentDirectory(), filePath);
                if (AssetImporter.GetAtPath(assetPath) is TextureImporter textureImporter)
                {
                    // NOTE: テクスチャに記録された Code Value はリニアである
                    textureImporter.sRGBTexture = false;
                    // NOTE: テクスチャは Block 圧縮に耐えられない
                    textureImporter.textureCompression = TextureImporterCompression.Uncompressed;
                    // NOTE: テクスチャは何段階か小さくしておく
                    var sdfTextureWidth = Mathf.NextPowerOfTwo(Mathf.Max(tex.width, tex.height)) >> 2;
                    textureImporter.maxTextureSize = sdfTextureWidth;
                    textureImporter.SaveAndReimport();
                }
                else
                {
                    throw new System.InvalidOperationException("Failed to import the generated texture");
                }
            }
            finally
            {
                DestroyImmediate(tex);
            }
        }
    }
}
