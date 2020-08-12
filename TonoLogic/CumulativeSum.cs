using System.Collections.Generic;

namespace Tono.Logic
{
    /// <summary>
    /// Cumulative Sum Utility 累積和
    /// </summary>
    public class CumulativeSum
    {
        private double[] data = new double[] { };

        private List<double> Buffer;

        public double[] Data
        {
            get => data;
            set
            {
                data = value;
                Buffer = new List<double> { 0, };
            }
        }

        /// <summary>
        /// Prepare the all result first.(Run after Data set)
        /// </summary>
        /// <returns></returns>
        public CumulativeSum Prepare()
        {
            Get(Data.Length);
            return this;
        }

        /// <summary>
        /// Get cumlative sum at the index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public double Get(int index)
        {
            if (index < Buffer.Count)
            {
                return Buffer[index];
            }
            else
            {
                for (var i = Buffer.Count; i <= index; i++)
                {
                    Buffer.Add(Buffer[i - 1] + Data[i - 1]);
                }
                return Buffer[index];
            }
        }

        /// <summary>
        /// Get Section Total
        /// </summary>
        /// <param name="startPoint"></param>
        /// <param name="endPoint"></param>
        /// <returns></returns>
        /// <remarks>
        /// 0   1   2   3   4   5  --- Point
        /// |1.2|3.2|2.0|7.8|1.6|
        ///  - SectionTotal(0,0) = 0.0
        ///  - SectionTotal(0,1) = 1.2
        ///  - SectionTotal(0,2) = 1.2 + 3.2
        ///  - SectionTotal(1,2) = 3.2
        ///  - SectionTotal(1,3) = 3.2 + 2.0
        /// </remarks>
        public double SectionTotal(int startPoint, int endPoint)
        {
            if (startPoint >= endPoint)
            {
                return 0;
            }

            return Get(endPoint) - Get(startPoint);
        }
    }
}
