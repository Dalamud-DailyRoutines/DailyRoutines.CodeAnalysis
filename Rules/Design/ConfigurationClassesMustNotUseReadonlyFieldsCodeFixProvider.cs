using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DailyRoutines.CodeAnalysis.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DailyRoutines.CodeAnalysis.Rules.Design;

/// <summary>
///     代码修复提供程序：移除配置类中的readonly修饰符
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ConfigurationClassesMustNotUseReadonlyFieldsCodeFixProvider)), Shared]
public class ConfigurationClassesMustNotUseReadonlyFieldsCodeFixProvider : BaseCodeFixProvider
{
    protected override ImmutableArray<string> GetFixableDiagnosticIds() =>
        ImmutableArray.Create(DiagnosticRules.ConfigurationClassesMustNotUseReadonlyFields.Id);

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root == null) return;

        var diagnostic = context.Diagnostics.First();
        var diagnosticSpan = diagnostic.Location.SourceSpan;

        // 找到需要修复的变量声明节点
        var variableDeclarator = root.FindNode(diagnosticSpan) as VariableDeclaratorSyntax;
        if (variableDeclarator == null) return;

        // 获取字段声明节点
        var fieldDeclaration = variableDeclarator.Parent?.Parent as FieldDeclarationSyntax;
        if (fieldDeclaration == null) return;

        // 注册代码修复
        context.RegisterCodeFix(
            CodeAction.Create(
                "移除readonly修饰符",
                c => RemoveReadonlyModifierAsync(context.Document, fieldDeclaration, c),
                nameof(ConfigurationClassesMustNotUseReadonlyFieldsCodeFixProvider)),
            diagnostic);
    }

    private static async Task<Document> RemoveReadonlyModifierAsync(Document document, FieldDeclarationSyntax fieldDeclaration, CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root == null) return document;

        // 创建新的修饰符列表，移除readonly修饰符
        var newModifiers = fieldDeclaration.Modifiers.Where(m => !m.IsKind(SyntaxKind.ReadOnlyKeyword));
        var newFieldDeclaration = fieldDeclaration.WithModifiers(SyntaxFactory.TokenList(newModifiers));

        // 替换节点
        var newRoot = root.ReplaceNode(fieldDeclaration, newFieldDeclaration);

        // 返回更新后的文档
        return document.WithSyntaxRoot(newRoot);
    }
}