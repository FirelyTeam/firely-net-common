/* 
 * Copyright (c) 2019, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/firely-net-sdk/master/LICENSE
 */

#if USE_CODE_GEN

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

#nullable enable

namespace Hl7.Fhir.Utility
{
    public static class PropertyInfoExtensions
    {
        public static Func<object> BuildFactoryMethod(this Type type)
        {
            var ti = type.GetTypeInfo();

            if (!ti.IsClass) throw new NotSupportedException($"Can only create factory methods for classes (which {type} is not).");

            var constructor = ti.GetConstructor(Type.EmptyTypes);
            if (constructor is null) throw new NotSupportedException($"Cannot generate factory method for type {type}: there is no default constructor.");

            DynamicMethod getter = new($"{type.Name}_new", typeof(object), Type.EmptyTypes);
            ILGenerator il = getter.GetILGenerator();

            il.Emit(OpCodes.Newobj, constructor);
            if (ti.IsValueType)
                il.Emit(OpCodes.Box, type);

            il.Emit(OpCodes.Ret);

            return (Func<object>)getter.CreateDelegate(typeof(Func<object>));
        }

        public static Func<IList> BuildListFactoryMethod(this Type type)
        {
            // Note that MakeGenericType() will return the same Type instance for the same List<T> type instantiations,
            // so we don't have to cache the result.
            var constructor = typeof(List<>).MakeGenericType(type).GetTypeInfo().GetConstructor(Type.EmptyTypes)
                ?? throw new ArgumentException($"Type {type.Name} does not have a parameterless constructor.");

            DynamicMethod getter = new($"new_list_of_{type.Name}", typeof(IList), Type.EmptyTypes);
            ILGenerator il = getter.GetILGenerator();

            il.Emit(OpCodes.Newobj, constructor);
            il.Emit(OpCodes.Ret);

            return (Func<IList>)getter.CreateDelegate(typeof(Func<IList>));
        }


        public static Func<T, object> GetValueGetter<T>(this PropertyInfo propertyInfo)
        {
#if NET40
            MethodInfo getMethod = propertyInfo.GetGetMethod();
#else
            MethodInfo getMethod = propertyInfo.GetMethod ?? throw new InvalidOperationException($"Property {propertyInfo.Name} does not have a getter.");
#endif

            if (getMethod == null)
                throw new InvalidOperationException("Property has no get method.");

            if (typeof(T) != propertyInfo.DeclaringType && typeof(T) != typeof(object))
                throw new ArgumentException("Generic param T should be the type of property's declaring class.", nameof(propertyInfo));

            DynamicMethod getter = new($"{propertyInfo.Name}_get", typeof(object), new Type[] { typeof(object) },
                propertyInfo.DeclaringType!);

            ILGenerator il = getter.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Castclass, propertyInfo.DeclaringType!);
            il.EmitCall(OpCodes.Callvirt, getMethod, null);

            if (propertyInfo.PropertyType.GetTypeInfo().IsValueType)
                il.Emit(OpCodes.Box, propertyInfo.PropertyType);

            il.Emit(OpCodes.Ret);

            return (Func<T, object>)getter.CreateDelegate(typeof(Func<T, object>));
        }

        public static Func<object, object> GetValueGetter(this PropertyInfo propertyInfo) =>
            GetValueGetter<object>(propertyInfo);

        public static Action<T, object> GetValueSetter<T>(this PropertyInfo propertyInfo)
        {
#if NET40
            MethodInfo setMethod = propertyInfo.GetSetMethod();
#else
            MethodInfo setMethod = propertyInfo.SetMethod ?? throw new InvalidOperationException($"Property {propertyInfo.Name} does not have a setter."); ;
#endif

            if (setMethod == null)
                throw new InvalidOperationException("Property has no set method.");

            if (typeof(T) != propertyInfo.DeclaringType && typeof(T) != typeof(object))
                throw new ArgumentException("Generic param T should be the type of property's declaring class.", nameof(propertyInfo));

            Type[] arguments = new Type[] { typeof(object), typeof(object) };
            DynamicMethod setter = new($"{propertyInfo.Name}_set", typeof(object), arguments, propertyInfo.DeclaringType!, true);
            ILGenerator il = setter.GetILGenerator();

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Castclass, propertyInfo.DeclaringType!);
            il.Emit(OpCodes.Ldarg_1);

            if (propertyInfo.PropertyType.GetTypeInfo().IsClass)
                il.Emit(OpCodes.Castclass, propertyInfo.PropertyType);
            else
                il.Emit(OpCodes.Unbox_Any, propertyInfo.PropertyType);

            il.EmitCall(OpCodes.Callvirt, setMethod, null);
            il.Emit(OpCodes.Ldarg_0);

            il.Emit(OpCodes.Ret);

            var del = (Func<T, object, object>)setter.CreateDelegate(typeof(Func<T, object, object>));
            void actionDelegate(T obj, object val) => del(obj, val);

            return actionDelegate;
        }

        public static Action<object, object> GetValueSetter(this PropertyInfo propertyInfo) => GetValueSetter<object>(propertyInfo);
    }
}

#nullable restore

#endif