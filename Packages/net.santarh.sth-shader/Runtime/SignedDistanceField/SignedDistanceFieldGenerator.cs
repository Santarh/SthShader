using System;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Experimental.Rendering;

namespace SthShader.SignedDistanceField
{
    public static class SignedDistanceFieldGenerator
    {
        public static Texture2D Generate(Texture2D sourceBinaryTexture, int spreadCount = 127, float thresholdRed = 0f, float thresholdGreen = 0f, float thresholdBlue = 0f)
        {
            var width = sourceBinaryTexture.width;
            var height = sourceBinaryTexture.height;

            var isInsideArray = GenerateInsideArrayFromInputTexture(sourceBinaryTexture, thresholdRed, thresholdGreen, thresholdBlue);
            var distanceArray = SignedDistanceFieldCalculatorCpu.Calculate(width, height, isInsideArray);
            return GenerateOutputTextureFromDistanceArray(width, height, distanceArray, spreadCount);
        }

        internal static bool[] GenerateInsideArrayFromInputTexture(Texture2D texture, float thresholdRed, float thresholdGreen, float thresholdBlue)
        {
            if (texture == null || !texture.isReadable)
            {
                throw new ArgumentException($"{nameof(texture)} is null or not readable");
            }

            var width = texture.width;
            var height = texture.height;

            var thresholdRedByte = (byte)(thresholdRed * 255);
            var thresholdGreenByte = (byte)(thresholdGreen * 255);
            var thresholdBlueByte = (byte)(thresholdBlue * 255);

            // NOTE: 入力テクスチャのフォーマットは不定のため GetPixelData ではなく GetPixels32 を使用して一意に変換する
            var sourcePixels = texture.GetPixels32(miplevel: 0);
            Assert.AreEqual(width * height, sourcePixels.Length);

            var isInsideArray = new bool[width * height];
            for (var idx = 0; idx < sourcePixels.Length; ++idx)
            {
                isInsideArray[idx] = sourcePixels[idx].r > thresholdRedByte ||
                                     sourcePixels[idx].g > thresholdGreenByte ||
                                     sourcePixels[idx].b > thresholdBlueByte;
            }

            return isInsideArray;
        }

        internal static Texture2D GenerateOutputTextureFromDistanceArray(int width, int height, double[] distanceArray, int spreadCount)
        {
            if (spreadCount is < 1 or > 127)
            {
                throw new ArgumentOutOfRangeException($"{nameof(spreadCount)} is out of range");
            }

            var dstTexture = new Texture2D(width, height, GraphicsFormat.R8G8B8A8_UNorm, 1, TextureCreationFlags.None);
            try
            {
                Assert.AreEqual(1, dstTexture.mipmapCount);

                var dstPixels = dstTexture.GetPixelData<Color32>(mipLevel: 0);
                Assert.AreEqual(width * height, dstPixels.Length);

                for (var y = 0; y < height; ++y)
                {
                    for (var x = 0; x < width; ++x)
                    {
                        var idx = y * width + x;
                        var distance = distanceArray[idx];

                        // NOTE: 距離を -1 から 1 に正規化する
                        var normalizedDistance = Math.Clamp(distance / spreadCount, -1.0, 1.0);
                        var normalizedValue = Math.Clamp(normalizedDistance * 0.5 + 0.5, 0, 1);
                        var value = (byte)(normalizedValue * 255);
                        dstPixels[idx] = new Color32(value, value, value, 255);
                    }
                }
                dstTexture.SetPixelData(dstPixels, mipLevel: 0);
                dstTexture.Apply(updateMipmaps: false);

                return dstTexture;
            }
            catch (Exception)
            {
                UnityEngine.Object.DestroyImmediate(dstTexture);
                throw;
            }
        }
    }
}
