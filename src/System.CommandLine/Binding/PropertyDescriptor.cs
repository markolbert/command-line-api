// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

namespace System.CommandLine.Binding
{
    public class PropertyDescriptor : IValueDescriptor
    {
        internal PropertyDescriptor(
            PropertyInfo propertyInfo,
            ModelDescriptor parent,
            PropertyDescriptor parentDescriptor = null )
        {
            ValueName = propertyInfo.Name;
            ValueType = propertyInfo.PropertyType;

            Parent = parent;
            PropertyInfo = propertyInfo;
            ParentDescriptor = parentDescriptor;

            var sb = new StringBuilder(ValueName);
            var curParentDescriptor = parentDescriptor;

            while( curParentDescriptor != null )
            {
                if( sb.Length > 0 ) 
                    sb.Insert( 0, "." );

                sb.Insert( 0, curParentDescriptor.ValueName );

                curParentDescriptor = curParentDescriptor.ParentDescriptor;
            }

            if( Parent != null )
                sb.Insert( 0, $"{Parent}." );

            Path = sb.ToString();

        }

        public string ValueName { get; }

        public ModelDescriptor Parent { get; }

        internal PropertyDescriptor ParentDescriptor { get; }
        internal bool IsBound { get; set; }
        internal PropertyInfo PropertyInfo { get; }
        internal string Path { get; }

        public Type ValueType { get; }

        public bool HasDefaultValue => false;

        public object GetDefaultValue() => ValueType.GetDefaultValueForType();

        public void SetValue( object instance, object value )
        {
            var piElements = GetPropertyInfoElements();
            var setIdx = piElements.Count - 1;

            for( var idx = 0; idx < piElements.Count; idx++ )
            {
                if( idx == setIdx ) piElements[ idx ].SetValue( instance, value );
                else instance = piElements[ idx ].GetValue( instance );
            }
        }

        public override string ToString() => $"{ValueType.Name} {Path}";

        private List<PropertyInfo> GetPropertyInfoElements()
        {
            var retVal = new List<PropertyInfo>() { PropertyInfo };

            var curParentDescriptor = ParentDescriptor;

            while( curParentDescriptor != null )
            {
                retVal.Add( curParentDescriptor.PropertyInfo );

                curParentDescriptor = curParentDescriptor.ParentDescriptor;
            }

            retVal.Reverse();

            return retVal;
        }
    }
}
