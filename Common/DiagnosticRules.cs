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
        /// 诊断规则：配置类中不允许使用 readonly 字段
        /// </summary>
        public static readonly DiagnosticDescriptor ConfigurationClassesMustNotUseReadonlyFields = DiagnosticDescriptorFactory.Create(
            id: "0008",
            title: "配置类中不允许使用 readonly 字段",
            messageFormat: "继承自{0}的类中不应使用 readonly 修饰符",
            category: DiagnosticCategories.Design,
            defaultSeverity: DiagnosticSeverity.Warning,
            description: "为了保持配置类的一致性和可扩展性，继承自 ManagerConfiguration 或 ModuleConfiguration 的类中不应使用 readonly 修饰符。"
        );
        
        /// <summary>
        /// 诊断规则：英文缩写大小写必须保持一致
        /// </summary>
        public static readonly DiagnosticDescriptor AcronymCasingConsistency = DiagnosticDescriptorFactory.Create(
            id: "0009",
            title: "英文缩写大小写必须保持一致",
            messageFormat: "缩写 '{0}' 应保持大小写一致，建议使用 '{1}' 或 '{2}'",
            category: DiagnosticCategories.Naming,
            defaultSeverity: DiagnosticSeverity.Warning,
            description: "为了保持代码风格一致性，英文缩写（如ID、NPC、API等）应始终保持所有字母全大写或全小写。"
        );
    }
}
