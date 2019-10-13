msbuild Hl7.Fhir.Common.sln /t:clean /v:minimal
msbuild Hl7.Fhir.Common.sln /t:restore /v:minimal
msbuild Hl7.Fhir.Common.sln /t:build /v:minimal
msbuild Hl7.Fhir.Common.sln /t:vstest /v:minimal
