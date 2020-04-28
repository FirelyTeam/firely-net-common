using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Hl7.Fhir.Validation.Profile
{
    public interface IElementCollectionFacade: ICollectionFacade<ProfileElementFacade>
    {
    }

    public class ElementCollectionFacade<TProfileElementFacade, TElementDefinition> : IElementCollectionFacade
        where TProfileElementFacade: ProfileElementFacade<TElementDefinition>, new()
        where TElementDefinition : new()
    {
        private readonly List<TProfileElementFacade> _facades = new List<TProfileElementFacade>(8);

        public IEnumerator<ProfileElementFacade> GetEnumerator() => _facades.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        internal void AddFacade(TProfileElementFacade facade)
        {
            _facades.Add(facade);
        }

        public ProfileElementFacade Insert(int? index = null)
        { 
            var item = new TElementDefinition();
            var facade = new TProfileElementFacade();
            facade.ElementDefinition = item;

            if (index is null || index >= _facades.Count)
            {
                _facades.Add(facade);
            }
            else
            {
                _facades.Insert(index.Value, facade);
            }

            return facade;
        }

        public void Remove(int index)
        {
            _facades.RemoveAt(index);
        }        
    }
}
