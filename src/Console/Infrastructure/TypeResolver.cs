using System;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Cli;

namespace Omnia.CLI.Infrastructure
{
    public sealed class TypeResolver : ITypeResolver
    {
        private readonly IServiceProvider _provider;

        public TypeResolver(IServiceProvider provider)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        }

        public object Resolve(Type type)
        {
            return _provider.GetRequiredService(type);
        }
    }
}