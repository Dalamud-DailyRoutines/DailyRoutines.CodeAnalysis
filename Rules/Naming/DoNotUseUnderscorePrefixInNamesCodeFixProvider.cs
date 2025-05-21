using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Rename;
using DailyRoutines.CodeAnalysis.Common;

namespace DailyRoutines.CodeAnalysis.Rules.Naming;

/// <summary>
/// 代码修复提供程序：移除下划线前缀
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(DoNotUseUnderscorePrefixInNamesCodeFixProvider)), Shared]
public class DoNotUseUnderscorePrefixInNamesCodeFixProvider : BaseCodeFixProvider
{
    protected override ImmutableArray<string> GetFixableDiagnosticIds() =>
        ImmutableArray.Create(DiagnosticRules.DoNotUseUnderscorePrefixInNames.Id);

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root == null) return;

        var model = await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);
        if (model == null) return;

        var diagnostic     = context.Diagnostics.First();
        var diagnosticSpan = diagnostic.Location.SourceSpan;

        // 查找包含下划线前缀的标识符
        var node = root.FindNode(diagnosticSpan);
        if (node == null) return;

        // 获取符号信息
        var symbol = model.GetDeclaredSymbol(node.Parent, context.CancellationToken);
        if (symbol == null) return;

        // 准备名称转换
        var oldName = symbol.Name;
        var newName = oldName.TrimStart('_');

        if (string.IsNullOrEmpty(newName) || oldName == newName) return;

        // 确保新名称遵循当前命名约定（如变量名首字母小写等）
        if (char.IsUpper(newName[0]) && symbol.Kind is SymbolKind.Local or SymbolKind.Field or SymbolKind.Parameter)
            newName = char.ToLowerInvariant(newName[0]) + newName.Substring(1);

        context.RegisterCodeFix(
            CodeAction.Create(
                title: $"移除下划线前缀 '{oldName}' -> '{newName}'",
                createChangedSolution: c => RenameSymbolAsync(context.Document, symbol, newName, c),
                equivalenceKey: nameof(DoNotUseUnderscorePrefixInNamesCodeFixProvider)),
            diagnostic);
    }

    private static async Task<Solution> RenameSymbolAsync(Document document, ISymbol symbol, string newName, CancellationToken cancellationToken)
    {
        var solution = document.Project.Solution;
        var options  = solution.Workspace.Options;

        // 使用工作区选项而不是RenameOptions
        return await Renamer.RenameSymbolAsync(solution, symbol, newName, options, cancellationToken).ConfigureAwait(false);
    }
}
