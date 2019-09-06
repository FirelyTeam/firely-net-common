using System;
using System.Runtime.CompilerServices;

// The following GUID is for the ID of the typelib if this project is exposed to COM

[assembly: CLSCompliant(true)]

#if DEBUG
[assembly: InternalsVisibleTo("Hl7.Fhir.ElementModel.Tests")]
[assembly: InternalsVisibleTo("Hl7.Fhir.Validation.Tests")]
#endif
