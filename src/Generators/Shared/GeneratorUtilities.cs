﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Microsoft.CodeAnalysis;
#if ROSLYN_4_0_OR_GREATER
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
#endif

[assembly: System.Resources.NeutralResourcesLanguage("en-us")]

#pragma warning disable CA1716
namespace Microsoft.Gen.Shared;
#pragma warning restore CA1716

#if !SHARED_PROJECT
[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
#endif
internal static class GeneratorUtilities
{
    public static string GeneratedCodeAttribute { get; } = $"global::System.CodeDom.Compiler.GeneratedCodeAttribute(" +
                       $"\"{typeof(GeneratorUtilities).Assembly.GetName().Name}\", " +
                       $"\"{typeof(GeneratorUtilities).Assembly.GetName().Version}\")";

    public static string FilePreamble { get; } = @$"
// <auto-generated/>
#nullable enable
#pragma warning disable CS1591 // Compensate for https://github.com/dotnet/roslyn/issues/54103
";

#if ROSLYN_4_0_OR_GREATER

    [ExcludeFromCodeCoverage]
    public static void Initialize(
        IncrementalGeneratorInitializationContext context,
        HashSet<string> fullyQualifiedAttributeNames,
        Action<Compilation, IEnumerable<SyntaxNode>, SourceProductionContext> process) => Initialize(context, fullyQualifiedAttributeNames, x => x, process);

    [ExcludeFromCodeCoverage]
    public static void Initialize(
        IncrementalGeneratorInitializationContext context,
        HashSet<string> fullyQualifiedAttributeNames,
        Func<SyntaxNode, SyntaxNode?> transform,
        Action<Compilation, IEnumerable<SyntaxNode>, SourceProductionContext> process)
    {
        // strip the namespace prefix and the Attribute suffix
        var shortAttributeNames = new HashSet<string>();
        foreach (var n in fullyQualifiedAttributeNames)
        {
            var index = n.LastIndexOf('.') + 1;
            _ = shortAttributeNames.Add(n.Substring(index, n.Length - index - "Attribute".Length));
        }

        var declarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                (node, _) => Predicate(node, shortAttributeNames),
                (gsc, ct) => Filter(gsc, fullyQualifiedAttributeNames, transform, ct))
            .Where(t => t is not null)
            .Select((t, _) => t!);

        var compilationAndTypes = context.CompilationProvider.Combine(declarations.Collect());

        context.RegisterSourceOutput(compilationAndTypes, (spc, source) =>
        {
            var compilation = source.Left;
            var nodes = source.Right;

            if (nodes.IsDefaultOrEmpty)
            {
                // nothing to do yet
                return;
            }

            process(compilation, nodes.Distinct(), spc);
        });

        static bool Predicate(SyntaxNode node, HashSet<string> shortAttributeNames)
        {
            if (node.IsKind(SyntaxKind.Attribute))
            {
                var attr = (AttributeSyntax)node;

                // see if we can trivially reject this node and avoid further work
                if (attr.Name is IdentifierNameSyntax id)
                {
                    return shortAttributeNames.Contains(id.Identifier.Text);
                }

                // too complicated to check further, the filter will have to decide
                return true;
            }

            return false;
        }

        static SyntaxNode? Filter(GeneratorSyntaxContext context, HashSet<string> fullyQualifiedAttributeNames, Func<SyntaxNode, SyntaxNode?> transform, CancellationToken cancellationToken)
        {
            var attributeSyntax = (AttributeSyntax)context.Node;

            var ctor = context.SemanticModel.GetSymbolInfo(attributeSyntax, cancellationToken).Symbol as IMethodSymbol;
            var attributeType = ctor?.ContainingType;
            if (attributeType != null && fullyQualifiedAttributeNames.Contains(GetAttributeDisplayName(attributeType)))
            {
                var node = attributeSyntax.Parent?.Parent;
                if (node != null)
                {
                    return transform(node);
                }
            }

            return null;
        }

        static string GetAttributeDisplayName(INamedTypeSymbol attributeType)
            => attributeType.IsGenericType ?
                attributeType.OriginalDefinition.ToDisplayString() :
                attributeType.ToDisplayString();
    }
#endif

    /// <summary>
    /// Reports will not be generated during design time to prevent file being written on every keystroke in VS.
    /// Refererences:
    ///   1. <see href=
    ///   "https://github.com/dotnet/project-system/blob/c872b4d46e3f308d4b859e684896e1122bdf03c2/docs/design-time-builds.md#determining-whether-a-target-is-running-in-a-design-time-build">
    /// Design-time build</see>.
    ///   2. <see href="https://github.com/dotnet/roslyn/blob/6c9697c56fe39d2335b61ae7c6b342e7b76779ef/docs/features/code-generation.cookbook.md#consume-msbuild-properties-and-metadata">
    ///   Reading MSBuild Properties in Source Generators</see>.
    /// </summary>
    /// <param name="context"><see cref="GeneratorExecutionContext"/>.</param>
    /// <param name="msBuildProperty">The name of the MSBuild property that determines whether to produce a report.</param>
    /// <returns>bool value to indicate if reports should be generated.</returns>
    public static bool ShouldGenerateReport(GeneratorExecutionContext context, string msBuildProperty)
    {
        _ = context.AnalyzerConfigOptions.GlobalOptions.TryGetValue(msBuildProperty, out var generateFiles);

        return string.Equals(generateFiles, bool.TrueString, StringComparison.OrdinalIgnoreCase);
    }
}