using System;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Experimental.Rendering;
using Object = UnityEngine.Object;

namespace SthShader.SignedDistanceField
{
    public sealed class SignedDistanceFieldGenerator
    {
        public Texture2D ConvertToSdfTexture(Texture2D sourceBinaryTexture, int spreadCount)
        {
            return ConvertToSdfTextureWithCpu(sourceBinaryTexture, spreadCount);
        }

        private static Texture2D ConvertToSdfTextureWithCpu(Texture2D sourceBinaryTexture, int spreadCount)
        {
            if (sourceBinaryTexture == null || !sourceBinaryTexture.isReadable)
            {
                throw new ArgumentException($"{nameof(sourceBinaryTexture)} is null or not readable");
            }
            if (spreadCount is < 1 or > 127)
            {
                throw new ArgumentOutOfRangeException($"{nameof(spreadCount)} is out of range");
            }

            var width = sourceBinaryTexture.width;
            var height = sourceBinaryTexture.height;

            // NOTE: 入力テクスチャのフォーマットは不定のため GetPixelData ではなく GetPixels32 を使用して一意に変換する
            var sourcePixels = sourceBinaryTexture.GetPixels32(miplevel: 0);
            Assert.AreEqual(width * height, sourcePixels.Length);

            var isInsideArray = new bool[width * height];
            for (var idx = 0; idx < sourcePixels.Length; ++idx)
            {
                isInsideArray[idx] = IsInside(sourcePixels[idx]);
            }

            var sdfResolver = new SignedDistanceFieldCalculatorCpu();
            var distanceArray = sdfResolver.Resolve(width, height, isInsideArray);

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
                Object.DestroyImmediate(dstTexture);
                throw;
            }
        }

        private static bool IsInside(in Color32 value)
        {
            return value.r > 0;
        }
    }
}
