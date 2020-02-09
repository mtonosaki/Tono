using System;
using System.Collections.Generic;
using System.Text;

namespace Tono.Logic
{
    public class CumulativeSum
    {
        private double[] data;

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
        /// Prepare the all result first.
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
                    Buffer.Add(Buffer[i - 1] + Data[i]);
                }
                return Buffer[index];
            }
        }
    }
}
