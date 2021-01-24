using Newtonsoft.Json;
using StateChartsDotNet.Common.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace StateChartsDotNet.Metadata
{
    [JsonArray]
    public class MetadataList<TMetadata> : IList<TMetadata> where TMetadata : IModelMetadata
    {
        private readonly List<TMetadata> _list;

        public MetadataList()
        {
            _list = new List<TMetadata>();
        }

        [JsonIgnore]
        internal Func<string> MetadataIdResolver { private get; set; }

        [JsonIgnore]
        internal Action ResolveDocumentOrder { private get; set; }

        private void AddResolvers(TMetadata metadata)
        {
            Debug.Assert(metadata != null);

            Func<IModelMetadata, string> func = meta =>
            {
                if (this.MetadataIdResolver == null)
                {
                    return null;
                }

                var idx = _list.IndexOf((TMetadata) meta);

                if (idx == -1)
                {
                    return null;
                }

                return $"{this.MetadataIdResolver()}[{idx}]";
            };

            dynamic item = metadata;

            item.MetadataIdResolver = func;

            if (this.ResolveDocumentOrder != null)
            {
                item.ResolveDocumentOrder = this.ResolveDocumentOrder;
            }
        }

        private void RemoveResolvers(TMetadata metadata)
        {
            ((dynamic) metadata).MetadataIdResolver = null;
        }

        public TMetadata this[int index] 
        {
            get => ((IList<TMetadata>)_list)[index];
            
            set
            {
                AddResolvers(value);
                ((IList<TMetadata>)_list)[index] = value;
            }
        }

        public int Count => ((ICollection<TMetadata>)_list).Count;

        public bool IsReadOnly => ((ICollection<TMetadata>)_list).IsReadOnly;

        public void Add(TMetadata item)
        {
            AddResolvers(item);
            ((ICollection<TMetadata>)_list).Add(item);
        }

        public void Clear()
        {
            _list.ForEach(RemoveResolvers);
            ((ICollection<TMetadata>)_list).Clear();
        }

        public bool Contains(TMetadata item)
        {
            return ((ICollection<TMetadata>)_list).Contains(item);
        }

        public void CopyTo(TMetadata[] array, int arrayIndex)
        {
            Array.ForEach(array, AddResolvers);
            ((ICollection<TMetadata>)_list).CopyTo(array, arrayIndex);
        }

        public IEnumerator<TMetadata> GetEnumerator()
        {
            return ((IEnumerable<TMetadata>)_list).GetEnumerator();
        }

        public int IndexOf(TMetadata item)
        {
            return ((IList<TMetadata>)_list).IndexOf(item);
        }

        public void Insert(int index, TMetadata item)
        {
            AddResolvers(item);
            ((IList<TMetadata>)_list).Insert(index, item);
        }

        public bool Remove(TMetadata item)
        {
            RemoveResolvers(item);
            return ((ICollection<TMetadata>)_list).Remove(item);
        }

        public void RemoveAt(int index)
        {
            var item = this[index];
            RemoveResolvers(item);
            ((IList<TMetadata>)_list).RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_list).GetEnumerator();
        }
    }
}
