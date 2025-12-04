// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#nullable disable

using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Syntax;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Test.Utilities;
using Xunit;

namespace Microsoft.CodeAnalysis.CSharp.UnitTests
{
    public class StructuredTriviaTests
    {
        [Fact]
        public void GetParentTrivia()
        {
            const string conditionName = "condition";

            var trivia1 = SyntaxFactory.Trivia(SyntaxFactory.IfDirectiveTrivia(SyntaxFactory.IdentifierName(conditionName), false, false, false));
            var structuredTrivia = trivia1.GetStructure() as IfDirectiveTriviaSyntax;
            structuredTrivia.Should().NotBeNull();
            ((IdentifierNameSyntax)structuredTrivia.Condition).Identifier.ValueText.Should().Be(conditionName);
            var trivia2 = structuredTrivia.ParentTrivia;
            trivia2.Should().Be(trivia1);
        }

        [Fact]
        public void TestStructuredTrivia()
        {
            var spaceTrivia = SyntaxTriviaListBuilder.Create().Add(SyntaxFactory.Whitespace(" ")).ToList();
            var emptyTrivia = SyntaxTriviaListBuilder.Create().ToList();

            var name = "goo";
            var xmlStartElement = SyntaxFactory.XmlElementStartTag(
                SyntaxFactory.Token(spaceTrivia, SyntaxKind.LessThanToken, default(SyntaxTriviaList)),
                SyntaxFactory.XmlName(null,
                    SyntaxFactory.Identifier(name)),
                default(SyntaxList<XmlAttributeSyntax>),
                SyntaxFactory.Token(default(SyntaxTriviaList), SyntaxKind.GreaterThanToken, spaceTrivia));

            var xmlEndElement = SyntaxFactory.XmlElementEndTag(
                SyntaxFactory.Token(SyntaxKind.LessThanSlashToken),
                SyntaxFactory.XmlName(null,
                    SyntaxFactory.Identifier(name)),
                SyntaxFactory.Token(default(SyntaxTriviaList), SyntaxKind.GreaterThanToken, spaceTrivia));

            var xmlElement = SyntaxFactory.XmlElement(xmlStartElement, default(SyntaxList<XmlNodeSyntax>), xmlEndElement);
            xmlElement.ToFullString().Should().Be(" <goo> </goo> ");
            xmlElement.ToString().Should().Be("<goo> </goo>");

            var docComment = SyntaxFactory.DocumentationCommentTrivia(SyntaxKind.SingleLineDocumentationCommentTrivia).WithContent(new SyntaxList<XmlNodeSyntax>(xmlElement));
            docComment.ToFullString().Should().Be(" <goo> </goo> ");
            // docComment.GetText().Should().Be("<goo> </goo>");
            var child = (XmlElementSyntax)docComment.ChildNodesAndTokens()[0];
            child.ToFullString().Should().Be(" <goo> </goo> ");
            child.ToString().Should().Be("<goo> </goo>");
            child.StartTag.ToFullString().Should().Be(" <goo> ");
            child.StartTag.ToString().Should().Be("<goo>");

            var sTrivia = SyntaxFactory.Trivia(docComment);
            sTrivia.Should().NotBe(default(SyntaxTrivia));
            var ident = SyntaxFactory.Identifier(SyntaxTriviaList.Create(sTrivia), "banana", spaceTrivia);

            ident.ToFullString().Should().Be(" <goo> </goo> banana ");
            ident.ToString().Should().Be("banana");
            ident.LeadingTrivia[0].ToFullString().Should().Be(" <goo> </goo> ");
            // ident.LeadingTrivia[0].GetText().Should().Be("<goo> </goo>");

            var identExpr = SyntaxFactory.IdentifierName(ident);

            // make sure FindLeaf digs into the structured trivia.
            var result = identExpr.FindToken(3, true);
            result.Kind().Should().Be(SyntaxKind.IdentifierToken);
            result.ToString().Should().Be("goo");

            var trResult = identExpr.FindTrivia(6, SyntaxTrivia.Any);
            trResult.Kind().Should().Be(SyntaxKind.WhitespaceTrivia);
            trResult.ToString().Should().Be(" ");

            var foundDocComment = result.Parent.Parent.Parent.Parent;
            foundDocComment.Parent.Should().BeNull();

            var identTrivia = identExpr.GetLeadingTrivia()[0];
            var foundTrivia = ((DocumentationCommentTriviaSyntax)foundDocComment).ParentTrivia;
            foundTrivia.Should().Be(identTrivia);

            // make sure FindLeafNodesOverlappingWithSpan does not dig into the structured trivia.
            var resultList = identExpr.DescendantTokens(t => t.FullSpan.OverlapsWith(new TextSpan(3, 18)));
            resultList.Count().Should().Be(1);
        }

        [Fact]
        public void ReferenceDirectives1()
        {
            var tree = SyntaxFactory.ParseSyntaxTree(@"
#r ""ref0""
#define Goo
#r ""ref1""
#r ""ref2""
using Blah;
using AwesomeAssertions;
#r ""ref3""
");
            var compilationUnit = tree.GetCompilationUnitRoot();
            var directives = compilationUnit.GetReferenceDirectives();
            directives.Count.Should().Be(3);
            directives[0].File.Value.Should().Be("ref0");
            directives[1].File.Value.Should().Be("ref1");
            directives[2].File.Value.Should().Be("ref2");
        }

        [Fact]
        public void ReferenceDirectives2()
        {
            var tree = SyntaxFactory.ParseSyntaxTree(@"
#r ""ref0""
");
            var compilationUnit = tree.GetCompilationUnitRoot();
            var directives = compilationUnit.GetReferenceDirectives();
            directives.Count.Should().Be(1);
            directives[0].File.Value.Should().Be("ref0");
        }

        [Fact]
        public void ReferenceDirectives3()
        {
            var tree = SyntaxFactory.ParseSyntaxTree(@"
");
            var compilationUnit = tree.GetCompilationUnitRoot();
            var directives = compilationUnit.GetReferenceDirectives();
            directives.Count.Should().Be(0);
        }

        [Fact]
        public void ReferenceDirectives4()
        {
            var tree = SyntaxFactory.ParseSyntaxTree(@"
#r 
#r ""
#r ""a"" blah
");
            var compilationUnit = tree.GetCompilationUnitRoot();
            var directives = compilationUnit.GetReferenceDirectives();
            directives.Count.Should().Be(3);
            directives[0].File.IsMissing.Should().BeTrue();
            directives[1].File.IsMissing.Should().BeFalse();
            directives[1].File.Value.Should().Be("");
            directives[2].File.Value.Should().Be("a");
        }

        [WorkItem(546207, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/546207")]
        [Fact]
        public void DocumentationCommentsLocation_SingleLine()
        {
            var tree = SyntaxFactory.ParseSyntaxTree(@"
class Program
{ /// <summary/>

    static void Main() { }
}
");

            var trivia = tree.GetCompilationUnitRoot().DescendantTrivia().Single(t => t.Kind() == SyntaxKind.SingleLineDocumentationCommentTrivia);
            trivia.Token.Kind().Should().Be(SyntaxKind.StaticKeyword);
        }

        [WorkItem(546207, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/546207")]
        [Fact]
        public void DocumentationCommentsLocation_MultiLine()
        {
            var tree = SyntaxFactory.ParseSyntaxTree(@"
class Program
{ /** <summary/> */

    static void Main() { }
}
");

            var trivia = tree.GetCompilationUnitRoot().DescendantTrivia().Single(t => t.Kind() == SyntaxKind.MultiLineDocumentationCommentTrivia);
            trivia.Token.Kind().Should().Be(SyntaxKind.StaticKeyword);
        }

        [Fact]
        public void TestTriviaList_getItemFailures()
        {
            var tree = SyntaxFactory.ParseSyntaxTree(" class goo {}");

            var trivia = tree.GetCompilationUnitRoot().Members[0].GetLeadingTrivia();
            var t1 = trivia[0];
            trivia.Count.Should().Be(1);

            // Bounds checking exceptions
            Assert.Throws<System.ArgumentOutOfRangeException>(delegate
            {
                var t2 = trivia[1];
            });

            Assert.Throws<System.ArgumentOutOfRangeException>(delegate
            {
                var t3 = trivia[-1];
            });

            // Invalid Use create SyntaxTriviaList
            Assert.Throws<System.ArgumentOutOfRangeException>(delegate
            {
                var trl = new SyntaxTriviaList();
                var t2 = trl[0];
            });
        }
    }
}
