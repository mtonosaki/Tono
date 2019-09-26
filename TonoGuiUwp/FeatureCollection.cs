using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Metadata;

namespace Tono.Gui.Uwp
{
    /// <summary>
    /// feature collection object (for XAML support)
    /// </summary>
    [ContractVersion(typeof(UniversalApiContract), 65536)]
    [MarshalingBehavior(MarshalingType.Agile)]
    [Threading(ThreadingModel.Both)]
    [WebHostHidden]
    public sealed class FeatureCollection : IList<FeatureBase>, IEnumerable<FeatureBase>
    {
        private readonly List<FeatureBase> features = new List<FeatureBase>();

        public FeatureCollection(TGuiView parent)
        {
            View = parent;
        }

        /// <summary>
        /// auto event when collection changed
        /// </summary>
        public event EventHandler<EventArgs> CollectionChanged;

        /// <summary>
        /// the owner TGuiView
        /// </summary>
        public TGuiView View { get; private set; }

        /// <summary>
        /// child feature accessor
        /// </summary>
        /// <param name="index">collection index (0 is the first one)</param>
        /// <returns></returns>
        public FeatureBase this[int index] { get => features[index]; set => features[index] = prepareFeature(value); }

        /// <summary>
        /// registered feature count
        /// </summary>
        public int Count => features.Count;

        /// <summary>
        /// feature collection lock
        /// </summary>
        public bool IsReadOnly => false;

        public override string ToString()
        {
            return $"Features Count = {Count}";
        }

        private static int _idcounter = 0;

        /// <summary>
        /// feature property common preparation
        /// </summary>
        /// <param name="fc"></param>
        private FeatureBase prepareFeature(FeatureBase fc)
        {
            Debug.Assert(fc != null);

            fc.View = View;

            if (fc.ID.IsNothing())
            {
                fc.ID = new Id { Value = ++_idcounter };
            }
            return fc;
        }

        /// <summary>
        /// add feature
        /// </summary>
        /// <param name="feature"></param>
        public void Add(FeatureBase feature)
        {
            features.Add(prepareFeature(feature));
            CollectionChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// remove the all registered features
        /// </summary>
        public void Clear()
        {
            features.Clear();
            CollectionChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// check contains specific feature
        /// </summary>
        /// <param name="feature"></param>
        /// <returns></returns>
        public bool Contains(FeatureBase feature)
        {
            return features.Contains(feature);
        }

        /// <summary>
        /// copy feature collection
        /// </summary>
        /// <param name="array"></param>
        /// <param name="arrayIndex"></param>
        public void CopyTo(FeatureBase[] array, int arrayIndex)
        {
            for (var i = arrayIndex; i < array.Length; i++)
            {
                array[i] = features[i % features.Count];
            }
        }

        public IEnumerator<FeatureBase> GetEnumerator()
        {
            return features.GetEnumerator();
        }

        public int IndexOf(FeatureBase item)
        {
            return features.IndexOf(item);
        }

        /// <summary>
        /// insert feature to the specific position
        /// </summary>
        /// <param name="index">collection index (0 is the fiest position)</param>
        /// <param name="feature"></param>
        public void Insert(int index, FeatureBase feature)
        {
            features.Insert(index, feature);
            CollectionChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// remove specific feature from collection
        /// </summary>
        /// <param name="feature"></param>
        /// <returns></returns>
        public bool Remove(FeatureBase feature)
        {
            CollectionChanged?.Invoke(this, EventArgs.Empty);
            return features.Remove(feature);
        }

        /// <summary>
        /// remove feature
        /// </summary>
        /// <param name="index"></param>
        public void RemoveAt(int index)
        {
            features.RemoveAt(index);
            CollectionChanged?.Invoke(this, EventArgs.Empty);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return features.GetEnumerator();
        }

        /// <summary>
        /// find feature by feature name
        /// NOTE: you can use this method in OnInitialInstance only
        /// </summary>
        /// <param name="fcName"></param>
        /// <returns></returns>
        /// <remarks>
        /// not recommended to change another feature's property
        /// </remarks>
        public FeatureBase Find(string fcName)
        {
            if (Findable == false)
            {
                throw new InvalidOperationException("Findは OnInitialInstance内でのみ使用できます");
            }
            var fs =
                from fc in features
                where fc.Name.Equals(fcName)
                select fc;
            return fs.FirstOrDefault();
        }

        /// <summary>
        /// check acceptable Find method or not. (means in OnInitialInstance?)
        /// </summary>
        internal bool Findable { get; set; }
    }
}
