using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hl7.Fhir.Validation.Profile
{
    public interface ICollectionFacade<out TFacade>: IEnumerable<TFacade>
    {
        TFacade Insert(int? index = null);
        void RemoveAt(int index);
    }
}
