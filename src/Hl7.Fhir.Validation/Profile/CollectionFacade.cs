using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Hl7.Fhir.Validation.Profile
{
    /// <summary>
    /// A collection of facades as a facade for a collection of items
    /// </summary>
    /// <typeparam name="TItemFacade">Type of the item facade</typeparam>
    /// <typeparam name="TItem">Type of the underlying item</typeparam>
    public class CollectionFacade<TItemFacade, TItem> : ICollectionFacade<TItemFacade>, ICommitable
        where TItem : new()
        where TItemFacade: ICommitable
    {
        private readonly IList<TItem> _items;
        private readonly Func<TItem, TItemFacade> _createFacade;
        private readonly bool _isReadOnly;

        private List<TItemFacade> _facades = null;        

        public CollectionFacade(Func<TItem, TItemFacade> createFacade, IEnumerable<TItemFacade> facades = null, bool isReadOnly = false) 
            : this(null, createFacade, isReadOnly)
        {
            _facades = facades?.ToList() ?? new List<TItemFacade>(8);
        }

        public CollectionFacade(IList<TItem> items, Func<TItem, TItemFacade> createFacade, bool isReadOnly = false)
        {
            _items = items;
            _createFacade = createFacade;
            _isReadOnly = isReadOnly;
        }

        private void ProvideItems()
        {
            if (_facades is null && _items is object)
            {
                _facades = _items.Select(item => _createFacade(item)).ToList();
            }
        }

        public IEnumerator<TItemFacade> GetEnumerator() 
        {
            ProvideItems(); 
            return _facades.GetEnumerator();
        }

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
            ProvideItems();

            var item = new TItem();
            var facade = _createFacade(item);

            if (index is null || index >= _facades.Count)
            {
                _items?.Add(item);
                _facades.Add(facade);
            }
            else
            {
                _items?.Insert(index.Value, item);
                _facades.Insert(index.Value, facade);
            }

            return facade;
        }

        public void RemoveAt(int index)
        {
            CheckWritable();
            ProvideItems();

            _facades.RemoveAt(index);
            _items?.RemoveAt(index);
        }

        public void Commit()
        {
            foreach(var itemFacade in _facades)
            {
                itemFacade.Commit();
            }
        }
    }
}
