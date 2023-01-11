# PLEASE NOTE
This repository has served as the place for code shared between the FHIR release branches for the [Firely .NET SDK](https://github.com/FirelyTeam/firely-net-sdk). However, since SDK 5.0, all these branches have been unified and this repo is no longer in use and just kept for archival purposes.


_(Original README continues below)_

|Branch||
|---|---|
|develop|[![Build Status](https://dev.azure.com/firely/firely-net-sdk/_apis/build/status/Common/FirelyTeam.firely-net-common?branchName=develop)](https://dev.azure.com/firely/firely-net-sdk/_build/latest?definitionId=83&branchName=develop)|

|NuGet Package||
|---|---|
|Hl7.Fhir.ElementModel|[![Nuget](https://img.shields.io/nuget/dt/Hl7.Fhir.ElementModel)](https://www.nuget.org/packages/Hl7.Fhir.ElementModel) |
|Hl7.Fhir.Serialization|[![Nuget](https://img.shields.io/nuget/dt/Hl7.Fhir.Serialization)](https://www.nuget.org/packages/Hl7.Fhir.Serialization)|
|Hl7.FhirPath|[![Nuget](https://img.shields.io/nuget/dt/Hl7.FhirPath)](https://www.nuget.org/packages/Hl7.FhirPath)|
|Hl7.Fhir.Support | [![Nuget](https://img.shields.io/nuget/dt/Hl7.Fhir.Support)](https://www.nuget.org/packages/Hl7.Fhir.Support)|
|Hl7.Fhir.Support.Poco|[![Nuget](https://img.shields.io/nuget/dt/Hl7.Fhir.Support.Poco)](https://www.nuget.org/packages/Hl7.Fhir.Support.Poco) |

## Introduction ##
This repository is a submodule-repository for the parent [Firely .NET SDK](https://github.com/FirelyTeam/firely-net-sdk). It contains code that works across all FHIR versions (e.g. DSTU2, R4, etc) and thus is shared by the branches of the parent repository. Although it contains functionality that can be used independently (e.g. the FhirPath evaluator), all issues should be reported to the parent repo.



