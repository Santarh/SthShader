using UnityEngine.UIElements;

namespace SthShader.Editor.UIToolkit
{
    internal static class VisualElementExtensions
    {
        public static T WithClass<T>(this T element, string className) where T : VisualElement
        {
            element.AddToClassList(className);
            return element;
        }

        public static T WithStyleSheet<T>(this T element, StyleSheet styleSheet) where T : VisualElement
        {
            element.styleSheets.Add(styleSheet);
            return element;
        }

        public static T WithChild<T>(this T parent, VisualElement child) where T : VisualElement
        {
            parent.Add(child);
            return parent;
        }
    }
}