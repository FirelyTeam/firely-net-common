/* 
 * Copyright (c) 2014, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/firely-net-sdk/master/LICENSE
 */

using System;

namespace Hl7.Fhir.Utility
{
    public interface ICodedException
    {
        string ErrorCode { get; }
        string Message { get; }

        Exception Exception { get; }

        ICodedException WithMessage(string message);
    }
}
