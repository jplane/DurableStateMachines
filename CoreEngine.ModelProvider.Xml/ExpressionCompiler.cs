using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CSharp.RuntimeBinder;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace CoreEngine.ModelProvider.Xml
{
    internal static class ExpressionCompiler
    {
        public static async Task<Func<dynamic, Task<T>>> Compile<T>(string expression)
        {
            expression.CheckArgNull(nameof(expression));

            var syntaxTree = LambdaRewriter.Rewrite<T>(expression);

            Debug.Assert(syntaxTree != null);

            syntaxTree = syntaxTree.WithRootAndOptions(syntaxTree.GetRoot(),
                                                       CSharpParseOptions.Default.WithKind(SourceCodeKind.Script));

            Debug.Assert(syntaxTree != null);

            var assemblyName = Path.GetRandomFileName();

            Debug.Assert(!string.IsNullOrWhiteSpace(assemblyName));

            var compileOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, usings: new[]
            {
                    "System",
                    "System.Collections",
                    "System.Collections.Generic",
                    "System.Dynamic",
                    "System.Linq",
                    "System.Threading.Tasks"
                });

            var references = new[]
            {
                    MetadataReference.CreateFromFile(Path.Combine(RuntimeEnvironment.GetRuntimeDirectory(), "netstandard.dll")),
                    MetadataReference.CreateFromFile(Path.Combine(RuntimeEnvironment.GetRuntimeDirectory(), "System.Runtime.dll")),
                    MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(IEnumerable<>).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(Task).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(DynamicObject).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(CSharpArgumentInfo).Assembly.Location)
                };

            var scriptCompilation = CSharpCompilation.CreateScriptCompilation(assemblyName,
                                                                              syntaxTree,
                                                                              references,
                                                                              compileOptions);

            Debug.Assert(scriptCompilation != null);

            var errorDiagnostics = scriptCompilation.GetDiagnostics().Where(x => x.Severity == DiagnosticSeverity.Error);

            if (errorDiagnostics.Any())
            {
                throw new ExpressionCompilerException(errorDiagnostics.Select(ed => ed.GetMessage()));
            }

            using (var peStream = new MemoryStream())
            {
                var emitResult = scriptCompilation.Emit(peStream);

                Debug.Assert(emitResult != null);

                if (emitResult.Success)
                {
                    return await ResolveFunction<T>(scriptCompilation, peStream);
                }
                else
                {
                    errorDiagnostics = emitResult.Diagnostics.Where(x => x.Severity == DiagnosticSeverity.Error);

                    throw new ExpressionCompilerException(errorDiagnostics.Select(ed => ed.GetMessage()));
                }
            }
        }

        private static async Task<Func<dynamic, Task<T>>> ResolveFunction<T>(CSharpCompilation scriptCompilation, MemoryStream peStream)
        {
            Debug.Assert(scriptCompilation != null);
            Debug.Assert(peStream != null);

            var assembly = Assembly.Load(peStream.ToArray());

            Debug.Assert(assembly != null);

            var entryPoint = scriptCompilation.GetEntryPoint(CancellationToken.None);

            Debug.Assert(entryPoint != null);

            var entryType = assembly.GetType($"{entryPoint.ContainingNamespace.MetadataName}.{entryPoint.ContainingType.MetadataName}");

            Debug.Assert(entryType != null);

            var entryMethod = entryType.GetMethod(entryPoint.MetadataName);

            Debug.Assert(entryMethod != null);

            var factory = (Func<object[], Task<object>>) entryMethod.CreateDelegate(typeof(Func<object[], Task<object>>));

            Debug.Assert(factory != null);

            var factoryTask = factory(new object[] { null, null });

            Debug.Assert(factoryTask != null);

            return (Func<dynamic, Task<T>>) await factoryTask;
        }

        private class LambdaRewriter : CSharpSyntaxRewriter
        {
            private readonly Type _returnType;
            private readonly ExpressionSyntax _expr;

            public static SyntaxTree Rewrite<T>(string expression)
            {
                expression.CheckArgNull(nameof(expression));

                var syntaxTree = CSharpSyntaxTree.ParseText(expression,
                                                            CSharpParseOptions.Default.WithKind(SourceCodeKind.Script));

                Debug.Assert(syntaxTree != null);

                var localsRewriter = new ReplaceLocalsRewriter();

                syntaxTree = localsRewriter.Visit(syntaxTree.GetRoot()).SyntaxTree;

                Debug.Assert(syntaxTree != null);

                var lambdaRewriter = new LambdaRewriter(syntaxTree, typeof(T));

                var expr = "(Func<dynamic, Task<RETURNTYPE>>)(async (dynamic data) => (RETURNTYPE) EXPR)";

                var targetSyntaxTree = CSharpSyntaxTree.ParseText(expr, CSharpParseOptions.Default.WithKind(SourceCodeKind.Script));

                Debug.Assert(targetSyntaxTree != null);

                return lambdaRewriter.Visit(targetSyntaxTree.GetRoot()).SyntaxTree;
            }

            private LambdaRewriter(SyntaxTree tree, Type returnType)
            {
                tree.CheckArgNull(nameof(tree));
                returnType.CheckArgNull(nameof(returnType));

                _returnType = returnType;

                _expr = tree.GetRoot().DescendantNodes()
                                      .OfType<ExpressionStatementSyntax>()
                                      .Single()
                                      .ChildNodes()
                                      .OfType<ExpressionSyntax>()
                                      .Single();
            }

            public override SyntaxNode VisitIdentifierName(IdentifierNameSyntax node)
            {
                Debug.Assert(node != null);
                Debug.Assert(_expr != null);
                Debug.Assert(_returnType != null);

                if (node.Identifier.ValueText == "EXPR")
                {
                    return _expr;
                }
                else if (node.Identifier.ValueText == "RETURNTYPE")
                {
                    return SyntaxFactory.IdentifierName(_returnType.Name);
                }

                return node;
            }
        }

        private class ReplaceLocalsRewriter : CSharpSyntaxRewriter
        {
            private readonly IdentifierNameSyntax _tempIdentifier;
            private readonly SyntaxNode _tempRoot;

            public ReplaceLocalsRewriter()
            {
                var expr = "(await data.__replace)";

                var syntaxTree = CSharpSyntaxTree.ParseText(expr, CSharpParseOptions.Default.WithKind(SourceCodeKind.Script));

                Debug.Assert(syntaxTree != null);

                _tempRoot = syntaxTree.GetRoot();

                _tempIdentifier = _tempRoot.DescendantNodes().OfType<IdentifierNameSyntax>()
                                                             .Single(ins => ins.Identifier.ValueText == "__replace");
            }

            private ExpressionSyntax GetUpdatedMemberAccess(IdentifierNameSyntax identifier)
            {
                Debug.Assert(identifier != null);
                Debug.Assert(_tempRoot != null);
                Debug.Assert(_tempIdentifier != null);

                var updatedRoot = _tempRoot.ReplaceNode(_tempIdentifier, identifier);

                return updatedRoot.DescendantNodes().OfType<ParenthesizedExpressionSyntax>().First();
            }

            public override SyntaxNode VisitIdentifierName(IdentifierNameSyntax node)
            {
                Debug.Assert(node != null);

                if (node.Parent is MemberAccessExpressionSyntax mae)
                {
                    if (mae.Name != node)
                    {
                        return GetUpdatedMemberAccess(node);
                    }
                    else
                    {
                        return node;
                    }
                }
                else
                {
                    return GetUpdatedMemberAccess(node);
                }
            }
        }
    }
}
