// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace System.CommandLine.Binding
{
    public class PropertyDescriptor : IValueDescriptor
    {
        private readonly List<PropertyInfo> _propertyPath;

        internal PropertyDescriptor(
            PropertyInfo propertyInfo,
            List<PropertyInfo> parentProps,
            ModelDescriptor parent)
        {
            ValueName = propertyInfo.Name;
            ValueType = propertyInfo.PropertyType;

            Parent = parent;

            _propertyPath = new List<PropertyInfo>( parentProps ) { propertyInfo };

            var sb = new StringBuilder();

            if( parent != null )
                sb.Append( Parent );

            foreach( var propInfo in _propertyPath )
            {
                if( sb.Length > 0 )
                    sb.Append( "." );

                sb.Append( propInfo.Name );
            }

            Path = sb.ToString();
        }

        public string ValueName { get; }

        public ModelDescriptor Parent { get; }

        internal string Path { get; }

        public Type ValueType { get; }

        public bool HasDefaultValue => false;

        public object GetDefaultValue() => ValueType.GetDefaultValueForType();

        public void SetValue(object instance, object value)
        {
            var lastIdx = _propertyPath.Count - 1;

            for( var idx = 0; idx < _propertyPath.Count; idx++ )
            {
                if( idx == lastIdx )
                    _propertyPath[ idx ].SetValue( instance, value );
                else instance = _propertyPath[ idx ].GetValue( instance );
            }
        }

        public override string ToString() => $"{ValueType.Name} {Path}";
    }
}
