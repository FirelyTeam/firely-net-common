﻿using System;
using System.Runtime.CompilerServices;

// The following GUID is for the ID of the typelib if this project is exposed to COM

[assembly: CLSCompliant(true)]

#if DEBUG || PROFILE_RELEASE
[assembly: InternalsVisibleTo("Hl7.Fhir.ElementModel.Tests")]
#endif

#if RELEASE
[assembly: InternalsVisibleTo("Hl7.Fhir.ElementModel.Tests, PublicKey=00240000048000009400000006020000002400005253413100040000010001001717d77343870eca52515a2ff9ba7ef2ff314f2f1e651f4a069401e35193d4d5124b33379a6380d510239044f012f720d395064192157eae8f67b3e4d524b79daadebd4e65ce67db327949b77bf26ca6c0f97c4ca1a578811202a537e4d112fffb2e42e852afdd71c3295c694911cdf0535f709b72ba172c40a2b1f2b607ffdc")]
#endif
