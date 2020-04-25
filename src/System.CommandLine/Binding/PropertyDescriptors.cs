using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace System.CommandLine.Binding
{
    public class PropertyDescriptors
    {
        private readonly Dictionary<string, PropertyDescriptor> _propertyDescriptors =
            new Dictionary<string, PropertyDescriptor>( StringComparer.Ordinal );

        // mostly for backwards compatibility...
        public ReadOnlyCollection<PropertyDescriptor> Descriptors =>
            _propertyDescriptors.Select(kvp => kvp.Value)
                .ToList()
                .AsReadOnly();

        public bool Add( PropertyDescriptor toAdd, bool throwOnDuplicate = true )
        {
            if( toAdd == null )
                return false;

            if( _propertyDescriptors.ContainsKey( toAdd.Path ) )
            {
                if( throwOnDuplicate )
                    throw new ArgumentException(
                        $"Duplicate property paths in the model class are not allowed ({toAdd.Parent})");

                return false;
            }

            _propertyDescriptors.Add( toAdd.Path, toAdd );

            return true;
        }

        public bool Add( PropertyInfo propInfo, List<PropertyInfo> parentProps, ModelDescriptor modelDescriptor, bool throwOnDuplicate = true )
        {
            parentProps ??=new List<PropertyInfo>();

            return Add( new PropertyDescriptor( propInfo, parentProps, modelDescriptor ), throwOnDuplicate );
        }

        // for backwards compatibility
        public PropertyDescriptor GetPropertyDescriptor( 
            Type propertyType, 
            string propertyName,
            bool throwOnMultiple = true )
        {
            var matches = _propertyDescriptors.Where( pd =>
                    pd.Value.ValueName.Equals( propertyName, StringComparison.Ordinal ) &&
                    pd.Value.ValueType == propertyType )
                .ToList();

            switch( matches.Count )
            {
                case 0:
                    return null;

                case 1:
                    return matches[ 0 ].Value;

                default:
                    if( !throwOnMultiple )
                        return matches[ 0 ].Value;

                    throw new ArgumentException(
                        $"Multiple {nameof(PropertyDescriptor)} objects have {nameof(PropertyDescriptor.ValueName)} '{propertyName}' and {nameof(PropertyDescriptor.ValueType)} '{propertyType.Name}'" );
            }
        }

        public PropertyDescriptor GetPropertyDescriptor( string propertyPath ) =>
            _propertyDescriptors.ContainsKey( propertyPath )
                ? _propertyDescriptors[ propertyPath ]
                : null;

        public PropertyDescriptor GetPropertyDescriptor<TModel, TValue>(
            Expression<Func<TModel, TValue>> propertySelector )
        {
            if( propertySelector == null )
                throw new NullReferenceException( $"{nameof(propertySelector)} is undefined" );

            // walk the expression tree to extract property path names and the property type
            var propNames = new List<string>();

            var curExpr = propertySelector.Body;
            Type propType = null;

            while( curExpr != null )
            {
                switch( curExpr )
                {
                    case MemberExpression memExpr:
                        add_target_property_name( memExpr );

                        // walk up expression tree
                        curExpr = memExpr.Expression;

                        break;

                    case UnaryExpression unaryExpr:
                        if( unaryExpr.Operand is MemberExpression unaryMemExpr )
                            add_target_property_name( unaryMemExpr );

                        // we're done; UnaryExpressions aren't part of an expression tree
                        curExpr = null;

                        break;

                    case ParameterExpression paramExpr:
                        // this is the root/anchor of the expression tree. we want 
                        // the simple type name, not the node's name
                        propNames.Add( paramExpr.Type.Name );

                        // we're done
                        curExpr = null;

                        break;
                }
            }

            propNames.Reverse();

            var retVal = GetPropertyDescriptor( string.Join( ".", propNames ) );

            // this mismatch should never occur but...
            if( retVal.ValueType != propType)
                throw new InvalidCastException($"{nameof(PropertyDescriptor.ValueType)} is '{retVal.ValueType}' should be '{propType}'");

            return retVal;

            void add_target_property_name( MemberExpression memExpr )
            {
                if( memExpr.Member is PropertyInfo propInfo )
                {
                    propNames.Add( propInfo.Name );
                    propType = propInfo.PropertyType;
                }
            }
        }
    }
}