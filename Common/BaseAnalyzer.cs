using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DailyRoutines.CodeAnalysis.Common
{
    /// <summary>
    /// 分析器基类，简化新规则的创建过程
    /// </summary>
    public abstract class BaseAnalyzer : DiagnosticAnalyzer
    {
        /// <summary>
        /// 诊断描述符
        /// </summary>
        protected readonly DiagnosticDescriptor Descriptor;

        /// <summary>
        /// 创建新的分析器实例
        /// </summary>
        /// <param name="descriptor">诊断描述符</param>
        protected BaseAnalyzer(DiagnosticDescriptor descriptor)
        {
            Descriptor = descriptor;
        }

        /// <summary>
        /// 获取支持的诊断描述符
        /// </summary>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => 
            ImmutableArray.Create(Descriptor);

        /// <summary>
        /// 初始化分析器
        /// </summary>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            
            RegisterAnalyzers(context);
        }

        /// <summary>
        /// 在子类中实现以注册特定的分析逻辑
        /// </summary>
        protected abstract void RegisterAnalyzers(AnalysisContext context);

        /// <summary>
        /// 创建并报告诊断问题
        /// </summary>
        protected void ReportDiagnostic(SyntaxNodeAnalysisContext context, Location location, params object[] messageArgs)
        {
            var diagnostic = Diagnostic.Create(Descriptor, location, messageArgs);
            context.ReportDiagnostic(diagnostic);
        }
    }
} 