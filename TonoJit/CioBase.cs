// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using System;
using System.Linq;

namespace Tono.Jit
{
    /// <summary> 
    /// in-command and out-constraint base class
    /// </summary>
    public abstract class CioBase : IJitObjectID
    {
        public string ID { get; set; } = JacInterpreter.MakeID("CIO");

        /// <summary>
        /// Parent Process of this Ci/Co
        /// </summary>
        /// <remarks>
        /// Automatically set by Process.Ci.Add / Process.Co.Add 
        /// </remarks>
        public JitProcess ParentProcess { get; set; }

        /// <summary>
        /// Parent Process from work.
        /// Ci = Same with ParentProcess Property. Co = NextProcess of work
        /// </summary>
        /// <param name="work"></param>
        /// <returns></returns>
        protected abstract JitProcess GetParentProcess(JitWork work);

        /// <summary>
        /// Remarks
        /// </summary>
        public string Remarks { get; set; }

        /// <summary>
        /// Make one point innstance value.
        /// </summary>
        /// <returns></returns>
        public virtual string MakeShortValue()
        {
            return "";
        }

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
                return cio.ID == ID;
            }
            else
            {
                return false;
            }
        }
        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }
        public override string ToString()
        {
            return $"{GetType().Name} ID={ID}";
        }
    }
}
