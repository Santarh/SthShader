using System;
using System.Collections.Generic;
using SthShader.Collections;

namespace SthShader.SignedDistanceField
{
    internal static class SignedDistanceFieldCalculatorCpu
    {
        public static double[] Calculate(int width, int height, bool[] isInsideArray)
        {
            if (isInsideArray.Length != width * height)
            {
                throw new ArgumentException($"{nameof(isInsideArray)} length is invalid");
            }

            var outsideDoubleDistanceArray = CalculateOutsideDoubleDistance(width, height, isInsideArray);
            var isOutsideArray = new bool[width * height];
            for (var idx = 0; idx < isInsideArray.Length; ++idx)
            {
                isOutsideArray[idx] = !isInsideArray[idx];
            }
            var insideDoubleDistanceArray = CalculateOutsideDoubleDistance(width, height, isOutsideArray);

            var distanceArray = new double[isInsideArray.Length];
            for (var idx = 0; idx < isInsideArray.Length; ++idx)
            {
                if (isInsideArray[idx])
                {
                    var (doubleX, doubleY) = insideDoubleDistanceArray[idx];
                    distanceArray[idx] = Math.Sqrt(doubleX * doubleX + doubleY * doubleY) / 2;
                }
                else
                {
                    var (doubleX, doubleY) = outsideDoubleDistanceArray[idx];
                    distanceArray[idx] = -Math.Sqrt(doubleX * doubleX + doubleY * doubleY) / 2;
                }
            }

            return distanceArray;
        }

        /// <summary>
        /// 与えられた領域内ピクセルの情報から、領域外ピクセルの、領域までの二倍の距離を計算して返す。
        /// 返却する距離は、半ピクセルを 1 とする距離で、X 軸 Y 軸それぞれの距離を含む。
        /// </summary>
        private static (int, int)[] CalculateOutsideDoubleDistance(int width, int height, ReadOnlySpan<bool> isInsideArray)
        {
            var resolvedArray = new bool[width * height];
            // NOTE: 半ピクセルの距離を 1 とする値が入る
            var doubleDistanceArray = new (int, int)[width * height];
            var queue = new PriorityQueue<Element>(width * height, new Element.Comparer());

            // STEP1: 領域内ピクセルを列挙し、それに8近傍で隣接する領域外ピクセルをキューに詰める
            for (var y = 0; y < height; ++y)
            {
                for (var x = 0; x < width; ++x)
                {
                    var idx = y * width + x;
                    if (!isInsideArray[idx])
                    {
                        continue;
                    }

                    for (var dy = -1; dy <= 1; ++dy)
                    {
                        for (var dx = -1; dx <= 1; ++dx)
                        {
                            var neighborX = x + dx;
                            var neighborY = y + dy;
                            if (neighborX < 0 || neighborX >= width || neighborY < 0 || neighborY >= height)
                            {
                                continue;
                            }

                            var neighborIdx = neighborY * width + neighborX;
                            if (isInsideArray[neighborIdx])
                            {
                                continue;
                            }

                            queue.Enqueue(new Element(
                                neighborX,
                                neighborY,
                                dx,
                                dy,
                                Math.Sqrt(dx * dx + dy * dy)
                            ));
                        }
                    }
                }
            }

            // STEP2: キューが空になるまで、キューから取り出して処理する
            while (queue.TryDequeue(out var current))
            {
                var x = current.X;
                var y = current.Y;
                var idx = y * width + x;

                // STEP2.1: 既に解決済みの場合はスキップ
                if (resolvedArray[idx])
                {
                    continue;
                }

                // STEP2.2: 解決済みにする
                resolvedArray[idx] = true;

                // STEP2.3: （実際の二倍で記録された）距離を記録する
                doubleDistanceArray[idx] = (current.Dx, current.Dy);

                // STEP2.4: 解決されていない、領域外の8近傍をキューに追加する
                for (var dy = -1; dy <= 1; ++dy)
                {
                    for (var dx = -1; dx <= 1; ++dx)
                    {
                        var neighborX = x + dx;
                        var neighborY = y + dy;
                        if (neighborX < 0 || neighborX >= width || neighborY < 0 || neighborY >= height)
                        {
                            continue;
                        }

                        var neighborIdx = neighborY * width + neighborX;
                        if (resolvedArray[neighborIdx] || isInsideArray[neighborIdx])
                        {
                            continue;
                        }

                        var neighborDx = current.Dx + 2 * dx;
                        var neighborDy = current.Dy + 2 * dy;
                        queue.Enqueue(new Element(
                            neighborX,
                            neighborY,
                            neighborDx,
                            neighborDy,
                            Math.Sqrt(neighborDx * neighborDx + neighborDy * neighborDy)
                        ));
                    }
                }
            }

            return doubleDistanceArray;
        }

        private readonly struct Element : IComparable<Element>
        {
            public readonly int X;
            public readonly int Y;
            public readonly int Dx;
            public readonly int Dy;
            public readonly double Distance;

            public Element(int x, int y, int dx, int dy, double distance)
            {
                X = x;
                Y = y;
                Dx = dx;
                Dy = dy;
                Distance = distance;
            }

            public int CompareTo(Element other)
            {
                // NOTE: 距離が小さいものほど優先度が高くなって欲しい
                return (this.Distance - other.Distance) switch
                {
                    < 0 => -1,
                    > 0 => +1,
                    _ => 0,
                };
            }

            public sealed class Comparer : IComparer<Element>
            {
                public int Compare(Element x, Element y) => x.CompareTo(y);
            }
        }
    }
}