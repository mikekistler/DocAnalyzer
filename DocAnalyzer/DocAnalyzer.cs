using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Xml;

namespace DocAnalyzer;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class DocAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "DocAnalyzer";

    // You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
    // See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/Localizing%20Analyzers.md for more on localization
    private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
    private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
    private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
    private const string Category = "Naming";

    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        
        context.RegisterSyntaxNodeAction(AnalyzeXml, SyntaxKind.XmlElement);
    }

    private static void AnalyzeXml(SyntaxNodeAnalysisContext context)
    {
        var xmlElement = (XmlElementSyntax)context.Node;

        // If the node name is not "summary", return.
        if (xmlElement.StartTag.Name.LocalName.Text != "summary")
        {
            return;
        }

        // If the node has no XmlElements in its content, return.
        if (!xmlElement.Content.OfType<XmlElementSyntax>().Any())
        {
            return;
        }

        // Check each XmlElement in the content. If any is a "typeparam" element,
        // report a diagnostic.
        foreach (var element in xmlElement.Content.OfType<XmlElementSyntax>())
        {
            if (element.StartTag.Name.LocalName.Text == "typeparam")
            {
                var diagnostic = Diagnostic.Create(Rule, element.GetLocation(), element.StartTag.Name.LocalName.Text);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
  
}
