// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#nullable disable

using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp.Test.Utilities;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Test.Utilities;
using Xunit;
using System;
using System.Threading;
using AwesomeAssertions;

namespace Microsoft.CodeAnalysis.CSharp.UnitTests
{
    public class XmlDocCommentTests : CSharpTestBase
    {
        private CSharpParseOptions GetOptions(string[] defines)
        {
            return new CSharpParseOptions(
                languageVersion: LanguageVersion.CSharp3,
                documentationMode: DocumentationMode.Diagnose,
                preprocessorSymbols: defines);
        }

        private SyntaxTree Parse(string text, params string[] defines)
        {
            var options = this.GetOptions(defines);
            var itext = SourceText.From(text);
            return SyntaxFactory.ParseSyntaxTree(itext, options);
        }

        [Fact]
        public void DocCommentWriteException()
        {
            var comp = CreateCompilation(@"
/// <summary>
/// Doc comment for <see href=""C"" />
/// </summary>
public class C
{
    /// <summary>
    /// Doc comment for method M
    /// </summary>
    public void M() { }
}");
            using (new EnsureEnglishUICulture())
            {
                var diags = new DiagnosticBag();
                var badStream = new BrokenStream();
                badStream.BreakHow = BrokenStream.BreakHowType.ThrowOnWrite;

                DocumentationCommentCompiler.WriteDocumentationCommentXml(
                    comp,
                    null,
                    badStream,
                    new BindingDiagnosticBag(diags),
                    default(CancellationToken));

                diags.Verify(
                    // error CS1569: Error writing to XML documentation file: I/O error occurred.
                    Diagnostic(ErrorCode.ERR_DocFileGen).WithArguments("I/O error occurred.").WithLocation(1, 1));
            }
        }

        [ClrOnlyFact]
        public void TestEmptyElementNoAttributes()
        {
            var text = "/// <goo />";
            var tree = Parse(text);
            tree.Should().NotBeNull();
            tree.GetCompilationUnitRoot().ToFullString().Should().Be(text);
            tree.GetCompilationUnitRoot().Errors().Length.Should().Be(0);
            var leading = tree.GetCompilationUnitRoot().EndOfFileToken.LeadingTrivia;
            leading.Count.Should().Be(1);
            var node = leading[0];
            node.Kind().Should().Be(SyntaxKind.SingleLineDocumentationCommentTrivia);
            node.ToFullString().Should().Be(text);
            var doc = (DocumentationCommentTriviaSyntax)node.GetStructure();
            doc.Content.Count.Should().Be(2);
            doc.Content[0].Kind().Should().Be(SyntaxKind.XmlText);
            doc.Content[1].Kind().Should().Be(SyntaxKind.XmlEmptyElement);
        }

        [WorkItem(537500, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/537500")]
        [Fact]
        public void TestFourOrMoreSlashesIsNotXmlComment()
        {
            var text = "//// <goo />";
            var tree = Parse(text);
            tree.Should().NotBeNull();
            tree.GetCompilationUnitRoot().ToFullString().Should().Be(text);
            tree.GetCompilationUnitRoot().Errors().Length.Should().Be(0);
            var leading = tree.GetCompilationUnitRoot().EndOfFileToken.LeadingTrivia;
            leading.Count.Should().Be(1);
            leading[0].Kind().Should().Be(SyntaxKind.SingleLineCommentTrivia);
            leading[0].ToFullString().Should().Be(text);
        }

        [WorkItem(537500, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/537500")]
        [Fact]
        public void TestFourOrMoreSlashesInsideXmlCommentIsNotXmlComment()
        {
            var text = @"/// <goo>
//// </goo>
";
            var tree = Parse(text);
            tree.Should().NotBeNull();
            tree.GetCompilationUnitRoot().ToFullString().Should().Be(text);
            tree.GetCompilationUnitRoot().ErrorsAndWarnings().Length.Should().NotBe(0);
            var leading = tree.GetCompilationUnitRoot().EndOfFileToken.LeadingTrivia;
            leading.Count.Should().Be(3);
            leading[0].Kind().Should().Be(SyntaxKind.SingleLineDocumentationCommentTrivia);
            leading[1].Kind().Should().Be(SyntaxKind.SingleLineCommentTrivia);
        }

        [WorkItem(537500, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/537500")]
        [Fact]
        public void TestThreeOrMoreAsterisksIsNotXmlComment()
        {
            var text = "/*** <goo /> */";
            var tree = Parse(text);
            tree.Should().NotBeNull();
            tree.GetCompilationUnitRoot().ToFullString().Should().Be(text);
            tree.GetCompilationUnitRoot().Errors().Length.Should().Be(0);
            var leading = tree.GetCompilationUnitRoot().EndOfFileToken.LeadingTrivia;
            leading.Count.Should().Be(1);
            leading[0].Kind().Should().Be(SyntaxKind.MultiLineCommentTrivia);
            leading[0].ToFullString().Should().Be(text);
        }

        [Fact]
        public void TestEmptyElementNoAttributesPrecedingClass()
        {
            var text =
@"/// <goo />
class C { }";
            var tree = Parse(text);
            tree.Should().NotBeNull();
            tree.GetCompilationUnitRoot().ToFullString().Should().Be(text);
            tree.GetCompilationUnitRoot().Errors().Length.Should().Be(0);
            tree.GetCompilationUnitRoot().Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var leading = tree.GetCompilationUnitRoot().Members[0].GetLeadingTrivia();
            leading.Count.Should().Be(1);
            var node = leading[0];
            node.Kind().Should().Be(SyntaxKind.SingleLineDocumentationCommentTrivia);
            node.ToFullString().Should().Be($"/// <goo />{Environment.NewLine}");
            var doc = (DocumentationCommentTriviaSyntax)node.GetStructure();
            doc.Content.Count.Should().Be(3);
            doc.Content[0].Kind().Should().Be(SyntaxKind.XmlText);
            doc.Content[0].HasLeadingTrivia.Should().BeTrue();
            doc.Content[1].Kind().Should().Be(SyntaxKind.XmlEmptyElement);
            doc.Content[2].Kind().Should().Be(SyntaxKind.XmlText);
        }

        [Fact]
        public void TestEmptyElementNoAttributesDelimited()
        {
            var text = "/** <goo /> */";
            var tree = Parse(text);
            tree.Should().NotBeNull();
            tree.GetCompilationUnitRoot().ToFullString().Should().Be(text);
            tree.GetCompilationUnitRoot().Errors().Length.Should().Be(0);
            var leading = tree.GetCompilationUnitRoot().EndOfFileToken.LeadingTrivia;
            leading.Count.Should().Be(1);
            var node = leading[0];
            node.Kind().Should().Be(SyntaxKind.MultiLineDocumentationCommentTrivia);
            node.ToFullString().Should().Be(text);
            var doc = (DocumentationCommentTriviaSyntax)node.GetStructure();
            doc.Content.Count.Should().Be(3);
            doc.Content[0].Kind().Should().Be(SyntaxKind.XmlText);
            doc.Content[0].HasLeadingTrivia.Should().BeTrue();
            doc.Content[1].Kind().Should().Be(SyntaxKind.XmlEmptyElement);
            doc.Content[2].Kind().Should().Be(SyntaxKind.XmlText);
        }

        [Fact]
        public void TestEmptyElementNoAttributesDelimitedPrecedingClass()
        {
            var text =
@"/** <goo /> */
class C { }";

            var tree = Parse(text);
            tree.Should().NotBeNull();
            tree.GetCompilationUnitRoot().ToFullString().Should().Be(text);
            tree.GetCompilationUnitRoot().Errors().Length.Should().Be(0);
            tree.GetCompilationUnitRoot().Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var leading = tree.GetCompilationUnitRoot().Members[0].GetLeadingTrivia();
            leading.Count.Should().Be(2); // a new line follows the comment
            var node = leading[0];
            node.Kind().Should().Be(SyntaxKind.MultiLineDocumentationCommentTrivia);
            node.ToFullString().Should().Be("/** <goo /> */");
            var doc = (DocumentationCommentTriviaSyntax)node.GetStructure();
            doc.Content.Count.Should().Be(3);
            doc.Content[0].Kind().Should().Be(SyntaxKind.XmlText);
            doc.Content[0].HasLeadingTrivia.Should().BeTrue();
            doc.Content[1].Kind().Should().Be(SyntaxKind.XmlEmptyElement);
            doc.Content[2].Kind().Should().Be(SyntaxKind.XmlText);
        }

        [Fact]
        public void TestEmptyElementWithAttributes()
        {
            var text =
@"/// <goo a=""xyz""/>";
            var tree = Parse(text);
            tree.Should().NotBeNull();
            tree.GetCompilationUnitRoot().ToFullString().Should().Be(text);
            tree.GetCompilationUnitRoot().Errors().Length.Should().Be(0);
            var leading = tree.GetCompilationUnitRoot().EndOfFileToken.GetLeadingTrivia();
            leading.Count.Should().Be(1);
            var node = leading[0];
            node.Kind().Should().Be(SyntaxKind.SingleLineDocumentationCommentTrivia);
            node.ToFullString().Should().Be(text);
            var doc = (DocumentationCommentTriviaSyntax)node.GetStructure();
            doc.Content.Count.Should().Be(2);
            doc.Content[0].Kind().Should().Be(SyntaxKind.XmlText);
            doc.Content[0].HasLeadingTrivia.Should().BeTrue();
            doc.Content[1].Kind().Should().Be(SyntaxKind.XmlEmptyElement);
            var element = (XmlEmptyElementSyntax)doc.Content[1];
            element.Attributes.Count.Should().Be(1);
        }

        [Fact]
        public void TestEmptyElementWithAttributesSingleQuoted()
        {
            var text =
@"/// <goo a='xyz'/>";
            var tree = Parse(text);
            tree.Should().NotBeNull();
            tree.GetCompilationUnitRoot().ToFullString().Should().Be(text);
            tree.GetCompilationUnitRoot().Errors().Length.Should().Be(0);
            var leading = tree.GetCompilationUnitRoot().EndOfFileToken.GetLeadingTrivia();
            leading.Count.Should().Be(1);
            var node = leading[0];
            node.Kind().Should().Be(SyntaxKind.SingleLineDocumentationCommentTrivia);
            node.ToFullString().Should().Be(text);
            var doc = (DocumentationCommentTriviaSyntax)node.GetStructure();
            doc.Content.Count.Should().Be(2);
            doc.Content[0].Kind().Should().Be(SyntaxKind.XmlText);
            doc.Content[0].HasLeadingTrivia.Should().BeTrue();
            doc.Content[1].Kind().Should().Be(SyntaxKind.XmlEmptyElement);
            var element = (XmlEmptyElementSyntax)doc.Content[1];
            element.Attributes.Count.Should().Be(1);
        }

        [Fact]
        public void TestEmptyElementWithAttributesNestedQuote()
        {
            var text =
@"/// <goo a=""x'y'z""/>";
            var tree = Parse(text);
            tree.Should().NotBeNull();
            tree.GetCompilationUnitRoot().ToFullString().Should().Be(text);
            tree.GetCompilationUnitRoot().Errors().Length.Should().Be(0);
            var leading = tree.GetCompilationUnitRoot().EndOfFileToken.GetLeadingTrivia();
            leading.Count.Should().Be(1);
            var node = leading[0];
            node.Kind().Should().Be(SyntaxKind.SingleLineDocumentationCommentTrivia);
            node.ToFullString().Should().Be(text);
            var doc = (DocumentationCommentTriviaSyntax)node.GetStructure();
            doc.Content.Count.Should().Be(2);
            doc.Content[0].Kind().Should().Be(SyntaxKind.XmlText);
            doc.Content[0].HasLeadingTrivia.Should().BeTrue();
            doc.Content[1].Kind().Should().Be(SyntaxKind.XmlEmptyElement);
            var element = (XmlEmptyElementSyntax)doc.Content[1];
            element.Attributes.Count.Should().Be(1);
            element.Attributes[0].Kind().Should().Be(SyntaxKind.XmlTextAttribute);
            var attr = (XmlTextAttributeSyntax)element.Attributes[0];
            attr.TextTokens.Count.Should().Be(1);
            attr.TextTokens[0].ToString().Should().Be("x'y'z");
        }

        [Fact]
        public void TestEmptyElementWithAttributesNestedQuoteSingleQuoted()
        {
            var text =
@"/// <goo a='x""y""z'/>";
            var tree = Parse(text);
            tree.Should().NotBeNull();
            tree.GetCompilationUnitRoot().ToFullString().Should().Be(text);
            tree.GetCompilationUnitRoot().Errors().Length.Should().Be(0);
            var leading = tree.GetCompilationUnitRoot().EndOfFileToken.GetLeadingTrivia();
            leading.Count.Should().Be(1);
            var node = leading[0];
            node.Kind().Should().Be(SyntaxKind.SingleLineDocumentationCommentTrivia);
            node.ToFullString().Should().Be(text);
            var doc = (DocumentationCommentTriviaSyntax)node.GetStructure();
            doc.Content.Count.Should().Be(2);
            doc.Content[0].Kind().Should().Be(SyntaxKind.XmlText);
            doc.Content[0].HasLeadingTrivia.Should().BeTrue();
            doc.Content[1].Kind().Should().Be(SyntaxKind.XmlEmptyElement);
            var element = (XmlEmptyElementSyntax)doc.Content[1];
            element.Attributes.Count.Should().Be(1);
            element.Attributes[0].Kind().Should().Be(SyntaxKind.XmlTextAttribute);
            var attr = (XmlTextAttributeSyntax)element.Attributes[0];
            attr.TextTokens.Count.Should().Be(1);
            attr.TextTokens[0].ToString().Should().Be("x\"y\"z");
        }

        [Fact]
        public void TestEmptyElementNoAttributesMultipleLines()
        {
            var text =
@"/// <goo 
/// />";
            var tree = Parse(text);
            tree.Should().NotBeNull();
            tree.GetCompilationUnitRoot().ToFullString().Should().Be(text);
            tree.GetCompilationUnitRoot().Errors().Length.Should().Be(0);
            var leading = tree.GetCompilationUnitRoot().EndOfFileToken.GetLeadingTrivia();
            leading.Count.Should().Be(1);
            var node = leading[0];
            node.Kind().Should().Be(SyntaxKind.SingleLineDocumentationCommentTrivia);
            node.ToFullString().Should().Be(text);
            var doc = (DocumentationCommentTriviaSyntax)node.GetStructure();
            doc.Content.Count.Should().Be(2);
            doc.Content[0].Kind().Should().Be(SyntaxKind.XmlText);
            doc.Content[0].HasLeadingTrivia.Should().BeTrue();
            doc.Content[1].Kind().Should().Be(SyntaxKind.XmlEmptyElement);
            doc.Content[1].ToFullString().Should().Be($"<goo {Environment.NewLine}/// />");
        }

        [Fact]
        public void TestEmptyElementNoAttributesMultipleLinesPrecedingClass()
        {
            var text =
@"/// <goo 
/// />
class C { }";
            var tree = Parse(text);
            tree.Should().NotBeNull();
            tree.GetCompilationUnitRoot().ToFullString().Should().Be(text);
            tree.GetCompilationUnitRoot().Errors().Length.Should().Be(0);
            var leading = tree.GetCompilationUnitRoot().Members[0].GetLeadingTrivia();
            leading.Count.Should().Be(1);
            var node = leading[0];
            node.Kind().Should().Be(SyntaxKind.SingleLineDocumentationCommentTrivia);
            node.ToFullString().Should().Be($"/// <goo {Environment.NewLine}/// />{Environment.NewLine}");
            var doc = (DocumentationCommentTriviaSyntax)node.GetStructure();
            doc.Content.Count.Should().Be(3);
            doc.Content[0].Kind().Should().Be(SyntaxKind.XmlText);
            doc.Content[0].HasLeadingTrivia.Should().BeTrue();
            doc.Content[1].Kind().Should().Be(SyntaxKind.XmlEmptyElement);
            doc.Content[1].ToFullString().Should().Be($"<goo {Environment.NewLine}/// />");
            doc.Content[2].Kind().Should().Be(SyntaxKind.XmlText);
        }

        [Fact]
        public void TestEmptyElementNoAttributesMultipleLinesDelimited()
        {
            var text =
@"/** <goo 
  * />
  */";
            var tree = Parse(text);
            tree.Should().NotBeNull();
            tree.GetCompilationUnitRoot().ToFullString().Should().Be(text);
            tree.GetCompilationUnitRoot().Errors().Length.Should().Be(0);
            var leading = tree.GetCompilationUnitRoot().EndOfFileToken.GetLeadingTrivia();
            leading.Count.Should().Be(1);
            var node = leading[0];
            node.Kind().Should().Be(SyntaxKind.MultiLineDocumentationCommentTrivia);
            node.ToFullString().Should().Be(text);
            var doc = (DocumentationCommentTriviaSyntax)node.GetStructure();
            doc.Content.Count.Should().Be(3);
            doc.Content[0].Kind().Should().Be(SyntaxKind.XmlText);
            doc.Content[0].HasLeadingTrivia.Should().BeTrue();
            doc.Content[1].Kind().Should().Be(SyntaxKind.XmlEmptyElement);
            doc.Content[1].ToFullString().Should().Be($"<goo {Environment.NewLine}  * />");
            doc.Content[2].Kind().Should().Be(SyntaxKind.XmlText);
        }

        [ClrOnlyFact]
        public void TestEmptyElementNoAttributesMultipleLinesDelimitedPrecedingClass()
        {
            var text =
@"/** <goo 
  * />
  */
class C { }";
            var tree = Parse(text);
            tree.Should().NotBeNull();
            tree.GetCompilationUnitRoot().ToFullString().Should().Be(text);
            tree.GetCompilationUnitRoot().Errors().Length.Should().Be(0);
            var leading = tree.GetCompilationUnitRoot().Members[0].GetLeadingTrivia();
            leading.Count.Should().Be(2);
            var node = leading[0];
            node.Kind().Should().Be(SyntaxKind.MultiLineDocumentationCommentTrivia);
            node.ToFullString().Should().Be($"/** <goo {Environment.NewLine}  * />{Environment.NewLine}  */");
            var doc = (DocumentationCommentTriviaSyntax)node.GetStructure();
            doc.Content.Count.Should().Be(3);
            doc.Content[0].Kind().Should().Be(SyntaxKind.XmlText);
            doc.Content[0].HasLeadingTrivia.Should().BeTrue();
            doc.Content[1].Kind().Should().Be(SyntaxKind.XmlEmptyElement);
            doc.Content[1].ToFullString().Should().Be($"<goo {Environment.NewLine}  * />");
            doc.Content[2].Kind().Should().Be(SyntaxKind.XmlText);
        }

        [Fact]
        public void TestEmptyElementWithAttributesDoubleQuoteMultipleLines()
        {
            var text =
@"/// <goo 
/// a
/// =
/// ""xyz""
/// />";
            var tree = Parse(text);
            tree.Should().NotBeNull();
            tree.GetCompilationUnitRoot().ToFullString().Should().Be(text);
            tree.GetCompilationUnitRoot().Errors().Length.Should().Be(0);
            var leading = tree.GetCompilationUnitRoot().EndOfFileToken.GetLeadingTrivia();
            leading.Count.Should().Be(1);
            var node = leading[0];
            node.Kind().Should().Be(SyntaxKind.SingleLineDocumentationCommentTrivia);
            node.ToFullString().Should().Be(text);
            var doc = (DocumentationCommentTriviaSyntax)node.GetStructure();
            doc.Content.Count.Should().Be(2);
            doc.Content[0].Kind().Should().Be(SyntaxKind.XmlText);
            doc.Content[0].HasLeadingTrivia.Should().BeTrue();
            doc.Content[1].Kind().Should().Be(SyntaxKind.XmlEmptyElement);
            var element = (XmlEmptyElementSyntax)doc.Content[1];
            element.Attributes.Count.Should().Be(1);
        }

        [Fact]
        public void TestEmptyElementWithAttributesQuoteMultipleLines()
        {
            var text =
@"/// <goo 
/// a
/// =
/// 'xyz'
/// />";
            var tree = Parse(text);
            tree.Should().NotBeNull();
            tree.GetCompilationUnitRoot().ToFullString().Should().Be(text);
            tree.GetCompilationUnitRoot().Errors().Length.Should().Be(0);
            var leading = tree.GetCompilationUnitRoot().EndOfFileToken.GetLeadingTrivia();
            leading.Count.Should().Be(1);
            var node = leading[0];
            node.Kind().Should().Be(SyntaxKind.SingleLineDocumentationCommentTrivia);
            node.ToFullString().Should().Be(text);
            var doc = (DocumentationCommentTriviaSyntax)node.GetStructure();
            doc.Content.Count.Should().Be(2);
            doc.Content[0].Kind().Should().Be(SyntaxKind.XmlText);
            doc.Content[0].HasLeadingTrivia.Should().BeTrue();
            doc.Content[1].Kind().Should().Be(SyntaxKind.XmlEmptyElement);
            var element = (XmlEmptyElementSyntax)doc.Content[1];
            element.Attributes.Count.Should().Be(1);
        }

        [Fact]
        public void TestEmptyElementWithAttributesQuoteMultipleLinesDelimited()
        {
            var text =
@"/** <goo 
  * a
  * =
  * 'xyz'
  * />
  */";
            var tree = Parse(text);
            tree.Should().NotBeNull();
            tree.GetCompilationUnitRoot().ToFullString().Should().Be(text);
            tree.GetCompilationUnitRoot().Errors().Length.Should().Be(0);
            var leading = tree.GetCompilationUnitRoot().EndOfFileToken.GetLeadingTrivia();
            leading.Count.Should().Be(1);
            var node = leading[0];
            node.Kind().Should().Be(SyntaxKind.MultiLineDocumentationCommentTrivia);
            node.ToFullString().Should().Be(text);
            var doc = (DocumentationCommentTriviaSyntax)node.GetStructure();
            doc.Content.Count.Should().Be(3);
            doc.Content[0].Kind().Should().Be(SyntaxKind.XmlText);
            doc.Content[0].HasLeadingTrivia.Should().BeTrue();
            doc.Content[1].Kind().Should().Be(SyntaxKind.XmlEmptyElement);
            var element = (XmlEmptyElementSyntax)doc.Content[1];
            element.Attributes.Count.Should().Be(1);
            doc.Content[2].Kind().Should().Be(SyntaxKind.XmlText);
        }

        [Fact]
        public void TestEmptyElementWithAttributesDoubleQuoteMultipleLinesDelimited()
        {
            var text =
@"/** <goo 
  * a
  * =
  * ""xyz""
  * />
  */";
            var tree = Parse(text);
            tree.Should().NotBeNull();
            tree.GetCompilationUnitRoot().ToFullString().Should().Be(text);
            tree.GetCompilationUnitRoot().Errors().Length.Should().Be(0);
            var leading = tree.GetCompilationUnitRoot().EndOfFileToken.GetLeadingTrivia();
            leading.Count.Should().Be(1);
            var node = leading[0];
            node.Kind().Should().Be(SyntaxKind.MultiLineDocumentationCommentTrivia);
            node.ToFullString().Should().Be(text);
            var doc = (DocumentationCommentTriviaSyntax)node.GetStructure();
            doc.Content.Count.Should().Be(3);
            doc.Content[0].Kind().Should().Be(SyntaxKind.XmlText);
            doc.Content[0].HasLeadingTrivia.Should().BeTrue();
            doc.Content[1].Kind().Should().Be(SyntaxKind.XmlEmptyElement);
            var element = (XmlEmptyElementSyntax)doc.Content[1];
            element.Attributes.Count.Should().Be(1);
            doc.Content[2].Kind().Should().Be(SyntaxKind.XmlText);
        }

        [Fact]
        public void TestEmptyElementWithAttributeQuoteAndAttributeTextOnMultipleLines()
        {
            var text =
@"/// <goo 
/// a
/// =
/// '
/// xyz
/// '
/// />";
            var tree = Parse(text);
            tree.Should().NotBeNull();
            tree.GetCompilationUnitRoot().ToFullString().Should().Be(text);
            tree.GetCompilationUnitRoot().Errors().Length.Should().Be(0);
            var leading = tree.GetCompilationUnitRoot().EndOfFileToken.GetLeadingTrivia();
            leading.Count.Should().Be(1);
            var node = leading[0];
            node.Kind().Should().Be(SyntaxKind.SingleLineDocumentationCommentTrivia);
            node.ToFullString().Should().Be(text);
            var doc = (DocumentationCommentTriviaSyntax)node.GetStructure();
            doc.Content.Count.Should().Be(2);
            doc.Content[0].Kind().Should().Be(SyntaxKind.XmlText);
            doc.Content[0].HasLeadingTrivia.Should().BeTrue();
            doc.Content[1].Kind().Should().Be(SyntaxKind.XmlEmptyElement);
            var element = (XmlEmptyElementSyntax)doc.Content[1];
            element.Attributes.Count.Should().Be(1);
        }

        [Fact]
        public void TestEmptyElementWithAttributeDoubleQuoteAndAttributeTextOnMultipleLines()
        {
            var text =
@"/// <goo 
/// a
/// =
/// ""
/// xyz
/// ""
/// />";
            var tree = Parse(text);
            tree.Should().NotBeNull();
            tree.GetCompilationUnitRoot().ToFullString().Should().Be(text);
            tree.GetCompilationUnitRoot().Errors().Length.Should().Be(0);
            var leading = tree.GetCompilationUnitRoot().EndOfFileToken.GetLeadingTrivia();
            leading.Count.Should().Be(1);
            var node = leading[0];
            node.Kind().Should().Be(SyntaxKind.SingleLineDocumentationCommentTrivia);
            node.ToFullString().Should().Be(text);
            var doc = (DocumentationCommentTriviaSyntax)node.GetStructure();
            doc.Content.Count.Should().Be(2);
            doc.Content[0].Kind().Should().Be(SyntaxKind.XmlText);
            doc.Content[0].HasLeadingTrivia.Should().BeTrue();
            doc.Content[1].Kind().Should().Be(SyntaxKind.XmlEmptyElement);
            var element = (XmlEmptyElementSyntax)doc.Content[1];
            element.Attributes.Count.Should().Be(1);
        }

        [Fact]
        public void TestEmptyElementWithAttributeDoubleQuoteAndAttributeTextOnMultipleLinesDelimited()
        {
            var text =
@"/** <goo 
  * a
  * =
  * ""
  * xyz
  * ""
  * />
  */";
            var tree = Parse(text);
            tree.Should().NotBeNull();
            tree.GetCompilationUnitRoot().ToFullString().Should().Be(text);
            tree.GetCompilationUnitRoot().Errors().Length.Should().Be(0);
            var leading = tree.GetCompilationUnitRoot().EndOfFileToken.GetLeadingTrivia();
            leading.Count.Should().Be(1);
            var node = leading[0];
            node.Kind().Should().Be(SyntaxKind.MultiLineDocumentationCommentTrivia);
            node.ToFullString().Should().Be(text);
            var doc = (DocumentationCommentTriviaSyntax)node.GetStructure();
            doc.Content.Count.Should().Be(3);
            doc.Content[0].Kind().Should().Be(SyntaxKind.XmlText);
            doc.Content[0].HasLeadingTrivia.Should().BeTrue();
            doc.Content[1].Kind().Should().Be(SyntaxKind.XmlEmptyElement);
            var element = (XmlEmptyElementSyntax)doc.Content[1];
            element.Attributes.Count.Should().Be(1);
            doc.Content[2].Kind().Should().Be(SyntaxKind.XmlText);
        }

        [Fact]
        public void TestEmptyElementWithAttributeQuoteAndAttributeTextOnMultipleLinesDelimited()
        {
            var text =
@"/** <goo 
  * a
  * =
  * '
  * xyz
  * '
  * />
  */";
            var tree = Parse(text);
            tree.Should().NotBeNull();
            tree.GetCompilationUnitRoot().ToFullString().Should().Be(text);
            tree.GetCompilationUnitRoot().Errors().Length.Should().Be(0);
            var leading = tree.GetCompilationUnitRoot().EndOfFileToken.GetLeadingTrivia();
            leading.Count.Should().Be(1);
            var node = leading[0];
            node.Kind().Should().Be(SyntaxKind.MultiLineDocumentationCommentTrivia);
            node.ToFullString().Should().Be(text);
            var doc = (DocumentationCommentTriviaSyntax)node.GetStructure();
            doc.Content.Count.Should().Be(3);
            doc.Content[0].Kind().Should().Be(SyntaxKind.XmlText);
            doc.Content[0].HasLeadingTrivia.Should().BeTrue();
            doc.Content[1].Kind().Should().Be(SyntaxKind.XmlEmptyElement);
            var element = (XmlEmptyElementSyntax)doc.Content[1];
            element.Attributes.Count.Should().Be(1);
            doc.Content[2].Kind().Should().Be(SyntaxKind.XmlText);
        }

        [Fact]
        public void TestElementDotInName()
        {
            var text = "/// <goo.bar />";
            var tree = Parse(text);
            tree.Should().NotBeNull();
            tree.GetCompilationUnitRoot().ToFullString().Should().Be(text);
            tree.GetCompilationUnitRoot().Errors().Length.Should().Be(0);
            var leading = tree.GetCompilationUnitRoot().EndOfFileToken.GetLeadingTrivia();
            leading.Count.Should().Be(1);
            var node = leading[0];
            node.Kind().Should().Be(SyntaxKind.SingleLineDocumentationCommentTrivia);
            node.ToFullString().Should().Be(text);
            var doc = (DocumentationCommentTriviaSyntax)node.GetStructure();
            doc.Content.Count.Should().Be(2);
            doc.Content[0].Kind().Should().Be(SyntaxKind.XmlText);
            doc.Content[0].HasLeadingTrivia.Should().BeTrue();
            doc.Content[1].Kind().Should().Be(SyntaxKind.XmlEmptyElement);
            var element = (XmlEmptyElementSyntax)doc.Content[1];
            element.Name.ToString().Should().Be("goo.bar");
        }

        [Fact]
        public void TestElementColonInName()
        {
            var text = "/// <goo:bar />";
            var tree = Parse(text);
            tree.Should().NotBeNull();
            tree.GetCompilationUnitRoot().ToFullString().Should().Be(text);
            tree.GetCompilationUnitRoot().Errors().Length.Should().Be(0);
            var leading = tree.GetCompilationUnitRoot().EndOfFileToken.GetLeadingTrivia();
            leading.Count.Should().Be(1);
            var node = leading[0];
            node.Kind().Should().Be(SyntaxKind.SingleLineDocumentationCommentTrivia);
            node.ToFullString().Should().Be(text);
            var doc = (DocumentationCommentTriviaSyntax)node.GetStructure();
            doc.Content.Count.Should().Be(2);
            doc.Content[0].Kind().Should().Be(SyntaxKind.XmlText);
            doc.Content[0].HasLeadingTrivia.Should().BeTrue();
            doc.Content[1].Kind().Should().Be(SyntaxKind.XmlEmptyElement);
            var element = (XmlEmptyElementSyntax)doc.Content[1];
            element.Name.ToString().Should().Be("goo:bar");
        }

        [Fact]
        public void TestElementDashInName()
        {
            var text = "/// <abc-def />";
            var tree = Parse(text);
            tree.Should().NotBeNull();
            tree.GetCompilationUnitRoot().ToFullString().Should().Be(text);
            tree.GetCompilationUnitRoot().Errors().Length.Should().Be(0);
            var leading = tree.GetCompilationUnitRoot().EndOfFileToken.GetLeadingTrivia();
            leading.Count.Should().Be(1);
            var node = leading[0];
            node.Kind().Should().Be(SyntaxKind.SingleLineDocumentationCommentTrivia);
            node.ToFullString().Should().Be(text);
            var doc = (DocumentationCommentTriviaSyntax)node.GetStructure();
            doc.Content.Count.Should().Be(2);
            doc.Content[0].Kind().Should().Be(SyntaxKind.XmlText);
            doc.Content[0].HasLeadingTrivia.Should().BeTrue();
            doc.Content[1].Kind().Should().Be(SyntaxKind.XmlEmptyElement);
            var element = (XmlEmptyElementSyntax)doc.Content[1];
            element.Name.ToString().Should().Be("abc-def");
        }

        [Fact]
        public void TestElementNumberInName()
        {
            var text = "/// <goo123 />";
            var tree = Parse(text);
            tree.Should().NotBeNull();
            tree.GetCompilationUnitRoot().ToFullString().Should().Be(text);
            tree.GetCompilationUnitRoot().Errors().Length.Should().Be(0);
            var leading = tree.GetCompilationUnitRoot().EndOfFileToken.GetLeadingTrivia();
            leading.Count.Should().Be(1);
            var node = leading[0];
            node.Kind().Should().Be(SyntaxKind.SingleLineDocumentationCommentTrivia);
            node.ToFullString().Should().Be(text);
            var doc = (DocumentationCommentTriviaSyntax)node.GetStructure();
            doc.Content.Count.Should().Be(2);
            doc.Content[0].Kind().Should().Be(SyntaxKind.XmlText);
            doc.Content[0].HasLeadingTrivia.Should().BeTrue();
            doc.Content[1].Kind().Should().Be(SyntaxKind.XmlEmptyElement);
            var element = (XmlEmptyElementSyntax)doc.Content[1];
            element.Name.ToString().Should().Be("goo123");
        }

        [Fact]
        public void TestElementNumberIsNameError()
        {
            var text = "/// <123 />";
            var tree = Parse(text);
            tree.Should().NotBeNull();
            tree.GetCompilationUnitRoot().ToFullString().Should().Be(text);
            tree.GetCompilationUnitRoot().ErrorsAndWarnings().Length.Should().NotBe(0);
            var leading = tree.GetCompilationUnitRoot().EndOfFileToken.GetLeadingTrivia();
            leading.Count.Should().Be(1);
            var node = leading[0];
            node.Kind().Should().Be(SyntaxKind.SingleLineDocumentationCommentTrivia);
            node.ToFullString().Should().Be(text);
            var doc = (DocumentationCommentTriviaSyntax)node.GetStructure();
            doc.ErrorsAndWarnings().Length.Should().NotBe(0);
        }

        [Fact]
        public void TestNonEmptyElementNoAttributes()
        {
            var text =
@"/// <goo>
/// bar
/// </goo>";
            var tree = Parse(text);
            tree.Should().NotBeNull();
            tree.GetCompilationUnitRoot().ToFullString().Should().Be(text);
            tree.GetCompilationUnitRoot().Errors().Length.Should().Be(0);
            var leading = tree.GetCompilationUnitRoot().EndOfFileToken.GetLeadingTrivia();
            leading.Count.Should().Be(1);
            var node = leading[0];
            node.Kind().Should().Be(SyntaxKind.SingleLineDocumentationCommentTrivia);
            node.ToFullString().Should().Be(text);
            var doc = (DocumentationCommentTriviaSyntax)node.GetStructure();
            doc.Content.Count.Should().Be(2);
            doc.Content[0].Kind().Should().Be(SyntaxKind.XmlText);
            doc.Content[0].HasLeadingTrivia.Should().BeTrue();
            doc.Content[1].Kind().Should().Be(SyntaxKind.XmlElement);
            var element = (XmlElementSyntax)doc.Content[1];
            element.StartTag.Name.ToString().Should().Be("goo");
            element.EndTag.Name.ToString().Should().Be("goo");
            element.Content.Count.Should().Be(1);
            var textsyntax = (XmlTextSyntax)element.Content[0];
            textsyntax.ChildNodesAndTokens().Count.Should().Be(4);
            textsyntax.ChildNodesAndTokens()[0].ToString().Should().Be(Environment.NewLine);
            textsyntax.ChildNodesAndTokens()[1].ToString().Should().Be(" bar");
            textsyntax.ChildNodesAndTokens()[2].ToString().Should().Be(Environment.NewLine);
            textsyntax.ChildNodesAndTokens()[3].ToString().Should().Be(" ");
        }

        [Fact]
        public void TestNonEmptyElementNoAttributesDelimited()
        {
            var text =
@"/** <goo>
  * bar
  * </goo>
  */";
            var tree = Parse(text);
            tree.Should().NotBeNull();
            tree.GetCompilationUnitRoot().ToFullString().Should().Be(text);
            tree.GetCompilationUnitRoot().Errors().Length.Should().Be(0);
            var leading = tree.GetCompilationUnitRoot().EndOfFileToken.GetLeadingTrivia();
            leading.Count.Should().Be(1);
            var node = leading[0];
            node.Kind().Should().Be(SyntaxKind.MultiLineDocumentationCommentTrivia);
            node.ToFullString().Should().Be(text);
            var doc = (DocumentationCommentTriviaSyntax)node.GetStructure();
            doc.Content.Count.Should().Be(3);
            doc.Content[0].Kind().Should().Be(SyntaxKind.XmlText);
            doc.Content[0].HasLeadingTrivia.Should().BeTrue();
            doc.Content[1].Kind().Should().Be(SyntaxKind.XmlElement);
            var element = (XmlElementSyntax)doc.Content[1];
            element.StartTag.Name.ToString().Should().Be("goo");
            element.EndTag.Name.ToString().Should().Be("goo");
            element.Content.Count.Should().Be(1);
            var textsyntax = (XmlTextSyntax)element.Content[0];
            textsyntax.ChildNodesAndTokens().Count.Should().Be(4);
            textsyntax.ChildNodesAndTokens()[0].ToString().Should().Be(Environment.NewLine);
            textsyntax.ChildNodesAndTokens()[1].ToString().Should().Be(" bar");
            textsyntax.ChildNodesAndTokens()[2].ToString().Should().Be(Environment.NewLine);
            textsyntax.ChildNodesAndTokens()[3].ToString().Should().Be(" ");
        }

        [Fact]
        public void TestCDataSection()
        {
            var text =
@"/// <![CDATA[ this is a test
/// of &some; cdata /// */ /**
/// ""']]<>/></text]]>";
            var tree = Parse(text);
            tree.Should().NotBeNull();
            tree.GetCompilationUnitRoot().ToFullString().Should().Be(text);
            tree.GetCompilationUnitRoot().Errors().Length.Should().Be(0);
            var leading = tree.GetCompilationUnitRoot().EndOfFileToken.GetLeadingTrivia();
            leading.Count.Should().Be(1);
            var node = leading[0];
            node.Kind().Should().Be(SyntaxKind.SingleLineDocumentationCommentTrivia);
            node.ToFullString().Should().Be(text);
            var doc = (DocumentationCommentTriviaSyntax)node.GetStructure();
            doc.Content.Count.Should().Be(2);
            doc.Content[0].Kind().Should().Be(SyntaxKind.XmlText);
            doc.Content[0].HasLeadingTrivia.Should().BeTrue();
            doc.Content[1].Kind().Should().Be(SyntaxKind.XmlCDataSection);
            var cdata = (XmlCDataSectionSyntax)doc.Content[1];
            cdata.TextTokens.Count.Should().Be(5);
            cdata.TextTokens[0].ToString().Should().Be(" this is a test");
            cdata.TextTokens[1].ToString().Should().Be(Environment.NewLine);
            cdata.TextTokens[2].ToString().Should().Be(" of &some; cdata /// */ /**");
            cdata.TextTokens[3].ToString().Should().Be(Environment.NewLine);
            cdata.TextTokens[4].ToString().Should().Be(" \"']]<>/></text");
        }

        [Fact]
        public void TestCDataSectionDelimited()
        {
            var text =
@"/** <![CDATA[ this is a test
  * of &some; cdata
  * ""']]<>/></text]]>
  */";
            var tree = Parse(text);
            tree.Should().NotBeNull();
            tree.GetCompilationUnitRoot().ToFullString().Should().Be(text);
            tree.GetCompilationUnitRoot().Errors().Length.Should().Be(0);
            var leading = tree.GetCompilationUnitRoot().EndOfFileToken.GetLeadingTrivia();
            leading.Count.Should().Be(1);
            var node = leading[0];
            node.Kind().Should().Be(SyntaxKind.MultiLineDocumentationCommentTrivia);
            node.ToFullString().Should().Be(text);
            var doc = (DocumentationCommentTriviaSyntax)node.GetStructure();
            doc.Content.Count.Should().Be(3);
            doc.Content[0].Kind().Should().Be(SyntaxKind.XmlText);
            doc.Content[0].HasLeadingTrivia.Should().BeTrue();
            doc.Content[1].Kind().Should().Be(SyntaxKind.XmlCDataSection);
            var cdata = (XmlCDataSectionSyntax)doc.Content[1];
            cdata.TextTokens.Count.Should().Be(5);
            cdata.TextTokens[0].ToString().Should().Be(" this is a test");
            cdata.TextTokens[1].ToString().Should().Be(Environment.NewLine);
            cdata.TextTokens[2].ToString().Should().Be(" of &some; cdata");
            cdata.TextTokens[3].ToString().Should().Be(Environment.NewLine);
            cdata.TextTokens[4].ToString().Should().Be(" \"']]<>/></text");
            doc.Content[2].Kind().Should().Be(SyntaxKind.XmlText);
        }

        [Fact]
        public void TestIncompleteEOFCDataSection()
        {
            var text = "/// <![CDATA[ incomplete"; // end of file
            var tree = Parse(text);
            tree.Should().NotBeNull();
            tree.GetCompilationUnitRoot().ToFullString().Should().Be(text);
            tree.GetCompilationUnitRoot().Warnings().Length.Should().Be(1);
            var leading = tree.GetCompilationUnitRoot().EndOfFileToken.GetLeadingTrivia();
            leading.Count.Should().Be(1);
            var node = leading[0];
            node.Kind().Should().Be(SyntaxKind.SingleLineDocumentationCommentTrivia);
            node.ToFullString().Should().Be(text);
            var doc = (DocumentationCommentTriviaSyntax)node.GetStructure();
            doc.Content.Count.Should().Be(2);
            doc.Content[0].Kind().Should().Be(SyntaxKind.XmlText);
            doc.Content[0].HasLeadingTrivia.Should().BeTrue();
            doc.Content[1].Kind().Should().Be(SyntaxKind.XmlCDataSection);
            var cdata = (XmlCDataSectionSyntax)doc.Content[1];
            cdata.ErrorsAndWarnings().Length.Should().Be(1);
            cdata.TextTokens.Count.Should().Be(1);
            cdata.TextTokens[0].ToString().Should().Be(" incomplete");
        }

        [Fact]
        public void TestIncompleteEOLCDataSection()
        {
            var text = @"/// <![CDATA[ incomplete
class C { }"; // end of line/comment
            var tree = Parse(text);
            tree.Should().NotBeNull();
            tree.GetCompilationUnitRoot().ToFullString().Should().Be(text);
            tree.GetCompilationUnitRoot().ErrorsAndWarnings().Length.Should().Be(1);
            var leading = tree.GetCompilationUnitRoot().Members[0].GetLeadingTrivia();
            leading.Count.Should().Be(1);
            var node = leading[0];
            node.Kind().Should().Be(SyntaxKind.SingleLineDocumentationCommentTrivia);
            node.ToFullString().Should().Be($"/// <![CDATA[ incomplete{Environment.NewLine}");
            var doc = (DocumentationCommentTriviaSyntax)node.GetStructure();
            doc.Content.Count.Should().Be(2);
            doc.Content[0].Kind().Should().Be(SyntaxKind.XmlText);
            doc.Content[0].HasLeadingTrivia.Should().BeTrue();
            doc.Content[1].Kind().Should().Be(SyntaxKind.XmlCDataSection);
            var cdata = (XmlCDataSectionSyntax)doc.Content[1];
            cdata.ErrorsAndWarnings().Length.Should().Be(1);
            cdata.TextTokens.Count.Should().Be(2);
            cdata.TextTokens[0].ToString().Should().Be(" incomplete");
            cdata.TextTokens[1].ToString().Should().Be(Environment.NewLine);
        }

        [Fact]
        public void TestIncompleteEOLCDataSection_OtherNewline()
        {
            SyntaxFacts.IsNewLine('\u0085').Should().BeTrue();
            var text = "/// <![CDATA[ incomplete\u0085class C { }"; // end of line/comment
            var tree = Parse(text);
            tree.Should().NotBeNull();
            tree.GetCompilationUnitRoot().ToFullString().Should().Be(text);
            tree.GetCompilationUnitRoot().ErrorsAndWarnings().Length.Should().Be(1);
            var leading = tree.GetCompilationUnitRoot().Members[0].GetLeadingTrivia();
            leading.Count.Should().Be(1);
            var node = leading[0];
            node.Kind().Should().Be(SyntaxKind.SingleLineDocumentationCommentTrivia);
            node.ToFullString().Should().Be("/// <![CDATA[ incomplete\u0085");
            var doc = (DocumentationCommentTriviaSyntax)node.GetStructure();
            doc.Content.Count.Should().Be(2);
            doc.Content[0].Kind().Should().Be(SyntaxKind.XmlText);
            doc.Content[0].HasLeadingTrivia.Should().BeTrue();
            doc.Content[1].Kind().Should().Be(SyntaxKind.XmlCDataSection);
            var cdata = (XmlCDataSectionSyntax)doc.Content[1];
            cdata.ErrorsAndWarnings().Length.Should().Be(1);
            cdata.TextTokens.Count.Should().Be(2);
            cdata.TextTokens[0].ToString().Should().Be(" incomplete");
            cdata.TextTokens[1].ToString().Should().Be("\u0085");
        }

        [Fact]
        public void TestIncompleteDelimitedCDataSection()
        {
            var text = "/** <![CDATA[ incomplete*/"; // end of comment
            var tree = Parse(text);
            tree.Should().NotBeNull();
            tree.GetCompilationUnitRoot().ToFullString().Should().Be(text);
            tree.GetCompilationUnitRoot().ErrorsAndWarnings().Length.Should().Be(1);
            var leading = tree.GetCompilationUnitRoot().EndOfFileToken.GetLeadingTrivia();
            leading.Count.Should().Be(1);
            var node = leading[0];
            node.Kind().Should().Be(SyntaxKind.MultiLineDocumentationCommentTrivia);
            node.ToFullString().Should().Be(text);
            var doc = (DocumentationCommentTriviaSyntax)node.GetStructure();
            doc.Content.Count.Should().Be(2);
            doc.Content[0].Kind().Should().Be(SyntaxKind.XmlText);
            doc.Content[0].HasLeadingTrivia.Should().BeTrue();
            doc.Content[1].Kind().Should().Be(SyntaxKind.XmlCDataSection);
            var cdata = (XmlCDataSectionSyntax)doc.Content[1];
            cdata.ErrorsAndWarnings().Length.Should().Be(1);
            cdata.TextTokens.Count.Should().Be(1);
            cdata.TextTokens[0].ToString().Should().Be(" incomplete");
        }

        [Fact]
        public void TestComment()
        {
            var text =
@"/// <!-- this is a test
/// of &some; comment
/// ""']]<>/></text-->";
            var tree = Parse(text);
            tree.Should().NotBeNull();
            tree.GetCompilationUnitRoot().ToFullString().Should().Be(text);
            tree.GetCompilationUnitRoot().Errors().Length.Should().Be(0);
            var leading = tree.GetCompilationUnitRoot().EndOfFileToken.GetLeadingTrivia();
            leading.Count.Should().Be(1);
            var node = leading[0];
            node.Kind().Should().Be(SyntaxKind.SingleLineDocumentationCommentTrivia);
            node.ToFullString().Should().Be(text);
            var doc = (DocumentationCommentTriviaSyntax)node.GetStructure();
            doc.Content.Count.Should().Be(2);
            doc.Content[0].Kind().Should().Be(SyntaxKind.XmlText);
            doc.Content[0].HasLeadingTrivia.Should().BeTrue();
            doc.Content[1].Kind().Should().Be(SyntaxKind.XmlComment);
            var comment = (XmlCommentSyntax)doc.Content[1];
            comment.TextTokens.Count.Should().Be(5);
            comment.TextTokens[0].ToString().Should().Be(" this is a test");
            comment.TextTokens[1].ToString().Should().Be(Environment.NewLine);
            comment.TextTokens[2].ToString().Should().Be(" of &some; comment");
            comment.TextTokens[3].ToString().Should().Be(Environment.NewLine);
            comment.TextTokens[4].ToString().Should().Be(" \"']]<>/></text");
        }

        [Fact]
        public void TestCommentDelimited()
        {
            var text =
@"/** <!-- this is a test
  * of &some; comment
  * ""']]<>/></text-->
  */";
            var tree = Parse(text);
            tree.Should().NotBeNull();
            tree.GetCompilationUnitRoot().ToFullString().Should().Be(text);
            tree.GetCompilationUnitRoot().Errors().Length.Should().Be(0);
            var leading = tree.GetCompilationUnitRoot().EndOfFileToken.GetLeadingTrivia();
            leading.Count.Should().Be(1);
            var node = leading[0];
            node.Kind().Should().Be(SyntaxKind.MultiLineDocumentationCommentTrivia);
            node.ToFullString().Should().Be(text);
            var doc = (DocumentationCommentTriviaSyntax)node.GetStructure();
            doc.Content.Count.Should().Be(3);
            doc.Content[0].Kind().Should().Be(SyntaxKind.XmlText);
            doc.Content[0].HasLeadingTrivia.Should().BeTrue();
            doc.Content[1].Kind().Should().Be(SyntaxKind.XmlComment);
            var comment = (XmlCommentSyntax)doc.Content[1];
            comment.TextTokens.Count.Should().Be(5);
            comment.TextTokens[0].ToString().Should().Be(" this is a test");
            comment.TextTokens[1].ToString().Should().Be(Environment.NewLine);
            comment.TextTokens[2].ToString().Should().Be(" of &some; comment");
            comment.TextTokens[3].ToString().Should().Be(Environment.NewLine);
            comment.TextTokens[4].ToString().Should().Be(" \"']]<>/></text");
            doc.Content[2].Kind().Should().Be(SyntaxKind.XmlText);
        }

        [Fact]
        public void TestIncompleteEOFComment()
        {
            var text = "/// <!-- incomplete"; // end of file
            var tree = Parse(text);
            tree.Should().NotBeNull();
            tree.GetCompilationUnitRoot().ToFullString().Should().Be(text);
            tree.GetCompilationUnitRoot().ErrorsAndWarnings().Length.Should().Be(1);
            var leading = tree.GetCompilationUnitRoot().EndOfFileToken.GetLeadingTrivia();
            leading.Count.Should().Be(1);
            var node = leading[0];
            node.Kind().Should().Be(SyntaxKind.SingleLineDocumentationCommentTrivia);
            node.ToFullString().Should().Be(text);
            var doc = (DocumentationCommentTriviaSyntax)node.GetStructure();
            doc.Content.Count.Should().Be(2);
            doc.Content[0].Kind().Should().Be(SyntaxKind.XmlText);
            doc.Content[0].HasLeadingTrivia.Should().BeTrue();
            doc.Content[1].Kind().Should().Be(SyntaxKind.XmlComment);
            var comment = (XmlCommentSyntax)doc.Content[1];
            comment.ErrorsAndWarnings().Length.Should().Be(1);
            comment.TextTokens.Count.Should().Be(1);
            comment.TextTokens[0].ToString().Should().Be(" incomplete");
        }

        [Fact]
        public void TestIncompleteEOLComment()
        {
            var text = @"/// <!-- incomplete
class C { }"; // end of line/comment
            var tree = Parse(text);
            tree.Should().NotBeNull();
            tree.GetCompilationUnitRoot().ToFullString().Should().Be(text);
            tree.GetCompilationUnitRoot().ErrorsAndWarnings().Length.Should().Be(1);
            var leading = tree.GetCompilationUnitRoot().Members[0].GetLeadingTrivia();
            leading.Count.Should().Be(1);
            var node = leading[0];
            node.Kind().Should().Be(SyntaxKind.SingleLineDocumentationCommentTrivia);
            node.ToFullString().Should().Be($"/// <!-- incomplete{Environment.NewLine}");
            var doc = (DocumentationCommentTriviaSyntax)node.GetStructure();
            doc.Content.Count.Should().Be(2);
            doc.Content[0].Kind().Should().Be(SyntaxKind.XmlText);
            doc.Content[0].HasLeadingTrivia.Should().BeTrue();
            doc.Content[1].Kind().Should().Be(SyntaxKind.XmlComment);
            var comment = (XmlCommentSyntax)doc.Content[1];
            comment.ErrorsAndWarnings().Length.Should().Be(1);
            comment.TextTokens.Count.Should().Be(2);
            comment.TextTokens[0].ToString().Should().Be(" incomplete");
            comment.TextTokens[1].ToString().Should().Be(Environment.NewLine);
        }

        [Fact]
        public void TestIncompleteDelimitedComment()
        {
            var text = "/** <!-- incomplete*/"; // end of comment
            var tree = Parse(text);
            tree.Should().NotBeNull();
            tree.GetCompilationUnitRoot().ToFullString().Should().Be(text);
            tree.GetCompilationUnitRoot().ErrorsAndWarnings().Length.Should().Be(1);
            var leading = tree.GetCompilationUnitRoot().EndOfFileToken.GetLeadingTrivia();
            leading.Count.Should().Be(1);
            var node = leading[0];
            node.Kind().Should().Be(SyntaxKind.MultiLineDocumentationCommentTrivia);
            node.ToFullString().Should().Be(text);
            var doc = (DocumentationCommentTriviaSyntax)node.GetStructure();
            doc.Content.Count.Should().Be(2);
            doc.Content[0].Kind().Should().Be(SyntaxKind.XmlText);
            doc.Content[0].HasLeadingTrivia.Should().BeTrue();
            doc.Content[1].Kind().Should().Be(SyntaxKind.XmlComment);
            var comment = (XmlCommentSyntax)doc.Content[1];
            comment.ErrorsAndWarnings().Length.Should().Be(1);
            comment.TextTokens.Count.Should().Be(1);
            comment.TextTokens[0].ToString().Should().Be(" incomplete");
        }

        [Fact]
        public void TestProcessingInstruction()
        {
            var text =
@"/// <?ProcessingInstruction this is a test
/// of &a; ProcessingInstruction /// */ /**
/// ""']]>/>?</text?>";
            var tree = Parse(text);
            tree.Should().NotBeNull();
            tree.GetCompilationUnitRoot().ToFullString().Should().Be(text);
            tree.GetCompilationUnitRoot().Errors().Length.Should().Be(0);
            var leading = tree.GetCompilationUnitRoot().EndOfFileToken.GetLeadingTrivia();
            leading.Count.Should().Be(1);
            var node = leading[0];
            node.Kind().Should().Be(SyntaxKind.SingleLineDocumentationCommentTrivia);
            node.ToFullString().Should().Be(text);
            var doc = (DocumentationCommentTriviaSyntax)node.GetStructure();
            doc.Content.Count.Should().Be(2);
            doc.Content[0].Kind().Should().Be(SyntaxKind.XmlText);
            doc.Content[0].HasLeadingTrivia.Should().BeTrue();
            doc.Content[1].Kind().Should().Be(SyntaxKind.XmlProcessingInstruction);
            var ProcessingInstruction = (XmlProcessingInstructionSyntax)doc.Content[1];
            ProcessingInstruction.Name.Prefix.Should().BeNull();
            ProcessingInstruction.Name.LocalName.Text.Should().Be("ProcessingInstruction");
            ProcessingInstruction.TextTokens.Count.Should().Be(5);
            ProcessingInstruction.TextTokens[0].ToString().Should().Be(" this is a test");
            ProcessingInstruction.TextTokens[1].ToString().Should().Be(Environment.NewLine);
            ProcessingInstruction.TextTokens[2].ToString().Should().Be(" of &a; ProcessingInstruction /// */ /**");
            ProcessingInstruction.TextTokens[3].ToString().Should().Be(Environment.NewLine);
            ProcessingInstruction.TextTokens[4].ToString().Should().Be(" \"']]>/>?</text");
        }

        [Fact]
        public void TestProcessingInstructionDelimited()
        {
            var text =
@"/** <?prefix:localname this is a test <!--
  * of &a; ProcessingInstruction
  * ""']]>/></text>]]>?>
  */";
            var tree = Parse(text);
            tree.Should().NotBeNull();
            tree.GetCompilationUnitRoot().ToFullString().Should().Be(text);
            tree.GetCompilationUnitRoot().Errors().Length.Should().Be(0);
            var leading = tree.GetCompilationUnitRoot().EndOfFileToken.GetLeadingTrivia();
            leading.Count.Should().Be(1);
            var node = leading[0];
            node.Kind().Should().Be(SyntaxKind.MultiLineDocumentationCommentTrivia);
            node.ToFullString().Should().Be(text);
            var doc = (DocumentationCommentTriviaSyntax)node.GetStructure();
            doc.Content.Count.Should().Be(3);
            doc.Content[0].Kind().Should().Be(SyntaxKind.XmlText);
            doc.Content[0].HasLeadingTrivia.Should().BeTrue();
            doc.Content[1].Kind().Should().Be(SyntaxKind.XmlProcessingInstruction);
            var ProcessingInstruction = (XmlProcessingInstructionSyntax)doc.Content[1];
            ProcessingInstruction.Name.Prefix.Prefix.Text.Should().Be("prefix");
            ProcessingInstruction.Name.Prefix.ColonToken.Text.Should().Be(":");
            ProcessingInstruction.Name.LocalName.Text.Should().Be("localname");
            ProcessingInstruction.TextTokens.Count.Should().Be(5);
            ProcessingInstruction.TextTokens[0].ToString().Should().Be(" this is a test <!--");
            ProcessingInstruction.TextTokens[1].ToString().Should().Be(Environment.NewLine);
            ProcessingInstruction.TextTokens[2].ToString().Should().Be(" of &a; ProcessingInstruction");
            ProcessingInstruction.TextTokens[3].ToString().Should().Be(Environment.NewLine);
            ProcessingInstruction.TextTokens[4].ToString().Should().Be(" \"']]>/></text>]]>");
            doc.Content[2].Kind().Should().Be(SyntaxKind.XmlText);
        }

        [Fact]
        public void TestIncompleteEOFProcessingInstruction()
        {
            var text = "/// <?incomplete"; // end of file
            var tree = Parse(text);
            tree.Should().NotBeNull();
            tree.GetCompilationUnitRoot().ToFullString().Should().Be(text);
            tree.GetCompilationUnitRoot().ErrorsAndWarnings().Length.Should().Be(1);
            var leading = tree.GetCompilationUnitRoot().EndOfFileToken.GetLeadingTrivia();
            leading.Count.Should().Be(1);
            var node = leading[0];
            node.Kind().Should().Be(SyntaxKind.SingleLineDocumentationCommentTrivia);
            node.ToFullString().Should().Be(text);
            var doc = (DocumentationCommentTriviaSyntax)node.GetStructure();
            doc.Content.Count.Should().Be(2);
            doc.Content[0].Kind().Should().Be(SyntaxKind.XmlText);
            doc.Content[0].HasLeadingTrivia.Should().BeTrue();
            doc.Content[1].Kind().Should().Be(SyntaxKind.XmlProcessingInstruction);
            var ProcessingInstruction = (XmlProcessingInstructionSyntax)doc.Content[1];
            ProcessingInstruction.Name.Prefix.Should().BeNull();
            ProcessingInstruction.Name.LocalName.Text.Should().Be("incomplete");
            ProcessingInstruction.ErrorsAndWarnings().Length.Should().Be(1);
            ProcessingInstruction.TextTokens.Count.Should().Be(0);
        }

        [Fact]
        public void TestIncompleteEOLProcessingInstruction_OtherNewline()
        {
            SyntaxFacts.IsNewLine('\u0085').Should().BeTrue();
            var text = "/// <?name incomplete\u0085class C { }"; // end of line/comment
            var tree = Parse(text);
            tree.Should().NotBeNull();
            tree.GetCompilationUnitRoot().ToFullString().Should().Be(text);
            tree.GetCompilationUnitRoot().ErrorsAndWarnings().Length.Should().Be(1);
            var leading = tree.GetCompilationUnitRoot().Members[0].GetLeadingTrivia();
            leading.Count.Should().Be(1);
            var node = leading[0];
            node.Kind().Should().Be(SyntaxKind.SingleLineDocumentationCommentTrivia);
            node.ToFullString().Should().Be("/// <?name incomplete\u0085");
            var doc = (DocumentationCommentTriviaSyntax)node.GetStructure();
            doc.Content.Count.Should().Be(2);
            doc.Content[0].Kind().Should().Be(SyntaxKind.XmlText);
            doc.Content[0].HasLeadingTrivia.Should().BeTrue();
            doc.Content[1].Kind().Should().Be(SyntaxKind.XmlProcessingInstruction);
            var ProcessingInstruction = (XmlProcessingInstructionSyntax)doc.Content[1];
            ProcessingInstruction.Name.Prefix.Should().BeNull();
            ProcessingInstruction.Name.LocalName.Text.Should().Be("name");
            ProcessingInstruction.ErrorsAndWarnings().Length.Should().Be(1);
            ProcessingInstruction.TextTokens.Count.Should().Be(2);
            ProcessingInstruction.TextTokens[0].ToString().Should().Be(" incomplete");
            ProcessingInstruction.TextTokens[1].ToString().Should().Be("\u0085");
        }

        [Fact]
        public void TestIncompleteDelimitedProcessingInstruction()
        {
            var text = "/** <?name incomplete*/"; // end of comment
            var tree = Parse(text);
            tree.Should().NotBeNull();
            tree.GetCompilationUnitRoot().ToFullString().Should().Be(text);
            tree.GetCompilationUnitRoot().ErrorsAndWarnings().Length.Should().Be(1);
            var leading = tree.GetCompilationUnitRoot().EndOfFileToken.GetLeadingTrivia();
            leading.Count.Should().Be(1);
            var node = leading[0];
            node.Kind().Should().Be(SyntaxKind.MultiLineDocumentationCommentTrivia);
            node.ToFullString().Should().Be(text);
            var doc = (DocumentationCommentTriviaSyntax)node.GetStructure();
            doc.Content.Count.Should().Be(2);
            doc.Content[0].Kind().Should().Be(SyntaxKind.XmlText);
            doc.Content[0].HasLeadingTrivia.Should().BeTrue();
            doc.Content[1].Kind().Should().Be(SyntaxKind.XmlProcessingInstruction);
            var ProcessingInstruction = (XmlProcessingInstructionSyntax)doc.Content[1];
            ProcessingInstruction.Name.Prefix.Should().BeNull();
            ProcessingInstruction.Name.LocalName.Text.Should().Be("name");
            ProcessingInstruction.ErrorsAndWarnings().Length.Should().Be(1);
            ProcessingInstruction.TextTokens.Count.Should().Be(1);
            ProcessingInstruction.TextTokens[0].ToString().Should().Be(" incomplete");
        }

        [WorkItem(899122, "DevDiv/Personal")]
        [Fact]
        public void TestIncompleteXMLComment()
        {
            var text = "/**\n"; // end of comment
            var tree = Parse(text);

            tree.Should().NotBeNull();
            tree.GetCompilationUnitRoot().ToFullString().Should().Be(text);
            tree.GetCompilationUnitRoot().Errors().Length.Should().Be(1);
            var leading = tree.GetCompilationUnitRoot().EndOfFileToken.GetLeadingTrivia();
            leading.Count.Should().Be(1);
            var node = leading[0];
            node.Kind().Should().Be(SyntaxKind.MultiLineDocumentationCommentTrivia);
            node.ToFullString().Should().Be(text);
            var doc = (DocumentationCommentTriviaSyntax)node.GetStructure();
            doc.Content.Count.Should().Be(1);
            doc.Content[0].Kind().Should().Be(SyntaxKind.XmlText);
            doc.Content[0].HasLeadingTrivia.Should().BeTrue();
        }

        [Fact]
        public void TestEarlyTerminationOfXmlParse()
        {
            var text =
@"/// <goo>
/// bar
/// </goo>
/// </uhoh>
///
class C { }";
            var tree = Parse(text);
            tree.Should().NotBeNull();
            tree.GetCompilationUnitRoot().ToFullString().Should().Be(text);
            tree.GetCompilationUnitRoot().ChildNodesAndTokens().Count.Should().Be(2);
            tree.GetCompilationUnitRoot().ChildNodesAndTokens()[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var classdecl = (TypeDeclarationSyntax)tree.GetCompilationUnitRoot().ChildNodesAndTokens()[0].AsNode();
            classdecl.ToString().Should().Be("class C { }");
            classdecl.HasLeadingTrivia.Should().BeTrue();
            var leading = classdecl.GetLeadingTrivia();
            leading.Count.Should().Be(1);
            var node = leading[0];
            node.ErrorsAndWarnings().Length.Should().NotBe(0);
            tree.GetCompilationUnitRoot().ChildNodesAndTokens()[1].Kind().Should().Be(SyntaxKind.EndOfFileToken);
        }

        [Fact]
        public void TestPredefinedXmlEntity()
        {
            var text =
@"/// &lt;";
            var tree = Parse(text);
            tree.Should().NotBeNull();
            tree.GetCompilationUnitRoot().ToFullString().Should().Be(text);
            tree.GetCompilationUnitRoot().Errors().Length.Should().Be(0);
            var leading = tree.GetCompilationUnitRoot().EndOfFileToken.GetLeadingTrivia();
            leading.Count.Should().Be(1);
            var node = leading[0];
            node.Kind().Should().Be(SyntaxKind.SingleLineDocumentationCommentTrivia);
            node.ToFullString().Should().Be(text);
            var doc = (DocumentationCommentTriviaSyntax)node.GetStructure();
            doc.Content.Count.Should().Be(1);
            doc.Content[0].Kind().Should().Be(SyntaxKind.XmlText);
            doc.Content[0].HasLeadingTrivia.Should().BeTrue();
            var xmltext = (XmlTextSyntax)doc.Content[0];
            xmltext.TextTokens.Count.Should().Be(2);
            xmltext.TextTokens[0].Value.Should().Be(" ");
            xmltext.TextTokens[1].Value.Should().Be("<");
        }

        [Fact]
        public void TestPredefinedXmlEntityDelimited()
        {
            var text =
@"/** &lt; */";
            var tree = Parse(text);
            tree.Should().NotBeNull();
            tree.GetCompilationUnitRoot().ToFullString().Should().Be(text);
            tree.GetCompilationUnitRoot().Errors().Length.Should().Be(0);
            var leading = tree.GetCompilationUnitRoot().EndOfFileToken.GetLeadingTrivia();
            leading.Count.Should().Be(1);
            var node = leading[0];
            node.Kind().Should().Be(SyntaxKind.MultiLineDocumentationCommentTrivia);
            node.ToFullString().Should().Be(text);
            var doc = (DocumentationCommentTriviaSyntax)node.GetStructure();
            doc.Content.Count.Should().Be(1);
            doc.Content[0].Kind().Should().Be(SyntaxKind.XmlText);
            doc.Content[0].HasLeadingTrivia.Should().BeTrue();
            var xmltext = (XmlTextSyntax)doc.Content[0];
            xmltext.TextTokens.Count.Should().Be(3);
            xmltext.TextTokens[0].Value.Should().Be(" ");
            xmltext.TextTokens[1].Value.Should().Be("<");
            xmltext.TextTokens[2].Value.Should().Be(" ");
        }

        [Fact]
        public void TestHexCharacterXmlEntity()
        {
            var text =
@"/// &#x41;";
            var tree = Parse(text);
            tree.Should().NotBeNull();
            tree.GetCompilationUnitRoot().ToFullString().Should().Be(text);
            tree.GetCompilationUnitRoot().Errors().Length.Should().Be(0);
            var leading = tree.GetCompilationUnitRoot().EndOfFileToken.GetLeadingTrivia();
            leading.Count.Should().Be(1);
            var node = leading[0];
            node.Kind().Should().Be(SyntaxKind.SingleLineDocumentationCommentTrivia);
            node.ToFullString().Should().Be(text);
            var doc = (DocumentationCommentTriviaSyntax)node.GetStructure();
            doc.Content.Count.Should().Be(1);
            doc.Content[0].Kind().Should().Be(SyntaxKind.XmlText);
            doc.Content[0].HasLeadingTrivia.Should().BeTrue();
            var xmltext = (XmlTextSyntax)doc.Content[0];
            xmltext.TextTokens.Count.Should().Be(2);
            xmltext.TextTokens[0].Value.Should().Be(" ");
            xmltext.TextTokens[1].Value.Should().Be("A");
        }

        [Fact]
        public void TestHexCharacterXmlEntityDelimited()
        {
            var text =
@"/** &#x41; */";
            var tree = Parse(text);
            tree.Should().NotBeNull();
            tree.GetCompilationUnitRoot().ToFullString().Should().Be(text);
            tree.GetCompilationUnitRoot().Errors().Length.Should().Be(0);
            var leading = tree.GetCompilationUnitRoot().EndOfFileToken.GetLeadingTrivia();
            leading.Count.Should().Be(1);
            var node = leading[0];
            node.Kind().Should().Be(SyntaxKind.MultiLineDocumentationCommentTrivia);
            node.ToFullString().Should().Be(text);
            var doc = (DocumentationCommentTriviaSyntax)node.GetStructure();
            doc.Content.Count.Should().Be(1);
            doc.Content[0].Kind().Should().Be(SyntaxKind.XmlText);
            doc.Content[0].HasLeadingTrivia.Should().BeTrue();
            var xmltext = (XmlTextSyntax)doc.Content[0];
            xmltext.TextTokens.Count.Should().Be(3);
            xmltext.TextTokens[0].Value.Should().Be(" ");
            xmltext.TextTokens[1].Value.Should().Be("A");
            xmltext.TextTokens[2].Value.Should().Be(" ");
        }

        [Fact]
        public void TestDecCharacterXmlEntity()
        {
            var text =
@"/// &#65;";
            var tree = Parse(text);
            tree.Should().NotBeNull();
            tree.GetCompilationUnitRoot().ToFullString().Should().Be(text);
            tree.GetCompilationUnitRoot().Errors().Length.Should().Be(0);
            var leading = tree.GetCompilationUnitRoot().EndOfFileToken.GetLeadingTrivia();
            leading.Count.Should().Be(1);
            var node = leading[0];
            node.Kind().Should().Be(SyntaxKind.SingleLineDocumentationCommentTrivia);
            node.ToFullString().Should().Be(text);
            var doc = (DocumentationCommentTriviaSyntax)node.GetStructure();
            doc.Content.Count.Should().Be(1);
            doc.Content[0].Kind().Should().Be(SyntaxKind.XmlText);
            doc.Content[0].HasLeadingTrivia.Should().BeTrue();
            var xmltext = (XmlTextSyntax)doc.Content[0];
            xmltext.TextTokens.Count.Should().Be(2);
            xmltext.TextTokens[0].Value.Should().Be(" ");
            xmltext.TextTokens[1].Value.Should().Be("A");
        }

        [Fact]
        public void TestDecCharacterXmlEntityDelimited()
        {
            var text =
@"/** &#65; */";
            var tree = Parse(text);
            tree.Should().NotBeNull();
            tree.GetCompilationUnitRoot().ToFullString().Should().Be(text);
            tree.GetCompilationUnitRoot().Errors().Length.Should().Be(0);
            var leading = tree.GetCompilationUnitRoot().EndOfFileToken.GetLeadingTrivia();
            leading.Count.Should().Be(1);
            var node = leading[0];
            node.Kind().Should().Be(SyntaxKind.MultiLineDocumentationCommentTrivia);
            node.ToFullString().Should().Be(text);
            var doc = (DocumentationCommentTriviaSyntax)node.GetStructure();
            doc.Content.Count.Should().Be(1);
            doc.Content[0].Kind().Should().Be(SyntaxKind.XmlText);
            doc.Content[0].HasLeadingTrivia.Should().BeTrue();
            var xmltext = (XmlTextSyntax)doc.Content[0];
            xmltext.TextTokens.Count.Should().Be(3);
            xmltext.TextTokens[0].Value.Should().Be(" ");
            xmltext.TextTokens[1].Value.Should().Be("A");
            xmltext.TextTokens[2].Value.Should().Be(" ");
        }

        [Fact]
        public void TestLargeHexCharacterXmlEntity()
        {
            var text =
@"/// &#x1d11e;";
            var tree = Parse(text);
            tree.Should().NotBeNull();
            tree.GetCompilationUnitRoot().ToFullString().Should().Be(text);
            tree.GetCompilationUnitRoot().Errors().Length.Should().Be(0);
            var leading = tree.GetCompilationUnitRoot().EndOfFileToken.GetLeadingTrivia();
            leading.Count.Should().Be(1);
            var node = leading[0];
            node.Kind().Should().Be(SyntaxKind.SingleLineDocumentationCommentTrivia);
            node.ToFullString().Should().Be(text);
            var doc = (DocumentationCommentTriviaSyntax)node.GetStructure();
            doc.Content.Count.Should().Be(1);
            doc.Content[0].Kind().Should().Be(SyntaxKind.XmlText);
            doc.Content[0].HasLeadingTrivia.Should().BeTrue();
            var xmltext = (XmlTextSyntax)doc.Content[0];
            xmltext.TextTokens.Count.Should().Be(2);
            xmltext.TextTokens[0].Value.Should().Be(" ");
            xmltext.TextTokens[1].Value.Should().Be("\U0001D11E");
        }

        [Fact]
        public void TestLargeHexCharacterXmlEntityDelimited()
        {
            var text =
@"/** &#x1D11E; */";
            var tree = Parse(text);
            tree.Should().NotBeNull();
            tree.GetCompilationUnitRoot().ToFullString().Should().Be(text);
            tree.GetCompilationUnitRoot().Errors().Length.Should().Be(0);
            var leading = tree.GetCompilationUnitRoot().EndOfFileToken.GetLeadingTrivia();
            leading.Count.Should().Be(1);
            var node = leading[0];
            node.Kind().Should().Be(SyntaxKind.MultiLineDocumentationCommentTrivia);
            node.ToFullString().Should().Be(text);
            var doc = (DocumentationCommentTriviaSyntax)node.GetStructure();
            doc.Content.Count.Should().Be(1);
            doc.Content[0].Kind().Should().Be(SyntaxKind.XmlText);
            doc.Content[0].HasLeadingTrivia.Should().BeTrue();
            var xmltext = (XmlTextSyntax)doc.Content[0];
            xmltext.TextTokens.Count.Should().Be(3);
            xmltext.TextTokens[0].Value.Should().Be(" ");
            xmltext.TextTokens[1].Value.Should().Be("\U0001D11E");
            xmltext.TextTokens[2].Value.Should().Be(" ");
        }

        [Fact]
        public void TestXmlEntityUndefined()
        {
            var text =
@"///&#abcdef;";
            var tree = Parse(text);
            tree.Should().NotBeNull();
            tree.GetCompilationUnitRoot().ToFullString().Should().Be(text);
            tree.GetCompilationUnitRoot().ErrorsAndWarnings().Length.Should().NotBe(0);
        }

        [Fact]
        public void TestXmlAttributeLessThan()
        {
            var text =
@"///<goo attr=""less<than"" />";
            var tree = Parse(text);
            tree.Should().NotBeNull();
            tree.GetCompilationUnitRoot().ToFullString().Should().Be(text);
            tree.GetCompilationUnitRoot().ErrorsAndWarnings().Length.Should().NotBe(0);
        }

        [Fact]
        public void TestXmlCommentDashDash()
        {
            var text =
@"///<!-- A Comment with -- -->";
            var tree = Parse(text);
            tree.Should().NotBeNull();
            tree.GetCompilationUnitRoot().ToFullString().Should().Be(text);
            tree.GetCompilationUnitRoot().ErrorsAndWarnings().Length.Should().NotBe(0);
        }

        [Fact]
        public void TestXmlElementMismatch()
        {
            var text =
@"///< goo > </ bar >";
            var tree = Parse(text);
            tree.Should().NotBeNull();
            tree.GetCompilationUnitRoot().ToFullString().Should().Be(text);
            tree.GetCompilationUnitRoot().ErrorsAndWarnings().Length.Should().NotBe(0);
        }

        [Fact]
        public void TestXmlElementDuplicateAttributes()
        {
            var text =
@"///< goo x = ""bar"" x = ""baz"" ";
            var tree = Parse(text);
            tree.Should().NotBeNull();
            tree.GetCompilationUnitRoot().ToFullString().Should().Be(text);
            tree.GetCompilationUnitRoot().ErrorsAndWarnings().Length.Should().NotBe(0);
        }

        [Fact]
        public void TestPredefinedXmlEntityInAttribute()
        {
            var text =
@"/// <goo a="" &lt; ""/>";
            var tree = Parse(text);
            tree.Should().NotBeNull();
            tree.GetCompilationUnitRoot().ToFullString().Should().Be(text);
            tree.GetCompilationUnitRoot().Errors().Length.Should().Be(0);
            var leading = tree.GetCompilationUnitRoot().EndOfFileToken.GetLeadingTrivia();
            leading.Count.Should().Be(1);
            var node = leading[0];
            node.Kind().Should().Be(SyntaxKind.SingleLineDocumentationCommentTrivia);
            node.ToFullString().Should().Be(text);
            var doc = (DocumentationCommentTriviaSyntax)node.GetStructure();
            doc.Content.Count.Should().Be(2);
            doc.Content[0].Kind().Should().Be(SyntaxKind.XmlText);
            doc.Content[0].HasLeadingTrivia.Should().BeTrue();
            doc.Content[1].Kind().Should().Be(SyntaxKind.XmlEmptyElement);
            var element = (XmlEmptyElementSyntax)doc.Content[1];
            element.Attributes.Count.Should().Be(1);
            var attribute = (XmlTextAttributeSyntax)element.Attributes[0];
            attribute.TextTokens.Count.Should().Be(3);
            attribute.TextTokens[0].Value.Should().Be(" ");
            attribute.TextTokens[1].Value.Should().Be("<");
            attribute.TextTokens[2].Value.Should().Be(" ");
        }

        [Fact]
        public void TestPredefinedXmlEntityInAttributeDelimited()
        {
            var text =
@"/** <goo a="" &lt; ""/>*/";
            var tree = Parse(text);
            tree.Should().NotBeNull();
            tree.GetCompilationUnitRoot().ToFullString().Should().Be(text);
            tree.GetCompilationUnitRoot().Errors().Length.Should().Be(0);
            var leading = tree.GetCompilationUnitRoot().EndOfFileToken.GetLeadingTrivia();
            leading.Count.Should().Be(1);
            var node = leading[0];
            node.Kind().Should().Be(SyntaxKind.MultiLineDocumentationCommentTrivia);
            node.ToFullString().Should().Be(text);
            var doc = (DocumentationCommentTriviaSyntax)node.GetStructure();
            doc.Content.Count.Should().Be(2);
            doc.Content[0].Kind().Should().Be(SyntaxKind.XmlText);
            doc.Content[0].HasLeadingTrivia.Should().BeTrue();
            doc.Content[1].Kind().Should().Be(SyntaxKind.XmlEmptyElement);
            var element = (XmlEmptyElementSyntax)doc.Content[1];
            element.Attributes.Count.Should().Be(1);
            var attribute = (XmlTextAttributeSyntax)element.Attributes[0];
            attribute.TextTokens.Count.Should().Be(3);
            attribute.TextTokens[0].Value.Should().Be(" ");
            attribute.TextTokens[1].Value.Should().Be("<");
            attribute.TextTokens[2].Value.Should().Be(" ");
        }

        [WorkItem(899590, "DevDiv/Personal")]
        [Fact]
        public void TestLessThanInAttributeTextIsError()
        {
            var text = @"/// <goo a = '<>'/>";
            var tree = Parse(text);
            tree.Should().NotBeNull();
            tree.GetCompilationUnitRoot().ToFullString().Should().Be(text);
            tree.GetCompilationUnitRoot().ErrorsAndWarnings().Length.Should().NotBe(0);
            var leading = tree.GetCompilationUnitRoot().EndOfFileToken.GetLeadingTrivia();
            leading.Count.Should().Be(1);
            var node = leading[0];
            node.Kind().Should().Be(SyntaxKind.SingleLineDocumentationCommentTrivia);
            node.ToFullString().Should().Be(text);
            var doc = (DocumentationCommentTriviaSyntax)node.GetStructure();
            doc.Content.Count.Should().Be(2);
            doc.Content[0].Kind().Should().Be(SyntaxKind.XmlText);
            doc.Content[1].Kind().Should().Be(SyntaxKind.XmlEmptyElement);
            var element = (XmlEmptyElementSyntax)doc.Content[1];
            element.Attributes.Count.Should().Be(1);
            element.Attributes[0].ErrorsAndWarnings().Length.Should().NotBe(0);
        }

        [WorkItem(899559, "DevDiv/Personal")]
        [Fact]
        public void TestNoZeroWidthTrivia()
        {
            var text =
@"/**
x
*/";
            var tree = Parse(text);
            tree.Should().NotBeNull();
            tree.GetCompilationUnitRoot().ToFullString().Should().Be(text);
            tree.GetCompilationUnitRoot().Errors().Length.Should().Be(0);
            var leading = tree.GetCompilationUnitRoot().EndOfFileToken.GetLeadingTrivia();
            leading.Count.Should().Be(1);
            var node = leading[0];
            node.Kind().Should().Be(SyntaxKind.MultiLineDocumentationCommentTrivia);
            node.ToFullString().Should().Be(text);
            var doc = (DocumentationCommentTriviaSyntax)node.GetStructure();
            doc.Content.Count.Should().Be(1);
            doc.Content[0].Kind().Should().Be(SyntaxKind.XmlText);
            var xmltext = (XmlTextSyntax)doc.Content[0];
            xmltext.ChildNodesAndTokens().Count.Should().Be(3);
            xmltext.ChildNodesAndTokens()[0].Kind().Should().Be(SyntaxKind.XmlTextLiteralNewLineToken);
            xmltext.ChildNodesAndTokens()[0].HasLeadingTrivia.Should().BeTrue();
            xmltext.ChildNodesAndTokens()[0].ToString().Should().Be(Environment.NewLine);
            xmltext.ChildNodesAndTokens()[1].Kind().Should().Be(SyntaxKind.XmlTextLiteralToken);
            xmltext.ChildNodesAndTokens()[1].HasLeadingTrivia.Should().BeFalse();
            xmltext.ChildNodesAndTokens()[1].ToString().Should().Be("x");
            xmltext.ChildNodesAndTokens()[2].Kind().Should().Be(SyntaxKind.XmlTextLiteralNewLineToken);
            xmltext.ChildNodesAndTokens()[2].HasLeadingTrivia.Should().BeFalse();
            xmltext.ChildNodesAndTokens()[2].ToString().Should().Be(Environment.NewLine);
        }

        [WorkItem(906364, "DevDiv/Personal")]
        [Fact]
        public void TestXmlAttributeWithoutEqualSign()
        {
            var text = @"/// <goo a""as""> </goo>";

            var tree = Parse(text);
            tree.Should().NotBeNull();
            tree.GetCompilationUnitRoot().ToFullString().Should().Be(text);

            // we should get a warning about the = token missing
            tree.GetCompilationUnitRoot().Warnings().Length.Should().Be(1);

            // we expect one warning
            VerifyDiagnostics(tree.GetCompilationUnitRoot(), new List<TestError>() { new TestError(1570, true) });
        }

        [WorkItem(906367, "DevDiv/Personal")]
        [Fact]
        public void TestXmlAttributeWithoutWhitespaceSeparators()
        {
            var text = @"/// <goo a=""as""b=""as""> </goo>";

            var tree = Parse(text);
            tree.Should().NotBeNull();
            tree.GetCompilationUnitRoot().ToFullString().Should().Be(text);

            // we should get a warning about the = token missing
            tree.GetCompilationUnitRoot().Warnings().Length.Should().Be(1);

            // we expect one warning
            VerifyDiagnostics(tree.GetCompilationUnitRoot(), new List<TestError>() { new TestError(1570, true) });
        }

        [Fact]
        public void TestSingleLineXmlCommentBetweenRegularComments()
        {
            var text = @"//Comment
/// <goo a=""as""> </goo>
//Comment
";

            var tree = Parse(text);
            tree.Should().NotBeNull();
            tree.GetCompilationUnitRoot().ToFullString().Should().Be(text);
            tree.GetCompilationUnitRoot().Errors().Length.Should().Be(0);

            // grab the trivia off the EOF token
            var trivias = tree.GetCompilationUnitRoot().EndOfFileToken.GetLeadingTrivia();

            // we should have 5 trivias
            trivias.Count.Should().Be(5);

            // we verify that the regular comments are also there
            trivias[0].HasStructure.Should().BeFalse();
            trivias[4].HasStructure.Should().BeFalse();

            // the 3rd one should be XmlDocComment
            trivias[2].HasStructure.Should().BeTrue();
            trivias[2].GetStructure().GetType().Should().Be(typeof(DocumentationCommentTriviaSyntax));
            var doc = trivias[2].GetStructure() as DocumentationCommentTriviaSyntax;

            // we validate the xml comment
            var xmlElement = doc.Content[1] as XmlElementSyntax;

            // we verify the content of the tag
            VerifyXmlElement(xmlElement, "goo", " ");
            VerifyXmlAttributes(xmlElement.StartTag.Attributes, new Dictionary<string, string>() { { "a", "as" } });
        }

        [Fact]
        public void TestSingleLineXmlCommentAfterMultilineXmlComment()
        {
            var text = @"/** <bar a='val'> 
* text
* </bar>
*/

/// <goo a=""as""> </goo>
";

            var tree = Parse(text);
            tree.Should().NotBeNull();
            tree.GetCompilationUnitRoot().ToFullString().Should().Be(text);
            tree.GetCompilationUnitRoot().Errors().Length.Should().Be(0);

            // grab the trivia off the EOF token
            var trivias = tree.GetCompilationUnitRoot().EndOfFileToken.GetLeadingTrivia();

            // we should have 4 trivias
            trivias.Count.Should().Be(4);

            // we should also have two xml comments
            trivias[0].GetStructure().GetType().Should().Be(typeof(DocumentationCommentTriviaSyntax));
            trivias[3].GetStructure().GetType().Should().Be(typeof(DocumentationCommentTriviaSyntax));

            // we grab the xml comments
            var firstComment = trivias[0].GetStructure() as DocumentationCommentTriviaSyntax;
            var secondComment = trivias[3].GetStructure() as DocumentationCommentTriviaSyntax;

            // verify that the xml elements contain the right info
            VerifyXmlElement(firstComment.Content[1] as XmlElementSyntax, "bar", @" 
* text
* ");
            VerifyXmlAttributes((firstComment.Content[1] as XmlElementSyntax).StartTag.Attributes, new Dictionary<string, string>() { { "a", "val" } });

            VerifyXmlElement(secondComment.Content[1] as XmlElementSyntax, "goo", " ");
            VerifyXmlAttributes((secondComment.Content[1] as XmlElementSyntax).StartTag.Attributes, new Dictionary<string, string>() { { "a", "as" } });
        }

        [Fact]
        public void TestSingleLineXmlCommentAfterInvalidMultilineXmlComment()
        {
            var text = @"/** <bar a='val'> 
* text
*/

/// <goo a=""as""> </goo>
";

            var tree = Parse(text);

            tree.Should().NotBeNull();
            tree.GetCompilationUnitRoot().ToFullString().Should().Be(text);

            // we expect 1 warning
            tree.GetCompilationUnitRoot().Warnings().Length.Should().Be(1);

            // grab the trivia off the EOF token
            var trivias = tree.GetCompilationUnitRoot().EndOfFileToken.GetLeadingTrivia();

            // we should have 4 trivias
            trivias.Count.Should().Be(4);

            // we should also have two xml comments
            trivias[0].GetStructure().GetType().Should().Be(typeof(DocumentationCommentTriviaSyntax));
            trivias[3].GetStructure().GetType().Should().Be(typeof(DocumentationCommentTriviaSyntax));

            // we grab the xml comments
            var firstComment = trivias[0].GetStructure() as DocumentationCommentTriviaSyntax;
            var secondComment = trivias[3].GetStructure() as DocumentationCommentTriviaSyntax;

            // we validate that the error is on the firstComment node
            firstComment.GetDiagnostics().Verify(
                // (3,1): warning CS1570: XML comment has badly formed XML -- 'Expected an end tag for element 'bar'.'
                // */
                Diagnostic(ErrorCode.WRN_XMLParseError, "").WithArguments("bar"));

            // verify that the xml elements contain the right info
            VerifyXmlElement(secondComment.Content[1] as XmlElementSyntax, "goo", " ");
            VerifyXmlAttributes((secondComment.Content[1] as XmlElementSyntax).StartTag.Attributes, new Dictionary<string, string>() { { "a", "as" } });
        }

        [Fact]
        public void TestSingleLineXmlCommentBeforeMethodDecl()
        {
            var text = @"class C{
///<goo a=""val""/>
  void Goo(){}
}";

            var tree = Parse(text);

            tree.Should().NotBeNull();
            tree.GetCompilationUnitRoot().ToFullString().Should().Be(text);
            tree.GetCompilationUnitRoot().Errors().Length.Should().Be(0);

            // we grab the void keyword
            (tree.GetCompilationUnitRoot().Members[0] as TypeDeclarationSyntax).Members[0].GetType().Should().Be(typeof(MethodDeclarationSyntax));

            var keyword = ((tree.GetCompilationUnitRoot().Members[0] as TypeDeclarationSyntax).Members[0] as MethodDeclarationSyntax).ReturnType;

            var trivias = keyword.GetLeadingTrivia();

            // we should have 2 trivias
            trivias.Count.Should().Be(2);

            // we should also have one comment
            trivias[0].GetStructure().GetType().Should().Be(typeof(DocumentationCommentTriviaSyntax));

            // we grab the xml comments
            var firstComment = trivias[0].GetStructure() as DocumentationCommentTriviaSyntax;

            // verify that the xml elements contain the right info
            VerifyXmlElement(firstComment.Content[0] as XmlEmptyElementSyntax, "goo");
            VerifyXmlAttributes((firstComment.Content[0] as XmlEmptyElementSyntax).Attributes, new Dictionary<string, string>() { { "a", "val" } });
        }

        [Fact]
        public void TestSingleLineXmlCommentBeforeGenericMethodDecl()
        {
            var text = @"class C{
///<goo a=""val""> </goo>
  void Goo<T>(){}
}";

            var tree = Parse(text);

            tree.Should().NotBeNull();
            tree.GetCompilationUnitRoot().ToFullString().Should().Be(text);
            tree.GetCompilationUnitRoot().Errors().Length.Should().Be(0);

            // we grab the void keyword
            (tree.GetCompilationUnitRoot().Members[0] as TypeDeclarationSyntax).Members[0].GetType().Should().Be(typeof(MethodDeclarationSyntax));

            var keyword = ((tree.GetCompilationUnitRoot().Members[0] as TypeDeclarationSyntax).Members[0] as MethodDeclarationSyntax).ReturnType;

            var trivias = keyword.GetLeadingTrivia();

            // we should have 2 trivias
            trivias.Count.Should().Be(2);

            // we should also have one comment
            trivias[0].GetStructure().GetType().Should().Be(typeof(DocumentationCommentTriviaSyntax));

            // we grab the xml comments
            var firstComment = trivias[0].GetStructure() as DocumentationCommentTriviaSyntax;

            // verify that the xml elements contain the right info
            VerifyXmlElement(firstComment.Content[0] as XmlElementSyntax, "goo", " ");
            VerifyXmlAttributes((firstComment.Content[0] as XmlElementSyntax).StartTag.Attributes, new Dictionary<string, string>() { { "a", "val" } });
        }

        [Fact]
        public void TestSingleLineXmlCommentBeforePropertyDecl()
        {
            var text = @"class C{
///<goo a=""val""/>
  int Goo{get;set;}
}";

            var tree = Parse(text);

            tree.Should().NotBeNull();
            tree.GetCompilationUnitRoot().ToFullString().Should().Be(text);
            tree.GetCompilationUnitRoot().Errors().Length.Should().Be(0);

            // we grab the void keyword
            (tree.GetCompilationUnitRoot().Members[0] as TypeDeclarationSyntax).Members[0].GetType().Should().Be(typeof(PropertyDeclarationSyntax));

            var keyword = ((tree.GetCompilationUnitRoot().Members[0] as TypeDeclarationSyntax).Members[0] as PropertyDeclarationSyntax).Type;

            var trivias = keyword.GetLeadingTrivia();

            // we should have 2 trivias
            trivias.Count.Should().Be(2);

            // we should also have one comment
            trivias[0].GetStructure().GetType().Should().Be(typeof(DocumentationCommentTriviaSyntax));

            // we grab the xml comments
            var firstComment = trivias[0].GetStructure() as DocumentationCommentTriviaSyntax;

            // verify that the xml elements contain the right info
            VerifyXmlElement(firstComment.Content[0] as XmlEmptyElementSyntax, "goo");
            VerifyXmlAttributes((firstComment.Content[0] as XmlEmptyElementSyntax).Attributes, new Dictionary<string, string>() { { "a", "val" } });
        }

        [Fact]
        public void TestSingleLineXmlCommentBeforeIndexerDecl()
        {
            var text = @"class C{
///<goo a=""val""/>
  int this[int x] { get { return 1; } set { } }
}";

            var tree = Parse(text);

            tree.Should().NotBeNull();
            tree.GetCompilationUnitRoot().ToFullString().Should().Be(text);
            tree.GetCompilationUnitRoot().Errors().Length.Should().Be(0);

            // we grab the void keyword
            (tree.GetCompilationUnitRoot().Members[0] as TypeDeclarationSyntax).Members[0].GetType().Should().Be(typeof(IndexerDeclarationSyntax));

            var keyword = ((tree.GetCompilationUnitRoot().Members[0] as TypeDeclarationSyntax).Members[0] as IndexerDeclarationSyntax).Type;

            var trivias = keyword.GetLeadingTrivia();

            // we should have 2 trivias
            trivias.Count.Should().Be(2);

            // we should also have one comment
            trivias[0].GetStructure().GetType().Should().Be(typeof(DocumentationCommentTriviaSyntax));

            // we grab the xml comments
            var firstComment = trivias[0].GetStructure() as DocumentationCommentTriviaSyntax;

            // verify that the xml elements contain the right info
            VerifyXmlElement(firstComment.Content[0] as XmlEmptyElementSyntax, "goo");
            VerifyXmlAttributes((firstComment.Content[0] as XmlEmptyElementSyntax).Attributes, new Dictionary<string, string>() { { "a", "val" } });
        }

        [WorkItem(906381, "DevDiv/Personal")]
        [Fact]
        public void TestMultiLineXmlCommentBeforeGenericTypeParameterOnMethodDecl()
        {
            var text = @"class C {
    void Goo</**<goo>test</goo>*/T>() { }
}";

            var tree = Parse(text);

            tree.Should().NotBeNull();
            tree.GetCompilationUnitRoot().ToFullString().Should().Be(text);
            tree.GetCompilationUnitRoot().Errors().Length.Should().Be(0);

            // do we parsed a method?
            (tree.GetCompilationUnitRoot().Members[0] as TypeDeclarationSyntax).Members[0].GetType().Should().Be(typeof(MethodDeclarationSyntax));

            // we grab the open bracket for the Goo method decl
            var method = (tree.GetCompilationUnitRoot().Members[0] as TypeDeclarationSyntax).Members[0] as MethodDeclarationSyntax;
            var typeParameter = method.TypeParameterList.Parameters.Single();

            var trivias = typeParameter.GetLeadingTrivia();

            // we should have 1 trivia
            trivias.Count.Should().Be(1);

            // we should also have one XML comment
            trivias[0].GetStructure().GetType().Should().Be(typeof(DocumentationCommentTriviaSyntax));

            // we grab the xml comments
            var firstComment = trivias[0].GetStructure() as DocumentationCommentTriviaSyntax;

            // verify that the xml elements contain the right info
            VerifyXmlElement(firstComment.Content[0] as XmlElementSyntax, "goo", "test");

            // we don't have any attributes
            (firstComment.Content[0] as XmlElementSyntax).StartTag.Attributes.Count.Should().Be(0);
        }

        [WorkItem(906381, "DevDiv/Personal")]
        [Fact]
        public void TestMultiLineXmlCommentBeforeGenericTypeParameterOnClassDecl()
        {
            var text = @"class C</**<goo>test</goo>*/T>{}";

            var tree = Parse(text);

            tree.Should().NotBeNull();
            tree.GetCompilationUnitRoot().ToFullString().Should().Be(text);
            tree.GetCompilationUnitRoot().Errors().Length.Should().Be(0);

            // do we parsed a method?
            tree.GetCompilationUnitRoot().Members[0].GetType().Should().Be(typeof(ClassDeclarationSyntax));

            // we grab the open bracket for the Goo method decl
            var typeParameter = (tree.GetCompilationUnitRoot().Members[0] as TypeDeclarationSyntax).TypeParameterList.Parameters.Single();

            var trivias = typeParameter.GetLeadingTrivia();

            // we should have 1 trivia
            trivias.Count.Should().Be(1);

            // we should also have one XML comment
            trivias[0].GetStructure().GetType().Should().Be(typeof(DocumentationCommentTriviaSyntax));

            // we grab the xml comments
            var firstComment = trivias[0].GetStructure() as DocumentationCommentTriviaSyntax;

            // verify that the xml elements contain the right info
            VerifyXmlElement(firstComment.Content[0] as XmlElementSyntax, "goo", "test");

            // we don't have any attributes
            (firstComment.Content[0] as XmlElementSyntax).StartTag.Attributes.Count.Should().Be(0);
        }

        [Fact]
        public void TestSingleLineXmlCommentBeforeIncompleteGenericMethodDecl()
        {
            var text = @"class C{
///<goo a=""val""> </goo>
  void Goo<T(){}
}";

            var tree = Parse(text);

            tree.Should().NotBeNull();
            tree.GetCompilationUnitRoot().ToFullString().Should().Be(text);
            tree.GetCompilationUnitRoot().Errors().Length.Should().Be(1); // 4 errors because of the incomplete class decl

            // we grab the void keyword
            (tree.GetCompilationUnitRoot().Members[0] as TypeDeclarationSyntax).Members[0].GetType().Should().Be(typeof(MethodDeclarationSyntax));

            var keyword = ((tree.GetCompilationUnitRoot().Members[0] as TypeDeclarationSyntax).Members[0] as MethodDeclarationSyntax).ReturnType;

            var trivias = keyword.GetLeadingTrivia();

            // we should have 2 trivias
            trivias.Count.Should().Be(2);

            // we should also have one comment
            trivias[0].Errors().Length.Should().Be(0);
            trivias[0].GetStructure().GetType().Should().Be(typeof(DocumentationCommentTriviaSyntax));

            // we grab the xml comments
            var firstComment = trivias[0].GetStructure() as DocumentationCommentTriviaSyntax;

            // verify that the xml elements contain the right info
            VerifyXmlElement(firstComment.Content[0] as XmlElementSyntax, "goo", " ");
            VerifyXmlAttributes((firstComment.Content[0] as XmlElementSyntax).StartTag.Attributes, new Dictionary<string, string>() { { "a", "val" } });
        }

        [Fact]
        public void TestSingleLineXmlCommentAfterMethodDecl()
        {
            var text = @"class C{
  void Goo(){}
///<goo a=""val""/>
}";

            var tree = Parse(text);

            tree.Should().NotBeNull();
            tree.GetCompilationUnitRoot().ToFullString().Should().Be(text);
            tree.GetCompilationUnitRoot().Errors().Length.Should().Be(0);

            var bracket = (tree.GetCompilationUnitRoot().Members[0] as TypeDeclarationSyntax).CloseBraceToken;

            var trivias = bracket.GetLeadingTrivia();

            // we should have 2 trivias
            trivias.Count.Should().Be(1);

            // we should also have one comment
            trivias[0].GetStructure().GetType().Should().Be(typeof(DocumentationCommentTriviaSyntax));

            // we grab the xml comments
            var firstComment = trivias[0].GetStructure() as DocumentationCommentTriviaSyntax;

            // verify that the xml elements contain the right info
            VerifyXmlElement(firstComment.Content[0] as XmlEmptyElementSyntax, "goo");
            VerifyXmlAttributes((firstComment.Content[0] as XmlEmptyElementSyntax).Attributes, new Dictionary<string, string>() { { "a", "val" } });
        }

        [Fact]
        public void TestSingleLineXmlCommentAfterIncompleteMethodDecl()
        {
            var text = @"class C{
  void Goo({}
///<goo a=""val""> </goo>
}";
            var tree = Parse(text);

            tree.Should().NotBeNull();
            tree.GetCompilationUnitRoot().ToFullString().Should().Be(text);
            tree.GetCompilationUnitRoot().Errors().Length.Should().Be(1); // error because of the incomplete class decl

            // we grab the close bracket for the class
            var classDecl = (TypeDeclarationSyntax)tree.GetCompilationUnitRoot().Members[0];
            var bracket = classDecl.CloseBraceToken;

            var trivias = bracket.GetLeadingTrivia();

            // we should have 2 trivias
            trivias.Count.Should().Be(1);

            // we should also have one comment
            trivias[0].Errors().Length.Should().Be(0);
            trivias[0].GetStructure().GetType().Should().Be(typeof(DocumentationCommentTriviaSyntax));

            // we grab the xml comments
            var firstComment = trivias[0].GetStructure() as DocumentationCommentTriviaSyntax;

            // verify that the xml elements contain the right info
            VerifyXmlElement(firstComment.Content[0] as XmlElementSyntax, "goo", " ");
            VerifyXmlAttributes((firstComment.Content[0] as XmlElementSyntax).StartTag.Attributes, new Dictionary<string, string>() { { "a", "val" } });
        }

        [Fact]
        public void TestSingleLineXmlCommentBeforePreprocessorDirective()
        {
            var text = @"///<goo></goo>
# if DOODAD
# endif";
            var tree = Parse(text);

            tree.Should().NotBeNull();
            tree.GetCompilationUnitRoot().ToFullString().Should().Be(text);
            tree.GetCompilationUnitRoot().Errors().Length.Should().Be(0); // 4 errors because of the incomplete class decl

            // we grab the close bracket from the OEF token
            var bracket = tree.GetCompilationUnitRoot().EndOfFileToken;

            var trivias = bracket.GetLeadingTrivia();

            // we should have 3 trivias
            trivias.Count.Should().Be(3);

            trivias[0].Errors().Length.Should().Be(0);
            trivias[0].GetStructure().GetType().Should().Be(typeof(DocumentationCommentTriviaSyntax));

            // we grab the xml comments
            var firstComment = trivias[0].GetStructure() as DocumentationCommentTriviaSyntax;

            // verify that the xml elements contain the right info
            VerifyXmlElement(firstComment.Content[0] as XmlElementSyntax, "goo", string.Empty);
        }

        [Fact]
        public void TestSingleLineXmlCommentAfterPreprocessorDirective()
        {
            var text = @"# if DOODAD
# endif
///<goo></goo>";
            var tree = Parse(text);

            tree.Should().NotBeNull();
            tree.GetCompilationUnitRoot().ToFullString().Should().Be(text);
            tree.GetCompilationUnitRoot().Errors().Length.Should().Be(0); // 4 errors because of the incomplete class decl

            // we grab the close bracket from the OEF token
            var bracket = tree.GetCompilationUnitRoot().EndOfFileToken;

            var trivias = bracket.GetLeadingTrivia();

            // we should have 3 trivias
            trivias.Count.Should().Be(3);

            trivias[0].Errors().Length.Should().Be(0);
            trivias[2].GetStructure().GetType().Should().Be(typeof(DocumentationCommentTriviaSyntax));

            // we grab the xml comments
            var firstComment = trivias[2].GetStructure() as DocumentationCommentTriviaSyntax;

            // verify that the xml elements contain the right info
            VerifyXmlElement(firstComment.Content[0] as XmlElementSyntax, "goo", string.Empty);
        }

        [Fact]
        public void TestSingleLineXmlCommentInsideMultiLineXmlComment()
        {
            var text = @"/** <goo> 
* /// <bar> </bar>
* </goo>
*/";
            var tree = Parse(text);

            tree.Should().NotBeNull();
            tree.GetCompilationUnitRoot().ToFullString().Should().Be(text);
            tree.GetCompilationUnitRoot().Errors().Length.Should().Be(0);

            // we grab the close bracket from the OEF token
            var bracket = tree.GetCompilationUnitRoot().EndOfFileToken;

            var trivias = bracket.GetLeadingTrivia();

            // we should have 2 trivias
            trivias.Count.Should().Be(1);
            trivias[0].Errors().Length.Should().Be(0);

            // make sure that the external node exists
            trivias[0].GetStructure().GetType().Should().Be(typeof(DocumentationCommentTriviaSyntax));

            // we grab the xml comments
            var outerComment = (trivias[0].GetStructure() as DocumentationCommentTriviaSyntax).Content[1] as XmlElementSyntax;
            var innerComment = outerComment.Content[1] as XmlElementSyntax;

            // verify that the xml elements contain the right info
            VerifyXmlElement(outerComment, "goo", @" 
* /// <bar> </bar>
* ");

            VerifyXmlElement(innerComment, "bar", " ");
        }

        [WorkItem(906500, "DevDiv/Personal")]
        [Fact]
        public void TestIncompleteMultiLineXmlComment()
        {
            var text = @"/** <goo/>";
            var tree = Parse(text);

            tree.Should().NotBeNull();
            tree.GetCompilationUnitRoot().ToFullString().Should().Be(text);

            // we expect one warning
            tree.GetCompilationUnitRoot().Errors().Length.Should().Be(1);
            VerifyDiagnostics(tree.GetCompilationUnitRoot(), new List<TestError>() { new TestError(1035, false) });
        }

        [Fact]
        public void TestSingleLineXmlCommentWithMultipleAttributes()
        {
            var text = @"///<goo attr1=""a"" attr2=""b"" attr3=""test""> </goo>
class C{}";
            var tree = Parse(text);

            tree.Should().NotBeNull();
            tree.GetCompilationUnitRoot().ToFullString().Should().Be(text);
            tree.GetCompilationUnitRoot().Errors().Length.Should().Be(0);

            var classKeyword = (tree.GetCompilationUnitRoot().Members[0] as TypeDeclarationSyntax).Keyword;

            var trivias = classKeyword.GetLeadingTrivia();

            trivias.Count.Should().Be(1);

            // we verify that we parsed a correct XML element
            VerifyXmlElement((trivias[0].GetStructure() as DocumentationCommentTriviaSyntax).Content[0] as XmlElementSyntax, "goo", " ");

            VerifyXmlAttributes(((trivias[0].GetStructure() as DocumentationCommentTriviaSyntax).Content[0] as XmlElementSyntax).StartTag.Attributes,
                new Dictionary<string, string>() { { "attr1", "a" }, { "attr2", "b" }, { "attr3", "test" } });
        }

        [Fact]
        public void TestNestedXmlTagsInsideSingleLineXmlDocComment()
        {
            var text = @"///<goo>
/// <bar>
///  <baz attr=""a"">
///  </baz>
/// </bar>
///</goo>";
            var tree = Parse(text);

            tree.Should().NotBeNull();
            tree.GetCompilationUnitRoot().ToFullString().Should().Be(text);
            tree.GetCompilationUnitRoot().Errors().Length.Should().Be(0);

            // we grab the top trivia.
            var eofToken = tree.GetCompilationUnitRoot().EndOfFileToken;

            var topTrivias = eofToken.GetLeadingTrivia();
            topTrivias.Count.Should().Be(1);

            var doc = topTrivias[0].GetStructure() as DocumentationCommentTriviaSyntax;

            var topTriviaElement = doc.Content[0] as XmlElementSyntax;
            VerifyXmlElement(topTriviaElement, "goo", @"
/// <bar>
///  <baz attr=""a"">
///  </baz>
/// </bar>
");
            var secondLevelTrivia = topTriviaElement.Content[1] as XmlElementSyntax;
            VerifyXmlElement(secondLevelTrivia, "bar", @"
///  <baz attr=""a"">
///  </baz>
/// ");

            var thirdLevelTrivia = secondLevelTrivia.Content[1] as XmlElementSyntax;
            VerifyXmlElement(thirdLevelTrivia, "baz", @"
///  ");
            VerifyXmlAttributes(thirdLevelTrivia.StartTag.Attributes, new Dictionary<string, string>() { { "attr", "a" } });
        }

        [Fact]
        public void TestMultiLineXmlCommentWithNestedTagThatContainsCDATA()
        {
            var text = @"/**
<goo>
  <bar> <![CDATA[ Some text
 ]]> </bar>
</goo>
*/";
            var tree = Parse(text);

            tree.Should().NotBeNull();
            tree.GetCompilationUnitRoot().ToFullString().Should().Be(text);
            tree.GetCompilationUnitRoot().Errors().Length.Should().Be(0);

            var eofToken = tree.GetCompilationUnitRoot().EndOfFileToken;

            var trivias = eofToken.GetLeadingTrivia();

            var doc = trivias[0].GetStructure() as DocumentationCommentTriviaSyntax;

            var topNode = doc.Content[1] as XmlElementSyntax;
            VerifyXmlElement(topNode, "goo", @"
  <bar> <![CDATA[ Some text
 ]]> </bar>
");
            var secondLevel = topNode.Content[1] as XmlElementSyntax;
            VerifyXmlElement(secondLevel, "bar", @" <![CDATA[ Some text
 ]]> ");

            // verify the CDATA content
            var cdata = secondLevel.Content[1];
            var actual = (cdata as XmlCDataSectionSyntax).TextTokens.ToFullString();
            actual.Should().Be(@" Some text
");
        }

        [Fact]
        public void TestSingleLineXmlCommentWithMismatchedUpperLowerCaseTagName()
        {
            var text = @"///<goo> </Goo>";
            var tree = Parse(text);

            tree.Should().NotBeNull();
            tree.GetCompilationUnitRoot().ToFullString().Should().Be(text);

            // we should get a warning
            tree.GetCompilationUnitRoot().Warnings().Length.Should().Be(1);

            // we get to the xml trivia
            var eofToken = tree.GetCompilationUnitRoot().EndOfFileToken;
            var trivias = eofToken.GetLeadingTrivia();

            // we got a trivia
            trivias.Count.Should().Be(1);

            VerifyDiagnostics(trivias[0], new List<TestError>() { new TestError(1570, true) });
        }

        [WorkItem(906704, "DevDiv/Personal")]
        [Fact]
        public void TestSingleLineXmlCommentWithMissingStartTag()
        {
            var text = @"///</Goo>
class C{}";
            var tree = Parse(text);

            tree.Should().NotBeNull();
            tree.GetCompilationUnitRoot().ToFullString().Should().Be(text);
            tree.GetCompilationUnitRoot().ErrorsAndWarnings().Length.Should().Be(1);

            // we get to the xml trivia
            var classKeyword = (tree.GetCompilationUnitRoot().Members[0] as TypeDeclarationSyntax).Keyword;

            // we get the xmldoc comment
            var doc = classKeyword.GetLeadingTrivia()[0].GetStructure() as DocumentationCommentTriviaSyntax;

            // we get the xmlText
            var xmlText = doc.Content[0] as XmlTextSyntax;

            // we have an error on that node
            VerifyDiagnostics(xmlText, new List<TestError>() { new TestError(1570, true) });

            // we should get just 2 nodes
            xmlText.TextTokens.Count.Should().Be(2);

            xmlText.TextTokens.ToFullString().Should().Be($"///</Goo>{Environment.NewLine}");
        }

        [WorkItem(906719, "DevDiv/Personal")]
        [Fact]
        public void TestMultiLineXmlCommentWithMissingStartTag()
        {
            var text = @"/**</Goo>*/
class C{}";
            var tree = Parse(text);

            tree.Should().NotBeNull();
            tree.GetCompilationUnitRoot().ToFullString().Should().Be(text);
            tree.GetCompilationUnitRoot().ErrorsAndWarnings().Length.Should().Be(1);

            // we get to the xml trivia
            var classKeyword = (tree.GetCompilationUnitRoot().Members[0] as TypeDeclarationSyntax).Keyword;

            // we get the xmldoc comment
            var doc = classKeyword.GetLeadingTrivia()[0].GetStructure() as DocumentationCommentTriviaSyntax;

            // we get the xmlText
            var xmlText = doc.Content[0] as XmlTextSyntax;

            // we have an error on that node
            VerifyDiagnostics(xmlText, new List<TestError>() { new TestError(1570, true) });

            // we should get just 2 nodes
            xmlText.TextTokens.Count.Should().Be(1);

            xmlText.TextTokens.ToFullString().Should().Be("/**</Goo>");
        }

        [Fact]
        public void TestSingleLineXmlCommentWithMissingEndTag()
        {
            var text = @"///<Goo>
class C{}";
            var tree = Parse(text);

            tree.Should().NotBeNull();
            tree.GetCompilationUnitRoot().ToFullString().Should().Be(text);

            var classKeyword = (tree.GetCompilationUnitRoot().Members[0] as TypeDeclarationSyntax).Keyword;

            var trivias = classKeyword.GetLeadingTrivia();

            var doc = trivias[0].GetStructure() as DocumentationCommentTriviaSyntax;

            doc.GetDiagnostics().Verify(
                // (2,1): warning CS1570: XML comment has badly formed XML -- 'Expected an end tag for element 'Goo'.'
                // class C{}
                Diagnostic(ErrorCode.WRN_XMLParseError, "").WithArguments("Goo"));
        }

        [WorkItem(906752, "DevDiv/Personal")]
        [Fact]
        public void TestMultiLineXmlCommentWithMissingEndTag()
        {
            var text = @"/**<Goo>*/
class C{}";
            var tree = Parse(text);

            tree.Should().NotBeNull();
            tree.GetCompilationUnitRoot().ToFullString().Should().Be(text);

            // we should get 1 warning
            tree.GetCompilationUnitRoot().Warnings().Length.Should().Be(1);

            var classKeyword = (tree.GetCompilationUnitRoot().Members[0] as TypeDeclarationSyntax).Keyword;

            var trivias = classKeyword.GetLeadingTrivia();

            var doc = trivias[0].GetStructure() as DocumentationCommentTriviaSyntax;

            doc.GetDiagnostics().Verify(
                // (1,9): warning CS1570: XML comment has badly formed XML -- 'Expected an end tag for element 'Goo'.'
                // /**<Goo>*/
                Diagnostic(ErrorCode.WRN_XMLParseError, "").WithArguments("Goo"));
        }

        [Fact]
        public void TestMultiLineXmlCommentWithInterleavedTags()
        {
            var text = @"/**<goo>
<bar></goo>
</bar>*/
class C{}";
            var tree = Parse(text);

            tree.Should().NotBeNull();
            tree.GetCompilationUnitRoot().ToFullString().Should().Be(text);

            // we should get 2 warnings
            tree.GetCompilationUnitRoot().ErrorsAndWarnings().Length.Should().Be(2);

            var classKeyword = (tree.GetCompilationUnitRoot().Members[0] as TypeDeclarationSyntax).Keyword;

            var trivias = classKeyword.GetLeadingTrivia();

            var doc = trivias[0].GetStructure() as DocumentationCommentTriviaSyntax;

            // we have an error on that node
            VerifyDiagnostics(doc, new List<TestError>() { new TestError(1570, true), new TestError(1570, true) });
        }

        [Fact]
        public void TestSingleLineXmlCommentWithInterleavedTags()
        {
            var text = @"///<goo>
///<bar></goo>
///</bar>
class C{}";
            var tree = Parse(text);

            tree.Should().NotBeNull();
            tree.GetCompilationUnitRoot().ToFullString().Should().Be(text);

            // we should get 2 warnings
            tree.GetCompilationUnitRoot().Warnings().Length.Should().Be(2);

            var classKeyword = (tree.GetCompilationUnitRoot().Members[0] as TypeDeclarationSyntax).Keyword;

            var trivias = classKeyword.GetLeadingTrivia();

            var doc = trivias[0].GetStructure() as DocumentationCommentTriviaSyntax;

            // we have an error on that node
            VerifyDiagnostics(doc, new List<TestError>() { new TestError(1570, true), new TestError(1570, true) });
        }

        [Fact]
        public void TestMultiLineXmlCommentWithIncompleteInterleavedTags()
        {
            var text = @"/**<goo>
<bar></goo>
*/
class C{}";
            var tree = Parse(text);

            tree.Should().NotBeNull();
            tree.GetCompilationUnitRoot().ToFullString().Should().Be(text);

            var classKeyword = (tree.GetCompilationUnitRoot().Members[0] as TypeDeclarationSyntax).Keyword;

            var trivias = classKeyword.LeadingTrivia;

            var doc = trivias[0].GetStructure() as DocumentationCommentTriviaSyntax;

            doc.GetDiagnostics().Verify(
                // (2,8): warning CS1570: XML comment has badly formed XML -- 'End tag 'goo' does not match the start tag 'bar'.'
                // <bar></goo>
                Diagnostic(ErrorCode.WRN_XMLParseError, "goo").WithArguments("goo", "bar"),
                // (3,1): warning CS1570: XML comment has badly formed XML -- 'Expected an end tag for element 'goo'.'
                // */
                Diagnostic(ErrorCode.WRN_XMLParseError, "").WithArguments("goo"));
        }

        [Fact]
        public void TestSingleLineXmlCommentWithIncompleteInterleavedTags()
        {
            var text = @"///<goo>
///<bar></goo>
class C{}";
            var tree = Parse(text);

            tree.Should().NotBeNull();
            tree.GetCompilationUnitRoot().ToFullString().Should().Be(text);

            var classKeyword = (tree.GetCompilationUnitRoot().Members[0] as TypeDeclarationSyntax).Keyword;

            var trivias = classKeyword.GetLeadingTrivia();

            var doc = trivias[0].GetStructure() as DocumentationCommentTriviaSyntax;

            doc.GetDiagnostics().Verify(
                // (2,11): warning CS1570: XML comment has badly formed XML -- 'End tag 'goo' does not match the start tag 'bar'.'
                // ///<bar></goo>
                Diagnostic(ErrorCode.WRN_XMLParseError, "goo").WithArguments("goo", "bar"),
                // (3,1): warning CS1570: XML comment has badly formed XML -- 'Expected an end tag for element 'goo'.'
                // class C{}
                Diagnostic(ErrorCode.WRN_XMLParseError, "").WithArguments("goo"));
        }

        [Fact]
        public void TestMultiLineXmlCommentWithMultipleStartTokens()
        {
            var text = @"/** <a 
<b 
*/";
            var tree = Parse(text);

            tree.Should().NotBeNull();
            tree.GetCompilationUnitRoot().ToFullString().Should().Be(text);

            // we should get 2 warnings
            tree.GetCompilationUnitRoot().Warnings().Length.Should().Be(2);
        }

        [Fact]
        public void TestMultiLineXmlCommentWithMultipleEndTags()
        {
            var text = @"/** <a> </a> </a> 

*/";
            var tree = Parse(text);

            tree.Should().NotBeNull();
            tree.GetCompilationUnitRoot().ToFullString().Should().Be(text);

            // we should get 2 warnings
            tree.GetCompilationUnitRoot().Warnings().Length.Should().Be(1);
        }

        [Fact]
        public void TestMultiLineXmlCommentWithMultipleEndTags2()
        {
            var text = @"/** <a> </b> </a> 
*/";
            var tree = Parse(text);

            tree.Should().NotBeNull();
            tree.GetCompilationUnitRoot().ToFullString().Should().Be(text);

            // we should get 2 warnings
            tree.GetCompilationUnitRoot().Warnings().Length.Should().Be(2);
        }

        [WorkItem(906814, "DevDiv/Personal")]
        [Fact]
        public void TestSingleLineXmlCommentWithInvalidStringAttributeValue()
        {
            var text = @"///<goo a=""</>""> </goo> 
class C{}";
            var tree = Parse(text);

            tree.Should().NotBeNull();
            tree.GetCompilationUnitRoot().ToFullString().Should().Be(text);

            var classKeyword = (tree.GetCompilationUnitRoot().Members[0] as TypeDeclarationSyntax).Keyword;

            var doc = classKeyword.GetLeadingTrivia()[0].GetStructure() as DocumentationCommentTriviaSyntax;

            doc.Content[0].GetType().Should().Be(typeof(XmlElementSyntax));
        }

        [WorkItem(537113, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/537113")]
        [Fact]
        public void TestSingleLineXmlCommentWithAttributeWithoutQuotes()
        {
            var text = @"///<goo a=4></goo>
class C{}";
            var tree = Parse(text);

            tree.Should().NotBeNull();
            tree.GetCompilationUnitRoot().ToFullString().Should().Be(text);

            var classKeyword = (tree.GetCompilationUnitRoot().Members[0] as TypeDeclarationSyntax).Keyword;

            var doc = classKeyword.GetLeadingTrivia()[0].GetStructure() as DocumentationCommentTriviaSyntax;

            // we should still get an XmlElement
            doc.Content[0].Should().BeOfType<XmlElementSyntax>();
        }

        [WorkItem(926873, "DevDiv/Personal")]
        [Fact]
        public void TestSomeXmlEntities()
        {
            var text = @"/// <doc>
/// <line>&#1631;</line>
/// <line>&#x65f;</line>
/// </doc>
class A {}";

            var tree = Parse(text);

            tree.Should().NotBeNull();
            tree.GetCompilationUnitRoot().ToFullString().Should().Be(text);
            tree.GetCompilationUnitRoot().Errors().Length.Should().Be(0);
        }

        [WorkItem(926683, "DevDiv/Personal")]
        [Fact]
        public void TestSomeBadXmlEntities()
        {
            var text = @"/// &#1;<doc1>&#2;</doc1>
/// <doc2><![CDATA[&#5;&#31;]]></doc2>
/// <doc3 x = ""&#14;""></doc3>&#xffff;
/// <!-- &#xfffe; -->
class A {}
";

            var tree = Parse(text);

            tree.Should().NotBeNull();
            tree.GetCompilationUnitRoot().ToFullString().Should().Be(text);
            tree.GetCompilationUnitRoot().ErrorsAndWarnings().Length.Should().Be(4);
            VerifyDiagnostics(tree.GetCompilationUnitRoot(), new List<TestError>
            {
                    new TestError(1570, true),
                    new TestError(1570, true),
                    new TestError(1570, true),
                    new TestError(1570, true)
            });
        }

        [WorkItem(926804, "DevDiv/Personal")]
        [Fact]
        public void TestSomeBadWhitespaceInTags()
        {
            var text = @"/// < doc></doc>
/// <abc> </ abc>
/// < a/>
/// <
///  b></b>
class A {}
";

            var tree = Parse(text);

            tree.Should().NotBeNull();
            tree.GetCompilationUnitRoot().ToFullString().Should().Be(text);
            tree.GetCompilationUnitRoot().ErrorsAndWarnings().Length.Should().Be(4);
            VerifyDiagnostics(tree.GetCompilationUnitRoot(), new List<TestError>
            {
                    new TestError(1570, true),
                    new TestError(1570, true),
                    new TestError(1570, true),
                    new TestError(1570, true)
            });
        }

        [WorkItem(926807, "DevDiv/Personal")]
        [Fact]
        public void TestCDataEndTagInXmlText()
        {
            var text = @"/// <doc> ]]> </doc>
/// <a>abc]]]>def</a>
/// <a attr=""]]>""></a>
class A {}";

            var tree = Parse(text);

            tree.Should().NotBeNull();
            tree.GetCompilationUnitRoot().ToFullString().Should().Be(text);

            var classKeyword = (tree.GetCompilationUnitRoot().Members[0] as TypeDeclarationSyntax).Keyword;
            var trivias = classKeyword.GetLeadingTrivia();
            var doc = trivias[0].GetStructure() as DocumentationCommentTriviaSyntax;

            doc.ErrorsAndWarnings().Length.Should().Be(2);
            VerifyDiagnostics(doc, new List<TestError>() { new TestError(1570, true), new TestError(1570, true) });
        }

        [WorkItem(536748, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/536748")]
        [Fact]
        public void AttributesInEndTag()
        {
            var text = @"
/// <summary attr=""A"">
/// </summary attr=""A"">
class A
{ 
}
";
            var tree = Parse(text);

            tree.Should().NotBeNull();
            tree.GetCompilationUnitRoot().ToFullString().Should().Be(text);

            var classKeyword = (tree.GetCompilationUnitRoot().Members[0] as TypeDeclarationSyntax).Keyword;
            var trivias = classKeyword.GetLeadingTrivia();
            var doc = trivias[1].GetStructure() as DocumentationCommentTriviaSyntax;

            doc.Content.Count.Should().Be(3);
            doc.Content[0].Kind().Should().Be(SyntaxKind.XmlText);
            doc.Content[1].Kind().Should().Be(SyntaxKind.XmlElement);
            doc.Content[2].Kind().Should().Be(SyntaxKind.XmlText);
        }

        [WorkItem(546989, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/546989")]
        [Fact]
        public void NonAsciiQuotationMarks()
        {
            var text = @"
class A
{
    /// <see cref=”A()”/>
    /// <param name=”x”/>
    /// <other attr=”value”/>
    void M(int x) { }
}";

            var tree = Parse(text);
            tree.GetDiagnostics().Verify(
                // (4,19): warning CS1570: XML comment has badly formed XML -- 'Non-ASCII quotations marks may not be used around string literals.'
                //     /// <see cref=”A()”/>
                Diagnostic(ErrorCode.WRN_XMLParseError, ""),
                // (4,23): warning CS1570: XML comment has badly formed XML -- 'Non-ASCII quotations marks may not be used around string literals.'
                //     /// <see cref=”A()”/>
                Diagnostic(ErrorCode.WRN_XMLParseError, ""),

                // (5,21): warning CS1570: XML comment has badly formed XML -- 'Non-ASCII quotations marks may not be used around string literals.'
                //     /// <param name=”x”/>
                Diagnostic(ErrorCode.WRN_XMLParseError, ""),
                // (5,23): warning CS1570: XML comment has badly formed XML -- 'Non-ASCII quotations marks may not be used around string literals.'
                //     /// <param name=”x”/>
                Diagnostic(ErrorCode.WRN_XMLParseError, ""),

                // What's happening with the text attribute is that "”/>" is correctly (if unintuitively) being consumed as part of the
                // attribute value.  It then complains about the missing closing quotation mark and '/>'.

                // (6,21): warning CS1570: XML comment has badly formed XML -- 'Non-ASCII quotations marks may not be used around string literals.'
                //     /// <other attr=”value”/>
                Diagnostic(ErrorCode.WRN_XMLParseError, ""),
                // (7,1): warning CS1570: XML comment has badly formed XML -- 'Missing closing quotation mark for string literal.'
                //     void M(int x) { }
                Diagnostic(ErrorCode.WRN_XMLParseError, ""),
                // (7,1): warning CS1570: XML comment has badly formed XML -- 'Expected '>' or '/>' to close tag 'other'.'
                //     void M(int x) { }
                Diagnostic(ErrorCode.WRN_XMLParseError, "").WithArguments("other"));
        }

        [WorkItem(546989, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/546989")]
        [Fact]
        public void Microsoft_TeamFoundation_Client_Dll()
        {
            var text = @"
/// <summary></summary>
public class Program
{
     static void Main() { }
    /// <summary>
    /// GetEntityConnectionString from the selected path
    /// path is of the format <project name>\<nodename>\<nodename>
    /// </summary>
    /// <param name=”metadata”></param>
    /// <param name=”provider”></param>
    protected void GetEntityConnectionString(
        string metadata,
        string provider)
    {
    }
}";

            var tree = Parse(text);
            tree.GetDiagnostics().Verify(
                // (8,44): warning CS1570: XML comment has badly formed XML -- 'Missing equals sign between attribute and attribute value.'
                //     /// path is of the format <project name>\<nodename>\<nodename>
                Diagnostic(ErrorCode.WRN_XMLParseError, ""),
                // (9,11): warning CS1570: XML comment has badly formed XML -- 'End tag 'summary' does not match the start tag 'nodename'.'
                //     /// </summary>
                Diagnostic(ErrorCode.WRN_XMLParseError, "summary").WithArguments("summary", "nodename"),
                // (10,21): warning CS1570: XML comment has badly formed XML -- 'Non-ASCII quotations marks may not be used around string literals.'
                //     /// <param name=”metadata”></param>
                Diagnostic(ErrorCode.WRN_XMLParseError, ""),
                // (10,30): warning CS1570: XML comment has badly formed XML -- 'Non-ASCII quotations marks may not be used around string literals.'
                //     /// <param name=”metadata”></param>
                Diagnostic(ErrorCode.WRN_XMLParseError, ""),
                // (11,21): warning CS1570: XML comment has badly formed XML -- 'Non-ASCII quotations marks may not be used around string literals.'
                //     /// <param name=”provider”></param>
                Diagnostic(ErrorCode.WRN_XMLParseError, ""),
                // (11,30): warning CS1570: XML comment has badly formed XML -- 'Non-ASCII quotations marks may not be used around string literals.'
                //     /// <param name=”provider”></param>
                Diagnostic(ErrorCode.WRN_XMLParseError, ""),
                // (12,1): warning CS1570: XML comment has badly formed XML -- 'Expected an end tag for element 'nodename'.'
                //     protected void GetEntityConnectionString(
                Diagnostic(ErrorCode.WRN_XMLParseError, "").WithArguments("nodename"),
                // (12,1): warning CS1570: XML comment has badly formed XML -- 'Expected an end tag for element 'project'.'
                //     protected void GetEntityConnectionString(
                Diagnostic(ErrorCode.WRN_XMLParseError, "").WithArguments("project"),
                // (12,1): warning CS1570: XML comment has badly formed XML -- 'Expected an end tag for element 'summary'.'
                //     protected void GetEntityConnectionString(
                Diagnostic(ErrorCode.WRN_XMLParseError, "").WithArguments("summary"));
        }

        [WorkItem(547188, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/547188")]
        [Fact]
        public void WhitespaceInXmlName()
        {
            var text = @"
/// <A:B/>
/// <A: B/>
/// <A :B/>
/// <A : B/>
public class Program
{
}";

            var tree = Parse(text);
            tree.GetDiagnostics().Verify(
                // (3,8): warning CS1570: XML comment has badly formed XML -- 'Whitespace is not allowed at this location.'
                // /// <A: B/>
                Diagnostic(ErrorCode.WRN_XMLParseError, " "),
                // (4,7): warning CS1570: XML comment has badly formed XML -- 'Whitespace is not allowed at this location.'
                // /// <A :B/>
                Diagnostic(ErrorCode.WRN_XMLParseError, " "),
                // (5,7): warning CS1570: XML comment has badly formed XML -- 'Whitespace is not allowed at this location.'
                // /// <A : B/>
                Diagnostic(ErrorCode.WRN_XMLParseError, " "),
                // (5,9): warning CS1570: XML comment has badly formed XML -- 'Whitespace is not allowed at this location.'
                // /// <A : B/>
                Diagnostic(ErrorCode.WRN_XMLParseError, " "));
        }

        [Fact]
        public void WhitespaceInXmlEndName()
        {
            var text = @"
/// <A:B>
///   good
/// </A:B>
/// <A:B>
///   bad
/// </A :B>
/// <A:B>
///   bad
/// </A: B>
public class Program
{
}";

            var tree = Parse(text);
            tree.GetDiagnostics().Verify(
                // (7,7): warning CS1570: XML comment has badly formed XML -- 'End tag 'A :B' does not match the start tag 'A:B'.'
                // /// </A :B>
                Diagnostic(ErrorCode.WRN_XMLParseError, "A :B").WithArguments("A :B", "A:B").WithLocation(7, 7),
                // (7,8): warning CS1570: XML comment has badly formed XML -- 'Whitespace is not allowed at this location.'
                // /// </A :B>
                Diagnostic(ErrorCode.WRN_XMLParseError, " ").WithLocation(7, 8),
                // (10,7): warning CS1570: XML comment has badly formed XML -- 'End tag 'A: B' does not match the start tag 'A:B'.'
                // /// </A: B>
                Diagnostic(ErrorCode.WRN_XMLParseError, "A: B").WithArguments("A: B", "A:B").WithLocation(10, 7),
                // (10,9): warning CS1570: XML comment has badly formed XML -- 'Whitespace is not allowed at this location.'
                // /// </A: B>
                Diagnostic(ErrorCode.WRN_XMLParseError, " ").WithLocation(10, 9)
                );
        }

        [Fact]
        [Trait("Feature", "Xml Documentation Comments")]
        public void TestDocumentationComment()
        {
            var expected = @"/// <summary>
/// This class provides extension methods for the <see cref=""TypeName""/> class.
/// </summary>
/// <threadsafety static=""true"" instance=""false""/>
/// <preliminary/>";

            DocumentationCommentTriviaSyntax documentationComment = SyntaxFactory.DocumentationComment(
                SyntaxFactory.XmlSummaryElement(
                    SyntaxFactory.XmlNewLine(Environment.NewLine),
                    SyntaxFactory.XmlText("This class provides extension methods for the "),
                    SyntaxFactory.XmlSeeElement(
                        SyntaxFactory.TypeCref(SyntaxFactory.ParseTypeName("TypeName"))),
                    SyntaxFactory.XmlText(" class."),
                    SyntaxFactory.XmlNewLine(Environment.NewLine)),
                SyntaxFactory.XmlNewLine(Environment.NewLine),
                SyntaxFactory.XmlThreadSafetyElement(),
                SyntaxFactory.XmlNewLine(Environment.NewLine),
                SyntaxFactory.XmlPreliminaryElement());

            var actual = documentationComment.ToFullString();

            actual.Should().Be(expected);
        }

        [Fact]
        [Trait("Feature", "Xml Documentation Comments")]
        public void TestXmlSummaryElement()
        {
            var expected =
@"/// <summary>
/// This class provides extension methods.
/// </summary>";

            DocumentationCommentTriviaSyntax documentationComment = SyntaxFactory.DocumentationComment(
                SyntaxFactory.XmlSummaryElement(
                    SyntaxFactory.XmlNewLine(Environment.NewLine),
                    SyntaxFactory.XmlText("This class provides extension methods."),
                    SyntaxFactory.XmlNewLine(Environment.NewLine)));

            var actual = documentationComment.ToFullString();

            actual.Should().Be(expected);
        }

        [Fact]
        [Trait("Feature", "Xml Documentation Comments")]
        public void TestXmlSeeElementAndXmlSeeAlsoElement()
        {
            var expected =
@"/// <summary>
/// This class provides extension methods for the <see cref=""TypeName""/> class and the <seealso cref=""TypeName2""/> class.
/// </summary>";

            DocumentationCommentTriviaSyntax documentationComment = SyntaxFactory.DocumentationComment(
                SyntaxFactory.XmlSummaryElement(
                    SyntaxFactory.XmlNewLine(Environment.NewLine),
                    SyntaxFactory.XmlText("This class provides extension methods for the "),
                    SyntaxFactory.XmlSeeElement(
                        SyntaxFactory.TypeCref(SyntaxFactory.ParseTypeName("TypeName"))),
                    SyntaxFactory.XmlText(" class and the "),
                    SyntaxFactory.XmlSeeAlsoElement(
                        SyntaxFactory.TypeCref(SyntaxFactory.ParseTypeName("TypeName2"))),
                    SyntaxFactory.XmlText(" class."),
                    SyntaxFactory.XmlNewLine(Environment.NewLine)));

            var actual = documentationComment.ToFullString();

            actual.Should().Be(expected);
        }

        [Fact]
        [Trait("Feature", "Xml Documentation Comments")]
        public void TestXmlNewLineElement()
        {
            var expected =
@"/// <summary>
/// This is a summary.
/// </summary>
/// 
/// 
/// <remarks>
/// 
/// </remarks>";

            DocumentationCommentTriviaSyntax documentationComment = SyntaxFactory.DocumentationComment(
                SyntaxFactory.XmlSummaryElement(
                    SyntaxFactory.XmlNewLine(Environment.NewLine),
                    SyntaxFactory.XmlText("This is a summary."),
                    SyntaxFactory.XmlNewLine(Environment.NewLine)),
                SyntaxFactory.XmlNewLine(Environment.NewLine),
                SyntaxFactory.XmlNewLine(Environment.NewLine),
                SyntaxFactory.XmlNewLine(Environment.NewLine),
                SyntaxFactory.XmlRemarksElement(
                    SyntaxFactory.XmlNewLine(Environment.NewLine),
                    SyntaxFactory.XmlNewLine(Environment.NewLine)));

            var actual = documentationComment.ToFullString();

            actual.Should().Be(expected);
        }

        [Fact]
        [Trait("Feature", "Xml Documentation Comments")]
        public void TestXmlParamAndParamRefElement()
        {
            var expected =
@"/// <summary>
/// <paramref name=""b""/>
/// </summary>
/// <param name=""a""></param>
/// <param name=""b""></param>";

            DocumentationCommentTriviaSyntax documentationComment = SyntaxFactory.DocumentationComment(
                SyntaxFactory.XmlSummaryElement(
                    SyntaxFactory.XmlNewLine(Environment.NewLine),
                    SyntaxFactory.XmlParamRefElement("b"),
                    SyntaxFactory.XmlNewLine(Environment.NewLine)),
                SyntaxFactory.XmlNewLine(Environment.NewLine),
                SyntaxFactory.XmlParamElement("a"),
                SyntaxFactory.XmlNewLine(Environment.NewLine),
                SyntaxFactory.XmlParamElement("b"));

            var actual = documentationComment.ToFullString();

            actual.Should().Be(expected);
        }

        [Fact]
        [Trait("Feature", "Xml Documentation Comments")]
        public void TestXmlReturnsElement()
        {
            var expected =
@"/// <summary>
/// 
/// </summary>
/// <returns>
/// Returns a value.
/// </returns>";

            DocumentationCommentTriviaSyntax documentationComment = SyntaxFactory.DocumentationComment(
                SyntaxFactory.XmlSummaryElement(
                    SyntaxFactory.XmlNewLine(Environment.NewLine),
                    SyntaxFactory.XmlNewLine(Environment.NewLine)),
                SyntaxFactory.XmlNewLine(Environment.NewLine),
                SyntaxFactory.XmlReturnsElement(
                    SyntaxFactory.XmlNewLine(Environment.NewLine),
                    SyntaxFactory.XmlText("Returns a value."),
                    SyntaxFactory.XmlNewLine(Environment.NewLine)));

            var actual = documentationComment.ToFullString();

            actual.Should().Be(expected);
        }

        [Fact]
        [Trait("Feature", "Xml Documentation Comments")]
        public void TestXmlRemarksElement()
        {
            var expected =
@"/// <summary>
/// 
/// </summary>
/// <remarks>
/// Same as in class <see cref=""TypeName""/>.
/// </remarks>";

            DocumentationCommentTriviaSyntax documentationComment = SyntaxFactory.DocumentationComment(
                SyntaxFactory.XmlSummaryElement(
                    SyntaxFactory.XmlNewLine(Environment.NewLine),
                    SyntaxFactory.XmlNewLine(Environment.NewLine)),
                SyntaxFactory.XmlNewLine(Environment.NewLine),
                SyntaxFactory.XmlRemarksElement(
                    SyntaxFactory.XmlNewLine(Environment.NewLine),
                    SyntaxFactory.XmlText("Same as in class "),
                    SyntaxFactory.XmlSeeElement(SyntaxFactory.TypeCref(SyntaxFactory.ParseTypeName("TypeName"))),
                    SyntaxFactory.XmlText("."),
                    SyntaxFactory.XmlNewLine(Environment.NewLine)));

            var actual = documentationComment.ToFullString();

            actual.Should().Be(expected);
        }

        [Fact]
        [Trait("Feature", "Xml Documentation Comments")]
        public void TestXmlExceptionElement()
        {
            var expected =
@"/// <summary>
/// 
/// </summary>
/// <exception cref=""InvalidOperationException"">This exception will be thrown if the object is in an invalid state when calling this method.</exception>";

            DocumentationCommentTriviaSyntax documentationComment = SyntaxFactory.DocumentationComment(
                SyntaxFactory.XmlSummaryElement(
                    SyntaxFactory.XmlNewLine(Environment.NewLine),
                    SyntaxFactory.XmlNewLine(Environment.NewLine)),
                SyntaxFactory.XmlNewLine(Environment.NewLine),
                SyntaxFactory.XmlExceptionElement(
                    SyntaxFactory.TypeCref(
                        SyntaxFactory.ParseTypeName("InvalidOperationException")),
                        SyntaxFactory.XmlText("This exception will be thrown if the object is in an invalid state when calling this method.")));

            var actual = documentationComment.ToFullString();

            actual.Should().Be(expected);
        }

        [Fact]
        [Trait("Feature", "Xml Documentation Comments")]
        public void TestXmlPermissionElement()
        {
            var expected =
@"/// <summary>
/// 
/// </summary>
/// <permission cref=""MyPermission"">Needs MyPermission to execute.</permission>";

            DocumentationCommentTriviaSyntax documentationComment = SyntaxFactory.DocumentationComment(
                SyntaxFactory.XmlSummaryElement(
                    SyntaxFactory.XmlNewLine(Environment.NewLine),
                    SyntaxFactory.XmlNewLine(Environment.NewLine)),
                SyntaxFactory.XmlNewLine(Environment.NewLine),
                SyntaxFactory.XmlPermissionElement(
                    SyntaxFactory.TypeCref(
                        SyntaxFactory.ParseTypeName("MyPermission")),
                    SyntaxFactory.XmlText("Needs MyPermission to execute.")));

            var actual = documentationComment.ToFullString();

            actual.Should().Be(expected);
        }

        [Fact]
        [WorkItem(39315, "https://github.com/dotnet/roslyn/issues/39315")]
        public void WriteDocumentationCommentXml_01()
        {
            var comp = CreateCompilation(new[] {
                Parse(@"
/// <summary> a
/// </summary>
"),
                Parse(@"

/// <summary> b
/// </summary>
")});

            var diags = DiagnosticBag.GetInstance();

            DocumentationCommentCompiler.WriteDocumentationCommentXml(
                comp,
                assemblyName: null,
                xmlDocStream: null,
                new BindingDiagnosticBag(diags),
                default(CancellationToken),
                filterTree: comp.SyntaxTrees[0]);

            diags.ToReadOnlyAndFree().Verify(
                // (2,1): warning CS1587: XML comment is not placed on a valid language element
                // /// <summary> a
                Diagnostic(ErrorCode.WRN_UnprocessedXMLComment, "/").WithLocation(2, 1)
                );

            diags = DiagnosticBag.GetInstance();

            DocumentationCommentCompiler.WriteDocumentationCommentXml(
                comp,
                assemblyName: null,
                xmlDocStream: null,
                new BindingDiagnosticBag(diags),
                default(CancellationToken),
                filterTree: comp.SyntaxTrees[0],
                filterSpanWithinTree: new TextSpan(0, 0));

            diags.ToReadOnlyAndFree().Verify();

            diags = DiagnosticBag.GetInstance();

            DocumentationCommentCompiler.WriteDocumentationCommentXml(
                comp,
                assemblyName: null,
                xmlDocStream: null,
                new BindingDiagnosticBag(diags),
                default(CancellationToken),
                filterTree: comp.SyntaxTrees[1]);

            diags.ToReadOnlyAndFree().Verify(
                // (3,1): warning CS1587: XML comment is not placed on a valid language element
                // /// <summary> b
                Diagnostic(ErrorCode.WRN_UnprocessedXMLComment, "/").WithLocation(3, 1)
                );

            diags = DiagnosticBag.GetInstance();

            DocumentationCommentCompiler.WriteDocumentationCommentXml(
                comp,
                assemblyName: null,
                xmlDocStream: null,
                new BindingDiagnosticBag(diags),
                default(CancellationToken),
                filterTree: null);

            diags.ToReadOnlyAndFree().Verify(
                // (2,1): warning CS1587: XML comment is not placed on a valid language element
                // /// <summary> a
                Diagnostic(ErrorCode.WRN_UnprocessedXMLComment, "/").WithLocation(2, 1),
                // (3,1): warning CS1587: XML comment is not placed on a valid language element
                // /// <summary> b
                Diagnostic(ErrorCode.WRN_UnprocessedXMLComment, "/").WithLocation(3, 1)
                );

            diags = DiagnosticBag.GetInstance();

            DocumentationCommentCompiler.WriteDocumentationCommentXml(
                comp,
                assemblyName: null,
                xmlDocStream: null,
                new BindingDiagnosticBag(diags),
                default(CancellationToken),
                filterTree: null,
                filterSpanWithinTree: new TextSpan(0, 0));

            diags.ToReadOnlyAndFree().Verify(
                // (2,1): warning CS1587: XML comment is not placed on a valid language element
                // /// <summary> a
                Diagnostic(ErrorCode.WRN_UnprocessedXMLComment, "/").WithLocation(2, 1),
                // (3,1): warning CS1587: XML comment is not placed on a valid language element
                // /// <summary> b
                Diagnostic(ErrorCode.WRN_UnprocessedXMLComment, "/").WithLocation(3, 1)
                );
        }

        #region Xml Test helpers

        /// <summary>
        /// Verifies that the errors on the given CSharpSyntaxNode match the expected error codes and types
        /// </summary>
        /// <param name="node">The node that has errors</param>
        /// <param name="errors">The list of expected errors</param>
        private void VerifyDiagnostics(CSharpSyntaxNode node, List<TestError> errors)
        {
            VerifyDiagnostics(node.ErrorsAndWarnings(), errors);
        }

        private void VerifyDiagnostics(SyntaxTrivia trivia, List<TestError> errors)
        {
            VerifyDiagnostics(trivia.ErrorsAndWarnings(), errors);
        }

        private void VerifyDiagnostics(IEnumerable<DiagnosticInfo> actual, List<TestError> expected)
        {
            expected.Count.Should().Be(actual.Count());

            var actualErrors = (from e in actual
                                orderby e.Code
                                select new TestError(e.Code, e.Severity == DiagnosticSeverity.Warning)).ToList();
            var expectedErrors = (from e in expected
                                  orderby e.ErrorCode
                                  select new TestError(e.ErrorCode, e.IsWarning)).ToList();

            for (int i = 0; i < expected.Count; i++)
            {
                actualErrors[i].ErrorCode.Should().Be(expectedErrors[i].ErrorCode);
                actualErrors[i].IsWarning.Should().Be(expectedErrors[i].IsWarning);
            }
        }

        /// <summary>
        /// Verify if a given XmlElement is correct
        /// </summary>
        /// <param name="xmlElement">The XmlElement object to validate</param>
        /// <param name="tagName">The name of the tag the XML element should have</param>
        /// <param name="innerText">The text inside the XmlElement</param>
        /// 
        private void VerifyXmlElement(XmlElementSyntax xmlElement, string tagName, string innerText)
        {
            // if the innerText is empty, then the content has no nodes.
            if (innerText == string.Empty)
            {
                xmlElement.Content.Count.Should().Be(0);
            }
            else
            {
                var elementInnerText = GetXmlElementText(xmlElement);
                elementInnerText.Should().Be(innerText);
            }

            xmlElement.StartTag.Name.LocalName.Value.Should().Be(tagName);
            xmlElement.EndTag.Name.LocalName.Value.Should().Be(tagName);
        }

        /// <summary>
        /// Gets the string representation for a XmlElementText
        /// </summary>
        /// <param name="xmlElement"></param>
        /// <returns></returns>
        private string GetXmlElementText(XmlElementSyntax xmlElement)
        {
            StringBuilder sb = new StringBuilder();

            foreach (var element in xmlElement.Content)
            {
                if (element.GetType() == typeof(XmlElementSyntax))
                {
                    sb.Append(element.ToFullString());
                }
                else if (element.GetType() == typeof(XmlTextSyntax))
                {
                    sb.Append((element as XmlTextSyntax).TextTokens.ToFullString());
                }
                else if (element.GetType() == typeof(XmlCDataSectionSyntax))
                {
                    sb.Append(element.ToFullString());
                }
            }

            return sb.ToString();

            // return getTextFromTextTokens((xmlElement.Content[0] as XmlTextSyntax).TextTokens);
        }

        /// <summary>
        /// Verifies an empty XmlElement
        /// </summary>
        /// <param name="xmlElement">The XmlElement object to validate</param>
        /// <param name="tagName">The name of the tag the XML element should have</param>
        private void VerifyXmlElement(XmlEmptyElementSyntax xmlElement, string tagName)
        {
            xmlElement.Name.LocalName.Value.Should().Be(tagName);
        }

        /// <summary>
        /// Verify if the attributes for a given XML node match the expected ones
        /// </summary>
        /// <param name="xmlAttributes">The list of attributes to verify</param>
        /// <param name="attributes">The dictionary contains the key-value pair for the expected attribute values</param>
        private void VerifyXmlAttributes(SyntaxList<XmlAttributeSyntax> xmlAttributes, Dictionary<string, string> attributes)
        {
            // we have the same number of attributes
            xmlAttributes.Count.Should().Be(attributes.Keys.Count);
            foreach (XmlTextAttributeSyntax attribute in xmlAttributes)
            {
                // we make sure we have that attribute
                attributes.ContainsKey(attribute.Name.LocalName.Value as string).Should().BeTrue();

                // we make sure that the value for the attribute is the right one.
                attribute.TextTokens.ToString().Should().Be(attributes[attribute.Name.LocalName.Value as string]);
            }
        }

        /// <summary>
        /// This class is used to represent the expected errors
        /// </summary>
        private class TestError
        {
            public bool IsWarning { get; }
            public int ErrorCode { get; }

            public TestError(int code, bool warning)
            {
                this.IsWarning = warning;
                this.ErrorCode = code;
            }
        }
        #endregion
    }
}
