// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#nullable disable

using System;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp.Test.Utilities;
using Roslyn.Test.Utilities;
using Xunit;
using InternalSyntax = Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;
using AwesomeAssertions;

namespace Microsoft.CodeAnalysis.CSharp.UnitTests
{
    /// <summary>
    /// Test SyntaxRewriter and InternalSyntax.SyntaxRewriter.
    /// </summary>
    public class SyntaxRewriterTests
    {
        #region Green Tree / SeparatedSyntaxList

        [Fact]
        public void TestGreenSeparatedDeleteNone()
        {
            //the type argument list is a SeparateSyntaxList
            var input = "A<B,C,D>"; //NB: no whitespace, since it would be deleted
            var output = input;

            var rewriter = new GreenRewriter();

            TestGreen(input, output, rewriter, isExpr: true);
        }

        #endregion Green Tree / SeparatedSyntaxList

        #region Green Tree / SyntaxList

        [Fact]
        public void TestGreenDeleteNone()
        {
            //statements in block constitute a SyntaxList
            var input = "{A();B();C();}"; //NB: no whitespace, since it would be deleted
            var output = input;

            var rewriter = new GreenRewriter();

            TestGreen(input, output, rewriter, isExpr: false);
        }

        #endregion Green Tree / SyntaxList

        #region Red Tree / SeparatedSyntaxList

        [Fact]
        public void TestRedSeparatedDeleteNone()
        {
            //the type argument list is a SeparateSyntaxList
            var input = "A<B,C,D>"; //NB: no whitespace, since it would be deleted
            var output = input;

            var rewriter = new RedRewriter();

            TestRed(input, output, rewriter, isExpr: true);
        }

        [Fact]
        public void TestRedSeparatedDeleteSome()
        {
            //the type argument list is a SeparateSyntaxList
            var input = "A<B,C,D>"; //NB: no whitespace, since it would be deleted

            //delete the middle type argument (should clear the following comma)
            var rewriter = new RedRewriter(rewriteNode: node =>
                (node.IsKind(SyntaxKind.IdentifierName) && node.ToString() == "C") ? null : node);

            Exception caught = null;
            try
            {
                TestRed(input, "", rewriter, isExpr: true);
            }
            catch (Exception e)
            {
                caught = e;
            }

            caught.Should().NotBeNull();
            caught is InvalidOperationException.Should().BeTrue();
        }

        [Fact]
        public void TestRedSeparatedDeleteAll()
        {
            //the type argument list is a SeparateSyntaxList
            var input = "A<B,C,D>"; //NB: no whitespace, since it would be deleted

            //delete all type arguments, should clear the intervening commas
            var rewriter = new RedRewriter(rewriteNode: node =>
                (node.IsKind(SyntaxKind.IdentifierName) && node.ToString() != "A") ? null : node);

            Exception caught = null;
            try
            {
                TestRed(input, "", rewriter, isExpr: true);
            }
            catch (Exception e)
            {
                caught = e;
            }

            caught.Should().NotBeNull();
            caught is InvalidOperationException.Should().BeTrue();
        }

        #endregion Red Tree / SeparatedSyntaxList

        #region Red Tree / SyntaxTokenList

        [Fact]
        public void TestRedTokenDeleteNone()
        {
            //commas in an implicit array creation constitute a SyntaxTokenList
            var input = "x=new[,,]{{{}}};"; //NB: no whitespace, since it would be deleted
            var output = input;

            var rewriter = new RedRewriter();

            TestRed(input, output, rewriter, isExpr: false);
        }

        [Fact]
        public void TestRedTokenDeleteSome()
        {
            //commas in an implicit array creation constitute a SyntaxTokenList
            var input = "x=new[,,]{{{}}};"; //NB: no whitespace, since it would be deleted
            var output = "x=new[,]{{{}}};";

            //delete one comma
            bool first = true;
            var rewriter = new RedRewriter(rewriteToken: token =>
            {
                if (token.Kind() == SyntaxKind.CommaToken && first)
                {
                    first = false;
                    return default(SyntaxToken);
                }
                return token;
            });

            TestRed(input, output, rewriter, isExpr: false);
        }

        [Fact]
        public void TestRedTokenDeleteAll()
        {
            //commas in an implicit array creation constitute a SyntaxTokenList
            var input = "x=new[,,]{{{}}};"; //NB: no whitespace, since it would be deleted
            var output = "x=new[]{{{}}};";

            //delete all commas
            var rewriter = new RedRewriter(rewriteToken: token =>
                (token.Kind() == SyntaxKind.CommaToken) ? default(SyntaxToken) : token);

            TestRed(input, output, rewriter, isExpr: false);
        }

        #endregion Red Tree / SyntaxTokenList

        #region Red Tree / SyntaxNodeOrTokenList

        //These only in the syntax tree inside SeparatedSyntaxLists, so they are not visitable.
        //We can't call this directly due to its protection level.

        #endregion Red Tree / SyntaxNodeOrTokenList

        #region Red Tree / SyntaxTriviaList

        [Fact]
        public void TestRedTriviaDeleteNone()
        {
            //whitespace and comments constitute a SyntaxTriviaList
            var input = " a(); //comment"; //NB: no whitespace, since it would be deleted
            var output = input;

            var rewriter = new RedRewriter();

            TestRed(input, output, rewriter, isExpr: false);
        }

        [Fact]
        public void TestRedTriviaDeleteSome()
        {
            //whitespace and comments constitute a SyntaxTriviaList
            var input = " a(); //comment"; //NB: no whitespace, since it would be deleted
            var output = "a();//comment";

            //delete all whitespace trivia (leave comments)
            var rewriter = new RedRewriter(rewriteTrivia: trivia =>
                trivia.Kind() == SyntaxKind.WhitespaceTrivia ? default(SyntaxTrivia) : trivia);

            TestRed(input, output, rewriter, isExpr: false);
        }

        [Fact]
        public void TestRedTriviaDeleteAll()
        {
            //whitespace and comments constitute a SyntaxTriviaList
            var input = " a(); //comment"; //NB: no whitespace, since it would be deleted
            var output = "a();";

            //delete all trivia
            var rewriter = new RedRewriter(rewriteTrivia: trivia => default(SyntaxTrivia));

            TestRed(input, output, rewriter, isExpr: false);
        }

        #endregion Red Tree / SyntaxTriviaList

        #region Red Tree / SyntaxList

        [Fact]
        public void TestRedDeleteNone()
        {
            //statements in block constitute a SyntaxList
            var input = "{A();B();C();}"; //NB: no whitespace, since it would be deleted
            var output = input;

            var rewriter = new RedRewriter();

            TestRed(input, output, rewriter, isExpr: false);
        }

        [Fact]
        public void TestRedDeleteSome()
        {
            //statements in block constitute a SyntaxList
            var input = "{A();B();C();}"; //NB: no whitespace, since it would be deleted
            var output = "{A();C();}";

            //delete the middle statement
            var rewriter = new RedRewriter(rewriteNode: node =>
                (node.IsKind(SyntaxKind.ExpressionStatement) && node.ToString().Contains("B")) ? null : node);

            TestRed(input, output, rewriter, isExpr: false);
        }

        [Fact]
        public void TestRedDeleteAll()
        {
            //statements in block constitute a SyntaxList
            var input = "{A();B();C();}"; //NB: no whitespace, since it would be deleted
            var output = "{}";

            //delete all statements
            var rewriter = new RedRewriter(rewriteNode: node =>
                (node.IsKind(SyntaxKind.ExpressionStatement)) ? null : node);

            TestRed(input, output, rewriter, isExpr: false);
        }

        #endregion Red Tree / SyntaxList

        #region Misc

        [Fact]
        public void TestRedConsecutiveSeparators()
        {
            //there are omitted array size expressions between the commas
            var input = "int[,,]a;"; //NB: no whitespace, since it would be deleted

            bool first = true;
            var rewriter = new RedRewriter(rewriteNode: node =>
            {
                if (node != null && node.IsKind(SyntaxKind.OmittedArraySizeExpression) && first)
                {
                    first = false;
                    return null;
                }
                return node;
            });

            Exception caught = null;
            try
            {
                TestRed(input, "", rewriter, isExpr: false);
            }
            catch (Exception e)
            {
                caught = e;
            }

            caught.Should().NotBeNull();
            caught is InvalidOperationException.Should().BeTrue();
        }

        [Fact]
        public void TestRedSeparatedDeleteLast()
        {
            //the type argument list is a SeparateSyntaxList
            var input = "A<B,C,D>"; //NB: no whitespace, since it would be deleted
            var output = "A<B,C,>";

            //delete the last type argument (should clear the *preceding* comma)
            var rewriter = new RedRewriter(rewriteNode: node =>
                (node.IsKind(SyntaxKind.IdentifierName) && node.ToString() == "D") ? null : node);

            TestRed(input, output, rewriter, isExpr: true);
        }

        [Fact]
        public void TestSyntaxTreeForFactoryGenerated()
        {
            var node = SyntaxFactory.ClassDeclaration("Class1");
            node.SyntaxTree.Should().NotBeNull();
            node.SyntaxTree.HasCompilationUnitRoot.Should().BeFalse("how did we get a CompilationUnit root?");
            node.SyntaxTree.GetRoot().Should().BeSameAs(node);
        }

        [Fact]
        public void TestSyntaxTreeForParsedSyntaxNode()
        {
            var node1 = SyntaxFactory.ParseCompilationUnit("class Class1<T> { }");
            node1.SyntaxTree.Should().NotBeNull();
            node1.SyntaxTree.HasCompilationUnitRoot.Should().BeTrue("how did we get a non-CompilationUnit root?");
            node1.SyntaxTree.GetRoot().Should().BeSameAs(node1);
            var node2 = SyntaxFactory.ParseExpression("2 + 2");
            node2.SyntaxTree.Should().NotBeNull();
            node2.SyntaxTree.HasCompilationUnitRoot.Should().BeFalse("how did we get a CompilationUnit root?");
            node2.SyntaxTree.GetRoot().Should().BeSameAs(node2);
        }

        [Fact]
        public void TestSyntaxTreeForSyntaxTreeWithReplacedToken()
        {
            var tree = SyntaxFactory.ParseSyntaxTree("class Class1<T> { }");
            var tokenT = tree.GetCompilationUnitRoot().DescendantTokens().Where(t => t.ToString() == "T").Single();
            tree.GetCompilationUnitRoot().ReplaceToken(tokenT, tokenT).SyntaxTree.Should().BeSameAs(tree);
            var newRoot = tree.GetCompilationUnitRoot().ReplaceToken(tokenT, SyntaxFactory.Identifier("U"));
            newRoot.SyntaxTree.Should().NotBeNull();
            newRoot.SyntaxTree.HasCompilationUnitRoot.Should().BeTrue("how did we get a non-CompilationUnit root?");
            newRoot.SyntaxTree.GetRoot().Should().BeSameAs(newRoot);
        }

        [Fact]
        public void TestSyntaxTreeForSyntaxTreeWithReplacedNode()
        {
            var tree = SyntaxFactory.ParseSyntaxTree("class Class1 : Class2<T> { }");
            var typeName = tree.GetCompilationUnitRoot().DescendantNodes().Where(n => n.IsKind(SyntaxKind.GenericName)).Single();
            tree.GetCompilationUnitRoot().ReplaceNode(typeName, typeName).SyntaxTree.Should().BeSameAs(tree);
            var newRoot = tree.GetCompilationUnitRoot().ReplaceNode(typeName, SyntaxFactory.ParseTypeName("Class2<U>"));
            newRoot.SyntaxTree.Should().NotBeNull();
            newRoot.SyntaxTree.HasCompilationUnitRoot.Should().BeTrue("how did we get a non-CompilationUnit root?");
            newRoot.SyntaxTree.GetRoot().Should().BeSameAs(newRoot);
        }

        [Fact]
        [WorkItem(22010, "https://github.com/dotnet/roslyn/issues/22010")]
        public void TestReplaceNodeShouldNotLoseParseOptions()
        {
            var tree = SyntaxFactory.ParseSyntaxTree("System.Console.Write(\"Before\")", TestOptions.Script);
            var root = tree.GetRoot();
            var before = root.DescendantNodes().OfType<LiteralExpressionSyntax>().Single();
            var after = SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal("After"));

            var newRoot = root.ReplaceNode(before, after);
            var newTree = newRoot.SyntaxTree;
            newTree.Options.Kind.Should().Be(SourceCodeKind.Script);
            newTree.Options.Should().Be(tree.Options);
        }

        [Fact]
        [WorkItem(22010, "https://github.com/dotnet/roslyn/issues/22010")]
        public void TestReplaceNodeInListShouldNotLoseParseOptions()
        {
            var tree = SyntaxFactory.ParseSyntaxTree("m(a, b)", TestOptions.Script);
            tree.Options.Kind.Should().Be(SourceCodeKind.Script);

            var argC = SyntaxFactory.Argument(SyntaxFactory.ParseExpression("c"));
            var argD = SyntaxFactory.Argument(SyntaxFactory.ParseExpression("d"));
            var root = tree.GetRoot();
            var invocation = root.DescendantNodes().OfType<InvocationExpressionSyntax>().Single();
            var newRoot = root.ReplaceNode(invocation.ArgumentList.Arguments[0], new SyntaxNode[] { argC, argD });
            newRoot.ToFullString().Should().Be("m(c,d, b)");

            var newTree = newRoot.SyntaxTree;
            newTree.Options.Kind.Should().Be(SourceCodeKind.Script);
            newTree.Options.Should().Be(tree.Options);
        }

        [Fact]
        [WorkItem(22010, "https://github.com/dotnet/roslyn/issues/22010")]
        public void TestInsertNodeShouldNotLoseParseOptions()
        {
            var tree = SyntaxFactory.ParseSyntaxTree("m(a, b)", TestOptions.Script);
            tree.Options.Kind.Should().Be(SourceCodeKind.Script);

            var argC = SyntaxFactory.Argument(SyntaxFactory.ParseExpression("c"));
            var argD = SyntaxFactory.Argument(SyntaxFactory.ParseExpression("d"));
            var root = tree.GetRoot();
            var invocation = root.DescendantNodes().OfType<InvocationExpressionSyntax>().Single();

            // insert before first
            var newNode = invocation.InsertNodesBefore(invocation.ArgumentList.Arguments[0], new SyntaxNode[] { argC, argD });
            newNode.ToFullString().Should().Be("m(c,d,a, b)");
            var newTree = newNode.SyntaxTree;
            newTree.Options.Kind.Should().Be(SourceCodeKind.Script);
            newTree.Options.Should().Be(tree.Options);

            // insert after first
            var newNode2 = invocation.InsertNodesAfter(invocation.ArgumentList.Arguments[0], new SyntaxNode[] { argC, argD });
            newNode2.ToFullString().Should().Be("m(a,c,d, b)");
            var newTree2 = newNode2.SyntaxTree;
            newTree2.Options.Kind.Should().Be(SourceCodeKind.Script);
            newTree2.Options.Should().Be(tree.Options);
        }

        [Fact]
        [WorkItem(22010, "https://github.com/dotnet/roslyn/issues/22010")]
        public void TestReplaceTokenShouldNotLoseParseOptions()
        {
            var tree = SyntaxFactory.ParseSyntaxTree("private class C { }", options: TestOptions.Script);
            tree.Options.Kind.Should().Be(SourceCodeKind.Script);

            var root = tree.GetRoot();
            var privateToken = root.DescendantTokens().First();
            var publicToken = SyntaxFactory.ParseToken("public ");
            var partialToken = SyntaxFactory.ParseToken("partial ");

            var newRoot = root.ReplaceToken(privateToken, new[] { publicToken, partialToken });
            newRoot.ToFullString().Should().Be("public partial class C { }");

            var newTree = newRoot.SyntaxTree;
            newTree.Options.Kind.Should().Be(SourceCodeKind.Script);
            newTree.Options.Should().Be(tree.Options);
        }

        [Fact]
        [WorkItem(22010, "https://github.com/dotnet/roslyn/issues/22010")]
        public void TestInsertTokenShouldNotLoseParseOptions()
        {
            var tree = SyntaxFactory.ParseSyntaxTree("public class C { }", options: TestOptions.Script);
            var root = tree.GetRoot();
            var publicToken = root.DescendantTokens().First();
            var partialToken = SyntaxFactory.ParseToken("partial ");
            var staticToken = SyntaxFactory.ParseToken("static ");

            var newRoot = root.InsertTokensBefore(publicToken, new[] { staticToken });
            newRoot.ToFullString().Should().Be("static public class C { }");
            var newTree = newRoot.SyntaxTree;
            newTree.Options.Kind.Should().Be(SourceCodeKind.Script);
            newTree.Options.Should().Be(tree.Options);

            var newRoot2 = root.InsertTokensAfter(publicToken, new[] { staticToken });
            newRoot2.ToFullString().Should().Be("public static class C { }");
            var newTree2 = newRoot2.SyntaxTree;
            newTree2.Options.Kind.Should().Be(SourceCodeKind.Script);
            newTree2.Options.Should().Be(tree.Options);
        }

        [Fact]
        [WorkItem(22010, "https://github.com/dotnet/roslyn/issues/22010")]
        public void TestReplaceTriviaShouldNotLoseParseOptions()
        {
            var tree = SyntaxFactory.ParseSyntaxTree("/* c */ identifier", options: TestOptions.Script);
            var root = tree.GetRoot();
            var leadingTrivia = root.GetLeadingTrivia();
            leadingTrivia.Count.Should().Be(2);
            var comment1 = leadingTrivia[0];
            comment1.Kind().Should().Be(SyntaxKind.MultiLineCommentTrivia);

            var newComment1 = SyntaxFactory.ParseLeadingTrivia("/* a */")[0];
            var newComment2 = SyntaxFactory.ParseLeadingTrivia("/* b */")[0];

            var newRoot = root.ReplaceTrivia(comment1, new[] { newComment1, newComment2 });
            newRoot.ToFullString().Should().Be("/* a *//* b */ identifier");
            var newTree = newRoot.SyntaxTree;
            newTree.Options.Kind.Should().Be(SourceCodeKind.Script);
            newTree.Options.Should().Be(tree.Options);

            var newRoot2 = root.ReplaceTrivia(comment1, new SyntaxTrivia[] { });
            newRoot2.ToFullString().Should().Be(" identifier");
            var newTree2 = newRoot2.SyntaxTree;
            newTree2.Options.Kind.Should().Be(SourceCodeKind.Script);
            newTree2.Options.Should().Be(tree.Options);
        }

        [Fact]
        [WorkItem(22010, "https://github.com/dotnet/roslyn/issues/22010")]
        public void TestInsertTriviaShouldNotLoseParseOptions()
        {
            var tree = SyntaxFactory.ParseSyntaxTree("/* c */ identifier", options: TestOptions.Script);
            var root = tree.GetRoot();
            var leadingTrivia = root.GetLeadingTrivia();
            leadingTrivia.Count.Should().Be(2);
            var comment1 = leadingTrivia[0];
            comment1.Kind().Should().Be(SyntaxKind.MultiLineCommentTrivia);

            var newComment1 = SyntaxFactory.ParseLeadingTrivia("/* a */")[0];
            var newComment2 = SyntaxFactory.ParseLeadingTrivia("/* b */")[0];

            var newRoot = root.InsertTriviaAfter(comment1, new[] { newComment1, newComment2 });
            newRoot.ToFullString().Should().Be("/* c *//* a *//* b */ identifier");

            var newTree = newRoot.SyntaxTree;
            newTree.Options.Kind.Should().Be(SourceCodeKind.Script);
            newTree.Options.Should().Be(tree.Options);
        }

        [Fact]
        [WorkItem(22010, "https://github.com/dotnet/roslyn/issues/22010")]
        public void TestRemoveNodeShouldNotLoseParseOptions()
        {
            var tree = SyntaxFactory.ParseSyntaxTree("private class C { }", options: TestOptions.Script);
            var root = tree.GetRoot();
            var newRoot = root.RemoveNode(root.DescendantNodes().First(), SyntaxRemoveOptions.KeepDirectives);

            var newTree = newRoot.SyntaxTree;
            newTree.Options.Kind.Should().Be(SourceCodeKind.Script);
            newTree.Options.Should().Be(tree.Options);
        }

        [Fact]
        [WorkItem(22010, "https://github.com/dotnet/roslyn/issues/22010")]
        public void TestNormalizeWhitespaceShouldNotLoseParseOptions()
        {
            var tree = SyntaxFactory.ParseSyntaxTree("private class C { }", options: TestOptions.Script);
            var root = tree.GetRoot();
            var newRoot = root.NormalizeWhitespace("  ");

            var newTree = newRoot.SyntaxTree;
            newTree.Options.Kind.Should().Be(SourceCodeKind.Script);
            newTree.Options.Should().Be(tree.Options);
        }

        [Fact]
        public void TestSyntaxTreeForRewrittenRoot()
        {
            var tree = SyntaxFactory.ParseSyntaxTree("class Class1<T> { }");
            tree.GetCompilationUnitRoot().SyntaxTree.Should().NotBeNull();
            var rewriter = new BadRewriter();
            var rewrittenRoot = rewriter.Visit(tree.GetCompilationUnitRoot());
            rewrittenRoot.SyntaxTree.Should().NotBeNull();
            ((SyntaxTree)rewrittenRoot.SyntaxTree).HasCompilationUnitRoot.Should().BeTrue("how did we get a non-CompilationUnit root?");
            rewrittenRoot.SyntaxTree.GetRoot().Should().BeSameAs(rewrittenRoot);
        }

        [WorkItem(545049, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/545049")]
        [WorkItem(896538, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/896538")]
        [Fact]
        public void RewriteMissingIdentifierInExpressionStatement_ImplicitlyCreatedSyntaxTree()
        {
            var ifStmt1 = (IfStatementSyntax)SyntaxFactory.ParseStatement("if (true)");
            var exprStmt1 = (ExpressionStatementSyntax)ifStmt1.Statement;
            var expr1 = (IdentifierNameSyntax)exprStmt1.Expression;
            var token1 = expr1.Identifier;

            expr1.SyntaxTree.Should().NotBeNull();
            expr1.SyntaxTree.HasCompilationUnitRoot.Should().BeFalse("how did we get a CompilationUnit root?");
            expr1.SyntaxTree.GetRoot().Should().BeSameAs(ifStmt1);
            expr1.IsMissing.Should().BeTrue();
            expr1.ContainsDiagnostics.Should().BeTrue();

            token1.IsMissing.Should().BeTrue();
            token1.ContainsDiagnostics.Should().BeFalse();

            var trivia = SyntaxFactory.ParseTrailingTrivia(" ");
            var rewriter = new RedRewriter(rewriteToken: tok => tok.Kind() == SyntaxKind.IdentifierToken ? tok.WithLeadingTrivia(trivia) : tok);

            var ifStmt2 = (IfStatementSyntax)rewriter.Visit(ifStmt1);
            var exprStmt2 = (ExpressionStatementSyntax)ifStmt2.Statement;
            var expr2 = (IdentifierNameSyntax)exprStmt2.Expression;
            var token2 = expr2.Identifier;

            expr2.Should().NotBe(expr1);
            expr2.SyntaxTree.Should().NotBeNull();
            expr2.SyntaxTree.HasCompilationUnitRoot.Should().BeFalse("how did we get a CompilationUnit root?");
            expr2.SyntaxTree.GetRoot().Should().BeSameAs(ifStmt2);
            expr2.IsMissing.Should().BeTrue();
            expr2.ContainsDiagnostics.Should().BeFalse(); //gone after rewrite

            token2.Should().NotBe(token1);
            token2.IsMissing.Should().BeTrue();
            token2.ContainsDiagnostics.Should().BeFalse();

            IsStatementExpression(expr1).Should().BeTrue();
            IsStatementExpression(expr2).Should().BeTrue();
        }

        internal static bool IsStatementExpression(CSharpSyntaxNode expression)
        {
            return SyntaxFacts.IsStatementExpression(expression);
        }

        [WorkItem(545049, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/545049")]
        [WorkItem(896538, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/896538")]
        [Fact]
        public void RewriteMissingIdentifierInExpressionStatement_WithSyntaxTree()
        {
            var tree1 = SyntaxFactory.ParseSyntaxTree("class C { static void Main() { if (true) } }");
            var ifStmt1 = tree1.GetCompilationUnitRoot().DescendantNodes().OfType<IfStatementSyntax>().Single();
            var exprStmt1 = (ExpressionStatementSyntax)ifStmt1.Statement;
            var expr1 = (IdentifierNameSyntax)exprStmt1.Expression;
            var token1 = expr1.Identifier;

            expr1.SyntaxTree.Should().Be(tree1);
            expr1.IsMissing.Should().BeTrue();
            expr1.ContainsDiagnostics.Should().BeTrue();

            token1.IsMissing.Should().BeTrue();
            token1.ContainsDiagnostics.Should().BeFalse();

            var trivia = SyntaxFactory.ParseTrailingTrivia(" ");
            var rewriter = new RedRewriter(rewriteToken: tok => tok.Kind() == SyntaxKind.IdentifierToken ? tok.WithLeadingTrivia(trivia) : tok);

            var ifStmt2 = (IfStatementSyntax)rewriter.Visit(ifStmt1);
            var exprStmt2 = (ExpressionStatementSyntax)ifStmt2.Statement;
            var expr2 = (IdentifierNameSyntax)exprStmt2.Expression;
            var token2 = expr2.Identifier;

            expr2.Should().NotBe(expr1);
            expr2.SyntaxTree.Should().NotBeNull();
            expr2.SyntaxTree.HasCompilationUnitRoot.Should().BeFalse("how did we get a CompilationUnit root?");
            expr2.SyntaxTree.GetRoot().Should().BeSameAs(ifStmt2);
            expr2.IsMissing.Should().BeTrue();
            expr2.ContainsDiagnostics.Should().BeFalse(); //gone after rewrite

            token2.Should().NotBe(token1);
            token2.IsMissing.Should().BeTrue();
            token2.ContainsDiagnostics.Should().BeFalse();

            IsStatementExpression(expr1).Should().BeTrue();
            IsStatementExpression(expr2).Should().BeTrue();
        }

        [Fact]
        public void RemoveDocCommentNode()
        {
            var oldSource = @"
/// <see cref='C'/>
class C { }
";

            var expectedNewSource = @"
/// 
class C { }
";

            var oldTree = CSharpTestBase.Parse(oldSource, options: TestOptions.RegularWithDocumentationComments);
            var oldRoot = oldTree.GetRoot();
            var xmlNode = oldRoot.DescendantNodes(descendIntoTrivia: true).OfType<XmlEmptyElementSyntax>().Single();
            var newRoot = oldRoot.RemoveNode(xmlNode, SyntaxRemoveOptions.KeepDirectives);

            newRoot.ToFullString().Should().Be(expectedNewSource);
        }

        [WorkItem(991474, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/991474")]
        [Fact]
        public void ReturnNullFromStructuredTriviaRoot_Succeeds()
        {
            var text =
@"#region
class C { }
#endregion";

            var expectedText =
@"class C { }
#endregion";

            var root = SyntaxFactory.ParseCompilationUnit(text);
            var newRoot = new RemoveRegionRewriter().Visit(root);

            newRoot.ToFullString().Should().Be(expectedText);
        }

        private class RemoveRegionRewriter : CSharpSyntaxRewriter
        {
            public RemoveRegionRewriter()
                : base(visitIntoStructuredTrivia: true)
            {
            }

            public override SyntaxNode VisitRegionDirectiveTrivia(RegionDirectiveTriviaSyntax node)
            {
                return null;
            }
        }

        #endregion Misc

        #region Helper Methods

        private static void TestGreen(string input, string output, GreenRewriter rewriter, bool isExpr)
        {
            var red = isExpr ? (CSharpSyntaxNode)SyntaxFactory.ParseExpression(input) : SyntaxFactory.ParseStatement(input);
            var green = red.CsGreen;

            green.ContainsDiagnostics.Should().BeFalse();

            var result = rewriter.Visit(green);

            ReferenceEquals(green, result).Should().Be(input == output);
            result.ToFullString().Should().Be(output);
        }

        private static void TestRed(string input, string output, RedRewriter rewriter, bool isExpr)
        {
            var red = isExpr ? (CSharpSyntaxNode)SyntaxFactory.ParseExpression(input) : SyntaxFactory.ParseStatement(input);

            red.ContainsDiagnostics.Should().BeFalse();

            var result = rewriter.Visit(red);

            ReferenceEquals(red, result).Should().Be(input == output);
            result.ToFullString().Should().Be(output);
        }

        #endregion Helper Methods

        #region Helper Types

        /// <summary>
        /// This Rewriter exposes delegates for the methods that would normally be overridden.
        /// </summary>
        internal class GreenRewriter : InternalSyntax.CSharpSyntaxRewriter
        {
            private readonly Func<InternalSyntax.CSharpSyntaxNode, InternalSyntax.CSharpSyntaxNode> _rewriteNode;
            private readonly Func<InternalSyntax.SyntaxToken, InternalSyntax.SyntaxToken> _rewriteToken;

            internal GreenRewriter(
                Func<InternalSyntax.CSharpSyntaxNode, InternalSyntax.CSharpSyntaxNode> rewriteNode = null,
                Func<InternalSyntax.SyntaxToken, InternalSyntax.SyntaxToken> rewriteToken = null)
            {
                _rewriteNode = rewriteNode;
                _rewriteToken = rewriteToken;
            }

            public override InternalSyntax.CSharpSyntaxNode Visit(InternalSyntax.CSharpSyntaxNode node)
            {
                var visited = base.Visit(node);
                return _rewriteNode == null ? visited : _rewriteNode(visited);
            }

            public override InternalSyntax.CSharpSyntaxNode VisitToken(InternalSyntax.SyntaxToken token)
            {
                var visited = (InternalSyntax.SyntaxToken)base.VisitToken(token);
                return _rewriteToken == null ? visited : _rewriteToken(visited);
            }
        }

        /// <summary>
        /// This Rewriter exposes delegates for the methods that would normally be overridden.
        /// </summary>
        internal class RedRewriter : CSharpSyntaxRewriter
        {
            private readonly Func<SyntaxNode, SyntaxNode> _rewriteNode;
            private readonly Func<SyntaxToken, SyntaxToken> _rewriteToken;
            private readonly Func<SyntaxTrivia, SyntaxTrivia> _rewriteTrivia;

            internal RedRewriter(
                Func<SyntaxNode, SyntaxNode> rewriteNode = null,
                Func<SyntaxToken, SyntaxToken> rewriteToken = null,
                Func<SyntaxTrivia, SyntaxTrivia> rewriteTrivia = null)
            {
                _rewriteNode = rewriteNode;
                _rewriteToken = rewriteToken;
                _rewriteTrivia = rewriteTrivia;
            }

            public override SyntaxNode Visit(SyntaxNode node)
            {
                var visited = base.Visit(node);
                return _rewriteNode == null ? visited : _rewriteNode(visited);
            }

            public override SyntaxToken VisitToken(SyntaxToken token)
            {
                var visited = base.VisitToken(token);
                return _rewriteToken == null ? visited : _rewriteToken(token);
            }

            public override SyntaxTrivia VisitTrivia(SyntaxTrivia trivia)
            {
                var visited = base.VisitTrivia(trivia);
                return _rewriteTrivia == null ? visited : _rewriteTrivia(trivia);
            }
        }

        internal class BadRewriter : CSharpSyntaxRewriter
        {
            public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
            {
                // This is garbage...the identifier can't be "class"...
                return SyntaxFactory.ClassDeclaration(SyntaxFactory.Identifier("class"));
            }
        }

        #endregion Helper Types
    }
}
