using Microsoft.CodeAnalysis;

namespace DailyRoutines.CodeAnalysis.Common
{
    /// <summary>
    /// 包含所有诊断规则描述符的中央存储
    /// </summary>
    public static class DiagnosticRules
    {
        /// <summary>
        /// 诊断规则：使用nint代替IntPtr
        /// </summary>
        public static readonly DiagnosticDescriptor UseNintInsteadOfIntPtr = DiagnosticDescriptorFactory.Create(
            id: "0001",
            title: "使用nint代替IntPtr",
            messageFormat: "应使用'nint'代替'IntPtr'",
            category: DiagnosticCategories.Usage,
            defaultSeverity: DiagnosticSeverity.Warning,
            description: "使用nint类型代替IntPtr类型以提高代码一致性。"
        );

        /// <summary>
        /// 诊断规则：不要在命名中使用下划线前缀
        /// </summary>
        public static readonly DiagnosticDescriptor DoNotUseUnderscorePrefixInNames = DiagnosticDescriptorFactory.Create(
            id: "0002",
            title: "不要在命名中使用下划线前缀",
            messageFormat: "命名不应以下划线开头",
            category: DiagnosticCategories.Naming,
            defaultSeverity: DiagnosticSeverity.Warning,
            description: "为了保持代码风格一致性，命名不应以下划线开头。"
        );
        
        /// <summary>
        /// 诊断规则：控制语句不允许在同一行写语句体
        /// </summary>
        public static readonly DiagnosticDescriptor ControlStatementBodyMustBeOnNewLine = DiagnosticDescriptorFactory.Create(
            id: "0004",
            title: "控制语句的语句体必须另起一行",
            messageFormat: "{0}语句的语句体必须另起一行",
            category: DiagnosticCategories.Design,
            defaultSeverity: DiagnosticSeverity.Warning,
            description: "为了提高代码可读性，控制语句(如if、foreach等)的语句体必须另起一行。"
        );
        
        /// <summary>
        /// 诊断规则：只有一行代码的控制语句不允许使用大括号
        /// </summary>
        public static readonly DiagnosticDescriptor SingleLineControlStatementMustNotUseBlock = DiagnosticDescriptorFactory.Create(
            id: "0005",
            title: "只有一行代码的控制语句不应使用大括号",
            messageFormat: "只有一行代码的{0}语句不应使用大括号",
            category: DiagnosticCategories.Design,
            defaultSeverity: DiagnosticSeverity.Warning,
            description: "为了保持代码风格一致性，只有一行代码的控制语句(如if、foreach等)不应使用大括号。"
        );
        
        /// <summary>
        /// 诊断规则：多行代码的控制语句必须使用大括号
        /// </summary>
        public static readonly DiagnosticDescriptor MultiLineControlStatementMustUseBlock = DiagnosticDescriptorFactory.Create(
            id: "0006",
            title: "多行代码的控制语句必须使用大括号",
            messageFormat: "多行代码的{0}语句必须使用大括号",
            category: DiagnosticCategories.Design,
            defaultSeverity: DiagnosticSeverity.Warning,
            description: "为了保持代码结构清晰，多行代码或包含嵌套控制语句的控制语句(如if、foreach等)必须使用大括号。"
        );
        
        /// <summary>
        /// 示例诊断规则 - 仅用于模板演示
        /// </summary>
        public static readonly DiagnosticDescriptor ExampleRule = DiagnosticDescriptorFactory.Create(
            id: "0003",
            title: "示例规则",
            messageFormat: "这是一个示例规则，用于演示如何创建新规则",
            category: DiagnosticCategories.Design,
            defaultSeverity: DiagnosticSeverity.Warning,
            description: "这是一个示例规则，用于模板演示目的。当需要添加新规则时，复制并修改此模式。"
        );
        
        /// <summary>
        /// 诊断规则：二元运算符必须在行尾而不是行首
        /// </summary>
        public static readonly DiagnosticDescriptor BinaryOperatorsMustBeAtEndOfLine = DiagnosticDescriptorFactory.Create(
            id: "0007",
            title: "二元运算符必须在行尾而不是行首",
            messageFormat: "二元运算符 '{0}' 应放在行尾而不是行首",
            category: DiagnosticCategories.Design,
            defaultSeverity: DiagnosticSeverity.Warning,
            description: "为了保持代码风格一致性，二元运算符（如 &&, ||, +, -, *, /, |, &）应放在行尾而不是行首。"
        );
    }
} 
