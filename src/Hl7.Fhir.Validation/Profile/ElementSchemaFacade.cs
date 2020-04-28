using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Hl7.Fhir.Validation.Profile
{
    public interface IChangeReceiver
    {
        void ProcessChange();
    }

    public class ElementSchemaFacade<TProfileElementFacade, TProfileSliceFacade, TElementDefinition>: IChangeReceiver
        where TProfileElementFacade: ProfileElementFacade<TElementDefinition>, new()
        where TProfileSliceFacade : ProfileSliceFacade<TElementDefinition>, new()
        where TElementDefinition : class, new()
    {
        private readonly List<TElementDefinition> _elements;
        private readonly ElementCollectionFacade<TProfileElementFacade, TElementDefinition> _elementsFacade
            = new ElementCollectionFacade<TProfileElementFacade, TElementDefinition>();
        private readonly bool _isReadOnly;
        private readonly string _typeName;

        public ElementSchemaFacade(List<TElementDefinition> elements,
            Func<TElementDefinition, string> getPath,
            Func<TElementDefinition, bool> getIsSliced,
            string typeName,
            bool isReadOnly = false)
        {
            _elements = elements;
            _isReadOnly = isReadOnly;
            _typeName = typeName;

            var elementEnumerable = (IEnumerable<TElementDefinition>)elements;
            AddElementFacades(_elementsFacade, ref elementEnumerable, getPath, getIsSliced, _typeName + ".");
        }

        private void AddElementFacades(
            ElementCollectionFacade<TProfileElementFacade, TElementDefinition> target,
            ref IEnumerable<TElementDefinition> items,
            Func<TElementDefinition, string> getPath,
            Func<TElementDefinition, bool> getIsSliced,
            string pathPrefix)
        {
            while (items.Any() && getPath(items.First()).StartsWith(pathPrefix))
            {
                var facade = CreateElementFacade(ref items, getPath, getIsSliced, pathPrefix);
                target.AddFacade(facade);
            }
        }

        private TProfileElementFacade CreateElementFacade(ref IEnumerable<TElementDefinition> items,
            Func<TElementDefinition, string> getPath,
            Func<TElementDefinition, bool> getIsSliced,
            string pathPrefix)
        {
            var element = items.First();
            var elementPath = getPath(element);
            var name = elementPath.Substring(pathPrefix.Length);

            if (name.Contains('.'))
            {
                throw new InvalidOperationException($"ElementDefinition '{getPath(element)}' is not preceded by a parent ElementDefinition");
            }

            var result = new TProfileElementFacade();
            result.ElementDefinition = element;

            if (getIsSliced(element))
            {
                items = items.Skip(1);                
            }

            AddSliceFacades((SliceCollectionFacade<TProfileSliceFacade, TElementDefinition>)result.Slices, 
                ref items, getPath, getIsSliced, pathPrefix);

            return result;
        }

        private void AddSliceFacades(SliceCollectionFacade<TProfileSliceFacade, TElementDefinition> target,
            ref IEnumerable<TElementDefinition> items,
            Func<TElementDefinition, string> getPath,
            Func<TElementDefinition, bool> getIsSliced,
            string pathPrefix)
        {
            while (items.Any() && getPath(items.First()).StartsWith(pathPrefix))
            {
                var facade = CreateSliceFacade(ref items, getPath, getIsSliced, pathPrefix);
                target.AddFacade(facade);
            }
        }

        private TProfileSliceFacade CreateSliceFacade(ref IEnumerable<TElementDefinition> items,
            Func<TElementDefinition, string> getPath,
            Func<TElementDefinition, bool> getIsSliced,
            string pathPrefix)
        {
            var element = items.First();
            var result = new TProfileSliceFacade();
            result.ElementDefinition = element;
            items = items.Skip(1);

            AddElementFacades((ElementCollectionFacade<TProfileElementFacade, TElementDefinition>)result.Elements, 
                ref items, getPath, getIsSliced, getPath(element) + ".");

            return result;
        }

        public void ProcessChange()
        {
            CheckWritable();

            _elements.Clear();
            _elements.AddRange(GetElementDefinitions(_elementsFacade));
        }

        private IEnumerable<TElementDefinition> GetElementDefinitions(ElementCollectionFacade<TProfileElementFacade, TElementDefinition> elementsFacade)
        {
            foreach(var element in GetElementDefinitions(elementsFacade))
            {
                yield return element;
            }
        }

        private void CheckWritable()
        {
            if (_isReadOnly)
            {
                throw new NotImplementedException("Editing is not implemented on this schema facade");
            }
        }        
    }
}
