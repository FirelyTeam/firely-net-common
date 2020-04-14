/* 
 * Copyright (c) 2020, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/fhir-net-api/master/LICENSE
 */

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hl7.Fhir.Utility
{
    public static class TaskHelper
    {
        public static T Await<T>(Func<Task<T>> asyncFunc)
        {
#if NET40
            return TaskEx.Run(asyncFunc).Result;
#else
            return Task.Run(asyncFunc).Result;
#endif
        }

        public static void Await(Func<Task> asyncFunc)
        {
#if NET40
            TaskEx.Run(asyncFunc).Wait();
#else
            Task.Run(asyncFunc).Wait();
#endif
        }

        public static async Task<bool> AnyAsync<TSource>(this IEnumerable<TSource> source, Func<TSource, Task<bool>> predicate)
        {
            foreach (var elem in source)
                if (await predicate(elem)) return true;

            return false;
        }
    }
}
