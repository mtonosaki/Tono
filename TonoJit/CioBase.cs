// Copyright (c) Manabu Tonosaki All rights reserved.
// Licensed under the MIT license.

using System;

namespace Tono.Jit
{
    /// <summary> 
    /// in-command and out-constraint base class
    /// </summary>
    public abstract class CioBase
    {
        public string Name { get; set; } = "Cio:" + Guid.NewGuid().ToString();

        /// <summary>
        /// get owner process from work object
        /// </summary>
        /// <param name="work"></param>
        /// <returns></returns>
        protected abstract JitProcess GetParentProcess(JitWork work);

        /// <summary>
        /// remarks
        /// </summary>
        public string Remarks { get; set; }

        // remember last work in-time (especially for Span constraint)
        public interface ILastInTime
        {
            DateTime LastInTime { get; set; }
        }
        public interface IWorkInReserved
        {
            void AddWorkInReserve(JitWork work);
            void RemoveWorkInReserve(JitWork work);
        }

        public override bool Equals(object obj)
        {
            if (obj is CioBase cio)
            {
                return cio.Name == Name;
            }
            else
            {
                return false;
            }
        }
        public override int GetHashCode()
        {
            return MathUtil.GetFnvHash(Name);
        }
        public override string ToString()
        {
            return $"{GetType().Name} Name={Name}";
        }
    }
}
