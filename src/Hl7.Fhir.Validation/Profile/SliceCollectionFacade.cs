using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hl7.Fhir.Validation.Profile
{
    public interface ISliceCollectionFacade: ICollectionFacade<ProfileSliceFacade>
    {
    }

    public class SliceCollectionFacade<TProfileSliceFacade, TElementDefinition> : ISliceCollectionFacade
        where TProfileSliceFacade: ProfileSliceFacade<TElementDefinition>, new()
        where TElementDefinition : new()
    {
        private readonly List<TProfileSliceFacade> _facades = new List<TProfileSliceFacade>(8);

        public IEnumerator<ProfileSliceFacade> GetEnumerator() => _facades.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        internal void AddFacade(TProfileSliceFacade facade)
        {
            _facades.Add(facade);
        }

        public ProfileSliceFacade Insert(int? index = null)
        {
            var item = new TElementDefinition();
            var facade = new TProfileSliceFacade();
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
