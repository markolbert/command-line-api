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

        private List<PropertyDescriptor> _propertyDescriptors;
        private List<ConstructorDescriptor> _constructorDescriptors;

        protected ModelDescriptor(Type modelType)
        {
            ModelType = modelType ??
                        throw new ArgumentNullException(nameof(modelType));
        }

        private void GetPropertyDescriptors(Type curType, ref List<PropertyDescriptor> propertyDescriptors, ref Stack<string> propNameStack)
        {
            if (propertyDescriptors == null)
                propertyDescriptors = new List<PropertyDescriptor>();

            if (propNameStack == null)
                propNameStack = new Stack<string>();

            var parentPropPath = propNameStack.Aggregate(
                new StringBuilder(),
                (sb, n) => sb.Length == 0 ? sb.Append(n) : sb.Insert(0, $".{n}"),
                sb => sb.ToString()
            );

            foreach (var propInfo in curType.GetProperties(CommonBindingFlags)
                .Where(pi => pi.CanWrite))
            {
                if (propInfo.PropertyType.IsClass)
                {
                    propNameStack.Push(propInfo.Name);

                    GetPropertyDescriptors(propInfo.PropertyType, ref propertyDescriptors, ref propNameStack);
                }
                else
                    propertyDescriptors.Add(new PropertyDescriptor(propInfo, parentPropPath, this));
            }

            propNameStack.Pop();
        }

        public IReadOnlyList<ConstructorDescriptor> ConstructorDescriptors =>
            _constructorDescriptors ??=
                ModelType.GetConstructors(CommonBindingFlags)
                         .Select(i => new ConstructorDescriptor(i, this))
                         .ToList();

        public IReadOnlyList<IValueDescriptor> PropertyDescriptors =>
            _propertyDescriptors ??=
                ModelType.GetProperties(CommonBindingFlags)
                         .Where(p => p.CanWrite)
                         .Select(i => new PropertyDescriptor(i, this))
                         .ToList();

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