// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace System.CommandLine.Binding
{
    public class ModelDescriptor
    {
        private const BindingFlags CommonBindingFlags =
            BindingFlags.IgnoreCase
            | BindingFlags.Public
            | BindingFlags.Instance;

        private static readonly ConcurrentDictionary<Type, ModelDescriptor> _modelDescriptors = new ConcurrentDictionary<Type, ModelDescriptor>();

        private List<ConstructorDescriptor> _constructorDescriptors;

        protected ModelDescriptor(Type modelType)
        {
            ModelType = modelType ??
                        throw new ArgumentNullException(nameof(modelType));

            // Find all the writeable properties in modelType. Note that the collection
            // is flattened and the property names must all be unique
            var props = new List<PropertyInfo>();

            GetPropertyDescriptors(modelType, null);
        }

        private void GetPropertyDescriptors(Type curType, PropertyDescriptor parentDescriptor )
        {
            foreach ( var propInfo in curType.GetProperties( CommonBindingFlags )
                .Where( pi => pi.CanWrite ) )
            {
                var propDescriptor = PropertyDescriptors.Add( propInfo, parentDescriptor, this );

                // recurse through writable properties
                if( propInfo.PropertyType.IsClass || propInfo.PropertyType.IsInterface )
                {
                    GetPropertyDescriptors( propInfo.PropertyType, propDescriptor );
                }
            }
        }

        public IReadOnlyList<ConstructorDescriptor> ConstructorDescriptors =>
            _constructorDescriptors ??=
                ModelType.GetConstructors(CommonBindingFlags)
                         .Select(i => new ConstructorDescriptor(i, this))
                         .ToList();

        public PropertyDescriptors PropertyDescriptors { get; } = new PropertyDescriptors();

        public Type ModelType { get; }

        public override string ToString() => $"{ModelType.Name}";

        public static ModelDescriptor FromType<T>() =>
            _modelDescriptors.GetOrAdd(
                typeof(T),
                _ => new ModelDescriptor(typeof(T)));

        public static ModelDescriptor FromType(Type type) =>
            _modelDescriptors.GetOrAdd(
                type,
                _ => new ModelDescriptor(type));
    }
}