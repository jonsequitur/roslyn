// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp.Test.Utilities;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Test.Utilities;
using Xunit;
using InternalSyntax = Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.CSharp.UnitTests
{
    public class SyntaxNodeTests
    {
        [Fact]
        [WorkItem(565382, "https://developercommunity.visualstudio.com/content/problem/565382/compiling-causes-a-stack-overflow-error.html")]
        public void TestLargeFluentCallWithDirective()
        {
            var builder = new StringBuilder();
            builder.AppendLine(
    @"
class C {
    C M(string x) { return this; }
    void M2() {
        new C()
#region Region
");
            for (int i = 0; i < 20000; i++)
            {
                builder.AppendLine(@"            .M(""test"")");
            }
            builder.AppendLine(
               @"            .M(""test"");
#endregion
    }
}");

            var tree = SyntaxFactory.ParseSyntaxTree(builder.ToString());
            var directives = tree.GetRoot().GetDirectives();
            directives.Count.Should().Be(2);
        }

        [Fact]
        public void TestQualifiedNameSyntaxWith()
        {
            // this is just a test to prove that at least one generate With method exists and functions correctly. :-)
            var qname = (QualifiedNameSyntax)SyntaxFactory.ParseName("A.B");
            var qname2 = qname.WithRight(SyntaxFactory.IdentifierName("C"));
            var text = qname2.ToString();
            text.Should().Be("A.C");
        }

        [WorkItem(9229, "DevDiv_Projects/Roslyn")]
        [Fact]
        public void TestAddBaseListTypes()
        {
            var cls = SyntaxFactory.ParseCompilationUnit("class C { }").Members[0] as ClassDeclarationSyntax;
            var cls2 = cls.AddBaseListTypes(SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName("B")));
        }

        [Fact]
        public void TestChildNodes()
        {
            var text = "m(a,b,c)";
            var expression = SyntaxFactory.ParseExpression(text);

            var nodes = expression.ChildNodes().ToList();
            nodes.Count.Should().Be(2);
            nodes[0].Kind().Should().Be(SyntaxKind.IdentifierName);
            nodes[1].Kind().Should().Be(SyntaxKind.ArgumentList);
        }

        [Fact]
        public void TestAncestors()
        {
            var text = "a + (b - (c * (d / e)))";
            var expression = SyntaxFactory.ParseExpression(text);
            var e = expression.DescendantNodes().OfType<IdentifierNameSyntax>().First(n => n.Identifier.Text == "e");

            var nodes = e.Ancestors().ToList();
            nodes.Count.Should().Be(7);
            nodes[0].Kind().Should().Be(SyntaxKind.DivideExpression);
            nodes[1].Kind().Should().Be(SyntaxKind.ParenthesizedExpression);
            nodes[2].Kind().Should().Be(SyntaxKind.MultiplyExpression);
            nodes[3].Kind().Should().Be(SyntaxKind.ParenthesizedExpression);
            nodes[4].Kind().Should().Be(SyntaxKind.SubtractExpression);
            nodes[5].Kind().Should().Be(SyntaxKind.ParenthesizedExpression);
            nodes[6].Kind().Should().Be(SyntaxKind.AddExpression);
        }

        [Fact]
        public void TestAncestorsAndSelf()
        {
            var text = "a + (b - (c * (d / e)))";
            var expression = SyntaxFactory.ParseExpression(text);
            var e = expression.DescendantNodes().OfType<IdentifierNameSyntax>().First(n => n.Identifier.Text == "e");

            var nodes = e.AncestorsAndSelf().ToList();
            nodes.Count.Should().Be(8);
            nodes[0].Kind().Should().Be(SyntaxKind.IdentifierName);
            nodes[1].Kind().Should().Be(SyntaxKind.DivideExpression);
            nodes[2].Kind().Should().Be(SyntaxKind.ParenthesizedExpression);
            nodes[3].Kind().Should().Be(SyntaxKind.MultiplyExpression);
            nodes[4].Kind().Should().Be(SyntaxKind.ParenthesizedExpression);
            nodes[5].Kind().Should().Be(SyntaxKind.SubtractExpression);
            nodes[6].Kind().Should().Be(SyntaxKind.ParenthesizedExpression);
            nodes[7].Kind().Should().Be(SyntaxKind.AddExpression);
        }

        [Fact]
        public void TestFirstAncestorOrSelf()
        {
            var text = "a + (b - (c * (d / e)))";
            var expression = SyntaxFactory.ParseExpression(text);
            var e = expression.DescendantNodes().OfType<IdentifierNameSyntax>().First(n => n.Identifier.Text == "e");

            var firstParens = e.FirstAncestorOrSelf<ExpressionSyntax>(n => n.Kind() == SyntaxKind.ParenthesizedExpression);
            firstParens.Should().NotBeNull();
            firstParens.ToString().Should().Be("(d / e)");
        }

        [Fact]
        public void TestDescendantNodes()
        {
            var text = "#if true\r\n  return true;";
            var statement = SyntaxFactory.ParseStatement(text);

            var nodes = statement.DescendantNodes().ToList();
            nodes.Count.Should().Be(1);
            nodes[0].Kind().Should().Be(SyntaxKind.TrueLiteralExpression);

            nodes = statement.DescendantNodes(descendIntoTrivia: true).ToList();
            nodes.Count.Should().Be(3);
            nodes[0].Kind().Should().Be(SyntaxKind.IfDirectiveTrivia);
            nodes[1].Kind().Should().Be(SyntaxKind.TrueLiteralExpression);
            nodes[2].Kind().Should().Be(SyntaxKind.TrueLiteralExpression);

            nodes = statement.DescendantNodes(n => n is StatementSyntax).ToList();
            nodes.Count.Should().Be(1);
            nodes[0].Kind().Should().Be(SyntaxKind.TrueLiteralExpression);

            nodes = statement.DescendantNodes(n => n is StatementSyntax, descendIntoTrivia: true).ToList();
            nodes.Count.Should().Be(2);
            nodes[0].Kind().Should().Be(SyntaxKind.IfDirectiveTrivia);
            nodes[1].Kind().Should().Be(SyntaxKind.TrueLiteralExpression);

            // all over again with spans
            nodes = statement.DescendantNodes(statement.FullSpan).ToList();
            nodes.Count.Should().Be(1);
            nodes[0].Kind().Should().Be(SyntaxKind.TrueLiteralExpression);

            nodes = statement.DescendantNodes(statement.FullSpan, descendIntoTrivia: true).ToList();
            nodes.Count.Should().Be(3);
            nodes[0].Kind().Should().Be(SyntaxKind.IfDirectiveTrivia);
            nodes[1].Kind().Should().Be(SyntaxKind.TrueLiteralExpression);
            nodes[2].Kind().Should().Be(SyntaxKind.TrueLiteralExpression);

            nodes = statement.DescendantNodes(statement.FullSpan, n => n is StatementSyntax).ToList();
            nodes.Count.Should().Be(1);
            nodes[0].Kind().Should().Be(SyntaxKind.TrueLiteralExpression);

            nodes = statement.DescendantNodes(statement.FullSpan, n => n is StatementSyntax, descendIntoTrivia: true).ToList();
            nodes.Count.Should().Be(2);
            nodes[0].Kind().Should().Be(SyntaxKind.IfDirectiveTrivia);
            nodes[1].Kind().Should().Be(SyntaxKind.TrueLiteralExpression);
        }

        [Fact]
        public void TestDescendantNodesAndSelf()
        {
            var text = "#if true\r\n  return true;";
            var statement = SyntaxFactory.ParseStatement(text);

            var nodes = statement.DescendantNodesAndSelf().ToList();
            nodes.Count.Should().Be(2);
            nodes[0].Kind().Should().Be(SyntaxKind.ReturnStatement);
            nodes[1].Kind().Should().Be(SyntaxKind.TrueLiteralExpression);

            nodes = statement.DescendantNodesAndSelf(descendIntoTrivia: true).ToList();
            nodes.Count.Should().Be(4);
            nodes[0].Kind().Should().Be(SyntaxKind.ReturnStatement);
            nodes[1].Kind().Should().Be(SyntaxKind.IfDirectiveTrivia);
            nodes[2].Kind().Should().Be(SyntaxKind.TrueLiteralExpression);
            nodes[3].Kind().Should().Be(SyntaxKind.TrueLiteralExpression);

            nodes = statement.DescendantNodesAndSelf(n => n is StatementSyntax).ToList();
            nodes.Count.Should().Be(2);
            nodes[0].Kind().Should().Be(SyntaxKind.ReturnStatement);
            nodes[1].Kind().Should().Be(SyntaxKind.TrueLiteralExpression);

            nodes = statement.DescendantNodesAndSelf(n => n is StatementSyntax, descendIntoTrivia: true).ToList();
            nodes.Count.Should().Be(3);
            nodes[0].Kind().Should().Be(SyntaxKind.ReturnStatement);
            nodes[1].Kind().Should().Be(SyntaxKind.IfDirectiveTrivia);
            nodes[2].Kind().Should().Be(SyntaxKind.TrueLiteralExpression);

            // all over again with spans
            nodes = statement.DescendantNodesAndSelf(statement.FullSpan).ToList();
            nodes.Count.Should().Be(2);
            nodes[0].Kind().Should().Be(SyntaxKind.ReturnStatement);
            nodes[1].Kind().Should().Be(SyntaxKind.TrueLiteralExpression);

            nodes = statement.DescendantNodesAndSelf(statement.FullSpan, descendIntoTrivia: true).ToList();
            nodes.Count.Should().Be(4);
            nodes[0].Kind().Should().Be(SyntaxKind.ReturnStatement);
            nodes[1].Kind().Should().Be(SyntaxKind.IfDirectiveTrivia);
            nodes[2].Kind().Should().Be(SyntaxKind.TrueLiteralExpression);
            nodes[3].Kind().Should().Be(SyntaxKind.TrueLiteralExpression);

            nodes = statement.DescendantNodesAndSelf(statement.FullSpan, n => n is StatementSyntax).ToList();
            nodes.Count.Should().Be(2);
            nodes[0].Kind().Should().Be(SyntaxKind.ReturnStatement);
            nodes[1].Kind().Should().Be(SyntaxKind.TrueLiteralExpression);

            nodes = statement.DescendantNodesAndSelf(statement.FullSpan, n => n is StatementSyntax, descendIntoTrivia: true).ToList();
            nodes.Count.Should().Be(3);
            nodes[0].Kind().Should().Be(SyntaxKind.ReturnStatement);
            nodes[1].Kind().Should().Be(SyntaxKind.IfDirectiveTrivia);
            nodes[2].Kind().Should().Be(SyntaxKind.TrueLiteralExpression);
        }

        [Fact]
        public void TestDescendantNodesAndTokens()
        {
            var text = "#if true\r\n  return true;";
            var statement = SyntaxFactory.ParseStatement(text);

            var nodesAndTokens = statement.DescendantNodesAndTokens().ToList();
            nodesAndTokens.Count.Should().Be(4);
            nodesAndTokens[0].Kind().Should().Be(SyntaxKind.ReturnKeyword);
            nodesAndTokens[1].Kind().Should().Be(SyntaxKind.TrueLiteralExpression);
            nodesAndTokens[2].Kind().Should().Be(SyntaxKind.TrueKeyword);
            nodesAndTokens[3].Kind().Should().Be(SyntaxKind.SemicolonToken);

            nodesAndTokens = statement.DescendantNodesAndTokens(descendIntoTrivia: true).ToList();
            nodesAndTokens.Count.Should().Be(10);
            nodesAndTokens[0].Kind().Should().Be(SyntaxKind.IfDirectiveTrivia);
            nodesAndTokens[1].Kind().Should().Be(SyntaxKind.HashToken);
            nodesAndTokens[2].Kind().Should().Be(SyntaxKind.IfKeyword);
            nodesAndTokens[3].Kind().Should().Be(SyntaxKind.TrueLiteralExpression);
            nodesAndTokens[4].Kind().Should().Be(SyntaxKind.TrueKeyword);
            nodesAndTokens[5].Kind().Should().Be(SyntaxKind.EndOfDirectiveToken);
            nodesAndTokens[6].Kind().Should().Be(SyntaxKind.ReturnKeyword);
            nodesAndTokens[7].Kind().Should().Be(SyntaxKind.TrueLiteralExpression);
            nodesAndTokens[8].Kind().Should().Be(SyntaxKind.TrueKeyword);
            nodesAndTokens[9].Kind().Should().Be(SyntaxKind.SemicolonToken);

            // with span
            nodesAndTokens = statement.DescendantNodesAndTokens(statement.FullSpan).ToList();
            nodesAndTokens.Count.Should().Be(4);
            nodesAndTokens[0].Kind().Should().Be(SyntaxKind.ReturnKeyword);
            nodesAndTokens[1].Kind().Should().Be(SyntaxKind.TrueLiteralExpression);
            nodesAndTokens[2].Kind().Should().Be(SyntaxKind.TrueKeyword);
            nodesAndTokens[3].Kind().Should().Be(SyntaxKind.SemicolonToken);
        }

        [Fact]
        public void TestDescendantNodesAndTokensAndSelf()
        {
            var text = "#if true\r\n  return true;";
            var statement = SyntaxFactory.ParseStatement(text);

            var nodesAndTokens = statement.DescendantNodesAndTokensAndSelf().ToList();
            nodesAndTokens.Count.Should().Be(5);
            nodesAndTokens[0].Kind().Should().Be(SyntaxKind.ReturnStatement);
            nodesAndTokens[1].Kind().Should().Be(SyntaxKind.ReturnKeyword);
            nodesAndTokens[2].Kind().Should().Be(SyntaxKind.TrueLiteralExpression);
            nodesAndTokens[3].Kind().Should().Be(SyntaxKind.TrueKeyword);
            nodesAndTokens[4].Kind().Should().Be(SyntaxKind.SemicolonToken);

            nodesAndTokens = statement.DescendantNodesAndTokensAndSelf(descendIntoTrivia: true).ToList();
            nodesAndTokens.Count.Should().Be(11);
            nodesAndTokens[0].Kind().Should().Be(SyntaxKind.ReturnStatement);
            nodesAndTokens[1].Kind().Should().Be(SyntaxKind.IfDirectiveTrivia);
            nodesAndTokens[2].Kind().Should().Be(SyntaxKind.HashToken);
            nodesAndTokens[3].Kind().Should().Be(SyntaxKind.IfKeyword);
            nodesAndTokens[4].Kind().Should().Be(SyntaxKind.TrueLiteralExpression);
            nodesAndTokens[5].Kind().Should().Be(SyntaxKind.TrueKeyword);
            nodesAndTokens[6].Kind().Should().Be(SyntaxKind.EndOfDirectiveToken);
            nodesAndTokens[7].Kind().Should().Be(SyntaxKind.ReturnKeyword);
            nodesAndTokens[8].Kind().Should().Be(SyntaxKind.TrueLiteralExpression);
            nodesAndTokens[9].Kind().Should().Be(SyntaxKind.TrueKeyword);
            nodesAndTokens[10].Kind().Should().Be(SyntaxKind.SemicolonToken);

            // with span
            nodesAndTokens = statement.DescendantNodesAndTokensAndSelf(statement.FullSpan).ToList();
            nodesAndTokens.Count.Should().Be(5);
            nodesAndTokens[0].Kind().Should().Be(SyntaxKind.ReturnStatement);
            nodesAndTokens[1].Kind().Should().Be(SyntaxKind.ReturnKeyword);
            nodesAndTokens[2].Kind().Should().Be(SyntaxKind.TrueLiteralExpression);
            nodesAndTokens[3].Kind().Should().Be(SyntaxKind.TrueKeyword);
            nodesAndTokens[4].Kind().Should().Be(SyntaxKind.SemicolonToken);
        }

        [Fact]
        public void TestDescendantNodesAndTokensAndSelfForEmptyCompilationUnit()
        {
            var text = "";
            var cu = SyntaxFactory.ParseCompilationUnit(text);
            var nodesAndTokens = cu.DescendantNodesAndTokensAndSelf().ToList();
            nodesAndTokens.Count.Should().Be(2);
            nodesAndTokens[0].Kind().Should().Be(SyntaxKind.CompilationUnit);
            nodesAndTokens[1].Kind().Should().Be(SyntaxKind.EndOfFileToken);
        }

        [Fact]
        public void TestDescendantNodesAndTokensAndSelfForDocumentationComment()
        {
            var text = "/// Goo\r\n x";
            var expr = SyntaxFactory.ParseExpression(text);

            var nodesAndTokens = expr.DescendantNodesAndTokensAndSelf(descendIntoTrivia: true).ToList();
            nodesAndTokens.Count.Should().Be(7);
            nodesAndTokens[0].Kind().Should().Be(SyntaxKind.IdentifierName);
            nodesAndTokens[1].Kind().Should().Be(SyntaxKind.SingleLineDocumentationCommentTrivia);
            nodesAndTokens[2].Kind().Should().Be(SyntaxKind.XmlText);
            nodesAndTokens[3].Kind().Should().Be(SyntaxKind.XmlTextLiteralToken);
            nodesAndTokens[4].Kind().Should().Be(SyntaxKind.XmlTextLiteralNewLineToken);
            nodesAndTokens[5].Kind().Should().Be(SyntaxKind.EndOfDocumentationCommentToken);
            nodesAndTokens[6].Kind().Should().Be(SyntaxKind.IdentifierToken);
        }

        [Fact]
        public void TestGetAllDirectivesUsingDescendantNodes()
        {
            var text = "#if false\r\n  eat a sandwich\r\n#endif\r\n x";
            var expr = SyntaxFactory.ParseExpression(text);

            var directives = expr.GetDirectives();
            var descendantDirectives = expr.DescendantNodesAndSelf(n => n.ContainsDirectives, descendIntoTrivia: true).OfType<DirectiveTriviaSyntax>().ToList();

            descendantDirectives.Count.Should().Be(directives.Count);
            for (int i = 0; i < directives.Count; i++)
            {
                descendantDirectives[i].Should().Be(directives[i]);
            }
        }

        [Fact]
        public void TestContainsDirective()
        {
            // Empty compilation unit shouldn't have any directives in it.
            for (var kind = SyntaxKind.TildeToken; kind < SyntaxKind.ScopedKeyword; kind++)
                SyntaxFactory.ParseCompilationUnit("").ContainsDirective(kind).Should().BeFalse();

            // basic file shouldn't have any directives in it.
            for (var kind = SyntaxKind.TildeToken; kind < SyntaxKind.ScopedKeyword; kind++)
                SyntaxFactory.ParseCompilationUnit("namespace N { }").ContainsDirective(kind).Should().BeFalse();

            // directive in trailing trivia is not a thing
            for (var kind = SyntaxKind.TildeToken; kind < SyntaxKind.ScopedKeyword; kind++)
            {
                var compilationUnit = SyntaxFactory.ParseCompilationUnit("namespace N { } #if false");
                compilationUnit.GetDiagnostics().Verify(
                    // (1,17): error CS1040: Preprocessor directives must appear as the first non-whitespace character on a line
                    // namespace N { } #if false
                    TestBase.Diagnostic(ErrorCode.ERR_BadDirectivePlacement, "#").WithLocation(1, 17),
                    // (1,26): error CS1027: #endif directive expected
                    // namespace N { } #if false
                    TestBase.Diagnostic(ErrorCode.ERR_EndifDirectiveExpected, "").WithLocation(1, 26));
                compilationUnit.ContainsDirective(kind).Should().BeFalse();
            }

            testContainsHelper1("#define x", SyntaxKind.DefineDirectiveTrivia);
            testContainsHelper1("#if true\r\n#elif true", SyntaxKind.IfDirectiveTrivia, SyntaxKind.ElifDirectiveTrivia);
            testContainsHelper1("#if false\r\n#elif true", SyntaxKind.IfDirectiveTrivia, SyntaxKind.ElifDirectiveTrivia);
            testContainsHelper1("#if false\r\n#elif false", SyntaxKind.IfDirectiveTrivia, SyntaxKind.ElifDirectiveTrivia);
            testContainsHelper1("#elif true", SyntaxKind.BadDirectiveTrivia);
            testContainsHelper1("#if true\r\n#else", SyntaxKind.IfDirectiveTrivia, SyntaxKind.ElseDirectiveTrivia);
            testContainsHelper1("#else", SyntaxKind.BadDirectiveTrivia);
            testContainsHelper1("#if true\r\n#endif", SyntaxKind.IfDirectiveTrivia, SyntaxKind.EndIfDirectiveTrivia);
            testContainsHelper1("#endif", SyntaxKind.BadDirectiveTrivia);
            testContainsHelper1("#region\r\n#endregion", SyntaxKind.RegionDirectiveTrivia, SyntaxKind.EndRegionDirectiveTrivia);
            testContainsHelper1("#endregion", SyntaxKind.BadDirectiveTrivia);
            testContainsHelper1("#error", SyntaxKind.ErrorDirectiveTrivia);
            testContainsHelper1("#if true", SyntaxKind.IfDirectiveTrivia);
            testContainsHelper1("#nullable enable", SyntaxKind.NullableDirectiveTrivia);
            testContainsHelper1("#region enable", SyntaxKind.RegionDirectiveTrivia);
            testContainsHelper1("#undef x", SyntaxKind.UndefDirectiveTrivia);
            testContainsHelper1("#warning", SyntaxKind.WarningDirectiveTrivia);

            // !# is special and is only recognized at start of a script file and nowhere else.
            testContainsHelper2(new[] { SyntaxKind.ShebangDirectiveTrivia }, SyntaxFactory.ParseCompilationUnit("#!command", options: TestOptions.Script));
            testContainsHelper2(new[] { SyntaxKind.BadDirectiveTrivia }, SyntaxFactory.ParseCompilationUnit(" #!command", options: TestOptions.Script));
            testContainsHelper2(new[] { SyntaxKind.BadDirectiveTrivia }, SyntaxFactory.ParseCompilationUnit("#!command", options: TestOptions.Regular));

            return;

            void testContainsHelper1(string directive, params SyntaxKind[] directiveKinds)
            {
                directiveKinds.Length > 0.Should().BeTrue();

                // directive on its own.
                testContainsHelper2(directiveKinds, SyntaxFactory.ParseCompilationUnit(directive));

                // Two of the same directive back to back.
                testContainsHelper2(directiveKinds, SyntaxFactory.ParseCompilationUnit($$"""
                    {{directive}}
                    {{directive}}
                    """));

                // Two of the same directive back to back with additional trivia
                testContainsHelper2(directiveKinds, SyntaxFactory.ParseCompilationUnit($$"""
                       {{directive}}
                       {{directive}}
                    """));

                // Directive inside a namespace
                testContainsHelper2(directiveKinds, SyntaxFactory.ParseCompilationUnit($$"""
                    namespace N
                    {
                    {{directive}}
                    }
                    """));

                // Multiple Directive inside a namespace
                testContainsHelper2(directiveKinds, SyntaxFactory.ParseCompilationUnit($$"""
                    namespace N
                    {
                    {{directive}}
                    {{directive}}
                    }
                    """));

                // Multiple Directive inside a namespace with additional trivia
                testContainsHelper2(directiveKinds, SyntaxFactory.ParseCompilationUnit($$"""
                    namespace N
                    {
                       {{directive}}
                       {{directive}}
                    }
                    """));

                // Directives on different elements in a namespace
                testContainsHelper2(directiveKinds, SyntaxFactory.ParseCompilationUnit($$"""
                    namespace N
                    {
                    {{directive}}
                        class C
                        {
                        }
                    {{directive}}
                        class D
                        {
                        }
                    }
                    """));

                // Directives on different elements in a namespace with additional trivia
                testContainsHelper2(directiveKinds, SyntaxFactory.ParseCompilationUnit($$"""
                    namespace N
                    {
                        {{directive}}
                        class C
                        {
                        }
                        {{directive}}
                        class D
                        {
                        }
                    }
                    """));
            }

            void testContainsHelper2(SyntaxKind[] directiveKinds, CompilationUnitSyntax compilationUnit)
            {
                compilationUnit.ContainsDirectives.Should().BeTrue();
                foreach (var directiveKind in directiveKinds)
                    compilationUnit.ContainsDirective(directiveKind).Should().BeTrue();

                for (var kind = SyntaxKind.TildeToken; kind < SyntaxKind.ScopedType; kind++)
                {
                    if (!directiveKinds.Contains(kind))
                        compilationUnit.ContainsDirective(kind).Should().BeFalse();
                }
            }
        }

        [Fact]
        public void TestGetAllAnnotatedNodesUsingDescendantNodes()
        {
            var text = "a + (b - (c * (d / e)))";
            var expr = SyntaxFactory.ParseExpression(text);
            var myAnnotation = new SyntaxAnnotation();

            var identifierNodes = expr.DescendantNodes().OfType<IdentifierNameSyntax>().ToList();
            var exprWithAnnotations = expr.ReplaceNodes(identifierNodes, (e, e2) => e2.WithAdditionalAnnotations(myAnnotation));

            var nodesWithMyAnnotations = exprWithAnnotations.DescendantNodesAndSelf(n => n.ContainsAnnotations).Where(n => n.HasAnnotation(myAnnotation)).ToList();

            nodesWithMyAnnotations.Count.Should().Be(identifierNodes.Count);

            for (int i = 0; i < identifierNodes.Count; i++)
            {
                // compare text because node identity changed when adding the annotation
                nodesWithMyAnnotations[i].ToString().Should().Be(identifierNodes[i].ToString());
            }
        }

        [Fact]
        public void TestDescendantTokens()
        {
            var s1 = "using Goo;";
            var t1 = SyntaxFactory.ParseSyntaxTree(s1);
            var tokens = t1.GetCompilationUnitRoot().DescendantTokens().ToList();
            tokens.Count.Should().Be(4);
            tokens[0].Kind().Should().Be(SyntaxKind.UsingKeyword);
            tokens[1].Kind().Should().Be(SyntaxKind.IdentifierToken);
            tokens[2].Kind().Should().Be(SyntaxKind.SemicolonToken);
            tokens[3].Kind().Should().Be(SyntaxKind.EndOfFileToken);
        }

        [Fact]
        public void TestDescendantTokensWithExtraWhitespace()
        {
            var s1 = "  using Goo  ;  ";
            var t1 = SyntaxFactory.ParseSyntaxTree(s1);
            var tokens = t1.GetCompilationUnitRoot().DescendantTokens().ToList();
            tokens.Count.Should().Be(4);
            tokens[0].Kind().Should().Be(SyntaxKind.UsingKeyword);
            tokens[1].Kind().Should().Be(SyntaxKind.IdentifierToken);
            tokens[2].Kind().Should().Be(SyntaxKind.SemicolonToken);
            tokens[3].Kind().Should().Be(SyntaxKind.EndOfFileToken);
        }

        [Fact]
        public void TestDescendantTokensEntireRange()
        {
            var s1 = "extern alias Bar;\r\n" + "using Goo;";
            var t1 = SyntaxFactory.ParseSyntaxTree(s1);
            var tokens = t1.GetCompilationUnitRoot().DescendantTokens().ToList();
            tokens.Count.Should().Be(8);
            tokens[0].Kind().Should().Be(SyntaxKind.ExternKeyword);
            tokens[1].Kind().Should().Be(SyntaxKind.AliasKeyword);
            tokens[2].Kind().Should().Be(SyntaxKind.IdentifierToken);
            tokens[3].Kind().Should().Be(SyntaxKind.SemicolonToken);
            tokens[4].Kind().Should().Be(SyntaxKind.UsingKeyword);
            tokens[5].Kind().Should().Be(SyntaxKind.IdentifierToken);
            tokens[6].Kind().Should().Be(SyntaxKind.SemicolonToken);
            tokens[7].Kind().Should().Be(SyntaxKind.EndOfFileToken);
        }

        [Fact]
        public void TestDescendantTokensOverFullSpan()
        {
            var s1 = "extern alias Bar;\r\n" + "using Goo;";
            var t1 = SyntaxFactory.ParseSyntaxTree(s1);
            var tokens = t1.GetCompilationUnitRoot().DescendantTokens(new TextSpan(0, 16)).ToList();
            tokens.Count.Should().Be(3);
            tokens[0].Kind().Should().Be(SyntaxKind.ExternKeyword);
            tokens[1].Kind().Should().Be(SyntaxKind.AliasKeyword);
            tokens[2].Kind().Should().Be(SyntaxKind.IdentifierToken);
        }

        [Fact]
        public void TestDescendantTokensOverInsideSpan()
        {
            var s1 = "extern alias Bar;\r\n" + "using Goo;";
            var t1 = SyntaxFactory.ParseSyntaxTree(s1);
            var tokens = t1.GetCompilationUnitRoot().DescendantTokens(new TextSpan(1, 14)).ToList();
            tokens.Count.Should().Be(3);
            tokens[0].Kind().Should().Be(SyntaxKind.ExternKeyword);
            tokens[1].Kind().Should().Be(SyntaxKind.AliasKeyword);
            tokens[2].Kind().Should().Be(SyntaxKind.IdentifierToken);
        }

        [Fact]
        public void TestDescendantTokensOverFullSpanOffset()
        {
            var s1 = "extern alias Bar;\r\n" + "using Goo;";
            var t1 = SyntaxFactory.ParseSyntaxTree(s1);
            var tokens = t1.GetCompilationUnitRoot().DescendantTokens(new TextSpan(7, 17)).ToList();
            tokens.Count.Should().Be(4);
            tokens[0].Kind().Should().Be(SyntaxKind.AliasKeyword);
            tokens[1].Kind().Should().Be(SyntaxKind.IdentifierToken);
            tokens[2].Kind().Should().Be(SyntaxKind.SemicolonToken);
            tokens[3].Kind().Should().Be(SyntaxKind.UsingKeyword);
        }

        [Fact]
        public void TestDescendantTokensOverInsideSpanOffset()
        {
            var s1 = "extern alias Bar;\r\n" + "using Goo;";
            var t1 = SyntaxFactory.ParseSyntaxTree(s1);
            var tokens = t1.GetCompilationUnitRoot().DescendantTokens(new TextSpan(8, 15)).ToList();
            tokens.Count.Should().Be(4);
            tokens[0].Kind().Should().Be(SyntaxKind.AliasKeyword);
            tokens[1].Kind().Should().Be(SyntaxKind.IdentifierToken);
            tokens[2].Kind().Should().Be(SyntaxKind.SemicolonToken);
            tokens[3].Kind().Should().Be(SyntaxKind.UsingKeyword);
        }

        [Fact]
        public void TestDescendantTrivia()
        {
            var text = "// goo\r\na + b";
            var expr = SyntaxFactory.ParseExpression(text);

            var list = expr.DescendantTrivia().ToList();
            list.Count.Should().Be(4);
            list[0].Kind().Should().Be(SyntaxKind.SingleLineCommentTrivia);
            list[1].Kind().Should().Be(SyntaxKind.EndOfLineTrivia);
            list[2].Kind().Should().Be(SyntaxKind.WhitespaceTrivia);
            list[3].Kind().Should().Be(SyntaxKind.WhitespaceTrivia);
        }

        [Fact]
        public void TestDescendantTriviaIntoStructuredTrivia()
        {
            var text = @"
/// <goo >
/// </goo>
a + b";
            var expr = SyntaxFactory.ParseExpression(text);

            var list = expr.DescendantTrivia(descendIntoTrivia: true).ToList();
            list.Count.Should().Be(7);
            list[0].Kind().Should().Be(SyntaxKind.EndOfLineTrivia);
            list[1].Kind().Should().Be(SyntaxKind.SingleLineDocumentationCommentTrivia);
            list[2].Kind().Should().Be(SyntaxKind.DocumentationCommentExteriorTrivia);
            list[3].Kind().Should().Be(SyntaxKind.WhitespaceTrivia);
            list[4].Kind().Should().Be(SyntaxKind.DocumentationCommentExteriorTrivia);
            list[5].Kind().Should().Be(SyntaxKind.WhitespaceTrivia);
            list[6].Kind().Should().Be(SyntaxKind.WhitespaceTrivia);
        }

        [Fact]
        public void Bug877223()
        {
            var s1 = "using Goo;";
            var t1 = SyntaxFactory.ParseSyntaxTree(s1);

            // var node = t1.GetCompilationUnitRoot().Usings[0].GetTokens(new TextSpan(6, 3)).First();
            var node = t1.GetCompilationUnitRoot().DescendantTokens(new TextSpan(6, 3)).First();
            node.ToString().Should().Be("Goo");
        }

        [Fact]
        public void TestFindToken()
        {
            var text = "class\n #if XX\n#endif\n goo { }";
            var tree = SyntaxFactory.ParseSyntaxTree(text);

            var token = tree.GetCompilationUnitRoot().FindToken("class\n #i".Length);
            token.Kind().Should().Be(SyntaxKind.IdentifierToken);
            token.ToString().Should().Be("goo");
            token = tree.GetCompilationUnitRoot().FindToken("class\n #i".Length, findInsideTrivia: true);
            token.Kind().Should().Be(SyntaxKind.IfKeyword);
        }

        [Fact]
        public void TestFindTokenInLargeList()
        {
            var identifier = SyntaxFactory.Identifier("x");
            var missingIdentifier = SyntaxFactory.MissingToken(SyntaxKind.IdentifierToken);
            var name = SyntaxFactory.IdentifierName(identifier);
            var missingName = SyntaxFactory.IdentifierName(missingIdentifier);
            var comma = SyntaxFactory.Token(SyntaxKind.CommaToken);
            var missingComma = SyntaxFactory.MissingToken(SyntaxKind.CommaToken);
            var argument = SyntaxFactory.Argument(name);
            var missingArgument = SyntaxFactory.Argument(missingName);

            // make a large list that has lots of zero-length nodes (that shouldn't be found)
            var nodesAndTokens = SyntaxFactory.NodeOrTokenList(
                missingArgument, missingComma,
                missingArgument, missingComma,
                missingArgument, missingComma,
                missingArgument, missingComma,
                missingArgument, missingComma,
                missingArgument, missingComma,
                missingArgument, missingComma,
                missingArgument, missingComma,
                argument);

            var argumentList = SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>(SyntaxFactory.NodeOrTokenList(nodesAndTokens)));
            var invocation = SyntaxFactory.InvocationExpression(name, argumentList);
            CheckFindToken(invocation);
        }

        private void CheckFindToken(SyntaxNode node)
        {
            for (int i = 0; i < node.FullSpan.End; i++)
            {
                var token = node.FindToken(i);
                token.FullSpan.Contains(i).Should().BeTrue();
            }
        }

        [WorkItem(755236, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/755236")]
        [Fact]
        public void TestFindNode()
        {
            var text = "class\n #if XX\n#endif\n goo { }\n class bar { }";
            var tree = SyntaxFactory.ParseSyntaxTree(text);

            var root = tree.GetRoot();
            root.FindNode(root.Span, findInsideTrivia: false).Should().Be(root);
            root.FindNode(root.Span, findInsideTrivia: true).Should().Be(root);

            var classDecl = (TypeDeclarationSyntax)root.ChildNodes().First();

            // IdentifierNameSyntax in trivia.
            var identifier = root.DescendantNodes(descendIntoTrivia: true).Single(n => n is IdentifierNameSyntax);
            var position = identifier.Span.Start + 1;

            root.FindNode(identifier.Span, findInsideTrivia: false).Should().Be(classDecl);
            root.FindNode(identifier.Span, findInsideTrivia: true).Should().Be(identifier);

            // Token span.
            root.FindNode(classDecl.Identifier.Span, findInsideTrivia: false).Should().Be(classDecl);

            // EOF Token span.
            var EOFSpan = new TextSpan(root.FullSpan.End, 0);
            root.FindNode(EOFSpan, findInsideTrivia: false).Should().Be(root);
            root.FindNode(EOFSpan, findInsideTrivia: true).Should().Be(root);

            // EOF Invalid span for childnode
            var classDecl2 = (TypeDeclarationSyntax)root.ChildNodes().Last();
            Assert.Throws<ArgumentOutOfRangeException>(() => classDecl2.FindNode(EOFSpan));

            // Check end position included in node span
            var nodeEndPositionSpan = new TextSpan(classDecl.FullSpan.End, 0);

            root.FindNode(nodeEndPositionSpan, findInsideTrivia: false).Should().Be(classDecl2);
            root.FindNode(nodeEndPositionSpan, findInsideTrivia: true).Should().Be(classDecl2);
            classDecl2.FindNode(nodeEndPositionSpan, findInsideTrivia: false).Should().Be(classDecl2);
            classDecl2.FindNode(nodeEndPositionSpan, findInsideTrivia: true).Should().Be(classDecl2);

            Assert.Throws<ArgumentOutOfRangeException>(() => classDecl.FindNode(nodeEndPositionSpan));

            // Invalid spans.
            var invalidSpan = new TextSpan(100, 100);
            Assert.Throws<ArgumentOutOfRangeException>(() => root.FindNode(invalidSpan));
            invalidSpan = new TextSpan(root.FullSpan.End - 1, 2);
            Assert.Throws<ArgumentOutOfRangeException>(() => root.FindNode(invalidSpan));
            invalidSpan = new TextSpan(classDecl2.FullSpan.Start - 1, root.FullSpan.End);
            Assert.Throws<ArgumentOutOfRangeException>(() => classDecl2.FindNode(invalidSpan));
            invalidSpan = new TextSpan(classDecl.FullSpan.End, root.FullSpan.End);
            Assert.Throws<ArgumentOutOfRangeException>(() => classDecl2.FindNode(invalidSpan));
            // Parent node's span.
            Assert.Throws<ArgumentOutOfRangeException>(() => classDecl.FindNode(root.FullSpan));
        }

        [WorkItem(539941, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/539941")]
        [Fact]
        public void TestFindTriviaNoTriviaExistsAtPosition()
        {
            var code = @"class Goo
{
    void Bar()
    {
    }
}";
            var tree = SyntaxFactory.ParseSyntaxTree(code);
            var position = tree.GetText().Lines[2].End - 1;
            // position points to the closing parenthesis on the line that has "void Bar()"
            // There should be no trivia at this position
            var trivia = tree.GetCompilationUnitRoot().FindTrivia(position);
            trivia.Kind().Should().Be(SyntaxKind.None);
            trivia.SpanStart.Should().Be(0);
            trivia.Span.End.Should().Be(0);
            trivia.Should().Be(default(SyntaxTrivia));
        }

        [Fact]
        public void TestTreeEquivalentToSelf()
        {
            var text = "class goo { }";
            var tree = SyntaxFactory.ParseSyntaxTree(text);
            tree.GetCompilationUnitRoot().IsEquivalentTo(tree.GetCompilationUnitRoot()).Should().BeTrue();
        }

        [Fact]
        public void TestTreeNotEquivalentToNull()
        {
            var text = "class goo { }";
            var tree = SyntaxFactory.ParseSyntaxTree(text);
            tree.GetCompilationUnitRoot().IsEquivalentTo(null).Should().BeFalse();
        }

        [Fact]
        public void TestTreesFromSameSourceEquivalent()
        {
            var text = "class goo { }";
            var tree1 = SyntaxFactory.ParseSyntaxTree(text);
            var tree2 = SyntaxFactory.ParseSyntaxTree(text);
            tree2.GetCompilationUnitRoot().Should().NotBe(tree1.GetCompilationUnitRoot());
            tree1.GetCompilationUnitRoot().IsEquivalentTo(tree2.GetCompilationUnitRoot()).Should().BeTrue();
        }

        [Fact]
        public void TestDifferentTreesNotEquivalent()
        {
            var tree1 = SyntaxFactory.ParseSyntaxTree("class goo { }");
            var tree2 = SyntaxFactory.ParseSyntaxTree("class bar { }");
            tree2.GetCompilationUnitRoot().Should().NotBe(tree1.GetCompilationUnitRoot());
            tree1.GetCompilationUnitRoot().IsEquivalentTo(tree2.GetCompilationUnitRoot()).Should().BeFalse();
        }

        [Fact]
        public void TestVastlyDifferentTreesNotEquivalent()
        {
            var tree1 = SyntaxFactory.ParseSyntaxTree("class goo { }");
            var tree2 = SyntaxFactory.ParseSyntaxTree(string.Empty);
            tree2.GetCompilationUnitRoot().Should().NotBe(tree1.GetCompilationUnitRoot());
            tree1.GetCompilationUnitRoot().IsEquivalentTo(tree2.GetCompilationUnitRoot()).Should().BeFalse();
        }

        [Fact]
        public void TestSimilarSubtreesEquivalent()
        {
            var tree1 = SyntaxFactory.ParseSyntaxTree("class goo { void M() { } }");
            var tree2 = SyntaxFactory.ParseSyntaxTree("class bar { void M() { } }");
            var m1 = ((TypeDeclarationSyntax)tree1.GetCompilationUnitRoot().Members[0]).Members[0];
            var m2 = ((TypeDeclarationSyntax)tree2.GetCompilationUnitRoot().Members[0]).Members[0];
            m1.Kind().Should().Be(SyntaxKind.MethodDeclaration);
            m2.Kind().Should().Be(SyntaxKind.MethodDeclaration);
            m2.Should().NotBe(m1);
            m1.IsEquivalentTo(m2).Should().BeTrue();
        }

        [Fact]
        public void TestTreesWithDifferentTriviaAreNotEquivalent()
        {
            var tree1 = SyntaxFactory.ParseSyntaxTree("class goo {void M() { }}");
            var tree2 = SyntaxFactory.ParseSyntaxTree("class goo { void M() { } }");
            tree1.GetCompilationUnitRoot().IsEquivalentTo(tree2.GetCompilationUnitRoot()).Should().BeFalse();
        }

        [Fact]
        public void TestNodeIncrementallyEquivalentToSelf()
        {
            var text = "class goo { }";
            var tree = SyntaxFactory.ParseSyntaxTree(text);
            tree.GetCompilationUnitRoot().IsIncrementallyIdenticalTo(tree.GetCompilationUnitRoot()).Should().BeTrue();
        }

        [Fact]
        public void TestTokenIncrementallyEquivalentToSelf()
        {
            var text = "class goo { }";
            var tree = SyntaxFactory.ParseSyntaxTree(text);
            tree.GetCompilationUnitRoot().EndOfFileToken.IsIncrementallyIdenticalTo(tree.GetCompilationUnitRoot().EndOfFileToken).Should().BeTrue();
        }

        [Fact]
        public void TestDifferentTokensFromSameTreeNotIncrementallyEquivalentToSelf()
        {
            var text = "class goo { }";
            var tree = SyntaxFactory.ParseSyntaxTree(text);
            tree.GetCompilationUnitRoot().GetFirstToken().IsIncrementallyIdenticalTo(tree.GetCompilationUnitRoot().GetFirstToken().GetNextToken()).Should().BeFalse();
        }

        [Fact]
        public void TestCachedTokensFromDifferentTreesIncrementallyEquivalentToSelf()
        {
            var text = "class goo { }";
            var tree1 = SyntaxFactory.ParseSyntaxTree(text);
            var tree2 = SyntaxFactory.ParseSyntaxTree(text);
            tree1.GetCompilationUnitRoot().GetFirstToken().IsIncrementallyIdenticalTo(tree2.GetCompilationUnitRoot().GetFirstToken()).Should().BeTrue();
        }

        [Fact]
        public void TestNodesFromSameContentNotIncrementallyParsedNotIncrementallyEquivalent()
        {
            var text = "class goo { }";
            var tree1 = SyntaxFactory.ParseSyntaxTree(text);
            var tree2 = SyntaxFactory.ParseSyntaxTree(text);
            tree1.GetCompilationUnitRoot().IsIncrementallyIdenticalTo(tree2.GetCompilationUnitRoot()).Should().BeFalse();
        }

        [Fact]
        public void TestNodesFromIncrementalParseIncrementallyEquivalent1()
        {
            var text = "class goo { void M() { } }";
            var tree1 = SyntaxFactory.ParseSyntaxTree(text);
            var tree2 = tree1.WithChangedText(tree1.GetText().WithChanges(new TextChange(default, " ")));
            tree1.GetCompilationUnitRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().Single().IsIncrementallyIdenticalTo(
                tree2.GetCompilationUnitRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().Single()).Should().BeTrue();
        }

        [Fact]
        public void TestNodesFromIncrementalParseNotIncrementallyEquivalent1()
        {
            var text = "class goo { void M() { } }";
            var tree1 = SyntaxFactory.ParseSyntaxTree(text);
            var tree2 = tree1.WithChangedText(tree1.GetText().WithChanges(new TextChange(new TextSpan(22, 0), " return; ")));
            tree1.GetCompilationUnitRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().Single().IsIncrementallyIdenticalTo(
                tree2.GetCompilationUnitRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().Single()).Should().BeFalse();
        }

        [Fact, WorkItem(536664, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/536664")]
        public void TestTriviaNodeCached()
        {
            var tree = SyntaxFactory.ParseSyntaxTree(" class goo {}");

            // get to the trivia node
            var trivia = tree.GetCompilationUnitRoot().Members[0].GetLeadingTrivia()[0];

            // we get the trivia again
            var triviaAgain = tree.GetCompilationUnitRoot().Members[0].GetLeadingTrivia()[0];

            // should NOT return two distinct objects for trivia and triviaAgain - struct now.
            SyntaxTrivia.Equals(trivia, triviaAgain).Should().BeTrue();
        }

        [Fact]
        public void TestGetFirstToken()
        {
            var tree = SyntaxFactory.ParseSyntaxTree("public static class goo { }");
            var first = tree.GetCompilationUnitRoot().GetFirstToken();
            first.Kind().Should().Be(SyntaxKind.PublicKeyword);
        }

        [Fact]
        public void TestGetFirstTokenIncludingZeroWidth()
        {
            var tree = SyntaxFactory.ParseSyntaxTree("public static class goo { }");
            var first = tree.GetCompilationUnitRoot().GetFirstToken(includeZeroWidth: true);
            first.Kind().Should().Be(SyntaxKind.PublicKeyword);
        }

        [Fact]
        public void TestGetLastToken()
        {
            var tree = SyntaxFactory.ParseSyntaxTree("public static class goo { }");
            var last = tree.GetCompilationUnitRoot().GetLastToken();
            last.Kind().Should().Be(SyntaxKind.CloseBraceToken);
        }

        [Fact]
        public void TestGetLastTokenIncludingZeroWidth()
        {
            var tree = SyntaxFactory.ParseSyntaxTree("public static class goo { ");
            var last = tree.GetCompilationUnitRoot().GetLastToken(includeZeroWidth: true);
            last.Kind().Should().Be(SyntaxKind.EndOfFileToken);

            last = tree.GetCompilationUnitRoot().Members[0].GetLastToken(includeZeroWidth: true);
            last.Kind().Should().Be(SyntaxKind.CloseBraceToken);
            last.IsMissing.Should().BeTrue();
            last.FullSpan.Start.Should().Be(26);
        }

        [Fact]
        public void TestReverseChildSyntaxList()
        {
            var tree1 = SyntaxFactory.ParseSyntaxTree("class A {} public class B {} public static class C {}");
            var root1 = tree1.GetCompilationUnitRoot();
            TestReverse(root1.ChildNodesAndTokens());
            TestReverse(root1.Members[0].ChildNodesAndTokens());
            TestReverse(root1.Members[1].ChildNodesAndTokens());
            TestReverse(root1.Members[2].ChildNodesAndTokens());
        }

        private void TestReverse(ChildSyntaxList children)
        {
            var list1 = children.AsEnumerable().Reverse().ToList();
            var list2 = children.Reverse().ToList();
            list2.Count.Should().Be(list1.Count);
            for (int i = 0; i < list1.Count; i++)
            {
                list2[i].Should().Be(list1[i]);
                list2[i].FullSpan.Start.Should().Be(list1[i].FullSpan.Start);
            }
        }

        [Fact]
        public void TestGetNextToken()
        {
            var tree = SyntaxFactory.ParseSyntaxTree("public static class goo { }");
            var tokens = tree.GetCompilationUnitRoot().DescendantTokens().ToList();

            var list = new List<SyntaxToken>();
            var token = tree.GetCompilationUnitRoot().GetFirstToken(includeSkipped: true);
            while (token.Kind() != SyntaxKind.None)
            {
                list.Add(token);
                token = token.GetNextToken(includeSkipped: true);
            }

            // descendant tokens include EOF
            list.Count.Should().Be(tokens.Count - 1);
            for (int i = 0; i < list.Count; i++)
            {
                tokens[i].Should().Be(list[i]);
            }
        }

        [Fact]
        public void TestGetNextTokenIncludingSkippedTokens()
        {
            var text =
@"garbage
using goo.bar;
";
            var tree = SyntaxFactory.ParseSyntaxTree(text);
            tree.GetCompilationUnitRoot().ToFullString().Should().Be(text);

            var tokens = tree.GetCompilationUnitRoot().DescendantTokens(descendIntoTrivia: true).Where(SyntaxToken.NonZeroWidth).ToList();
            tokens.Count.Should().Be(6);
            tokens[0].Text.Should().Be("garbage");

            var list = new List<SyntaxToken>(tokens.Count);
            var token = tree.GetCompilationUnitRoot().GetFirstToken(includeSkipped: true);
            while (token.Kind() != SyntaxKind.None)
            {
                list.Add(token);
                token = token.GetNextToken(includeSkipped: true);
            }

            list.Count.Should().Be(tokens.Count);
            for (int i = 0; i < tokens.Count; i++)
            {
                tokens[i].Should().Be(list[i]);
            }
        }

        [Fact]
        public void TestGetNextTokenExcludingSkippedTokens()
        {
            var tree = SyntaxFactory.ParseSyntaxTree(
@"garbage
using goo.bar;
");
            var tokens = tree.GetCompilationUnitRoot().DescendantTokens().ToList();
            tokens.Count.Should().Be(6);

            var list = new List<SyntaxToken>(tokens.Count);
            var token = tree.GetCompilationUnitRoot().GetFirstToken(includeSkipped: false);
            while (token.Kind() != SyntaxKind.None)
            {
                list.Add(token);
                token = token.GetNextToken(includeSkipped: false);
            }

            // descendant tokens includes EOF
            list.Count.Should().Be(tokens.Count - 1);
            for (int i = 0; i < list.Count; i++)
            {
                tokens[i].Should().Be(list[i]);
            }
        }

        [Fact]
        public void TestGetNextTokenCommon()
        {
            SyntaxTree syntaxTree = SyntaxFactory.ParseSyntaxTree("public static class goo { }");
            List<SyntaxToken> tokens = syntaxTree.GetRoot().DescendantTokens().ToList();

            List<SyntaxToken> list = new List<SyntaxToken>();
            SyntaxToken token = syntaxTree.GetRoot().GetFirstToken();
            while (token.RawKind != 0)
            {
                list.Add(token);
                token = token.GetNextToken();
            }

            // descendant tokens includes EOF
            list.Count.Should().Be(tokens.Count - 1);
            for (int i = 0; i < list.Count; i++)
            {
                tokens[i].Should().Be(list[i]);
            }
        }

        [Fact]
        public void TestGetPreviousToken()
        {
            var tree = SyntaxFactory.ParseSyntaxTree("public static class goo { }");
            var tokens = tree.GetCompilationUnitRoot().DescendantTokens().ToList();

            var list = new List<SyntaxToken>();
            var token = tree.GetCompilationUnitRoot().GetLastToken(); // skip EOF
            while (token.Kind() != SyntaxKind.None)
            {
                list.Add(token);
                token = token.GetPreviousToken();
            }
            list.Reverse();

            // descendant tokens includes EOF
            list.Count.Should().Be(tokens.Count - 1);
            for (int i = 0; i < list.Count; i++)
            {
                tokens[i].Should().Be(list[i]);
            }
        }

        [Fact]
        public void TestGetPreviousTokenIncludingSkippedTokens()
        {
            var text =
@"garbage
using goo.bar;
";
            var tree = SyntaxFactory.ParseSyntaxTree(text);
            tree.GetCompilationUnitRoot().ToFullString().Should().Be(text);

            var tokens = tree.GetCompilationUnitRoot().DescendantTokens(descendIntoTrivia: true).Where(SyntaxToken.NonZeroWidth).ToList();
            tokens.Count.Should().Be(6);
            tokens[0].Text.Should().Be("garbage");

            var list = new List<SyntaxToken>(tokens.Count);
            var token = tree.GetCompilationUnitRoot().GetLastToken(includeSkipped: true);
            while (token.Kind() != SyntaxKind.None)
            {
                list.Add(token);
                token = token.GetPreviousToken(includeSkipped: true);
            }
            list.Reverse();

            list.Count.Should().Be(tokens.Count);
            for (int i = 0; i < tokens.Count; i++)
            {
                tokens[i].Should().Be(list[i]);
            }
        }

        [Fact]
        public void TestGetPreviousTokenExcludingSkippedTokens()
        {
            var text =
@"garbage
using goo.bar;
";
            var tree = SyntaxFactory.ParseSyntaxTree(text);
            tree.GetCompilationUnitRoot().ToFullString().Should().Be(text);

            var tokens = tree.GetCompilationUnitRoot().DescendantTokens().ToList();
            tokens.Count.Should().Be(6);

            var list = new List<SyntaxToken>(tokens.Count);
            var token = tree.GetCompilationUnitRoot().GetLastToken(includeSkipped: false);
            while (token.Kind() != SyntaxKind.None)
            {
                list.Add(token);
                token = token.GetPreviousToken(includeSkipped: false);
            }
            list.Reverse();

            // descendant tokens includes EOF
            list.Count + 1.Should().Be(tokens.Count);
            for (int i = 0; i < list.Count; i++)
            {
                list[i].Should().Be(tokens[i]);
            }
        }

        [Fact]
        public void TestGetPreviousTokenCommon()
        {
            SyntaxTree syntaxTree = SyntaxFactory.ParseSyntaxTree("public static class goo { }");
            List<SyntaxToken> tokens = syntaxTree.GetRoot().DescendantTokens().ToList();

            List<SyntaxToken> list = new List<SyntaxToken>();
            var token = syntaxTree.GetRoot().GetLastToken(includeZeroWidth: false); // skip EOF

            while (token.RawKind != 0)
            {
                list.Add(token);
                token = token.GetPreviousToken();
            }
            list.Reverse();

            // descendant tokens include EOF
            list.Count.Should().Be(tokens.Count - 1);
            for (int i = 0; i < list.Count; i++)
            {
                tokens[i].Should().Be(list[i]);
            }
        }

        [Fact]
        public void TestGetNextTokenIncludingZeroWidth()
        {
            var tree = SyntaxFactory.ParseSyntaxTree("public static class goo {");
            var tokens = tree.GetCompilationUnitRoot().DescendantTokens().ToList();

            var list = new List<SyntaxToken>();
            var token = tree.GetCompilationUnitRoot().GetFirstToken(includeZeroWidth: true);
            while (token.Kind() != SyntaxKind.None)
            {
                list.Add(token);
                token = token.GetNextToken(includeZeroWidth: true);
            }

            list.Count.Should().Be(tokens.Count);
            for (int i = 0; i < tokens.Count; i++)
            {
                tokens[i].Should().Be(list[i]);
            }
        }

        [Fact]
        public void TestGetNextTokenIncludingZeroWidthCommon()
        {
            SyntaxTree syntaxTree = SyntaxFactory.ParseSyntaxTree("public static class goo {");
            List<SyntaxToken> tokens = syntaxTree.GetRoot().DescendantTokens().ToList();

            List<SyntaxToken> list = new List<SyntaxToken>();
            SyntaxToken token = syntaxTree.GetRoot().GetFirstToken(includeZeroWidth: true);
            while (token.RawKind != 0)
            {
                list.Add(token);
                token = token.GetNextToken(includeZeroWidth: true);
            }

            list.Count.Should().Be(tokens.Count);
            for (int i = 0; i < tokens.Count; i++)
            {
                tokens[i].Should().Be(list[i]);
            }
        }

        [Fact]
        public void TestGetPreviousTokenIncludingZeroWidth()
        {
            var tree = SyntaxFactory.ParseSyntaxTree("public static class goo {");
            var tokens = tree.GetCompilationUnitRoot().DescendantTokens().ToList();

            var list = new List<SyntaxToken>();
            var token = tree.GetCompilationUnitRoot().EndOfFileToken.GetPreviousToken(includeZeroWidth: true);
            while (token.Kind() != SyntaxKind.None)
            {
                list.Add(token);
                token = token.GetPreviousToken(includeZeroWidth: true);
            }

            list.Reverse();

            // descendant tokens include EOF
            list.Count.Should().Be(tokens.Count - 1);
            for (int i = 0; i < list.Count; i++)
            {
                tokens[i].Should().Be(list[i]);
            }
        }

        [Fact]
        public void TestGetPreviousTokenIncludingZeroWidthCommon()
        {
            SyntaxTree syntaxTree = SyntaxFactory.ParseSyntaxTree("public static class goo {");
            List<SyntaxToken> tokens = syntaxTree.GetRoot().DescendantTokens().ToList();

            List<SyntaxToken> list = new List<SyntaxToken>();
            SyntaxToken token = ((SyntaxToken)((SyntaxTree)syntaxTree).GetCompilationUnitRoot().EndOfFileToken).GetPreviousToken(includeZeroWidth: true);
            while (token.RawKind != 0)
            {
                list.Add(token);
                token = token.GetPreviousToken(includeZeroWidth: true);
            }
            list.Reverse();

            // descendant tokens includes EOF
            list.Count.Should().Be(tokens.Count - 1);
            for (int i = 0; i < list.Count; i++)
            {
                list[i].Should().Be(tokens[i]);
            }
        }

        [Fact]
        public void TestGetNextSibling()
        {
            var tree = SyntaxFactory.ParseSyntaxTree("public static class goo { }");
            var children = tree.GetCompilationUnitRoot().Members[0].ChildNodesAndTokens().ToList();
            var list = new List<SyntaxNodeOrToken>();
            for (var child = children[0]; child.Kind() != SyntaxKind.None; child = child.GetNextSibling())
            {
                list.Add(child);
            }

            list.Count.Should().Be(children.Count);
            for (int i = 0; i < children.Count; i++)
            {
                children[i].Should().Be(list[i]);
            }
        }

        [Fact]
        public void TestGetPreviousSibling()
        {
            var tree = SyntaxFactory.ParseSyntaxTree("public static class goo { }");
            var children = tree.GetCompilationUnitRoot().Members[0].ChildNodesAndTokens().ToList();
            var reversed = children.AsEnumerable().Reverse().ToList();
            var list = new List<SyntaxNodeOrToken>();
            for (var child = children[children.Count - 1]; child.Kind() != SyntaxKind.None; child = child.GetPreviousSibling())
            {
                list.Add(child);
            }

            list.Count.Should().Be(children.Count);
            for (int i = 0; i < reversed.Count; i++)
            {
                reversed[i].Should().Be(list[i]);
            }
        }

        [Fact]
        public void TestSyntaxNodeOrTokenEquality()
        {
            var tree = SyntaxFactory.ParseSyntaxTree("public static class goo { }");
            var child = tree.GetCompilationUnitRoot().ChildNodesAndTokens()[0];
            var member = (TypeDeclarationSyntax)tree.GetCompilationUnitRoot().Members[0];
            child.Should().Be((SyntaxNodeOrToken)member);

            var name = member.Identifier;
            var nameChild = member.ChildNodesAndTokens()[3];
            nameChild.Should().Be((SyntaxNodeOrToken)name);

            var closeBraceToken = member.CloseBraceToken;
            var closeBraceChild = member.GetLastToken();
            closeBraceChild.Should().Be((SyntaxNodeOrToken)closeBraceToken);
        }

        [Fact]
        public void TestStructuredTriviaHasNoParent()
        {
            var tree = SyntaxFactory.ParseSyntaxTree("#define GOO");
            var trivia = tree.GetCompilationUnitRoot().EndOfFileToken.GetLeadingTrivia()[0];
            trivia.Kind().Should().Be(SyntaxKind.DefineDirectiveTrivia);
            trivia.HasStructure.Should().BeTrue();
            trivia.GetStructure().Should().NotBeNull();
            trivia.GetStructure().Parent.Should().BeNull();
        }

        [Fact]
        public void TestStructuredTriviaHasParentTrivia()
        {
            var tree = SyntaxFactory.ParseSyntaxTree("#define GOO");
            var trivia = tree.GetCompilationUnitRoot().EndOfFileToken.GetLeadingTrivia()[0];
            trivia.Kind().Should().Be(SyntaxKind.DefineDirectiveTrivia);
            trivia.HasStructure.Should().BeTrue();
            trivia.GetStructure().Should().NotBeNull();
            var parentTrivia = trivia.GetStructure().ParentTrivia;
            parentTrivia.Kind().Should().NotBe(SyntaxKind.None);
            parentTrivia.Should().Be(trivia);
        }

        [Fact]
        public void TestStructuredTriviaParentTrivia()
        {
            var def = SyntaxFactory.DefineDirectiveTrivia(SyntaxFactory.Identifier("GOO"), false);

            // unrooted structured trivia should report parent trivia as default 
            def.ParentTrivia.Should().Be(default(SyntaxTrivia));

            var trivia = SyntaxFactory.Trivia(def);
            var structure = trivia.GetStructure();
            structure.Should().NotBe(def);  // these should not be identity equals
            def.IsEquivalentTo(structure).Should().BeTrue(); // they should be equivalent though
            structure.ParentTrivia.Should().Be(trivia); // parent trivia should be equal to original trivia

            // attach trivia to token and walk down to structured trivia and back up again
            var token = SyntaxFactory.Identifier(default(SyntaxTriviaList), "x", SyntaxTriviaList.Create(trivia));
            var tokenTrivia = token.TrailingTrivia[0];
            var tokenStructuredTrivia = tokenTrivia.GetStructure();
            var tokenStructuredParentTrivia = tokenStructuredTrivia.ParentTrivia;
            tokenStructuredParentTrivia.Should().Be(tokenTrivia);
            tokenStructuredParentTrivia.Token.Should().Be(token);
        }

        [Fact]
        public void TestGetFirstDirective()
        {
            var tree = SyntaxFactory.ParseSyntaxTree("#define GOO");
            var d = tree.GetCompilationUnitRoot().GetFirstDirective();
            d.Should().NotBeNull();
            d.Kind().Should().Be(SyntaxKind.DefineDirectiveTrivia);
        }

        [Fact]
        public void TestGetLastDirective()
        {
            var tree = SyntaxFactory.ParseSyntaxTree(
@"#define GOO
#undef GOO
");
            var d = tree.GetCompilationUnitRoot().GetLastDirective();
            d.Should().NotBeNull();
            d.Kind().Should().Be(SyntaxKind.UndefDirectiveTrivia);
        }

        [Fact]
        public void TestGetNextDirective()
        {
            var tree = SyntaxFactory.ParseSyntaxTree(
@"#define GOO
#define BAR
class C {
#if GOO
   void M() { }
#endif
}
");
            var d1 = tree.GetCompilationUnitRoot().GetFirstDirective();
            d1.Should().NotBeNull();
            d1.Kind().Should().Be(SyntaxKind.DefineDirectiveTrivia);
            var d2 = d1.GetNextDirective();
            d2.Should().NotBeNull();
            d2.Kind().Should().Be(SyntaxKind.DefineDirectiveTrivia);
            var d3 = d2.GetNextDirective();
            d3.Should().NotBeNull();
            d3.Kind().Should().Be(SyntaxKind.IfDirectiveTrivia);
            var d4 = d3.GetNextDirective();
            d4.Should().NotBeNull();
            d4.Kind().Should().Be(SyntaxKind.EndIfDirectiveTrivia);
            var d5 = d4.GetNextDirective();
            d5.Should().BeNull();
        }

        [Fact]
        public void TestGetPreviousDirective()
        {
            var tree = SyntaxFactory.ParseSyntaxTree(
@"#define GOO
#define BAR
class C {
#if GOO
   void M() { }
#endif
}
");
            var d1 = tree.GetCompilationUnitRoot().GetLastDirective();
            d1.Should().NotBeNull();
            d1.Kind().Should().Be(SyntaxKind.EndIfDirectiveTrivia);
            var d2 = d1.GetPreviousDirective();
            d2.Should().NotBeNull();
            d2.Kind().Should().Be(SyntaxKind.IfDirectiveTrivia);
            var d3 = d2.GetPreviousDirective();
            d3.Should().NotBeNull();
            d3.Kind().Should().Be(SyntaxKind.DefineDirectiveTrivia);
            var d4 = d3.GetPreviousDirective();
            d4.Should().NotBeNull();
            d4.Kind().Should().Be(SyntaxKind.DefineDirectiveTrivia);
            var d5 = d4.GetPreviousDirective();
            d5.Should().BeNull();
        }

        [Fact]
        public void TestGetNextAndPreviousDirectiveWithDuplicateTrivia()
        {
            var tree = SyntaxFactory.ParseSyntaxTree(
@"#region R
class C {
}
");
            var c = tree.GetCompilationUnitRoot().Members[0];

            // Duplicate the leading trivia on the class
            c = c.WithLeadingTrivia(c.GetLeadingTrivia().Concat(c.GetLeadingTrivia()));

            var leadingTriviaWithDuplicate = c.GetLeadingTrivia();
            leadingTriviaWithDuplicate.Count.Should().Be(2);

            var firstDirective = leadingTriviaWithDuplicate[0].GetStructure().Should().BeOfType<RegionDirectiveTriviaSyntax>();
            var secondDirective = leadingTriviaWithDuplicate[1].GetStructure().Should().BeOfType<RegionDirectiveTriviaSyntax>();

            // Test GetNextDirective works correctly
            firstDirective.GetNextDirective().Should().BeSameAs(secondDirective);
            secondDirective.GetNextDirective().Should().BeNull();

            // Test GetPreviousDirective works correctly
            firstDirective.GetPreviousDirective().Should().BeNull();
            firstDirective.Should().BeSameAs(secondDirective.GetPreviousDirective());
        }

        [Fact]
        public void TestGetDirectivesRelatedToIf()
        {
            var tree = SyntaxFactory.ParseSyntaxTree(
@"#define GOO
#if GOO
class A { }
#elif BAR
class B { }
#elif BAZ
class B { }
#else 
class C { }
#endif
");
            var d = tree.GetCompilationUnitRoot().GetFirstDirective();
            d.Should().NotBeNull();
            d.Kind().Should().Be(SyntaxKind.DefineDirectiveTrivia);
            d = d.GetNextDirective();
            d.Should().NotBeNull();
            d.Kind().Should().Be(SyntaxKind.IfDirectiveTrivia);
            var related = d.GetRelatedDirectives();
            related.Should().NotBeNull();
            related.Count.Should().Be(5);
            related[0].Kind().Should().Be(SyntaxKind.IfDirectiveTrivia);
            related[1].Kind().Should().Be(SyntaxKind.ElifDirectiveTrivia);
            related[2].Kind().Should().Be(SyntaxKind.ElifDirectiveTrivia);
            related[3].Kind().Should().Be(SyntaxKind.ElseDirectiveTrivia);
            related[4].Kind().Should().Be(SyntaxKind.EndIfDirectiveTrivia);
        }

        [Fact]
        public void TestGetDirectivesRelatedToIfElements()
        {
            var tree = SyntaxFactory.ParseSyntaxTree(
@"#define GOO
#if GOO
class A { }
#elif BAR
class B { }
#elif BAZ
class B { }
#else 
class C { }
#endif
");
            var d = tree.GetCompilationUnitRoot().GetFirstDirective();
            d.Should().NotBeNull();
            d.Kind().Should().Be(SyntaxKind.DefineDirectiveTrivia);
            d = d.GetNextDirective();
            d.Should().NotBeNull();
            d.Kind().Should().Be(SyntaxKind.IfDirectiveTrivia);
            var related = d.GetRelatedDirectives();
            related.Should().NotBeNull();
            related.Count.Should().Be(5);
            related[0].Kind().Should().Be(SyntaxKind.IfDirectiveTrivia);
            related[1].Kind().Should().Be(SyntaxKind.ElifDirectiveTrivia);
            related[2].Kind().Should().Be(SyntaxKind.ElifDirectiveTrivia);
            related[3].Kind().Should().Be(SyntaxKind.ElseDirectiveTrivia);
            related[4].Kind().Should().Be(SyntaxKind.EndIfDirectiveTrivia);

            // get directives related to elif
            var related2 = related[1].GetRelatedDirectives();
            related.SequenceEqual(related2).Should().BeTrue();

            // get directives related to else
            var related3 = related[3].GetRelatedDirectives();
            related.SequenceEqual(related3).Should().BeTrue();
        }

        [Fact]
        public void TestGetDirectivesRelatedToEndIf()
        {
            var tree = SyntaxFactory.ParseSyntaxTree(
@"#define GOO
#if GOO
class A { }
#elif BAR
class B { }
#elif BAZ
class B { }
#else 
class C { }
#endif
");
            var d = tree.GetCompilationUnitRoot().GetLastDirective();
            d.Should().NotBeNull();
            d.Kind().Should().Be(SyntaxKind.EndIfDirectiveTrivia);
            var related = d.GetRelatedDirectives();
            related.Should().NotBeNull();
            related.Count.Should().Be(5);
            related[0].Kind().Should().Be(SyntaxKind.IfDirectiveTrivia);
            related[1].Kind().Should().Be(SyntaxKind.ElifDirectiveTrivia);
            related[2].Kind().Should().Be(SyntaxKind.ElifDirectiveTrivia);
            related[3].Kind().Should().Be(SyntaxKind.ElseDirectiveTrivia);
            related[4].Kind().Should().Be(SyntaxKind.EndIfDirectiveTrivia);
        }

        [Fact]
        public void TestGetDirectivesRelatedToIfWithNestedIfEndIF()
        {
            var tree = SyntaxFactory.ParseSyntaxTree(
@"#define GOO
#if GOO
class A { }
#if ZED
  class A1 { }
#endif
#elif BAR
class B { }
#elif BAZ
class B { }
#else 
class C { }
#endif
");
            var d = tree.GetCompilationUnitRoot().GetFirstDirective();
            d.Should().NotBeNull();
            d.Kind().Should().Be(SyntaxKind.DefineDirectiveTrivia);
            d = d.GetNextDirective();
            d.Should().NotBeNull();
            d.Kind().Should().Be(SyntaxKind.IfDirectiveTrivia);
            var related = d.GetRelatedDirectives();
            related.Should().NotBeNull();
            related.Count.Should().Be(5);
            related[0].Kind().Should().Be(SyntaxKind.IfDirectiveTrivia);
            related[1].Kind().Should().Be(SyntaxKind.ElifDirectiveTrivia);
            related[2].Kind().Should().Be(SyntaxKind.ElifDirectiveTrivia);
            related[3].Kind().Should().Be(SyntaxKind.ElseDirectiveTrivia);
            related[4].Kind().Should().Be(SyntaxKind.EndIfDirectiveTrivia);
        }

        [Fact]
        public void TestGetDirectivesRelatedToIfWithNestedRegionEndRegion()
        {
            var tree = SyntaxFactory.ParseSyntaxTree(
@"#define GOO
#if GOO
class A { }
#region some region
  class A1 { }
#endregion
#elif BAR
class B { }
#elif BAZ
class B { }
#else 
class C { }
#endif
");
            var d = tree.GetCompilationUnitRoot().GetFirstDirective();
            d.Should().NotBeNull();
            d.Kind().Should().Be(SyntaxKind.DefineDirectiveTrivia);
            d = d.GetNextDirective();
            d.Should().NotBeNull();
            d.Kind().Should().Be(SyntaxKind.IfDirectiveTrivia);
            var related = d.GetRelatedDirectives();
            related.Should().NotBeNull();
            related.Count.Should().Be(5);
            related[0].Kind().Should().Be(SyntaxKind.IfDirectiveTrivia);
            related[1].Kind().Should().Be(SyntaxKind.ElifDirectiveTrivia);
            related[2].Kind().Should().Be(SyntaxKind.ElifDirectiveTrivia);
            related[3].Kind().Should().Be(SyntaxKind.ElseDirectiveTrivia);
            related[4].Kind().Should().Be(SyntaxKind.EndIfDirectiveTrivia);
        }

        [Fact]
        public void TestGetDirectivesRelatedToEndIfWithNestedIfEndIf()
        {
            var tree = SyntaxFactory.ParseSyntaxTree(
@"#define GOO
#if GOO
class A { }
#if ZED
  class A1 { }
#endif
#elif BAR
class B { }
#elif BAZ
class B { }
#else 
class C { }
#endif
");
            var d = tree.GetCompilationUnitRoot().GetLastDirective();
            d.Should().NotBeNull();
            d.Kind().Should().Be(SyntaxKind.EndIfDirectiveTrivia);
            var related = d.GetRelatedDirectives();
            related.Should().NotBeNull();
            related.Count.Should().Be(5);
            related[0].Kind().Should().Be(SyntaxKind.IfDirectiveTrivia);
            related[1].Kind().Should().Be(SyntaxKind.ElifDirectiveTrivia);
            related[2].Kind().Should().Be(SyntaxKind.ElifDirectiveTrivia);
            related[3].Kind().Should().Be(SyntaxKind.ElseDirectiveTrivia);
            related[4].Kind().Should().Be(SyntaxKind.EndIfDirectiveTrivia);
        }

        [Fact]
        public void TestGetDirectivesRelatedToEndIfWithNestedRegionEndRegion()
        {
            var tree = SyntaxFactory.ParseSyntaxTree(
@"#define GOO
#if GOO
#region some region
class A { }
#endregion
#elif BAR
class B { }
#elif BAZ
class B { }
#else 
class C { }
#endif
");
            var d = tree.GetCompilationUnitRoot().GetLastDirective();
            d.Should().NotBeNull();
            d.Kind().Should().Be(SyntaxKind.EndIfDirectiveTrivia);
            var related = d.GetRelatedDirectives();
            related.Should().NotBeNull();
            related.Count.Should().Be(5);
            related[0].Kind().Should().Be(SyntaxKind.IfDirectiveTrivia);
            related[1].Kind().Should().Be(SyntaxKind.ElifDirectiveTrivia);
            related[2].Kind().Should().Be(SyntaxKind.ElifDirectiveTrivia);
            related[3].Kind().Should().Be(SyntaxKind.ElseDirectiveTrivia);
            related[4].Kind().Should().Be(SyntaxKind.EndIfDirectiveTrivia);
        }

        [Fact]
        public void TestGetDirectivesRelatedToRegion()
        {
            var tree = SyntaxFactory.ParseSyntaxTree(
@"#region Some Region
class A { }
#endregion
#if GOO
#endif
");
            var d = tree.GetCompilationUnitRoot().GetFirstDirective();
            d.Should().NotBeNull();
            d.Kind().Should().Be(SyntaxKind.RegionDirectiveTrivia);
            var related = d.GetRelatedDirectives();
            related.Should().NotBeNull();
            related.Count.Should().Be(2);
            related[0].Kind().Should().Be(SyntaxKind.RegionDirectiveTrivia);
            related[1].Kind().Should().Be(SyntaxKind.EndRegionDirectiveTrivia);
        }

        [Fact]
        public void TestGetDirectivesRelatedToEndRegion()
        {
            var tree = SyntaxFactory.ParseSyntaxTree(
@"
#if GOO
#endif
#region Some Region
class A { }
#endregion
");
            var d = tree.GetCompilationUnitRoot().GetLastDirective();
            d.Should().NotBeNull();
            d.Kind().Should().Be(SyntaxKind.EndRegionDirectiveTrivia);
            var related = d.GetRelatedDirectives();
            related.Should().NotBeNull();
            related.Count.Should().Be(2);
            related[0].Kind().Should().Be(SyntaxKind.RegionDirectiveTrivia);
            related[1].Kind().Should().Be(SyntaxKind.EndRegionDirectiveTrivia);
        }

        [WorkItem(536995, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/536995")]
        [Fact]
        public void TestTextAndSpanWithTrivia1()
        {
            var tree = SyntaxFactory.ParseSyntaxTree(
@"/*START*/namespace Microsoft.CSharp.Test
{
}/*END*/");
            var rootNode = tree.GetCompilationUnitRoot();

            rootNode.ToFullString().Length.Should().Be(rootNode.FullSpan.Length);
            rootNode.ToString().Length.Should().Be(rootNode.Span.Length);
            rootNode.ToString().Contains("/*END*/").Should().BeTrue();
            rootNode.ToString().Contains("/*START*/").Should().BeFalse();
        }

        [WorkItem(536996, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/536996")]
        [Fact]
        public void TestTextAndSpanWithTrivia2()
        {
            var tree = SyntaxFactory.ParseSyntaxTree(
@"/*START*/
namespace Microsoft.CSharp.Test
{
}
/*END*/");
            var rootNode = tree.GetCompilationUnitRoot();

            rootNode.ToFullString().Length.Should().Be(rootNode.FullSpan.Length);
            rootNode.ToString().Length.Should().Be(rootNode.Span.Length);
            rootNode.ToString().Contains("/*END*/").Should().BeTrue();
            rootNode.ToString().Contains("/*START*/").Should().BeFalse();
        }

        [Fact]
        public void TestCreateCommonSyntaxNode()
        {
            var rootNode = SyntaxFactory.ParseSyntaxTree("using X; namespace Y { }").GetCompilationUnitRoot();
            var namespaceNode = rootNode.ChildNodesAndTokens()[1].AsNode();
            var nodeOrToken = (SyntaxNodeOrToken)namespaceNode;
            nodeOrToken.IsNode.Should().BeTrue();
            nodeOrToken.AsNode().Should().Be(namespaceNode);
            nodeOrToken.Parent.Should().Be(rootNode);
            nodeOrToken.FullSpan.Should().Be(namespaceNode.FullSpan);
            nodeOrToken.Span.Should().Be(namespaceNode.Span);
        }

        [Fact, WorkItem(537070, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/537070")]
        public void TestTraversalUsingCommonSyntaxNodeOrToken()
        {
            SyntaxTree syntaxTree = SyntaxFactory.ParseSyntaxTree(@"class c1
{
}");
            var nodeOrToken = (SyntaxNodeOrToken)syntaxTree.GetRoot();
            syntaxTree.GetDiagnostics().Count().Should().Be(0);
            Action<SyntaxNodeOrToken> walk = null;
            walk = (SyntaxNodeOrToken nOrT) =>
            {
                syntaxTree.GetDiagnostics(nOrT).Count().Should().Be(0);
                foreach (var child in nOrT.ChildNodesAndTokens())
                {
                    walk(child);
                }
            };
            walk(nodeOrToken);
        }

        [WorkItem(537747, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/537747")]
        [Fact]
        public void SyntaxTriviaDefaultIsDirective()
        {
            SyntaxTrivia trivia = new SyntaxTrivia();
            trivia.IsDirective.Should().BeFalse();
        }

        [Fact]
        public void SyntaxNames()
        {
            var cc = SyntaxFactory.Token(SyntaxKind.ColonColonToken);
            var lt = SyntaxFactory.Token(SyntaxKind.LessThanToken);
            var gt = SyntaxFactory.Token(SyntaxKind.GreaterThanToken);
            var dot = SyntaxFactory.Token(SyntaxKind.DotToken);
            var gp = SyntaxFactory.SingletonSeparatedList<TypeSyntax>(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.IntKeyword)));

            var externAlias = SyntaxFactory.IdentifierName("alias");
            var goo = SyntaxFactory.IdentifierName("Goo");
            var bar = SyntaxFactory.IdentifierName("Bar");

            // Goo.Bar
            var qualified = SyntaxFactory.QualifiedName(goo, dot, bar);
            qualified.ToString().Should().Be("Goo.Bar");
            qualified.GetUnqualifiedName().Identifier.ValueText.Should().Be("Bar");

            // Bar<int>
            var generic = SyntaxFactory.GenericName(bar.Identifier, SyntaxFactory.TypeArgumentList(lt, gp, gt));
            generic.ToString().Should().Be("Bar<int>");
            generic.GetUnqualifiedName().Identifier.ValueText.Should().Be("Bar");

            // Goo.Bar<int>
            var qualifiedGeneric = SyntaxFactory.QualifiedName(goo, dot, generic);
            qualifiedGeneric.ToString().Should().Be("Goo.Bar<int>");
            qualifiedGeneric.GetUnqualifiedName().Identifier.ValueText.Should().Be("Bar");

            // alias::Goo
            var alias = SyntaxFactory.AliasQualifiedName(externAlias, cc, goo);
            alias.ToString().Should().Be("alias::Goo");
            alias.GetUnqualifiedName().Identifier.ValueText.Should().Be("Goo");

            // alias::Bar<int>
            var aliasGeneric = SyntaxFactory.AliasQualifiedName(externAlias, cc, generic);
            aliasGeneric.ToString().Should().Be("alias::Bar<int>");
            aliasGeneric.GetUnqualifiedName().Identifier.ValueText.Should().Be("Bar");

            // alias::Goo.Bar
            var aliasQualified = SyntaxFactory.QualifiedName(alias, dot, bar);
            aliasQualified.ToString().Should().Be("alias::Goo.Bar");
            aliasQualified.GetUnqualifiedName().Identifier.ValueText.Should().Be("Bar");

            // alias::Goo.Bar<int>
            var aliasQualifiedGeneric = SyntaxFactory.QualifiedName(alias, dot, generic);
            aliasQualifiedGeneric.ToString().Should().Be("alias::Goo.Bar<int>");
            aliasQualifiedGeneric.GetUnqualifiedName().Identifier.ValueText.Should().Be("Bar");
        }

        [Fact]
        public void ZeroWidthTokensInListAreUnique()
        {
            var someToken = SyntaxFactory.MissingToken(SyntaxKind.IntKeyword);
            var list = SyntaxFactory.TokenList(someToken, someToken);
            someToken.Should().Be(someToken);
            list[1].Should().NotBe(list[0]);
        }

        [Fact]
        public void ZeroWidthTokensInParentAreUnique()
        {
            var missingComma = SyntaxFactory.MissingToken(SyntaxKind.CommaToken);
            var omittedArraySize = SyntaxFactory.OmittedArraySizeExpression(SyntaxFactory.Token(SyntaxKind.OmittedArraySizeExpressionToken));
            var spec = SyntaxFactory.ArrayRankSpecifier(
                SyntaxFactory.Token(SyntaxKind.OpenBracketToken),
                SyntaxFactory.SeparatedList<ExpressionSyntax>(new SyntaxNodeOrToken[] { omittedArraySize, missingComma, omittedArraySize, missingComma, omittedArraySize, missingComma, omittedArraySize }),
                SyntaxFactory.Token(SyntaxKind.CloseBracketToken)
                );

            var sizes = spec.Sizes;
            sizes.Count.Should().Be(4);
            sizes.SeparatorCount.Should().Be(3);

            sizes[1].Should().NotBe(sizes[0]);
            sizes[2].Should().NotBe(sizes[0]);
            sizes[3].Should().NotBe(sizes[0]);
            sizes[2].Should().NotBe(sizes[1]);
            sizes[3].Should().NotBe(sizes[1]);
            sizes[3].Should().NotBe(sizes[2]);

            sizes.GetSeparator(1).Should().NotBe(sizes.GetSeparator(0));
            sizes.GetSeparator(2).Should().NotBe(sizes.GetSeparator(0));
            sizes.GetSeparator(2).Should().NotBe(sizes.GetSeparator(1));
        }

        [Fact]
        public void ZeroWidthStructuredTrivia()
        {
            // create zero width structured trivia (not sure how these come about but its not impossible)
            var zeroWidth = SyntaxFactory.ElseDirectiveTrivia(SyntaxFactory.MissingToken(SyntaxKind.HashToken), SyntaxFactory.MissingToken(SyntaxKind.ElseKeyword), SyntaxFactory.MissingToken(SyntaxKind.EndOfDirectiveToken), false, false);
            zeroWidth.Width.Should().Be(0);

            // create token with more than one instance of same zero width structured trivia!
            var someToken = SyntaxFactory.Identifier(default(SyntaxTriviaList), "goo", SyntaxFactory.TriviaList(SyntaxFactory.Trivia(zeroWidth), SyntaxFactory.Trivia(zeroWidth)));

            // create node with this token
            var someNode = SyntaxFactory.IdentifierName(someToken);

            someNode.Identifier.TrailingTrivia.Count.Should().Be(2);
            someNode.Identifier.TrailingTrivia[0].HasStructure.Should().BeTrue();
            someNode.Identifier.TrailingTrivia[1].HasStructure.Should().BeTrue();

            // prove that trivia have different identity
            someNode.Identifier.TrailingTrivia[0].Equals(someNode.Identifier.TrailingTrivia[1]).Should().BeFalse();

            var tt0 = someNode.Identifier.TrailingTrivia[0];
            var tt1 = someNode.Identifier.TrailingTrivia[1];

            var str0 = tt0.GetStructure();
            var str1 = tt1.GetStructure();

            // prove that structures have different identity
            str1.Should().NotBe(str0);

            // prove that structured trivia can get back to original trivia with correct identity
            var tr0 = str0.ParentTrivia;
            tr0.Should().Be(tt0);

            var tr1 = str1.ParentTrivia;
            tr1.Should().Be(tt1);
        }

        [Fact]
        public void ZeroWidthStructuredTriviaOnZeroWidthToken()
        {
            // create zero width structured trivia (not sure how these come about but its not impossible)
            var zeroWidth = SyntaxFactory.ElseDirectiveTrivia(SyntaxFactory.MissingToken(SyntaxKind.HashToken), SyntaxFactory.MissingToken(SyntaxKind.ElseKeyword), SyntaxFactory.MissingToken(SyntaxKind.EndOfDirectiveToken), false, false);
            zeroWidth.Width.Should().Be(0);

            // create token with more than one instance of same zero width structured trivia!
            var someToken = SyntaxFactory.Identifier(default(SyntaxTriviaList), "", SyntaxFactory.TriviaList(SyntaxFactory.Trivia(zeroWidth), SyntaxFactory.Trivia(zeroWidth)));

            // create node with this token
            var someNode = SyntaxFactory.IdentifierName(someToken);

            someNode.Identifier.TrailingTrivia.Count.Should().Be(2);
            someNode.Identifier.TrailingTrivia[0].HasStructure.Should().BeTrue();
            someNode.Identifier.TrailingTrivia[1].HasStructure.Should().BeTrue();

            // prove that trivia have different identity
            someNode.Identifier.TrailingTrivia[0].Equals(someNode.Identifier.TrailingTrivia[1]).Should().BeFalse();

            var tt0 = someNode.Identifier.TrailingTrivia[0];
            var tt1 = someNode.Identifier.TrailingTrivia[1];

            var str0 = tt0.GetStructure();
            var str1 = tt1.GetStructure();

            // prove that structures have different identity
            str1.Should().NotBe(str0);

            // prove that structured trivia can get back to original trivia with correct identity
            var tr0 = str0.ParentTrivia;
            tr0.Should().Be(tt0);

            var tr1 = str1.ParentTrivia;
            tr1.Should().Be(tt1);
        }

        [WorkItem(537059, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/537059")]
        [Fact]
        public void TestIncompleteDeclWithDotToken()
        {
            var tree = SyntaxFactory.ParseSyntaxTree(
@"
class Test
{
  int IX.GOO
");

            // Verify the kind of the CSharpSyntaxNode "int IX.GOO" is MethodDeclaration and NOT FieldDeclaration
            tree.GetCompilationUnitRoot().ChildNodesAndTokens()[0].ChildNodesAndTokens()[3].Kind().Should().Be(SyntaxKind.MethodDeclaration);
        }

        [WorkItem(538360, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/538360")]
        [Fact]
        public void TestGetTokensLanguageAny()
        {
            SyntaxTree syntaxTree = SyntaxFactory.ParseSyntaxTree("class C {}");

            var actualTokens = syntaxTree.GetCompilationUnitRoot().DescendantTokens();

            var expectedTokenKinds = new SyntaxKind[]
            {
                SyntaxKind.ClassKeyword,
                SyntaxKind.IdentifierToken,
                SyntaxKind.OpenBraceToken,
                SyntaxKind.CloseBraceToken,
                SyntaxKind.EndOfFileToken,
            };

            actualTokens.Count().Should().Be(expectedTokenKinds.Count()); //redundant but helps debug
            expectedTokenKinds.SequenceEqual(actualTokens.Select(t => t.Kind())).Should().BeTrue();
        }

        [WorkItem(538360, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/538360")]
        [Fact]
        public void TestGetTokensCommonAny()
        {
            SyntaxTree syntaxTree = SyntaxFactory.ParseSyntaxTree("class C {}");

            var actualTokens = syntaxTree.GetRoot().DescendantTokens(syntaxTree.GetRoot().FullSpan);

            var expectedTokenKinds = new SyntaxKind[]
            {
                SyntaxKind.ClassKeyword,
                SyntaxKind.IdentifierToken,
                SyntaxKind.OpenBraceToken,
                SyntaxKind.CloseBraceToken,
                SyntaxKind.EndOfFileToken,
            };

            actualTokens.Count().Should().Be(expectedTokenKinds.Count()); //redundant but helps debug
            expectedTokenKinds.SequenceEqual(actualTokens.Select(t => (SyntaxKind)t.RawKind)).Should().BeTrue();
        }

        [Fact]
        public void TestGetLocation()
        {
            var tree = SyntaxFactory.ParseSyntaxTree("class C { void F() { } }");
            dynamic root = tree.GetCompilationUnitRoot();
            MethodDeclarationSyntax method = root.Members[0].Members[0];

            var nodeLocation = method.GetLocation();
            nodeLocation.IsInSource.Should().BeTrue();
            nodeLocation.SourceTree.Should().Be(tree);
            nodeLocation.SourceSpan.Should().Be(method.Span);

            var tokenLocation = method.Identifier.GetLocation();
            tokenLocation.IsInSource.Should().BeTrue();
            tokenLocation.SourceTree.Should().Be(tree);
            tokenLocation.SourceSpan.Should().Be(method.Identifier.Span);

            var triviaLocation = method.ReturnType.GetLastToken().TrailingTrivia[0].GetLocation();
            triviaLocation.IsInSource.Should().BeTrue();
            triviaLocation.SourceTree.Should().Be(tree);
            triviaLocation.SourceSpan.Should().Be(method.ReturnType.GetLastToken().TrailingTrivia[0].Span);

            var textSpan = new TextSpan(5, 10);
            var spanLocation = tree.GetLocation(textSpan);
            spanLocation.IsInSource.Should().BeTrue();
            spanLocation.SourceTree.Should().Be(tree);
            spanLocation.SourceSpan.Should().Be(textSpan);
        }

        [Fact]
        public void TestReplaceNode()
        {
            var expr = SyntaxFactory.ParseExpression("a + b");
            var bex = (BinaryExpressionSyntax)expr;
            var expr2 = bex.ReplaceNode(bex.Right, SyntaxFactory.ParseExpression("c"));
            expr2.ToFullString().Should().Be("a + c");
        }

        [Fact]
        public void TestReplaceNodes()
        {
            var expr = SyntaxFactory.ParseExpression("a + b + c + d");

            // replace each expression with a parenthesized expression
            var replaced = expr.ReplaceNodes(
                expr.DescendantNodes().OfType<ExpressionSyntax>(),
                (node, rewritten) => SyntaxFactory.ParenthesizedExpression(rewritten));

            var replacedText = replaced.ToFullString();
            replacedText.Should().Be("(((a )+ (b ))+ (c ))+ (d)");
        }

        [Fact]
        public void TestReplaceNodeInListWithMultiple()
        {
            var invocation = (InvocationExpressionSyntax)SyntaxFactory.ParseExpression("m(a, b)");
            var argC = SyntaxFactory.Argument(SyntaxFactory.ParseExpression("c"));
            var argD = SyntaxFactory.Argument(SyntaxFactory.ParseExpression("d"));

            // replace first with multiple
            var newNode = invocation.ReplaceNode(invocation.ArgumentList.Arguments[0], new SyntaxNode[] { argC, argD });
            newNode.ToFullString().Should().Be("m(c,d, b)");

            // replace last with multiple
            newNode = invocation.ReplaceNode(invocation.ArgumentList.Arguments[1], new SyntaxNode[] { argC, argD });
            newNode.ToFullString().Should().Be("m(a, c,d)");

            // replace first with empty list
            newNode = invocation.ReplaceNode(invocation.ArgumentList.Arguments[0], new SyntaxNode[] { });
            newNode.ToFullString().Should().Be("m(b)");

            // replace last with empty list
            newNode = invocation.ReplaceNode(invocation.ArgumentList.Arguments[1], new SyntaxNode[] { });
            newNode.ToFullString().Should().Be("m(a)");
        }

        [Fact]
        public void TestReplaceNonListNodeWithMultiple()
        {
            var ifstatement = (IfStatementSyntax)SyntaxFactory.ParseStatement("if (a < b) m(c)");
            var then = ifstatement.Statement;

            var stat1 = SyntaxFactory.ParseStatement("m1(x)");
            var stat2 = SyntaxFactory.ParseStatement("m2(y)");

            // you cannot replace a node that is a single node member with multiple nodes
            Assert.Throws<InvalidOperationException>(() => ifstatement.ReplaceNode(then, new[] { stat1, stat2 }));

            // you cannot replace a node that is a single node member with an empty list
            Assert.Throws<InvalidOperationException>(() => ifstatement.ReplaceNode(then, new StatementSyntax[] { }));
        }

        [Fact]
        public void TestInsertNodesInList()
        {
            var invocation = (InvocationExpressionSyntax)SyntaxFactory.ParseExpression("m(a, b)");
            var argC = SyntaxFactory.Argument(SyntaxFactory.ParseExpression("c"));
            var argD = SyntaxFactory.Argument(SyntaxFactory.ParseExpression("d"));

            // insert before first
            var newNode = invocation.InsertNodesBefore(invocation.ArgumentList.Arguments[0], new SyntaxNode[] { argC, argD });
            newNode.ToFullString().Should().Be("m(c,d,a, b)");

            // insert after first
            newNode = invocation.InsertNodesAfter(invocation.ArgumentList.Arguments[0], new SyntaxNode[] { argC, argD });
            newNode.ToFullString().Should().Be("m(a,c,d, b)");

            // insert before last
            newNode = invocation.InsertNodesBefore(invocation.ArgumentList.Arguments[1], new SyntaxNode[] { argC, argD });
            newNode.ToFullString().Should().Be("m(a,c,d, b)");

            // insert after last
            newNode = invocation.InsertNodesAfter(invocation.ArgumentList.Arguments[1], new SyntaxNode[] { argC, argD });
            newNode.ToFullString().Should().Be("m(a, b,c,d)");
        }

        [Fact]
        public void TestInsertNodesRelativeToNonListNode()
        {
            var ifstatement = (IfStatementSyntax)SyntaxFactory.ParseStatement("if (a < b) m(c)");
            var then = ifstatement.Statement;

            var stat1 = SyntaxFactory.ParseStatement("m1(x)");
            var stat2 = SyntaxFactory.ParseStatement("m2(y)");

            // you cannot insert nodes before/after a node that is not part of a list
            Assert.Throws<InvalidOperationException>(() => ifstatement.InsertNodesBefore(then, new[] { stat1, stat2 }));

            // you cannot insert nodes before/after a node that is not part of a list
            Assert.Throws<InvalidOperationException>(() => ifstatement.InsertNodesAfter(then, new StatementSyntax[] { }));
        }

        [Fact]
        public void TestReplaceStatementInListWithMultiple()
        {
            var block = (BlockSyntax)SyntaxFactory.ParseStatement("{ var x = 10; var y = 20; }");
            var stmt1 = SyntaxFactory.ParseStatement("var z = 30; ");
            var stmt2 = SyntaxFactory.ParseStatement("var q = 40; ");

            // replace first with multiple
            var newBlock = block.ReplaceNode(block.Statements[0], new[] { stmt1, stmt2 });
            newBlock.ToFullString().Should().Be("{ var z = 30; var q = 40; var y = 20; }");

            // replace second with multiple
            newBlock = block.ReplaceNode(block.Statements[1], new[] { stmt1, stmt2 });
            newBlock.ToFullString().Should().Be("{ var x = 10; var z = 30; var q = 40; }");

            // replace first with empty list
            newBlock = block.ReplaceNode(block.Statements[0], new SyntaxNode[] { });
            newBlock.ToFullString().Should().Be("{ var y = 20; }");

            // replace second with empty list
            newBlock = block.ReplaceNode(block.Statements[1], new SyntaxNode[] { });
            newBlock.ToFullString().Should().Be("{ var x = 10; }");
        }

        [Fact]
        public void TestInsertStatementsInList()
        {
            var block = (BlockSyntax)SyntaxFactory.ParseStatement("{ var x = 10; var y = 20; }");
            var stmt1 = SyntaxFactory.ParseStatement("var z = 30; ");
            var stmt2 = SyntaxFactory.ParseStatement("var q = 40; ");

            // insert before first
            var newBlock = block.InsertNodesBefore(block.Statements[0], new[] { stmt1, stmt2 });
            newBlock.ToFullString().Should().Be("{ var z = 30; var q = 40; var x = 10; var y = 20; }");

            // insert after first
            newBlock = block.InsertNodesAfter(block.Statements[0], new[] { stmt1, stmt2 });
            newBlock.ToFullString().Should().Be("{ var x = 10; var z = 30; var q = 40; var y = 20; }");

            // insert before last
            newBlock = block.InsertNodesBefore(block.Statements[1], new[] { stmt1, stmt2 });
            newBlock.ToFullString().Should().Be("{ var x = 10; var z = 30; var q = 40; var y = 20; }");

            // insert after last
            newBlock = block.InsertNodesAfter(block.Statements[1], new[] { stmt1, stmt2 });
            newBlock.ToFullString().Should().Be("{ var x = 10; var y = 20; var z = 30; var q = 40; }");
        }

        [Fact]
        public void TestReplaceSingleToken()
        {
            var expr = SyntaxFactory.ParseExpression("a + b");
            var bToken = expr.DescendantTokens().First(t => t.Text == "b");
            var expr2 = expr.ReplaceToken(bToken, SyntaxFactory.ParseToken("c"));
            expr2.ToString().Should().Be("a + c");
        }

        [Fact]
        public void TestReplaceMultipleTokens()
        {
            var expr = SyntaxFactory.ParseExpression("a + b + c");
            var d = SyntaxFactory.ParseToken("d ");
            var tokens = expr.DescendantTokens().Where(t => t.IsKind(SyntaxKind.IdentifierToken)).ToList();
            var replaced = expr.ReplaceTokens(tokens, (tok, tok2) => d);
            replaced.ToFullString().Should().Be("d + d + d ");
        }

        [Fact]
        public void TestReplaceSingleTokenWithMultipleTokens()
        {
            var cu = SyntaxFactory.ParseCompilationUnit("private class C { }");
            var privateToken = ((ClassDeclarationSyntax)cu.Members[0]).Modifiers[0];
            var publicToken = SyntaxFactory.ParseToken("public ");
            var partialToken = SyntaxFactory.ParseToken("partial ");

            var cu1 = cu.ReplaceToken(privateToken, publicToken);
            cu1.ToFullString().Should().Be("public class C { }");

            var cu2 = cu.ReplaceToken(privateToken, new[] { publicToken, partialToken });
            cu2.ToFullString().Should().Be("public partial class C { }");

            var cu3 = cu.ReplaceToken(privateToken, new SyntaxToken[] { });
            cu3.ToFullString().Should().Be("class C { }");
        }

        [Fact]
        public void TestReplaceNonListTokenWithMultipleTokensFails()
        {
            var cu = SyntaxFactory.ParseCompilationUnit("private class C { }");
            var identifierC = cu.DescendantTokens().First(t => t.Text == "C");

            var identifierA = SyntaxFactory.ParseToken("A");
            var identifierB = SyntaxFactory.ParseToken("B");

            // you cannot replace a token that is a single token member with multiple tokens
            Assert.Throws<InvalidOperationException>(() => cu.ReplaceToken(identifierC, new[] { identifierA, identifierB }));

            // you cannot replace a token that is a single token member with an empty list of tokens
            Assert.Throws<InvalidOperationException>(() => cu.ReplaceToken(identifierC, new SyntaxToken[] { }));
        }

        [Fact]
        public void TestInsertTokens()
        {
            var cu = SyntaxFactory.ParseCompilationUnit("public class C { }");
            var publicToken = ((ClassDeclarationSyntax)cu.Members[0]).Modifiers[0];
            var partialToken = SyntaxFactory.ParseToken("partial ");
            var staticToken = SyntaxFactory.ParseToken("static ");

            var cu1 = cu.InsertTokensBefore(publicToken, new[] { staticToken });
            cu1.ToFullString().Should().Be("static public class C { }");

            var cu2 = cu.InsertTokensAfter(publicToken, new[] { staticToken });
            cu2.ToFullString().Should().Be("public static class C { }");
        }

        [Fact]
        public void TestInsertTokensRelativeToNonListToken()
        {
            var cu = SyntaxFactory.ParseCompilationUnit("public class C { }");
            var identifierC = cu.DescendantTokens().First(t => t.Text == "C");

            var identifierA = SyntaxFactory.ParseToken("A");
            var identifierB = SyntaxFactory.ParseToken("B");

            // you cannot insert a token before/after a token that is not part of a list of tokens
            Assert.Throws<InvalidOperationException>(() => cu.InsertTokensBefore(identifierC, new[] { identifierA, identifierB }));

            // you cannot insert a token before/after a token that is not part of a list of tokens
            Assert.Throws<InvalidOperationException>(() => cu.InsertTokensAfter(identifierC, new[] { identifierA, identifierB }));
        }

        [Fact]
        public void ReplaceMissingToken()
        {
            var text = "return x";
            var expr = SyntaxFactory.ParseStatement(text);

            var token = expr.DescendantTokens().First(t => t.IsMissing);

            var expr2 = expr.ReplaceToken(token, SyntaxFactory.Token(token.Kind()));
            var text2 = expr2.ToFullString();

            text2.Should().Be("return x;");
        }

        [Fact]
        public void ReplaceEndOfCommentToken()
        {
            var text = "/// Goo\r\n return x;";
            var expr = SyntaxFactory.ParseStatement(text);

            var tokens = expr.DescendantTokens(descendIntoTrivia: true).ToList();
            var token = tokens.First(t => t.Kind() == SyntaxKind.EndOfDocumentationCommentToken);

            var expr2 = expr.ReplaceToken(token, SyntaxFactory.Token(SyntaxTriviaList.Create(SyntaxFactory.Whitespace("garbage")), token.Kind(), default(SyntaxTriviaList)));
            var text2 = expr2.ToFullString();

            text2.Should().Be("/// Goo\r\ngarbage return x;");
        }

        [Fact]
        public void ReplaceEndOfFileToken()
        {
            var text = "";
            var cu = SyntaxFactory.ParseCompilationUnit(text);
            var token = cu.DescendantTokens().Single(t => t.Kind() == SyntaxKind.EndOfFileToken);

            var cu2 = cu.ReplaceToken(token, SyntaxFactory.Token(SyntaxTriviaList.Create(SyntaxFactory.Whitespace("  ")), token.Kind(), default(SyntaxTriviaList)));
            var text2 = cu2.ToFullString();

            text2.Should().Be("  ");
        }

        [Fact]
        public void TestReplaceTriviaDeep()
        {
            var expr = SyntaxFactory.ParseExpression("#if true\r\na + \r\n#endif\r\n + b");

            // get whitespace trivia inside structured directive trivia
            var deepTrivia = expr.GetDirectives().SelectMany(d => d.DescendantTrivia().Where(tr => tr.Kind() == SyntaxKind.WhitespaceTrivia)).ToList();

            // replace deep trivia with double-whitespace trivia
            var twoSpace = SyntaxFactory.Whitespace("  ");
            var expr2 = expr.ReplaceTrivia(deepTrivia, (tr, tr2) => twoSpace);

            expr2.ToFullString().Should().Be("#if  true\r\na + \r\n#endif\r\n + b");
        }

        [Fact]
        public void TestReplaceSingleTriviaInNode()
        {
            var expr = SyntaxFactory.ParseExpression("a + b");
            var trivia = expr.DescendantTokens().First(t => t.Text == "a").TrailingTrivia[0];
            var twoSpaces = SyntaxFactory.Whitespace("  ");
            var expr2 = expr.ReplaceTrivia(trivia, twoSpaces);
            expr2.ToFullString().Should().Be("a  + b");
        }

        [Fact]
        public void TestReplaceMultipleTriviaInNode()
        {
            var expr = SyntaxFactory.ParseExpression("a + b");
            var twoSpaces = SyntaxFactory.Whitespace("  ");
            var trivia = expr.DescendantTrivia().Where(tr => tr.IsKind(SyntaxKind.WhitespaceTrivia)).ToList();
            var replaced = expr.ReplaceTrivia(trivia, (tr, tr2) => twoSpaces);
            replaced.ToFullString().Should().Be("a  +  b");
        }

        [Fact]
        public void TestReplaceSingleTriviaWithMultipleTriviaInNode()
        {
            var ex = SyntaxFactory.ParseExpression("/* c */ identifier");
            var leadingTrivia = ex.GetLeadingTrivia();
            leadingTrivia.Count.Should().Be(2);
            var comment1 = leadingTrivia[0];
            comment1.Kind().Should().Be(SyntaxKind.MultiLineCommentTrivia);

            var newComment1 = SyntaxFactory.ParseLeadingTrivia("/* a */")[0];
            var newComment2 = SyntaxFactory.ParseLeadingTrivia("/* b */")[0];

            var ex1 = ex.ReplaceTrivia(comment1, newComment1);
            ex1.ToFullString().Should().Be("/* a */ identifier");

            var ex2 = ex.ReplaceTrivia(comment1, new[] { newComment1, newComment2 });
            ex2.ToFullString().Should().Be("/* a *//* b */ identifier");

            var ex3 = ex.ReplaceTrivia(comment1, new SyntaxTrivia[] { });
            ex3.ToFullString().Should().Be(" identifier");
        }

        [Fact]
        public void TestInsertTriviaInNode()
        {
            var ex = SyntaxFactory.ParseExpression("/* c */ identifier");
            var leadingTrivia = ex.GetLeadingTrivia();
            leadingTrivia.Count.Should().Be(2);
            var comment1 = leadingTrivia[0];
            comment1.Kind().Should().Be(SyntaxKind.MultiLineCommentTrivia);

            var newComment1 = SyntaxFactory.ParseLeadingTrivia("/* a */")[0];
            var newComment2 = SyntaxFactory.ParseLeadingTrivia("/* b */")[0];

            var ex1 = ex.InsertTriviaBefore(comment1, new[] { newComment1, newComment2 });
            ex1.ToFullString().Should().Be("/* a *//* b *//* c */ identifier");

            var ex2 = ex.InsertTriviaAfter(comment1, new[] { newComment1, newComment2 });
            ex2.ToFullString().Should().Be("/* c *//* a *//* b */ identifier");
        }

        [Fact]
        public void TestReplaceSingleTriviaInToken()
        {
            var id = SyntaxFactory.ParseToken("a ");
            var trivia = id.TrailingTrivia[0];
            var twoSpace = SyntaxFactory.Whitespace("  ");
            var id2 = id.ReplaceTrivia(trivia, twoSpace);
            id2.ToFullString().Should().Be("a  ");
        }

        [Fact]
        public void TestReplaceMultipleTriviaInToken()
        {
            var id = SyntaxFactory.ParseToken("a // goo\r\n");

            // replace each trivia with a single space
            var id2 = id.ReplaceTrivia(id.GetAllTrivia(), (tr, tr2) => SyntaxFactory.Space);

            // should be 3 spaces (one for original space, comment and end-of-line)
            id2.ToFullString().Should().Be("a   ");
        }

        [Fact]
        public void TestRemoveNodeInSeparatedList_KeepExteriorTrivia()
        {
            var expr = SyntaxFactory.ParseExpression("m(a, b, /* trivia */ c)");

            var b = expr.DescendantTokens().Where(t => t.Text == "b").Select(t => t.Parent.FirstAncestorOrSelf<ArgumentSyntax>()).FirstOrDefault();
            b.Should().NotBeNull();

            var expr2 = expr.RemoveNode(b, SyntaxRemoveOptions.KeepExteriorTrivia);

            var text = expr2.ToFullString();
            text.Should().Be("m(a , /* trivia */ c)");
        }

        [Fact]
        public void TestRemoveNodeInSeparatedList_KeepExteriorTrivia_2()
        {
            var expr = SyntaxFactory.ParseExpression(@"m(a, b, /* trivia */
c)");

            var b = expr.DescendantTokens().Where(t => t.Text == "b").Select(t => t.Parent.FirstAncestorOrSelf<ArgumentSyntax>()).FirstOrDefault();
            b.Should().NotBeNull();

            var expr2 = expr.RemoveNode(b, SyntaxRemoveOptions.KeepExteriorTrivia);

            var text = expr2.ToFullString();
            text.Should().Be(@"m(a,  /* trivia */
c)");
        }

        [Fact]
        public void TestRemoveNodeInSeparatedList_KeepExteriorTrivia_3()
        {
            var expr = SyntaxFactory.ParseExpression(@"m(a, b,
/* trivia */ c)");

            var b = expr.DescendantTokens().Where(t => t.Text == "b").Select(t => t.Parent.FirstAncestorOrSelf<ArgumentSyntax>()).FirstOrDefault();
            b.Should().NotBeNull();

            var expr2 = expr.RemoveNode(b, SyntaxRemoveOptions.KeepExteriorTrivia);

            var text = expr2.ToFullString();
            text.Should().Be(@"m(a, 
/* trivia */ c)");
        }

        [Fact]
        public void TestRemoveNodeInSeparatedList_KeepExteriorTrivia_4()
        {
            var expr = SyntaxFactory.ParseExpression(@"SomeMethod(/*arg1:*/ a,
    /*arg2:*/ b,
    /*arg3:*/ c)");

            var b = expr.DescendantTokens().Where(t => t.Text == "b").Select(t => t.Parent.FirstAncestorOrSelf<ArgumentSyntax>()).FirstOrDefault();
            b.Should().NotBeNull();

            var expr2 = expr.RemoveNode(b, SyntaxRemoveOptions.KeepExteriorTrivia);

            var text = expr2.ToFullString();
            text.Should().Be(@"SomeMethod(/*arg1:*/ a,
    /*arg2:*/ 
    /*arg3:*/ c)");
        }

        [Fact]
        public void TestRemoveNodeInSeparatedList_KeepExteriorTrivia_5()
        {
            var expr = SyntaxFactory.ParseExpression(@"SomeMethod(// comment about a
           a,
           // some comment about b
           b,
           // some comment about c
           c)");

            var b = expr.DescendantTokens().Where(t => t.Text == "b").Select(t => t.Parent.FirstAncestorOrSelf<ArgumentSyntax>()).FirstOrDefault();
            b.Should().NotBeNull();

            var expr2 = expr.RemoveNode(b, SyntaxRemoveOptions.KeepExteriorTrivia);

            var text = expr2.ToFullString();
            text.Should().Be(@"SomeMethod(// comment about a
           a,
           // some comment about b
           
           // some comment about c
           c)");
        }

        [Fact]
        public void TestRemoveNodeInSeparatedList_KeepNoTrivia()
        {
            var expr = SyntaxFactory.ParseExpression("m(a, b, /* trivia */ c)");

            var b = expr.DescendantTokens().Where(t => t.Text == "b").Select(t => t.Parent.FirstAncestorOrSelf<ArgumentSyntax>()).FirstOrDefault();
            b.Should().NotBeNull();

            var expr2 = expr.RemoveNode(b, SyntaxRemoveOptions.KeepNoTrivia);

            var text = expr2.ToFullString();
            text.Should().Be("m(a, /* trivia */ c)");
        }

        [Fact]
        public void TestRemoveNodeInSeparatedList_KeepNoTrivia_2()
        {
            var expr = SyntaxFactory.ParseExpression(
                @"m(a, b, /* trivia */ 
c)");

            var b = expr.DescendantTokens().Where(t => t.Text == "b").Select(t => t.Parent.FirstAncestorOrSelf<ArgumentSyntax>()).FirstOrDefault();
            b.Should().NotBeNull();

            var expr2 = expr.RemoveNode(b, SyntaxRemoveOptions.KeepNoTrivia);

            var text = expr2.ToFullString();
            text.Should().Be(@"m(a, c)");
        }

        [Fact]
        public void TestRemoveNodeInSeparatedList_KeepNoTrivia_3()
        {
            var expr = SyntaxFactory.ParseExpression(
                @"m(a, b,
/* trivia */ c)");

            var b = expr.DescendantTokens().Where(t => t.Text == "b").Select(t => t.Parent.FirstAncestorOrSelf<ArgumentSyntax>()).FirstOrDefault();
            b.Should().NotBeNull();

            var expr2 = expr.RemoveNode(b, SyntaxRemoveOptions.KeepNoTrivia);

            var text = expr2.ToFullString();
            text.Should().Be(@"m(a, /* trivia */ c)");
        }

        [Fact]
        public void TestRemoveNodeInSeparatedList_KeepNoTrivia_4()
        {
            var expr = SyntaxFactory.ParseExpression(@"SomeMethod(/*arg1:*/ a,
    /*arg2:*/ b,
    /*arg3:*/ c)");

            var b = expr.DescendantTokens().Where(t => t.Text == "b").Select(t => t.Parent.FirstAncestorOrSelf<ArgumentSyntax>()).FirstOrDefault();
            b.Should().NotBeNull();

            var expr2 = expr.RemoveNode(b, SyntaxRemoveOptions.KeepNoTrivia);

            var text = expr2.ToFullString();
            text.Should().Be(@"SomeMethod(/*arg1:*/ a,
    /*arg3:*/ c)");
        }

        [Fact]
        public void TestRemoveNodeInSeparatedList_KeepNoTrivia_5()
        {
            var expr = SyntaxFactory.ParseExpression(@"SomeMethod(// comment about a
           a,
           // some comment about b
           b,
           // some comment about c
           c)");

            var b = expr.DescendantTokens().Where(t => t.Text == "b").Select(t => t.Parent.FirstAncestorOrSelf<ArgumentSyntax>()).FirstOrDefault();
            b.Should().NotBeNull();

            var expr2 = expr.RemoveNode(b, SyntaxRemoveOptions.KeepNoTrivia);

            var text = expr2.ToFullString();
            text.Should().Be(@"SomeMethod(// comment about a
           a,
           // some comment about c
           c)");
        }

        [Fact]
        public void TestRemoveOnlyNodeInSeparatedList_KeepExteriorTrivia()
        {
            var expr = SyntaxFactory.ParseExpression("m(/* before */ a /* after */)");

            var n = expr.DescendantTokens().Where(t => t.Text == "a").Select(t => t.Parent.FirstAncestorOrSelf<ArgumentSyntax>()).FirstOrDefault();
            n.Should().NotBeNull();

            var expr2 = expr.RemoveNode(n, SyntaxRemoveOptions.KeepExteriorTrivia);

            var text = expr2.ToFullString();
            text.Should().Be("m(/* before */  /* after */)");
        }

        [Fact]
        public void TestRemoveFirstNodeInSeparatedList_KeepExteriorTrivia()
        {
            var expr = SyntaxFactory.ParseExpression("m(/* before */ a /* after */, b, c)");

            var n = expr.DescendantTokens().Where(t => t.Text == "a").Select(t => t.Parent.FirstAncestorOrSelf<ArgumentSyntax>()).FirstOrDefault();
            n.Should().NotBeNull();

            var expr2 = expr.RemoveNode(n, SyntaxRemoveOptions.KeepExteriorTrivia);

            var text = expr2.ToFullString();
            text.Should().Be("m(/* before */  /* after */ b, c)");
        }

        [Fact]
        public void TestRemoveLastNodeInSeparatedList_KeepExteriorTrivia()
        {
            var expr = SyntaxFactory.ParseExpression("m(a, b, /* before */ c /* after */)");

            var n = expr.DescendantTokens().Where(t => t.Text == "c").Select(t => t.Parent.FirstAncestorOrSelf<ArgumentSyntax>()).FirstOrDefault();
            n.Should().NotBeNull();

            var expr2 = expr.RemoveNode(n, SyntaxRemoveOptions.KeepExteriorTrivia);

            var text = expr2.ToFullString();
            text.Should().Be("m(a, b /* before */  /* after */)");
        }

        [Fact]
        public void TestRemoveNode_KeepNoTrivia()
        {
            var expr = SyntaxFactory.ParseStatement("{ a; b; /* trivia */ c }");

            var b = expr.DescendantTokens().Where(t => t.Text == "b").Select(t => t.Parent.FirstAncestorOrSelf<StatementSyntax>()).FirstOrDefault();
            b.Should().NotBeNull();

            var expr2 = expr.RemoveNode(b, SyntaxRemoveOptions.KeepNoTrivia);

            var text = expr2.ToFullString();
            text.Should().Be("{ a; c }");
        }

        [Fact]
        public void TestRemoveNode_KeepExteriorTrivia()
        {
            var expr = SyntaxFactory.ParseStatement("{ a; b; /* trivia */ c }");

            var b = expr.DescendantTokens().Where(t => t.Text == "b").Select(t => t.Parent.FirstAncestorOrSelf<StatementSyntax>()).FirstOrDefault();
            b.Should().NotBeNull();

            var expr2 = expr.RemoveNode(b, SyntaxRemoveOptions.KeepExteriorTrivia);

            var text = expr2.ToFullString();
            text.Should().Be("{ a;  /* trivia */ c }");
        }

        [Fact]
        public void TestRemoveLastNode_KeepExteriorTrivia()
        {
            // this tests removing the last node in a non-terminal such that there is no token to the right of the removed
            // node to attach the kept trivia too.  The trivia must be attached to the previous token.

            var cu = SyntaxFactory.ParseCompilationUnit("class C { void M() { } /* trivia */ }");

            var m = cu.DescendantNodes().OfType<MethodDeclarationSyntax>().FirstOrDefault();
            m.Should().NotBeNull();

            // remove the body block from the method syntax (since it can be set to null)
            var m2 = m.RemoveNode(m.Body, SyntaxRemoveOptions.KeepExteriorTrivia);

            var text = m2.ToFullString();

            text.Should().Be("void M()  /* trivia */ ");
        }

        [Fact]
        public void TestRemove_KeepExteriorTrivia_KeepUnbalancedDirectives()
        {
            var cu = SyntaxFactory.ParseCompilationUnit(@"
class C
{
// before
void M()
{
#region Fred
} // after
#endregion
}");

            var expectedText = @"
class C
{
// before
#region Fred
 // after
#endregion
}";

            var m = cu.DescendantNodes().OfType<MethodDeclarationSyntax>().FirstOrDefault();
            m.Should().NotBeNull();

            var cu2 = cu.RemoveNode(m, SyntaxRemoveOptions.KeepExteriorTrivia | SyntaxRemoveOptions.KeepUnbalancedDirectives);

            var text = cu2.ToFullString();

            text.Should().Be(expectedText);
        }

        [Fact]
        public void TestRemove_KeepUnbalancedDirectives()
        {
            var inputText = @"
class C
{
// before
#region Fred
// more before
void M()
{
} // after
#endregion
}";

            var expectedText = @"
class C
{

#region Fred
#endregion
}";

            TestWithWindowsAndUnixEndOfLines(inputText, expectedText, (cu, expected) =>
            {
                var m = cu.DescendantNodes().OfType<MethodDeclarationSyntax>().FirstOrDefault();
                m.Should().NotBeNull();

                var cu2 = cu.RemoveNode(m, SyntaxRemoveOptions.KeepUnbalancedDirectives);

                var text = cu2.ToFullString();

                text.Should().Be(expected);
            });
        }

        [Fact]
        public void TestRemove_KeepDirectives()
        {
            var inputText = @"
class C
{
// before
#region Fred
// more before
void M()
{
#if true
#endif
} // after
#endregion
}";

            var expectedText = @"
class C
{

#region Fred
#if true
#endif
#endregion
}";

            TestWithWindowsAndUnixEndOfLines(inputText, expectedText, (cu, expected) =>
            {
                var m = cu.DescendantNodes().OfType<MethodDeclarationSyntax>().FirstOrDefault();
                m.Should().NotBeNull();

                var cu2 = cu.RemoveNode(m, SyntaxRemoveOptions.KeepDirectives);

                var text = cu2.ToFullString();

                text.Should().Be(expected);
            });
        }

        [Fact]
        [WorkItem(22924, "https://github.com/dotnet/roslyn/issues/22924")]
        public void TestRemove_KeepEndOfLine()
        {
            var inputText = @"
class C
{
// before
void M()
{
} // after
}";

            var expectedText = @"
class C
{

}";

            TestWithWindowsAndUnixEndOfLines(inputText, expectedText, (cu, expected) =>
            {
                var m = cu.DescendantNodes().OfType<MethodDeclarationSyntax>().FirstOrDefault();
                m.Should().NotBeNull();

                var cu2 = cu.RemoveNode(m, SyntaxRemoveOptions.KeepEndOfLine);

                var text = cu2.ToFullString();

                text.Should().Be(expected);
            });
        }

        [Fact]
        [WorkItem(22924, "https://github.com/dotnet/roslyn/issues/22924")]
        public void TestRemoveWithoutEOL_KeepEndOfLine()
        {
            var cu = SyntaxFactory.ParseCompilationUnit(@"class A { } class B { } // test");

            var m = cu.DescendantNodes().OfType<TypeDeclarationSyntax>().LastOrDefault();
            m.Should().NotBeNull();

            var cu2 = cu.RemoveNode(m, SyntaxRemoveOptions.KeepEndOfLine);

            var text = cu2.ToFullString();

            text.Should().Be("class A { } ");
        }

        [Fact]
        [WorkItem(22924, "https://github.com/dotnet/roslyn/issues/22924")]
        public void TestRemoveBadDirectiveWithoutEOL_KeepEndOfLine_KeepDirectives()
        {
            var cu = SyntaxFactory.ParseCompilationUnit(@"class A { } class B { } #endregion");

            var m = cu.DescendantNodes().OfType<TypeDeclarationSyntax>().LastOrDefault();
            m.Should().NotBeNull();

            var cu2 = cu.RemoveNode(m, SyntaxRemoveOptions.KeepEndOfLine | SyntaxRemoveOptions.KeepDirectives);

            var text = cu2.ToFullString();

            text.Should().Be("class A { } \r\n#endregion");
        }

        [Fact]
        [WorkItem(22924, "https://github.com/dotnet/roslyn/issues/22924")]
        public void TestRemoveDocument_KeepEndOfLine()
        {
            var cu = SyntaxFactory.ParseCompilationUnit(@"
#region A
class A 
{ } 
#endregion");

            var cu2 = cu.RemoveNode(cu, SyntaxRemoveOptions.KeepEndOfLine);

            cu2.Should().BeNull();
        }

        [Fact]
        [WorkItem(22924, "https://github.com/dotnet/roslyn/issues/22924")]
        public void TestRemoveFirstParameterEOLCommaTokenTrailingTrivia_KeepEndOfLine()
        {
            // EOL should be found on CommaToken TrailingTrivia
            var inputText = @"
class C
{
void M(
// before a
int a,
// after a
// before b
int b
/* after b*/)
{
}
}";

            var expectedText = @"
class C
{
void M(

// after a
// before b
int b
/* after b*/)
{
}
}";

            TestWithWindowsAndUnixEndOfLines(inputText, expectedText, (cu, expected) =>
            {
                var m = cu.DescendantNodes().OfType<ParameterSyntax>().FirstOrDefault();
                m.Should().NotBeNull();

                var cu2 = cu.RemoveNode(m, SyntaxRemoveOptions.KeepEndOfLine);

                var text = cu2.ToFullString();

                text.Should().Be(expected);
            });
        }

        [Fact]
        [WorkItem(22924, "https://github.com/dotnet/roslyn/issues/22924")]
        public void TestRemoveFirstParameterEOLParameterSyntaxTrailingTrivia_KeepEndOfLine()
        {
            // EOL should be found on ParameterSyntax TrailingTrivia
            var inputText = @"
class C
{
void M(
// before a
int a
, /* after comma */ int b
/* after b*/)
{
}
}";

            var expectedText = @"
class C
{
void M(

int b
/* after b*/)
{
}
}";

            TestWithWindowsAndUnixEndOfLines(inputText, expectedText, (cu, expected) =>
            {
                var m = cu.DescendantNodes().OfType<ParameterSyntax>().FirstOrDefault();
                m.Should().NotBeNull();

                var cu2 = cu.RemoveNode(m, SyntaxRemoveOptions.KeepEndOfLine);

                var text = cu2.ToFullString();

                text.Should().Be(expected);
            });
        }

        [Fact]
        [WorkItem(22924, "https://github.com/dotnet/roslyn/issues/22924")]
        public void TestRemoveFirstParameterEOLCommaTokenLeadingTrivia_KeepEndOfLine()
        {
            // EOL should be found on CommaToken LeadingTrivia and also on ParameterSyntax TrailingTrivia
            // but only one will be added
            var inputText = @"
class C
{
void M(
// before a
int a

// before b
, /* after comma */ int b
/* after b*/)
{
}
}";

            var expectedText = @"
class C
{
void M(

int b
/* after b*/)
{
}
}";

            TestWithWindowsAndUnixEndOfLines(inputText, expectedText, (cu, expected) =>
            {
                var m = cu.DescendantNodes().OfType<ParameterSyntax>().FirstOrDefault();
                m.Should().NotBeNull();

                var cu2 = cu.RemoveNode(m, SyntaxRemoveOptions.KeepEndOfLine);

                var text = cu2.ToFullString();

                text.Should().Be(expected);
            });
        }

        [Fact]
        [WorkItem(22924, "https://github.com/dotnet/roslyn/issues/22924")]
        public void TestRemoveFirstParameter_KeepTrailingTrivia()
        {
            var cu = SyntaxFactory.ParseCompilationUnit(@"
class C
{
void M(
// before a
int a

// before b
, /* after comma */ int b
/* after b*/)
{
}
}");

            var expectedText = @"
class C
{
void M(


// before b
 /* after comma */ int b
/* after b*/)
{
}
}";

            var m = cu.DescendantNodes().OfType<ParameterSyntax>().FirstOrDefault();
            m.Should().NotBeNull();

            var cu2 = cu.RemoveNode(m, SyntaxRemoveOptions.KeepTrailingTrivia);

            var text = cu2.ToFullString();

            text.Should().Be(expectedText);
        }

        [Fact]
        [WorkItem(22924, "https://github.com/dotnet/roslyn/issues/22924")]
        public void TestRemoveLastParameterEOLCommaTokenLeadingTrivia_KeepEndOfLine()
        {
            // EOL should be found on CommaToken LeadingTrivia
            var inputText = @"
class C
{
void M(
// before a
int a

// after a
, /* after comma*/ int b /* after b*/)
{
}
}";

            var expectedText = @"
class C
{
void M(
// before a
int a

)
{
}
}";

            TestWithWindowsAndUnixEndOfLines(inputText, expectedText, (cu, expected) =>
            {
                var m = cu.DescendantNodes().OfType<ParameterSyntax>().LastOrDefault();
                m.Should().NotBeNull();

                var cu2 = cu.RemoveNode(m, SyntaxRemoveOptions.KeepEndOfLine);

                var text = cu2.ToFullString();

                text.Should().Be(expected);
            });
        }

        [Fact]
        [WorkItem(22924, "https://github.com/dotnet/roslyn/issues/22924")]
        public void TestRemoveLastParameterEOLCommaTokenTrailingTrivia_KeepEndOfLine()
        {
            // EOL should be found on CommaToken TrailingTrivia
            var inputText = @"
class C
{
void M(
// before a
int a, /* after comma*/ 
int b /* after b*/)
{
}
}";

            var expectedText = @"
class C
{
void M(
// before a
int a
)
{
}
}";

            TestWithWindowsAndUnixEndOfLines(inputText, expectedText, (cu, expected) =>
            {
                var m = cu.DescendantNodes().OfType<ParameterSyntax>().LastOrDefault();
                m.Should().NotBeNull();

                var cu2 = cu.RemoveNode(m, SyntaxRemoveOptions.KeepEndOfLine);

                var text = cu2.ToFullString();

                text.Should().Be(expected);
            });
        }

        [Fact]
        [WorkItem(22924, "https://github.com/dotnet/roslyn/issues/22924")]
        public void TestRemoveLastParameterEOLParameterSyntaxLeadingTrivia_KeepEndOfLine()
        {
            // EOL should be found on ParameterSyntax LeadingTrivia and also on CommaToken TrailingTrivia
            // but only one will be added
            var inputText = @"
class C
{
void M(
// before a
int a, /* after comma */ 

// before b
int b /* after b*/)
{
}
}";

            var expectedText = @"
class C
{
void M(
// before a
int a
)
{
}
}";

            TestWithWindowsAndUnixEndOfLines(inputText, expectedText, (cu, expected) =>
            {
                var m = cu.DescendantNodes().OfType<ParameterSyntax>().LastOrDefault();
                m.Should().NotBeNull();

                var cu2 = cu.RemoveNode(m, SyntaxRemoveOptions.KeepEndOfLine);

                var text = cu2.ToFullString();

                text.Should().Be(expected);
            });
        }

        [Fact]
        [WorkItem(22924, "https://github.com/dotnet/roslyn/issues/22924")]
        public void TestRemoveLastParameter_KeepLeadingTrivia()
        {
            var cu = SyntaxFactory.ParseCompilationUnit(@"
class C
{
void M(
// before a
int a, /* after comma */ 

// before b
int b /* after b*/)
{
}
}");

            var expectedText = @"
class C
{
void M(
// before a
int a /* after comma */ 

// before b
)
{
}
}";

            var m = cu.DescendantNodes().OfType<ParameterSyntax>().LastOrDefault();
            m.Should().NotBeNull();

            var cu2 = cu.RemoveNode(m, SyntaxRemoveOptions.KeepLeadingTrivia);

            var text = cu2.ToFullString();

            text.Should().Be(expectedText);
        }

        [Fact]
        [WorkItem(22924, "https://github.com/dotnet/roslyn/issues/22924")]
        public void TestRemoveClassWithEndRegionDirectiveWithoutEOL_KeepEndOfLine_KeepDirectives()
        {
            var inputText = @"
#region A
class A { } #endregion";

            var expectedText = @"
#region A
#endregion";

            TestWithWindowsAndUnixEndOfLines(inputText, expectedText, (cu, expected) =>
            {
                var m = cu.DescendantNodes().OfType<TypeDeclarationSyntax>().FirstOrDefault();
                m.Should().NotBeNull();

                var cu2 = cu.RemoveNode(m, SyntaxRemoveOptions.KeepEndOfLine | SyntaxRemoveOptions.KeepDirectives);

                var text = cu2.ToFullString();

                text.Should().Be(expected);
            });
        }

        [Fact]
        public void SeparatorsOfSeparatedSyntaxLists()
        {
            var s1 = "int goo(int a, int b, int c) {}";
            var tree = SyntaxFactory.ParseSyntaxTree(s1);

            var root = tree.GetCompilationUnitRoot();
            var method = (LocalFunctionStatementSyntax)((GlobalStatementSyntax)root.Members[0]).Statement;

            var list = (SeparatedSyntaxList<ParameterSyntax>)method.ParameterList.Parameters;

            ((SyntaxToken)list.GetSeparator(0)).Kind().Should().Be(SyntaxKind.CommaToken);
            ((SyntaxToken)list.GetSeparator(1)).Kind().Should().Be(SyntaxKind.CommaToken);

            foreach (var index in new int[] { -1, 2 })
            {
                bool exceptionThrown = false;
                try
                {
                    var unused = list.GetSeparator(2);
                }
                catch (ArgumentOutOfRangeException)
                {
                    exceptionThrown = true;
                }
                exceptionThrown.Should().BeTrue();
            }

            var internalParameterList = (InternalSyntax.ParameterListSyntax)method.ParameterList.Green;
            var internalParameters = internalParameterList.Parameters;

            internalParameters.SeparatorCount.Should().Be(2);
            (new SyntaxToken(internalParameters.GetSeparator(0))).Kind().Should().Be(SyntaxKind.CommaToken);
            (new SyntaxToken(internalParameters.GetSeparator(1))).Kind().Should().Be(SyntaxKind.CommaToken);

            internalParameters.Count.Should().Be(3);
            internalParameters[0].Identifier.ValueText.Should().Be("a");
            internalParameters[1].Identifier.ValueText.Should().Be("b");
            internalParameters[2].Identifier.ValueText.Should().Be("c");
        }

        [Fact]
        public void ThrowIfUnderlyingNodeIsNullForList()
        {
            var list = new SyntaxNodeOrTokenList();
            list.Count.Should().Be(0);

            foreach (var index in new int[] { -1, 0, 23 })
            {
                bool exceptionThrown = false;
                try
                {
                    var unused = list[0];
                }
                catch (ArgumentOutOfRangeException)
                {
                    exceptionThrown = true;
                }
                exceptionThrown.Should().BeTrue();
            }
        }

        [WorkItem(541188, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/541188")]
        [Fact]
        public void GetDiagnosticsOnMissingToken()
        {
            var syntaxTree = SyntaxFactory.ParseSyntaxTree(@"namespace n1 { c1<t");
            var token = syntaxTree.FindNodeOrTokenByKind(SyntaxKind.GreaterThanToken);
            var diag = syntaxTree.GetDiagnostics(token).ToList();

            token.IsMissing.Should().BeTrue();
            diag.Count.Should().Be(1);
        }

        [WorkItem(541325, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/541325")]
        [Fact]
        public void GetDiagnosticsOnMissingToken2()
        {
            var syntaxTree = SyntaxFactory.ParseSyntaxTree(@"
class Base<T>
{
    public virtual int Property
    {
        get { return 0; }
        // Note: Repro for bug 7990 requires a missing close brace token i.e. missing } below
        set { 
    }
    public virtual void Method()
    {
    }
}");
            foreach (var t in syntaxTree.GetCompilationUnitRoot().DescendantTokens())
            {
                // Bug 7990: Below for loop is an infinite loop.
                foreach (var e in syntaxTree.GetDiagnostics(t))
                {
                }
            }

            // TODO: Please add meaningful checks once the above deadlock issue is fixed.
        }

        [WorkItem(541648, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/541648")]
        [Fact]
        public void GetDiagnosticsOnMissingToken4()
        {
            string code = @"
public class MyClass
{	
using Lib;
using Lib2;

public class Test1
{
}
}";
            var syntaxTree = SyntaxFactory.ParseSyntaxTree(code);
            var token = syntaxTree.GetCompilationUnitRoot().FindToken(code.IndexOf("using Lib;", StringComparison.Ordinal));
            var diag = syntaxTree.GetDiagnostics(token).ToList();

            token.IsMissing.Should().BeTrue();
            diag.Count.Should().Be(3);
        }

        [WorkItem(541630, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/541630")]
        [Fact]
        public void GetDiagnosticsOnBadReferenceDirective()
        {
            string code = @"class c1
{
    #r
    void m1()
    {
    }
}";
            var tree = SyntaxFactory.ParseSyntaxTree(code);
            var trivia = tree.GetCompilationUnitRoot().FindTrivia(code.IndexOf("#r", StringComparison.Ordinal)); // ReferenceDirective.

            foreach (var diag in tree.GetDiagnostics(trivia))
            {
                diag.Should().NotBeNull();
                // TODO: Please add any additional validations if necessary.
            }
        }

        [WorkItem(528626, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/528626")]
        [Fact]
        public void SpanOfNodeWithMissingChildren()
        {
            string code = @"delegate = 1;";
            var tree = SyntaxFactory.ParseSyntaxTree(code);
            var compilationUnit = tree.GetCompilationUnitRoot();
            var delegateDecl = (DelegateDeclarationSyntax)compilationUnit.Members[0];
            var paramList = delegateDecl.ParameterList;

            // For (non-EOF) tokens, IsMissing is true if and only if Width is 0.
            compilationUnit.DescendantTokens(node => true).
                Where(token => token.Kind() != SyntaxKind.EndOfFileToken).
                All(token => token.IsMissing == (token.Width == 0)).Should().BeTrue();

            // For non-terminals, Is true if Width is 0, but the converse may not hold.
            paramList.IsMissing.Should().BeTrue();
            paramList.Width.Should().NotBe(0);
            paramList.FullWidth.Should().NotBe(0);
        }

        [WorkItem(542457, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/542457")]
        [Fact]
        public void AddMethodModifier()
        {
            var tree = SyntaxFactory.ParseSyntaxTree(@"
class Program
{
    static void Main(string[] args)
    {
    }
}");
            var compilationUnit = tree.GetCompilationUnitRoot();
            var @class = (ClassDeclarationSyntax)compilationUnit.Members.Single();
            var method = (MethodDeclarationSyntax)@class.Members.Single();
            var newModifiers = method.Modifiers.Add(SyntaxFactory.Token(default(SyntaxTriviaList), SyntaxKind.UnsafeKeyword, SyntaxFactory.TriviaList(SyntaxFactory.Space)));
            newModifiers.ToFullString().Should().Be("    static unsafe ");
            newModifiers.Count.Should().Be(2);
            newModifiers[0].Kind().Should().Be(SyntaxKind.StaticKeyword);
            newModifiers[1].Kind().Should().Be(SyntaxKind.UnsafeKeyword);
        }

        [Fact]
        public void SeparatedSyntaxListValidation()
        {
            var intType = SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.IntKeyword));
            var commaToken = SyntaxFactory.Token(SyntaxKind.CommaToken);

            SyntaxFactory.SingletonSeparatedList<TypeSyntax>(intType);
            SyntaxFactory.SeparatedList<TypeSyntax>(new SyntaxNodeOrToken[] { intType, commaToken });
            SyntaxFactory.SeparatedList<TypeSyntax>(new SyntaxNodeOrToken[] { intType, commaToken, intType });
            SyntaxFactory.SeparatedList<TypeSyntax>(new SyntaxNodeOrToken[] { intType, commaToken, intType, commaToken });

            Assert.Throws<ArgumentException>(() => SyntaxFactory.SeparatedList<TypeSyntax>(new SyntaxNodeOrToken[] { commaToken }));
            Assert.Throws<ArgumentException>(() => SyntaxFactory.SeparatedList<TypeSyntax>(new SyntaxNodeOrToken[] { intType, commaToken, commaToken }));
            Assert.Throws<ArgumentException>(() => SyntaxFactory.SeparatedList<TypeSyntax>(new SyntaxNodeOrToken[] { intType, intType }));
        }

        [WorkItem(543310, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/543310")]
        [Fact]
        public void SyntaxDotParseCompilationUnitContainingOnlyWhitespace()
        {
            var node = SyntaxFactory.ParseCompilationUnit("  ");
            node.HasLeadingTrivia.Should().BeTrue();
            node.GetLeadingTrivia().Count.Should().Be(1);
            node.DescendantTrivia().Count().Should().Be(1);
            node.GetLeadingTrivia().First().ToString().Should().Be("  ");
        }

        [WorkItem(543310, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/543310")]
        [Fact]
        public void SyntaxTreeDotParseCompilationUnitContainingOnlyWhitespace()
        {
            var node = SyntaxFactory.ParseSyntaxTree("  ").GetCompilationUnitRoot();
            node.HasLeadingTrivia.Should().BeTrue();
            node.GetLeadingTrivia().Count.Should().Be(1);
            node.DescendantTrivia().Count().Should().Be(1);
            node.GetLeadingTrivia().First().ToString().Should().Be("  ");
        }

        [Fact]
        public void SyntaxNodeAndTokenToString()
        {
            var text = @"class A { }";
            var root = SyntaxFactory.ParseCompilationUnit(text);
            var children = root.DescendantNodesAndTokens();

            var nodeOrToken = children.First();
            nodeOrToken.ToString().Should().Be("class A { }");
            nodeOrToken.ToString().Should().Be(text);

            var node = (SyntaxNode)children.First(n => n.IsNode);
            node.ToString().Should().Be("class A { }");
            node.ToFullString().Should().Be(text);

            var token = (SyntaxToken)children.First(n => n.IsToken);
            token.ToString().Should().Be("class");
            token.ToFullString().Should().Be("class ");

            var trivia = root.DescendantTrivia().First();
            trivia.ToString().Should().Be(" ");
            trivia.ToFullString().Should().Be(" ");
        }

        [WorkItem(545116, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/545116")]
        [Fact]
        public void FindTriviaOutsideNode()
        {
            var text = @"// This is trivia
class C
{
    static void Main()
    {
    }
}
";
            var root = SyntaxFactory.ParseCompilationUnit(text);
            0.Should().BeInRange(root.FullSpan.Start, root.FullSpan.End);
            var rootTrivia = root.FindTrivia(0);
            rootTrivia.ToString().Trim().Should().Be("// This is trivia");

            var method = root.DescendantNodes().OfType<MethodDeclarationSyntax>().Single();
            0.Should().NotBeInRange(method.FullSpan.Start, method.FullSpan.End);
            var methodTrivia = method.FindTrivia(0);
            methodTrivia.Should().Be(default(SyntaxTrivia));
        }

        [Fact]
        public void TestSyntaxTriviaListEquals()
        {
            var emptyWhitespace = SyntaxFactory.Whitespace("");
            var emptyToken = SyntaxFactory.MissingToken(SyntaxKind.IdentifierToken).WithTrailingTrivia(emptyWhitespace, emptyWhitespace);
            var emptyTokenList = SyntaxFactory.TokenList(emptyToken, emptyToken);

            // elements should be not equal
            emptyTokenList[1].TrailingTrivia[0].Should().NotBe(emptyTokenList[0].TrailingTrivia[0]);

            // lists should be not equal
            emptyTokenList[1].TrailingTrivia.Should().NotBe(emptyTokenList[0].TrailingTrivia);

            // Two lists with the same parent node, but different indexes should NOT be the same.
            var emptyTriviaList = SyntaxFactory.TriviaList(emptyWhitespace, emptyWhitespace);
            emptyToken = emptyToken.WithLeadingTrivia(emptyTriviaList).WithTrailingTrivia(emptyTriviaList);

            // elements should be not equal
            emptyToken.TrailingTrivia[0].Should().NotBe(emptyToken.LeadingTrivia[0]);

            // lists should be not equal
            emptyToken.TrailingTrivia.Should().NotBe(emptyToken.LeadingTrivia);
        }

        [Fact]
        public void Test_SyntaxTree_ParseTextInvalidArguments()
        {
            // Invalid arguments - Validate Exceptions     
            Assert.Throws<System.ArgumentNullException>(delegate
            {
                SourceText st = null;
                var treeFromSource_invalid2 = SyntaxFactory.ParseSyntaxTree(st);
            });
        }

        [Fact]
        public void TestSyntaxTree_Changes()
        {
            string SourceText = @"using System;
using System.Linq;
using System.Collections;

namespace HelloWorld
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(""Hello, World!"");
        }
    }";

            SyntaxTree tree = SyntaxFactory.ParseSyntaxTree(SourceText);
            var root = (CompilationUnitSyntax)tree.GetRoot();

            // Get the Imports Clauses
            var FirstUsingClause = root.Usings[0];
            var SecondUsingClause = root.Usings[1];
            var ThirdUsingClause = root.Usings[2];

            var ChangesForDifferentTrees = FirstUsingClause.SyntaxTree.GetChanges(SecondUsingClause.SyntaxTree);
            ChangesForDifferentTrees.Count.Should().Be(0);

            // Do a transform to Replace and Existing Tree
            NameSyntax name = SyntaxFactory.QualifiedName(SyntaxFactory.IdentifierName("System"), SyntaxFactory.IdentifierName("Collections.Generic"));

            UsingDirectiveSyntax newUsingClause = ThirdUsingClause.WithName(name);

            // Replace Node with a different Imports Clause
            root = root.ReplaceNode(ThirdUsingClause, newUsingClause);

            var ChangesFromTransform = ThirdUsingClause.SyntaxTree.GetChanges(newUsingClause.SyntaxTree);
            ChangesFromTransform.Count.Should().Be(2);

            // Using the Common Syntax Changes Method
            SyntaxTree x = ThirdUsingClause.SyntaxTree;
            SyntaxTree y = newUsingClause.SyntaxTree;

            var changes2UsingCommonSyntax = x.GetChanges(y);
            changes2UsingCommonSyntax.Count.Should().Be(2);

            // Verify Changes from CS Specific SyntaxTree and Common SyntaxTree are the same
            changes2UsingCommonSyntax.Should().Be(ChangesFromTransform);
        }

        [Fact, WorkItem(658329, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/658329")]
        public void TestSyntaxTree_GetChangesInvalid()
        {
            string SourceText = @"using System;
using System.Linq;
using System.Collections;

namespace HelloWorld
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(""Hello, World!"");
        }
    }";

            SyntaxTree tree = SyntaxFactory.ParseSyntaxTree(SourceText);
            var root = (CompilationUnitSyntax)tree.GetRoot();

            // Get the Imports Clauses
            var FirstUsingClause = root.Usings[0];
            var SecondUsingClause = root.Usings[1];
            var ThirdUsingClause = root.Usings[2];

            var ChangesForDifferentTrees = FirstUsingClause.SyntaxTree.GetChanges(SecondUsingClause.SyntaxTree);
            ChangesForDifferentTrees.Count.Should().Be(0);

            // With null tree
            SyntaxTree BlankTree = null;
            Assert.Throws<ArgumentNullException>(() => FirstUsingClause.SyntaxTree.GetChanges(BlankTree));
        }

        [Fact, WorkItem(658329, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/658329")]
        public void TestSyntaxTree_GetChangedSpansInvalid()
        {
            string SourceText = @"using System;
using System.Linq;
using System.Collections;
using AwesomeAssertions;

namespace HelloWorld
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(""Hello, World!"");
        }
    }";

            SyntaxTree tree = SyntaxFactory.ParseSyntaxTree(SourceText);
            var root = (CompilationUnitSyntax)tree.GetRoot();

            // Get the Imports Clauses
            var FirstUsingClause = root.Usings[0];
            var SecondUsingClause = root.Usings[1];
            var ThirdUsingClause = root.Usings[2];

            var ChangesForDifferentTrees = FirstUsingClause.SyntaxTree.GetChangedSpans(SecondUsingClause.SyntaxTree);
            ChangesForDifferentTrees.Count.Should().Be(0);

            // With null tree
            SyntaxTree BlankTree = null;
            Assert.Throws<ArgumentNullException>(() => FirstUsingClause.SyntaxTree.GetChangedSpans(BlankTree));
        }

        [Fact]
        public void TestTriviaExists()
        {
            // token constructed using factory w/o specifying trivia (should have zero-width elastic trivia)
            var idToken = SyntaxFactory.Identifier("goo");
            idToken.HasLeadingTrivia.Should().BeTrue();
            idToken.LeadingTrivia.Count.Should().Be(1);
            idToken.LeadingTrivia.Span.Length.Should().Be(0); // zero-width elastic trivia
            idToken.HasTrailingTrivia.Should().BeTrue();
            idToken.TrailingTrivia.Count.Should().Be(1);
            idToken.TrailingTrivia.Span.Length.Should().Be(0); // zero-width elastic trivia

            // token constructed by parser w/o trivia
            idToken = SyntaxFactory.ParseToken("x");
            idToken.HasLeadingTrivia.Should().BeFalse();
            idToken.LeadingTrivia.Count.Should().Be(0);
            idToken.HasTrailingTrivia.Should().BeFalse();
            idToken.TrailingTrivia.Count.Should().Be(0);

            // token constructed by parser with trivia
            idToken = SyntaxFactory.ParseToken(" x  ");
            idToken.HasLeadingTrivia.Should().BeTrue();
            idToken.LeadingTrivia.Count.Should().Be(1);
            idToken.LeadingTrivia.Span.Length.Should().Be(1);
            idToken.HasTrailingTrivia.Should().BeTrue();
            idToken.TrailingTrivia.Count.Should().Be(1);
            idToken.TrailingTrivia.Span.Length.Should().Be(2);

            // node constructed using factory w/o specifying trivia
            SyntaxNode namedNode = SyntaxFactory.IdentifierName("goo");
            namedNode.HasLeadingTrivia.Should().BeTrue();
            namedNode.GetLeadingTrivia().Count.Should().Be(1);
            namedNode.GetLeadingTrivia().Span.Length.Should().Be(0);  // zero-width elastic trivia
            namedNode.HasTrailingTrivia.Should().BeTrue();
            namedNode.GetTrailingTrivia().Count.Should().Be(1);
            namedNode.GetTrailingTrivia().Span.Length.Should().Be(0);  // zero-width elastic trivia

            // node constructed by parse w/o trivia
            namedNode = SyntaxFactory.ParseExpression("goo");
            namedNode.HasLeadingTrivia.Should().BeFalse();
            namedNode.GetLeadingTrivia().Count.Should().Be(0);
            namedNode.HasTrailingTrivia.Should().BeFalse();
            namedNode.GetTrailingTrivia().Count.Should().Be(0);

            // node constructed by parse with trivia
            namedNode = SyntaxFactory.ParseExpression(" goo  ");
            namedNode.HasLeadingTrivia.Should().BeTrue();
            namedNode.GetLeadingTrivia().Count.Should().Be(1);
            namedNode.GetLeadingTrivia().Span.Length.Should().Be(1);
            namedNode.HasTrailingTrivia.Should().BeTrue();
            namedNode.GetTrailingTrivia().Count.Should().Be(1);
            namedNode.GetTrailingTrivia().Span.Length.Should().Be(2);

            // nodeOrToken with token constructed from factory w/o specifying trivia
            SyntaxNodeOrToken nodeOrToken = SyntaxFactory.Identifier("goo");
            nodeOrToken.HasLeadingTrivia.Should().BeTrue();
            nodeOrToken.GetLeadingTrivia().Count.Should().Be(1);
            nodeOrToken.GetLeadingTrivia().Span.Length.Should().Be(0); // zero-width elastic trivia
            nodeOrToken.HasTrailingTrivia.Should().BeTrue();
            nodeOrToken.GetTrailingTrivia().Count.Should().Be(1);
            nodeOrToken.GetTrailingTrivia().Span.Length.Should().Be(0); // zero-width elastic trivia

            // nodeOrToken with node constructed from factory w/o specifying trivia
            nodeOrToken = SyntaxFactory.IdentifierName("goo");
            nodeOrToken.HasLeadingTrivia.Should().BeTrue();
            nodeOrToken.GetLeadingTrivia().Count.Should().Be(1);
            nodeOrToken.GetLeadingTrivia().Span.Length.Should().Be(0); // zero-width elastic trivia
            nodeOrToken.HasTrailingTrivia.Should().BeTrue();
            nodeOrToken.GetTrailingTrivia().Count.Should().Be(1);
            nodeOrToken.GetTrailingTrivia().Span.Length.Should().Be(0); // zero-width elastic trivia

            // nodeOrToken with token parsed from factory w/o trivia
            nodeOrToken = SyntaxFactory.ParseToken("goo");
            nodeOrToken.HasLeadingTrivia.Should().BeFalse();
            nodeOrToken.GetLeadingTrivia().Count.Should().Be(0);
            nodeOrToken.HasTrailingTrivia.Should().BeFalse();
            nodeOrToken.GetTrailingTrivia().Count.Should().Be(0);

            // nodeOrToken with node parsed from factory w/o trivia
            nodeOrToken = SyntaxFactory.ParseExpression("goo");
            nodeOrToken.HasLeadingTrivia.Should().BeFalse();
            nodeOrToken.GetLeadingTrivia().Count.Should().Be(0);
            nodeOrToken.HasTrailingTrivia.Should().BeFalse();
            nodeOrToken.GetTrailingTrivia().Count.Should().Be(0);

            // nodeOrToken with token parsed from factory with trivia
            nodeOrToken = SyntaxFactory.ParseToken(" goo  ");
            nodeOrToken.HasLeadingTrivia.Should().BeTrue();
            nodeOrToken.GetLeadingTrivia().Count.Should().Be(1);
            nodeOrToken.GetLeadingTrivia().Span.Length.Should().Be(1); // zero-width elastic trivia
            nodeOrToken.HasTrailingTrivia.Should().BeTrue();
            nodeOrToken.GetTrailingTrivia().Count.Should().Be(1);
            nodeOrToken.GetTrailingTrivia().Span.Length.Should().Be(2); // zero-width elastic trivia

            // nodeOrToken with node parsed from factory with trivia
            nodeOrToken = SyntaxFactory.ParseExpression(" goo  ");
            nodeOrToken.HasLeadingTrivia.Should().BeTrue();
            nodeOrToken.GetLeadingTrivia().Count.Should().Be(1);
            nodeOrToken.GetLeadingTrivia().Span.Length.Should().Be(1); // zero-width elastic trivia
            nodeOrToken.HasTrailingTrivia.Should().BeTrue();
            nodeOrToken.GetTrailingTrivia().Count.Should().Be(1);
            nodeOrToken.GetTrailingTrivia().Span.Length.Should().Be(2); // zero-width elastic trivia
        }

        [WorkItem(6536, "https://github.com/dotnet/roslyn/issues/6536")]
        [Fact]
        public void TestFindTrivia_NoStackOverflowOnLargeExpression()
        {
            StringBuilder code = new StringBuilder();
            code.Append(
@"class Goo
{
    void Bar()
    {
        string test = ");
            for (var i = 0; i < 3000; i++)
            {
                code.Append(@"""asdf"" + ");
            }
            code.Append(@"""last"";
    }
}");
            var tree = SyntaxFactory.ParseSyntaxTree(code.ToString());
            var position = 4000;
            var trivia = tree.GetCompilationUnitRoot().FindTrivia(position);
            // no stack overflow
        }

        [Fact, WorkItem(8625, "https://github.com/dotnet/roslyn/issues/8625")]
        public void SyntaxNodeContains()
        {
            var text = "a + (b - (c * (d / e)))";
            var expression = SyntaxFactory.ParseExpression(text);
            var a = expression.DescendantNodes().OfType<IdentifierNameSyntax>().First(n => n.Identifier.Text == "a");
            var e = expression.DescendantNodes().OfType<IdentifierNameSyntax>().First(n => n.Identifier.Text == "e");

            var firstParens = e.FirstAncestorOrSelf<ExpressionSyntax>(n => n.Kind() == SyntaxKind.ParenthesizedExpression);

            firstParens.Contains(a).Should().BeFalse();  // fixing #8625 allows this to return quicker
            firstParens.Contains(e).Should().BeTrue();
        }

        [Fact, WorkItem(54239, "https://github.com/dotnet/roslyn/issues/54239")]
        public void TestWithAsyncKeyword_AnonymousMethodExpressionSyntax_AddAsync()
        {
            var text = "static delegate(int i) { }";
            var expression = (AnonymousMethodExpressionSyntax)SyntaxFactory.ParseExpression(text);
            var withAsync = expression.WithAsyncKeyword(SyntaxFactory.Token(SyntaxKind.AsyncKeyword).WithTrailingTrivia(SyntaxFactory.Space)).ToString();
            withAsync.Should().Be("static async delegate(int i) { }");
        }

        [Fact, WorkItem(54239, "https://github.com/dotnet/roslyn/issues/54239")]
        public void TestWithAsyncKeyword_ParenthesizedLambdaExpressionSyntax_AddAsync()
        {
            var text = "static (a) => { }";
            var expression = (ParenthesizedLambdaExpressionSyntax)SyntaxFactory.ParseExpression(text);
            var withAsync = expression.WithAsyncKeyword(SyntaxFactory.Token(SyntaxKind.AsyncKeyword).WithTrailingTrivia(SyntaxFactory.Space)).ToString();
            withAsync.Should().Be("static async (a) => { }");
        }

        [Fact, WorkItem(54239, "https://github.com/dotnet/roslyn/issues/54239")]
        public void TestWithAsyncKeyword_SimpleLambdaExpressionSyntax_AddAsync()
        {
            var text = "static a => { }";
            var expression = (SimpleLambdaExpressionSyntax)SyntaxFactory.ParseExpression(text);
            var withAsync = expression.WithAsyncKeyword(SyntaxFactory.Token(SyntaxKind.AsyncKeyword).WithTrailingTrivia(SyntaxFactory.Space)).ToString();
            withAsync.Should().Be("static async a => { }");
        }

        [Fact, WorkItem(54239, "https://github.com/dotnet/roslyn/issues/54239")]
        public void TestWithAsyncKeyword_AnonymousMethodExpressionSyntax_ReplaceAsync()
        {
            var text = "static async/**/delegate(int i) { }";
            var expression = (AnonymousMethodExpressionSyntax)SyntaxFactory.ParseExpression(text);
            var withAsync = expression.WithAsyncKeyword(SyntaxFactory.Token(SyntaxKind.AsyncKeyword).WithTrailingTrivia(SyntaxFactory.Space)).ToString();
            withAsync.Should().Be("static async delegate(int i) { }");
        }

        [Fact, WorkItem(54239, "https://github.com/dotnet/roslyn/issues/54239")]
        public void TestWithAsyncKeyword_ParenthesizedLambdaExpressionSyntax_ReplaceAsync()
        {
            var text = "static async/**/(a) => { }";
            var expression = (ParenthesizedLambdaExpressionSyntax)SyntaxFactory.ParseExpression(text);
            var withAsync = expression.WithAsyncKeyword(SyntaxFactory.Token(SyntaxKind.AsyncKeyword).WithTrailingTrivia(SyntaxFactory.Space)).ToString();
            withAsync.Should().Be("static async (a) => { }");
        }

        [Fact, WorkItem(54239, "https://github.com/dotnet/roslyn/issues/54239")]
        public void TestWithAsyncKeyword_SimpleLambdaExpressionSyntax_ReplaceAsync()
        {
            var text = "static async/**/a => { }";
            var expression = (SimpleLambdaExpressionSyntax)SyntaxFactory.ParseExpression(text);
            var withAsync = expression.WithAsyncKeyword(SyntaxFactory.Token(SyntaxKind.AsyncKeyword).WithTrailingTrivia(SyntaxFactory.Space)).ToString();
            withAsync.Should().Be("static async a => { }");
        }

        [Fact, WorkItem(54239, "https://github.com/dotnet/roslyn/issues/54239")]
        public void TestWithAsyncKeyword_AnonymousMethodExpressionSyntax_RemoveExistingAsync()
        {
            var text = "static async/**/delegate(int i) { }";
            var expression = (AnonymousMethodExpressionSyntax)SyntaxFactory.ParseExpression(text);
            var withAsync = expression.WithAsyncKeyword(default).ToString();
            withAsync.Should().Be("static delegate(int i) { }");
        }

        [Fact, WorkItem(54239, "https://github.com/dotnet/roslyn/issues/54239")]
        public void TestWithAsyncKeyword_ParenthesizedLambdaExpressionSyntax_RemoveExistingAsync()
        {
            var text = "static async (a) => { }";
            var expression = (ParenthesizedLambdaExpressionSyntax)SyntaxFactory.ParseExpression(text);
            var withAsync = expression.WithAsyncKeyword(default).ToString();
            withAsync.Should().Be("static (a) => { }");
        }

        [Fact, WorkItem(54239, "https://github.com/dotnet/roslyn/issues/54239")]
        public void TestWithAsyncKeyword_SimpleLambdaExpressionSyntax_RemoveExistingAsync()
        {
            var text = "static async/**/a => { }";
            var expression = (SimpleLambdaExpressionSyntax)SyntaxFactory.ParseExpression(text);
            var withAsync = expression.WithAsyncKeyword(default).ToString();
            withAsync.Should().Be("static a => { }");
        }

        [Fact, WorkItem(54239, "https://github.com/dotnet/roslyn/issues/54239")]
        public void TestWithAsyncKeyword_AnonymousMethodExpressionSyntax_RemoveNonExistingAsync()
        {
            var text = "static delegate(int i) { }";
            var expression = (AnonymousMethodExpressionSyntax)SyntaxFactory.ParseExpression(text);
            var withAsync = expression.WithAsyncKeyword(default).ToString();
            withAsync.Should().Be(text);
        }

        [Fact, WorkItem(54239, "https://github.com/dotnet/roslyn/issues/54239")]
        public void TestWithAsyncKeyword_ParenthesizedLambdaExpressionSyntax_RemoveNonExistingAsync()
        {
            var text = "static (a) => { }";
            var expression = (ParenthesizedLambdaExpressionSyntax)SyntaxFactory.ParseExpression(text);
            var withAsync = expression.WithAsyncKeyword(default).ToString();
            withAsync.Should().Be(text);
        }

        [Fact, WorkItem(54239, "https://github.com/dotnet/roslyn/issues/54239")]
        public void TestWithAsyncKeyword_SimpleLambdaExpressionSyntax_RemoveNonExistingAsync()
        {
            var text = "static a => { }";
            var expression = (SimpleLambdaExpressionSyntax)SyntaxFactory.ParseExpression(text);
            var withAsync = expression.WithAsyncKeyword(default).ToString();
            withAsync.Should().Be(text);
        }

        [Fact, WorkItem(54239, "https://github.com/dotnet/roslyn/issues/54239")]
        public void TestWithAsyncKeyword_AnonymousMethodExpressionSyntax_ReplaceAsync_ExistingTwoKeywords()
        {
            var text = "static async/*async1*/ async/*async2*/delegate(int i) { }";
            var expression = (AnonymousMethodExpressionSyntax)SyntaxFactory.ParseExpression(text);
            var newAsync = SyntaxFactory.Token(SyntaxKind.AsyncKeyword).WithTrailingTrivia(SyntaxFactory.Space);
            var withAsync = expression.WithAsyncKeyword(newAsync).ToString();
            withAsync.Should().Be("static async async/*async2*/delegate(int i) { }");
        }

        [Fact, WorkItem(54239, "https://github.com/dotnet/roslyn/issues/54239")]
        public void TestWithAsyncKeyword_AnonymousMethodExpressionSyntax_RemoveAllExistingAsync()
        {
            var text = "static async/*async1*/ async/*async2*/ delegate(int i) { }";
            var expression = (AnonymousMethodExpressionSyntax)SyntaxFactory.ParseExpression(text);
            var withAsync = expression.WithAsyncKeyword(default);
            withAsync.ToString().Should().Be("static delegate(int i) { }");
            withAsync.AsyncKeyword.Should().Be(default);
        }

        private static void TestWithWindowsAndUnixEndOfLines(string inputText, string expectedText, Action<CompilationUnitSyntax, string> action)
        {
            inputText = inputText.NormalizeLineEndings();
            expectedText = expectedText.NormalizeLineEndings();

            var tests = new Dictionary<string, string>
            {
                {inputText, expectedText}, // Test CRLF (Windows)
                {inputText.Replace("\r", ""), expectedText.Replace("\r", "")}, // Test LF (Unix)
            };

            foreach (var test in tests)
            {
                action(SyntaxFactory.ParseCompilationUnit(test.Key), test.Value);
            }
        }

        [Fact]
        [WorkItem(56740, "https://github.com/dotnet/roslyn/issues/56740")]
        public void TestStackAllocKeywordUpdate()
        {
            var text = "stackalloc/**/int[50]";
            var expression = (StackAllocArrayCreationExpressionSyntax)SyntaxFactory.ParseExpression(text);
            var replacedKeyword = SyntaxFactory.Token(SyntaxKind.StackAllocKeyword).WithTrailingTrivia(SyntaxFactory.Space);
            var newExpression = expression.Update(replacedKeyword, expression.Type).ToString();
            newExpression.Should().Be("stackalloc int[50]");
        }

        [Fact]
        [WorkItem(58597, "https://github.com/dotnet/roslyn/issues/58597")]
        public void TestExclamationExclamationUpdate()
        {
            var text = "(string s!!)";
            var parameter = SyntaxFactory.ParseParameterList(text).Parameters[0];
            var newParameter = parameter.Update(parameter.AttributeLists, parameter.Modifiers, parameter.Type, parameter.Identifier, parameter.Default);
            newParameter.ToFullString().Should().Be("string s!!");
            newParameter.ToString().Should().Be("string s");
        }
    }
}
