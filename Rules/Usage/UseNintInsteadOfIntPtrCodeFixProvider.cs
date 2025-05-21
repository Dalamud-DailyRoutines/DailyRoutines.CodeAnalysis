using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using DailyRoutines.CodeAnalysis.Common;

namespace DailyRoutines.CodeAnalysis.Rules.Usage
{
    /// <summary>
    /// 代码修复提供程序：将IntPtr替换为nint
    /// </summary>
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseNintInsteadOfIntPtrCodeFixProvider)), Shared]
    public class UseNintInsteadOfIntPtrCodeFixProvider : BaseCodeFixProvider
    {
        protected override ImmutableArray<string> GetFixableDiagnosticIds() =>
            ImmutableArray.Create(DiagnosticRules.UseNintInsteadOfIntPtr.Id);

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            if (root == null) return;

            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            // 查找需要修复的IntPtr标识符
            var typeNode = root.FindNode(diagnosticSpan);
            if (typeNode == null) return;

            // 注册代码修复
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "使用nint代替IntPtr",
                    createChangedDocument: c => ReplaceWithNintAsync(context.Document, typeNode, c),
                    equivalenceKey: nameof(UseNintInsteadOfIntPtrCodeFixProvider)),
                diagnostic);
        }

        private static async Task<Document> ReplaceWithNintAsync(Document document, SyntaxNode typeNode, CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            if (root == null) return document;

            SyntaxNode? newNode = null;

            switch (typeNode)
            {
                case IdentifierNameSyntax:
                    newNode = SyntaxFactory.IdentifierName("nint");
                    break;
                case QualifiedNameSyntax qualifiedName:
                {
                    // 只有当右侧是IntPtr时才替换
                    if (qualifiedName.Right.Identifier.Text == "IntPtr")
                        newNode = SyntaxFactory.IdentifierName("nint");

                    break;
                }
            }

            if (newNode == null) return document;

            // 确保非空
            var newRoot = root.ReplaceNode(typeNode, newNode);
            return document.WithSyntaxRoot(newRoot);
        }
    }
}
