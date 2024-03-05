using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SthShader.SignedDistanceField;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace SthShader.ShadowThresholdMap
{
    public static class ShadowThresholdMapGenerator
    {
        public static Texture2D Generate(IReadOnlyList<Texture2D> textures)
        {
            foreach (var texture in textures)
            {
                if (texture == null)
                {
                    throw new ArgumentException("Texture is null");
                }
            }

            var width = textures[0].width;
            var height = textures[0].height;
            var pixelCount = width * height;
            foreach (var texture in textures)
            {
                if (texture.width != width || texture.height != height)
                {
                    throw new ArgumentException("Texture size is not match");
                }
            }

            var insideArrays = new bool[textures.Count][];
            var distanceArrays = new double[textures.Count][];
            for (var idx = 0; idx < textures.Count; ++idx)
            {
                insideArrays[idx] = SignedDistanceFieldGenerator.GenerateInsideArrayFromInputTexture(textures[idx], 0, 0, 0);
            }
            Parallel.ForEach(textures, (texture, _, index) =>
            {
                distanceArrays[index] = SignedDistanceFieldCalculatorCpu.Calculate(width, height, insideArrays[index]);
            });

            var pairCount = textures.Count - 1;
            var pairIndices = Enumerable.Range(0, pairCount).Select(x => (x, x + 1)).ToArray();
            var gradationArrays = new double[pairCount][];
            Parallel.ForEach(pairIndices, (pair, _, pairIndex) =>
            {
                var (indexA, indexB) = pair;
                var insideArrayA = insideArrays[indexA];
                var insideArrayB = insideArrays[indexB];
                var distanceArrayA = distanceArrays[indexA];
                var distanceArrayB = distanceArrays[indexB];

                var gradationArray = new double[pixelCount];
                for (var idx = 0; idx < pixelCount; idx++)
                {
                    if (insideArrayA[idx])
                    {
                        // NOTE: A が領域内の場合は 1
                        gradationArray[idx] = 1;
                    }
                    else if (!insideArrayB[idx])
                    {
                        // NOTE: A も B も領域外の場合は 0
                        gradationArray[idx] = 0;
                    }
                    else
                    {
                        // NOTE: A が領域外で B が領域内の場合は A と B の距離に応じた値
                        var distanceA = distanceArrayA[idx];
                        var distanceB = distanceArrayB[idx];
                        gradationArray[idx] = 1.0 - distanceA / (distanceA - distanceB);
                    }
                }
                gradationArrays[pairIndex] = gradationArray;
            });

            var thresholdArray = new double[pixelCount];
            for (var idx = 0; idx < pixelCount; ++idx)
            {
                for (var pairIndex = 0; pairIndex < pairCount; ++pairIndex)
                {
                    thresholdArray[idx] += gradationArrays[pairIndex][idx];
                }
                thresholdArray[idx] = Math.Clamp(thresholdArray[idx] / pairCount, 0, 1);
            }

            var dstTexture = new Texture2D(width, height, GraphicsFormat.R8G8B8A8_UNorm, 1, TextureCreationFlags.None);
            try
            {
                var dstPixels = dstTexture.GetPixelData<Color32>(mipLevel: 0);
                for (var idx = 0; idx < pixelCount; ++idx)
                {
                    var value = (byte)(thresholdArray[idx] * 255);
                    dstPixels[idx] = new Color32(value, value, value, 255);
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