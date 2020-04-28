using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Hl7.Fhir.Validation.Profile
{
    public interface ICollectionFacade<TItemFacade>: IEnumerable<TItemFacade>
    {
        TItemFacade Insert(int? index = null);
        void Remove(int index);
    }

    public class CollectionFacade<TItemFacade, TItem>: ICollectionFacade<TItemFacade>
        where TItem: new()
    {
        private readonly IList<TItem> _items;
        private readonly List<TItemFacade> _facades;
        private readonly Func<TItem, TItemFacade> _createFacade;
        private readonly bool _isReadOnly;

        public CollectionFacade(IList<TItem> items, Func<TItem, TItemFacade> createFacade, bool isReadOnly = false)
        {
            _items = items;
            _createFacade = createFacade;
            _isReadOnly = isReadOnly;
            _facades = items.Select(item => createFacade(item)).ToList();
        }

        public IEnumerator<TItemFacade> GetEnumerator() => _facades.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private void CheckWritable()
        {
            if (_isReadOnly)
            {
                throw new NotImplementedException("Editing is not implemented on this collection facade");
            }
        }

        public TItemFacade Insert(int? index = null)
        {
            CheckWritable();

            var item = new TItem();
            var facade = _createFacade(item);

            if (index is null || index >= _items.Count)
            {
                _items.Add(item);
                _facades.Add(facade);
            }
            else
            {
                _items.Insert(index.Value, item);
                _facades.Insert(index.Value, facade);
            }

            return facade;
        }

        public void Remove(int index)
        {
            CheckWritable();

            _facades.RemoveAt(index);
            _items.RemoveAt(index);
        }
    }
}
