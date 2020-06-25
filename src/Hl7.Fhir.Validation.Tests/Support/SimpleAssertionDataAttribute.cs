using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

namespace Hl7.Fhir.Validation.Tests.Support
{
    public abstract class SimpleAssertionDataAttribute : Attribute, ITestDataSource
    {
        public abstract IEnumerable<object[]> GetData();

        public IEnumerable<object[]> GetData(MethodInfo methodInfo)
            => GetData();

        public string GetDisplayName(MethodInfo methodInfo, object[] data)
        {
            if (data != null)
                return string.Format(CultureInfo.CurrentCulture, "{0} ({1})", methodInfo.Name, string.Join(",", data));

            return null;
        }
    }
}
