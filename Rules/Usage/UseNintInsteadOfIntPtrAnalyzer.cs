using DailyRoutines.CodeAnalysis.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DailyRoutines.CodeAnalysis.Rules.Usage;

/// <summary>
///     分析器：使用nint代替IntPtr
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class UseNintInsteadOfIntPtrAnalyzer() : BaseAnalyzer(DiagnosticRules.UseNintInsteadOfIntPtr)
{
    protected override void RegisterAnalyzers(AnalysisContext context)
    {
        // 检查变量声明
        context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.IdentifierName);
        context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.QualifiedName);
    }

    private void AnalyzeNode(SyntaxNodeAnalysisContext context)
    {
        var typeNode = context.Node;
        var typeName = string.Empty;

        switch (typeNode)
        {
            case IdentifierNameSyntax identifierName:
                typeName = identifierName.Identifier.Text;
                break;
            case QualifiedNameSyntax qualifiedName:
            {
                // 处理带命名空间的类型名称，如 System.IntPtr
                if (qualifiedName.Right.Identifier.Text == "IntPtr") typeName = "IntPtr";
                break;
            }
        }

        if (typeName == "IntPtr") 
            ReportDiagnostic(context, typeNode.GetLocation());
    }
}
