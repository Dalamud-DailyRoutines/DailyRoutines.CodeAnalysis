// 注意：此文件为模板，用于创建新规则时参考
// 使用方法：复制此模板并重命名，替换对应的占位符
// 规则类别请放在对应的文件夹下（Naming、Usage、Performance等）

using DailyRoutines.CodeAnalysis.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DailyRoutines.CodeAnalysis.Templates;

/// <summary>
///     分析器：[规则描述]
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ExampleRuleAnalyzer : BaseAnalyzer
{
    /// <summary>
    ///     规则诊断描述符 - 在DiagnosticRules.cs中定义
    ///     使用下面代码创建诊断描述符：
    ///     <code>
    /// public static readonly DiagnosticDescriptor ExampleRule = DiagnosticDescriptorFactory.Create(
    ///     id: "0003", // 替换为下一个可用的规则ID（4位数字）
    ///     title: "规则标题",
    ///     messageFormat: "发现问题的消息",
    ///     category: DiagnosticCategories.Usage, // 使用已定义的类别
    ///     description: "详细的规则描述。"
    /// );
    /// </code>
    /// </summary>

    // 构造函数接收诊断描述符
    public ExampleRuleAnalyzer()
        : base(DiagnosticRules.ExampleRule) // 使用DiagnosticRules中定义的规则
    {
    }

    protected override void RegisterAnalyzers(AnalysisContext context)
    {
        // 注册分析器处理的语法节点类型
        context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.MethodDeclaration); // 示例：分析方法声明
    }

    private void AnalyzeNode(SyntaxNodeAnalysisContext context)
    {
        // 1. 获取需要分析的语法节点
        // var node = context.Node;

        // 2. 检查节点是否符合规则要求
        // 示例：if (node is MethodDeclarationSyntax methodDecl && methodDecl.Identifier.Text.Contains("Example"))

        // 3. 如果发现问题，报告诊断
        // ReportDiagnostic(context, node.GetLocation());
    }
}
