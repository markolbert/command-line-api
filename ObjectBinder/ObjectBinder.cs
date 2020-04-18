﻿using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Binding;
using System.CommandLine.Invocation;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace ObjectBinder
{
    /// <summary>
    /// Implements an ICommandHandler for mapping parse results to a TModel
    /// object by specifying the alias-to-property bindings explicitly
    /// </summary>
    /// <typeparam name="TModel"></typeparam>
    public class ObjectBinder<TModel> : IObjectBinder<TModel> 
        where TModel : class
    {
        public ObjectBinder(Command command, TModel target)
        {
            Command = command ?? throw new NullReferenceException(nameof(command));
            Target = target ?? throw new NullReferenceException(nameof(target));
            ModelBinder = new ModelBinder<TModel>();
        }

        public Command Command { get; }
        public ModelBinder<TModel> ModelBinder { get; }
        public TModel Target { get; }

        public Task<int> Bind( InvocationContext context )
        {
            if( context == null )
                return Task.FromResult( 1 );

            var cancelToken = context.GetCancellationToken();
            if( cancelToken.IsCancellationRequested )
                return (Task<int>) Task.FromCanceled( cancelToken );

            ModelBinder.UpdateInstance( Target, context.BindingContext );

            return Task.FromResult( 0 );
        }
    }
}
