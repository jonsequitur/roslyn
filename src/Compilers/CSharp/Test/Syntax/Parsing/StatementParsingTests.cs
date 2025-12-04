// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#nullable disable

using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp.Test.Utilities;
using Microsoft.CodeAnalysis.Test.Utilities;
using Roslyn.Test.Utilities;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.CodeAnalysis.CSharp.UnitTests
{
    public class StatementParsingTests : ParsingTests
    {
        public StatementParsingTests(ITestOutputHelper output) : base(output) { }

        private StatementSyntax ParseStatement(string text, int offset = 0, ParseOptions options = null)
        {
            return SyntaxFactory.ParseStatement(text, offset, options);
        }

        [Fact]
        [WorkItem(17458, "https://github.com/dotnet/roslyn/issues/17458")]
        public void ParsePrivate()
        {
            UsingStatement("private",
                // (1,1): error CS1073: Unexpected token 'private'
                // private
                Diagnostic(ErrorCode.ERR_UnexpectedToken, "").WithArguments("private").WithLocation(1, 1),
                // (1,1): error CS1525: Invalid expression term 'private'
                // private
                Diagnostic(ErrorCode.ERR_InvalidExprTerm, "private").WithArguments("private").WithLocation(1, 1),
                // (1,1): error CS1002: ; expected
                // private
                Diagnostic(ErrorCode.ERR_SemicolonExpected, "private").WithLocation(1, 1)
                );
            M(SyntaxKind.ExpressionStatement);
            {
                M(SyntaxKind.IdentifierName);
                {
                    M(SyntaxKind.IdentifierToken);
                }
                M(SyntaxKind.SemicolonToken);
            }
            EOF();
        }

        [Fact]
        public void TestName()
        {
            var text = "a();";
            var statement = this.ParseStatement(text);

            statement.Should().NotBeNull();
            statement.Kind().Should().Be(SyntaxKind.ExpressionStatement);
            statement.ToString().Should().Be(text);
            statement.Errors().Length.Should().Be(0);

            var es = (ExpressionStatementSyntax)statement;
            es.Expression.Should().NotBeNull();
            es.Expression.Kind().Should().Be(SyntaxKind.InvocationExpression);
            ((InvocationExpressionSyntax)es.Expression).Expression.Kind().Should().Be(SyntaxKind.IdentifierName);
            es.Expression.ToString().Should().Be("a()");
            es.SemicolonToken.Should().NotBe(default);
            es.SemicolonToken.IsMissing.Should().BeFalse();
        }

        [Fact]
        public void TestDottedName()
        {
            var text = "a.b();";
            var statement = this.ParseStatement(text);

            statement.Should().NotBeNull();
            statement.Kind().Should().Be(SyntaxKind.ExpressionStatement);
            statement.ToString().Should().Be(text);
            statement.Errors().Length.Should().Be(0);

            var es = (ExpressionStatementSyntax)statement;
            es.Expression.Should().NotBeNull();
            es.Expression.Kind().Should().Be(SyntaxKind.InvocationExpression);
            ((InvocationExpressionSyntax)es.Expression).Expression.Kind().Should().Be(SyntaxKind.SimpleMemberAccessExpression);
            es.Expression.ToString().Should().Be("a.b()");
            es.SemicolonToken.Should().NotBe(default);
            es.SemicolonToken.IsMissing.Should().BeFalse();
        }

        [Fact]
        public void TestGenericName()
        {
            var text = "a<b>();";
            var statement = this.ParseStatement(text);

            statement.Should().NotBeNull();
            statement.Kind().Should().Be(SyntaxKind.ExpressionStatement);
            statement.ToString().Should().Be(text);
            statement.Errors().Length.Should().Be(0);
            var es = (ExpressionStatementSyntax)statement;
            es.Expression.Should().NotBeNull();
            es.Expression.Kind().Should().Be(SyntaxKind.InvocationExpression);
            ((InvocationExpressionSyntax)es.Expression).Expression.Kind().Should().Be(SyntaxKind.GenericName);
            es.Expression.ToString().Should().Be("a<b>()");
            es.SemicolonToken.Should().NotBe(default);
            es.SemicolonToken.IsMissing.Should().BeFalse();
        }

        [Fact]
        public void TestGenericDotName()
        {
            var text = "a<b>.c();";
            var statement = this.ParseStatement(text);

            statement.Should().NotBeNull();
            statement.Kind().Should().Be(SyntaxKind.ExpressionStatement);
            statement.ToString().Should().Be(text);
            statement.Errors().Length.Should().Be(0);

            var es = (ExpressionStatementSyntax)statement;
            es.Expression.Should().NotBeNull();
            es.Expression.Kind().Should().Be(SyntaxKind.InvocationExpression);
            ((InvocationExpressionSyntax)es.Expression).Expression.Kind().Should().Be(SyntaxKind.SimpleMemberAccessExpression);
            es.Expression.ToString().Should().Be("a<b>.c()");
            es.SemicolonToken.Should().NotBe(default);
            es.SemicolonToken.IsMissing.Should().BeFalse();
        }

        [Fact]
        public void TestDotGenericName()
        {
            var text = "a.b<c>();";
            var statement = this.ParseStatement(text);

            statement.Should().NotBeNull();
            statement.Kind().Should().Be(SyntaxKind.ExpressionStatement);
            statement.ToString().Should().Be(text);
            statement.Errors().Length.Should().Be(0);

            var es = (ExpressionStatementSyntax)statement;
            es.Expression.Should().NotBeNull();
            es.Expression.Kind().Should().Be(SyntaxKind.InvocationExpression);
            ((InvocationExpressionSyntax)es.Expression).Expression.Kind().Should().Be(SyntaxKind.SimpleMemberAccessExpression);
            es.Expression.ToString().Should().Be("a.b<c>()");
            es.SemicolonToken.Should().NotBe(default);
            es.SemicolonToken.IsMissing.Should().BeFalse();
        }

        private void TestPostfixUnaryOperator(SyntaxKind kind, ParseOptions options = null)
        {
            var text = "a" + SyntaxFacts.GetText(kind) + ";";
            var statement = this.ParseStatement(text, options: options);

            statement.Should().NotBeNull();
            statement.Kind().Should().Be(SyntaxKind.ExpressionStatement);
            statement.ToString().Should().Be(text);
            statement.Errors().Length.Should().Be(0);

            var es = (ExpressionStatementSyntax)statement;
            es.Expression.Should().NotBeNull();
            es.SemicolonToken.Should().NotBe(default);
            es.SemicolonToken.IsMissing.Should().BeFalse();

            var opKind = SyntaxFacts.GetPostfixUnaryExpression(kind);
            es.Expression.Kind().Should().Be(opKind);
            var us = (PostfixUnaryExpressionSyntax)es.Expression;
            us.Operand.ToString().Should().Be("a");
            us.OperatorToken.Kind().Should().Be(kind);
        }

        [Fact]
        public void TestPostfixUnaryOperators()
        {
            TestPostfixUnaryOperator(SyntaxKind.PlusPlusToken);
            TestPostfixUnaryOperator(SyntaxKind.MinusMinusToken);
            TestPostfixUnaryOperator(SyntaxKind.ExclamationToken, TestOptions.Regular8);
        }

        [Fact]
        public void TestLocalDeclarationStatement()
        {
            var text = "T a;";
            var statement = this.ParseStatement(text);

            statement.Should().NotBeNull();
            statement.Kind().Should().Be(SyntaxKind.LocalDeclarationStatement);
            statement.ToString().Should().Be(text);
            statement.Errors().Length.Should().Be(0);

            var ds = (LocalDeclarationStatementSyntax)statement;
            ds.Modifiers.Count.Should().Be(0);
            ds.Declaration.Type.Should().NotBeNull();
            ds.Declaration.Type.ToString().Should().Be("T");
            ds.Declaration.Variables.Count.Should().Be(1);

            ds.Declaration.Variables[0].Identifier.Should().NotBe(default);
            ds.Declaration.Variables[0].Identifier.ToString().Should().Be("a");
            ds.Declaration.Variables[0].ArgumentList.Should().BeNull();
            ds.Declaration.Variables[0].Initializer.Should().BeNull();

            ds.SemicolonToken.Should().NotBe(default);
            ds.SemicolonToken.IsMissing.Should().BeFalse();
        }

        [Fact]
        public void TestLocalDeclarationStatementWithVar()
        {
            // note: semantically this would require an initializer, but we don't know 
            // about var being special until we bind.
            var text = "var a;";
            var statement = this.ParseStatement(text);

            statement.Should().NotBeNull();
            statement.Kind().Should().Be(SyntaxKind.LocalDeclarationStatement);
            statement.ToString().Should().Be(text);
            statement.Errors().Length.Should().Be(0);

            var ds = (LocalDeclarationStatementSyntax)statement;
            ds.Modifiers.Count.Should().Be(0);
            ds.Declaration.Type.Should().NotBeNull();
            ds.Declaration.Type.ToString().Should().Be("var");
            ds.Declaration.Type.Kind().Should().Be(SyntaxKind.IdentifierName);
            ((IdentifierNameSyntax)ds.Declaration.Type).Identifier.Kind().Should().Be(SyntaxKind.IdentifierToken);
            ds.Declaration.Variables.Count.Should().Be(1);

            ds.Declaration.Variables[0].Identifier.Should().NotBe(default);
            ds.Declaration.Variables[0].Identifier.ToString().Should().Be("a");
            ds.Declaration.Variables[0].ArgumentList.Should().BeNull();
            ds.Declaration.Variables[0].Initializer.Should().BeNull();

            ds.SemicolonToken.Should().NotBe(default);
            ds.SemicolonToken.IsMissing.Should().BeFalse();
        }

        [Fact]
        public void TestLocalDeclarationStatementWithTuple()
        {
            var text = "(int, int) a;";
            var statement = this.ParseStatement(text, options: TestOptions.Regular);

            (text).ToString();

            statement.Should().NotBeNull();
            statement.Kind().Should().Be(SyntaxKind.LocalDeclarationStatement);
            statement.ToString().Should().Be(text);
            statement.Errors().Length.Should().Be(0);

            var ds = (LocalDeclarationStatementSyntax)statement;
            ds.Modifiers.Count.Should().Be(0);
            ds.Declaration.Type.Should().NotBeNull();
            ds.Declaration.Type.ToString().Should().Be("(int, int)");
            ds.Declaration.Type.Kind().Should().Be(SyntaxKind.TupleType);

            var tt = (TupleTypeSyntax)ds.Declaration.Type;

            tt.Elements[0].Type.Kind().Should().Be(SyntaxKind.PredefinedType);
            tt.Elements[1].Identifier.Kind().Should().Be(SyntaxKind.None);
            tt.Elements.Count.Should().Be(2);

            ds.Declaration.Variables[0].Identifier.Should().NotBe(default);
            ds.Declaration.Variables[0].Identifier.ToString().Should().Be("a");
            ds.Declaration.Variables[0].ArgumentList.Should().BeNull();
            ds.Declaration.Variables[0].Initializer.Should().BeNull();

            ds.SemicolonToken.Should().NotBe(default);
            ds.SemicolonToken.IsMissing.Should().BeFalse();
        }

        [Fact]
        public void TestLocalDeclarationStatementWithNamedTuple()
        {
            var text = "(T x, (U k, V l, W m) y) a;";
            var statement = this.ParseStatement(text);

            (text).ToString();

            statement.Should().NotBeNull();
            statement.Kind().Should().Be(SyntaxKind.LocalDeclarationStatement);
            statement.ToString().Should().Be(text);
            statement.Errors().Length.Should().Be(0);

            var ds = (LocalDeclarationStatementSyntax)statement;
            ds.Modifiers.Count.Should().Be(0);
            ds.Declaration.Type.Should().NotBeNull();
            ds.Declaration.Type.ToString().Should().Be("(T x, (U k, V l, W m) y)");
            ds.Declaration.Type.Kind().Should().Be(SyntaxKind.TupleType);

            var tt = (TupleTypeSyntax)ds.Declaration.Type;

            tt.Elements[0].Type.Kind().Should().Be(SyntaxKind.IdentifierName);
            tt.Elements[1].Identifier.ToString().Should().Be("y");
            tt.Elements.Count.Should().Be(2);

            tt = (TupleTypeSyntax)tt.Elements[1].Type;

            tt.ToString().Should().Be("(U k, V l, W m)");
            tt.Elements[0].Type.Kind().Should().Be(SyntaxKind.IdentifierName);
            tt.Elements[1].Identifier.ToString().Should().Be("l");
            tt.Elements.Count.Should().Be(3);

            ds.Declaration.Variables[0].Identifier.Should().NotBe(default);
            ds.Declaration.Variables[0].Identifier.ToString().Should().Be("a");
            ds.Declaration.Variables[0].ArgumentList.Should().BeNull();
            ds.Declaration.Variables[0].Initializer.Should().BeNull();

            ds.SemicolonToken.Should().NotBe(default);
            ds.SemicolonToken.IsMissing.Should().BeFalse();
        }

        [Fact]
        public void TestLocalDeclarationStatementWithDynamic()
        {
            // note: semantically this would require an initializer, but we don't know 
            // about dynamic being special until we bind.
            var text = "dynamic a;";
            var statement = this.ParseStatement(text);

            statement.Should().NotBeNull();
            statement.Kind().Should().Be(SyntaxKind.LocalDeclarationStatement);
            statement.ToString().Should().Be(text);
            statement.Errors().Length.Should().Be(0);

            var ds = (LocalDeclarationStatementSyntax)statement;
            ds.Modifiers.Count.Should().Be(0);
            ds.Declaration.Type.Should().NotBeNull();
            ds.Declaration.Type.ToString().Should().Be("dynamic");
            ds.Declaration.Type.Kind().Should().Be(SyntaxKind.IdentifierName);
            ((IdentifierNameSyntax)ds.Declaration.Type).Identifier.Kind().Should().Be(SyntaxKind.IdentifierToken);
            ds.Declaration.Variables.Count.Should().Be(1);

            ds.Declaration.Variables[0].Identifier.Should().NotBe(default);
            ds.Declaration.Variables[0].Identifier.ToString().Should().Be("a");
            ds.Declaration.Variables[0].ArgumentList.Should().BeNull();
            ds.Declaration.Variables[0].Initializer.Should().BeNull();

            ds.SemicolonToken.Should().NotBe(default);
            ds.SemicolonToken.IsMissing.Should().BeFalse();
        }

        [Fact]
        public void TestLocalDeclarationStatementWithGenericType()
        {
            var text = "T<a> b;";
            var statement = this.ParseStatement(text);

            statement.Should().NotBeNull();
            statement.Kind().Should().Be(SyntaxKind.LocalDeclarationStatement);
            statement.ToString().Should().Be(text);
            statement.Errors().Length.Should().Be(0);

            var ds = (LocalDeclarationStatementSyntax)statement;
            ds.Modifiers.Count.Should().Be(0);
            ds.Declaration.Type.Should().NotBeNull();
            ds.Declaration.Type.ToString().Should().Be("T<a>");
            ds.Declaration.Variables.Count.Should().Be(1);

            ds.Declaration.Variables[0].Identifier.Should().NotBe(default);
            ds.Declaration.Variables[0].Identifier.ToString().Should().Be("b");
            ds.Declaration.Variables[0].ArgumentList.Should().BeNull();
            ds.Declaration.Variables[0].Initializer.Should().BeNull();

            ds.SemicolonToken.Should().NotBe(default);
            ds.SemicolonToken.IsMissing.Should().BeFalse();
        }

        [Fact]
        public void TestLocalDeclarationStatementWithDottedType()
        {
            var text = "T.X.Y a;";
            var statement = this.ParseStatement(text);

            statement.Should().NotBeNull();
            statement.Kind().Should().Be(SyntaxKind.LocalDeclarationStatement);
            statement.ToString().Should().Be(text);
            statement.Errors().Length.Should().Be(0);

            var ds = (LocalDeclarationStatementSyntax)statement;
            ds.Modifiers.Count.Should().Be(0);
            ds.Declaration.Type.Should().NotBeNull();
            ds.Declaration.Type.ToString().Should().Be("T.X.Y");
            ds.Declaration.Variables.Count.Should().Be(1);

            ds.Declaration.Variables[0].Identifier.Should().NotBe(default);
            ds.Declaration.Variables[0].Identifier.ToString().Should().Be("a");
            ds.Declaration.Variables[0].ArgumentList.Should().BeNull();
            ds.Declaration.Variables[0].Initializer.Should().BeNull();

            ds.SemicolonToken.Should().NotBe(default);
            ds.SemicolonToken.IsMissing.Should().BeFalse();
        }

        [Fact]
        public void TestLocalDeclarationStatementWithMixedType()
        {
            var text = "T<t>.X<x>.Y<y> a;";
            var statement = this.ParseStatement(text);

            statement.Should().NotBeNull();
            statement.Kind().Should().Be(SyntaxKind.LocalDeclarationStatement);
            statement.ToString().Should().Be(text);
            statement.Errors().Length.Should().Be(0);

            var ds = (LocalDeclarationStatementSyntax)statement;
            ds.Modifiers.Count.Should().Be(0);
            ds.Declaration.Type.Should().NotBeNull();
            ds.Declaration.Type.ToString().Should().Be("T<t>.X<x>.Y<y>");
            ds.Declaration.Variables.Count.Should().Be(1);

            ds.Declaration.Variables[0].Identifier.Should().NotBe(default);
            ds.Declaration.Variables[0].Identifier.ToString().Should().Be("a");
            ds.Declaration.Variables[0].ArgumentList.Should().BeNull();
            ds.Declaration.Variables[0].Initializer.Should().BeNull();

            ds.SemicolonToken.Should().NotBe(default);
            ds.SemicolonToken.IsMissing.Should().BeFalse();
        }

        [Fact]
        public void TestLocalDeclarationStatementWithArrayType()
        {
            var text = "T[][,][,,] a;";
            var statement = this.ParseStatement(text);

            statement.Should().NotBeNull();
            statement.Kind().Should().Be(SyntaxKind.LocalDeclarationStatement);
            statement.ToString().Should().Be(text);
            statement.Errors().Length.Should().Be(0);

            var ds = (LocalDeclarationStatementSyntax)statement;
            ds.Modifiers.Count.Should().Be(0);
            ds.Declaration.Type.Should().NotBeNull();
            ds.Declaration.Type.ToString().Should().Be("T[][,][,,]");
            ds.Declaration.Variables.Count.Should().Be(1);

            ds.Declaration.Variables[0].Identifier.Should().NotBe(default);
            ds.Declaration.Variables[0].Identifier.ToString().Should().Be("a");
            ds.Declaration.Variables[0].ArgumentList.Should().BeNull();
            ds.Declaration.Variables[0].Initializer.Should().BeNull();

            ds.SemicolonToken.Should().NotBe(default);
            ds.SemicolonToken.IsMissing.Should().BeFalse();
        }

        [Fact]
        public void TestLocalDeclarationStatementWithPointerType()
        {
            var text = "T* a;";
            var statement = this.ParseStatement(text);

            statement.Should().NotBeNull();
            statement.Kind().Should().Be(SyntaxKind.LocalDeclarationStatement);
            statement.ToString().Should().Be(text);
            statement.Errors().Length.Should().Be(0);

            var ds = (LocalDeclarationStatementSyntax)statement;
            ds.Modifiers.Count.Should().Be(0);
            ds.Declaration.Type.Should().NotBeNull();
            ds.Declaration.Type.ToString().Should().Be("T*");
            ds.Declaration.Variables.Count.Should().Be(1);

            ds.Declaration.Variables[0].Identifier.Should().NotBe(default);
            ds.Declaration.Variables[0].Identifier.ToString().Should().Be("a");
            ds.Declaration.Variables[0].ArgumentList.Should().BeNull();
            ds.Declaration.Variables[0].Initializer.Should().BeNull();

            ds.SemicolonToken.Should().NotBe(default);
            ds.SemicolonToken.IsMissing.Should().BeFalse();
        }

        [Fact]
        public void TestLocalDeclarationStatementWithNullableType()
        {
            var text = "T? a;";
            var statement = this.ParseStatement(text);

            statement.Should().NotBeNull();
            statement.Kind().Should().Be(SyntaxKind.LocalDeclarationStatement);
            statement.ToString().Should().Be(text);
            statement.Errors().Length.Should().Be(0);

            var ds = (LocalDeclarationStatementSyntax)statement;
            ds.Modifiers.Count.Should().Be(0);
            ds.Declaration.Type.Should().NotBeNull();
            ds.Declaration.Type.ToString().Should().Be("T?");
            ds.Declaration.Variables.Count.Should().Be(1);

            ds.Declaration.Variables[0].Identifier.Should().NotBe(default);
            ds.Declaration.Variables[0].Identifier.ToString().Should().Be("a");
            ds.Declaration.Variables[0].ArgumentList.Should().BeNull();
            ds.Declaration.Variables[0].Initializer.Should().BeNull();

            ds.SemicolonToken.Should().NotBe(default);
            ds.SemicolonToken.IsMissing.Should().BeFalse();
        }

        [Fact]
        public void TestLocalDeclarationStatementWithMultipleVariables()
        {
            var text = "T a, b, c;";
            var statement = this.ParseStatement(text);

            statement.Should().NotBeNull();
            statement.Kind().Should().Be(SyntaxKind.LocalDeclarationStatement);
            statement.ToString().Should().Be(text);
            statement.Errors().Length.Should().Be(0);

            var ds = (LocalDeclarationStatementSyntax)statement;
            ds.Modifiers.Count.Should().Be(0);
            ds.Declaration.Type.Should().NotBeNull();
            ds.Declaration.Type.ToString().Should().Be("T");
            ds.Declaration.Variables.Count.Should().Be(3);

            ds.Declaration.Variables[0].Identifier.Should().NotBe(default);
            ds.Declaration.Variables[0].Identifier.ToString().Should().Be("a");
            ds.Declaration.Variables[0].ArgumentList.Should().BeNull();
            ds.Declaration.Variables[0].Initializer.Should().BeNull();

            ds.Declaration.Variables[1].Identifier.Should().NotBe(default);
            ds.Declaration.Variables[1].Identifier.ToString().Should().Be("b");
            ds.Declaration.Variables[1].ArgumentList.Should().BeNull();
            ds.Declaration.Variables[1].Initializer.Should().BeNull();

            ds.Declaration.Variables[2].Identifier.Should().NotBe(default);
            ds.Declaration.Variables[2].Identifier.ToString().Should().Be("c");
            ds.Declaration.Variables[2].Initializer.Should().BeNull();

            ds.SemicolonToken.Should().NotBe(default);
            ds.SemicolonToken.IsMissing.Should().BeFalse();
        }

        [Fact]
        public void TestLocalDeclarationStatementWithInitializer()
        {
            var text = "T a = b;";
            var statement = this.ParseStatement(text);

            statement.Should().NotBeNull();
            statement.Kind().Should().Be(SyntaxKind.LocalDeclarationStatement);
            statement.ToString().Should().Be(text);
            statement.Errors().Length.Should().Be(0);

            var ds = (LocalDeclarationStatementSyntax)statement;
            ds.Modifiers.Count.Should().Be(0);
            ds.Declaration.Type.Should().NotBeNull();
            ds.Declaration.Type.ToString().Should().Be("T");
            ds.Declaration.Variables.Count.Should().Be(1);

            ds.Declaration.Variables[0].Identifier.Should().NotBe(default);
            ds.Declaration.Variables[0].Identifier.ToString().Should().Be("a");
            ds.Declaration.Variables[0].ArgumentList.Should().BeNull();
            ds.Declaration.Variables[0].Initializer.Should().NotBeNull();
            ds.Declaration.Variables[0].Initializer.EqualsToken.Should().NotBe(default);
            ds.Declaration.Variables[0].Initializer.EqualsToken.IsMissing.Should().BeFalse();
            ds.Declaration.Variables[0].Initializer.Value.Should().NotBeNull();
            ds.Declaration.Variables[0].Initializer.Value.ToString().Should().Be("b");

            ds.SemicolonToken.Should().NotBe(default);
            ds.SemicolonToken.IsMissing.Should().BeFalse();
        }

        [Fact]
        public void TestLocalDeclarationStatementWithMultipleVariablesAndInitializers()
        {
            var text = "T a = va, b = vb, c = vc;";
            var statement = this.ParseStatement(text);

            statement.Should().NotBeNull();
            statement.Kind().Should().Be(SyntaxKind.LocalDeclarationStatement);
            statement.ToString().Should().Be(text);
            statement.Errors().Length.Should().Be(0);

            var ds = (LocalDeclarationStatementSyntax)statement;
            ds.Modifiers.Count.Should().Be(0);
            ds.Declaration.Type.Should().NotBeNull();
            ds.Declaration.Type.ToString().Should().Be("T");
            ds.Declaration.Variables.Count.Should().Be(3);

            ds.Declaration.Variables[0].Identifier.Should().NotBe(default);
            ds.Declaration.Variables[0].Identifier.ToString().Should().Be("a");
            ds.Declaration.Variables[0].ArgumentList.Should().BeNull();
            ds.Declaration.Variables[0].Initializer.EqualsToken.Should().NotBe(default);
            ds.Declaration.Variables[0].Initializer.EqualsToken.IsMissing.Should().BeFalse();
            ds.Declaration.Variables[0].Initializer.Value.Should().NotBeNull();
            ds.Declaration.Variables[0].Initializer.Value.ToString().Should().Be("va");

            ds.Declaration.Variables[1].Identifier.Should().NotBe(default);
            ds.Declaration.Variables[1].Identifier.ToString().Should().Be("b");
            ds.Declaration.Variables[1].ArgumentList.Should().BeNull();
            ds.Declaration.Variables[1].Initializer.EqualsToken.Should().NotBe(default);
            ds.Declaration.Variables[1].Initializer.EqualsToken.IsMissing.Should().BeFalse();
            ds.Declaration.Variables[1].Initializer.Value.Should().NotBeNull();
            ds.Declaration.Variables[1].Initializer.Value.ToString().Should().Be("vb");

            ds.Declaration.Variables[2].Identifier.Should().NotBe(default);
            ds.Declaration.Variables[2].Identifier.ToString().Should().Be("c");
            ds.Declaration.Variables[2].ArgumentList.Should().BeNull();
            ds.Declaration.Variables[2].Initializer.EqualsToken.Should().NotBe(default);
            ds.Declaration.Variables[2].Initializer.EqualsToken.IsMissing.Should().BeFalse();
            ds.Declaration.Variables[2].Initializer.Value.Should().NotBeNull();
            ds.Declaration.Variables[2].Initializer.Value.ToString().Should().Be("vc");

            ds.SemicolonToken.Should().NotBe(default);
            ds.SemicolonToken.IsMissing.Should().BeFalse();
        }

        [Fact]
        public void TestLocalDeclarationStatementWithArrayInitializer()
        {
            var text = "T a = {b, c};";
            var statement = this.ParseStatement(text);

            statement.Should().NotBeNull();
            statement.Kind().Should().Be(SyntaxKind.LocalDeclarationStatement);
            statement.ToString().Should().Be(text);
            statement.Errors().Length.Should().Be(0);

            var ds = (LocalDeclarationStatementSyntax)statement;
            ds.Modifiers.Count.Should().Be(0);
            ds.Declaration.Type.Should().NotBeNull();
            ds.Declaration.Type.ToString().Should().Be("T");
            ds.Declaration.Variables.Count.Should().Be(1);

            ds.Declaration.Variables[0].Identifier.Should().NotBe(default);
            ds.Declaration.Variables[0].Identifier.ToString().Should().Be("a");
            ds.Declaration.Variables[0].ArgumentList.Should().BeNull();
            ds.Declaration.Variables[0].Initializer.Should().NotBeNull();
            ds.Declaration.Variables[0].Initializer.EqualsToken.Should().NotBe(default);
            ds.Declaration.Variables[0].Initializer.EqualsToken.IsMissing.Should().BeFalse();
            ds.Declaration.Variables[0].Initializer.Value.Should().NotBeNull();
            ds.Declaration.Variables[0].Initializer.Value.Kind().Should().Be(SyntaxKind.ArrayInitializerExpression);
            ds.Declaration.Variables[0].Initializer.Value.ToString().Should().Be("{b, c}");

            ds.SemicolonToken.Should().NotBe(default);
            ds.SemicolonToken.IsMissing.Should().BeFalse();
        }

        [Fact]
        public void TestConstLocalDeclarationStatement()
        {
            var text = "const T a = b;";
            var statement = this.ParseStatement(text);

            statement.Should().NotBeNull();
            statement.Kind().Should().Be(SyntaxKind.LocalDeclarationStatement);
            statement.ToString().Should().Be(text);
            statement.Errors().Length.Should().Be(0);

            var ds = (LocalDeclarationStatementSyntax)statement;
            ds.Modifiers.Count.Should().Be(1);
            ds.Modifiers[0].Kind().Should().Be(SyntaxKind.ConstKeyword);
            ds.Declaration.Type.Should().NotBeNull();
            ds.Declaration.Type.ToString().Should().Be("T");
            ds.Declaration.Variables.Count.Should().Be(1);

            ds.Declaration.Variables[0].Identifier.Should().NotBe(default);
            ds.Declaration.Variables[0].Identifier.ToString().Should().Be("a");
            ds.Declaration.Variables[0].ArgumentList.Should().BeNull();
            ds.Declaration.Variables[0].Initializer.Should().NotBeNull();
            ds.Declaration.Variables[0].Initializer.EqualsToken.Should().NotBe(default);
            ds.Declaration.Variables[0].Initializer.EqualsToken.IsMissing.Should().BeFalse();
            ds.Declaration.Variables[0].Initializer.Value.Should().NotBeNull();
            ds.Declaration.Variables[0].Initializer.Value.ToString().Should().Be("b");

            ds.SemicolonToken.Should().NotBe(default);
            ds.SemicolonToken.IsMissing.Should().BeFalse();
        }

        [Fact]
        public void TestStaticLocalDeclarationStatement()
        {
            var text = "static T a = b;";
            var statement = this.ParseStatement(text);

            statement.Should().NotBeNull();
            statement.Kind().Should().Be(SyntaxKind.LocalDeclarationStatement);
            statement.ToString().Should().Be(text);
            statement.Errors().Length.Should().Be(1);
            statement.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_BadMemberFlag);

            var ds = (LocalDeclarationStatementSyntax)statement;
            ds.Modifiers.Count.Should().Be(1);
            ds.Modifiers[0].Kind().Should().Be(SyntaxKind.StaticKeyword);
            ds.Declaration.Type.Should().NotBeNull();
            ds.Declaration.Type.ToString().Should().Be("T");
            ds.Declaration.Variables.Count.Should().Be(1);

            ds.Declaration.Variables[0].Identifier.Should().NotBe(default);
            ds.Declaration.Variables[0].Identifier.ToString().Should().Be("a");
            ds.Declaration.Variables[0].ArgumentList.Should().BeNull();
            ds.Declaration.Variables[0].Initializer.Should().NotBeNull();
            ds.Declaration.Variables[0].Initializer.EqualsToken.Should().NotBe(default);
            ds.Declaration.Variables[0].Initializer.EqualsToken.IsMissing.Should().BeFalse();
            ds.Declaration.Variables[0].Initializer.Value.Should().NotBeNull();
            ds.Declaration.Variables[0].Initializer.Value.ToString().Should().Be("b");

            ds.SemicolonToken.Should().NotBe(default);
            ds.SemicolonToken.IsMissing.Should().BeFalse();
        }

        [Fact]
        public void TestReadOnlyLocalDeclarationStatement()
        {
            var text = "readonly T a = b;";
            var statement = this.ParseStatement(text);

            statement.Should().NotBeNull();
            statement.Kind().Should().Be(SyntaxKind.LocalDeclarationStatement);
            statement.ToString().Should().Be(text);
            statement.Errors().Length.Should().Be(1);
            statement.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_BadMemberFlag);

            var ds = (LocalDeclarationStatementSyntax)statement;
            ds.Modifiers.Count.Should().Be(1);
            ds.Modifiers[0].Kind().Should().Be(SyntaxKind.ReadOnlyKeyword);
            ds.Declaration.Type.Should().NotBeNull();
            ds.Declaration.Type.ToString().Should().Be("T");
            ds.Declaration.Variables.Count.Should().Be(1);

            ds.Declaration.Variables[0].Identifier.Should().NotBe(default);
            ds.Declaration.Variables[0].Identifier.ToString().Should().Be("a");
            ds.Declaration.Variables[0].ArgumentList.Should().BeNull();
            ds.Declaration.Variables[0].Initializer.Should().NotBeNull();
            ds.Declaration.Variables[0].Initializer.EqualsToken.Should().NotBe(default);
            ds.Declaration.Variables[0].Initializer.EqualsToken.IsMissing.Should().BeFalse();
            ds.Declaration.Variables[0].Initializer.Value.Should().NotBeNull();
            ds.Declaration.Variables[0].Initializer.Value.ToString().Should().Be("b");

            ds.SemicolonToken.Should().NotBe(default);
            ds.SemicolonToken.IsMissing.Should().BeFalse();
        }

        [Fact]
        public void TestVolatileLocalDeclarationStatement()
        {
            var text = "volatile T a = b;";
            var statement = this.ParseStatement(text);

            statement.Should().NotBeNull();
            statement.Kind().Should().Be(SyntaxKind.LocalDeclarationStatement);
            statement.ToString().Should().Be(text);
            statement.Errors().Length.Should().Be(1);
            statement.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_BadMemberFlag);

            var ds = (LocalDeclarationStatementSyntax)statement;
            ds.Modifiers.Count.Should().Be(1);
            ds.Modifiers[0].Kind().Should().Be(SyntaxKind.VolatileKeyword);
            ds.Declaration.Type.Should().NotBeNull();
            ds.Declaration.Type.ToString().Should().Be("T");
            ds.Declaration.Variables.Count.Should().Be(1);

            ds.Declaration.Variables[0].Identifier.Should().NotBe(default);
            ds.Declaration.Variables[0].Identifier.ToString().Should().Be("a");
            ds.Declaration.Variables[0].ArgumentList.Should().BeNull();
            ds.Declaration.Variables[0].Initializer.Should().NotBeNull();
            ds.Declaration.Variables[0].Initializer.EqualsToken.Should().NotBe(default);
            ds.Declaration.Variables[0].Initializer.EqualsToken.IsMissing.Should().BeFalse();
            ds.Declaration.Variables[0].Initializer.Value.Should().NotBeNull();
            ds.Declaration.Variables[0].Initializer.Value.ToString().Should().Be("b");

            ds.SemicolonToken.Should().NotBe(default);
            ds.SemicolonToken.IsMissing.Should().BeFalse();
        }

        [Fact]
        public void TestRefLocalDeclarationStatement()
        {
            var text = "ref T a;";
            var statement = this.ParseStatement(text);

            statement.Should().NotBeNull();
            statement.Kind().Should().Be(SyntaxKind.LocalDeclarationStatement);
            statement.ToString().Should().Be(text);
            statement.Errors().Length.Should().Be(0);

            var ds = (LocalDeclarationStatementSyntax)statement;
            ds.Modifiers.Count.Should().Be(0);
            ds.Declaration.Type.Should().NotBeNull();
            ds.Declaration.Type.ToString().Should().Be("ref T");
            ds.Declaration.Variables.Count.Should().Be(1);

            ds.Declaration.Variables[0].Identifier.Should().NotBe(default);
            ds.Declaration.Variables[0].Identifier.ToString().Should().Be("a");
            ds.Declaration.Variables[0].ArgumentList.Should().BeNull();
            ds.Declaration.Variables[0].Initializer.Should().BeNull();

            ds.SemicolonToken.Should().NotBe(default);
            ds.SemicolonToken.IsMissing.Should().BeFalse();
        }

        [Fact]
        public void TestRefLocalDeclarationStatementWithInitializer()
        {
            var text = "ref T a = ref b;";
            var statement = this.ParseStatement(text);

            statement.Should().NotBeNull();
            statement.Kind().Should().Be(SyntaxKind.LocalDeclarationStatement);
            statement.ToString().Should().Be(text);
            statement.Errors().Length.Should().Be(0);

            var ds = (LocalDeclarationStatementSyntax)statement;
            ds.Modifiers.Count.Should().Be(0);
            ds.Declaration.Type.Should().NotBeNull();
            ds.Declaration.Type.ToString().Should().Be("ref T");
            ds.Declaration.Variables.Count.Should().Be(1);

            ds.Declaration.Variables[0].Identifier.Should().NotBe(default);
            ds.Declaration.Variables[0].Identifier.ToString().Should().Be("a");
            ds.Declaration.Variables[0].ArgumentList.Should().BeNull();
            var initializer = ds.Declaration.Variables[0].Initializer as EqualsValueClauseSyntax;
            initializer.Should().NotBeNull();
            initializer.EqualsToken.Should().NotBe(default);
            initializer.EqualsToken.IsMissing.Should().BeFalse();
            initializer.Value.Kind().Should().Be(SyntaxKind.RefExpression);
            initializer.Value.ToString().Should().Be("ref b");

            ds.SemicolonToken.Should().NotBe(default);
            ds.SemicolonToken.IsMissing.Should().BeFalse();
        }

        [Fact]
        public void TestRefLocalDeclarationStatementWithMultipleInitializers()
        {
            var text = "ref T a = ref b, c = ref d;";
            var statement = this.ParseStatement(text);

            statement.Should().NotBeNull();
            statement.Kind().Should().Be(SyntaxKind.LocalDeclarationStatement);
            statement.ToString().Should().Be(text);
            statement.Errors().Length.Should().Be(0);

            var ds = (LocalDeclarationStatementSyntax)statement;
            ds.Modifiers.Count.Should().Be(0);
            ds.Declaration.Type.Should().NotBeNull();
            ds.Declaration.Type.ToString().Should().Be("ref T");
            ds.Declaration.Variables.Count.Should().Be(2);

            ds.Declaration.Variables[0].Identifier.Should().NotBe(default);
            ds.Declaration.Variables[0].Identifier.ToString().Should().Be("a");
            ds.Declaration.Variables[0].ArgumentList.Should().BeNull();
            var initializer = ds.Declaration.Variables[0].Initializer as EqualsValueClauseSyntax;
            initializer.Should().NotBeNull();
            initializer.EqualsToken.Should().NotBe(default);
            initializer.EqualsToken.IsMissing.Should().BeFalse();
            initializer.Value.Kind().Should().Be(SyntaxKind.RefExpression);
            initializer.Value.ToString().Should().Be("ref b");

            ds.Declaration.Variables[1].Identifier.Should().NotBe(default);
            ds.Declaration.Variables[1].Identifier.ToString().Should().Be("c");
            ds.Declaration.Variables[1].ArgumentList.Should().BeNull();
            initializer = ds.Declaration.Variables[1].Initializer as EqualsValueClauseSyntax;
            initializer.Should().NotBeNull();
            initializer.EqualsToken.Should().NotBe(default);
            initializer.EqualsToken.IsMissing.Should().BeFalse();
            initializer.Value.Kind().Should().Be(SyntaxKind.RefExpression);
            initializer.Value.ToString().Should().Be("ref d");

            ds.SemicolonToken.Should().NotBe(default);
            ds.SemicolonToken.IsMissing.Should().BeFalse();
        }

        [Fact]
        public void TestFixedStatement()
        {
            var text = "fixed(T a = b) { }";
            var statement = this.ParseStatement(text);

            statement.Should().NotBeNull();
            statement.Kind().Should().Be(SyntaxKind.FixedStatement);
            statement.ToString().Should().Be(text);
            statement.Errors().Length.Should().Be(0);

            var fs = (FixedStatementSyntax)statement;
            fs.FixedKeyword.Should().NotBe(default);
            fs.FixedKeyword.IsMissing.Should().BeFalse();
            fs.OpenParenToken.Should().NotBe(default);
            fs.FixedKeyword.IsMissing.Should().BeFalse();
            fs.Declaration.Should().NotBeNull();
            fs.Declaration.Kind().Should().Be(SyntaxKind.VariableDeclaration);
            fs.Declaration.Type.Should().NotBeNull();
            fs.Declaration.Type.ToString().Should().Be("T");
            fs.Declaration.Variables.Count.Should().Be(1);
            fs.Declaration.Variables[0].ToString().Should().Be("a = b");
            fs.Statement.Should().NotBeNull();
            fs.Statement.Kind().Should().Be(SyntaxKind.Block);
            fs.Statement.ToString().Should().Be("{ }");
        }

        [Fact]
        public void TestFixedVarStatement()
        {
            var text = "fixed(var a = b) { }";
            var statement = this.ParseStatement(text);

            statement.Should().NotBeNull();
            statement.Kind().Should().Be(SyntaxKind.FixedStatement);
            statement.ToString().Should().Be(text);
            statement.Errors().Length.Should().Be(0);

            var fs = (FixedStatementSyntax)statement;
            fs.FixedKeyword.Should().NotBe(default);
            fs.FixedKeyword.IsMissing.Should().BeFalse();
            fs.OpenParenToken.Should().NotBe(default);
            fs.FixedKeyword.IsMissing.Should().BeFalse();
            fs.Declaration.Should().NotBeNull();
            fs.Declaration.Kind().Should().Be(SyntaxKind.VariableDeclaration);
            fs.Declaration.Type.Should().NotBeNull();
            fs.Declaration.Type.ToString().Should().Be("var");
            fs.Declaration.Type.IsVar.Should().BeTrue();
            fs.Declaration.Type.Kind().Should().Be(SyntaxKind.IdentifierName);
            ((IdentifierNameSyntax)fs.Declaration.Type).Identifier.Kind().Should().Be(SyntaxKind.IdentifierToken);
            fs.Declaration.Variables.Count.Should().Be(1);
            fs.Declaration.Variables[0].ToString().Should().Be("a = b");
            fs.Statement.Should().NotBeNull();
            fs.Statement.Kind().Should().Be(SyntaxKind.Block);
            fs.Statement.ToString().Should().Be("{ }");
        }

        [Fact]
        public void TestFixedStatementWithMultipleVariables()
        {
            var text = "fixed(T a = b, c = d) { }";
            var statement = this.ParseStatement(text);

            statement.Should().NotBeNull();
            statement.Kind().Should().Be(SyntaxKind.FixedStatement);
            statement.ToString().Should().Be(text);
            statement.Errors().Length.Should().Be(0);

            var fs = (FixedStatementSyntax)statement;
            fs.FixedKeyword.Should().NotBe(default);
            fs.FixedKeyword.IsMissing.Should().BeFalse();
            fs.OpenParenToken.Should().NotBe(default);
            fs.FixedKeyword.IsMissing.Should().BeFalse();
            fs.Declaration.Should().NotBeNull();
            fs.Declaration.Kind().Should().Be(SyntaxKind.VariableDeclaration);
            fs.Declaration.Type.Should().NotBeNull();
            fs.Declaration.Type.ToString().Should().Be("T");
            fs.Declaration.Variables.Count.Should().Be(2);
            fs.Declaration.Variables[0].ToString().Should().Be("a = b");
            fs.Declaration.Variables[1].ToString().Should().Be("c = d");
            fs.Statement.Should().NotBeNull();
            fs.Statement.Kind().Should().Be(SyntaxKind.Block);
            fs.Statement.ToString().Should().Be("{ }");
        }

        [Fact]
        public void TestEmptyStatement()
        {
            var text = ";";
            var statement = this.ParseStatement(text);

            statement.Should().NotBeNull();
            statement.Kind().Should().Be(SyntaxKind.EmptyStatement);
            statement.ToString().Should().Be(text);
            statement.Errors().Length.Should().Be(0);

            var es = (EmptyStatementSyntax)statement;
            es.SemicolonToken.Should().NotBe(default);
            es.SemicolonToken.IsMissing.Should().BeFalse();
        }

        [Fact]
        public void TestLabeledStatement()
        {
            var text = "label: ;";
            var statement = this.ParseStatement(text);

            statement.Should().NotBeNull();
            statement.Kind().Should().Be(SyntaxKind.LabeledStatement);
            statement.ToString().Should().Be(text);
            statement.Errors().Length.Should().Be(0);

            var ls = (LabeledStatementSyntax)statement;
            ls.Identifier.Should().NotBe(default);
            ls.Identifier.ToString().Should().Be("label");
            ls.ColonToken.Should().NotBe(default);
            ls.ColonToken.Kind().Should().Be(SyntaxKind.ColonToken);
            ls.Statement.Should().NotBeNull();
            ls.Statement.Kind().Should().Be(SyntaxKind.EmptyStatement);
            ls.Statement.ToString().Should().Be(";");
        }

        [Fact]
        public void TestBreakStatement()
        {
            var text = "break;";
            var statement = this.ParseStatement(text);

            statement.Should().NotBeNull();
            statement.Kind().Should().Be(SyntaxKind.BreakStatement);
            statement.ToString().Should().Be(text);
            statement.Errors().Length.Should().Be(0);

            var b = (BreakStatementSyntax)statement;
            b.BreakKeyword.Should().NotBe(default);
            b.BreakKeyword.IsMissing.Should().BeFalse();
            b.BreakKeyword.Kind().Should().Be(SyntaxKind.BreakKeyword);
            b.SemicolonToken.Should().NotBe(default);
            b.SemicolonToken.IsMissing.Should().BeFalse();
        }

        [Fact]
        public void TestContinueStatement()
        {
            var text = "continue;";
            var statement = this.ParseStatement(text);

            statement.Should().NotBeNull();
            statement.Kind().Should().Be(SyntaxKind.ContinueStatement);
            statement.ToString().Should().Be(text);
            statement.Errors().Length.Should().Be(0);

            var cs = (ContinueStatementSyntax)statement;
            cs.ContinueKeyword.Should().NotBe(default);
            cs.ContinueKeyword.IsMissing.Should().BeFalse();
            cs.ContinueKeyword.Kind().Should().Be(SyntaxKind.ContinueKeyword);
            cs.SemicolonToken.Should().NotBe(default);
            cs.SemicolonToken.IsMissing.Should().BeFalse();
        }

        [Fact]
        public void TestGotoStatement()
        {
            var text = "goto label;";
            var statement = this.ParseStatement(text);

            statement.Should().NotBeNull();
            statement.Kind().Should().Be(SyntaxKind.GotoStatement);
            statement.ToString().Should().Be(text);
            statement.Errors().Length.Should().Be(0);

            var gs = (GotoStatementSyntax)statement;
            gs.GotoKeyword.Should().NotBe(default);
            gs.GotoKeyword.IsMissing.Should().BeFalse();
            gs.GotoKeyword.Kind().Should().Be(SyntaxKind.GotoKeyword);
            gs.CaseOrDefaultKeyword.Kind().Should().Be(SyntaxKind.None);
            gs.Expression.Should().NotBeNull();
            gs.Expression.ToString().Should().Be("label");
            gs.SemicolonToken.Should().NotBe(default);
            gs.SemicolonToken.IsMissing.Should().BeFalse();
        }

        [Fact]
        public void TestGotoCaseStatement()
        {
            var text = "goto case label;";
            var statement = this.ParseStatement(text);

            statement.Should().NotBeNull();
            statement.Kind().Should().Be(SyntaxKind.GotoCaseStatement);
            statement.ToString().Should().Be(text);
            statement.Errors().Length.Should().Be(0);

            var gs = (GotoStatementSyntax)statement;
            gs.GotoKeyword.Should().NotBe(default);
            gs.GotoKeyword.IsMissing.Should().BeFalse();
            gs.GotoKeyword.Kind().Should().Be(SyntaxKind.GotoKeyword);
            gs.CaseOrDefaultKeyword.Should().NotBe(default);
            gs.CaseOrDefaultKeyword.IsMissing.Should().BeFalse();
            gs.CaseOrDefaultKeyword.Kind().Should().Be(SyntaxKind.CaseKeyword);
            gs.Expression.Should().NotBeNull();
            gs.Expression.ToString().Should().Be("label");
            gs.SemicolonToken.Should().NotBe(default);
            gs.SemicolonToken.IsMissing.Should().BeFalse();
        }

        [Fact]
        public void TestGotoDefault()
        {
            var text = "goto default;";
            var statement = this.ParseStatement(text);

            statement.Should().NotBeNull();
            statement.Kind().Should().Be(SyntaxKind.GotoDefaultStatement);
            statement.ToString().Should().Be(text);
            statement.Errors().Length.Should().Be(0);

            var gs = (GotoStatementSyntax)statement;
            gs.GotoKeyword.Should().NotBe(default);
            gs.GotoKeyword.IsMissing.Should().BeFalse();
            gs.GotoKeyword.Kind().Should().Be(SyntaxKind.GotoKeyword);
            gs.CaseOrDefaultKeyword.Should().NotBe(default);
            gs.CaseOrDefaultKeyword.IsMissing.Should().BeFalse();
            gs.CaseOrDefaultKeyword.Kind().Should().Be(SyntaxKind.DefaultKeyword);
            gs.Expression.Should().BeNull();
            gs.SemicolonToken.Should().NotBe(default);
            gs.SemicolonToken.IsMissing.Should().BeFalse();
        }

        [Fact]
        public void TestReturn()
        {
            var text = "return;";
            var statement = this.ParseStatement(text);

            statement.Should().NotBeNull();
            statement.Kind().Should().Be(SyntaxKind.ReturnStatement);
            statement.ToString().Should().Be(text);
            statement.Errors().Length.Should().Be(0);

            var rs = (ReturnStatementSyntax)statement;
            rs.ReturnKeyword.Should().NotBe(default);
            rs.ReturnKeyword.IsMissing.Should().BeFalse();
            rs.ReturnKeyword.Kind().Should().Be(SyntaxKind.ReturnKeyword);
            rs.Expression.Should().BeNull();
            rs.SemicolonToken.Should().NotBe(default);
            rs.SemicolonToken.IsMissing.Should().BeFalse();
        }

        [Fact]
        public void TestReturnExpression()
        {
            var text = "return a;";
            var statement = this.ParseStatement(text);

            statement.Should().NotBeNull();
            statement.Kind().Should().Be(SyntaxKind.ReturnStatement);
            statement.ToString().Should().Be(text);
            statement.Errors().Length.Should().Be(0);

            var rs = (ReturnStatementSyntax)statement;
            rs.ReturnKeyword.Should().NotBe(default);
            rs.ReturnKeyword.IsMissing.Should().BeFalse();
            rs.ReturnKeyword.Kind().Should().Be(SyntaxKind.ReturnKeyword);
            rs.Expression.Should().NotBeNull();
            rs.Expression.ToString().Should().Be("a");
            rs.SemicolonToken.Should().NotBe(default);
            rs.SemicolonToken.IsMissing.Should().BeFalse();
        }

        [Fact]
        public void TestYieldReturnExpression()
        {
            var text = "yield return a;";
            var statement = this.ParseStatement(text);

            statement.Should().NotBeNull();
            statement.Kind().Should().Be(SyntaxKind.YieldReturnStatement);
            statement.ToString().Should().Be(text);
            statement.Errors().Length.Should().Be(0);

            var ys = (YieldStatementSyntax)statement;
            ys.YieldKeyword.Should().NotBe(default);
            ys.YieldKeyword.IsMissing.Should().BeFalse();
            ys.YieldKeyword.Kind().Should().Be(SyntaxKind.YieldKeyword);
            ys.ReturnOrBreakKeyword.Should().NotBe(default);
            ys.ReturnOrBreakKeyword.IsMissing.Should().BeFalse();
            ys.ReturnOrBreakKeyword.Kind().Should().Be(SyntaxKind.ReturnKeyword);
            ys.Expression.Should().NotBeNull();
            ys.Expression.ToString().Should().Be("a");
            ys.SemicolonToken.Should().NotBe(default);
            ys.SemicolonToken.IsMissing.Should().BeFalse();
        }

        [Fact]
        public void TestYieldBreakExpression()
        {
            var text = "yield break;";
            var statement = this.ParseStatement(text);

            statement.Should().NotBeNull();
            statement.Kind().Should().Be(SyntaxKind.YieldBreakStatement);
            statement.ToString().Should().Be(text);
            statement.Errors().Length.Should().Be(0);

            var ys = (YieldStatementSyntax)statement;
            ys.YieldKeyword.Should().NotBe(default);
            ys.YieldKeyword.IsMissing.Should().BeFalse();
            ys.YieldKeyword.Kind().Should().Be(SyntaxKind.YieldKeyword);
            ys.ReturnOrBreakKeyword.Should().NotBe(default);
            ys.ReturnOrBreakKeyword.IsMissing.Should().BeFalse();
            ys.ReturnOrBreakKeyword.Kind().Should().Be(SyntaxKind.BreakKeyword);
            ys.Expression.Should().BeNull();
            ys.SemicolonToken.Should().NotBe(default);
            ys.SemicolonToken.IsMissing.Should().BeFalse();
        }

        [Fact]
        public void TestThrow()
        {
            var text = "throw;";
            var statement = this.ParseStatement(text);

            statement.Should().NotBeNull();
            statement.Kind().Should().Be(SyntaxKind.ThrowStatement);
            statement.ToString().Should().Be(text);
            statement.Errors().Length.Should().Be(0);

            var ts = (ThrowStatementSyntax)statement;
            ts.ThrowKeyword.Should().NotBe(default);
            ts.ThrowKeyword.IsMissing.Should().BeFalse();
            ts.ThrowKeyword.ContextualKind().Should().Be(SyntaxKind.ThrowKeyword);
            ts.Expression.Should().BeNull();
            ts.SemicolonToken.Should().NotBe(default);
            ts.SemicolonToken.IsMissing.Should().BeFalse();
        }

        [Fact]
        public void TestThrowExpression()
        {
            var text = "throw a;";
            var statement = this.ParseStatement(text);

            statement.Should().NotBeNull();
            statement.Kind().Should().Be(SyntaxKind.ThrowStatement);
            statement.ToString().Should().Be(text);
            statement.Errors().Length.Should().Be(0);

            var ts = (ThrowStatementSyntax)statement;
            ts.ThrowKeyword.Should().NotBe(default);
            ts.ThrowKeyword.IsMissing.Should().BeFalse();
            ts.ThrowKeyword.ContextualKind().Should().Be(SyntaxKind.ThrowKeyword);
            ts.Expression.Should().NotBeNull();
            ts.Expression.ToString().Should().Be("a");
            ts.SemicolonToken.Should().NotBe(default);
            ts.SemicolonToken.IsMissing.Should().BeFalse();
        }

        [Fact]
        public void TestTryCatch()
        {
            var text = "try { } catch(T e) { }";
            var statement = this.ParseStatement(text);

            statement.Should().NotBeNull();
            statement.Kind().Should().Be(SyntaxKind.TryStatement);
            statement.ToString().Should().Be(text);
            statement.Errors().Length.Should().Be(0);

            var ts = (TryStatementSyntax)statement;
            ts.TryKeyword.Should().NotBe(default);
            ts.TryKeyword.IsMissing.Should().BeFalse();
            ts.Block.Should().NotBeNull();

            ts.Catches.Count.Should().Be(1);
            ts.Catches[0].CatchKeyword.Should().NotBe(default);
            ts.Catches[0].Declaration.Should().NotBeNull();
            ts.Catches[0].Declaration.OpenParenToken.Should().NotBe(default);
            ts.Catches[0].Declaration.Type.Should().NotBeNull();
            ts.Catches[0].Declaration.Type.ToString().Should().Be("T");
            ts.Catches[0].Declaration.Identifier.Should().NotBe(default);
            ts.Catches[0].Declaration.Identifier.ToString().Should().Be("e");
            ts.Catches[0].Declaration.CloseParenToken.Should().NotBe(default);
            ts.Catches[0].Block.Should().NotBeNull();

            ts.Finally.Should().BeNull();
        }

        [Fact]
        public void TestTryCatchWithNoExceptionName()
        {
            var text = "try { } catch(T) { }";
            var statement = this.ParseStatement(text);

            statement.Should().NotBeNull();
            statement.Kind().Should().Be(SyntaxKind.TryStatement);
            statement.ToString().Should().Be(text);
            statement.Errors().Length.Should().Be(0);

            var ts = (TryStatementSyntax)statement;
            ts.TryKeyword.Should().NotBe(default);
            ts.TryKeyword.IsMissing.Should().BeFalse();
            ts.Block.Should().NotBeNull();

            ts.Catches.Count.Should().Be(1);
            ts.Catches[0].CatchKeyword.Should().NotBe(default);
            ts.Catches[0].Declaration.Should().NotBeNull();
            ts.Catches[0].Declaration.OpenParenToken.Should().NotBe(default);
            ts.Catches[0].Declaration.Type.Should().NotBeNull();
            ts.Catches[0].Declaration.Type.ToString().Should().Be("T");
            ts.Catches[0].Declaration.Identifier.Kind().Should().Be(SyntaxKind.None);
            ts.Catches[0].Declaration.CloseParenToken.Should().NotBe(default);
            ts.Catches[0].Block.Should().NotBeNull();

            ts.Finally.Should().BeNull();
        }

        [Fact]
        public void TestTryCatchWithNoExceptionDeclaration()
        {
            var text = "try { } catch { }";
            var statement = this.ParseStatement(text);

            statement.Should().NotBeNull();
            statement.Kind().Should().Be(SyntaxKind.TryStatement);
            statement.ToString().Should().Be(text);
            statement.Errors().Length.Should().Be(0);

            var ts = (TryStatementSyntax)statement;
            ts.TryKeyword.Should().NotBe(default);
            ts.TryKeyword.IsMissing.Should().BeFalse();
            ts.Block.Should().NotBeNull();

            ts.Catches.Count.Should().Be(1);
            ts.Catches[0].CatchKeyword.Should().NotBe(default);
            ts.Catches[0].Declaration.Should().BeNull();
            ts.Catches[0].Block.Should().NotBeNull();

            ts.Finally.Should().BeNull();
        }

        [Fact]
        public void TestTryCatchWithMultipleCatches()
        {
            var text = "try { } catch(T e) { } catch(T2) { } catch { }";
            var statement = this.ParseStatement(text);

            statement.Should().NotBeNull();
            statement.Kind().Should().Be(SyntaxKind.TryStatement);
            statement.ToString().Should().Be(text);
            statement.Errors().Length.Should().Be(0);

            var ts = (TryStatementSyntax)statement;
            ts.TryKeyword.Should().NotBe(default);
            ts.TryKeyword.IsMissing.Should().BeFalse();
            ts.Block.Should().NotBeNull();

            ts.Catches.Count.Should().Be(3);

            ts.Catches[0].CatchKeyword.Should().NotBe(default);
            ts.Catches[0].Declaration.Should().NotBeNull();
            ts.Catches[0].Declaration.OpenParenToken.Should().NotBe(default);
            ts.Catches[0].Declaration.Type.Should().NotBeNull();
            ts.Catches[0].Declaration.Type.ToString().Should().Be("T");
            ts.Catches[0].Declaration.Identifier.Should().NotBe(default);
            ts.Catches[0].Declaration.Identifier.ToString().Should().Be("e");
            ts.Catches[0].Declaration.CloseParenToken.Should().NotBe(default);
            ts.Catches[0].Block.Should().NotBeNull();

            ts.Catches[1].CatchKeyword.Should().NotBe(default);
            ts.Catches[1].Declaration.Should().NotBeNull();
            ts.Catches[1].Declaration.OpenParenToken.Should().NotBe(default);
            ts.Catches[1].Declaration.Type.Should().NotBeNull();
            ts.Catches[1].Declaration.Type.ToString().Should().Be("T2");
            ts.Catches[1].Declaration.CloseParenToken.Should().NotBe(default);
            ts.Catches[1].Block.Should().NotBeNull();

            ts.Catches[2].CatchKeyword.Should().NotBe(default);
            ts.Catches[2].Declaration.Should().BeNull();
            ts.Catches[2].Block.Should().NotBeNull();

            ts.Finally.Should().BeNull();
        }

        [Fact]
        public void TestTryFinally()
        {
            var text = "try { } finally { }";
            var statement = this.ParseStatement(text);

            statement.Should().NotBeNull();
            statement.Kind().Should().Be(SyntaxKind.TryStatement);
            statement.ToString().Should().Be(text);
            statement.Errors().Length.Should().Be(0);

            var ts = (TryStatementSyntax)statement;
            ts.TryKeyword.Should().NotBe(default);
            ts.TryKeyword.IsMissing.Should().BeFalse();
            ts.Block.Should().NotBeNull();

            ts.Catches.Count.Should().Be(0);

            ts.Finally.Should().NotBeNull();
            ts.Finally.FinallyKeyword.Should().NotBe(default);
            ts.Finally.Block.Should().NotBeNull();
        }

        [Fact]
        public void TestTryCatchWithMultipleCatchesAndFinally()
        {
            var text = "try { } catch(T e) { } catch(T2) { } catch { } finally { }";
            var statement = this.ParseStatement(text);

            statement.Should().NotBeNull();
            statement.Kind().Should().Be(SyntaxKind.TryStatement);
            statement.ToString().Should().Be(text);
            statement.Errors().Length.Should().Be(0);

            var ts = (TryStatementSyntax)statement;
            ts.TryKeyword.Should().NotBe(default);
            ts.TryKeyword.IsMissing.Should().BeFalse();
            ts.Block.Should().NotBeNull();

            ts.Catches.Count.Should().Be(3);

            ts.Catches[0].CatchKeyword.Should().NotBe(default);
            ts.Catches[0].Declaration.Should().NotBeNull();
            ts.Catches[0].Declaration.OpenParenToken.Should().NotBe(default);
            ts.Catches[0].Declaration.Type.Should().NotBeNull();
            ts.Catches[0].Declaration.Type.ToString().Should().Be("T");
            ts.Catches[0].Declaration.Identifier.Should().NotBe(default);
            ts.Catches[0].Declaration.Identifier.ToString().Should().Be("e");
            ts.Catches[0].Declaration.CloseParenToken.Should().NotBe(default);
            ts.Catches[0].Block.Should().NotBeNull();

            ts.Catches[1].CatchKeyword.Should().NotBe(default);
            ts.Catches[1].Declaration.Should().NotBeNull();
            ts.Catches[1].Declaration.OpenParenToken.Should().NotBe(default);
            ts.Catches[1].Declaration.Type.Should().NotBeNull();
            ts.Catches[1].Declaration.Type.ToString().Should().Be("T2");
            ts.Catches[1].Declaration.CloseParenToken.Should().NotBe(default);
            ts.Catches[1].Block.Should().NotBeNull();

            ts.Catches[2].CatchKeyword.Should().NotBe(default);
            ts.Catches[2].Declaration.Should().BeNull();
            ts.Catches[2].Block.Should().NotBeNull();

            ts.Finally.Should().NotBeNull();
            ts.Finally.FinallyKeyword.Should().NotBe(default);
            ts.Finally.Block.Should().NotBeNull();
        }

        [Fact]
        public void TestChecked()
        {
            var text = "checked { }";
            var statement = this.ParseStatement(text);

            statement.Should().NotBeNull();
            statement.Kind().Should().Be(SyntaxKind.CheckedStatement);
            statement.ToString().Should().Be(text);
            statement.Errors().Length.Should().Be(0);

            var cs = (CheckedStatementSyntax)statement;
            cs.Keyword.Should().NotBe(default);
            cs.Keyword.Kind().Should().Be(SyntaxKind.CheckedKeyword);
            cs.Block.Should().NotBeNull();
        }

        [Fact]
        public void TestUnchecked()
        {
            var text = "unchecked { }";
            var statement = this.ParseStatement(text);

            statement.Should().NotBeNull();
            statement.Kind().Should().Be(SyntaxKind.UncheckedStatement);
            statement.ToString().Should().Be(text);
            statement.Errors().Length.Should().Be(0);

            var cs = (CheckedStatementSyntax)statement;
            cs.Keyword.Should().NotBe(default);
            cs.Keyword.Kind().Should().Be(SyntaxKind.UncheckedKeyword);
            cs.Block.Should().NotBeNull();
        }

        [Fact]
        public void TestUnsafe()
        {
            var text = "unsafe { }";
            var statement = this.ParseStatement(text);

            statement.Should().NotBeNull();
            statement.Kind().Should().Be(SyntaxKind.UnsafeStatement);
            statement.ToString().Should().Be(text);
            statement.Errors().Length.Should().Be(0);

            var us = (UnsafeStatementSyntax)statement;
            us.UnsafeKeyword.Should().NotBe(default);
            us.UnsafeKeyword.Kind().Should().Be(SyntaxKind.UnsafeKeyword);
            us.Block.Should().NotBeNull();
        }

        [Fact]
        public void TestWhile()
        {
            var text = "while(a) { }";
            var statement = this.ParseStatement(text);

            statement.Should().NotBeNull();
            statement.Kind().Should().Be(SyntaxKind.WhileStatement);
            statement.ToString().Should().Be(text);
            statement.Errors().Length.Should().Be(0);

            var ws = (WhileStatementSyntax)statement;
            ws.WhileKeyword.Should().NotBe(default);
            ws.WhileKeyword.Kind().Should().Be(SyntaxKind.WhileKeyword);
            ws.OpenParenToken.Should().NotBe(default);
            ws.Condition.Should().NotBeNull();
            ws.CloseParenToken.Should().NotBe(default);
            ws.Condition.ToString().Should().Be("a");
            ws.Statement.Should().NotBeNull();
            ws.Statement.Kind().Should().Be(SyntaxKind.Block);
        }

        [Fact]
        public void TestDoWhile()
        {
            var text = "do { } while (a);";
            var statement = this.ParseStatement(text);

            statement.Should().NotBeNull();
            statement.Kind().Should().Be(SyntaxKind.DoStatement);
            statement.ToString().Should().Be(text);
            statement.Errors().Length.Should().Be(0);

            var ds = (DoStatementSyntax)statement;
            ds.DoKeyword.Should().NotBe(default);
            ds.DoKeyword.Kind().Should().Be(SyntaxKind.DoKeyword);
            ds.Statement.Should().NotBeNull();
            ds.WhileKeyword.Should().NotBe(default);
            ds.WhileKeyword.Kind().Should().Be(SyntaxKind.WhileKeyword);
            ds.Statement.Kind().Should().Be(SyntaxKind.Block);
            ds.OpenParenToken.Should().NotBe(default);
            ds.Condition.Should().NotBeNull();
            ds.CloseParenToken.Should().NotBe(default);
            ds.Condition.ToString().Should().Be("a");
            ds.SemicolonToken.Should().NotBe(default);
        }

        [Fact]
        public void TestFor()
        {
            var text = "for(;;) { }";
            var statement = this.ParseStatement(text);

            statement.Should().NotBeNull();
            statement.Kind().Should().Be(SyntaxKind.ForStatement);
            statement.ToString().Should().Be(text);
            statement.Errors().Length.Should().Be(0);

            var fs = (ForStatementSyntax)statement;
            fs.ForKeyword.Should().NotBe(default);
            fs.ForKeyword.IsMissing.Should().BeFalse();
            fs.ForKeyword.Kind().Should().Be(SyntaxKind.ForKeyword);
            fs.OpenParenToken.Should().NotBe(default);
            fs.Declaration.Should().BeNull();
            fs.Initializers.Count.Should().Be(0);
            fs.FirstSemicolonToken.Should().NotBe(default);
            fs.Condition.Should().BeNull();
            fs.SecondSemicolonToken.Should().NotBe(default);
            fs.Incrementors.Count.Should().Be(0);
            fs.CloseParenToken.Should().NotBe(default);
            fs.Statement.Should().NotBeNull();
        }

        [Fact]
        public void TestForWithVariableDeclaration()
        {
            var text = "for(T a = 0;;) { }";
            var statement = this.ParseStatement(text);

            statement.Should().NotBeNull();
            statement.Kind().Should().Be(SyntaxKind.ForStatement);
            statement.ToString().Should().Be(text);
            statement.Errors().Length.Should().Be(0);

            var fs = (ForStatementSyntax)statement;
            fs.ForKeyword.Should().NotBe(default);
            fs.ForKeyword.IsMissing.Should().BeFalse();
            fs.ForKeyword.Kind().Should().Be(SyntaxKind.ForKeyword);
            fs.OpenParenToken.Should().NotBe(default);

            fs.Declaration.Should().NotBeNull();
            fs.Declaration.Type.Should().NotBeNull();
            fs.Declaration.Type.ToString().Should().Be("T");
            fs.Declaration.Variables.Count.Should().Be(1);
            fs.Declaration.Variables[0].Identifier.Should().NotBe(default);
            fs.Declaration.Variables[0].Identifier.ToString().Should().Be("a");
            fs.Declaration.Variables[0].Initializer.Should().NotBeNull();
            fs.Declaration.Variables[0].Initializer.EqualsToken.Should().NotBe(default);
            fs.Declaration.Variables[0].Initializer.Value.Should().NotBeNull();
            fs.Declaration.Variables[0].Initializer.Value.ToString().Should().Be("0");

            fs.Initializers.Count.Should().Be(0);
            fs.FirstSemicolonToken.Should().NotBe(default);
            fs.Condition.Should().BeNull();
            fs.SecondSemicolonToken.Should().NotBe(default);
            fs.Incrementors.Count.Should().Be(0);
            fs.CloseParenToken.Should().NotBe(default);
            fs.Statement.Should().NotBeNull();
        }

        [Fact]
        public void TestForWithVarDeclaration()
        {
            var text = "for(var a = 0;;) { }";
            var statement = this.ParseStatement(text);

            statement.Should().NotBeNull();
            statement.Kind().Should().Be(SyntaxKind.ForStatement);
            statement.ToString().Should().Be(text);
            statement.Errors().Length.Should().Be(0);

            var fs = (ForStatementSyntax)statement;
            fs.ForKeyword.Should().NotBe(default);
            fs.ForKeyword.IsMissing.Should().BeFalse();
            fs.ForKeyword.Kind().Should().Be(SyntaxKind.ForKeyword);
            fs.OpenParenToken.Should().NotBe(default);

            fs.Declaration.Should().NotBeNull();
            fs.Declaration.Type.Should().NotBeNull();
            fs.Declaration.Type.ToString().Should().Be("var");
            fs.Declaration.Type.Kind().Should().Be(SyntaxKind.IdentifierName);
            ((IdentifierNameSyntax)fs.Declaration.Type).Identifier.Kind().Should().Be(SyntaxKind.IdentifierToken);
            fs.Declaration.Variables.Count.Should().Be(1);
            fs.Declaration.Variables[0].Identifier.Should().NotBe(default);
            fs.Declaration.Variables[0].Identifier.ToString().Should().Be("a");
            fs.Declaration.Variables[0].Initializer.Should().NotBeNull();
            fs.Declaration.Variables[0].Initializer.EqualsToken.Should().NotBe(default);
            fs.Declaration.Variables[0].Initializer.Value.Should().NotBeNull();
            fs.Declaration.Variables[0].Initializer.Value.ToString().Should().Be("0");

            fs.Initializers.Count.Should().Be(0);
            fs.FirstSemicolonToken.Should().NotBe(default);
            fs.Condition.Should().BeNull();
            fs.SecondSemicolonToken.Should().NotBe(default);
            fs.Incrementors.Count.Should().Be(0);
            fs.CloseParenToken.Should().NotBe(default);
            fs.Statement.Should().NotBeNull();
        }

        [Fact]
        public void TestForWithMultipleVariableDeclarations()
        {
            var text = "for(T a = 0, b = 1;;) { }";
            var statement = this.ParseStatement(text);

            statement.Should().NotBeNull();
            statement.Kind().Should().Be(SyntaxKind.ForStatement);
            statement.ToString().Should().Be(text);
            statement.Errors().Length.Should().Be(0);

            var fs = (ForStatementSyntax)statement;
            fs.ForKeyword.Should().NotBe(default);
            fs.ForKeyword.IsMissing.Should().BeFalse();
            fs.ForKeyword.Kind().Should().Be(SyntaxKind.ForKeyword);
            fs.OpenParenToken.Should().NotBe(default);

            fs.Declaration.Should().NotBeNull();
            fs.Declaration.Type.Should().NotBeNull();
            fs.Declaration.Type.ToString().Should().Be("T");
            fs.Declaration.Variables.Count.Should().Be(2);

            fs.Declaration.Variables[0].Identifier.Should().NotBe(default);
            fs.Declaration.Variables[0].Identifier.ToString().Should().Be("a");
            fs.Declaration.Variables[0].Initializer.Should().NotBeNull();
            fs.Declaration.Variables[0].Initializer.EqualsToken.Should().NotBe(default);
            fs.Declaration.Variables[0].Initializer.Value.Should().NotBeNull();
            fs.Declaration.Variables[0].Initializer.Value.ToString().Should().Be("0");

            fs.Declaration.Variables[1].Identifier.Should().NotBe(default);
            fs.Declaration.Variables[1].Identifier.ToString().Should().Be("b");
            fs.Declaration.Variables[1].Initializer.Should().NotBeNull();
            fs.Declaration.Variables[1].Initializer.EqualsToken.Should().NotBe(default);
            fs.Declaration.Variables[1].Initializer.Value.Should().NotBeNull();
            fs.Declaration.Variables[1].Initializer.Value.ToString().Should().Be("1");

            fs.Initializers.Count.Should().Be(0);
            fs.FirstSemicolonToken.Should().NotBe(default);
            fs.Condition.Should().BeNull();
            fs.SecondSemicolonToken.Should().NotBe(default);
            fs.Incrementors.Count.Should().Be(0);
            fs.CloseParenToken.Should().NotBe(default);
            fs.Statement.Should().NotBeNull();
        }

        [Fact, CompilerTrait(CompilerFeature.RefLocalsReturns)]
        public void TestForWithRefVariableDeclaration()
        {
            var text = "for(ref T a = ref b, c = ref d;;) { }";
            var statement = this.ParseStatement(text);

            UsingNode(statement);
            N(SyntaxKind.ForStatement);
            {
                N(SyntaxKind.ForKeyword);
                N(SyntaxKind.OpenParenToken);
                N(SyntaxKind.VariableDeclaration);
                {
                    N(SyntaxKind.RefType);
                    {
                        N(SyntaxKind.RefKeyword);
                        N(SyntaxKind.IdentifierName);
                        {
                            N(SyntaxKind.IdentifierToken, "T");
                        }
                    }
                    N(SyntaxKind.VariableDeclarator);
                    {
                        N(SyntaxKind.IdentifierToken, "a");
                        N(SyntaxKind.EqualsValueClause);
                        {
                            N(SyntaxKind.EqualsToken);
                            N(SyntaxKind.RefExpression);
                            {
                                N(SyntaxKind.RefKeyword);
                                N(SyntaxKind.IdentifierName);
                                {
                                    N(SyntaxKind.IdentifierToken, "b");
                                }
                            }
                        }
                    }
                    N(SyntaxKind.CommaToken);
                    N(SyntaxKind.VariableDeclarator);
                    {
                        N(SyntaxKind.IdentifierToken, "c");
                        N(SyntaxKind.EqualsValueClause);
                        {
                            N(SyntaxKind.EqualsToken);
                            N(SyntaxKind.RefExpression);
                            {
                                N(SyntaxKind.RefKeyword);
                                N(SyntaxKind.IdentifierName);
                                {
                                    N(SyntaxKind.IdentifierToken, "d");
                                }
                            }
                        }
                    }
                }
                N(SyntaxKind.SemicolonToken);
                N(SyntaxKind.SemicolonToken);
                N(SyntaxKind.CloseParenToken);
                N(SyntaxKind.Block);
                N(SyntaxKind.OpenBraceToken);
                N(SyntaxKind.CloseBraceToken);
            }
        }

        [Fact]
        public void TestForWithVariableInitializer()
        {
            var text = "for(a = 0;;) { }";
            var statement = this.ParseStatement(text);

            statement.Should().NotBeNull();
            statement.Kind().Should().Be(SyntaxKind.ForStatement);
            statement.ToString().Should().Be(text);
            statement.Errors().Length.Should().Be(0);

            var fs = (ForStatementSyntax)statement;
            fs.ForKeyword.Should().NotBe(default);
            fs.ForKeyword.IsMissing.Should().BeFalse();
            fs.ForKeyword.Kind().Should().Be(SyntaxKind.ForKeyword);
            fs.OpenParenToken.Should().NotBe(default);

            fs.Declaration.Should().BeNull();
            fs.Initializers.Count.Should().Be(1);
            fs.Initializers[0].ToString().Should().Be("a = 0");

            fs.FirstSemicolonToken.Should().NotBe(default);
            fs.Condition.Should().BeNull();
            fs.SecondSemicolonToken.Should().NotBe(default);
            fs.Incrementors.Count.Should().Be(0);
            fs.CloseParenToken.Should().NotBe(default);
            fs.Statement.Should().NotBeNull();
        }

        [Fact]
        public void TestForWithMultipleVariableInitializers()
        {
            var text = "for(a = 0, b = 1;;) { }";
            var statement = this.ParseStatement(text);

            statement.Should().NotBeNull();
            statement.Kind().Should().Be(SyntaxKind.ForStatement);
            statement.ToString().Should().Be(text);
            statement.Errors().Length.Should().Be(0);

            var fs = (ForStatementSyntax)statement;
            fs.ForKeyword.Should().NotBe(default);
            fs.ForKeyword.IsMissing.Should().BeFalse();
            fs.ForKeyword.Kind().Should().Be(SyntaxKind.ForKeyword);
            fs.OpenParenToken.Should().NotBe(default);

            fs.Declaration.Should().BeNull();
            fs.Initializers.Count.Should().Be(2);
            fs.Initializers[0].ToString().Should().Be("a = 0");
            fs.Initializers[1].ToString().Should().Be("b = 1");

            fs.FirstSemicolonToken.Should().NotBe(default);
            fs.Condition.Should().BeNull();
            fs.SecondSemicolonToken.Should().NotBe(default);
            fs.Incrementors.Count.Should().Be(0);
            fs.CloseParenToken.Should().NotBe(default);
            fs.Statement.Should().NotBeNull();
        }

        [Fact]
        public void TestForWithCondition()
        {
            var text = "for(; a;) { }";
            var statement = this.ParseStatement(text);

            statement.Should().NotBeNull();
            statement.Kind().Should().Be(SyntaxKind.ForStatement);
            statement.ToString().Should().Be(text);
            statement.Errors().Length.Should().Be(0);

            var fs = (ForStatementSyntax)statement;
            fs.ForKeyword.Should().NotBe(default);
            fs.ForKeyword.IsMissing.Should().BeFalse();
            fs.ForKeyword.Kind().Should().Be(SyntaxKind.ForKeyword);
            fs.OpenParenToken.Should().NotBe(default);

            fs.Declaration.Should().BeNull();
            fs.Initializers.Count.Should().Be(0);
            fs.FirstSemicolonToken.Should().NotBe(default);

            fs.Condition.Should().NotBeNull();
            fs.Condition.ToString().Should().Be("a");

            fs.SecondSemicolonToken.Should().NotBe(default);
            fs.Incrementors.Count.Should().Be(0);
            fs.CloseParenToken.Should().NotBe(default);
            fs.Statement.Should().NotBeNull();
        }

        [Fact]
        public void TestForWithIncrementor()
        {
            var text = "for(; ; a++) { }";
            var statement = this.ParseStatement(text);

            statement.Should().NotBeNull();
            statement.Kind().Should().Be(SyntaxKind.ForStatement);
            statement.ToString().Should().Be(text);
            statement.Errors().Length.Should().Be(0);

            var fs = (ForStatementSyntax)statement;
            fs.ForKeyword.Should().NotBe(default);
            fs.ForKeyword.IsMissing.Should().BeFalse();
            fs.ForKeyword.Kind().Should().Be(SyntaxKind.ForKeyword);
            fs.OpenParenToken.Should().NotBe(default);

            fs.Declaration.Should().BeNull();
            fs.Initializers.Count.Should().Be(0);
            fs.FirstSemicolonToken.Should().NotBe(default);
            fs.Condition.Should().BeNull();
            fs.SecondSemicolonToken.Should().NotBe(default);

            fs.Incrementors.Count.Should().Be(1);
            fs.Incrementors[0].ToString().Should().Be("a++");

            fs.CloseParenToken.Should().NotBe(default);
            fs.Statement.Should().NotBeNull();
        }

        [Fact]
        public void TestForWithMultipleIncrementors()
        {
            var text = "for(; ; a++, b++) { }";
            var statement = this.ParseStatement(text);

            statement.Should().NotBeNull();
            statement.Kind().Should().Be(SyntaxKind.ForStatement);
            statement.ToString().Should().Be(text);
            statement.Errors().Length.Should().Be(0);

            var fs = (ForStatementSyntax)statement;
            fs.ForKeyword.Should().NotBe(default);
            fs.ForKeyword.IsMissing.Should().BeFalse();
            fs.ForKeyword.Kind().Should().Be(SyntaxKind.ForKeyword);
            fs.OpenParenToken.Should().NotBe(default);

            fs.Declaration.Should().BeNull();
            fs.Initializers.Count.Should().Be(0);
            fs.FirstSemicolonToken.Should().NotBe(default);
            fs.Condition.Should().BeNull();
            fs.SecondSemicolonToken.Should().NotBe(default);

            fs.Incrementors.Count.Should().Be(2);
            fs.Incrementors[0].ToString().Should().Be("a++");
            fs.Incrementors[1].ToString().Should().Be("b++");

            fs.CloseParenToken.Should().NotBe(default);
            fs.Statement.Should().NotBeNull();
        }

        [Fact]
        public void TestForWithDeclarationConditionAndIncrementor()
        {
            var text = "for(T a = 0; a < 10; a++) { }";
            var statement = this.ParseStatement(text);

            statement.Should().NotBeNull();
            statement.Kind().Should().Be(SyntaxKind.ForStatement);
            statement.ToString().Should().Be(text);
            statement.Errors().Length.Should().Be(0);

            var fs = (ForStatementSyntax)statement;
            fs.ForKeyword.Should().NotBe(default);
            fs.ForKeyword.IsMissing.Should().BeFalse();
            fs.ForKeyword.Kind().Should().Be(SyntaxKind.ForKeyword);
            fs.OpenParenToken.Should().NotBe(default);

            fs.Declaration.Should().NotBeNull();
            fs.Declaration.Type.Should().NotBeNull();
            fs.Declaration.Type.ToString().Should().Be("T");
            fs.Declaration.Variables.Count.Should().Be(1);
            fs.Declaration.Variables[0].Identifier.Should().NotBe(default);
            fs.Declaration.Variables[0].Identifier.ToString().Should().Be("a");
            fs.Declaration.Variables[0].Initializer.Should().NotBeNull();
            fs.Declaration.Variables[0].Initializer.EqualsToken.Should().NotBe(default);
            fs.Declaration.Variables[0].Initializer.Value.Should().NotBeNull();
            fs.Declaration.Variables[0].Initializer.Value.ToString().Should().Be("0");

            fs.Initializers.Count.Should().Be(0);

            fs.FirstSemicolonToken.Should().NotBe(default);
            fs.Condition.Should().NotBeNull();
            fs.Condition.ToString().Should().Be("a < 10");

            fs.SecondSemicolonToken.Should().NotBe(default);

            fs.Incrementors.Count.Should().Be(1);
            fs.Incrementors[0].ToString().Should().Be("a++");

            fs.CloseParenToken.Should().NotBe(default);
            fs.Statement.Should().NotBeNull();
        }

        [Fact]
        public void TestForEach()
        {
            var text = "foreach(T a in b) { }";
            var statement = this.ParseStatement(text);

            statement.Should().NotBeNull();
            statement.Kind().Should().Be(SyntaxKind.ForEachStatement);
            statement.ToString().Should().Be(text);
            statement.Errors().Length.Should().Be(0);

            var fs = (ForEachStatementSyntax)statement;
            fs.ForEachKeyword.Should().NotBe(default);
            fs.ForEachKeyword.Kind().Should().Be(SyntaxKind.ForEachKeyword);

            fs.OpenParenToken.Should().NotBe(default);
            fs.Type.Should().NotBeNull();
            fs.Type.ToString().Should().Be("T");
            fs.Identifier.Should().NotBe(default);
            fs.Identifier.ToString().Should().Be("a");
            fs.InKeyword.Should().NotBe(default);
            fs.InKeyword.IsMissing.Should().BeFalse();
            fs.InKeyword.Kind().Should().Be(SyntaxKind.InKeyword);
            fs.Expression.Should().NotBeNull();
            fs.Expression.ToString().Should().Be("b");
            fs.CloseParenToken.Should().NotBe(default);
            fs.Statement.Should().NotBeNull();
        }

        [Fact]
        public void TestForAsForEach()
        {
            var text = "for(T a in b) { }";
            var statement = this.ParseStatement(text);

            statement.Should().NotBeNull();
            statement.Kind().Should().Be(SyntaxKind.ForEachStatement);
            statement.ToString().Should().Be(text);
            statement.Errors().Length.Should().Be(1);

            var fs = (ForEachStatementSyntax)statement;
            fs.ForEachKeyword.Should().NotBe(default);
            fs.ForEachKeyword.Kind().Should().Be(SyntaxKind.ForEachKeyword);
            fs.ForEachKeyword.IsMissing.Should().BeTrue();
            fs.ForEachKeyword.TrailingTrivia.Count.Should().Be(1);
            fs.ForEachKeyword.TrailingTrivia[0].Kind().Should().Be(SyntaxKind.SkippedTokensTrivia);
            fs.ForEachKeyword.TrailingTrivia[0].ToString().Should().Be("for");

            fs.OpenParenToken.Should().NotBe(default);
            fs.Type.Should().NotBeNull();
            fs.Type.ToString().Should().Be("T");
            fs.Identifier.Should().NotBe(default);
            fs.Identifier.ToString().Should().Be("a");
            fs.InKeyword.Should().NotBe(default);
            fs.InKeyword.IsMissing.Should().BeFalse();
            fs.InKeyword.Kind().Should().Be(SyntaxKind.InKeyword);
            fs.Expression.Should().NotBeNull();
            fs.Expression.ToString().Should().Be("b");
            fs.CloseParenToken.Should().NotBe(default);
            fs.Statement.Should().NotBeNull();
        }

        [Fact]
        public void TestForEachWithVar()
        {
            var text = "foreach(var a in b) { }";
            var statement = this.ParseStatement(text);

            statement.Should().NotBeNull();
            statement.Kind().Should().Be(SyntaxKind.ForEachStatement);
            statement.ToString().Should().Be(text);
            statement.Errors().Length.Should().Be(0);

            var fs = (ForEachStatementSyntax)statement;
            fs.ForEachKeyword.Should().NotBe(default);
            fs.ForEachKeyword.Kind().Should().Be(SyntaxKind.ForEachKeyword);

            fs.OpenParenToken.Should().NotBe(default);
            fs.Type.Should().NotBeNull();
            fs.Type.ToString().Should().Be("var");
            fs.Type.Kind().Should().Be(SyntaxKind.IdentifierName);
            ((IdentifierNameSyntax)fs.Type).Identifier.Kind().Should().Be(SyntaxKind.IdentifierToken);
            fs.Identifier.Should().NotBe(default);
            fs.Identifier.ToString().Should().Be("a");
            fs.InKeyword.Should().NotBe(default);
            fs.InKeyword.IsMissing.Should().BeFalse();
            fs.InKeyword.Kind().Should().Be(SyntaxKind.InKeyword);
            fs.Expression.Should().NotBeNull();
            fs.Expression.ToString().Should().Be("b");
            fs.CloseParenToken.Should().NotBe(default);
            fs.Statement.Should().NotBeNull();
        }

        [Fact]
        public void TestIf()
        {
            var text = "if (a) { }";
            var statement = this.ParseStatement(text);

            statement.Should().NotBeNull();
            statement.Kind().Should().Be(SyntaxKind.IfStatement);
            statement.ToString().Should().Be(text);
            statement.Errors().Length.Should().Be(0);

            var ss = (IfStatementSyntax)statement;
            ss.IfKeyword.Should().NotBe(default);
            ss.IfKeyword.Kind().Should().Be(SyntaxKind.IfKeyword);
            ss.OpenParenToken.Should().NotBe(default);
            ss.Condition.Should().NotBeNull();
            ss.Condition.ToString().Should().Be("a");
            ss.CloseParenToken.Should().NotBe(default);
            ss.Statement.Should().NotBeNull();

            ss.Else.Should().BeNull();
        }

        [Fact]
        public void TestIfElse()
        {
            var text = "if (a) { } else { }";
            var statement = this.ParseStatement(text);

            statement.Should().NotBeNull();
            statement.Kind().Should().Be(SyntaxKind.IfStatement);
            statement.ToString().Should().Be(text);
            statement.Errors().Length.Should().Be(0);

            var ss = (IfStatementSyntax)statement;
            ss.IfKeyword.Should().NotBe(default);
            ss.IfKeyword.Kind().Should().Be(SyntaxKind.IfKeyword);
            ss.OpenParenToken.Should().NotBe(default);
            ss.Condition.Should().NotBeNull();
            ss.Condition.ToString().Should().Be("a");
            ss.CloseParenToken.Should().NotBe(default);
            ss.Statement.Should().NotBeNull();

            ss.Else.Should().NotBeNull();
            ss.Else.ElseKeyword.Should().NotBe(default);
            ss.Else.ElseKeyword.Kind().Should().Be(SyntaxKind.ElseKeyword);
            ss.Else.Statement.Should().NotBeNull();
        }

        [Fact]
        public void TestIfElseIf()
        {
            var text = "if (a) { } else if (b) { }";
            var statement = this.ParseStatement(text);

            statement.Should().NotBeNull();
            statement.Kind().Should().Be(SyntaxKind.IfStatement);
            statement.ToString().Should().Be(text);
            statement.Errors().Length.Should().Be(0);

            var ss = (IfStatementSyntax)statement;
            ss.IfKeyword.Should().NotBe(default);
            ss.IfKeyword.Kind().Should().Be(SyntaxKind.IfKeyword);
            ss.OpenParenToken.Should().NotBe(default);
            ss.Condition.Should().NotBeNull();
            ss.Condition.ToString().Should().Be("a");
            ss.CloseParenToken.Should().NotBe(default);
            ss.Statement.Should().NotBeNull();

            ss.Else.Should().NotBeNull();
            ss.Else.ElseKeyword.Should().NotBe(default);
            ss.Else.ElseKeyword.Kind().Should().Be(SyntaxKind.ElseKeyword);
            ss.Else.Statement.Should().NotBeNull();

            var subIf = (IfStatementSyntax)ss.Else.Statement;
            subIf.IfKeyword.Should().NotBe(default);
            subIf.IfKeyword.Kind().Should().Be(SyntaxKind.IfKeyword);
            subIf.Condition.Should().NotBeNull();
            subIf.Condition.ToString().Should().Be("b");
            subIf.CloseParenToken.Should().NotBe(default);
            subIf.Statement.Should().NotBeNull();
        }

        [Fact]
        public void TestLock()
        {
            var text = "lock (a) { }";
            var statement = this.ParseStatement(text);

            statement.Should().NotBeNull();
            statement.Kind().Should().Be(SyntaxKind.LockStatement);
            statement.ToString().Should().Be(text);
            statement.Errors().Length.Should().Be(0);

            var ls = (LockStatementSyntax)statement;
            ls.LockKeyword.Should().NotBe(default);
            ls.LockKeyword.Kind().Should().Be(SyntaxKind.LockKeyword);
            ls.OpenParenToken.Should().NotBe(default);
            ls.Expression.Should().NotBeNull();
            ls.Expression.ToString().Should().Be("a");
            ls.CloseParenToken.Should().NotBe(default);
            ls.Statement.Should().NotBeNull();
        }

        [Fact]
        public void TestSwitch()
        {
            var text = "switch (a) { }";
            var statement = this.ParseStatement(text);

            statement.Should().NotBeNull();
            statement.Kind().Should().Be(SyntaxKind.SwitchStatement);
            statement.ToString().Should().Be(text);
            statement.Errors().Length.Should().Be(0);
            var diags = statement.ErrorsAndWarnings();
            diags.Length.Should().Be(0);

            var ss = (SwitchStatementSyntax)statement;
            ss.SwitchKeyword.Should().NotBe(default);
            ss.SwitchKeyword.Kind().Should().Be(SyntaxKind.SwitchKeyword);
            ss.OpenParenToken.Should().NotBe(default);
            ss.Expression.Should().NotBeNull();
            ss.Expression.ToString().Should().Be("a");
            ss.CloseParenToken.Should().NotBe(default);
            ss.OpenBraceToken.Should().NotBe(default);
            ss.Sections.Count.Should().Be(0);
            ss.CloseBraceToken.Should().NotBe(default);
        }

        [Fact]
        public void TestSwitchWithCase()
        {
            var text = "switch (a) { case b:; }";
            var statement = this.ParseStatement(text);

            statement.Should().NotBeNull();
            statement.Kind().Should().Be(SyntaxKind.SwitchStatement);
            statement.ToString().Should().Be(text);
            statement.Errors().Length.Should().Be(0);

            var ss = (SwitchStatementSyntax)statement;
            ss.SwitchKeyword.Should().NotBe(default);
            ss.SwitchKeyword.Kind().Should().Be(SyntaxKind.SwitchKeyword);
            ss.OpenParenToken.Should().NotBe(default);
            ss.Expression.Should().NotBeNull();
            ss.Expression.ToString().Should().Be("a");
            ss.CloseParenToken.Should().NotBe(default);
            ss.OpenBraceToken.Should().NotBe(default);

            ss.Sections.Count.Should().Be(1);
            ss.Sections[0].Labels.Count.Should().Be(1);
            ss.Sections[0].Labels[0].Keyword.Should().NotBe(default);
            ss.Sections[0].Labels[0].Keyword.Kind().Should().Be(SyntaxKind.CaseKeyword);
            var caseLabelSyntax = ss.Sections[0].Labels[0] as CaseSwitchLabelSyntax;
            caseLabelSyntax.Should().NotBeNull();
            caseLabelSyntax.Value.Should().NotBeNull();
            caseLabelSyntax.Value.ToString().Should().Be("b");
            caseLabelSyntax.ColonToken.Should().NotBe(default);
            ss.Sections[0].Statements.Count.Should().Be(1);
            ss.Sections[0].Statements[0].ToString().Should().Be(";");

            ss.CloseBraceToken.Should().NotBe(default);
        }

        [Fact]
        public void TestSwitchWithMultipleCases()
        {
            var text = "switch (a) { case b:; case c:; }";
            var statement = this.ParseStatement(text);

            statement.Should().NotBeNull();
            statement.Kind().Should().Be(SyntaxKind.SwitchStatement);
            statement.ToString().Should().Be(text);
            statement.Errors().Length.Should().Be(0);

            var ss = (SwitchStatementSyntax)statement;
            ss.SwitchKeyword.Should().NotBe(default);
            ss.SwitchKeyword.Kind().Should().Be(SyntaxKind.SwitchKeyword);
            ss.OpenParenToken.Should().NotBe(default);
            ss.Expression.Should().NotBeNull();
            ss.Expression.ToString().Should().Be("a");
            ss.CloseParenToken.Should().NotBe(default);
            ss.OpenBraceToken.Should().NotBe(default);

            ss.Sections.Count.Should().Be(2);

            ss.Sections[0].Labels.Count.Should().Be(1);
            ss.Sections[0].Labels[0].Keyword.Should().NotBe(default);
            ss.Sections[0].Labels[0].Keyword.Kind().Should().Be(SyntaxKind.CaseKeyword);
            var caseLabelSyntax = ss.Sections[0].Labels[0] as CaseSwitchLabelSyntax;
            caseLabelSyntax.Should().NotBeNull();
            caseLabelSyntax.Value.Should().NotBeNull();
            caseLabelSyntax.Value.ToString().Should().Be("b");
            caseLabelSyntax.ColonToken.Should().NotBe(default);
            ss.Sections[0].Statements.Count.Should().Be(1);
            ss.Sections[0].Statements[0].ToString().Should().Be(";");

            ss.Sections[1].Labels.Count.Should().Be(1);
            ss.Sections[1].Labels[0].Keyword.Should().NotBe(default);
            ss.Sections[1].Labels[0].Keyword.Kind().Should().Be(SyntaxKind.CaseKeyword);
            var caseLabelSyntax2 = ss.Sections[1].Labels[0] as CaseSwitchLabelSyntax;
            caseLabelSyntax2.Should().NotBeNull();
            caseLabelSyntax2.Value.Should().NotBeNull();
            caseLabelSyntax2.Value.ToString().Should().Be("c");
            caseLabelSyntax2.ColonToken.Should().NotBe(default);
            ss.Sections[1].Statements.Count.Should().Be(1);
            ss.Sections[0].Statements[0].ToString().Should().Be(";");

            ss.CloseBraceToken.Should().NotBe(default);
        }

        [Fact]
        public void TestSwitchWithDefaultCase()
        {
            var text = "switch (a) { default:; }";
            var statement = this.ParseStatement(text);

            statement.Should().NotBeNull();
            statement.Kind().Should().Be(SyntaxKind.SwitchStatement);
            statement.ToString().Should().Be(text);
            statement.Errors().Length.Should().Be(0);

            var ss = (SwitchStatementSyntax)statement;
            ss.SwitchKeyword.Should().NotBe(default);
            ss.SwitchKeyword.Kind().Should().Be(SyntaxKind.SwitchKeyword);
            ss.OpenParenToken.Should().NotBe(default);
            ss.Expression.Should().NotBeNull();
            ss.Expression.ToString().Should().Be("a");
            ss.CloseParenToken.Should().NotBe(default);
            ss.OpenBraceToken.Should().NotBe(default);

            ss.Sections.Count.Should().Be(1);

            ss.Sections[0].Labels.Count.Should().Be(1);
            ss.Sections[0].Labels[0].Keyword.Should().NotBe(default);
            ss.Sections[0].Labels[0].Keyword.Kind().Should().Be(SyntaxKind.DefaultKeyword);
            ss.Sections[0].Labels[0].Kind().Should().Be(SyntaxKind.DefaultSwitchLabel);
            ss.Sections[0].Labels[0].ColonToken.Should().NotBe(default);
            ss.Sections[0].Statements.Count.Should().Be(1);
            ss.Sections[0].Statements[0].ToString().Should().Be(";");

            ss.CloseBraceToken.Should().NotBe(default);
        }

        [Fact]
        public void TestSwitchWithMultipleLabelsOnOneCase()
        {
            var text = "switch (a) { case b: case c:; }";
            var statement = this.ParseStatement(text);

            statement.Should().NotBeNull();
            statement.Kind().Should().Be(SyntaxKind.SwitchStatement);
            statement.ToString().Should().Be(text);
            statement.Errors().Length.Should().Be(0);

            var ss = (SwitchStatementSyntax)statement;
            ss.SwitchKeyword.Should().NotBe(default);
            ss.SwitchKeyword.Kind().Should().Be(SyntaxKind.SwitchKeyword);
            ss.OpenParenToken.Should().NotBe(default);
            ss.Expression.Should().NotBeNull();
            ss.Expression.ToString().Should().Be("a");
            ss.CloseParenToken.Should().NotBe(default);
            ss.OpenBraceToken.Should().NotBe(default);

            ss.Sections.Count.Should().Be(1);

            ss.Sections[0].Labels.Count.Should().Be(2);
            ss.Sections[0].Labels[0].Keyword.Should().NotBe(default);
            ss.Sections[0].Labels[0].Keyword.Kind().Should().Be(SyntaxKind.CaseKeyword);
            var caseLabelSyntax = ss.Sections[0].Labels[0] as CaseSwitchLabelSyntax;
            caseLabelSyntax.Should().NotBeNull();
            caseLabelSyntax.Value.Should().NotBeNull();
            caseLabelSyntax.Value.ToString().Should().Be("b");
            ss.Sections[0].Labels[1].Keyword.Should().NotBe(default);
            ss.Sections[0].Labels[1].Keyword.Kind().Should().Be(SyntaxKind.CaseKeyword);
            var caseLabelSyntax2 = ss.Sections[0].Labels[1] as CaseSwitchLabelSyntax;
            caseLabelSyntax2.Should().NotBeNull();
            caseLabelSyntax2.Value.Should().NotBeNull();
            caseLabelSyntax2.Value.ToString().Should().Be("c");
            ss.Sections[0].Labels[0].ColonToken.Should().NotBe(default);
            ss.Sections[0].Statements.Count.Should().Be(1);
            ss.Sections[0].Statements[0].ToString().Should().Be(";");

            ss.CloseBraceToken.Should().NotBe(default);
        }

        [Fact]
        public void TestSwitchWithMultipleStatementsOnOneCase()
        {
            var text = "switch (a) { case b: s1(); s2(); }";
            var statement = this.ParseStatement(text);

            statement.Should().NotBeNull();
            statement.Kind().Should().Be(SyntaxKind.SwitchStatement);
            statement.ToString().Should().Be(text);
            statement.Errors().Length.Should().Be(0);

            var ss = (SwitchStatementSyntax)statement;
            ss.SwitchKeyword.Should().NotBe(default);
            ss.SwitchKeyword.Kind().Should().Be(SyntaxKind.SwitchKeyword);
            ss.OpenParenToken.Should().NotBe(default);
            ss.Expression.Should().NotBeNull();
            ss.Expression.ToString().Should().Be("a");
            ss.CloseParenToken.Should().NotBe(default);
            ss.OpenBraceToken.Should().NotBe(default);

            ss.Sections.Count.Should().Be(1);

            ss.Sections[0].Labels.Count.Should().Be(1);
            ss.Sections[0].Labels[0].Keyword.Should().NotBe(default);
            ss.Sections[0].Labels[0].Keyword.Kind().Should().Be(SyntaxKind.CaseKeyword);
            var caseLabelSyntax = ss.Sections[0].Labels[0] as CaseSwitchLabelSyntax;
            caseLabelSyntax.Should().NotBeNull();
            caseLabelSyntax.Value.Should().NotBeNull();
            caseLabelSyntax.Value.ToString().Should().Be("b");
            ss.Sections[0].Statements.Count.Should().Be(2);
            ss.Sections[0].Statements[0].ToString().Should().Be("s1();");
            ss.Sections[0].Statements[1].ToString().Should().Be("s2();");

            ss.CloseBraceToken.Should().NotBe(default);
        }

        [Fact]
        public void TestUsingWithExpression()
        {
            var text = "using (a) { }";
            var statement = this.ParseStatement(text);

            statement.Should().NotBeNull();
            statement.Kind().Should().Be(SyntaxKind.UsingStatement);
            statement.ToString().Should().Be(text);
            statement.Errors().Length.Should().Be(0);

            var us = (UsingStatementSyntax)statement;
            us.UsingKeyword.Should().NotBe(default);
            us.UsingKeyword.Kind().Should().Be(SyntaxKind.UsingKeyword);
            us.OpenParenToken.Should().NotBe(default);
            us.Declaration.Should().BeNull();
            us.Expression.Should().NotBeNull();
            us.Expression.ToString().Should().Be("a");
            us.CloseParenToken.Should().NotBe(default);
            us.Statement.Should().NotBeNull();
        }

        [Fact]
        public void TestUsingWithDeclaration()
        {
            var text = "using (T a = b) { }";
            var statement = this.ParseStatement(text);

            statement.Should().NotBeNull();
            statement.Kind().Should().Be(SyntaxKind.UsingStatement);
            statement.ToString().Should().Be(text);
            statement.Errors().Length.Should().Be(0);

            var us = (UsingStatementSyntax)statement;
            us.UsingKeyword.Should().NotBe(default);
            us.UsingKeyword.Kind().Should().Be(SyntaxKind.UsingKeyword);
            us.OpenParenToken.Should().NotBe(default);

            us.Declaration.Should().NotBeNull();
            us.Declaration.Type.Should().NotBeNull();
            us.Declaration.Type.ToString().Should().Be("T");
            us.Declaration.Variables.Count.Should().Be(1);
            us.Declaration.Variables[0].Identifier.Should().NotBe(default);
            us.Declaration.Variables[0].Identifier.ToString().Should().Be("a");
            us.Declaration.Variables[0].ArgumentList.Should().BeNull();
            us.Declaration.Variables[0].Initializer.Should().NotBeNull();
            us.Declaration.Variables[0].Initializer.EqualsToken.Should().NotBe(default);
            us.Declaration.Variables[0].Initializer.Value.Should().NotBeNull();
            us.Declaration.Variables[0].Initializer.Value.ToString().Should().Be("b");

            us.Expression.Should().BeNull();

            us.CloseParenToken.Should().NotBe(default);
            us.Statement.Should().NotBeNull();
        }

        [Fact]
        public void TestUsingVarWithDeclaration()
        {
            var text = "using T a = b;";
            var statement = this.ParseStatement(text, options: TestOptions.Regular8);

            statement.Should().NotBeNull();
            statement.Kind().Should().Be(SyntaxKind.LocalDeclarationStatement);
            statement.ToString().Should().Be(text);
            statement.Errors().Length.Should().Be(0);

            var us = (LocalDeclarationStatementSyntax)statement;
            us.UsingKeyword.Should().NotBe(default);
            us.UsingKeyword.Kind().Should().Be(SyntaxKind.UsingKeyword);

            us.Declaration.Should().NotBeNull();
            us.Declaration.Type.Should().NotBeNull();
            us.Declaration.Type.ToString().Should().Be("T");
            us.Declaration.Variables.Count.Should().Be(1);
            us.Declaration.Variables[0].Identifier.Should().NotBe(default);
            us.Declaration.Variables[0].Identifier.ToString().Should().Be("a");
            us.Declaration.Variables[0].ArgumentList.Should().BeNull();
            us.Declaration.Variables[0].Initializer.Should().NotBeNull();
            us.Declaration.Variables[0].Initializer.EqualsToken.Should().NotBe(default);
            us.Declaration.Variables[0].Initializer.Value.Should().NotBeNull();
            us.Declaration.Variables[0].Initializer.Value.ToString().Should().Be("b");
            us.SemicolonToken.Should().NotBe(default);
        }

        [Fact]
        public void TestUsingVarWithDeclarationTree()
        {
            UsingStatement(@"using T a = b;", options: TestOptions.Regular8);
            N(SyntaxKind.LocalDeclarationStatement);
            {
                N(SyntaxKind.UsingKeyword);
                N(SyntaxKind.VariableDeclaration);
                {
                    N(SyntaxKind.IdentifierName, "T");
                    {
                        N(SyntaxKind.IdentifierToken);
                    }
                    N(SyntaxKind.VariableDeclarator);
                    {
                        N(SyntaxKind.IdentifierToken);
                        N(SyntaxKind.EqualsValueClause);
                        {
                            N(SyntaxKind.EqualsToken);
                            N(SyntaxKind.IdentifierName, "b");
                            {
                                N(SyntaxKind.IdentifierToken);
                            }
                        }
                    }
                }
                N(SyntaxKind.SemicolonToken);
            }
        }

        [Fact]
        public void TestUsingWithVarDeclaration()
        {
            var text = "using (var a = b) { }";
            var statement = this.ParseStatement(text);

            statement.Should().NotBeNull();
            statement.Kind().Should().Be(SyntaxKind.UsingStatement);
            statement.ToString().Should().Be(text);
            statement.Errors().Length.Should().Be(0);

            var us = (UsingStatementSyntax)statement;
            us.UsingKeyword.Should().NotBe(default);
            us.UsingKeyword.Kind().Should().Be(SyntaxKind.UsingKeyword);
            us.OpenParenToken.Should().NotBe(default);

            us.Declaration.Should().NotBeNull();
            us.Declaration.Type.Should().NotBeNull();
            us.Declaration.Type.ToString().Should().Be("var");
            us.Declaration.Type.Kind().Should().Be(SyntaxKind.IdentifierName);
            ((IdentifierNameSyntax)us.Declaration.Type).Identifier.Kind().Should().Be(SyntaxKind.IdentifierToken);
            us.Declaration.Variables.Count.Should().Be(1);
            us.Declaration.Variables[0].Identifier.Should().NotBe(default);
            us.Declaration.Variables[0].Identifier.ToString().Should().Be("a");
            us.Declaration.Variables[0].ArgumentList.Should().BeNull();
            us.Declaration.Variables[0].Initializer.Should().NotBeNull();
            us.Declaration.Variables[0].Initializer.EqualsToken.Should().NotBe(default);
            us.Declaration.Variables[0].Initializer.Value.Should().NotBeNull();
            us.Declaration.Variables[0].Initializer.Value.ToString().Should().Be("b");

            us.Expression.Should().BeNull();

            us.CloseParenToken.Should().NotBe(default);
            us.Statement.Should().NotBeNull();
        }

        [Fact]
        public void TestUsingVarWithVarDeclaration()
        {
            var text = "using var a = b;";
            var statement = this.ParseStatement(text, options: TestOptions.Regular8);

            statement.Should().NotBeNull();
            statement.Kind().Should().Be(SyntaxKind.LocalDeclarationStatement);
            statement.ToString().Should().Be(text);
            statement.Errors().Length.Should().Be(0);

            var us = (LocalDeclarationStatementSyntax)statement;
            us.UsingKeyword.Should().NotBe(default);
            us.UsingKeyword.Kind().Should().Be(SyntaxKind.UsingKeyword);

            us.Declaration.Should().NotBeNull();
            us.Declaration.Type.Should().NotBeNull();
            us.Declaration.Type.ToString().Should().Be("var");
            us.Declaration.Type.Kind().Should().Be(SyntaxKind.IdentifierName);
            ((IdentifierNameSyntax)us.Declaration.Type).Identifier.Kind().Should().Be(SyntaxKind.IdentifierToken);
            us.Declaration.Variables.Count.Should().Be(1);
            us.Declaration.Variables[0].Identifier.Should().NotBe(default);
            us.Declaration.Variables[0].Identifier.ToString().Should().Be("a");
            us.Declaration.Variables[0].ArgumentList.Should().BeNull();
            us.Declaration.Variables[0].Initializer.Should().NotBeNull();
            us.Declaration.Variables[0].Initializer.EqualsToken.Should().NotBe(default);
            us.Declaration.Variables[0].Initializer.Value.Should().NotBeNull();
            us.Declaration.Variables[0].Initializer.Value.ToString().Should().Be("b");
        }

        [Fact]
        [WorkItem(36413, "https://github.com/dotnet/roslyn/issues/36413")]
        public void TestUsingVarWithInvalidDeclaration()
        {
            var text = "using public readonly var a = b;";
            var statement = this.ParseStatement(text, options: TestOptions.Regular8);

            statement.Should().NotBeNull();
            statement.Kind().Should().Be(SyntaxKind.LocalDeclarationStatement);
            statement.ToString().Should().Be(text);
            statement.Errors().Length.Should().Be(2);
            statement.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_BadMemberFlag);
            statement.Errors()[0].Arguments[0].Should().Be("public");
            statement.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_BadMemberFlag);
            statement.Errors()[1].Arguments[0].Should().Be("readonly");

            var us = (LocalDeclarationStatementSyntax)statement;
            us.UsingKeyword.Should().NotBe(default);
            us.UsingKeyword.Kind().Should().Be(SyntaxKind.UsingKeyword);

            us.Declaration.Should().NotBeNull();
            us.Declaration.Type.Should().NotBeNull();
            us.Declaration.Type.ToString().Should().Be("var");
            us.Declaration.Type.Kind().Should().Be(SyntaxKind.IdentifierName);
            ((IdentifierNameSyntax)us.Declaration.Type).Identifier.Kind().Should().Be(SyntaxKind.IdentifierToken);
            us.Modifiers.Count.Should().Be(2);
            us.Modifiers[0].ToString().Should().Be("public");
            us.Modifiers[1].ToString().Should().Be("readonly");
            us.Declaration.Variables.Count.Should().Be(1);
            us.Declaration.Variables[0].Identifier.Should().NotBe(default);
            us.Declaration.Variables[0].Identifier.ToString().Should().Be("a");
            us.Declaration.Variables[0].ArgumentList.Should().BeNull();
            us.Declaration.Variables[0].Initializer.Should().NotBeNull();
            us.Declaration.Variables[0].Initializer.EqualsToken.Should().NotBe(default);
            us.Declaration.Variables[0].Initializer.Value.Should().NotBeNull();
            us.Declaration.Variables[0].Initializer.Value.ToString().Should().Be("b");
        }

        [Fact]
        public void TestUsingVarWithVarDeclarationTree()
        {
            UsingStatement(@"using var a = b;", options: TestOptions.Regular8);
            N(SyntaxKind.LocalDeclarationStatement);
            {
                N(SyntaxKind.UsingKeyword);
                N(SyntaxKind.VariableDeclaration);
                {
                    N(SyntaxKind.IdentifierName, "var");
                    {
                        N(SyntaxKind.IdentifierToken, "var");
                    }
                    N(SyntaxKind.VariableDeclarator);
                    {
                        N(SyntaxKind.IdentifierToken, "a");
                        N(SyntaxKind.EqualsValueClause);
                        {
                            N(SyntaxKind.EqualsToken);
                            N(SyntaxKind.IdentifierName, "b");
                            {
                                N(SyntaxKind.IdentifierToken);
                            }
                        }
                    }
                }
                N(SyntaxKind.SemicolonToken);
            }
        }

        [Fact]
        public void TestAwaitUsingVarWithDeclarationTree()
        {
            UsingStatement(@"await using T a = b;", TestOptions.Regular8);
            N(SyntaxKind.LocalDeclarationStatement);
            {
                N(SyntaxKind.AwaitKeyword);
                N(SyntaxKind.UsingKeyword);
                N(SyntaxKind.VariableDeclaration);
                {
                    N(SyntaxKind.IdentifierName, "T");
                    {
                        N(SyntaxKind.IdentifierToken);
                    }
                    N(SyntaxKind.VariableDeclarator);
                    {
                        N(SyntaxKind.IdentifierToken, "a");
                        N(SyntaxKind.EqualsValueClause);
                        {
                            N(SyntaxKind.EqualsToken);
                            N(SyntaxKind.IdentifierName, "b");
                            {
                                N(SyntaxKind.IdentifierToken);
                            }
                        }
                    }
                }
                N(SyntaxKind.SemicolonToken);
            }
        }

        [Fact]
        public void TestAwaitUsingWithVarDeclaration()
        {
            var text = "await using var a = b;";
            var statement = this.ParseStatement(text, 0, TestOptions.Regular8);

            statement.Should().NotBeNull();
            statement.Kind().Should().Be(SyntaxKind.LocalDeclarationStatement);
            statement.ToString().Should().Be(text);
            statement.Errors().Length.Should().Be(0);

            var us = (LocalDeclarationStatementSyntax)statement;
            us.AwaitKeyword.Should().NotBe(default);
            us.AwaitKeyword.ContextualKind().Should().Be(SyntaxKind.AwaitKeyword);
            us.UsingKeyword.Should().NotBe(default);
            us.UsingKeyword.Kind().Should().Be(SyntaxKind.UsingKeyword);

            us.Declaration.Should().NotBeNull();
            us.Declaration.Type.Should().NotBeNull();
            us.Declaration.Type.ToString().Should().Be("var");
            us.Declaration.Type.Kind().Should().Be(SyntaxKind.IdentifierName);
            ((IdentifierNameSyntax)us.Declaration.Type).Identifier.Kind().Should().Be(SyntaxKind.IdentifierToken);
            us.Declaration.Variables.Count.Should().Be(1);
            us.Declaration.Variables[0].Identifier.Should().NotBe(default);
            us.Declaration.Variables[0].Identifier.ToString().Should().Be("a");
            us.Declaration.Variables[0].ArgumentList.Should().BeNull();
            us.Declaration.Variables[0].Initializer.Should().NotBeNull();
            us.Declaration.Variables[0].Initializer.EqualsToken.Should().NotBe(default);
            us.Declaration.Variables[0].Initializer.Value.Should().NotBeNull();
            us.Declaration.Variables[0].Initializer.Value.ToString().Should().Be("b");
        }

        [Fact]
        public void TestAwaitUsingVarWithVarDeclarationTree()
        {
            UsingStatement(@"await using var a = b;", TestOptions.Regular8);
            N(SyntaxKind.LocalDeclarationStatement);
            {
                N(SyntaxKind.AwaitKeyword);
                N(SyntaxKind.UsingKeyword);
                N(SyntaxKind.VariableDeclaration);
                {
                    N(SyntaxKind.IdentifierName, "var");
                    {
                        N(SyntaxKind.IdentifierToken, "var");
                    }
                    N(SyntaxKind.VariableDeclarator);
                    {
                        N(SyntaxKind.IdentifierToken, "a");
                        N(SyntaxKind.EqualsValueClause);
                        {
                            N(SyntaxKind.EqualsToken);
                            N(SyntaxKind.IdentifierName, "b");
                            {
                                N(SyntaxKind.IdentifierToken);
                            }
                        }
                    }
                }
                N(SyntaxKind.SemicolonToken);
            }
        }

        [Fact, WorkItem(30565, "https://github.com/dotnet/roslyn/issues/30565")]
        public void AwaitUsingVarWithVarDecl_Reversed()
        {
            UsingTree(@"
class C
{
    async void M()
    {
        using await var x = null;
using AwesomeAssertions;
    }
}
",
                // (6,15): error CS4003: 'await' cannot be used as an identifier within an async method or lambda expression
                //         using await var x = null;
                Diagnostic(ErrorCode.ERR_BadAwaitAsIdentifier, "await").WithLocation(6, 15),
                // (6,25): error CS1002: ; expected
                //         using await var x = null;
                Diagnostic(ErrorCode.ERR_SemicolonExpected, "x").WithLocation(6, 25));
            N(SyntaxKind.CompilationUnit);
            {
                N(SyntaxKind.ClassDeclaration);
                {
                    N(SyntaxKind.ClassKeyword);
                    N(SyntaxKind.IdentifierToken, "C");
                    N(SyntaxKind.OpenBraceToken);
                    N(SyntaxKind.MethodDeclaration);
                    {
                        N(SyntaxKind.AsyncKeyword);
                        N(SyntaxKind.PredefinedType);
                        {
                            N(SyntaxKind.VoidKeyword);
                        }
                        N(SyntaxKind.IdentifierToken, "M");
                        N(SyntaxKind.ParameterList);
                        {
                            N(SyntaxKind.OpenParenToken);
                            N(SyntaxKind.CloseParenToken);
                        }
                        N(SyntaxKind.Block);
                        {
                            N(SyntaxKind.OpenBraceToken);
                            N(SyntaxKind.LocalDeclarationStatement);
                            {
                                N(SyntaxKind.UsingKeyword);
                                N(SyntaxKind.VariableDeclaration);
                                {
                                    N(SyntaxKind.IdentifierName);
                                    {
                                        N(SyntaxKind.IdentifierToken, "await");
                                    }
                                    N(SyntaxKind.VariableDeclarator);
                                    {
                                        N(SyntaxKind.IdentifierToken, "var");
                                    }
                                }
                                M(SyntaxKind.SemicolonToken);
                            }
                            N(SyntaxKind.ExpressionStatement);
                            {
                                N(SyntaxKind.SimpleAssignmentExpression);
                                {
                                    N(SyntaxKind.IdentifierName);
                                    {
                                        N(SyntaxKind.IdentifierToken, "x");
                                    }
                                    N(SyntaxKind.EqualsToken);
                                    N(SyntaxKind.NullLiteralExpression);
                                    {
                                        N(SyntaxKind.NullKeyword);
                                    }
                                }
                                N(SyntaxKind.SemicolonToken);
                            }
                            N(SyntaxKind.CloseBraceToken);
                        }
                    }
                    N(SyntaxKind.CloseBraceToken);
                }
                N(SyntaxKind.EndOfFileToken);
            }
            EOF();
        }

        [Fact]
        public void TestAwaitUsingVarWithVarAndNoUsingDeclarationTree()
        {
            UsingStatement(@"await var a = b;", TestOptions.Regular8,
                // (1,1): error CS1073: Unexpected token 'a'
                // await var a = b;
                Diagnostic(ErrorCode.ERR_UnexpectedToken, "await var ").WithArguments("a").WithLocation(1, 1),
                // (1,11): error CS1002: ; expected
                // await var a = b;
                Diagnostic(ErrorCode.ERR_SemicolonExpected, "a").WithLocation(1, 11));

            N(SyntaxKind.ExpressionStatement);
            {
                N(SyntaxKind.AwaitExpression);
                {
                    N(SyntaxKind.AwaitKeyword);
                    N(SyntaxKind.IdentifierName);
                    {
                        N(SyntaxKind.IdentifierToken, "var");
                    }
                }
                M(SyntaxKind.SemicolonToken);
            }
            EOF();
        }

        [Fact]
        public void TestUsingWithDeclarationWithMultipleVariables()
        {
            var text = "using (T a = b, c = d) { }";
            var statement = this.ParseStatement(text);

            statement.Should().NotBeNull();
            statement.Kind().Should().Be(SyntaxKind.UsingStatement);
            statement.ToString().Should().Be(text);
            statement.Errors().Length.Should().Be(0);

            var us = (UsingStatementSyntax)statement;
            us.UsingKeyword.Should().NotBe(default);
            us.UsingKeyword.Kind().Should().Be(SyntaxKind.UsingKeyword);
            us.OpenParenToken.Should().NotBe(default);

            us.Declaration.Should().NotBeNull();
            us.Declaration.Type.Should().NotBeNull();
            us.Declaration.Type.ToString().Should().Be("T");

            us.Declaration.Variables.Count.Should().Be(2);

            us.Declaration.Variables[0].Identifier.Should().NotBe(default);
            us.Declaration.Variables[0].Identifier.ToString().Should().Be("a");
            us.Declaration.Variables[0].ArgumentList.Should().BeNull();
            us.Declaration.Variables[0].Initializer.Should().NotBeNull();
            us.Declaration.Variables[0].Initializer.EqualsToken.Should().NotBe(default);
            us.Declaration.Variables[0].Initializer.Value.Should().NotBeNull();
            us.Declaration.Variables[0].Initializer.Value.ToString().Should().Be("b");

            us.Declaration.Variables[1].Identifier.Should().NotBe(default);
            us.Declaration.Variables[1].Identifier.ToString().Should().Be("c");
            us.Declaration.Variables[1].ArgumentList.Should().BeNull();
            us.Declaration.Variables[1].Initializer.Should().NotBeNull();
            us.Declaration.Variables[1].Initializer.EqualsToken.Should().NotBe(default);
            us.Declaration.Variables[1].Initializer.Value.Should().NotBeNull();
            us.Declaration.Variables[1].Initializer.Value.ToString().Should().Be("d");

            us.Expression.Should().BeNull();

            us.CloseParenToken.Should().NotBe(default);
            us.Statement.Should().NotBeNull();
        }

        [Fact]
        public void TestUsingVarWithDeclarationWithMultipleVariables()
        {
            var text = "using T a = b, c = d;";
            var statement = this.ParseStatement(text, options: TestOptions.Regular8);

            statement.Should().NotBeNull();
            statement.Kind().Should().Be(SyntaxKind.LocalDeclarationStatement);
            statement.ToString().Should().Be(text);
            statement.Errors().Length.Should().Be(0);

            var us = (LocalDeclarationStatementSyntax)statement;
            us.UsingKeyword.Should().NotBe(default);
            us.UsingKeyword.Kind().Should().Be(SyntaxKind.UsingKeyword);

            us.Declaration.Should().NotBeNull();
            us.Declaration.Type.Should().NotBeNull();
            us.Declaration.Type.ToString().Should().Be("T");

            us.Declaration.Variables.Count.Should().Be(2);

            us.Declaration.Variables[0].Identifier.Should().NotBe(default);
            us.Declaration.Variables[0].Identifier.ToString().Should().Be("a");
            us.Declaration.Variables[0].ArgumentList.Should().BeNull();
            us.Declaration.Variables[0].Initializer.Should().NotBeNull();
            us.Declaration.Variables[0].Initializer.EqualsToken.Should().NotBe(default);
            us.Declaration.Variables[0].Initializer.Value.Should().NotBeNull();
            us.Declaration.Variables[0].Initializer.Value.ToString().Should().Be("b");

            us.Declaration.Variables[1].Identifier.Should().NotBe(default);
            us.Declaration.Variables[1].Identifier.ToString().Should().Be("c");
            us.Declaration.Variables[1].ArgumentList.Should().BeNull();
            us.Declaration.Variables[1].Initializer.Should().NotBeNull();
            us.Declaration.Variables[1].Initializer.EqualsToken.Should().NotBe(default);
            us.Declaration.Variables[1].Initializer.Value.Should().NotBeNull();
            us.Declaration.Variables[1].Initializer.Value.ToString().Should().Be("d");
        }

        [Fact]
        public void TestUsingVarWithDeclarationMultipleVariablesTree()
        {
            UsingStatement(@"using T a = b, c = d;", options: TestOptions.Regular8);
            N(SyntaxKind.LocalDeclarationStatement);
            {
                N(SyntaxKind.UsingKeyword);
                N(SyntaxKind.VariableDeclaration);
                {
                    N(SyntaxKind.IdentifierName, "T");
                    {
                        N(SyntaxKind.IdentifierToken, "T");
                    }
                    N(SyntaxKind.VariableDeclarator);
                    {
                        N(SyntaxKind.IdentifierToken, "a");
                        N(SyntaxKind.EqualsValueClause);
                        {
                            N(SyntaxKind.EqualsToken);
                            N(SyntaxKind.IdentifierName, "b");
                            {
                                N(SyntaxKind.IdentifierToken, "b");
                            }
                        }
                    }
                    N(SyntaxKind.CommaToken);
                    N(SyntaxKind.VariableDeclarator);
                    {
                        N(SyntaxKind.IdentifierToken, "c");
                        N(SyntaxKind.EqualsValueClause);
                        {
                            N(SyntaxKind.EqualsToken);
                            N(SyntaxKind.IdentifierName, "d");
                            {
                                N(SyntaxKind.IdentifierToken, "d");
                            }
                        }
                    }
                }
                N(SyntaxKind.SemicolonToken);
            }
        }

        [Fact]
        public void TestUsingSpecialCase1()
        {
            var text = "using (f ? x = a : x = b) { }";
            var statement = this.ParseStatement(text);

            statement.Should().NotBeNull();
            statement.Kind().Should().Be(SyntaxKind.UsingStatement);
            statement.ToString().Should().Be(text);
            statement.Errors().Length.Should().Be(0);

            var us = (UsingStatementSyntax)statement;
            us.UsingKeyword.Should().NotBe(default);
            us.UsingKeyword.Kind().Should().Be(SyntaxKind.UsingKeyword);
            us.OpenParenToken.Should().NotBe(default);
            us.Declaration.Should().BeNull();
            us.Expression.Should().NotBeNull();
            us.Expression.ToString().Should().Be("f ? x = a : x = b");
            us.CloseParenToken.Should().NotBe(default);
            us.Statement.Should().NotBeNull();
        }

        [Fact]
        public void TestUsingVarSpecialCase1()
        {
            var text = "using var x = f ? a : b;";
            var statement = this.ParseStatement(text, options: TestOptions.Regular8);

            statement.Should().NotBeNull();
            statement.Kind().Should().Be(SyntaxKind.LocalDeclarationStatement);
            statement.ToString().Should().Be(text);
            statement.Errors().Length.Should().Be(0);

            var us = (LocalDeclarationStatementSyntax)statement;
            us.UsingKeyword.Should().NotBe(default);
            us.UsingKeyword.Kind().Should().Be(SyntaxKind.UsingKeyword);
            us.Declaration.Should().NotBeNull();
            us.Declaration.ToString().Should().Be("var x = f ? a : b");
        }

        [Fact]
        public void TestUsingVarSpecialCase1Tree()
        {
            UsingStatement(@"using var x = f ? a : b;", options: TestOptions.Regular8);
            N(SyntaxKind.LocalDeclarationStatement);
            {
                N(SyntaxKind.UsingKeyword);
                N(SyntaxKind.VariableDeclaration);
                {
                    N(SyntaxKind.IdentifierName, "var");
                    {
                        N(SyntaxKind.IdentifierToken, "var");
                    }
                    N(SyntaxKind.VariableDeclarator);
                    {
                        N(SyntaxKind.IdentifierToken, "x");
                        N(SyntaxKind.EqualsValueClause);
                        {
                            N(SyntaxKind.EqualsToken);
                            N(SyntaxKind.ConditionalExpression);
                            {
                                N(SyntaxKind.IdentifierName, "f");
                                {
                                    N(SyntaxKind.IdentifierToken, "f");
                                }
                                N(SyntaxKind.QuestionToken);
                                N(SyntaxKind.IdentifierName, "a");
                                {
                                    N(SyntaxKind.IdentifierToken, "a");
                                }
                                N(SyntaxKind.ColonToken);
                                N(SyntaxKind.IdentifierName, "b");
                                {
                                    N(SyntaxKind.IdentifierToken, "b");
                                }
                            }
                        }
                    }
                }
                N(SyntaxKind.SemicolonToken);
            }
        }

        [Fact]
        public void TestUsingSpecialCase2()
        {
            var text = "using (f ? x = a) { }";
            var statement = this.ParseStatement(text, options: TestOptions.Regular8);

            statement.Should().NotBeNull();
            statement.Kind().Should().Be(SyntaxKind.UsingStatement);
            statement.ToString().Should().Be(text);
            statement.Errors().Length.Should().Be(0);

            var us = (UsingStatementSyntax)statement;
            us.UsingKeyword.Should().NotBe(default);
            us.UsingKeyword.Kind().Should().Be(SyntaxKind.UsingKeyword);
            us.OpenParenToken.Should().NotBe(default);
            us.Declaration.Should().NotBeNull();
            us.Declaration.ToString().Should().Be("f ? x = a");
            us.Expression.Should().BeNull();
            us.CloseParenToken.Should().NotBe(default);
            us.Statement.Should().NotBeNull();
        }

        [Fact]
        public void TestUsingVarSpecialCase2()
        {
            var text = "using f ? x = a;";
            var statement = this.ParseStatement(text, options: TestOptions.Regular8);

            statement.Should().NotBeNull();
            statement.Kind().Should().Be(SyntaxKind.LocalDeclarationStatement);
            statement.ToString().Should().Be(text);
            statement.Errors().Length.Should().Be(0);

            var us = (LocalDeclarationStatementSyntax)statement;
            us.UsingKeyword.Should().NotBe(default);
            us.UsingKeyword.Kind().Should().Be(SyntaxKind.UsingKeyword);
            us.Declaration.Should().NotBeNull();
            us.Declaration.ToString().Should().Be("f ? x = a");
        }

        [Fact]
        public void TestUsingVarSpecialCase2Tree()
        {
            UsingStatement(@"using f ? x = a;", options: TestOptions.Regular8);
            N(SyntaxKind.LocalDeclarationStatement);
            {
                N(SyntaxKind.UsingKeyword);
                N(SyntaxKind.VariableDeclaration);
                {
                    N(SyntaxKind.NullableType);
                    {
                        N(SyntaxKind.IdentifierName, "f");
                        {
                            N(SyntaxKind.IdentifierToken, "f");
                        }
                        N(SyntaxKind.QuestionToken);
                    }
                    N(SyntaxKind.VariableDeclarator);
                    N(SyntaxKind.IdentifierToken, "x");
                    N(SyntaxKind.EqualsValueClause);
                    {
                        N(SyntaxKind.EqualsToken);
                        N(SyntaxKind.IdentifierName, "a");
                        {
                            N(SyntaxKind.IdentifierToken, "a");
                        }
                    }
                }
                N(SyntaxKind.SemicolonToken);
            }
        }

        [Fact]
        public void TestUsingSpecialCase3()
        {
            var text = "using (f ? x, y) { }";
            var statement = this.ParseStatement(text);

            statement.Should().NotBeNull();
            statement.Kind().Should().Be(SyntaxKind.UsingStatement);
            statement.ToString().Should().Be(text);
            statement.Errors().Length.Should().Be(0);

            var us = (UsingStatementSyntax)statement;
            us.UsingKeyword.Should().NotBe(default);
            us.UsingKeyword.Kind().Should().Be(SyntaxKind.UsingKeyword);
            us.OpenParenToken.Should().NotBe(default);
            us.Declaration.Should().NotBeNull();
            us.Declaration.ToString().Should().Be("f ? x, y");
            us.Expression.Should().BeNull();
            us.CloseParenToken.Should().NotBe(default);
            us.Statement.Should().NotBeNull();
        }

        [Fact]
        public void TestUsingVarSpecialCase3()
        {
            var text = "using f ? x, y;";
            var statement = this.ParseStatement(text, options: TestOptions.Regular8);

            statement.Should().NotBeNull();
            statement.Kind().Should().Be(SyntaxKind.LocalDeclarationStatement);
            statement.ToString().Should().Be(text);
            statement.Errors().Length.Should().Be(0);

            var us = (LocalDeclarationStatementSyntax)statement;
            us.UsingKeyword.Should().NotBe(default);
            us.UsingKeyword.Kind().Should().Be(SyntaxKind.UsingKeyword);
            us.Declaration.Should().NotBeNull();
            us.Declaration.ToString().Should().Be("f ? x, y");
        }

        [Fact]
        public void TestUsingVarSpecialCase3Tree()
        {
            UsingStatement("using f? x, y;", options: TestOptions.Regular8);
            N(SyntaxKind.LocalDeclarationStatement);
            {
                N(SyntaxKind.UsingKeyword);
                N(SyntaxKind.VariableDeclaration);
                {
                    N(SyntaxKind.NullableType);
                    {
                        N(SyntaxKind.IdentifierName, "f");
                        {
                            N(SyntaxKind.IdentifierToken, "f");
                        }
                        N(SyntaxKind.QuestionToken);
                    }
                    N(SyntaxKind.VariableDeclarator);
                    {
                        N(SyntaxKind.IdentifierToken, "x");
                    }
                    N(SyntaxKind.CommaToken);
                    N(SyntaxKind.VariableDeclarator);
                    {
                        N(SyntaxKind.IdentifierToken, "y");
                    }
                }
            }
            N(SyntaxKind.SemicolonToken);
        }

        [Fact]
        public void TestUsingVarRefTree()
        {
            UsingStatement("using ref int x = ref y;", TestOptions.Regular8);
            N(SyntaxKind.LocalDeclarationStatement);
            {
                N(SyntaxKind.UsingKeyword);
                N(SyntaxKind.VariableDeclaration);
                {
                    N(SyntaxKind.RefType);
                    {
                        N(SyntaxKind.RefKeyword);
                        N(SyntaxKind.PredefinedType);
                        {
                            N(SyntaxKind.IntKeyword);
                        }
                    }
                    N(SyntaxKind.VariableDeclarator);
                    {
                        N(SyntaxKind.IdentifierToken, "x");
                        N(SyntaxKind.EqualsValueClause);
                        {
                            N(SyntaxKind.EqualsToken);
                            N(SyntaxKind.RefExpression);
                            {
                                N(SyntaxKind.RefKeyword);
                                N(SyntaxKind.IdentifierName, "y");
                                {
                                    N(SyntaxKind.IdentifierToken, "y");
                                }
                            }
                        }
                    }
                }
            }
            N(SyntaxKind.SemicolonToken);
        }

        [Fact]
        public void TestUsingVarRefReadonlyTree()
        {
            UsingStatement("using ref readonly int x = ref y;", TestOptions.Regular8);
            N(SyntaxKind.LocalDeclarationStatement);
            {
                N(SyntaxKind.UsingKeyword);
                N(SyntaxKind.VariableDeclaration);
                {
                    N(SyntaxKind.RefType);
                    {
                        N(SyntaxKind.RefKeyword);
                        N(SyntaxKind.ReadOnlyKeyword);
                        N(SyntaxKind.PredefinedType);
                        {
                            N(SyntaxKind.IntKeyword);
                        }
                    }
                    N(SyntaxKind.VariableDeclarator);
                    {
                        N(SyntaxKind.IdentifierToken, "x");
                        N(SyntaxKind.EqualsValueClause);
                        {
                            N(SyntaxKind.EqualsToken);
                            N(SyntaxKind.RefExpression);
                            {
                                N(SyntaxKind.RefKeyword);
                                N(SyntaxKind.IdentifierName, "y");
                                {
                                    N(SyntaxKind.IdentifierToken, "y");
                                }
                            }
                        }
                    }
                }
            }
            N(SyntaxKind.SemicolonToken);
        }

        [Fact]
        public void TestUsingVarRefVarTree()
        {
            UsingStatement("using ref var x = ref y;", TestOptions.Regular8);
            N(SyntaxKind.LocalDeclarationStatement);
            {
                N(SyntaxKind.UsingKeyword);
                N(SyntaxKind.VariableDeclaration);
                {
                    N(SyntaxKind.RefType);
                    {
                        N(SyntaxKind.RefKeyword);
                        N(SyntaxKind.IdentifierName, "var");
                        {
                            N(SyntaxKind.IdentifierToken, "var");
                        }
                    }
                    N(SyntaxKind.VariableDeclarator);
                    {
                        N(SyntaxKind.IdentifierToken, "x");
                        N(SyntaxKind.EqualsValueClause);
                        {
                            N(SyntaxKind.EqualsToken);
                            N(SyntaxKind.RefExpression);
                            {
                                N(SyntaxKind.RefKeyword);
                                N(SyntaxKind.IdentifierName, "y");
                                {
                                    N(SyntaxKind.IdentifierToken, "y");
                                }
                            }
                        }
                    }
                }
            }
            N(SyntaxKind.SemicolonToken);
        }

        [Fact]
        public void TestUsingVarRefVarIsYTree()
        {
            UsingStatement("using ref var x = y;", TestOptions.Regular8);
            N(SyntaxKind.LocalDeclarationStatement);
            {
                N(SyntaxKind.UsingKeyword);
                N(SyntaxKind.VariableDeclaration);
                {
                    N(SyntaxKind.RefType);
                    {
                        N(SyntaxKind.RefKeyword);
                        N(SyntaxKind.IdentifierName, "var");
                        {
                            N(SyntaxKind.IdentifierToken, "var");
                        }
                    }
                    N(SyntaxKind.VariableDeclarator);
                    {
                        N(SyntaxKind.IdentifierToken, "x");
                        N(SyntaxKind.EqualsValueClause);
                        {
                            N(SyntaxKind.EqualsToken);
                            N(SyntaxKind.IdentifierName, "y");
                            {
                                N(SyntaxKind.IdentifierToken, "y");
                            }
                        }
                    }
                }
            }
            N(SyntaxKind.SemicolonToken);
        }

        [Fact]
        public void TestUsingVarReadonlyMultipleDeclarations()
        {
            UsingStatement("using readonly var x, y = ref z;", TestOptions.Regular8,
                // (1,7): error CS0106: The modifier 'readonly' is not valid for this item
                // using readonly var x, y = ref z;
                Diagnostic(ErrorCode.ERR_BadMemberFlag, "readonly").WithArguments("readonly").WithLocation(1, 7));
            N(SyntaxKind.LocalDeclarationStatement);
            {
                N(SyntaxKind.UsingKeyword);
                N(SyntaxKind.ReadOnlyKeyword);
                N(SyntaxKind.VariableDeclaration);
                {
                    N(SyntaxKind.IdentifierName, "var");
                    {
                        N(SyntaxKind.IdentifierToken, "var");
                    }
                    N(SyntaxKind.VariableDeclarator);
                    {
                        N(SyntaxKind.IdentifierToken, "x");
                    }
                    N(SyntaxKind.CommaToken);
                    N(SyntaxKind.VariableDeclarator);
                    {
                        N(SyntaxKind.IdentifierToken, "y");
                    }
                    N(SyntaxKind.EqualsValueClause);
                    {
                        N(SyntaxKind.EqualsToken);
                        N(SyntaxKind.RefExpression);
                        {
                            N(SyntaxKind.RefKeyword);
                            N(SyntaxKind.IdentifierName, "z");
                            {
                                N(SyntaxKind.IdentifierToken, "z");
                            }
                        }
                    }
                }
            }
            N(SyntaxKind.SemicolonToken);
        }

        [Fact]
        public void TestContextualKeywordsAsLocalVariableTypes()
        {
            TestContextualKeywordAsLocalVariableType(SyntaxKind.PartialKeyword);
            TestContextualKeywordAsLocalVariableType(SyntaxKind.AsyncKeyword);
            TestContextualKeywordAsLocalVariableType(SyntaxKind.AwaitKeyword);
        }

        private void TestContextualKeywordAsLocalVariableType(SyntaxKind kind)
        {
            var keywordText = SyntaxFacts.GetText(kind);
            var text = keywordText + " o = null;";
            var statement = this.ParseStatement(text);
            statement.Should().NotBeNull();
            statement.Kind().Should().Be(SyntaxKind.LocalDeclarationStatement);
            statement.ToString().Should().Be(text);

            var decl = (LocalDeclarationStatementSyntax)statement;
            decl.Declaration.Type.ToString().Should().Be(keywordText);
            decl.Declaration.Type.Should().BeOfType<IdentifierNameSyntax>();
            var name = (IdentifierNameSyntax)decl.Declaration.Type;
            name.Identifier.ContextualKind().Should().Be(kind);
            name.Identifier.Kind().Should().Be(SyntaxKind.IdentifierToken);
        }

        [Fact]
        public void Bug862649()
        {
            var text = @"static char[] delimiter;";
            var tree = SyntaxFactory.ParseStatement(text);
            var toText = tree.ToFullString();
            toText.Should().Be(text);
        }

        [Fact]
        public void TestForEachAfterOffset()
        {
            const string prefix = "GARBAGE";
            var text = "foreach(T a in b) { }";
            var statement = this.ParseStatement(prefix + text, offset: prefix.Length);

            statement.Should().NotBeNull();
            statement.Kind().Should().Be(SyntaxKind.ForEachStatement);
            statement.ToString().Should().Be(text);
            statement.Errors().Length.Should().Be(0);

            var fs = (ForEachStatementSyntax)statement;
            fs.ForEachKeyword.Should().NotBe(default);
            fs.ForEachKeyword.Kind().Should().Be(SyntaxKind.ForEachKeyword);

            fs.OpenParenToken.Should().NotBe(default);
            fs.Type.Should().NotBeNull();
            fs.Type.ToString().Should().Be("T");
            fs.Identifier.Should().NotBe(default);
            fs.Identifier.ToString().Should().Be("a");
            fs.InKeyword.Should().NotBe(default);
            fs.InKeyword.IsMissing.Should().BeFalse();
            fs.InKeyword.Kind().Should().Be(SyntaxKind.InKeyword);
            fs.Expression.Should().NotBeNull();
            fs.Expression.ToString().Should().Be("b");
            fs.CloseParenToken.Should().NotBe(default);
            fs.Statement.Should().NotBeNull();
        }

        [WorkItem(684860, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/684860")]
        [Fact]
        public void Bug684860_SkippedTokens()
        {
            const int n = 100000;
            // 100000 instances of "0+" in:
            // #pragma warning disable 1 0+0+0+...
            var builder = new System.Text.StringBuilder();
            builder.Append("#pragma warning disable 1 ");
            for (int i = 0; i < n; i++)
            {
                builder.Append("0+");
            }
            builder.AppendLine();
            var text = builder.ToString();
            var tree = SyntaxFactory.ParseSyntaxTree(text);
            var root = tree.GetRoot();
            var walker = new TokenAndTriviaWalker();
            walker.Visit(root);
            walker.Tokens > n.Should().BeTrue();
            var tokens1 = root.DescendantTokens(descendIntoTrivia: false).ToArray();
            var tokens2 = root.DescendantTokens(descendIntoTrivia: true).ToArray();
            (tokens2.Length - tokens1.Length) > n.Should().BeTrue();
        }

        [WorkItem(684860, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/684860")]
        [Fact]
        public void Bug684860_XmlText()
        {
            const int n = 100000;
            // 100000 instances of "&lt;" in:
            // /// <x a="&lt;&lt;&lt;..."/>
            // class { }
            var builder = new System.Text.StringBuilder();
            builder.Append("/// <x a=\"");
            for (int i = 0; i < n; i++)
            {
                builder.Append("&lt;");
            }
            builder.AppendLine("\"/>");
            builder.AppendLine("class C { }");
            var text = builder.ToString();
            var tree = SyntaxFactory.ParseSyntaxTree(text, options: new CSharpParseOptions(documentationMode: DocumentationMode.Parse));
            var root = tree.GetRoot();
            var walker = new TokenAndTriviaWalker();
            walker.Visit(root);
            walker.Tokens > n.Should().BeTrue();
            var tokens = root.DescendantTokens(descendIntoTrivia: true).ToArray();
            tokens.Length > n.Should().BeTrue();
        }

        [Fact]
        public void ExceptionFilter_IfKeyword()
        {
            const string source = @"
class C
{
    void M()
    {
        try { }
        catch (System.Exception e) if (true) { }
    }
}
";

            var tree = SyntaxFactory.ParseSyntaxTree(source);
            var root = tree.GetRoot();
            tree.GetDiagnostics(root).Verify(
                // (7,36): error CS1003: Syntax error, 'when' expected
                //         catch (System.Exception e) if (true) { }
                CSharpTestBase.Diagnostic(ErrorCode.ERR_SyntaxError, "if").WithArguments("when").WithLocation(7, 36));

            var filterClause = root.DescendantNodes().OfType<CatchFilterClauseSyntax>().Single();
            filterClause.WhenKeyword.Kind().Should().Be(SyntaxKind.WhenKeyword);
            filterClause.WhenKeyword.HasStructuredTrivia.Should().BeTrue();
        }

        [Fact]
        public void Tuple001()
        {
            var source = @"
class C1
{
    static void Test(int arg1, (byte, byte) arg2)
    {
        (int, int)? t1 = new(int, int)?();
        (int, int)? t1a = new(int, int)?((1,1));
        (int, int)? t1b = new(int, int)?[1];
        (int, int)? t1c = new(int, int)?[] {(1,1)};

        (int, int)? t2 = default((int a, int b));

        (int, int) t3 = (a: (int)arg1, b: (int)arg1);

        (int, int) t4 = ((int a, int b))(arg1, arg1);
        (int, int) t5 = ((int, int))arg2;

        List<(int, int)> l = new List<(int, int)>() { (a: arg1, b: arg1), (arg1, arg1) };

        Func<(int a, int b), (int a, int b)> f = ((int a, int b) t) => t;
        
        var x = from i in ""qq""
                from j in ""ee""
                select (i, j);

        foreach ((int, int) e in new (int, int)[10])
        {
        }
    }
}
";
            var tree = SyntaxFactory.ParseSyntaxTree(source, options: TestOptions.Regular);
            tree.GetDiagnostics().Verify();
        }

        [Fact]
        [WorkItem(684860, "https://devdiv.visualstudio.com/DevDiv/_workitems/edit/266237")]
        public void DevDiv266237()
        {
            var source = @"
class Program
{
    static void Go()
    {
        using (var p = new P
        {

        }

    protected override void M()
    {

    }
}
";

            var tree = SyntaxFactory.ParseSyntaxTree(source, options: TestOptions.Regular);
            tree.GetDiagnostics(tree.GetRoot()).Verify(
                // (9,10): error CS1026: ) expected
                //         }
                CSharpTestBase.Diagnostic(ErrorCode.ERR_CloseParenExpected, "").WithLocation(9, 10),
                // (9,10): error CS1002: ; expected
                //         }
                CSharpTestBase.Diagnostic(ErrorCode.ERR_SemicolonExpected, "").WithLocation(9, 10),
                // (9,10): error CS1513: } expected
                //         }
                CSharpTestBase.Diagnostic(ErrorCode.ERR_RbraceExpected, "").WithLocation(9, 10));
        }

        [WorkItem(6676, "https://github.com/dotnet/roslyn/issues/6676")]
        [Fact]
        public void TestRunEmbeddedStatementNotFollowedBySemicolon()
        {
            var text = @"if (true)
System.Console.WriteLine(true)";
            var statement = this.ParseStatement(text);

            statement.Should().NotBeNull();
            statement.Kind().Should().Be(SyntaxKind.IfStatement);
            statement.ToString().Should().Be(text);
            statement.Errors().Length.Should().Be(1);
            statement.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_SemicolonExpected);
        }

        [WorkItem(266237, "https://devdiv.visualstudio.com/DefaultCollection/DevDiv/_workitems?_a=edit&id=266237")]
        [Fact]
        public void NullExceptionInLabeledStatement()
        {
            UsingStatement(@"{ label: public",
                // (1,1): error CS1073: Unexpected token 'public'
                // { label: public
                Diagnostic(ErrorCode.ERR_UnexpectedToken, "{ label: ").WithArguments("public").WithLocation(1, 1),
                // (1,10): error CS1002: ; expected
                // { label: public
                Diagnostic(ErrorCode.ERR_SemicolonExpected, "public").WithLocation(1, 10),
                // (1,10): error CS1513: } expected
                // { label: public
                Diagnostic(ErrorCode.ERR_RbraceExpected, "public").WithLocation(1, 10)
                );

            N(SyntaxKind.Block);
            {
                N(SyntaxKind.OpenBraceToken);
                N(SyntaxKind.LabeledStatement);
                {
                    N(SyntaxKind.IdentifierToken, "label");
                    N(SyntaxKind.ColonToken);
                    M(SyntaxKind.EmptyStatement);
                    {
                        M(SyntaxKind.SemicolonToken);
                    }
                }
                M(SyntaxKind.CloseBraceToken);
            }
            EOF();
        }

        [WorkItem(27866, "https://github.com/dotnet/roslyn/issues/27866")]
        [Fact]
        public void ParseElseWithoutPrecedingIfStatement()
        {
            UsingStatement("else {}",
                // (1,1): error CS8641: 'else' cannot start a statement.
                // else {}
                Diagnostic(ErrorCode.ERR_ElseCannotStartStatement, "else").WithLocation(1, 1),
                // (1,1): error CS1003: Syntax error, '(' expected
                // else {}
                Diagnostic(ErrorCode.ERR_SyntaxError, "else").WithArguments("(").WithLocation(1, 1),
                // (1,1): error CS1525: Invalid expression term 'else'
                // else {}
                Diagnostic(ErrorCode.ERR_InvalidExprTerm, "else").WithArguments("else").WithLocation(1, 1),
                // (1,1): error CS1026: ) expected
                // else {}
                Diagnostic(ErrorCode.ERR_CloseParenExpected, "else").WithLocation(1, 1),
                // (1,1): error CS1525: Invalid expression term 'else'
                // else {}
                Diagnostic(ErrorCode.ERR_InvalidExprTerm, "else").WithArguments("else").WithLocation(1, 1),
                // (1,1): error CS1002: ; expected
                // else {}
                Diagnostic(ErrorCode.ERR_SemicolonExpected, "else").WithLocation(1, 1)
                );
            N(SyntaxKind.IfStatement);
            {
                M(SyntaxKind.IfKeyword);
                M(SyntaxKind.OpenParenToken);
                M(SyntaxKind.IdentifierName);
                {
                    M(SyntaxKind.IdentifierToken);
                }
                M(SyntaxKind.CloseParenToken);
                M(SyntaxKind.ExpressionStatement);
                {
                    M(SyntaxKind.IdentifierName);
                    {
                        M(SyntaxKind.IdentifierToken);
                    }
                    M(SyntaxKind.SemicolonToken);
                }
                N(SyntaxKind.ElseClause);
                {
                    N(SyntaxKind.ElseKeyword);
                    N(SyntaxKind.Block);
                    {
                        N(SyntaxKind.OpenBraceToken);
                        N(SyntaxKind.CloseBraceToken);
                    }
                }
            }
            EOF();
        }

        [WorkItem(27866, "https://github.com/dotnet/roslyn/issues/27866")]
        [Fact]
        public void ParseElseAndElseWithoutPrecedingIfStatement()
        {
            UsingStatement("{ else {} else {} }",
                // (1,3): error CS8641: 'else' cannot start a statement.
                // { else {} else {} }
                Diagnostic(ErrorCode.ERR_ElseCannotStartStatement, "else").WithLocation(1, 3),
                // (1,3): error CS1003: Syntax error, '(' expected
                // { else {} else {} }
                Diagnostic(ErrorCode.ERR_SyntaxError, "else").WithArguments("(").WithLocation(1, 3),
                // (1,3): error CS1525: Invalid expression term 'else'
                // { else {} else {} }
                Diagnostic(ErrorCode.ERR_InvalidExprTerm, "else").WithArguments("else").WithLocation(1, 3),
                // (1,3): error CS1026: ) expected
                // { else {} else {} }
                Diagnostic(ErrorCode.ERR_CloseParenExpected, "else").WithLocation(1, 3),
                // (1,3): error CS1525: Invalid expression term 'else'
                // { else {} else {} }
                Diagnostic(ErrorCode.ERR_InvalidExprTerm, "else").WithArguments("else").WithLocation(1, 3),
                // (1,3): error CS1002: ; expected
                // { else {} else {} }
                Diagnostic(ErrorCode.ERR_SemicolonExpected, "else").WithLocation(1, 3),
                // (1,11): error CS8641: 'else' cannot start a statement.
                // { else {} else {} }
                Diagnostic(ErrorCode.ERR_ElseCannotStartStatement, "else").WithLocation(1, 11),
                // (1,11): error CS1003: Syntax error, '(' expected
                // { else {} else {} }
                Diagnostic(ErrorCode.ERR_SyntaxError, "else").WithArguments("(").WithLocation(1, 11),
                // (1,11): error CS1525: Invalid expression term 'else'
                // { else {} else {} }
                Diagnostic(ErrorCode.ERR_InvalidExprTerm, "else").WithArguments("else").WithLocation(1, 11),
                // (1,11): error CS1026: ) expected
                // { else {} else {} }
                Diagnostic(ErrorCode.ERR_CloseParenExpected, "else").WithLocation(1, 11),
                // (1,11): error CS1525: Invalid expression term 'else'
                // { else {} else {} }
                Diagnostic(ErrorCode.ERR_InvalidExprTerm, "else").WithArguments("else").WithLocation(1, 11),
                // (1,11): error CS1002: ; expected
                // { else {} else {} }
                Diagnostic(ErrorCode.ERR_SemicolonExpected, "else").WithLocation(1, 11)
                );
            N(SyntaxKind.Block);
            {
                N(SyntaxKind.OpenBraceToken);
                N(SyntaxKind.IfStatement);
                {
                    M(SyntaxKind.IfKeyword);
                    M(SyntaxKind.OpenParenToken);
                    M(SyntaxKind.IdentifierName);
                    {
                        M(SyntaxKind.IdentifierToken);
                    }
                    M(SyntaxKind.CloseParenToken);
                    M(SyntaxKind.ExpressionStatement);
                    {
                        M(SyntaxKind.IdentifierName);
                        {
                            M(SyntaxKind.IdentifierToken);
                        }
                        M(SyntaxKind.SemicolonToken);
                    }
                    N(SyntaxKind.ElseClause);
                    {
                        N(SyntaxKind.ElseKeyword);
                        N(SyntaxKind.Block);
                        {
                            N(SyntaxKind.OpenBraceToken);
                            N(SyntaxKind.CloseBraceToken);
                        }
                    }
                }
                N(SyntaxKind.IfStatement);
                {
                    M(SyntaxKind.IfKeyword);
                    M(SyntaxKind.OpenParenToken);
                    M(SyntaxKind.IdentifierName);
                    {
                        M(SyntaxKind.IdentifierToken);
                    }
                    M(SyntaxKind.CloseParenToken);
                    M(SyntaxKind.ExpressionStatement);
                    {
                        M(SyntaxKind.IdentifierName);
                        {
                            M(SyntaxKind.IdentifierToken);
                        }
                        M(SyntaxKind.SemicolonToken);
                    }
                    N(SyntaxKind.ElseClause);
                    {
                        N(SyntaxKind.ElseKeyword);
                        N(SyntaxKind.Block);
                        {
                            N(SyntaxKind.OpenBraceToken);
                            N(SyntaxKind.CloseBraceToken);
                        }
                    }
                }
                N(SyntaxKind.CloseBraceToken);
            }
            EOF();
        }

        [WorkItem(27866, "https://github.com/dotnet/roslyn/issues/27866")]
        [Fact]
        public void ParseSubsequentElseWithoutPrecedingIfStatement()
        {
            UsingStatement("{ if (a) { } else { } else { } }",
                // (1,23): error CS8641: 'else' cannot start a statement.
                // { if (a) { } else { } else { } }
                Diagnostic(ErrorCode.ERR_ElseCannotStartStatement, "else").WithLocation(1, 23),
                // (1,23): error CS1003: Syntax error, '(' expected
                // { if (a) { } else { } else { } }
                Diagnostic(ErrorCode.ERR_SyntaxError, "else").WithArguments("(").WithLocation(1, 23),
                // (1,23): error CS1525: Invalid expression term 'else'
                // { if (a) { } else { } else { } }
                Diagnostic(ErrorCode.ERR_InvalidExprTerm, "else").WithArguments("else").WithLocation(1, 23),
                // (1,23): error CS1026: ) expected
                // { if (a) { } else { } else { } }
                Diagnostic(ErrorCode.ERR_CloseParenExpected, "else").WithLocation(1, 23),
                // (1,23): error CS1525: Invalid expression term 'else'
                // { if (a) { } else { } else { } }
                Diagnostic(ErrorCode.ERR_InvalidExprTerm, "else").WithArguments("else").WithLocation(1, 23),
                // (1,23): error CS1002: ; expected
                // { if (a) { } else { } else { } }
                Diagnostic(ErrorCode.ERR_SemicolonExpected, "else").WithLocation(1, 23)
                );
            N(SyntaxKind.Block);
            {
                N(SyntaxKind.OpenBraceToken);
                N(SyntaxKind.IfStatement);
                {
                    N(SyntaxKind.IfKeyword);
                    N(SyntaxKind.OpenParenToken);
                    N(SyntaxKind.IdentifierName);
                    {
                        N(SyntaxKind.IdentifierToken, "a");
                    }
                    N(SyntaxKind.CloseParenToken);
                    N(SyntaxKind.Block);
                    {
                        N(SyntaxKind.OpenBraceToken);
                        N(SyntaxKind.CloseBraceToken);
                    }
                    N(SyntaxKind.ElseClause);
                    {
                        N(SyntaxKind.ElseKeyword);
                        N(SyntaxKind.Block);
                        {
                            N(SyntaxKind.OpenBraceToken);
                            N(SyntaxKind.CloseBraceToken);
                        }
                    }
                }
                N(SyntaxKind.IfStatement);
                {
                    M(SyntaxKind.IfKeyword);
                    M(SyntaxKind.OpenParenToken);
                    M(SyntaxKind.IdentifierName);
                    {
                        M(SyntaxKind.IdentifierToken);
                    }
                    M(SyntaxKind.CloseParenToken);
                    M(SyntaxKind.ExpressionStatement);
                    {
                        M(SyntaxKind.IdentifierName);
                        {
                            M(SyntaxKind.IdentifierToken);
                        }
                        M(SyntaxKind.SemicolonToken);
                    }
                    N(SyntaxKind.ElseClause);
                    {
                        N(SyntaxKind.ElseKeyword);
                        N(SyntaxKind.Block);
                        {
                            N(SyntaxKind.OpenBraceToken);
                            N(SyntaxKind.CloseBraceToken);
                        }
                    }
                }
                N(SyntaxKind.CloseBraceToken);
            }
            EOF();
        }

        [WorkItem(27866, "https://github.com/dotnet/roslyn/issues/27866")]
        [Fact]
        public void ParseElseKeywordPlacedAsIfEmbeddedStatement()
        {
            UsingStatement("if (a) else {}",
                // (1,8): error CS8641: 'else' cannot start a statement.
                // if (a) else {}
                Diagnostic(ErrorCode.ERR_ElseCannotStartStatement, "else").WithLocation(1, 8),
                // (1,8): error CS1003: Syntax error, '(' expected
                // if (a) else {}
                Diagnostic(ErrorCode.ERR_SyntaxError, "else").WithArguments("(").WithLocation(1, 8),
                // (1,8): error CS1525: Invalid expression term 'else'
                // if (a) else {}
                Diagnostic(ErrorCode.ERR_InvalidExprTerm, "else").WithArguments("else").WithLocation(1, 8),
                // (1,8): error CS1026: ) expected
                // if (a) else {}
                Diagnostic(ErrorCode.ERR_CloseParenExpected, "else").WithLocation(1, 8),
                // (1,8): error CS1525: Invalid expression term 'else'
                // if (a) else {}
                Diagnostic(ErrorCode.ERR_InvalidExprTerm, "else").WithArguments("else").WithLocation(1, 8),
                // (1,8): error CS1002: ; expected
                // if (a) else {}
                Diagnostic(ErrorCode.ERR_SemicolonExpected, "else").WithLocation(1, 8)
                );
            N(SyntaxKind.IfStatement);
            {
                N(SyntaxKind.IfKeyword);
                N(SyntaxKind.OpenParenToken);
                N(SyntaxKind.IdentifierName);
                {
                    N(SyntaxKind.IdentifierToken, "a");
                }
                N(SyntaxKind.CloseParenToken);
                N(SyntaxKind.IfStatement);
                {
                    M(SyntaxKind.IfKeyword);
                    M(SyntaxKind.OpenParenToken);
                    M(SyntaxKind.IdentifierName);
                    {
                        M(SyntaxKind.IdentifierToken);
                    }
                    M(SyntaxKind.CloseParenToken);
                    M(SyntaxKind.ExpressionStatement);
                    {
                        M(SyntaxKind.IdentifierName);
                        {
                            M(SyntaxKind.IdentifierToken);
                        }
                        M(SyntaxKind.SemicolonToken);
                    }
                    N(SyntaxKind.ElseClause);
                    {
                        N(SyntaxKind.ElseKeyword);
                        N(SyntaxKind.Block);
                        {
                            N(SyntaxKind.OpenBraceToken);
                            N(SyntaxKind.CloseBraceToken);
                        }
                    }
                }
            }
            EOF();
        }

        [Fact]
        public void ParseSwitch01()
        {
            UsingStatement("switch 1+2 {}",
                // (1,8): error CS8415: Parentheses are required around the switch governing expression.
                // switch 1+2 {}
                Diagnostic(ErrorCode.ERR_SwitchGoverningExpressionRequiresParens, "1+2").WithLocation(1, 8)
                );
            N(SyntaxKind.SwitchStatement);
            {
                N(SyntaxKind.SwitchKeyword);
                M(SyntaxKind.OpenParenToken);
                N(SyntaxKind.AddExpression);
                {
                    N(SyntaxKind.NumericLiteralExpression);
                    {
                        N(SyntaxKind.NumericLiteralToken, "1");
                    }
                    N(SyntaxKind.PlusToken);
                    N(SyntaxKind.NumericLiteralExpression);
                    {
                        N(SyntaxKind.NumericLiteralToken, "2");
                    }
                }
                M(SyntaxKind.CloseParenToken);
                N(SyntaxKind.OpenBraceToken);
                N(SyntaxKind.CloseBraceToken);
            }
            EOF();
        }

        [Fact]
        public void ParseSwitch02()
        {
            UsingStatement("switch (a: 0) {}",
                // (1,13): error CS8124: Tuple must contain at least two elements.
                // switch (a: 0) {}
                Diagnostic(ErrorCode.ERR_TupleTooFewElements, ")").WithLocation(1, 13)
                );
            N(SyntaxKind.SwitchStatement);
            {
                N(SyntaxKind.SwitchKeyword);
                N(SyntaxKind.TupleExpression);
                {
                    N(SyntaxKind.OpenParenToken);
                    N(SyntaxKind.Argument);
                    {
                        N(SyntaxKind.NameColon);
                        {
                            N(SyntaxKind.IdentifierName);
                            {
                                N(SyntaxKind.IdentifierToken, "a");
                            }
                            N(SyntaxKind.ColonToken);
                        }
                        N(SyntaxKind.NumericLiteralExpression);
                        {
                            N(SyntaxKind.NumericLiteralToken, "0");
                        }
                    }
                    M(SyntaxKind.CommaToken);
                    M(SyntaxKind.Argument);
                    {
                        M(SyntaxKind.IdentifierName);
                        {
                            M(SyntaxKind.IdentifierToken);
                        }
                    }
                    N(SyntaxKind.CloseParenToken);
                }
                N(SyntaxKind.OpenBraceToken);
                N(SyntaxKind.CloseBraceToken);
            }
            EOF();
        }

        [Fact]
        public void ParseSwitch03()
        {
            UsingStatement("switch (a: 0, b: 4) {}");
            N(SyntaxKind.SwitchStatement);
            {
                N(SyntaxKind.SwitchKeyword);
                N(SyntaxKind.TupleExpression);
                {
                    N(SyntaxKind.OpenParenToken);
                    N(SyntaxKind.Argument);
                    {
                        N(SyntaxKind.NameColon);
                        {
                            N(SyntaxKind.IdentifierName);
                            {
                                N(SyntaxKind.IdentifierToken, "a");
                            }
                            N(SyntaxKind.ColonToken);
                        }
                        N(SyntaxKind.NumericLiteralExpression);
                        {
                            N(SyntaxKind.NumericLiteralToken, "0");
                        }
                    }
                    N(SyntaxKind.CommaToken);
                    N(SyntaxKind.Argument);
                    {
                        N(SyntaxKind.NameColon);
                        {
                            N(SyntaxKind.IdentifierName);
                            {
                                N(SyntaxKind.IdentifierToken, "b");
                            }
                            N(SyntaxKind.ColonToken);
                        }
                        N(SyntaxKind.NumericLiteralExpression);
                        {
                            N(SyntaxKind.NumericLiteralToken, "4");
                        }
                    }
                    N(SyntaxKind.CloseParenToken);
                }
                N(SyntaxKind.OpenBraceToken);
                N(SyntaxKind.CloseBraceToken);
            }
            EOF();
        }

        [Fact]
        public void ParseSwitch04()
        {
            UsingStatement("switch (1) + (2) {}",
                // (1,8): error CS8415: Parentheses are required around the switch governing expression.
                // switch (1) + (2) {}
                Diagnostic(ErrorCode.ERR_SwitchGoverningExpressionRequiresParens, "(1) + (2)").WithLocation(1, 8)
                );
            N(SyntaxKind.SwitchStatement);
            {
                N(SyntaxKind.SwitchKeyword);
                M(SyntaxKind.OpenParenToken);
                N(SyntaxKind.AddExpression);
                {
                    N(SyntaxKind.ParenthesizedExpression);
                    {
                        N(SyntaxKind.OpenParenToken);
                        N(SyntaxKind.NumericLiteralExpression);
                        {
                            N(SyntaxKind.NumericLiteralToken, "1");
                        }
                        N(SyntaxKind.CloseParenToken);
                    }
                    N(SyntaxKind.PlusToken);
                    N(SyntaxKind.ParenthesizedExpression);
                    {
                        N(SyntaxKind.OpenParenToken);
                        N(SyntaxKind.NumericLiteralExpression);
                        {
                            N(SyntaxKind.NumericLiteralToken, "2");
                        }
                        N(SyntaxKind.CloseParenToken);
                    }
                }
                M(SyntaxKind.CloseParenToken);
                N(SyntaxKind.OpenBraceToken);
                N(SyntaxKind.CloseBraceToken);
            }
            EOF();
        }

        [Fact]
        public void ParseCreateNullableTuple_01()
        {
            UsingStatement("_ = new (int, int)? {};");
            N(SyntaxKind.ExpressionStatement);
            {
                N(SyntaxKind.SimpleAssignmentExpression);
                {
                    N(SyntaxKind.IdentifierName);
                    {
                        N(SyntaxKind.IdentifierToken, "_");
                    }
                    N(SyntaxKind.EqualsToken);
                    N(SyntaxKind.ObjectCreationExpression);
                    {
                        N(SyntaxKind.NewKeyword);
                        N(SyntaxKind.NullableType);
                        {
                            N(SyntaxKind.TupleType);
                            {
                                N(SyntaxKind.OpenParenToken);
                                N(SyntaxKind.TupleElement);
                                {
                                    N(SyntaxKind.PredefinedType);
                                    {
                                        N(SyntaxKind.IntKeyword);
                                    }
                                }
                                N(SyntaxKind.CommaToken);
                                N(SyntaxKind.TupleElement);
                                {
                                    N(SyntaxKind.PredefinedType);
                                    {
                                        N(SyntaxKind.IntKeyword);
                                    }
                                }
                                N(SyntaxKind.CloseParenToken);
                            }
                            N(SyntaxKind.QuestionToken);
                        }
                        N(SyntaxKind.ObjectInitializerExpression);
                        {
                            N(SyntaxKind.OpenBraceToken);
                            N(SyntaxKind.CloseBraceToken);
                        }
                    }
                }
                N(SyntaxKind.SemicolonToken);
            }
            EOF();
        }

        [Fact]
        public void ParseCreateNullableTuple_02()
        {
            UsingStatement("_ = new (int, int) ? (x) : (y);",
                // (1,1): error CS1073: Unexpected token ':'
                // _ = new (int, int) ? (x) : (y);
                Diagnostic(ErrorCode.ERR_UnexpectedToken, "_ = new (int, int) ? (x) ").WithArguments(":").WithLocation(1, 1),
                // (1,26): error CS1002: ; expected
                // _ = new (int, int) ? (x) : (y);
                Diagnostic(ErrorCode.ERR_SemicolonExpected, ":").WithLocation(1, 26)
                );
            N(SyntaxKind.ExpressionStatement);
            {
                N(SyntaxKind.SimpleAssignmentExpression);
                {
                    N(SyntaxKind.IdentifierName);
                    {
                        N(SyntaxKind.IdentifierToken, "_");
                    }
                    N(SyntaxKind.EqualsToken);
                    N(SyntaxKind.ObjectCreationExpression);
                    {
                        N(SyntaxKind.NewKeyword);
                        N(SyntaxKind.NullableType);
                        {
                            N(SyntaxKind.TupleType);
                            {
                                N(SyntaxKind.OpenParenToken);
                                N(SyntaxKind.TupleElement);
                                {
                                    N(SyntaxKind.PredefinedType);
                                    {
                                        N(SyntaxKind.IntKeyword);
                                    }
                                }
                                N(SyntaxKind.CommaToken);
                                N(SyntaxKind.TupleElement);
                                {
                                    N(SyntaxKind.PredefinedType);
                                    {
                                        N(SyntaxKind.IntKeyword);
                                    }
                                }
                                N(SyntaxKind.CloseParenToken);
                            }
                            N(SyntaxKind.QuestionToken);
                        }
                        N(SyntaxKind.ArgumentList);
                        {
                            N(SyntaxKind.OpenParenToken);
                            N(SyntaxKind.Argument);
                            {
                                N(SyntaxKind.IdentifierName);
                                {
                                    N(SyntaxKind.IdentifierToken, "x");
                                }
                            }
                            N(SyntaxKind.CloseParenToken);
                        }
                    }
                }
                M(SyntaxKind.SemicolonToken);
            }
            EOF();
        }

        [Fact]
        public void ParsePointerToArray()
        {
            UsingStatement("int []* p;",
                // (1,7): error CS1001: Identifier expected
                // int []* p;
                Diagnostic(ErrorCode.ERR_IdentifierExpected, "*").WithLocation(1, 7),
                // (1,7): error CS1003: Syntax error, ',' expected
                // int []* p;
                Diagnostic(ErrorCode.ERR_SyntaxError, "*").WithArguments(",").WithLocation(1, 7)
                );
            N(SyntaxKind.LocalDeclarationStatement);
            {
                N(SyntaxKind.VariableDeclaration);
                {
                    N(SyntaxKind.ArrayType);
                    {
                        N(SyntaxKind.PredefinedType);
                        {
                            N(SyntaxKind.IntKeyword);
                        }
                        N(SyntaxKind.ArrayRankSpecifier);
                        {
                            N(SyntaxKind.OpenBracketToken);
                            N(SyntaxKind.OmittedArraySizeExpression);
                            {
                                N(SyntaxKind.OmittedArraySizeExpressionToken);
                            }
                            N(SyntaxKind.CloseBracketToken);
                        }
                    }
                    M(SyntaxKind.VariableDeclarator);
                    {
                        M(SyntaxKind.IdentifierToken);
                    }
                }
                N(SyntaxKind.SemicolonToken);
            }
            EOF();
        }

        [Fact]
        public void ParseNewNullableWithInitializer()
        {
            UsingStatement("_ = new int? {};");
            N(SyntaxKind.ExpressionStatement);
            {
                N(SyntaxKind.SimpleAssignmentExpression);
                {
                    N(SyntaxKind.IdentifierName);
                    {
                        N(SyntaxKind.IdentifierToken, "_");
                    }
                    N(SyntaxKind.EqualsToken);
                    N(SyntaxKind.ObjectCreationExpression);
                    {
                        N(SyntaxKind.NewKeyword);
                        N(SyntaxKind.NullableType);
                        {
                            N(SyntaxKind.PredefinedType);
                            {
                                N(SyntaxKind.IntKeyword);
                            }
                            N(SyntaxKind.QuestionToken);
                        }
                        N(SyntaxKind.ObjectInitializerExpression);
                        {
                            N(SyntaxKind.OpenBraceToken);
                            N(SyntaxKind.CloseBraceToken);
                        }
                    }
                }
                N(SyntaxKind.SemicolonToken);
            }
            EOF();
        }

        private sealed class TokenAndTriviaWalker : CSharpSyntaxWalker
        {
            public int Tokens;
            public TokenAndTriviaWalker()
                : base(SyntaxWalkerDepth.StructuredTrivia)
            {
            }
            public override void VisitToken(SyntaxToken token)
            {
                Tokens++;
                base.VisitToken(token);
            }
        }
    }
}
