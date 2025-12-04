// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#nullable disable

using System;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp.Test.Utilities;
using Microsoft.CodeAnalysis.Test.Utilities;
using Roslyn.Test.Utilities;
using Xunit;

namespace Microsoft.CodeAnalysis.CSharp.UnitTests
{
    public class ParsingErrorRecoveryTests : CSharpTestBase
    {
        private CompilationUnitSyntax ParseTree(string text, CSharpParseOptions options = null)
        {
            return SyntaxFactory.ParseCompilationUnit(text, options: options);
        }

        [Theory]
        [InlineData("public")]
        [InlineData("internal")]
        [InlineData("protected")]
        [InlineData("private")]
        public void AccessibilityModifierErrorRecovery(string accessibility)
        {
            var file = ParseTree($@"
class C
{{
    void M()
    {{
        // bad visibility modifier
        {accessibility} void localFunc() {{}}
    }}
    void M2()
    {{
        typing
        {accessibility} void localFunc() {{}}
    }}
    void M3()
    {{
    // Ambiguous between local func with bad modifier and missing closing
    // brace on previous method. Parsing currently assumes the former,
    // assuming the tokens are parseable as a local func.
    {accessibility} void M4() {{}}
}}");

            file.Should().NotBeNull();
            file.GetDiagnostics().Verify(
                // (7,9): error CS0106: The modifier '{accessibility}' is not valid for this item
                //         {accessibility} void localFunc() {}
                Diagnostic(ErrorCode.ERR_BadMemberFlag, accessibility).WithArguments(accessibility).WithLocation(7, 9),
                // (11,15): error CS1002: ; expected
                //         typing
                Diagnostic(ErrorCode.ERR_SemicolonExpected, "").WithLocation(11, 15),
                // (12,9): error CS0106: The modifier '{accessibility}' is not valid for this item
                //         {accessibility} void localFunc() {}
                Diagnostic(ErrorCode.ERR_BadMemberFlag, accessibility).WithArguments(accessibility).WithLocation(12, 9),
                // (19,5): error CS0106: The modifier '{accessibility}' is not valid for this item
                //     {accessibility} void M4() {}
                Diagnostic(ErrorCode.ERR_BadMemberFlag, accessibility).WithArguments(accessibility).WithLocation(19, 5),
                // (20,2): error CS1513: } expected
                // }
                Diagnostic(ErrorCode.ERR_RbraceExpected, "").WithLocation(20, 2)
                );
        }

        [Fact]
        public void TestGlobalAttributeGarbageAfterLocation()
        {
            var text = "[assembly: $";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.AttributeLists.Count.Should().Be(1);
            file.Errors().Length.Should().Be(3);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_IdentifierExpected);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_UnexpectedCharacter);
            file.Errors()[2].Code.Should().Be((int)ErrorCode.ERR_SyntaxError);
        }

        [Fact]
        public void TestGlobalAttributeUsingAfterLocation()
        {
            var text = "[assembly: using n;";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.AttributeLists.Count.Should().Be(1);
            file.Members.Count.Should().Be(0);
            file.Errors().Length.Should().Be(3);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_IdentifierExpected);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_SyntaxError);
            file.Errors()[2].Code.Should().Be((int)ErrorCode.ERR_UsingAfterElements);
        }

        [Fact]
        public void TestGlobalAttributeExternAfterLocation()
        {
            var text = "[assembly: extern alias a;";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.AttributeLists.Count.Should().Be(1);
            file.Members.Count.Should().Be(0);
            file.Errors().Length.Should().Be(3);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_IdentifierExpected);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_SyntaxError);
            file.Errors()[2].Code.Should().Be((int)ErrorCode.ERR_ExternAfterElements);
        }

        [Fact]
        public void TestGlobalAttributeNamespaceAfterLocation()
        {
            var text = "[assembly: namespace n { }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.AttributeLists.Count.Should().Be(1);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.NamespaceDeclaration);
            file.Errors().Length.Should().Be(2);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_IdentifierExpected);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_SyntaxError);
        }

        [Fact]
        public void TestGlobalAttributeClassAfterLocation()
        {
            var text = "[assembly: class c { }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.AttributeLists.Count.Should().Be(1);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            file.Errors().Length.Should().Be(2);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_IdentifierExpected);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_SyntaxError);
        }

        [Fact]
        public void TestGlobalAttributeAttributeAfterLocation()
        {
            var text = "[assembly: [assembly: attr]";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.AttributeLists.Count.Should().Be(2);
            file.Members.Count.Should().Be(0);
            file.Errors().Length.Should().Be(2);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_IdentifierExpected);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_SyntaxError);
        }

        [Fact]
        public void TestGlobalAttributeEOFAfterLocation()
        {
            var text = "[assembly: ";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.AttributeLists.Count.Should().Be(1);
            file.Members.Count.Should().Be(0);
            file.Errors().Length.Should().Be(2);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_IdentifierExpected);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_SyntaxError);
        }

        [Fact]
        public void TestGlobalAttributeGarbageAfterAttribute()
        {
            var text = "[assembly: a $";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.AttributeLists.Count.Should().Be(1);
            file.Members.Count.Should().Be(0);
            file.Errors().Length.Should().Be(2);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_UnexpectedCharacter);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_SyntaxError);
        }

        [Fact]
        public void TestGlobalAttributeGarbageAfterParameterStart()
        {
            var text = "[assembly: a( $";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.AttributeLists.Count.Should().Be(1);
            file.Members.Count.Should().Be(0);
            file.Errors().Length.Should().Be(3);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_UnexpectedCharacter);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_CloseParenExpected);
            file.Errors()[2].Code.Should().Be((int)ErrorCode.ERR_SyntaxError);
        }

        [Fact]
        public void TestGlobalAttributeGarbageAfterParameter()
        {
            var text = "[assembly: a(b $";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.AttributeLists.Count.Should().Be(1);
            file.Members.Count.Should().Be(0);
            file.Errors().Length.Should().Be(3);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_UnexpectedCharacter);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_CloseParenExpected);
            file.Errors()[2].Code.Should().Be((int)ErrorCode.ERR_SyntaxError);
        }

        [Fact]
        public void TestGlobalAttributeMissingCommaBetweenParameters()
        {
            var text = "[assembly: a(b c)";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.AttributeLists.Count.Should().Be(1);
            file.Members.Count.Should().Be(0);
            file.Errors().Length.Should().Be(2);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_SyntaxError);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_SyntaxError);
        }

        [Fact]
        public void TestGlobalAttributeWithGarbageBetweenParameters()
        {
            var text = "[assembly: a(b $ c)";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.AttributeLists.Count.Should().Be(1);
            file.Members.Count.Should().Be(0);
            file.Errors().Length.Should().Be(3);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_UnexpectedCharacter);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_SyntaxError);
            file.Errors()[2].Code.Should().Be((int)ErrorCode.ERR_SyntaxError);
        }

        [Fact]
        public void TestGlobalAttributeWithGarbageBetweenAttributes()
        {
            var text = "[assembly: a $ b";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.AttributeLists.Count.Should().Be(1);
            file.Members.Count.Should().Be(0);
            file.Errors().Length.Should().Be(3);
            file.Errors().Verify(
                // error CS1056: Unexpected character '$'
                Diagnostic(ErrorCode.ERR_UnexpectedCharacter).WithArguments("$"),
                // error CS1003: Syntax error, ',' expected
                Diagnostic(ErrorCode.ERR_SyntaxError).WithArguments(","),
                // error CS1003: Syntax error, ']' expected
                Diagnostic(ErrorCode.ERR_SyntaxError).WithArguments("]")
                );
        }

        [Fact]
        public void TestGlobalAttributeWithUsingAfterParameterStart()
        {
            var text = "[assembly: a( using n;";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.AttributeLists.Count.Should().Be(1);
            file.Members.Count.Should().Be(0);
            file.Errors().Length.Should().Be(3);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_CloseParenExpected);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_SyntaxError);
            file.Errors()[2].Code.Should().Be((int)ErrorCode.ERR_UsingAfterElements);
        }

        [Fact]
        public void TestGlobalAttributeWithExternAfterParameterStart()
        {
            var text = "[assembly: a( extern alias n;";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.AttributeLists.Count.Should().Be(1);
            file.Members.Count.Should().Be(0);
            file.Errors().Length.Should().Be(3);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_CloseParenExpected);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_SyntaxError);
            file.Errors()[2].Code.Should().Be((int)ErrorCode.ERR_ExternAfterElements);
        }

        [Fact]
        public void TestGlobalAttributeWithNamespaceAfterParameterStart()
        {
            var text = "[assembly: a( namespace n { }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.AttributeLists.Count.Should().Be(1);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.NamespaceDeclaration);
            file.Errors().Length.Should().Be(2);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_CloseParenExpected);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_SyntaxError);
        }

        [Fact]
        public void TestGlobalAttributeWithClassAfterParameterStart()
        {
            var text = "[assembly: a( class c { }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.AttributeLists.Count.Should().Be(1);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            file.Errors().Length.Should().Be(2);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_CloseParenExpected);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_SyntaxError);
        }

        [Fact]
        public void TestGarbageBeforeNamespace()
        {
            var text = "$ namespace n { }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.NamespaceDeclaration);
            file.Errors().Length.Should().Be(1);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_UnexpectedCharacter);
        }

        [Fact]
        public void TestGarbageAfterNamespace()
        {
            var text = "namespace n { } $";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.NamespaceDeclaration);
            file.Errors().Length.Should().Be(1);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_UnexpectedCharacter);
        }

        [Fact]
        public void MultipleSubsequentMisplacedCharactersSingleError1()
        {
            var text = "namespace n { } ,,,,,,,,";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.NamespaceDeclaration);
            file.Errors().Length.Should().Be(1);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_EOFExpected);
        }

        [Fact]
        public void MultipleSubsequentMisplacedCharactersSingleError2()
        {
            var text = ",,,, namespace n { } ,,,,";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.NamespaceDeclaration);
            file.Errors().Length.Should().Be(2);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_EOFExpected);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_EOFExpected);
        }

        [Fact]
        public void TestGarbageInsideNamespace()
        {
            var text = "namespace n { $ }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.NamespaceDeclaration);
            file.Errors().Length.Should().Be(1);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_UnexpectedCharacter);
        }

        [Fact]
        public void TestIncompleteGlobalMembers()
        {
            var text = @"
asas]
extern alias A;
asas
using System;
sadasdasd]

[assembly: goo]

class C
{
}


[a]fod;
[b";
            var file = this.ParseTree(text);
            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
        }

        [Fact]
        public void TestAttributeWithGarbageAfterStart()
        {
            var text = "[ $";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.IncompleteMember);
            file.Errors().Length.Should().Be(3);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_IdentifierExpected);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_UnexpectedCharacter);
            file.Errors()[2].Code.Should().Be((int)ErrorCode.ERR_SyntaxError);
        }

        [Fact]
        public void TestAttributeWithGarbageAfterName()
        {
            var text = "[a $";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.IncompleteMember);
            file.Errors().Length.Should().Be(2);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_UnexpectedCharacter);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_SyntaxError);
        }

        [Fact]
        public void TestAttributeWithClassAfterBracket()
        {
            var text = "[ class c { }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            file.Errors().Length.Should().Be(2);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_IdentifierExpected);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_SyntaxError);
        }

        [Fact]
        public void TestAttributeWithClassAfterName()
        {
            var text = "[a class c { }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            file.Errors().Length.Should().Be(1);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_SyntaxError);
        }

        [Fact]
        public void TestAttributeWithClassAfterParameterStart()
        {
            var text = "[a( class c { }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            file.Errors().Length.Should().Be(2);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_CloseParenExpected);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_SyntaxError);
        }

        [Fact]
        public void TestAttributeWithClassAfterParameter()
        {
            var text = "[a(b class c { }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            file.Errors().Length.Should().Be(2);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_CloseParenExpected);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_SyntaxError);
        }

        [Fact]
        public void TestAttributeWithClassAfterParameterAndComma()
        {
            var text = "[a(b, class c { }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            file.Errors().Length.Should().Be(3);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_InvalidExprTerm);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_CloseParenExpected);
            file.Errors()[2].Code.Should().Be((int)ErrorCode.ERR_SyntaxError);
        }

        [Fact]
        public void TestAttributeWithCommaAfterParameterStart()
        {
            var text = "[a(, class c { }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            file.Errors().Length.Should().Be(4);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_InvalidExprTerm);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_InvalidExprTerm);
            file.Errors()[2].Code.Should().Be((int)ErrorCode.ERR_CloseParenExpected);
            file.Errors()[3].Code.Should().Be((int)ErrorCode.ERR_SyntaxError);
        }

        [Fact]
        public void TestAttributeWithCommasAfterParameterStart()
        {
            var text = "[a(,, class c { }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            file.Errors().Length.Should().Be(5);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_InvalidExprTerm);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_InvalidExprTerm);
            file.Errors()[2].Code.Should().Be((int)ErrorCode.ERR_InvalidExprTerm);
            file.Errors()[3].Code.Should().Be((int)ErrorCode.ERR_CloseParenExpected);
            file.Errors()[4].Code.Should().Be((int)ErrorCode.ERR_SyntaxError);
        }

        [Fact]
        public void TestAttributeWithMissingFirstParameter()
        {
            var text = "[a(, b class c { }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            file.Errors().Length.Should().Be(3);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_InvalidExprTerm);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_CloseParenExpected);
            file.Errors()[2].Code.Should().Be((int)ErrorCode.ERR_SyntaxError);
        }

        [Fact]
        public void TestNamespaceWithGarbage()
        {
            var text = "namespace n { $ }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.NamespaceDeclaration);
            file.Errors().Length.Should().Be(1);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_UnexpectedCharacter);
        }

        [Fact]
        public void TestNamespaceWithUnexpectedKeyword()
        {
            var text = "namespace n { int }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.NamespaceDeclaration);
            file.Errors().Length.Should().Be(1);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_NamespaceUnexpected);
        }

        [Fact]
        public void TestNamespaceWithUnexpectedBracing()
        {
            var text = "namespace n { { }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.NamespaceDeclaration);
            file.Errors().Length.Should().Be(1);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_EOFExpected);
        }

        [Fact]
        public void TestGlobalNamespaceWithUnexpectedBracingAtEnd()
        {
            var text = "namespace n { } }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.NamespaceDeclaration);
            file.Errors().Length.Should().Be(1);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_EOFExpected);
        }

        [Fact]
        public void TestGlobalNamespaceWithUnexpectedBracingAtStart()
        {
            var text = "} namespace n { }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.NamespaceDeclaration);
            file.Errors().Length.Should().Be(1);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_EOFExpected);
        }

        [Fact]
        public void TestGlobalNamespaceWithOpenBraceBeforeNamespace()
        {
            var text = "{ namespace n { }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
        }

        [Fact]
        public void TestPartialNamespace()
        {
            var text = "partial namespace n { }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.NamespaceDeclaration);
            file.Errors().Length.Should().Be(0);
        }

        [Fact]
        public void TestClassAfterStartOfBaseTypeList()
        {
            var text = "class c : class b { }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(2);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            file.Members[1].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            file.Errors().Length.Should().Be(3);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_TypeExpected);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_LbraceExpected);
            file.Errors()[2].Code.Should().Be((int)ErrorCode.ERR_RbraceExpected);
        }

        [Fact]
        public void TestClassAfterBaseType()
        {
            var text = "class c : t class b { }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(2);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            file.Members[1].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            file.Errors().Length.Should().Be(2);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_LbraceExpected);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_RbraceExpected);
        }

        [Fact]
        public void TestClassAfterBaseTypeAndComma()
        {
            var text = "class c : t, class b { }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(2);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            file.Members[1].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            file.Errors().Length.Should().Be(3);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_TypeExpected);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_LbraceExpected);
            file.Errors()[2].Code.Should().Be((int)ErrorCode.ERR_RbraceExpected);
        }

        [Fact]
        public void TestClassAfterBaseTypesWithMissingComma()
        {
            var text = "class c : x y class b { }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(2);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            file.Members[1].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            file.Errors().Length.Should().Be(3);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_SyntaxError);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_LbraceExpected);
            file.Errors()[2].Code.Should().Be((int)ErrorCode.ERR_RbraceExpected);
        }

        [Fact]
        public void TestGarbageAfterStartOfBaseTypeList()
        {
            var text = "class c : $ { }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            file.Errors().Length.Should().Be(2);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_TypeExpected);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_UnexpectedCharacter);
        }

        [Fact]
        public void TestGarbageAfterBaseType()
        {
            var text = "class c : t $ { }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            file.Errors().Length.Should().Be(1);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_UnexpectedCharacter);
        }

        [Fact]
        public void TestGarbageAfterBaseTypeAndComma()
        {
            var text = "class c : t, $ { }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            file.Errors().Length.Should().Be(2);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_TypeExpected);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_UnexpectedCharacter);
        }

        [Fact]
        public void TestGarbageAfterBaseTypesWithMissingComma()
        {
            var text = "class c : x y $ { }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            file.Errors().Length.Should().Be(2);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_SyntaxError);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_UnexpectedCharacter);
        }

        [Fact]
        public void TestConstraintAfterStartOfBaseTypeList()
        {
            var text = "class c<t> : where t : b { }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            file.Errors().Length.Should().Be(1);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_TypeExpected);
        }

        [Fact]
        public void TestConstraintAfterBaseType()
        {
            var text = "class c<t> : x where t : b { }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            file.Errors().Length.Should().Be(0);
        }

        [Fact]
        public void TestConstraintAfterBaseTypeComma()
        {
            var text = "class c<t> : x, where t : b { }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            file.Errors().Length.Should().Be(1);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_TypeExpected);
        }

        [Fact]
        public void TestConstraintAfterBaseTypes()
        {
            var text = "class c<t> : x, y where t : b { }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            file.Errors().Length.Should().Be(0);
        }

        [Fact]
        public void TestConstraintAfterBaseTypesWithMissingComma()
        {
            var text = "class c<t> : x y where t : b { }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            file.Errors().Length.Should().Be(1);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_SyntaxError);
        }

        [Fact]
        public void TestOpenBraceAfterStartOfBaseTypeList()
        {
            var text = "class c<t> : { }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            file.Errors().Length.Should().Be(1);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_TypeExpected);
        }

        [Fact]
        public void TestOpenBraceAfterBaseType()
        {
            var text = "class c<t> : x { }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            file.Errors().Length.Should().Be(0);
        }

        [Fact]
        public void TestOpenBraceAfterBaseTypeComma()
        {
            var text = "class c<t> : x, { }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            file.Errors().Length.Should().Be(1);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_TypeExpected);
        }

        [Fact]
        public void TestOpenBraceAfterBaseTypes()
        {
            var text = "class c<t> : x, y { }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            file.Errors().Length.Should().Be(0);
        }

        [Fact]
        public void TestBaseTypesWithMissingComma()
        {
            var text = "class c<t> : x y { }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            file.Errors().Length.Should().Be(1);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_SyntaxError);
        }

        [Fact]
        public void TestOpenBraceAfterConstraintStart()
        {
            var text = "class c<t> where { }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            file.Errors().Length.Should().Be(3);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_IdentifierExpected);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_SyntaxError);
            file.Errors()[2].Code.Should().Be((int)ErrorCode.ERR_TypeExpected);
        }

        [Fact]
        public void TestOpenBraceAfterConstraintName()
        {
            var text = "class c<t> where t { }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            file.Errors().Length.Should().Be(2);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_SyntaxError);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_TypeExpected);
        }

        [Fact]
        public void TestOpenBraceAfterConstraintNameAndColon()
        {
            var text = "class c<t> where t : { }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            file.Errors().Length.Should().Be(1);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_TypeExpected);
        }

        [Fact]
        public void TestOpenBraceAfterConstraintNameAndTypeAndComma()
        {
            var text = "class c<t> where t : x, { }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            file.Errors().Length.Should().Be(1);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_TypeExpected);
        }

        [Fact]
        public void TestConstraintAfterConstraintStart()
        {
            var text = "class c<t> where where t : a { }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            file.Errors().Length.Should().Be(3);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_IdentifierExpected);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_SyntaxError);
            file.Errors()[2].Code.Should().Be((int)ErrorCode.ERR_TypeExpected);
        }

        [Fact]
        public void TestConstraintAfterConstraintName()
        {
            var text = "class c<t> where t where t : a { }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            file.Errors().Length.Should().Be(2);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_SyntaxError);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_TypeExpected);
        }

        [Fact]
        public void TestConstraintAfterConstraintNameAndColon()
        {
            var text = "class c<t> where t : where t : a { }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            file.Errors().Length.Should().Be(1);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_TypeExpected);
        }

        [Fact]
        public void TestConstraintAfterConstraintNameColonTypeAndComma()
        {
            var text = "class c<t> where t : a, where t : a { }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            file.Errors().Length.Should().Be(1);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_TypeExpected);
        }

        [Fact]
        public void TestGarbageAfterConstraintStart()
        {
            var text = "class c<t> where $ { }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            file.Errors().Length.Should().Be(4);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_IdentifierExpected);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_SyntaxError);
            file.Errors()[2].Code.Should().Be((int)ErrorCode.ERR_TypeExpected);
            file.Errors()[3].Code.Should().Be((int)ErrorCode.ERR_UnexpectedCharacter);
        }

        [Fact]
        public void TestGarbageAfterConstraintName()
        {
            var text = "class c<t> where t $ { }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            file.Errors().Length.Should().Be(3);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_SyntaxError);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_TypeExpected);
            file.Errors()[2].Code.Should().Be((int)ErrorCode.ERR_UnexpectedCharacter);
        }

        [Fact]
        public void TestGarbageAfterConstraintNameAndColon()
        {
            var text = "class c<t> where t : $ { }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            file.Errors().Length.Should().Be(2);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_TypeExpected);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_UnexpectedCharacter);
        }

        [Fact]
        public void TestGarbageAfterConstraintNameColonAndType()
        {
            var text = "class c<t> where t : x $ { }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            file.Errors().Length.Should().Be(1);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_UnexpectedCharacter);
        }

        [Fact]
        public void TestGarbageAfterConstraintNameColonTypeAndComma()
        {
            var text = "class c<t> where t : x, $ { }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            file.Errors().Length.Should().Be(2);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_TypeExpected);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_UnexpectedCharacter);
        }

        [Fact]
        public void TestGarbageAfterGenericClassNameStart()
        {
            var text = "class c<$> { }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            file.Errors().Length.Should().Be(2);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_IdentifierExpected);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_UnexpectedCharacter);
        }

        [Fact]
        public void TestGarbageAfterGenericClassNameType()
        {
            var text = "class c<t $> { }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            file.Errors().Length.Should().Be(1);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_UnexpectedCharacter);
        }

        [Fact]
        public void TestGarbageAfterGenericClassNameTypeAndComma()
        {
            var text = "class c<t, $> { }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            file.Errors().Length.Should().Be(2);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_IdentifierExpected);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_UnexpectedCharacter);
        }

        [Fact]
        public void TestOpenBraceAfterGenericClassNameStart()
        {
            var text = "class c< { }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            file.Errors().Length.Should().Be(2);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_IdentifierExpected);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_SyntaxError);
        }

        [Fact]
        public void TestOpenBraceAfterGenericClassNameAndType()
        {
            var text = "class c<t { }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            file.Errors().Length.Should().Be(1);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_SyntaxError);
        }

        [Fact]
        public void TestClassAfterGenericClassNameStart()
        {
            var text = "class c< class c { }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(2);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            file.Members[1].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            file.Errors().Length.Should().Be(4);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_IdentifierExpected);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_SyntaxError);
            file.Errors()[2].Code.Should().Be((int)ErrorCode.ERR_LbraceExpected);
            file.Errors()[3].Code.Should().Be((int)ErrorCode.ERR_RbraceExpected);
        }

        [Fact]
        public void TestClassAfterGenericClassNameAndType()
        {
            var text = "class c<t class c { }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(2);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            file.Members[1].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            file.Errors().Length.Should().Be(3);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_SyntaxError);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_LbraceExpected);
            file.Errors()[2].Code.Should().Be((int)ErrorCode.ERR_RbraceExpected);
        }

        [Fact]
        public void TestClassAfterGenericClassNameTypeAndComma()
        {
            var text = "class c<t, class c { }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(2);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            file.Members[1].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            file.Errors().Length.Should().Be(4);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_IdentifierExpected);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_SyntaxError);
            file.Errors()[2].Code.Should().Be((int)ErrorCode.ERR_LbraceExpected);
            file.Errors()[3].Code.Should().Be((int)ErrorCode.ERR_RbraceExpected);
        }

        [Fact]
        public void TestBaseTypeAfterGenericClassNameStart()
        {
            var text = "class c< : x { }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            file.Errors().Length.Should().Be(2);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_IdentifierExpected);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_SyntaxError);
        }

        [Fact]
        public void TestBaseTypeAfterGenericClassNameAndType()
        {
            var text = "class c<t : x { }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            file.Errors().Length.Should().Be(1);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_SyntaxError);
        }

        [Fact]
        public void TestBaseTypeAfterGenericClassNameTypeAndComma()
        {
            var text = "class c<t, : x { }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            file.Errors().Length.Should().Be(2);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_IdentifierExpected);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_SyntaxError);
        }

        [Fact]
        public void TestConstraintAfterGenericClassNameStart()
        {
            var text = "class c< where t : x { }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            file.Errors().Length.Should().Be(2);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_IdentifierExpected);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_SyntaxError);
        }

        [Fact]
        public void TestConstraintAfterGenericClassNameAndType()
        {
            var text = "class c<t where t : x { }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            file.Errors().Length.Should().Be(1);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_SyntaxError);
        }

        [Fact]
        public void TestConstraintAfterGenericClassNameTypeAndComma()
        {
            var text = "class c<t, where t : x { }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            file.Errors().Length.Should().Be(2);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_IdentifierExpected);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_SyntaxError);
        }

        [Fact]
        public void TestFieldAfterFieldStart()
        {
            var text = "class c { int int y; }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(2);
            agg.Members[0].Kind().Should().Be(SyntaxKind.IncompleteMember);
            agg.Members[1].Kind().Should().Be(SyntaxKind.FieldDeclaration);
            file.Errors().Length.Should().Be(1);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_InvalidMemberDecl);
        }

        [Fact]
        public void TestFieldAfterFieldTypeAndName()
        {
            var text = "class c { int x int y; }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(2);
            agg.Members[0].Kind().Should().Be(SyntaxKind.FieldDeclaration);
            agg.Members[1].Kind().Should().Be(SyntaxKind.FieldDeclaration);
            file.Errors().Length.Should().Be(1);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_SemicolonExpected);
        }

        [Fact]
        public void TestFieldAfterFieldTypeNameAndComma()
        {
            var text = "class c { int x, int y; }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(2);
            agg.Members[0].Kind().Should().Be(SyntaxKind.FieldDeclaration);
            agg.Members[1].Kind().Should().Be(SyntaxKind.FieldDeclaration);
            file.Errors().Length.Should().Be(2);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_IdentifierExpected);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_SemicolonExpected);
        }

        [Fact]
        public void TestGarbageAfterFieldStart()
        {
            var text = "class c { int $ int y; }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(2);
            agg.Members[0].Kind().Should().Be(SyntaxKind.IncompleteMember);
            agg.Members[1].Kind().Should().Be(SyntaxKind.FieldDeclaration);
            file.Errors().Length.Should().Be(3);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_InvalidMemberDecl);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_UnexpectedCharacter);
            file.Errors()[2].Code.Should().Be((int)ErrorCode.ERR_InvalidMemberDecl);
        }

        [Fact]
        public void TestGarbageAfterFieldTypeAndName()
        {
            var text = "class c { int x $ int y; }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(2);
            agg.Members[0].Kind().Should().Be(SyntaxKind.FieldDeclaration);
            agg.Members[1].Kind().Should().Be(SyntaxKind.FieldDeclaration);
            file.Errors().Length.Should().Be(2);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_UnexpectedCharacter);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_SemicolonExpected);
        }

        [Fact]
        public void TestGarbageAfterFieldTypeNameAndComma()
        {
            var text = "class c { int x, $ int y; }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(2);
            agg.Members[0].Kind().Should().Be(SyntaxKind.FieldDeclaration);
            agg.Members[1].Kind().Should().Be(SyntaxKind.FieldDeclaration);
            file.Errors().Length.Should().Be(3);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_IdentifierExpected);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_UnexpectedCharacter);
            file.Errors()[2].Code.Should().Be((int)ErrorCode.ERR_SemicolonExpected);
        }

        [Fact]
        public void TestEndBraceAfterFieldStart()
        {
            var text = "class c { int }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(1);
            agg.Members[0].Kind().Should().Be(SyntaxKind.IncompleteMember);
            file.Errors().Length.Should().Be(1);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_InvalidMemberDecl);
        }

        [Fact]
        public void TestEndBraceAfterFieldName()
        {
            var text = "class c { int x }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(1);
            agg.Members[0].Kind().Should().Be(SyntaxKind.FieldDeclaration);
            file.Errors().Length.Should().Be(1);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_SemicolonExpected);
        }

        [Fact]
        public void TestEndBraceAfterFieldNameAndComma()
        {
            var text = "class c { int x, }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(1);
            agg.Members[0].Kind().Should().Be(SyntaxKind.FieldDeclaration);
            file.Errors().Length.Should().Be(2);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_IdentifierExpected);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_SemicolonExpected);
        }

        [Fact]
        public void TestEndBraceAfterMethodParameterStart()
        {
            var text = "class c { int m( }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(1);
            agg.Members[0].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            file.Errors().Length.Should().Be(2);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_CloseParenExpected);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_SemicolonExpected);
        }

        [Fact]
        public void TestEndBraceAfterMethodParameterType()
        {
            var text = "class c { int m(x }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(1);
            agg.Members[0].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            file.Errors().Length.Should().Be(3);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_IdentifierExpected);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_CloseParenExpected);
            file.Errors()[2].Code.Should().Be((int)ErrorCode.ERR_SemicolonExpected);
        }

        [Fact]
        public void TestEndBraceAfterMethodParameterName()
        {
            var text = "class c { int m(x y}";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(1);
            agg.Members[0].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            file.Errors().Length.Should().Be(2);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_CloseParenExpected);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_SemicolonExpected);
        }

        [Fact]
        public void TestEndBraceAfterMethodParameterTypeNameAndComma()
        {
            var text = "class c { int m(x y, }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(1);
            agg.Members[0].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            file.Errors().Length.Should().Be(4);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_TypeExpected);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_IdentifierExpected);
            file.Errors()[2].Code.Should().Be((int)ErrorCode.ERR_CloseParenExpected);
            file.Errors()[3].Code.Should().Be((int)ErrorCode.ERR_SemicolonExpected);
        }

        [Fact]
        public void TestEndBraceAfterMethodParameters()
        {
            var text = "class c { int m() }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(1);
            agg.Members[0].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            file.Errors().Length.Should().Be(1);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_SemicolonExpected);
        }

        [Fact]
        public void TestGarbageAfterMethodParameterStart()
        {
            var text = "class c { int m( $ ); }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(1);
            agg.Members[0].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            file.Errors().Length.Should().Be(1);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_UnexpectedCharacter);
        }

        [Fact]
        public void TestGarbageAfterMethodParameterType()
        {
            var text = "class c { int m( x $ ); }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(1);
            agg.Members[0].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            file.Errors().Length.Should().Be(2);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_IdentifierExpected);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_UnexpectedCharacter);
        }

        [Fact]
        public void TestGarbageAfterMethodParameterTypeAndName()
        {
            var text = "class c { int m( x y $ ); }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(1);
            agg.Members[0].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            file.Errors().Length.Should().Be(1);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_UnexpectedCharacter);
        }

        [Fact]
        public void TestGarbageAfterMethodParameterTypeNameAndComma()
        {
            var text = "class c { int m( x y, $ ); }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(1);
            agg.Members[0].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            file.Errors().Length.Should().Be(3);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_TypeExpected);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_IdentifierExpected);
            file.Errors()[2].Code.Should().Be((int)ErrorCode.ERR_UnexpectedCharacter);
        }

        [Fact]
        public void TestMethodAfterMethodParameterStart()
        {
            var text = "class c { int m( public void m() { } }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(2);
            agg.Members[0].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            agg.Members[1].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            file.Errors().Length.Should().Be(2);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_CloseParenExpected);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_SemicolonExpected);
        }

        [Fact]
        public void TestMethodAfterMethodParameterType()
        {
            var text = "class c { int m(x public void m() { } }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(2);
            agg.Members[0].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            agg.Members[1].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            file.Errors().Length.Should().Be(3);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_IdentifierExpected);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_CloseParenExpected);
            file.Errors()[2].Code.Should().Be((int)ErrorCode.ERR_SemicolonExpected);
        }

        [Fact]
        public void TestMethodAfterMethodParameterTypeAndName()
        {
            var text = "class c { int m(x y public void m() { } }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(2);
            agg.Members[0].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            agg.Members[1].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            file.Errors().Length.Should().Be(2);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_CloseParenExpected);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_SemicolonExpected);
        }

        [Fact]
        public void TestMethodAfterMethodParameterTypeNameAndComma()
        {
            var text = "class c { int m(x y, public void m() { } }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(2);
            agg.Members[0].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            agg.Members[1].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            file.Errors().Length.Should().Be(4);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_TypeExpected);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_IdentifierExpected);
            file.Errors()[2].Code.Should().Be((int)ErrorCode.ERR_CloseParenExpected);
            file.Errors()[3].Code.Should().Be((int)ErrorCode.ERR_SemicolonExpected);
        }

        [Fact]
        public void TestMethodAfterMethodParameterList()
        {
            var text = "class c { int m(x y) public void m() { } }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(2);
            agg.Members[0].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            agg.Members[1].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            file.Errors().Length.Should().Be(1);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_SemicolonExpected);
        }

        [Fact]
        public void TestMethodBodyAfterMethodParameterListStart()
        {
            var text = "class c { int m( { } }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(1);
            agg.Members[0].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            file.Errors().Length.Should().Be(1);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_CloseParenExpected);
        }

        [Fact]
        public void TestSemicolonAfterMethodParameterListStart()
        {
            var text = "class c { int m( ; }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(1);
            agg.Members[0].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            file.Errors().Length.Should().Be(1);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_CloseParenExpected);
        }

        [Fact]
        public void TestConstructorBodyAfterConstructorParameterListStart()
        {
            var text = "class c { c( { } }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(1);
            agg.Members[0].Kind().Should().Be(SyntaxKind.ConstructorDeclaration);
            file.Errors().Length.Should().Be(1);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_CloseParenExpected);
        }

        [Fact]
        public void TestSemicolonAfterDelegateParameterListStart()
        {
            var text = "delegate void d( ;";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.DelegateDeclaration);
            var agg = (DelegateDeclarationSyntax)file.Members[0];
            file.Errors().Length.Should().Be(1);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_CloseParenExpected);
        }

        [Fact]
        public void TestEndBraceAfterIndexerParameterStart()
        {
            var text = "class c { int this[ }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(1);
            agg.Members[0].Kind().Should().Be(SyntaxKind.IndexerDeclaration);
            file.Errors().Length.Should().Be(3);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_SyntaxError);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_LbraceExpected);
            file.Errors()[2].Code.Should().Be((int)ErrorCode.ERR_RbraceExpected);

            CreateCompilation(text).VerifyDiagnostics(
                // (1,21): error CS1003: Syntax error, ']' expected
                // class c { int this[ }
                Diagnostic(ErrorCode.ERR_SyntaxError, "}").WithArguments("]").WithLocation(1, 21),
                // (1,21): error CS1514: { expected
                // class c { int this[ }
                Diagnostic(ErrorCode.ERR_LbraceExpected, "}").WithLocation(1, 21),
                // (1,22): error CS1513: } expected
                // class c { int this[ }
                Diagnostic(ErrorCode.ERR_RbraceExpected, "").WithLocation(1, 22),
                // (1,7): warning CS8981: The type name 'c' only contains lower-cased ascii characters. Such names may become reserved for the language.
                // class c { int this[ }
                Diagnostic(ErrorCode.WRN_LowerCaseTypeName, "c").WithArguments("c").WithLocation(1, 7),
                // (1,19): error CS1551: Indexers must have at least one parameter
                // class c { int this[ }
                Diagnostic(ErrorCode.ERR_IndexerNeedsParam, "[").WithLocation(1, 19),
                // (1,15): error CS0548: 'c.this': property or indexer must have at least one accessor
                // class c { int this[ }
                Diagnostic(ErrorCode.ERR_PropertyWithNoAccessors, "this").WithArguments("c.this").WithLocation(1, 15));
        }

        [Fact]
        public void TestEndBraceAfterIndexerParameterType()
        {
            var text = "class c { int this[x }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(1);
            agg.Members[0].Kind().Should().Be(SyntaxKind.IndexerDeclaration);
            file.Errors().Length.Should().Be(4);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_IdentifierExpected);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_SyntaxError);
            file.Errors()[2].Code.Should().Be((int)ErrorCode.ERR_LbraceExpected);
            file.Errors()[3].Code.Should().Be((int)ErrorCode.ERR_RbraceExpected);
        }

        [Fact]
        public void TestEndBraceAfterIndexerParameterName()
        {
            var text = "class c { int this[x y }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(1);
            agg.Members[0].Kind().Should().Be(SyntaxKind.IndexerDeclaration);
            file.Errors().Length.Should().Be(3);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_SyntaxError);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_LbraceExpected);
            file.Errors()[2].Code.Should().Be((int)ErrorCode.ERR_RbraceExpected);
        }

        [Fact]
        public void TestEndBraceAfterIndexerParameterTypeNameAndComma()
        {
            var text = "class c { int this[x y, }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(1);
            agg.Members[0].Kind().Should().Be(SyntaxKind.IndexerDeclaration);
            file.Errors().Length.Should().Be(5);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_TypeExpected);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_IdentifierExpected);
            file.Errors()[2].Code.Should().Be((int)ErrorCode.ERR_SyntaxError);
            file.Errors()[3].Code.Should().Be((int)ErrorCode.ERR_LbraceExpected);
            file.Errors()[4].Code.Should().Be((int)ErrorCode.ERR_RbraceExpected);
        }

        [Fact]
        public void TestEndBraceAfterIndexerParameters()
        {
            var text = "class c { int this[x y] }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(1);
            agg.Members[0].Kind().Should().Be(SyntaxKind.IndexerDeclaration);
            file.Errors().Length.Should().Be(2);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_LbraceExpected);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_RbraceExpected);
        }

        [Fact]
        public void TestGarbageAfterIndexerParameterStart()
        {
            var text = "class c { int this[ $ ] { } }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(1);
            agg.Members[0].Kind().Should().Be(SyntaxKind.IndexerDeclaration);
            file.Errors().Length.Should().Be(1);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_UnexpectedCharacter);

            CreateCompilation(text).VerifyDiagnostics(
                // (1,21): error CS1056: Unexpected character '$'
                // class c { int this[ $ ] { } }
                Diagnostic(ErrorCode.ERR_UnexpectedCharacter, "").WithArguments("$").WithLocation(1, 21),
                // (1,7): warning CS8981: The type name 'c' only contains lower-cased ascii characters. Such names may become reserved for the language.
                // class c { int this[ $ ] { } }
                Diagnostic(ErrorCode.WRN_LowerCaseTypeName, "c").WithArguments("c").WithLocation(1, 7),
                // (1,23): error CS1551: Indexers must have at least one parameter
                // class c { int this[ $ ] { } }
                Diagnostic(ErrorCode.ERR_IndexerNeedsParam, "]").WithLocation(1, 23),
                // (1,15): error CS0548: 'c.this': property or indexer must have at least one accessor
                // class c { int this[ $ ] { } }
                Diagnostic(ErrorCode.ERR_PropertyWithNoAccessors, "this").WithArguments("c.this").WithLocation(1, 15));
        }

        [Fact]
        public void TestGarbageAfterIndexerParameterType()
        {
            var text = "class c { int this[ x $ ] { } }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(1);
            agg.Members[0].Kind().Should().Be(SyntaxKind.IndexerDeclaration);
            file.Errors().Length.Should().Be(2);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_IdentifierExpected);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_UnexpectedCharacter);
        }

        [Fact]
        public void TestGarbageAfterIndexerParameterTypeAndName()
        {
            var text = "class c { int this[ x y $ ] { } }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(1);
            agg.Members[0].Kind().Should().Be(SyntaxKind.IndexerDeclaration);
            file.Errors().Length.Should().Be(1);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_UnexpectedCharacter);
        }

        [Fact]
        public void TestGarbageAfterIndexerParameterTypeNameAndComma()
        {
            var text = "class c { int this[ x y, $ ] { } }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(1);
            agg.Members[0].Kind().Should().Be(SyntaxKind.IndexerDeclaration);
            file.Errors().Length.Should().Be(3);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_TypeExpected);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_IdentifierExpected);
            file.Errors()[2].Code.Should().Be((int)ErrorCode.ERR_UnexpectedCharacter);
        }

        [Fact]
        public void TestMethodAfterIndexerParameterStart()
        {
            var text = "class c { int this[ public void m() { } }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(2);
            agg.Members[0].Kind().Should().Be(SyntaxKind.IndexerDeclaration);
            agg.Members[1].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            file.Errors().Length.Should().Be(3);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_SyntaxError);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_LbraceExpected);
            file.Errors()[2].Code.Should().Be((int)ErrorCode.ERR_RbraceExpected);

            CreateCompilation(text).VerifyDiagnostics(
                // (1,21): error CS1003: Syntax error, ']' expected
                // class c { int this[ public void m() { } }
                Diagnostic(ErrorCode.ERR_SyntaxError, "public").WithArguments("]").WithLocation(1, 21),
                // (1,21): error CS1514: { expected
                // class c { int this[ public void m() { } }
                Diagnostic(ErrorCode.ERR_LbraceExpected, "public").WithLocation(1, 21),
                // (1,21): error CS1513: } expected
                // class c { int this[ public void m() { } }
                Diagnostic(ErrorCode.ERR_RbraceExpected, "public").WithLocation(1, 21),
                // (1,7): warning CS8981: The type name 'c' only contains lower-cased ascii characters. Such names may become reserved for the language.
                // class c { int this[ public void m() { } }
                Diagnostic(ErrorCode.WRN_LowerCaseTypeName, "c").WithArguments("c").WithLocation(1, 7),
                // (1,19): error CS1551: Indexers must have at least one parameter
                // class c { int this[ public void m() { } }
                Diagnostic(ErrorCode.ERR_IndexerNeedsParam, "[").WithLocation(1, 19),
                // (1,15): error CS0548: 'c.this': property or indexer must have at least one accessor
                // class c { int this[ public void m() { } }
                Diagnostic(ErrorCode.ERR_PropertyWithNoAccessors, "this").WithArguments("c.this").WithLocation(1, 15));
        }

        [Fact]
        public void TestMethodAfterIndexerParameterType()
        {
            var text = "class c { int this[x public void m() { } }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(2);
            agg.Members[0].Kind().Should().Be(SyntaxKind.IndexerDeclaration);
            agg.Members[1].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            file.Errors().Length.Should().Be(4);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_IdentifierExpected);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_SyntaxError);
            file.Errors()[2].Code.Should().Be((int)ErrorCode.ERR_LbraceExpected);
            file.Errors()[3].Code.Should().Be((int)ErrorCode.ERR_RbraceExpected);
        }

        [Fact]
        public void TestMethodAfterIndexerParameterTypeAndName()
        {
            var text = "class c { int this[x y public void m() { } }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(2);
            agg.Members[0].Kind().Should().Be(SyntaxKind.IndexerDeclaration);
            agg.Members[1].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            file.Errors().Length.Should().Be(3);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_SyntaxError);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_LbraceExpected);
            file.Errors()[2].Code.Should().Be((int)ErrorCode.ERR_RbraceExpected);
        }

        [Fact]
        public void TestMethodAfterIndexerParameterTypeNameAndComma()
        {
            var text = "class c { int this[x y, public void m() { } }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(2);
            agg.Members[0].Kind().Should().Be(SyntaxKind.IndexerDeclaration);
            agg.Members[1].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            file.Errors().Length.Should().Be(5);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_TypeExpected);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_IdentifierExpected);
            file.Errors()[2].Code.Should().Be((int)ErrorCode.ERR_SyntaxError);
            file.Errors()[3].Code.Should().Be((int)ErrorCode.ERR_LbraceExpected);
            file.Errors()[4].Code.Should().Be((int)ErrorCode.ERR_RbraceExpected);
        }

        [Fact]
        public void TestMethodAfterIndexerParameterList()
        {
            var text = "class c { int this[x y] public void m() { } }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(2);
            agg.Members[0].Kind().Should().Be(SyntaxKind.IndexerDeclaration);
            agg.Members[1].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            file.Errors().Length.Should().Be(2);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_LbraceExpected);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_RbraceExpected);
        }

        [Fact]
        public void TestEOFAfterDelegateStart()
        {
            var text = "delegate";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.DelegateDeclaration);
            file.Errors().Length.Should().Be(5);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_TypeExpected);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_IdentifierExpected);
            file.Errors()[2].Code.Should().Be((int)ErrorCode.ERR_SyntaxError);
            file.Errors()[3].Code.Should().Be((int)ErrorCode.ERR_CloseParenExpected);
            file.Errors()[4].Code.Should().Be((int)ErrorCode.ERR_SemicolonExpected);
        }

        [Fact]
        public void TestEOFAfterDelegateType()
        {
            var text = "delegate d";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.DelegateDeclaration);
            file.Errors().Length.Should().Be(4);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_IdentifierExpected);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_SyntaxError);
            file.Errors()[2].Code.Should().Be((int)ErrorCode.ERR_CloseParenExpected);
            file.Errors()[3].Code.Should().Be((int)ErrorCode.ERR_SemicolonExpected);
        }

        [Fact]
        public void TestEOFAfterDelegateName()
        {
            var text = "delegate void d";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.DelegateDeclaration);
            file.Errors().Length.Should().Be(3);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_SyntaxError);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_CloseParenExpected);
            file.Errors()[2].Code.Should().Be((int)ErrorCode.ERR_SemicolonExpected);
        }

        [Fact]
        public void TestEOFAfterDelegateParameterStart()
        {
            var text = "delegate void d(";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.DelegateDeclaration);
            file.Errors().Length.Should().Be(2);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_CloseParenExpected);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_SemicolonExpected);
        }

        [Fact]
        public void TestEOFAfterDelegateParameterType()
        {
            var text = "delegate void d(t";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.DelegateDeclaration);
            file.Errors().Length.Should().Be(3);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_IdentifierExpected);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_CloseParenExpected);
            file.Errors()[2].Code.Should().Be((int)ErrorCode.ERR_SemicolonExpected);
        }

        [Fact]
        public void TestEOFAfterDelegateParameterTypeName()
        {
            var text = "delegate void d(t n";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.DelegateDeclaration);
            file.Errors().Length.Should().Be(2);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_CloseParenExpected);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_SemicolonExpected);
        }

        [Fact]
        public void TestEOFAfterDelegateParameterList()
        {
            var text = "delegate void d(t n)";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.DelegateDeclaration);
            file.Errors().Length.Should().Be(1);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_SemicolonExpected);
        }

        [Fact]
        public void TestEOFAfterDelegateParameterTypeNameAndComma()
        {
            var text = "delegate void d(t n, ";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.DelegateDeclaration);
            file.Errors().Length.Should().Be(4);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_TypeExpected);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_IdentifierExpected);
            file.Errors()[2].Code.Should().Be((int)ErrorCode.ERR_CloseParenExpected);
            file.Errors()[3].Code.Should().Be((int)ErrorCode.ERR_SemicolonExpected);
        }

        [Fact]
        public void TestClassAfterDelegateStart()
        {
            var text = "delegate class c { }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(2);
            file.Members[0].Kind().Should().Be(SyntaxKind.DelegateDeclaration);
            file.Members[1].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            file.Errors().Length.Should().Be(5);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_TypeExpected);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_IdentifierExpected);
            file.Errors()[2].Code.Should().Be((int)ErrorCode.ERR_SyntaxError);
            file.Errors()[3].Code.Should().Be((int)ErrorCode.ERR_CloseParenExpected);
            file.Errors()[4].Code.Should().Be((int)ErrorCode.ERR_SemicolonExpected);
        }

        [Fact]
        public void TestClassAfterDelegateType()
        {
            var text = "delegate d class c { }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(2);
            file.Members[0].Kind().Should().Be(SyntaxKind.DelegateDeclaration);
            file.Members[1].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            file.Errors().Length.Should().Be(4);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_IdentifierExpected);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_SyntaxError);
            file.Errors()[2].Code.Should().Be((int)ErrorCode.ERR_CloseParenExpected);
            file.Errors()[3].Code.Should().Be((int)ErrorCode.ERR_SemicolonExpected);
        }

        [Fact]
        public void TestClassAfterDelegateName()
        {
            var text = "delegate void d class c { }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(2);
            file.Members[0].Kind().Should().Be(SyntaxKind.DelegateDeclaration);
            file.Members[1].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            file.Errors().Length.Should().Be(3);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_SyntaxError);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_CloseParenExpected);
            file.Errors()[2].Code.Should().Be((int)ErrorCode.ERR_SemicolonExpected);
        }

        [Fact]
        public void TestClassAfterDelegateParameterStart()
        {
            var text = "delegate void d( class c { }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(2);
            file.Members[0].Kind().Should().Be(SyntaxKind.DelegateDeclaration);
            file.Members[1].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            file.Errors().Length.Should().Be(2);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_CloseParenExpected);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_SemicolonExpected);
        }

        [Fact]
        public void TestClassAfterDelegateParameterType()
        {
            var text = "delegate void d(t class c { }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(2);
            file.Members[0].Kind().Should().Be(SyntaxKind.DelegateDeclaration);
            file.Members[1].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            file.Errors().Length.Should().Be(3);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_IdentifierExpected);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_CloseParenExpected);
            file.Errors()[2].Code.Should().Be((int)ErrorCode.ERR_SemicolonExpected);
        }

        [Fact]
        public void TestClassAfterDelegateParameterTypeName()
        {
            var text = "delegate void d(t n class c { }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(2);
            file.Members[0].Kind().Should().Be(SyntaxKind.DelegateDeclaration);
            file.Members[1].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            file.Errors().Length.Should().Be(2);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_CloseParenExpected);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_SemicolonExpected);
        }

        [Fact]
        public void TestClassAfterDelegateParameterList()
        {
            var text = "delegate void d(t n) class c { }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(2);
            file.Members[0].Kind().Should().Be(SyntaxKind.DelegateDeclaration);
            file.Members[1].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            file.Errors().Length.Should().Be(1);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_SemicolonExpected);
        }

        [Fact]
        public void TestClassAfterDelegateParameterTypeNameAndComma()
        {
            var text = "delegate void d(t n, class c { }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(2);
            file.Members[0].Kind().Should().Be(SyntaxKind.DelegateDeclaration);
            file.Members[1].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            file.Errors().Length.Should().Be(4);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_TypeExpected);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_IdentifierExpected);
            file.Errors()[2].Code.Should().Be((int)ErrorCode.ERR_CloseParenExpected);
            file.Errors()[3].Code.Should().Be((int)ErrorCode.ERR_SemicolonExpected);
        }

        [Fact]
        public void TestGarbageAfterDelegateParameterStart()
        {
            var text = "delegate void d($);";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.DelegateDeclaration);
            file.Errors().Length.Should().Be(1);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_UnexpectedCharacter);
        }

        [Fact]
        public void TestGarbageAfterDelegateParameterType()
        {
            var text = "delegate void d(t $);";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.DelegateDeclaration);
            file.Errors().Length.Should().Be(2);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_IdentifierExpected);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_UnexpectedCharacter);
        }

        [Fact]
        public void TestGarbageAfterDelegateParameterTypeAndName()
        {
            var text = "delegate void d(t n $);";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.DelegateDeclaration);
            file.Errors().Length.Should().Be(1);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_UnexpectedCharacter);
        }

        [Fact]
        public void TestGarbageAfterDelegateParameterTypeNameAndComma()
        {
            var text = "delegate void d(t n, $);";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.DelegateDeclaration);
            file.Errors().Length.Should().Be(3);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_TypeExpected);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_IdentifierExpected);
            file.Errors()[2].Code.Should().Be((int)ErrorCode.ERR_UnexpectedCharacter);
        }

        [Fact]
        public void TestGarbageAfterEnumStart()
        {
            var text = "enum e { $ }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.EnumDeclaration);
            file.Errors().Length.Should().Be(1);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_UnexpectedCharacter);
        }

        [Fact]
        public void TestGarbageAfterEnumName()
        {
            var text = "enum e { n $ }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.EnumDeclaration);
            file.Errors().Length.Should().Be(1);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_UnexpectedCharacter);
        }

        [Fact]
        public void TestGarbageBeforeEnumName()
        {
            var text = "enum e { $ n }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.EnumDeclaration);
            file.Errors().Length.Should().Be(1);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_UnexpectedCharacter);
        }

        [Fact]
        public void TestGarbageAferEnumNameAndComma()
        {
            var text = "enum e { n, $ }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.EnumDeclaration);
            file.Errors().Length.Should().Be(1);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_UnexpectedCharacter);
        }

        [Fact]
        public void TestGarbageAferEnumNameCommaAndName()
        {
            var text = "enum e { n, n $ }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.EnumDeclaration);
            file.Errors().Length.Should().Be(1);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_UnexpectedCharacter);
        }

        [Fact]
        public void TestGarbageBetweenEnumNames()
        {
            var text = "enum e { n, $ n }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.EnumDeclaration);
            file.Errors().Length.Should().Be(1);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_UnexpectedCharacter);
        }

        [Fact]
        public void TestGarbageBetweenEnumNamesWithMissingComma()
        {
            var text = "enum e { n $ n }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.EnumDeclaration);
            file.Errors().Length.Should().Be(2);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_UnexpectedCharacter);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_SyntaxError);
        }

        [Fact]
        public void TestGarbageAferEnumNameAndEquals()
        {
            var text = "enum e { n = $ }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.EnumDeclaration);
            file.Errors().Length.Should().Be(2);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_InvalidExprTerm);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_UnexpectedCharacter);
        }

        [Fact]
        public void TestEOFAfterEnumStart()
        {
            var text = "enum e { ";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.EnumDeclaration);
            file.Errors().Length.Should().Be(1);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_RbraceExpected);
        }

        [Fact]
        public void TestEOFAfterEnumName()
        {
            var text = "enum e { n ";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.EnumDeclaration);
            file.Errors().Length.Should().Be(1);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_RbraceExpected);
        }

        [Fact]
        public void TestEOFAfterEnumNameAndComma()
        {
            var text = "enum e { n, ";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.EnumDeclaration);
            file.Errors().Length.Should().Be(1);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_RbraceExpected);
        }

        [Fact]
        public void TestClassAfterEnumStart()
        {
            var text = "enum e { class c { }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(2);
            file.Members[0].Kind().Should().Be(SyntaxKind.EnumDeclaration);
            file.Members[1].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            file.Errors().Length.Should().Be(1);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_RbraceExpected);
        }

        [Fact]
        public void TestClassAfterEnumName()
        {
            var text = "enum e { n class c { }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(2);
            file.Members[0].Kind().Should().Be(SyntaxKind.EnumDeclaration);
            file.Members[1].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            file.Errors().Length.Should().Be(1);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_RbraceExpected);
        }

        [Fact]
        public void TestClassAfterEnumNameAndComma()
        {
            var text = "enum e { n, class c { }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(2);
            file.Members[0].Kind().Should().Be(SyntaxKind.EnumDeclaration);
            file.Members[1].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            file.Errors().Length.Should().Be(1);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_RbraceExpected);
        }

        [Fact]
        public void TestGarbageAfterFixedFieldRankStart()
        {
            var text = "class c { fixed int x[$]; }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members[0].Kind().Should().Be(SyntaxKind.FieldDeclaration);
            file.Errors().Length.Should().Be(2);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_UnexpectedCharacter);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_ValueExpected);
        }

        [Fact]
        public void TestGarbageBeforeFixedFieldRankSize()
        {
            var text = "class c { fixed int x[$ 10]; }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members[0].Kind().Should().Be(SyntaxKind.FieldDeclaration);
            file.Errors().Length.Should().Be(1);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_UnexpectedCharacter);
        }

        [Fact]
        public void TestGarbageAfterFixedFieldRankSize()
        {
            var text = "class c { fixed int x[10 $]; }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members[0].Kind().Should().Be(SyntaxKind.FieldDeclaration);
            file.Errors().Length.Should().Be(3);
            (ErrorCode)file.Errors()[0].Code.Should().Be(ErrorCode.ERR_SyntaxError); //expected comma
            (ErrorCode)file.Errors()[1].Code.Should().Be(ErrorCode.ERR_UnexpectedCharacter); //didn't expect '$'
            (ErrorCode)file.Errors()[2].Code.Should().Be(ErrorCode.ERR_ValueExpected); //expected value after (missing) comma
        }

        [Fact]
        public void TestGarbageAfterFieldTypeRankStart()
        {
            var text = "class c { int[$] x; }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members[0].Kind().Should().Be(SyntaxKind.FieldDeclaration);
            file.Errors().Length.Should().Be(1);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_UnexpectedCharacter);
        }

        [Fact]
        public void TestGarbageAfterFieldTypeRankComma()
        {
            var text = "class c { int[,$] x; }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members[0].Kind().Should().Be(SyntaxKind.FieldDeclaration);
            file.Errors().Length.Should().Be(1);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_UnexpectedCharacter);
        }

        [Fact]
        public void TestGarbageBeforeFieldTypeRankComma()
        {
            var text = "class c { int[$,] x; }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members[0].Kind().Should().Be(SyntaxKind.FieldDeclaration);
            file.Errors().Length.Should().Be(1);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_UnexpectedCharacter);
        }

        [Fact]
        public void TestEndBraceAfterFieldRankStart()
        {
            var text = "class c { int[ }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members[0].Kind().Should().Be(SyntaxKind.IncompleteMember);
            file.Errors().Length.Should().Be(1);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_SyntaxError);
        }

        [Fact]
        public void TestEndBraceAfterFieldRankComma()
        {
            var text = "class c { int[, }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members[0].Kind().Should().Be(SyntaxKind.IncompleteMember);
            file.Errors().Length.Should().Be(1);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_SyntaxError);
        }

        [Fact]
        public void TestMethodAfterFieldRankStart()
        {
            var text = "class c { int[ public void m() { } }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(2);
            agg.Members[0].Kind().Should().Be(SyntaxKind.IncompleteMember);
            agg.Members[1].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            file.Errors().Length.Should().Be(1);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_SyntaxError);
        }

        [Fact]
        public void TestMethodAfterFieldRankComma()
        {
            var text = "class c { int[, public void m() { } }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(2);
            agg.Members[0].Kind().Should().Be(SyntaxKind.IncompleteMember);
            agg.Members[1].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            file.Errors().Length.Should().Be(1);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_SyntaxError);
        }

        [Fact]
        public void TestStatementAfterLocalDeclarationStart()
        {
            var text = "class c { void m() { int if (x) y(); } }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(1);
            agg.Members[0].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            var ms = (MethodDeclarationSyntax)agg.Members[0];
            ms.Body.Should().NotBeNull();
            ms.Body.Statements.Count.Should().Be(2);
            ms.Body.Statements[0].Kind().Should().Be(SyntaxKind.LocalDeclarationStatement);
            ms.Body.Statements[1].Kind().Should().Be(SyntaxKind.IfStatement);
            file.Errors().Length.Should().Be(2);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_IdentifierExpected);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_SemicolonExpected);
        }

        [Fact]
        public void TestStatementAfterLocalRankStart()
        {
            var text = "class c { void m() { int [ if (x) y(); } }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(1);
            agg.Members[0].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            var ms = (MethodDeclarationSyntax)agg.Members[0];
            ms.Body.Should().NotBeNull();
            ms.Body.Statements.Count.Should().Be(2);
            ms.Body.Statements[0].Kind().Should().Be(SyntaxKind.LocalDeclarationStatement);
            ms.Body.Statements[1].Kind().Should().Be(SyntaxKind.IfStatement);
            file.Errors().Length.Should().Be(3);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_SyntaxError);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_IdentifierExpected);
            file.Errors()[2].Code.Should().Be((int)ErrorCode.ERR_SemicolonExpected);
        }

        [Fact]
        public void TestStatementAfterLocalRankComma()
        {
            var text = "class c { void m() { int [, if (x) y(); } }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(1);
            agg.Members[0].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            var ms = (MethodDeclarationSyntax)agg.Members[0];
            ms.Body.Should().NotBeNull();
            ms.Body.Statements.Count.Should().Be(2);
            ms.Body.Statements[0].Kind().Should().Be(SyntaxKind.LocalDeclarationStatement);
            ms.Body.Statements[1].Kind().Should().Be(SyntaxKind.IfStatement);
            file.Errors().Length.Should().Be(3);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_SyntaxError);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_IdentifierExpected);
            file.Errors()[2].Code.Should().Be((int)ErrorCode.ERR_SemicolonExpected);
        }

        [Fact]
        public void TestStatementAfterLocalDeclarationWithMissingSemicolon()
        {
            var text = "class c { void m() { int a if (x) y(); } }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(1);
            agg.Members[0].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            var ms = (MethodDeclarationSyntax)agg.Members[0];
            ms.Body.Should().NotBeNull();
            ms.Body.Statements.Count.Should().Be(2);
            ms.Body.Statements[0].Kind().Should().Be(SyntaxKind.LocalDeclarationStatement);
            ms.Body.Statements[1].Kind().Should().Be(SyntaxKind.IfStatement);
            file.Errors().Length.Should().Be(1);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_SemicolonExpected);
        }

        [Fact]
        public void TestStatementAfterLocalDeclarationWithCommaAndMissingSemicolon()
        {
            var text = "class c { void m() { int a, if (x) y(); } }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(1);
            agg.Members[0].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            var ms = (MethodDeclarationSyntax)agg.Members[0];
            ms.Body.Should().NotBeNull();
            ms.Body.Statements.Count.Should().Be(2);
            ms.Body.Statements[0].Kind().Should().Be(SyntaxKind.LocalDeclarationStatement);
            ms.Body.Statements[1].Kind().Should().Be(SyntaxKind.IfStatement);
            file.Errors().Length.Should().Be(2);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_IdentifierExpected);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_SemicolonExpected);
        }

        [Fact]
        public void TestStatementAfterLocalDeclarationEquals()
        {
            var text = "class c { void m() { int a = if (x) y(); } }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(1);
            agg.Members[0].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            var ms = (MethodDeclarationSyntax)agg.Members[0];
            ms.Body.Should().NotBeNull();
            ms.Body.Statements.Count.Should().Be(2);
            ms.Body.Statements[0].Kind().Should().Be(SyntaxKind.LocalDeclarationStatement);
            ms.Body.Statements[1].Kind().Should().Be(SyntaxKind.IfStatement);
            file.Errors().Length.Should().Be(2);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_InvalidExprTerm);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_SemicolonExpected);
        }

        [Fact]
        public void TestStatementAfterLocalDeclarationArrayInitializerStart()
        {
            var text = "class c { void m() { int a = { if (x) y(); } }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(1);
            agg.Members[0].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            var ms = (MethodDeclarationSyntax)agg.Members[0];
            ms.Body.Should().NotBeNull();
            ms.Body.Statements.Count.Should().Be(2);
            ms.Body.Statements[0].Kind().Should().Be(SyntaxKind.LocalDeclarationStatement);
            ms.Body.Statements[1].Kind().Should().Be(SyntaxKind.IfStatement);
            file.Errors().Length.Should().Be(2);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_RbraceExpected);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_SemicolonExpected);
        }

        [Fact]
        public void TestStatementAfterLocalDeclarationArrayInitializerExpression()
        {
            var text = "class c { void m() { int a = { e if (x) y(); } }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(1);
            agg.Members[0].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            var ms = (MethodDeclarationSyntax)agg.Members[0];
            ms.Body.Should().NotBeNull();
            ms.Body.Statements.Count.Should().Be(2);
            ms.Body.Statements[0].Kind().Should().Be(SyntaxKind.LocalDeclarationStatement);
            ms.Body.Statements[1].Kind().Should().Be(SyntaxKind.IfStatement);
            file.Errors().Length.Should().Be(2);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_RbraceExpected);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_SemicolonExpected);
        }

        [Fact]
        public void TestStatementAfterLocalDeclarationArrayInitializerExpressionAndComma()
        {
            var text = "class c { void m() { int a = { e, if (x) y(); } }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(1);
            agg.Members[0].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            var ms = (MethodDeclarationSyntax)agg.Members[0];
            ms.Body.Should().NotBeNull();
            ms.Body.Statements.Count.Should().Be(2);
            ms.Body.Statements[0].Kind().Should().Be(SyntaxKind.LocalDeclarationStatement);
            ms.Body.Statements[1].Kind().Should().Be(SyntaxKind.IfStatement);
            file.Errors().Length.Should().Be(2);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_RbraceExpected);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_SemicolonExpected);
        }

        [Fact]
        public void TestGarbageAfterLocalDeclarationArrayInitializerStart()
        {
            var text = "class c { void m() { int a = { $ }; } }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(1);
            agg.Members[0].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            var ms = (MethodDeclarationSyntax)agg.Members[0];
            ms.Body.Should().NotBeNull();
            ms.Body.Statements.Count.Should().Be(1);
            ms.Body.Statements[0].Kind().Should().Be(SyntaxKind.LocalDeclarationStatement);
            file.Errors().Length.Should().Be(1);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_UnexpectedCharacter);
        }

        [Fact]
        public void TestGarbageAfterLocalDeclarationArrayInitializerExpression()
        {
            var text = "class c { void m() { int a = { e $ }; } }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(1);
            agg.Members[0].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            var ms = (MethodDeclarationSyntax)agg.Members[0];
            ms.Body.Should().NotBeNull();
            ms.Body.Statements.Count.Should().Be(1);
            ms.Body.Statements[0].Kind().Should().Be(SyntaxKind.LocalDeclarationStatement);
            file.Errors().Length.Should().Be(1);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_UnexpectedCharacter);
        }

        [Fact]
        public void TestGarbageBeforeLocalDeclarationArrayInitializerExpression()
        {
            var text = "class c { void m() { int a = { $ e }; } }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(1);
            agg.Members[0].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            var ms = (MethodDeclarationSyntax)agg.Members[0];
            ms.Body.Should().NotBeNull();
            ms.Body.Statements.Count.Should().Be(1);
            ms.Body.Statements[0].Kind().Should().Be(SyntaxKind.LocalDeclarationStatement);
            file.Errors().Length.Should().Be(1);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_UnexpectedCharacter);
        }

        [Fact]
        public void TestGarbageAfterLocalDeclarationArrayInitializerExpressionAndComma()
        {
            var text = "class c { void m() { int a = { e, $ }; } }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(1);
            agg.Members[0].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            var ms = (MethodDeclarationSyntax)agg.Members[0];
            ms.Body.Should().NotBeNull();
            ms.Body.Statements.Count.Should().Be(1);
            ms.Body.Statements[0].Kind().Should().Be(SyntaxKind.LocalDeclarationStatement);
            file.Errors().Length.Should().Be(1);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_UnexpectedCharacter);
        }

        [Fact]
        public void TestGarbageAfterLocalDeclarationArrayInitializerExpressions()
        {
            var text = "class c { void m() { int a = { e, e $ }; } }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(1);
            agg.Members[0].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            var ms = (MethodDeclarationSyntax)agg.Members[0];
            ms.Body.Should().NotBeNull();
            ms.Body.Statements.Count.Should().Be(1);
            ms.Body.Statements[0].Kind().Should().Be(SyntaxKind.LocalDeclarationStatement);
            file.Errors().Length.Should().Be(1);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_UnexpectedCharacter);
        }

        [Fact]
        public void TestGarbageBetweenLocalDeclarationArrayInitializerExpressions()
        {
            var text = "class c { void m() { int a = { e, $ e }; } }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(1);
            agg.Members[0].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            var ms = (MethodDeclarationSyntax)agg.Members[0];
            ms.Body.Should().NotBeNull();
            ms.Body.Statements.Count.Should().Be(1);
            ms.Body.Statements[0].Kind().Should().Be(SyntaxKind.LocalDeclarationStatement);
            file.Errors().Length.Should().Be(1);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_UnexpectedCharacter);
        }

        [Fact]
        public void TestGarbageBetweenLocalDeclarationArrayInitializerExpressionsWithMissingComma()
        {
            var text = "class c { void m() { int a = { e $ e }; } }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(1);
            agg.Members[0].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            var ms = (MethodDeclarationSyntax)agg.Members[0];
            ms.Body.Should().NotBeNull();
            ms.Body.Statements.Count.Should().Be(1);
            ms.Body.Statements[0].Kind().Should().Be(SyntaxKind.LocalDeclarationStatement);
            file.Errors().Length.Should().Be(2);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_UnexpectedCharacter);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_SyntaxError);
        }

        [Fact]
        public void TestGarbageAfterMethodCallStart()
        {
            var text = "class c { void m() { m($); } }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(1);
            agg.Members[0].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            var ms = (MethodDeclarationSyntax)agg.Members[0];
            ms.Body.Should().NotBeNull();
            ms.Body.Statements.Count.Should().Be(1);
            ms.Body.Statements[0].Kind().Should().Be(SyntaxKind.ExpressionStatement);
            var es = (ExpressionStatementSyntax)ms.Body.Statements[0];
            es.Expression.Kind().Should().Be(SyntaxKind.InvocationExpression);
            file.Errors().Length.Should().Be(1);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_UnexpectedCharacter);
        }

        [Fact]
        public void TestGarbageAfterMethodArgument()
        {
            var text = "class c { void m() { m(a $); } }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(1);
            agg.Members[0].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            var ms = (MethodDeclarationSyntax)agg.Members[0];
            ms.Body.Should().NotBeNull();
            ms.Body.Statements.Count.Should().Be(1);
            ms.Body.Statements[0].Kind().Should().Be(SyntaxKind.ExpressionStatement);
            var es = (ExpressionStatementSyntax)ms.Body.Statements[0];
            es.Expression.Kind().Should().Be(SyntaxKind.InvocationExpression);
            file.Errors().Length.Should().Be(1);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_UnexpectedCharacter);
        }

        [Fact]
        public void TestGarbageBeforeMethodArgument()
        {
            var text = "class c { void m() { m($ a); } }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(1);
            agg.Members[0].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            var ms = (MethodDeclarationSyntax)agg.Members[0];
            ms.Body.Should().NotBeNull();
            ms.Body.Statements.Count.Should().Be(1);
            ms.Body.Statements[0].Kind().Should().Be(SyntaxKind.ExpressionStatement);
            var es = (ExpressionStatementSyntax)ms.Body.Statements[0];
            es.Expression.Kind().Should().Be(SyntaxKind.InvocationExpression);
            file.Errors().Length.Should().Be(1);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_UnexpectedCharacter);
        }

        [Fact]
        public void TestGarbageBeforeMethodArgumentAndComma()
        {
            var text = "class c { void m() { m(a, $); } }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(1);
            agg.Members[0].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            var ms = (MethodDeclarationSyntax)agg.Members[0];
            ms.Body.Should().NotBeNull();
            ms.Body.Statements.Count.Should().Be(1);
            ms.Body.Statements[0].Kind().Should().Be(SyntaxKind.ExpressionStatement);
            var es = (ExpressionStatementSyntax)ms.Body.Statements[0];
            es.Expression.Kind().Should().Be(SyntaxKind.InvocationExpression);
            file.Errors().Length.Should().Be(2);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_InvalidExprTerm);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_UnexpectedCharacter);
        }

        [Fact]
        public void TestSemiColonAfterMethodCallStart()
        {
            var text = "class c { void m() { m(; } }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(1);
            agg.Members[0].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            var ms = (MethodDeclarationSyntax)agg.Members[0];
            ms.Body.Should().NotBeNull();
            ms.Body.Statements.Count.Should().Be(1);
            ms.Body.Statements[0].Kind().Should().Be(SyntaxKind.ExpressionStatement);
            var es = (ExpressionStatementSyntax)ms.Body.Statements[0];
            es.Expression.Kind().Should().Be(SyntaxKind.InvocationExpression);
            file.Errors().Length.Should().Be(1);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_CloseParenExpected);
        }

        [Fact]
        public void TestSemiColonAfterMethodCallArgument()
        {
            var text = "class c { void m() { m(a; } }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(1);
            agg.Members[0].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            var ms = (MethodDeclarationSyntax)agg.Members[0];
            ms.Body.Should().NotBeNull();
            ms.Body.Statements.Count.Should().Be(1);
            ms.Body.Statements[0].Kind().Should().Be(SyntaxKind.ExpressionStatement);
            var es = (ExpressionStatementSyntax)ms.Body.Statements[0];
            es.Expression.Kind().Should().Be(SyntaxKind.InvocationExpression);
            file.Errors().Length.Should().Be(1);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_CloseParenExpected);
        }

        [Fact]
        public void TestSemiColonAfterMethodCallArgumentAndComma()
        {
            var text = "class c { void m() { m(a,; } }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(1);
            agg.Members[0].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            var ms = (MethodDeclarationSyntax)agg.Members[0];
            ms.Body.Should().NotBeNull();
            ms.Body.Statements.Count.Should().Be(1);
            ms.Body.Statements[0].Kind().Should().Be(SyntaxKind.ExpressionStatement);
            var es = (ExpressionStatementSyntax)ms.Body.Statements[0];
            es.Expression.Kind().Should().Be(SyntaxKind.InvocationExpression);
            file.Errors().Length.Should().Be(2);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_InvalidExprTerm);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_CloseParenExpected);
        }

        [Fact]
        public void TestClosingBraceAfterMethodCallArgumentAndCommaWithWhitespace()
        {
            var text = "class c { void m() { m(a,\t\t\n\t\t\t} }";
            var file = this.ParseTree(text);

            var md = (file.Members[0] as TypeDeclarationSyntax).Members[0] as MethodDeclarationSyntax;
            var ie = (md.Body.Statements[0] as ExpressionStatementSyntax).Expression as InvocationExpressionSyntax;

            // whitespace trivia is part of the following '}', not the invocation expression
            ie.ArgumentList.CloseParenToken.ToFullString().Should().Be("");
            md.Body.CloseBraceToken.ToFullString().Should().Be("\t\t\t} ");

            file.Errors().Length.Should().Be(3);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_InvalidExprTerm);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_CloseParenExpected);
            file.Errors()[2].Code.Should().Be((int)ErrorCode.ERR_SemicolonExpected);
        }

        [Fact]
        public void TestStatementAfterMethodCallStart()
        {
            var text = "class c { void m() { m( if(e) {} } }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(1);
            agg.Members[0].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            var ms = (MethodDeclarationSyntax)agg.Members[0];
            ms.Body.Should().NotBeNull();
            ms.Body.Statements.Count.Should().Be(2);
            ms.Body.Statements[0].Kind().Should().Be(SyntaxKind.ExpressionStatement);
            ms.Body.Statements[1].Kind().Should().Be(SyntaxKind.IfStatement);
            var es = (ExpressionStatementSyntax)ms.Body.Statements[0];
            es.Expression.Kind().Should().Be(SyntaxKind.InvocationExpression);
            file.Errors().Length.Should().Be(2);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_CloseParenExpected);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_SemicolonExpected);
        }

        [Fact]
        public void TestStatementAfterMethodCallArgument()
        {
            var text = "class c { void m() { m(a if(e) {} } }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(1);
            agg.Members[0].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            var ms = (MethodDeclarationSyntax)agg.Members[0];
            ms.Body.Should().NotBeNull();
            ms.Body.Statements.Count.Should().Be(2);
            ms.Body.Statements[0].Kind().Should().Be(SyntaxKind.ExpressionStatement);
            ms.Body.Statements[1].Kind().Should().Be(SyntaxKind.IfStatement);
            var es = (ExpressionStatementSyntax)ms.Body.Statements[0];
            es.Expression.Kind().Should().Be(SyntaxKind.InvocationExpression);
            file.Errors().Length.Should().Be(2);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_CloseParenExpected);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_SemicolonExpected);
        }

        [Fact]
        public void TestStatementAfterMethodCallArgumentAndComma()
        {
            var text = "class c { void m() { m(a, if(e) {} } }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(1);
            agg.Members[0].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            var ms = (MethodDeclarationSyntax)agg.Members[0];
            ms.Body.Should().NotBeNull();
            ms.Body.Statements.Count.Should().Be(2);
            ms.Body.Statements[0].Kind().Should().Be(SyntaxKind.ExpressionStatement);
            ms.Body.Statements[1].Kind().Should().Be(SyntaxKind.IfStatement);
            var es = (ExpressionStatementSyntax)ms.Body.Statements[0];
            es.Expression.Kind().Should().Be(SyntaxKind.InvocationExpression);
            file.Errors().Length.Should().Be(3);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_InvalidExprTerm);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_CloseParenExpected);
            file.Errors()[2].Code.Should().Be((int)ErrorCode.ERR_SemicolonExpected);
        }

        [Fact]
        public void TestCloseBraceAfterMethodCallStart()
        {
            var text = "class c { void m() { m( } }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(1);
            agg.Members[0].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            var ms = (MethodDeclarationSyntax)agg.Members[0];
            ms.Body.Should().NotBeNull();
            ms.Body.Statements.Count.Should().Be(1);
            ms.Body.Statements[0].Kind().Should().Be(SyntaxKind.ExpressionStatement);
            var es = (ExpressionStatementSyntax)ms.Body.Statements[0];
            es.Expression.Kind().Should().Be(SyntaxKind.InvocationExpression);
            file.Errors().Length.Should().Be(2);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_CloseParenExpected);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_SemicolonExpected);
        }

        [Fact]
        public void TestCloseBraceAfterMethodCallArgument()
        {
            var text = "class c { void m() { m(a } }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(1);
            agg.Members[0].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            var ms = (MethodDeclarationSyntax)agg.Members[0];
            ms.Body.Should().NotBeNull();
            ms.Body.Statements.Count.Should().Be(1);
            ms.Body.Statements[0].Kind().Should().Be(SyntaxKind.ExpressionStatement);
            var es = (ExpressionStatementSyntax)ms.Body.Statements[0];
            es.Expression.Kind().Should().Be(SyntaxKind.InvocationExpression);
            file.Errors().Length.Should().Be(2);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_CloseParenExpected);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_SemicolonExpected);
        }

        [Fact]
        public void TestCloseBraceAfterMethodCallArgumentAndComma()
        {
            var text = "class c { void m() { m(a, } }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(1);
            agg.Members[0].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            var ms = (MethodDeclarationSyntax)agg.Members[0];
            ms.Body.Should().NotBeNull();
            ms.Body.Statements.Count.Should().Be(1);
            ms.Body.Statements[0].Kind().Should().Be(SyntaxKind.ExpressionStatement);
            var es = (ExpressionStatementSyntax)ms.Body.Statements[0];
            es.Expression.Kind().Should().Be(SyntaxKind.InvocationExpression);
            file.Errors().Length.Should().Be(3);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_InvalidExprTerm);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_CloseParenExpected);
            file.Errors()[2].Code.Should().Be((int)ErrorCode.ERR_SemicolonExpected);
        }

        [Fact]
        public void TestGarbageAfterIndexerStart()
        {
            var text = "class c { void m() { ++a[$]; } }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(1);
            agg.Members[0].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            var ms = (MethodDeclarationSyntax)agg.Members[0];
            ms.Body.Should().NotBeNull();
            ms.Body.Statements.Count.Should().Be(1);
            ms.Body.Statements[0].Kind().Should().Be(SyntaxKind.ExpressionStatement);
            var es = (ExpressionStatementSyntax)ms.Body.Statements[0];
            es.Expression.Kind().Should().Be(SyntaxKind.PreIncrementExpression);
            ((PrefixUnaryExpressionSyntax)es.Expression).Operand.Kind().Should().Be(SyntaxKind.ElementAccessExpression);
            file.Errors().Length.Should().Be(1);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_UnexpectedCharacter);
        }

        [Fact]
        public void TestGarbageAfterIndexerArgument()
        {
            var text = "class c { void m() { ++a[e $]; } }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(1);
            agg.Members[0].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            var ms = (MethodDeclarationSyntax)agg.Members[0];
            ms.Body.Should().NotBeNull();
            ms.Body.Statements.Count.Should().Be(1);
            ms.Body.Statements[0].Kind().Should().Be(SyntaxKind.ExpressionStatement);
            var es = (ExpressionStatementSyntax)ms.Body.Statements[0];
            es.Expression.Kind().Should().Be(SyntaxKind.PreIncrementExpression);
            ((PrefixUnaryExpressionSyntax)es.Expression).Operand.Kind().Should().Be(SyntaxKind.ElementAccessExpression);
            file.Errors().Length.Should().Be(1);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_UnexpectedCharacter);
        }

        [Fact]
        public void TestGarbageBeforeIndexerArgument()
        {
            var text = "class c { void m() { ++a[$ e]; } }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(1);
            agg.Members[0].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            var ms = (MethodDeclarationSyntax)agg.Members[0];
            ms.Body.Should().NotBeNull();
            ms.Body.Statements.Count.Should().Be(1);
            ms.Body.Statements[0].Kind().Should().Be(SyntaxKind.ExpressionStatement);
            var es = (ExpressionStatementSyntax)ms.Body.Statements[0];
            es.Expression.Kind().Should().Be(SyntaxKind.PreIncrementExpression);
            ((PrefixUnaryExpressionSyntax)es.Expression).Operand.Kind().Should().Be(SyntaxKind.ElementAccessExpression);
            file.Errors().Length.Should().Be(1);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_UnexpectedCharacter);
        }

        [Fact]
        public void TestGarbageBeforeIndexerArgumentAndComma()
        {
            var text = "class c { void m() { ++a[e, $]; } }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(1);
            agg.Members[0].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            var ms = (MethodDeclarationSyntax)agg.Members[0];
            ms.Body.Should().NotBeNull();
            ms.Body.Statements.Count.Should().Be(1);
            ms.Body.Statements[0].Kind().Should().Be(SyntaxKind.ExpressionStatement);
            var es = (ExpressionStatementSyntax)ms.Body.Statements[0];
            es.Expression.Kind().Should().Be(SyntaxKind.PreIncrementExpression);
            ((PrefixUnaryExpressionSyntax)es.Expression).Operand.Kind().Should().Be(SyntaxKind.ElementAccessExpression);
            file.Errors().Length.Should().Be(2);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_InvalidExprTerm);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_UnexpectedCharacter);
        }

        [Fact]
        public void TestSemiColonAfterIndexerStart()
        {
            var text = "class c { void m() { ++a[; } }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(1);
            agg.Members[0].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            var ms = (MethodDeclarationSyntax)agg.Members[0];
            ms.Body.Should().NotBeNull();
            ms.Body.Statements.Count.Should().Be(1);
            ms.Body.Statements[0].Kind().Should().Be(SyntaxKind.ExpressionStatement);
            var es = (ExpressionStatementSyntax)ms.Body.Statements[0];
            es.Expression.Kind().Should().Be(SyntaxKind.PreIncrementExpression);
            ((PrefixUnaryExpressionSyntax)es.Expression).Operand.Kind().Should().Be(SyntaxKind.ElementAccessExpression);
            file.Errors().Length.Should().Be(1);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_SyntaxError);
        }

        [Fact]
        public void TestSemiColonAfterIndexerArgument()
        {
            var text = "class c { void m() { ++a[e; } }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(1);
            agg.Members[0].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            var ms = (MethodDeclarationSyntax)agg.Members[0];
            ms.Body.Should().NotBeNull();
            ms.Body.Statements.Count.Should().Be(1);
            ms.Body.Statements[0].Kind().Should().Be(SyntaxKind.ExpressionStatement);
            var es = (ExpressionStatementSyntax)ms.Body.Statements[0];
            es.Expression.Kind().Should().Be(SyntaxKind.PreIncrementExpression);
            ((PrefixUnaryExpressionSyntax)es.Expression).Operand.Kind().Should().Be(SyntaxKind.ElementAccessExpression);
            file.Errors().Length.Should().Be(1);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_SyntaxError);
        }

        [Fact]
        public void TestSemiColonAfterIndexerArgumentAndComma()
        {
            var text = "class c { void m() { ++a[e,; } }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(1);
            agg.Members[0].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            var ms = (MethodDeclarationSyntax)agg.Members[0];
            ms.Body.Should().NotBeNull();
            ms.Body.Statements.Count.Should().Be(1);
            ms.Body.Statements[0].Kind().Should().Be(SyntaxKind.ExpressionStatement);
            var es = (ExpressionStatementSyntax)ms.Body.Statements[0];
            es.Expression.Kind().Should().Be(SyntaxKind.PreIncrementExpression);
            ((PrefixUnaryExpressionSyntax)es.Expression).Operand.Kind().Should().Be(SyntaxKind.ElementAccessExpression);
            file.Errors().Length.Should().Be(2);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_InvalidExprTerm);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_SyntaxError);
        }

        [Fact]
        public void TestStatementAfterIndexerStart()
        {
            var text = "class c { void m() { ++a[ if(e) {} } }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(1);
            agg.Members[0].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            var ms = (MethodDeclarationSyntax)agg.Members[0];
            ms.Body.Should().NotBeNull();
            ms.Body.Statements.Count.Should().Be(2);
            ms.Body.Statements[0].Kind().Should().Be(SyntaxKind.ExpressionStatement);
            ms.Body.Statements[1].Kind().Should().Be(SyntaxKind.IfStatement);
            var es = (ExpressionStatementSyntax)ms.Body.Statements[0];
            es.Expression.Kind().Should().Be(SyntaxKind.PreIncrementExpression);
            ((PrefixUnaryExpressionSyntax)es.Expression).Operand.Kind().Should().Be(SyntaxKind.ElementAccessExpression);
            file.Errors().Length.Should().Be(2);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_SyntaxError);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_SemicolonExpected);
        }

        [Fact]
        public void TestStatementAfterIndexerArgument()
        {
            var text = "class c { void m() { ++a[e if(e) {} } }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(1);
            agg.Members[0].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            var ms = (MethodDeclarationSyntax)agg.Members[0];
            ms.Body.Should().NotBeNull();
            ms.Body.Statements.Count.Should().Be(2);
            ms.Body.Statements[0].Kind().Should().Be(SyntaxKind.ExpressionStatement);
            ms.Body.Statements[1].Kind().Should().Be(SyntaxKind.IfStatement);
            var es = (ExpressionStatementSyntax)ms.Body.Statements[0];
            es.Expression.Kind().Should().Be(SyntaxKind.PreIncrementExpression);
            ((PrefixUnaryExpressionSyntax)es.Expression).Operand.Kind().Should().Be(SyntaxKind.ElementAccessExpression);
            file.Errors().Length.Should().Be(2);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_SyntaxError);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_SemicolonExpected);
        }

        [Fact]
        public void TestStatementAfterIndexerArgumentAndComma()
        {
            var text = "class c { void m() { ++a[e, if(e) {} } }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(1);
            agg.Members[0].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            var ms = (MethodDeclarationSyntax)agg.Members[0];
            ms.Body.Should().NotBeNull();
            ms.Body.Statements.Count.Should().Be(2);
            ms.Body.Statements[0].Kind().Should().Be(SyntaxKind.ExpressionStatement);
            ms.Body.Statements[1].Kind().Should().Be(SyntaxKind.IfStatement);
            var es = (ExpressionStatementSyntax)ms.Body.Statements[0];
            es.Expression.Kind().Should().Be(SyntaxKind.PreIncrementExpression);
            ((PrefixUnaryExpressionSyntax)es.Expression).Operand.Kind().Should().Be(SyntaxKind.ElementAccessExpression);
            file.Errors().Length.Should().Be(3);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_InvalidExprTerm);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_SyntaxError);
            file.Errors()[2].Code.Should().Be((int)ErrorCode.ERR_SemicolonExpected);
        }

        [Fact]
        public void TestCloseBraceAfterIndexerStart()
        {
            var text = "class c { void m() { ++a[ } }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(1);
            agg.Members[0].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            var ms = (MethodDeclarationSyntax)agg.Members[0];
            ms.Body.Should().NotBeNull();
            ms.Body.Statements.Count.Should().Be(1);
            ms.Body.Statements[0].Kind().Should().Be(SyntaxKind.ExpressionStatement);
            var es = (ExpressionStatementSyntax)ms.Body.Statements[0];
            es.Expression.Kind().Should().Be(SyntaxKind.PreIncrementExpression);
            ((PrefixUnaryExpressionSyntax)es.Expression).Operand.Kind().Should().Be(SyntaxKind.ElementAccessExpression);
            file.Errors().Length.Should().Be(2);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_SyntaxError);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_SemicolonExpected);
        }

        [Fact]
        public void TestCloseBraceAfterIndexerArgument()
        {
            var text = "class c { void m() { ++a[e } }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(1);
            agg.Members[0].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            var ms = (MethodDeclarationSyntax)agg.Members[0];
            ms.Body.Should().NotBeNull();
            ms.Body.Statements.Count.Should().Be(1);
            ms.Body.Statements[0].Kind().Should().Be(SyntaxKind.ExpressionStatement);
            var es = (ExpressionStatementSyntax)ms.Body.Statements[0];
            es.Expression.Kind().Should().Be(SyntaxKind.PreIncrementExpression);
            ((PrefixUnaryExpressionSyntax)es.Expression).Operand.Kind().Should().Be(SyntaxKind.ElementAccessExpression);
            file.Errors().Length.Should().Be(2);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_SyntaxError);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_SemicolonExpected);
        }

        [Fact]
        public void TestCloseBraceAfterIndexerArgumentAndComma()
        {
            var text = "class c { void m() { ++a[e, } }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(1);
            agg.Members[0].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            var ms = (MethodDeclarationSyntax)agg.Members[0];
            ms.Body.Should().NotBeNull();
            ms.Body.Statements.Count.Should().Be(1);
            ms.Body.Statements[0].Kind().Should().Be(SyntaxKind.ExpressionStatement);
            var es = (ExpressionStatementSyntax)ms.Body.Statements[0];
            es.Expression.Kind().Should().Be(SyntaxKind.PreIncrementExpression);
            ((PrefixUnaryExpressionSyntax)es.Expression).Operand.Kind().Should().Be(SyntaxKind.ElementAccessExpression);
            file.Errors().Length.Should().Be(3);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_InvalidExprTerm);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_SyntaxError);
            file.Errors()[2].Code.Should().Be((int)ErrorCode.ERR_SemicolonExpected);
        }

        [Fact]
        public void TestOpenBraceAfterFixedStatementStart()
        {
            var text = "class c { void m() { fixed(t v { } } }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(1);
            agg.Members[0].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            var ms = (MethodDeclarationSyntax)agg.Members[0];
            ms.Body.Should().NotBeNull();
            ms.Body.Statements.Count.Should().Be(1);
            ms.Body.Statements[0].Kind().Should().Be(SyntaxKind.FixedStatement);
            file.Errors().Length.Should().Be(1);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_CloseParenExpected);
        }

        [Fact]
        public void TestSemiColonAfterFixedStatementStart()
        {
            var text = "class c { void m() { fixed(t v; } }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(1);
            agg.Members[0].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            var ms = (MethodDeclarationSyntax)agg.Members[0];
            ms.Body.Should().NotBeNull();
            ms.Body.Statements.Count.Should().Be(1);
            ms.Body.Statements[0].Kind().Should().Be(SyntaxKind.FixedStatement);
            var diags = file.ErrorsAndWarnings();
            diags.Length.Should().Be(1);
            diags[0].Code.Should().Be((int)ErrorCode.ERR_CloseParenExpected);

            CreateCompilation(text).VerifyDiagnostics(
                // (1,31): error CS1026: ) expected
                // class c { void m() { fixed(t v; } }
                Diagnostic(ErrorCode.ERR_CloseParenExpected, ";").WithLocation(1, 31),
                // (1,7): warning CS8981: The type name 'c' only contains lower-cased ascii characters. Such names may become reserved for the language.
                // class c { void m() { fixed(t v; } }
                Diagnostic(ErrorCode.WRN_LowerCaseTypeName, "c").WithArguments("c").WithLocation(1, 7),
                // (1,22): error CS0214: Pointers and fixed size buffers may only be used in an unsafe context
                // class c { void m() { fixed(t v; } }
                Diagnostic(ErrorCode.ERR_UnsafeNeeded, "fixed(t v;").WithLocation(1, 22),
                // (1,28): error CS0246: The type or namespace name 't' could not be found (are you missing a using directive or an assembly reference?)
                // class c { void m() { fixed(t v; } }
                Diagnostic(ErrorCode.ERR_SingleTypeNameNotFound, "t").WithArguments("t").WithLocation(1, 28),
                // (1,30): error CS0209: The type of a local declared in a fixed statement must be a pointer type
                // class c { void m() { fixed(t v; } }
                Diagnostic(ErrorCode.ERR_BadFixedInitType, "v").WithLocation(1, 30),
                // (1,30): error CS0210: You must provide an initializer in a fixed or using statement declaration
                // class c { void m() { fixed(t v; } }
                Diagnostic(ErrorCode.ERR_FixedMustInit, "v").WithLocation(1, 30),
                // (1,31): warning CS0642: Possible mistaken empty statement
                // class c { void m() { fixed(t v; } }
                Diagnostic(ErrorCode.WRN_PossibleMistakenNullStatement, ";").WithLocation(1, 31));
        }

        [Fact]
        public void TestSemiColonAfterFixedStatementType()
        {
            var text = "class c { void m() { fixed(t ) { } } }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(1);
            agg.Members[0].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            var ms = (MethodDeclarationSyntax)agg.Members[0];
            ms.Body.Should().NotBeNull();
            ms.Body.Statements.Count.Should().Be(1);
            ms.Body.Statements[0].Kind().Should().Be(SyntaxKind.FixedStatement);
            file.Errors().Length.Should().Be(1);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_IdentifierExpected);
        }

        [Fact]
        public void TestCatchAfterTryBlockStart()
        {
            var text = "class c { void m() { try { catch { } } }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(1);
            agg.Members[0].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            var ms = (MethodDeclarationSyntax)agg.Members[0];
            ms.Body.Should().NotBeNull();
            ms.Body.Statements.Count.Should().Be(1);
            ms.Body.Statements[0].Kind().Should().Be(SyntaxKind.TryStatement);
            file.Errors().Length.Should().Be(1);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_RbraceExpected);
        }

        [Fact]
        public void TestFinallyAfterTryBlockStart()
        {
            var text = "class c { void m() { try { finally { } } }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(1);
            agg.Members[0].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            var ms = (MethodDeclarationSyntax)agg.Members[0];
            ms.Body.Should().NotBeNull();
            ms.Body.Statements.Count.Should().Be(1);
            ms.Body.Statements[0].Kind().Should().Be(SyntaxKind.TryStatement);
            file.Errors().Length.Should().Be(1);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_RbraceExpected);
        }

        [Fact]
        public void TestFinallyAfterCatchStart()
        {
            var text = "class c { void m() { try { } catch finally { } } }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(1);
            agg.Members[0].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            var ms = (MethodDeclarationSyntax)agg.Members[0];
            ms.Body.Should().NotBeNull();
            ms.Body.Statements.Count.Should().Be(1);
            ms.Body.Statements[0].Kind().Should().Be(SyntaxKind.TryStatement);
            file.Errors().Length.Should().Be(2);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_LbraceExpected);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_RbraceExpected);
        }

        [Fact]
        public void TestCatchAfterCatchStart()
        {
            var text = "class c { void m() { try { } catch catch { } } }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(1);
            agg.Members[0].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            var ms = (MethodDeclarationSyntax)agg.Members[0];
            ms.Body.Should().NotBeNull();
            ms.Body.Statements.Count.Should().Be(1);
            ms.Body.Statements[0].Kind().Should().Be(SyntaxKind.TryStatement);
            file.Errors().Length.Should().Be(2);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_LbraceExpected);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_RbraceExpected);
        }

        [Fact]
        public void TestFinallyAfterCatchParameterStart()
        {
            var text = "class c { void m() { try { } catch (t finally { } } }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(1);
            agg.Members[0].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            var ms = (MethodDeclarationSyntax)agg.Members[0];
            ms.Body.Should().NotBeNull();
            ms.Body.Statements.Count.Should().Be(1);
            ms.Body.Statements[0].Kind().Should().Be(SyntaxKind.TryStatement);
            file.Errors().Length.Should().Be(3);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_CloseParenExpected);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_LbraceExpected);
            file.Errors()[2].Code.Should().Be((int)ErrorCode.ERR_RbraceExpected);
        }

        [Fact]
        public void TestCatchAfterCatchParameterStart()
        {
            var text = "class c { void m() { try { } catch (t catch { } } }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(1);
            agg.Members[0].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            var ms = (MethodDeclarationSyntax)agg.Members[0];
            ms.Body.Should().NotBeNull();
            ms.Body.Statements.Count.Should().Be(1);
            ms.Body.Statements[0].Kind().Should().Be(SyntaxKind.TryStatement);
            file.Errors().Length.Should().Be(3);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_CloseParenExpected);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_LbraceExpected);
            file.Errors()[2].Code.Should().Be((int)ErrorCode.ERR_RbraceExpected);
        }

        [Fact]
        public void TestCloseBraceAfterCatchStart()
        {
            var text = "class c { void m() { try { } catch } } }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(1);
            agg.Members[0].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            var ms = (MethodDeclarationSyntax)agg.Members[0];
            ms.Body.Should().NotBeNull();
            ms.Body.Statements.Count.Should().Be(1);
            ms.Body.Statements[0].Kind().Should().Be(SyntaxKind.TryStatement);
            file.Errors().Length.Should().Be(1);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_LbraceExpected);
        }

        [Fact]
        public void TestCloseBraceAfterCatchParameterStart()
        {
            var text = "class c { void m() { try { } catch(t } } }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(1);
            agg.Members[0].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            var ms = (MethodDeclarationSyntax)agg.Members[0];
            ms.Body.Should().NotBeNull();
            ms.Body.Statements.Count.Should().Be(1);
            ms.Body.Statements[0].Kind().Should().Be(SyntaxKind.TryStatement);
            file.Errors().Length.Should().Be(2);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_CloseParenExpected);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_LbraceExpected);
        }

        [Fact]
        public void TestSemiColonAfterDoWhileExpressionIndexer()
        {
            // this shows that ';' is an exit condition for the expression
            var text = "class c { void m() { do { } while(e[; } }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(1);
            agg.Members[0].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            var ms = (MethodDeclarationSyntax)agg.Members[0];
            ms.Body.Should().NotBeNull();
            ms.Body.Statements.Count.Should().Be(1);
            ms.Body.Statements[0].Kind().Should().Be(SyntaxKind.DoStatement);
            file.Errors().Length.Should().Be(2);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_SyntaxError);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_CloseParenExpected);
        }

        [Fact]
        public void TestCloseParenAfterDoWhileExpressionIndexerStart()
        {
            // this shows that ')' is an exit condition for the expression
            var text = "class c { void m() { do { } while(e[); } }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(1);
            agg.Members[0].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            var ms = (MethodDeclarationSyntax)agg.Members[0];
            ms.Body.Should().NotBeNull();
            ms.Body.Statements.Count.Should().Be(1);
            ms.Body.Statements[0].Kind().Should().Be(SyntaxKind.DoStatement);
            file.Errors().Verify(
                // error CS1003: Syntax error, ']' expected
                Diagnostic(ErrorCode.ERR_SyntaxError).WithArguments("]").WithLocation(1, 1),
                // error CS1026: ) expected
                Diagnostic(ErrorCode.ERR_CloseParenExpected).WithLocation(1, 1)
                );
        }

        [Fact]
        public void TestCloseParenAfterForStatementInitializerStart()
        {
            // this shows that ';' is an exit condition for the initializer expression
            var text = "class c { void m() { for (a[;;) { } } }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(1);
            agg.Members[0].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            var ms = (MethodDeclarationSyntax)agg.Members[0];
            ms.Body.Should().NotBeNull();
            ms.Body.Statements.Count.Should().Be(1);
            ms.Body.Statements[0].Kind().Should().Be(SyntaxKind.ForStatement);
            file.Errors().Length.Should().Be(1);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_SyntaxError);
        }

        [Fact]
        public void TestOpenBraceAfterForStatementInitializerStart()
        {
            // this shows that '{' is an exit condition for the initializer expression
            var text = "class c { void m() { for (a[ { } } }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(1);
            agg.Members[0].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            var ms = (MethodDeclarationSyntax)agg.Members[0];
            ms.Body.Should().NotBeNull();
            ms.Body.Statements.Count.Should().Be(1);
            ms.Body.Statements[0].Kind().Should().Be(SyntaxKind.ForStatement);
            file.Errors().Length.Should().Be(5);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_SyntaxError);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_SemicolonExpected);
            file.Errors()[2].Code.Should().Be((int)ErrorCode.ERR_InvalidExprTerm);
            file.Errors()[3].Code.Should().Be((int)ErrorCode.ERR_SemicolonExpected);
            file.Errors()[4].Code.Should().Be((int)ErrorCode.ERR_CloseParenExpected);
        }

        [Fact]
        public void TestCloseBraceAfterForStatementInitializerStart()
        {
            // this shows that '}' is an exit condition for the initializer expression
            var text = "class c { void m() { for (a[ } }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(1);
            agg.Members[0].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            var ms = (MethodDeclarationSyntax)agg.Members[0];
            ms.Body.Should().NotBeNull();
            ms.Body.Statements.Count.Should().Be(1);
            ms.Body.Statements[0].Kind().Should().Be(SyntaxKind.ForStatement);
            file.Errors().Length.Should().Be(7);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_SyntaxError);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_SemicolonExpected);
            file.Errors()[2].Code.Should().Be((int)ErrorCode.ERR_InvalidExprTerm);
            file.Errors()[3].Code.Should().Be((int)ErrorCode.ERR_SemicolonExpected);
            file.Errors()[4].Code.Should().Be((int)ErrorCode.ERR_CloseParenExpected);
            file.Errors()[5].Code.Should().Be((int)ErrorCode.ERR_InvalidExprTerm);
            file.Errors()[6].Code.Should().Be((int)ErrorCode.ERR_SemicolonExpected);
        }

        [Fact]
        public void TestCloseParenAfterForStatementConditionStart()
        {
            var text = "class c { void m() { for (;a[;) { } } }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(1);
            agg.Members[0].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            var ms = (MethodDeclarationSyntax)agg.Members[0];
            ms.Body.Should().NotBeNull();
            ms.Body.Statements.Count.Should().Be(1);
            ms.Body.Statements[0].Kind().Should().Be(SyntaxKind.ForStatement);
            file.Errors().Length.Should().Be(1);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_SyntaxError);
        }

        [Fact]
        public void TestOpenBraceAfterForStatementConditionStart()
        {
            var text = "class c { void m() { for (;a[ { } } }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(1);
            agg.Members[0].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            var ms = (MethodDeclarationSyntax)agg.Members[0];
            ms.Body.Should().NotBeNull();
            ms.Body.Statements.Count.Should().Be(1);
            ms.Body.Statements[0].Kind().Should().Be(SyntaxKind.ForStatement);
            file.Errors().Length.Should().Be(3);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_SyntaxError);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_SemicolonExpected);
            file.Errors()[2].Code.Should().Be((int)ErrorCode.ERR_CloseParenExpected);
        }

        [Fact]
        public void TestCloseBraceAfterForStatementConditionStart()
        {
            var text = "class c { void m() { for (;a[ } }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(1);
            agg.Members[0].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            var ms = (MethodDeclarationSyntax)agg.Members[0];
            ms.Body.Should().NotBeNull();
            ms.Body.Statements.Count.Should().Be(1);
            ms.Body.Statements[0].Kind().Should().Be(SyntaxKind.ForStatement);
            file.Errors().Length.Should().Be(5);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_SyntaxError);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_SemicolonExpected);
            file.Errors()[2].Code.Should().Be((int)ErrorCode.ERR_CloseParenExpected);
            file.Errors()[3].Code.Should().Be((int)ErrorCode.ERR_InvalidExprTerm);
            file.Errors()[4].Code.Should().Be((int)ErrorCode.ERR_SemicolonExpected);
        }

        [Fact]
        public void TestCloseParenAfterForStatementIncrementerStart()
        {
            var text = "class c { void m() { for (;;++a[) { } } }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(1);
            agg.Members[0].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            var ms = (MethodDeclarationSyntax)agg.Members[0];
            ms.Body.Should().NotBeNull();
            ms.Body.Statements.Count.Should().Be(1);
            ms.Body.Statements[0].Kind().Should().Be(SyntaxKind.ForStatement);
            file.Errors().Verify(
                // error CS1003: Syntax error, ']' expected
                Diagnostic(ErrorCode.ERR_SyntaxError).WithArguments("]").WithLocation(1, 1),
                // error CS1026: ) expected
                Diagnostic(ErrorCode.ERR_CloseParenExpected).WithLocation(1, 1)
                );
        }

        [Fact]
        public void TestOpenBraceAfterForStatementIncrementerStart()
        {
            var text = "class c { void m() { for (;;++a[ { } } }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(1);
            agg.Members[0].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            var ms = (MethodDeclarationSyntax)agg.Members[0];
            ms.Body.Should().NotBeNull();
            ms.Body.Statements.Count.Should().Be(1);
            ms.Body.Statements[0].Kind().Should().Be(SyntaxKind.ForStatement);
            file.Errors().Length.Should().Be(2);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_SyntaxError);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_CloseParenExpected);
        }

        [Fact]
        public void TestCloseBraceAfterForStatementIncrementerStart()
        {
            var text = "class c { void m() { for (;;++a[ } }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(1);
            agg.Members[0].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            var ms = (MethodDeclarationSyntax)agg.Members[0];
            ms.Body.Should().NotBeNull();
            ms.Body.Statements.Count.Should().Be(1);
            ms.Body.Statements[0].Kind().Should().Be(SyntaxKind.ForStatement);
            file.Errors().Length.Should().Be(4);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_SyntaxError);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_CloseParenExpected);
            file.Errors()[2].Code.Should().Be((int)ErrorCode.ERR_InvalidExprTerm);
            file.Errors()[3].Code.Should().Be((int)ErrorCode.ERR_SemicolonExpected);
        }

        [Fact]
        public void TestCloseBraceAfterAnonymousTypeStart()
        {
            // empty anonymous type is perfectly legal
            var text = "class c { void m() { var x = new {}; } }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(1);
            agg.Members[0].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            var ms = (MethodDeclarationSyntax)agg.Members[0];
            ms.Body.Should().NotBeNull();
            ms.Body.Statements.Count.Should().Be(1);
            ms.Body.Statements[0].Kind().Should().Be(SyntaxKind.LocalDeclarationStatement);
            var ds = (LocalDeclarationStatementSyntax)ms.Body.Statements[0];
            ds.Declaration.Variables.Count.Should().Be(1);
            ds.Declaration.Variables[0].Initializer.Should().NotBeNull();
            ds.Declaration.Variables[0].Initializer.EqualsToken.Kind().Should().NotBe(SyntaxKind.None);
            ds.Declaration.Variables[0].Initializer.Value.Should().NotBeNull();
            ds.Declaration.Variables[0].Initializer.Value.Kind().Should().Be(SyntaxKind.AnonymousObjectCreationExpression);
            file.Errors().Length.Should().Be(0);
        }

        [Fact]
        public void TestSemicolonAfterAnonymousTypeStart()
        {
            var text = "class c { void m() { var x = new {; } }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(1);
            agg.Members[0].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            var ms = (MethodDeclarationSyntax)agg.Members[0];
            ms.Body.Should().NotBeNull();
            ms.Body.Statements.Count.Should().Be(1);
            ms.Body.Statements[0].Kind().Should().Be(SyntaxKind.LocalDeclarationStatement);
            var ds = (LocalDeclarationStatementSyntax)ms.Body.Statements[0];
            ds.Declaration.Variables.Count.Should().Be(1);
            ds.Declaration.Variables[0].Initializer.Kind().Should().NotBe(SyntaxKind.None);
            ds.Declaration.Variables[0].Initializer.EqualsToken.Should().NotBe(default);
            ds.Declaration.Variables[0].Initializer.Value.Should().NotBeNull();
            ds.Declaration.Variables[0].Initializer.Value.Kind().Should().Be(SyntaxKind.AnonymousObjectCreationExpression);
            file.Errors().Length.Should().Be(1);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_RbraceExpected);
        }

        [Fact]
        public void TestSemicolonAfterAnonymousTypeMemberStart()
        {
            var text = "class c { void m() { var x = new {a; } }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(1);
            agg.Members[0].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            var ms = (MethodDeclarationSyntax)agg.Members[0];
            ms.Body.Should().NotBeNull();
            ms.Body.Statements.Count.Should().Be(1);
            ms.Body.Statements[0].Kind().Should().Be(SyntaxKind.LocalDeclarationStatement);
            var ds = (LocalDeclarationStatementSyntax)ms.Body.Statements[0];
            ds.Declaration.Variables.Count.Should().Be(1);
            ds.Declaration.Variables[0].Initializer.Should().NotBeNull();
            ds.Declaration.Variables[0].Initializer.EqualsToken.Kind().Should().NotBe(SyntaxKind.None);
            ds.Declaration.Variables[0].Initializer.Value.Should().NotBeNull();
            ds.Declaration.Variables[0].Initializer.Value.Kind().Should().Be(SyntaxKind.AnonymousObjectCreationExpression);
            file.Errors().Length.Should().Be(1);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_RbraceExpected);
        }

        [Fact]
        public void TestSemicolonAfterAnonymousTypeMemberEquals()
        {
            var text = "class c { void m() { var x = new {a =; } }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(1);
            agg.Members[0].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            var ms = (MethodDeclarationSyntax)agg.Members[0];
            ms.Body.Should().NotBeNull();
            ms.Body.Statements.Count.Should().Be(1);
            ms.Body.Statements[0].Kind().Should().Be(SyntaxKind.LocalDeclarationStatement);
            var ds = (LocalDeclarationStatementSyntax)ms.Body.Statements[0];
            ds.Declaration.Variables.Count.Should().Be(1);
            ds.Declaration.Variables[0].Initializer.Should().NotBeNull();
            ds.Declaration.Variables[0].Initializer.EqualsToken.Kind().Should().NotBe(SyntaxKind.None);
            ds.Declaration.Variables[0].Initializer.Value.Should().NotBeNull();
            ds.Declaration.Variables[0].Initializer.Value.Kind().Should().Be(SyntaxKind.AnonymousObjectCreationExpression);
            file.Errors().Length.Should().Be(2);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_InvalidExprTerm);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_RbraceExpected);
        }

        [Fact]
        public void TestSemicolonAfterAnonymousTypeMember()
        {
            var text = "class c { void m() { var x = new {a = b; } }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(1);
            agg.Members[0].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            var ms = (MethodDeclarationSyntax)agg.Members[0];
            ms.Body.Should().NotBeNull();
            ms.Body.Statements.Count.Should().Be(1);
            ms.Body.Statements[0].Kind().Should().Be(SyntaxKind.LocalDeclarationStatement);
            var ds = (LocalDeclarationStatementSyntax)ms.Body.Statements[0];
            ds.Declaration.Variables.Count.Should().Be(1);
            ds.Declaration.Variables[0].Initializer.Should().NotBeNull();
            ds.Declaration.Variables[0].Initializer.EqualsToken.Kind().Should().NotBe(SyntaxKind.None);
            ds.Declaration.Variables[0].Initializer.Value.Should().NotBeNull();
            ds.Declaration.Variables[0].Initializer.Value.Kind().Should().Be(SyntaxKind.AnonymousObjectCreationExpression);
            file.Errors().Length.Should().Be(1);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_RbraceExpected);
        }

        [Fact]
        public void TestSemicolonAfterAnonymousTypeMemberComma()
        {
            var text = "class c { void m() { var x = new {a = b, ; } }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(1);
            agg.Members[0].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            var ms = (MethodDeclarationSyntax)agg.Members[0];
            ms.Body.Should().NotBeNull();
            ms.Body.Statements.Count.Should().Be(1);
            ms.Body.Statements[0].Kind().Should().Be(SyntaxKind.LocalDeclarationStatement);
            var ds = (LocalDeclarationStatementSyntax)ms.Body.Statements[0];
            ds.Declaration.Variables.Count.Should().Be(1);
            ds.Declaration.Variables[0].Initializer.Should().NotBeNull();
            ds.Declaration.Variables[0].Initializer.EqualsToken.Kind().Should().NotBe(SyntaxKind.None);
            ds.Declaration.Variables[0].Initializer.Value.Should().NotBeNull();
            ds.Declaration.Variables[0].Initializer.Value.Kind().Should().Be(SyntaxKind.AnonymousObjectCreationExpression);
            file.Errors().Length.Should().Be(1);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_RbraceExpected);
        }

        [Fact]
        public void TestStatementAfterAnonymousTypeStart()
        {
            var text = "class c { void m() { var x = new { while (x) {} } }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(1);
            agg.Members[0].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            var ms = (MethodDeclarationSyntax)agg.Members[0];
            ms.Body.Should().NotBeNull();
            ms.Body.Statements.Count.Should().Be(2);
            ms.Body.Statements[0].Kind().Should().Be(SyntaxKind.LocalDeclarationStatement);
            ms.Body.Statements[1].Kind().Should().Be(SyntaxKind.WhileStatement);
            var ds = (LocalDeclarationStatementSyntax)ms.Body.Statements[0];
            ds.Declaration.Variables.Count.Should().Be(1);
            ds.Declaration.Variables[0].Initializer.Should().NotBeNull();
            ds.Declaration.Variables[0].Initializer.EqualsToken.Kind().Should().NotBe(SyntaxKind.None);
            ds.Declaration.Variables[0].Initializer.Value.Should().NotBeNull();
            ds.Declaration.Variables[0].Initializer.Value.Kind().Should().Be(SyntaxKind.AnonymousObjectCreationExpression);
            file.Errors().Length.Should().Be(2);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_RbraceExpected);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_SemicolonExpected);
        }

        [Fact]
        public void TestStatementAfterAnonymousTypeMemberStart()
        {
            var text = "class c { void m() { var x = new { a while (x) {} } }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(1);
            agg.Members[0].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            var ms = (MethodDeclarationSyntax)agg.Members[0];
            ms.Body.Should().NotBeNull();
            ms.Body.Statements.Count.Should().Be(2);
            ms.Body.Statements[0].Kind().Should().Be(SyntaxKind.LocalDeclarationStatement);
            ms.Body.Statements[1].Kind().Should().Be(SyntaxKind.WhileStatement);
            var ds = (LocalDeclarationStatementSyntax)ms.Body.Statements[0];
            ds.Declaration.Variables.Count.Should().Be(1);
            ds.Declaration.Variables[0].Initializer.Should().NotBeNull();
            ds.Declaration.Variables[0].Initializer.EqualsToken.Kind().Should().NotBe(SyntaxKind.None);
            ds.Declaration.Variables[0].Initializer.Value.Should().NotBeNull();
            ds.Declaration.Variables[0].Initializer.Value.Kind().Should().Be(SyntaxKind.AnonymousObjectCreationExpression);
            file.Errors().Length.Should().Be(2);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_RbraceExpected);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_SemicolonExpected);
        }

        [Fact]
        public void TestStatementAfterAnonymousTypeMemberEquals()
        {
            var text = "class c { void m() { var x = new { a = while (x) {} } }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(1);
            agg.Members[0].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            var ms = (MethodDeclarationSyntax)agg.Members[0];
            ms.Body.Should().NotBeNull();
            ms.Body.Statements.Count.Should().Be(2);
            ms.Body.Statements[0].Kind().Should().Be(SyntaxKind.LocalDeclarationStatement);
            ms.Body.Statements[1].Kind().Should().Be(SyntaxKind.WhileStatement);
            var ds = (LocalDeclarationStatementSyntax)ms.Body.Statements[0];
            ds.Declaration.Variables.Count.Should().Be(1);
            ds.Declaration.Variables[0].Initializer.Should().NotBeNull();
            ds.Declaration.Variables[0].Initializer.EqualsToken.Kind().Should().NotBe(SyntaxKind.None);
            ds.Declaration.Variables[0].Initializer.Value.Should().NotBeNull();
            ds.Declaration.Variables[0].Initializer.Value.Kind().Should().Be(SyntaxKind.AnonymousObjectCreationExpression);
            file.Errors().Length.Should().Be(3);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_InvalidExprTerm);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_RbraceExpected);
            file.Errors()[2].Code.Should().Be((int)ErrorCode.ERR_SemicolonExpected);
        }

        [Fact]
        public void TestStatementAfterAnonymousTypeMember()
        {
            var text = "class c { void m() { var x = new { a = b while (x) {} } }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(1);
            agg.Members[0].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            var ms = (MethodDeclarationSyntax)agg.Members[0];
            ms.Body.Should().NotBeNull();
            ms.Body.Statements.Count.Should().Be(2);
            ms.Body.Statements[0].Kind().Should().Be(SyntaxKind.LocalDeclarationStatement);
            ms.Body.Statements[1].Kind().Should().Be(SyntaxKind.WhileStatement);
            var ds = (LocalDeclarationStatementSyntax)ms.Body.Statements[0];
            ds.Declaration.Variables.Count.Should().Be(1);
            ds.Declaration.Variables[0].Initializer.Should().NotBeNull();
            ds.Declaration.Variables[0].Initializer.EqualsToken.Kind().Should().NotBe(SyntaxKind.None);
            ds.Declaration.Variables[0].Initializer.Value.Should().NotBeNull();
            ds.Declaration.Variables[0].Initializer.Value.Kind().Should().Be(SyntaxKind.AnonymousObjectCreationExpression);
            file.Errors().Length.Should().Be(2);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_RbraceExpected);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_SemicolonExpected);
        }

        [Fact]
        public void TestStatementAfterAnonymousTypeMemberComma()
        {
            var text = "class c { void m() { var x = new { a = b, while (x) {} } }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(1);
            agg.Members[0].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            var ms = (MethodDeclarationSyntax)agg.Members[0];
            ms.Body.Should().NotBeNull();
            ms.Body.Statements.Count.Should().Be(2);
            ms.Body.Statements[0].Kind().Should().Be(SyntaxKind.LocalDeclarationStatement);
            ms.Body.Statements[1].Kind().Should().Be(SyntaxKind.WhileStatement);
            var ds = (LocalDeclarationStatementSyntax)ms.Body.Statements[0];
            ds.Declaration.Variables.Count.Should().Be(1);
            ds.Declaration.Variables[0].Initializer.Should().NotBeNull();
            ds.Declaration.Variables[0].Initializer.EqualsToken.Kind().Should().NotBe(SyntaxKind.None);
            ds.Declaration.Variables[0].Initializer.Value.Should().NotBeNull();
            ds.Declaration.Variables[0].Initializer.Value.Kind().Should().Be(SyntaxKind.AnonymousObjectCreationExpression);
            file.Errors().Length.Should().Be(2);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_RbraceExpected);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_SemicolonExpected);
        }

        [Fact]
        public void TestGarbageAfterAnonymousTypeStart()
        {
            var text = "class c { void m() { var x = new { $ }; } }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(1);
            agg.Members[0].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            var ms = (MethodDeclarationSyntax)agg.Members[0];
            ms.Body.Should().NotBeNull();
            ms.Body.Statements.Count.Should().Be(1);
            ms.Body.Statements[0].Kind().Should().Be(SyntaxKind.LocalDeclarationStatement);
            var ds = (LocalDeclarationStatementSyntax)ms.Body.Statements[0];
            ds.Declaration.Variables.Count.Should().Be(1);
            ds.Declaration.Variables[0].Initializer.Should().NotBeNull();
            ds.Declaration.Variables[0].Initializer.EqualsToken.Kind().Should().NotBe(SyntaxKind.None);
            ds.Declaration.Variables[0].Initializer.Value.Should().NotBeNull();
            ds.Declaration.Variables[0].Initializer.Value.Kind().Should().Be(SyntaxKind.AnonymousObjectCreationExpression);
            file.Errors().Length.Should().Be(1);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_UnexpectedCharacter);
        }

        [Fact]
        public void TestGarbageBeforeAnonymousTypeMemberStart()
        {
            var text = "class c { void m() { var x = new { $ a }; } }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(1);
            agg.Members[0].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            var ms = (MethodDeclarationSyntax)agg.Members[0];
            ms.Body.Should().NotBeNull();
            ms.Body.Statements.Count.Should().Be(1);
            ms.Body.Statements[0].Kind().Should().Be(SyntaxKind.LocalDeclarationStatement);
            var ds = (LocalDeclarationStatementSyntax)ms.Body.Statements[0];
            ds.Declaration.Variables.Count.Should().Be(1);
            ds.Declaration.Variables[0].Initializer.Should().NotBeNull();
            ds.Declaration.Variables[0].Initializer.EqualsToken.Kind().Should().NotBe(SyntaxKind.None);
            ds.Declaration.Variables[0].Initializer.Value.Should().NotBeNull();
            ds.Declaration.Variables[0].Initializer.Value.Kind().Should().Be(SyntaxKind.AnonymousObjectCreationExpression);
            file.Errors().Length.Should().Be(1);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_UnexpectedCharacter);
        }

        [Fact]
        public void TestGarbageAfterAnonymousTypeMemberStart()
        {
            var text = "class c { void m() { var x = new { a $ }; } }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(1);
            agg.Members[0].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            var ms = (MethodDeclarationSyntax)agg.Members[0];
            ms.Body.Should().NotBeNull();
            ms.Body.Statements.Count.Should().Be(1);
            ms.Body.Statements[0].Kind().Should().Be(SyntaxKind.LocalDeclarationStatement);
            var ds = (LocalDeclarationStatementSyntax)ms.Body.Statements[0];
            ds.Declaration.Variables.Count.Should().Be(1);
            ds.Declaration.Variables[0].Initializer.Should().NotBeNull();
            ds.Declaration.Variables[0].Initializer.EqualsToken.Kind().Should().NotBe(SyntaxKind.None);
            ds.Declaration.Variables[0].Initializer.Value.Should().NotBeNull();
            ds.Declaration.Variables[0].Initializer.Value.Kind().Should().Be(SyntaxKind.AnonymousObjectCreationExpression);
            file.Errors().Length.Should().Be(1);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_UnexpectedCharacter);
        }

        [Fact]
        public void TestGarbageAfterAnonymousTypeMemberEquals()
        {
            var text = "class c { void m() { var x = new { a = $ }; } }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(1);
            agg.Members[0].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            var ms = (MethodDeclarationSyntax)agg.Members[0];
            ms.Body.Should().NotBeNull();
            ms.Body.Statements.Count.Should().Be(1);
            ms.Body.Statements[0].Kind().Should().Be(SyntaxKind.LocalDeclarationStatement);
            var ds = (LocalDeclarationStatementSyntax)ms.Body.Statements[0];
            ds.Declaration.Variables.Count.Should().Be(1);
            ds.Declaration.Variables[0].Initializer.Should().NotBeNull();
            ds.Declaration.Variables[0].Initializer.EqualsToken.Kind().Should().NotBe(SyntaxKind.None);
            ds.Declaration.Variables[0].Initializer.Value.Should().NotBeNull();
            ds.Declaration.Variables[0].Initializer.Value.Kind().Should().Be(SyntaxKind.AnonymousObjectCreationExpression);
            file.Errors().Length.Should().Be(2);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_InvalidExprTerm);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_UnexpectedCharacter);
        }

        [Fact]
        public void TestGarbageAfterAnonymousTypeMember()
        {
            var text = "class c { void m() { var x = new { a = b $ }; } }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(1);
            agg.Members[0].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            var ms = (MethodDeclarationSyntax)agg.Members[0];
            ms.Body.Should().NotBeNull();
            ms.Body.Statements.Count.Should().Be(1);
            ms.Body.Statements[0].Kind().Should().Be(SyntaxKind.LocalDeclarationStatement);
            var ds = (LocalDeclarationStatementSyntax)ms.Body.Statements[0];
            ds.Declaration.Variables.Count.Should().Be(1);
            ds.Declaration.Variables[0].Initializer.Should().NotBeNull();
            ds.Declaration.Variables[0].Initializer.EqualsToken.Kind().Should().NotBe(SyntaxKind.None);
            ds.Declaration.Variables[0].Initializer.Value.Should().NotBeNull();
            ds.Declaration.Variables[0].Initializer.Value.Kind().Should().Be(SyntaxKind.AnonymousObjectCreationExpression);
            file.Errors().Length.Should().Be(1);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_UnexpectedCharacter);
        }

        [Fact]
        public void TestGarbageAfterAnonymousTypeMemberComma()
        {
            var text = "class c { void m() { var x = new { a = b, $ }; } }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(1);
            agg.Members[0].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            var ms = (MethodDeclarationSyntax)agg.Members[0];
            ms.Body.Should().NotBeNull();
            ms.Body.Statements.Count.Should().Be(1);
            ms.Body.Statements[0].Kind().Should().Be(SyntaxKind.LocalDeclarationStatement);
            var ds = (LocalDeclarationStatementSyntax)ms.Body.Statements[0];
            ds.Declaration.Variables.Count.Should().Be(1);
            ds.Declaration.Variables[0].Initializer.Should().NotBeNull();
            ds.Declaration.Variables[0].Initializer.EqualsToken.Kind().Should().NotBe(SyntaxKind.None);
            ds.Declaration.Variables[0].Initializer.Value.Should().NotBeNull();
            ds.Declaration.Variables[0].Initializer.Value.Kind().Should().Be(SyntaxKind.AnonymousObjectCreationExpression);
            file.Errors().Length.Should().Be(1);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_UnexpectedCharacter);
        }

        [Fact]
        public void TestCloseBraceAfterObjectInitializerStart()
        {
            // empty object initializer is perfectly legal
            var text = "class c { void m() { var x = new C {}; } }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(1);
            agg.Members[0].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            var ms = (MethodDeclarationSyntax)agg.Members[0];
            ms.Body.Should().NotBeNull();
            ms.Body.Statements.Count.Should().Be(1);
            ms.Body.Statements[0].Kind().Should().Be(SyntaxKind.LocalDeclarationStatement);
            var ds = (LocalDeclarationStatementSyntax)ms.Body.Statements[0];
            ds.Declaration.Variables.Count.Should().Be(1);
            ds.Declaration.Variables[0].Initializer.Should().NotBeNull();
            ds.Declaration.Variables[0].Initializer.EqualsToken.Kind().Should().NotBe(SyntaxKind.None);
            ds.Declaration.Variables[0].Initializer.Value.Should().NotBeNull();
            ds.Declaration.Variables[0].Initializer.Value.Kind().Should().Be(SyntaxKind.ObjectCreationExpression);
            file.Errors().Length.Should().Be(0);
        }

        [Fact]
        public void TestSemicolonAfterObjectInitializerStart()
        {
            var text = "class c { void m() { var x = new C {; } }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(1);
            agg.Members[0].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            var ms = (MethodDeclarationSyntax)agg.Members[0];
            ms.Body.Should().NotBeNull();
            ms.Body.Statements.Count.Should().Be(1);
            ms.Body.Statements[0].Kind().Should().Be(SyntaxKind.LocalDeclarationStatement);
            var ds = (LocalDeclarationStatementSyntax)ms.Body.Statements[0];
            ds.Declaration.Variables.Count.Should().Be(1);
            ds.Declaration.Variables[0].Initializer.Should().NotBeNull();
            ds.Declaration.Variables[0].Initializer.EqualsToken.Kind().Should().NotBe(SyntaxKind.None);
            ds.Declaration.Variables[0].Initializer.Value.Should().NotBeNull();
            ds.Declaration.Variables[0].Initializer.Value.Kind().Should().Be(SyntaxKind.ObjectCreationExpression);
            file.Errors().Length.Should().Be(1);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_RbraceExpected);
        }

        [Fact]
        public void TestSemicolonAfterObjectInitializerMemberStart()
        {
            var text = "class c { void m() { var x = new C { a; } }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(1);
            agg.Members[0].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            var ms = (MethodDeclarationSyntax)agg.Members[0];
            ms.Body.Should().NotBeNull();
            ms.Body.Statements.Count.Should().Be(1);
            ms.Body.Statements[0].Kind().Should().Be(SyntaxKind.LocalDeclarationStatement);
            var ds = (LocalDeclarationStatementSyntax)ms.Body.Statements[0];
            ds.Declaration.Variables.Count.Should().Be(1);
            ds.Declaration.Variables[0].Initializer.Should().NotBeNull();
            ds.Declaration.Variables[0].Initializer.EqualsToken.Kind().Should().NotBe(SyntaxKind.None);
            ds.Declaration.Variables[0].Initializer.Value.Should().NotBeNull();
            ds.Declaration.Variables[0].Initializer.Value.Kind().Should().Be(SyntaxKind.ObjectCreationExpression);
            file.Errors().Length.Should().Be(1);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_RbraceExpected);
        }

        [Fact]
        public void TestSemicolonAfterObjectInitializerMemberEquals()
        {
            var text = "class c { void m() { var x = new C { a =; } }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(1);
            agg.Members[0].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            var ms = (MethodDeclarationSyntax)agg.Members[0];
            ms.Body.Should().NotBeNull();
            ms.Body.Statements.Count.Should().Be(1);
            ms.Body.Statements[0].Kind().Should().Be(SyntaxKind.LocalDeclarationStatement);
            var ds = (LocalDeclarationStatementSyntax)ms.Body.Statements[0];
            ds.Declaration.Variables.Count.Should().Be(1);
            ds.Declaration.Variables[0].Initializer.Should().NotBeNull();
            ds.Declaration.Variables[0].Initializer.EqualsToken.Kind().Should().NotBe(SyntaxKind.None);
            ds.Declaration.Variables[0].Initializer.Value.Should().NotBeNull();
            ds.Declaration.Variables[0].Initializer.Value.Kind().Should().Be(SyntaxKind.ObjectCreationExpression);
            file.Errors().Length.Should().Be(2);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_InvalidExprTerm);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_RbraceExpected);
        }

        [Fact]
        public void TestSemicolonAfterObjectInitializerMember()
        {
            var text = "class c { void m() { var x = new C { a = b; } }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(1);
            agg.Members[0].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            var ms = (MethodDeclarationSyntax)agg.Members[0];
            ms.Body.Should().NotBeNull();
            ms.Body.Statements.Count.Should().Be(1);
            ms.Body.Statements[0].Kind().Should().Be(SyntaxKind.LocalDeclarationStatement);
            var ds = (LocalDeclarationStatementSyntax)ms.Body.Statements[0];
            ds.Declaration.Variables.Count.Should().Be(1);
            ds.Declaration.Variables[0].Initializer.Should().NotBeNull();
            ds.Declaration.Variables[0].Initializer.EqualsToken.Kind().Should().NotBe(SyntaxKind.None);
            ds.Declaration.Variables[0].Initializer.Value.Should().NotBeNull();
            ds.Declaration.Variables[0].Initializer.Value.Kind().Should().Be(SyntaxKind.ObjectCreationExpression);
            file.Errors().Length.Should().Be(1);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_RbraceExpected);
        }

        [Fact]
        public void TestSemicolonAfterObjectInitializerMemberComma()
        {
            var text = "class c { void m() { var x = new C { a = b, ; } }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(1);
            agg.Members[0].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            var ms = (MethodDeclarationSyntax)agg.Members[0];
            ms.Body.Should().NotBeNull();
            ms.Body.Statements.Count.Should().Be(1);
            ms.Body.Statements[0].Kind().Should().Be(SyntaxKind.LocalDeclarationStatement);
            var ds = (LocalDeclarationStatementSyntax)ms.Body.Statements[0];
            ds.Declaration.Variables.Count.Should().Be(1);
            ds.Declaration.Variables[0].Initializer.Should().NotBeNull();
            ds.Declaration.Variables[0].Initializer.EqualsToken.Kind().Should().NotBe(SyntaxKind.None);
            ds.Declaration.Variables[0].Initializer.Value.Should().NotBeNull();
            ds.Declaration.Variables[0].Initializer.Value.Kind().Should().Be(SyntaxKind.ObjectCreationExpression);
            file.Errors().Length.Should().Be(2);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_InvalidExprTerm);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_RbraceExpected);
        }

        [Fact]
        public void TestStatementAfterObjectInitializerStart()
        {
            var text = "class c { void m() { var x = new C { while (x) {} } }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(1);
            agg.Members[0].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            var ms = (MethodDeclarationSyntax)agg.Members[0];
            ms.Body.Should().NotBeNull();
            ms.Body.Statements.Count.Should().Be(2);
            ms.Body.Statements[0].Kind().Should().Be(SyntaxKind.LocalDeclarationStatement);
            ms.Body.Statements[1].Kind().Should().Be(SyntaxKind.WhileStatement);
            var ds = (LocalDeclarationStatementSyntax)ms.Body.Statements[0];
            ds.Declaration.Variables.Count.Should().Be(1);
            ds.Declaration.Variables[0].Initializer.Should().NotBeNull();
            ds.Declaration.Variables[0].Initializer.EqualsToken.Kind().Should().NotBe(SyntaxKind.None);
            ds.Declaration.Variables[0].Initializer.Value.Should().NotBeNull();
            ds.Declaration.Variables[0].Initializer.Value.Kind().Should().Be(SyntaxKind.ObjectCreationExpression);
            file.Errors().Length.Should().Be(2);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_RbraceExpected);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_SemicolonExpected);
        }

        [Fact]
        public void TestStatementAfterObjectInitializerMemberStart()
        {
            var text = "class c { void m() { var x = new C { a while (x) {} } }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(1);
            agg.Members[0].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            var ms = (MethodDeclarationSyntax)agg.Members[0];
            ms.Body.Should().NotBeNull();
            ms.Body.Statements.Count.Should().Be(2);
            ms.Body.Statements[0].Kind().Should().Be(SyntaxKind.LocalDeclarationStatement);
            ms.Body.Statements[1].Kind().Should().Be(SyntaxKind.WhileStatement);
            var ds = (LocalDeclarationStatementSyntax)ms.Body.Statements[0];
            ds.Declaration.Variables.Count.Should().Be(1);
            ds.Declaration.Variables[0].Initializer.Should().NotBeNull();
            ds.Declaration.Variables[0].Initializer.EqualsToken.Kind().Should().NotBe(SyntaxKind.None);
            ds.Declaration.Variables[0].Initializer.Value.Should().NotBeNull();
            ds.Declaration.Variables[0].Initializer.Value.Kind().Should().Be(SyntaxKind.ObjectCreationExpression);
            file.Errors().Length.Should().Be(2);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_RbraceExpected);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_SemicolonExpected);
        }

        [Fact]
        public void TestStatementAfterObjectInitializerMemberEquals()
        {
            var text = "class c { void m() { var x = new C { a = while (x) {} } }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(1);
            agg.Members[0].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            var ms = (MethodDeclarationSyntax)agg.Members[0];
            ms.Body.Should().NotBeNull();
            ms.Body.Statements.Count.Should().Be(2);
            ms.Body.Statements[0].Kind().Should().Be(SyntaxKind.LocalDeclarationStatement);
            ms.Body.Statements[1].Kind().Should().Be(SyntaxKind.WhileStatement);
            var ds = (LocalDeclarationStatementSyntax)ms.Body.Statements[0];
            ds.Declaration.Variables.Count.Should().Be(1);
            ds.Declaration.Variables[0].Initializer.Should().NotBeNull();
            ds.Declaration.Variables[0].Initializer.EqualsToken.Kind().Should().NotBe(SyntaxKind.None);
            ds.Declaration.Variables[0].Initializer.Value.Should().NotBeNull();
            ds.Declaration.Variables[0].Initializer.Value.Kind().Should().Be(SyntaxKind.ObjectCreationExpression);
            file.Errors().Length.Should().Be(3);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_InvalidExprTerm);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_RbraceExpected);
            file.Errors()[2].Code.Should().Be((int)ErrorCode.ERR_SemicolonExpected);
        }

        [Fact]
        public void TestStatementAfterObjectInitializerMember()
        {
            var text = "class c { void m() { var x = new C { a = b while (x) {} } }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(1);
            agg.Members[0].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            var ms = (MethodDeclarationSyntax)agg.Members[0];
            ms.Body.Should().NotBeNull();
            ms.Body.Statements.Count.Should().Be(2);
            ms.Body.Statements[0].Kind().Should().Be(SyntaxKind.LocalDeclarationStatement);
            ms.Body.Statements[1].Kind().Should().Be(SyntaxKind.WhileStatement);
            var ds = (LocalDeclarationStatementSyntax)ms.Body.Statements[0];
            ds.Declaration.Variables.Count.Should().Be(1);
            ds.Declaration.Variables[0].Initializer.Should().NotBeNull();
            ds.Declaration.Variables[0].Initializer.EqualsToken.Kind().Should().NotBe(SyntaxKind.None);
            ds.Declaration.Variables[0].Initializer.Value.Should().NotBeNull();
            ds.Declaration.Variables[0].Initializer.Value.Kind().Should().Be(SyntaxKind.ObjectCreationExpression);
            file.Errors().Length.Should().Be(2);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_RbraceExpected);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_SemicolonExpected);
        }

        [Fact]
        public void TestStatementAfterObjectInitializerMemberComma()
        {
            var text = "class c { void m() { var x = new C { a = b, while (x) {} } }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(1);
            agg.Members[0].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            var ms = (MethodDeclarationSyntax)agg.Members[0];
            ms.Body.Should().NotBeNull();
            ms.Body.Statements.Count.Should().Be(2);
            ms.Body.Statements[0].Kind().Should().Be(SyntaxKind.LocalDeclarationStatement);
            ms.Body.Statements[1].Kind().Should().Be(SyntaxKind.WhileStatement);
            var ds = (LocalDeclarationStatementSyntax)ms.Body.Statements[0];
            ds.Declaration.Variables.Count.Should().Be(1);
            ds.Declaration.Variables[0].Initializer.Should().NotBeNull();
            ds.Declaration.Variables[0].Initializer.EqualsToken.Kind().Should().NotBe(SyntaxKind.None);
            ds.Declaration.Variables[0].Initializer.Value.Should().NotBeNull();
            ds.Declaration.Variables[0].Initializer.Value.Kind().Should().Be(SyntaxKind.ObjectCreationExpression);
            file.Errors().Length.Should().Be(3);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_InvalidExprTerm);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_RbraceExpected);
            file.Errors()[2].Code.Should().Be((int)ErrorCode.ERR_SemicolonExpected);
        }

        [Fact]
        public void TestGarbageAfterObjectInitializerStart()
        {
            var text = "class c { void m() { var x = new C { $ }; } }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(1);
            agg.Members[0].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            var ms = (MethodDeclarationSyntax)agg.Members[0];
            ms.Body.Should().NotBeNull();
            ms.Body.Statements.Count.Should().Be(1);
            ms.Body.Statements[0].Kind().Should().Be(SyntaxKind.LocalDeclarationStatement);
            var ds = (LocalDeclarationStatementSyntax)ms.Body.Statements[0];
            ds.Declaration.Variables.Count.Should().Be(1);
            ds.Declaration.Variables[0].Initializer.Should().NotBeNull();
            ds.Declaration.Variables[0].Initializer.EqualsToken.Kind().Should().NotBe(SyntaxKind.None);
            ds.Declaration.Variables[0].Initializer.Value.Should().NotBeNull();
            ds.Declaration.Variables[0].Initializer.Value.Kind().Should().Be(SyntaxKind.ObjectCreationExpression);
            file.Errors().Length.Should().Be(1);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_UnexpectedCharacter);
        }

        [Fact]
        public void TestGarbageBeforeObjectInitializerMemberStart()
        {
            var text = "class c { void m() { var x = new C { $ a }; } }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(1);
            agg.Members[0].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            var ms = (MethodDeclarationSyntax)agg.Members[0];
            ms.Body.Should().NotBeNull();
            ms.Body.Statements.Count.Should().Be(1);
            ms.Body.Statements[0].Kind().Should().Be(SyntaxKind.LocalDeclarationStatement);
            var ds = (LocalDeclarationStatementSyntax)ms.Body.Statements[0];
            ds.Declaration.Variables.Count.Should().Be(1);
            ds.Declaration.Variables[0].Initializer.Should().NotBeNull();
            ds.Declaration.Variables[0].Initializer.EqualsToken.Kind().Should().NotBe(SyntaxKind.None);
            ds.Declaration.Variables[0].Initializer.Value.Should().NotBeNull();
            ds.Declaration.Variables[0].Initializer.Value.Kind().Should().Be(SyntaxKind.ObjectCreationExpression);
            file.Errors().Length.Should().Be(1);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_UnexpectedCharacter);
        }

        [Fact]
        public void TestGarbageAfterObjectInitializerMemberStart()
        {
            var text = "class c { void m() { var x = new C { a $ }; } }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(1);
            agg.Members[0].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            var ms = (MethodDeclarationSyntax)agg.Members[0];
            ms.Body.Should().NotBeNull();
            ms.Body.Statements.Count.Should().Be(1);
            ms.Body.Statements[0].Kind().Should().Be(SyntaxKind.LocalDeclarationStatement);
            var ds = (LocalDeclarationStatementSyntax)ms.Body.Statements[0];
            ds.Declaration.Variables.Count.Should().Be(1);
            ds.Declaration.Variables[0].Initializer.Should().NotBeNull();
            ds.Declaration.Variables[0].Initializer.EqualsToken.Kind().Should().NotBe(SyntaxKind.None);
            ds.Declaration.Variables[0].Initializer.Value.Should().NotBeNull();
            ds.Declaration.Variables[0].Initializer.Value.Kind().Should().Be(SyntaxKind.ObjectCreationExpression);
            file.Errors().Length.Should().Be(1);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_UnexpectedCharacter);
        }

        [Fact]
        public void TestGarbageAfterObjectInitializerMemberEquals()
        {
            var text = "class c { void m() { var x = new C { a = $ }; } }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(1);
            agg.Members[0].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            var ms = (MethodDeclarationSyntax)agg.Members[0];
            ms.Body.Should().NotBeNull();
            ms.Body.Statements.Count.Should().Be(1);
            ms.Body.Statements[0].Kind().Should().Be(SyntaxKind.LocalDeclarationStatement);
            var ds = (LocalDeclarationStatementSyntax)ms.Body.Statements[0];
            ds.Declaration.Variables.Count.Should().Be(1);
            ds.Declaration.Variables[0].Initializer.Should().NotBeNull();
            ds.Declaration.Variables[0].Initializer.EqualsToken.Kind().Should().NotBe(SyntaxKind.None);
            ds.Declaration.Variables[0].Initializer.Value.Should().NotBeNull();
            ds.Declaration.Variables[0].Initializer.Value.Kind().Should().Be(SyntaxKind.ObjectCreationExpression);
            file.Errors().Length.Should().Be(2);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_InvalidExprTerm);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_UnexpectedCharacter);
        }

        [Fact]
        public void TestGarbageAfterObjectInitializerMember()
        {
            var text = "class c { void m() { var x = new C { a = b $ }; } }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(1);
            agg.Members[0].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            var ms = (MethodDeclarationSyntax)agg.Members[0];
            ms.Body.Should().NotBeNull();
            ms.Body.Statements.Count.Should().Be(1);
            ms.Body.Statements[0].Kind().Should().Be(SyntaxKind.LocalDeclarationStatement);
            var ds = (LocalDeclarationStatementSyntax)ms.Body.Statements[0];
            ds.Declaration.Variables.Count.Should().Be(1);
            ds.Declaration.Variables[0].Initializer.Should().NotBeNull();
            ds.Declaration.Variables[0].Initializer.EqualsToken.Kind().Should().NotBe(SyntaxKind.None);
            ds.Declaration.Variables[0].Initializer.Value.Should().NotBeNull();
            ds.Declaration.Variables[0].Initializer.Value.Kind().Should().Be(SyntaxKind.ObjectCreationExpression);
            file.Errors().Length.Should().Be(1);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_UnexpectedCharacter);
        }

        [Fact]
        public void TestGarbageAfterObjectInitializerMemberComma()
        {
            var text = "class c { void m() { var x = new C { a = b, $ }; } }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(1);
            agg.Members[0].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            var ms = (MethodDeclarationSyntax)agg.Members[0];
            ms.Body.Should().NotBeNull();
            ms.Body.Statements.Count.Should().Be(1);
            ms.Body.Statements[0].Kind().Should().Be(SyntaxKind.LocalDeclarationStatement);
            var ds = (LocalDeclarationStatementSyntax)ms.Body.Statements[0];
            ds.Declaration.Variables.Count.Should().Be(1);
            ds.Declaration.Variables[0].Initializer.Should().NotBeNull();
            ds.Declaration.Variables[0].Initializer.EqualsToken.Kind().Should().NotBe(SyntaxKind.None);
            ds.Declaration.Variables[0].Initializer.Value.Should().NotBeNull();
            ds.Declaration.Variables[0].Initializer.Value.Kind().Should().Be(SyntaxKind.ObjectCreationExpression);
            file.Errors().Length.Should().Be(2);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_InvalidExprTerm);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_UnexpectedCharacter);
        }

        [Fact]
        public void TestSemicolonAfterLambdaParameter()
        {
            var text = "class c { void m() { var x = (Y y, ; } }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(1);
            agg.Members[0].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            var ms = (MethodDeclarationSyntax)agg.Members[0];
            ms.Body.Should().NotBeNull();
            ms.Body.Statements.Count.Should().Be(1);
            ms.Body.Statements[0].Kind().Should().Be(SyntaxKind.LocalDeclarationStatement);
            var ds = (LocalDeclarationStatementSyntax)ms.Body.Statements[0];
            ds.Declaration.Variables.Count.Should().Be(1);
            ds.Declaration.Variables[0].Initializer.Should().NotBeNull();
            ds.Declaration.Variables[0].Initializer.EqualsToken.Kind().Should().NotBe(SyntaxKind.None);
            ds.Declaration.Variables[0].Initializer.Value.Should().NotBeNull();
            ds.Declaration.Variables[0].Initializer.Value.Kind().Should().Be(SyntaxKind.TupleExpression);
            file.Errors().Verify(
                // error CS1525: Invalid expression term ';'
                Diagnostic(ErrorCode.ERR_InvalidExprTerm).WithArguments(";").WithLocation(1, 1),
                // error CS1026: ) expected
                Diagnostic(ErrorCode.ERR_CloseParenExpected).WithLocation(1, 1)
                );
        }

        [Fact]
        public void TestSemicolonAfterUntypedLambdaParameter()
        {
            var text = "class c { void m() { var x = (y, ; } }";
            var file = this.ParseTree(text, options: TestOptions.Regular);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(1);
            agg.Members[0].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            var ms = (MethodDeclarationSyntax)agg.Members[0];
            ms.Body.Should().NotBeNull();
            ms.Body.Statements.Count.Should().Be(1);
            ms.Body.Statements[0].Kind().Should().Be(SyntaxKind.LocalDeclarationStatement);
            var ds = (LocalDeclarationStatementSyntax)ms.Body.Statements[0];
            ds.Declaration.Variables.Count.Should().Be(1);
            ds.Declaration.Variables[0].Initializer.Should().NotBeNull();
            ds.Declaration.Variables[0].Initializer.EqualsToken.Kind().Should().NotBe(SyntaxKind.None);
            ds.Declaration.Variables[0].Initializer.Value.Should().NotBeNull();
            ds.Declaration.Variables[0].Initializer.Value.Kind().Should().Be(SyntaxKind.TupleExpression);
            file.Errors().Length.Should().Be(2);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_InvalidExprTerm);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_CloseParenExpected);
        }

        [Fact]
        public void TestSemicolonAfterUntypedLambdaParameterWithCSharp6()
        {
            var text = "class c { void m() { var x = (y, ; } }";
            var file = this.ParseTree(text, TestOptions.Regular.WithLanguageVersion(LanguageVersion.CSharp6));

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(1);
            agg.Members[0].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            var ms = (MethodDeclarationSyntax)agg.Members[0];
            ms.Body.Should().NotBeNull();
            ms.Body.Statements.Count.Should().Be(1);
            ms.Body.Statements[0].Kind().Should().Be(SyntaxKind.LocalDeclarationStatement);
            var ds = (LocalDeclarationStatementSyntax)ms.Body.Statements[0];
            ds.Declaration.Variables.Count.Should().Be(1);
            ds.Declaration.Variables[0].Initializer.Should().NotBeNull();
            ds.Declaration.Variables[0].Initializer.EqualsToken.Kind().Should().NotBe(SyntaxKind.None);
            ds.Declaration.Variables[0].Initializer.Value.Should().NotBeNull();
            ds.Declaration.Variables[0].Initializer.Value.Kind().Should().Be(SyntaxKind.TupleExpression);

            (int)ErrorCode.ERR_CloseParenExpected
                            }.Should().Be(new[] {
                                (int)ErrorCode.ERR_InvalidExprTerm, file.Errors().Select(e => e.Code));

            CreateCompilation(text, parseOptions: TestOptions.Regular6).VerifyDiagnostics(
                // (1,7): warning CS8981: The type name 'c' only contains lower-cased ascii characters. Such names may become reserved for the language.
                // class c { void m() { var x = (y, ; } }
                Diagnostic(ErrorCode.WRN_LowerCaseTypeName, "c").WithArguments("c").WithLocation(1, 7),
                // (1,30): error CS8059: Feature 'tuples' is not available in C# 6. Please use language version 7.0 or greater.
                // class c { void m() { var x = (y, ; } }
                Diagnostic(ErrorCode.ERR_FeatureNotAvailableInVersion6, "(y, ").WithArguments("tuples", "7.0").WithLocation(1, 30),
                // (1,31): error CS0103: The name 'y' does not exist in the current context
                // class c { void m() { var x = (y, ; } }
                Diagnostic(ErrorCode.ERR_NameNotInContext, "y").WithArguments("y").WithLocation(1, 31),
                // (1,34): error CS1525: Invalid expression term ';'
                // class c { void m() { var x = (y, ; } }
                Diagnostic(ErrorCode.ERR_InvalidExprTerm, ";").WithArguments(";").WithLocation(1, 34),
                // (1,34): error CS1026: ) expected
                // class c { void m() { var x = (y, ; } }
                Diagnostic(ErrorCode.ERR_CloseParenExpected, ";").WithLocation(1, 34));
        }

        [Fact]
        public void TestStatementAfterLambdaParameter()
        {
            var text = "class c { void m() { var x = (Y y, while (c) { } } }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(1);
            agg.Members[0].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            var ms = (MethodDeclarationSyntax)agg.Members[0];
            ms.Body.Should().NotBeNull();
            ms.Body.Statements.Count.Should().Be(2);
            ms.Body.Statements[0].Kind().Should().Be(SyntaxKind.LocalDeclarationStatement);
            ms.Body.Statements[1].Kind().Should().Be(SyntaxKind.WhileStatement);
            var ds = (LocalDeclarationStatementSyntax)ms.Body.Statements[0];
            ds.Declaration.Variables.Count.Should().Be(1);
            ds.Declaration.Variables[0].Initializer.Should().NotBeNull();
            ds.Declaration.Variables[0].Initializer.EqualsToken.Kind().Should().NotBe(SyntaxKind.None);
            ds.Declaration.Variables[0].Initializer.Value.Should().NotBeNull();
            ds.Declaration.Variables[0].Initializer.Value.Kind().Should().Be(SyntaxKind.TupleExpression);
            file.Errors().Verify(
                // error CS1525: Invalid expression term 'while'
                Diagnostic(ErrorCode.ERR_InvalidExprTerm).WithArguments("while").WithLocation(1, 1),
                // error CS1026: ) expected
                Diagnostic(ErrorCode.ERR_CloseParenExpected).WithLocation(1, 1),
                // error CS1002: ; expected
                Diagnostic(ErrorCode.ERR_SemicolonExpected).WithLocation(1, 1)
                );
        }

        [Fact]
        public void TestStatementAfterUntypedLambdaParameter()
        {
            var text = "class c { void m() { var x = (y, while (c) { } } }";
            var file = this.ParseTree(text, options: TestOptions.Regular);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(1);
            agg.Members[0].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            var ms = (MethodDeclarationSyntax)agg.Members[0];
            ms.Body.Should().NotBeNull();
            ms.Body.Statements.Count.Should().Be(2);
            ms.Body.Statements[0].Kind().Should().Be(SyntaxKind.LocalDeclarationStatement);
            ms.Body.Statements[1].Kind().Should().Be(SyntaxKind.WhileStatement);
            var ds = (LocalDeclarationStatementSyntax)ms.Body.Statements[0];
            ds.Declaration.Variables.Count.Should().Be(1);
            ds.Declaration.Variables[0].Initializer.Should().NotBeNull();
            ds.Declaration.Variables[0].Initializer.EqualsToken.Kind().Should().NotBe(SyntaxKind.None);
            ds.Declaration.Variables[0].Initializer.Value.Should().NotBeNull();
            ds.Declaration.Variables[0].Initializer.Value.Kind().Should().Be(SyntaxKind.TupleExpression);
            file.Errors().Length.Should().Be(3);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_InvalidExprTerm);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_CloseParenExpected);
            file.Errors()[2].Code.Should().Be((int)ErrorCode.ERR_SemicolonExpected);
        }

        [Fact]
        public void TestStatementAfterUntypedLambdaParameterWithCSharp6()
        {
            var text = "class c { void m() { var x = (y, while (c) { } } }";
            var file = this.ParseTree(text, options: TestOptions.Regular.WithLanguageVersion(LanguageVersion.CSharp6));

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(1);
            agg.Members[0].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            var ms = (MethodDeclarationSyntax)agg.Members[0];
            ms.Body.Should().NotBeNull();
            ms.Body.Statements.Count.Should().Be(2);
            ms.Body.Statements[0].Kind().Should().Be(SyntaxKind.LocalDeclarationStatement);
            ms.Body.Statements[1].Kind().Should().Be(SyntaxKind.WhileStatement);

            var ds = (LocalDeclarationStatementSyntax)ms.Body.Statements[0];
            ds.ToFullString().Should().Be("var x = (y, ");
            ds.Declaration.Variables.Count.Should().Be(1);
            ds.Declaration.Variables[0].Initializer.Should().NotBeNull();
            ds.Declaration.Variables[0].Initializer.EqualsToken.Kind().Should().NotBe(SyntaxKind.None);
            ds.Declaration.Variables[0].Initializer.Value.Should().NotBeNull();
            ds.Declaration.Variables[0].Initializer.Value.Kind().Should().Be(SyntaxKind.TupleExpression);

            file.Errors().Select(e => e.Code).Should().Equal(new[] {
                                (int)ErrorCode.ERR_InvalidExprTerm,
                                (int)ErrorCode.ERR_CloseParenExpected,
                                (int)ErrorCode.ERR_SemicolonExpected
                            });

            CreateCompilation(text, parseOptions: TestOptions.Regular6).VerifyDiagnostics(
                // (1,7): warning CS8981: The type name 'c' only contains lower-cased ascii characters. Such names may become reserved for the language.
                // class c { void m() { var x = (y, while (c) { } } }
                Diagnostic(ErrorCode.WRN_LowerCaseTypeName, "c").WithArguments("c").WithLocation(1, 7),
                // (1,30): error CS8059: Feature 'tuples' is not available in C# 6. Please use language version 7.0 or greater.
                // class c { void m() { var x = (y, while (c) { } } }
                Diagnostic(ErrorCode.ERR_FeatureNotAvailableInVersion6, "(y, ").WithArguments("tuples", "7.0").WithLocation(1, 30),
                // (1,31): error CS0103: The name 'y' does not exist in the current context
                // class c { void m() { var x = (y, while (c) { } } }
                Diagnostic(ErrorCode.ERR_NameNotInContext, "y").WithArguments("y").WithLocation(1, 31),
                // (1,34): error CS1525: Invalid expression term 'while'
                // class c { void m() { var x = (y, while (c) { } } }
                Diagnostic(ErrorCode.ERR_InvalidExprTerm, "while").WithArguments("while").WithLocation(1, 34),
                // (1,34): error CS1026: ) expected
                // class c { void m() { var x = (y, while (c) { } } }
                Diagnostic(ErrorCode.ERR_CloseParenExpected, "while").WithLocation(1, 34),
                // (1,34): error CS1002: ; expected
                // class c { void m() { var x = (y, while (c) { } } }
                Diagnostic(ErrorCode.ERR_SemicolonExpected, "while").WithLocation(1, 34),
                // (1,41): error CS0119: 'c' is a type, which is not valid in the given context
                // class c { void m() { var x = (y, while (c) { } } }
                Diagnostic(ErrorCode.ERR_BadSKunknown, "c").WithArguments("c", "type").WithLocation(1, 41));
        }

        [Fact]
        public void TestPropertyWithNoAccessors()
        {
            // this is syntactically valid (even though it will produce a binding error)
            var text = "class c { int p { } }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(1);
            agg.Members[0].Kind().Should().Be(SyntaxKind.PropertyDeclaration);
            var pd = (PropertyDeclarationSyntax)agg.Members[0];
            pd.AccessorList.Should().NotBeNull();
            pd.AccessorList.OpenBraceToken.Should().NotBe(default);
            pd.AccessorList.OpenBraceToken.IsMissing.Should().BeFalse();
            pd.AccessorList.CloseBraceToken.Should().NotBe(default);
            pd.AccessorList.CloseBraceToken.IsMissing.Should().BeFalse();
            pd.AccessorList.Accessors.Count.Should().Be(0);
            file.Errors().Length.Should().Be(0);
        }

        [Fact]
        public void TestMethodAfterPropertyStart()
        {
            // this is syntactically valid (even though it will produce a binding error)
            var text = "class c { int p { int M() {} }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(2);
            agg.Members[0].Kind().Should().Be(SyntaxKind.PropertyDeclaration);
            agg.Members[1].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            var pd = (PropertyDeclarationSyntax)agg.Members[0];
            pd.AccessorList.Should().NotBeNull();
            pd.AccessorList.OpenBraceToken.Should().NotBe(default);
            pd.AccessorList.OpenBraceToken.IsMissing.Should().BeFalse();
            pd.AccessorList.CloseBraceToken.Should().NotBe(default);
            pd.AccessorList.CloseBraceToken.IsMissing.Should().BeTrue();
            pd.AccessorList.Accessors.Count.Should().Be(0);
            file.Errors().Length.Should().Be(1);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_RbraceExpected);
        }

        [Fact]
        public void TestMethodAfterPropertyGet()
        {
            var text = "class c { int p { get int M() {} }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(2);
            agg.Members[0].Kind().Should().Be(SyntaxKind.PropertyDeclaration);
            agg.Members[1].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            var pd = (PropertyDeclarationSyntax)agg.Members[0];
            pd.AccessorList.Should().NotBeNull();
            pd.AccessorList.OpenBraceToken.Should().NotBe(default);
            pd.AccessorList.OpenBraceToken.IsMissing.Should().BeFalse();
            pd.AccessorList.CloseBraceToken.Should().NotBe(default);
            pd.AccessorList.CloseBraceToken.IsMissing.Should().BeTrue();
            pd.AccessorList.Accessors.Count.Should().Be(1);
            var acc = pd.AccessorList.Accessors[0];
            acc.Kind().Should().Be(SyntaxKind.GetAccessorDeclaration);
            acc.Keyword.Should().NotBe(default);
            acc.Keyword.IsMissing.Should().BeFalse();
            acc.Keyword.Kind().Should().Be(SyntaxKind.GetKeyword);
            acc.Body.Should().BeNull();
            acc.SemicolonToken.Should().NotBe(default);
            acc.SemicolonToken.IsMissing.Should().BeTrue();

            file.Errors().Length.Should().Be(2);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_SemiOrLBraceOrArrowExpected);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_RbraceExpected);
        }

        [Fact]
        public void TestClassAfterPropertyGetBrace()
        {
            var text = "class c { int p { get { class d {} }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(2);
            agg.Members[0].Kind().Should().Be(SyntaxKind.PropertyDeclaration);
            agg.Members[1].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var pd = (PropertyDeclarationSyntax)agg.Members[0];
            pd.AccessorList.Should().NotBeNull();
            pd.AccessorList.OpenBraceToken.Should().NotBe(default);
            pd.AccessorList.OpenBraceToken.IsMissing.Should().BeFalse();
            pd.AccessorList.CloseBraceToken.Should().NotBe(default);
            pd.AccessorList.CloseBraceToken.IsMissing.Should().BeTrue();
            pd.AccessorList.Accessors.Count.Should().Be(1);
            var acc = pd.AccessorList.Accessors[0];
            acc.Kind().Should().Be(SyntaxKind.GetAccessorDeclaration);
            acc.Keyword.Should().NotBe(default);
            acc.Keyword.IsMissing.Should().BeFalse();
            acc.Keyword.Kind().Should().Be(SyntaxKind.GetKeyword);
            acc.Body.Should().NotBeNull();
            acc.Body.OpenBraceToken.Should().NotBe(default);
            acc.Body.OpenBraceToken.IsMissing.Should().BeFalse();
            acc.Body.Statements.Count.Should().Be(0);
            acc.Body.CloseBraceToken.Should().NotBe(default);
            acc.Body.CloseBraceToken.IsMissing.Should().BeTrue();
            acc.SemicolonToken.Kind().Should().Be(SyntaxKind.None);

            file.Errors().Length.Should().Be(2);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_RbraceExpected);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_RbraceExpected);
        }

        [Fact]
        public void TestModifiedMemberAfterPropertyGetBrace()
        {
            var text = "class c { int p { get { public class d {} }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(2);
            agg.Members[0].Kind().Should().Be(SyntaxKind.PropertyDeclaration);
            agg.Members[1].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var pd = (PropertyDeclarationSyntax)agg.Members[0];
            pd.AccessorList.Should().NotBeNull();
            pd.AccessorList.OpenBraceToken.Should().NotBe(default);
            pd.AccessorList.OpenBraceToken.IsMissing.Should().BeFalse();
            pd.AccessorList.CloseBraceToken.Should().NotBe(default);
            pd.AccessorList.CloseBraceToken.IsMissing.Should().BeTrue();
            pd.AccessorList.Accessors.Count.Should().Be(1);
            var acc = pd.AccessorList.Accessors[0];
            acc.Kind().Should().Be(SyntaxKind.GetAccessorDeclaration);
            acc.Keyword.Should().NotBe(default);
            acc.Keyword.IsMissing.Should().BeFalse();
            acc.Keyword.Kind().Should().Be(SyntaxKind.GetKeyword);
            acc.Body.Should().NotBeNull();
            acc.Body.OpenBraceToken.Should().NotBe(default);
            acc.Body.OpenBraceToken.IsMissing.Should().BeFalse();
            acc.Body.Statements.Count.Should().Be(0);
            acc.Body.CloseBraceToken.Should().NotBe(default);
            acc.Body.CloseBraceToken.IsMissing.Should().BeTrue();
            acc.SemicolonToken.Kind().Should().Be(SyntaxKind.None);

            file.Errors().Length.Should().Be(2);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_RbraceExpected);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_RbraceExpected);
        }

        [Fact]
        public void TestPropertyAccessorMissingOpenBrace()
        {
            var text = "class c { int p { get return 0; } } }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);

            var classDecl = (TypeDeclarationSyntax)file.Members[0];
            var propertyDecl = (PropertyDeclarationSyntax)classDecl.Members[0];

            var accessorDecls = propertyDecl.AccessorList.Accessors;
            accessorDecls.Count.Should().Be(1);

            var getDecl = accessorDecls[0];
            getDecl.Keyword.Kind().Should().Be(SyntaxKind.GetKeyword);

            var getBodyDecl = getDecl.Body;
            getBodyDecl.Should().NotBeNull();
            getBodyDecl.OpenBraceToken.IsMissing.Should().BeTrue();

            var getBodyStmts = getBodyDecl.Statements;
            getBodyStmts.Count.Should().Be(1);
            getBodyStmts[0].GetFirstToken().Kind().Should().Be(SyntaxKind.ReturnKeyword);
            getBodyStmts[0].ContainsDiagnostics.Should().BeFalse();

            file.Errors().Length.Should().Be(1);
            (ErrorCode)file.Errors()[0].Code.Should().Be(ErrorCode.ERR_SemiOrLBraceOrArrowExpected);
        }

        [Fact]
        public void TestPropertyAccessorsWithoutBodiesOrSemicolons()
        {
            var text = "class c { int p { get set } }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);

            var classDecl = (TypeDeclarationSyntax)file.Members[0];
            var propertyDecl = (PropertyDeclarationSyntax)classDecl.Members[0];

            var accessorDecls = propertyDecl.AccessorList.Accessors;
            accessorDecls.Count.Should().Be(2);

            var getDecl = accessorDecls[0];
            getDecl.Keyword.Kind().Should().Be(SyntaxKind.GetKeyword);
            getDecl.Body.Should().BeNull();
            getDecl.SemicolonToken.IsMissing.Should().BeTrue();

            var setDecl = accessorDecls[1];
            setDecl.Keyword.Kind().Should().Be(SyntaxKind.SetKeyword);
            setDecl.Body.Should().BeNull();
            setDecl.SemicolonToken.IsMissing.Should().BeTrue();

            file.Errors().Length.Should().Be(2);
            (ErrorCode)file.Errors()[0].Code.Should().Be(ErrorCode.ERR_SemiOrLBraceOrArrowExpected);
            (ErrorCode)file.Errors()[1].Code.Should().Be(ErrorCode.ERR_SemiOrLBraceOrArrowExpected);
        }

        [Fact]
        public void TestSemicolonAfterOrderingStart()
        {
            var text = "class c { void m() { var q = from x in y orderby; } }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(1);
            agg.Members[0].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            var md = (MethodDeclarationSyntax)agg.Members[0];

            md.Body.Should().NotBeNull();
            md.Body.OpenBraceToken.Should().NotBe(default);
            md.Body.OpenBraceToken.IsMissing.Should().BeFalse();
            md.Body.CloseBraceToken.Should().NotBe(default);
            md.Body.CloseBraceToken.IsMissing.Should().BeFalse();
            md.Body.Statements.Count.Should().Be(1);
            md.Body.Statements[0].Kind().Should().Be(SyntaxKind.LocalDeclarationStatement);
            var ds = (LocalDeclarationStatementSyntax)md.Body.Statements[0];
            ds.Declaration.Variables.Count.Should().Be(1);
            ds.Declaration.Variables[0].Initializer.Should().NotBeNull();
            ds.Declaration.Variables[0].Initializer.Value.Should().NotBeNull();
            ds.Declaration.Variables[0].Initializer.Value.Kind().Should().Be(SyntaxKind.QueryExpression);
            var qx = (QueryExpressionSyntax)ds.Declaration.Variables[0].Initializer.Value;
            qx.Body.Clauses.Count.Should().Be(1);
            qx.FromClause.Kind().Should().Be(SyntaxKind.FromClause);
            qx.Body.Clauses[0].Kind().Should().Be(SyntaxKind.OrderByClause);
            var oc = (OrderByClauseSyntax)qx.Body.Clauses[0];
            oc.OrderByKeyword.Should().NotBe(default);
            oc.OrderByKeyword.IsMissing.Should().BeFalse();
            oc.Orderings.Count.Should().Be(1);
            oc.Orderings[0].Expression.Should().NotBeNull();
            oc.Orderings[0].Expression.Kind().Should().Be(SyntaxKind.IdentifierName);
            var nm = (IdentifierNameSyntax)oc.Orderings[0].Expression;
            nm.IsMissing.Should().BeTrue();

            file.Errors().Length.Should().Be(2);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_InvalidExprTerm);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_ExpectedSelectOrGroup);
        }

        [Fact]
        public void TestSemicolonAfterOrderingExpression()
        {
            var text = "class c { void m() { var q = from x in y orderby e; } }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(1);
            agg.Members[0].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            var md = (MethodDeclarationSyntax)agg.Members[0];

            md.Body.Should().NotBeNull();
            md.Body.OpenBraceToken.Should().NotBe(default);
            md.Body.OpenBraceToken.IsMissing.Should().BeFalse();
            md.Body.CloseBraceToken.Should().NotBe(default);
            md.Body.CloseBraceToken.IsMissing.Should().BeFalse();
            md.Body.Statements.Count.Should().Be(1);
            md.Body.Statements[0].Kind().Should().Be(SyntaxKind.LocalDeclarationStatement);
            var ds = (LocalDeclarationStatementSyntax)md.Body.Statements[0];
            ds.Declaration.Variables.Count.Should().Be(1);
            ds.Declaration.Variables[0].Initializer.Should().NotBeNull();
            ds.Declaration.Variables[0].Initializer.Value.Should().NotBeNull();
            ds.Declaration.Variables[0].Initializer.Value.Kind().Should().Be(SyntaxKind.QueryExpression);
            var qx = (QueryExpressionSyntax)ds.Declaration.Variables[0].Initializer.Value;
            qx.Body.Clauses.Count.Should().Be(1);
            qx.FromClause.Kind().Should().Be(SyntaxKind.FromClause);
            qx.Body.Clauses[0].Kind().Should().Be(SyntaxKind.OrderByClause);
            var oc = (OrderByClauseSyntax)qx.Body.Clauses[0];
            oc.OrderByKeyword.Should().NotBe(default);
            oc.OrderByKeyword.IsMissing.Should().BeFalse();
            oc.Orderings.Count.Should().Be(1);
            oc.Orderings[0].Expression.Should().NotBeNull();
            oc.Orderings[0].Expression.Kind().Should().Be(SyntaxKind.IdentifierName);
            var nm = (IdentifierNameSyntax)oc.Orderings[0].Expression;
            nm.IsMissing.Should().BeFalse();

            file.Errors().Length.Should().Be(1);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_ExpectedSelectOrGroup);
        }

        [Fact]
        public void TestSemicolonAfterOrderingExpressionAndComma()
        {
            var text = "class c { void m() { var q = from x in y orderby e, ; } }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(1);
            agg.Members[0].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            var md = (MethodDeclarationSyntax)agg.Members[0];

            md.Body.Should().NotBeNull();
            md.Body.OpenBraceToken.Should().NotBe(default);
            md.Body.OpenBraceToken.IsMissing.Should().BeFalse();
            md.Body.CloseBraceToken.Should().NotBe(default);
            md.Body.CloseBraceToken.IsMissing.Should().BeFalse();
            md.Body.Statements.Count.Should().Be(1);
            md.Body.Statements[0].Kind().Should().Be(SyntaxKind.LocalDeclarationStatement);
            var ds = (LocalDeclarationStatementSyntax)md.Body.Statements[0];
            ds.Declaration.Variables.Count.Should().Be(1);
            ds.Declaration.Variables[0].Initializer.Should().NotBeNull();
            ds.Declaration.Variables[0].Initializer.Value.Should().NotBeNull();
            ds.Declaration.Variables[0].Initializer.Value.Kind().Should().Be(SyntaxKind.QueryExpression);
            var qx = (QueryExpressionSyntax)ds.Declaration.Variables[0].Initializer.Value;
            qx.Body.Clauses.Count.Should().Be(1);
            qx.FromClause.Kind().Should().Be(SyntaxKind.FromClause);
            qx.Body.Clauses[0].Kind().Should().Be(SyntaxKind.OrderByClause);
            var oc = (OrderByClauseSyntax)qx.Body.Clauses[0];
            oc.OrderByKeyword.Should().NotBe(default);
            oc.OrderByKeyword.IsMissing.Should().BeFalse();
            oc.Orderings.Count.Should().Be(2);
            oc.Orderings[0].Expression.Should().NotBeNull();
            oc.Orderings[0].Expression.Kind().Should().Be(SyntaxKind.IdentifierName);
            var nm = (IdentifierNameSyntax)oc.Orderings[0].Expression;
            nm.IsMissing.Should().BeFalse();
            oc.Orderings[1].Expression.Should().NotBeNull();
            oc.Orderings[0].Expression.Kind().Should().Be(SyntaxKind.IdentifierName);
            nm = (IdentifierNameSyntax)oc.Orderings[1].Expression;
            nm.IsMissing.Should().BeTrue();

            file.Errors().Length.Should().Be(2);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_InvalidExprTerm);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_ExpectedSelectOrGroup);
        }

        [Fact]
        public void TestMemberAfterOrderingStart()
        {
            var text = "class c { void m() { var q = from x in y orderby public int Goo; }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(2);
            agg.Members[0].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            agg.Members[1].Kind().Should().Be(SyntaxKind.FieldDeclaration);
            var md = (MethodDeclarationSyntax)agg.Members[0];

            md.Body.Should().NotBeNull();
            md.Body.OpenBraceToken.Should().NotBe(default);
            md.Body.OpenBraceToken.IsMissing.Should().BeFalse();
            md.Body.CloseBraceToken.Should().NotBe(default);
            md.Body.CloseBraceToken.IsMissing.Should().BeTrue();
            md.Body.Statements.Count.Should().Be(1);
            md.Body.Statements[0].Kind().Should().Be(SyntaxKind.LocalDeclarationStatement);
            var ds = (LocalDeclarationStatementSyntax)md.Body.Statements[0];
            ds.Declaration.Variables.Count.Should().Be(1);
            ds.Declaration.Variables[0].Initializer.Should().NotBeNull();
            ds.Declaration.Variables[0].Initializer.Value.Should().NotBeNull();
            ds.Declaration.Variables[0].Initializer.Value.Kind().Should().Be(SyntaxKind.QueryExpression);
            var qx = (QueryExpressionSyntax)ds.Declaration.Variables[0].Initializer.Value;
            qx.Body.Clauses.Count.Should().Be(1);
            qx.FromClause.Kind().Should().Be(SyntaxKind.FromClause);
            qx.Body.Clauses[0].Kind().Should().Be(SyntaxKind.OrderByClause);
            var oc = (OrderByClauseSyntax)qx.Body.Clauses[0];
            oc.OrderByKeyword.Should().NotBe(default);
            oc.OrderByKeyword.IsMissing.Should().BeFalse();
            oc.Orderings.Count.Should().Be(1);
            oc.Orderings[0].Expression.Should().NotBeNull();
            oc.Orderings[0].Expression.Kind().Should().Be(SyntaxKind.IdentifierName);
            var nm = (IdentifierNameSyntax)oc.Orderings[0].Expression;
            nm.IsMissing.Should().BeTrue();

            file.Errors().Length.Should().Be(4);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_InvalidExprTerm);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_ExpectedSelectOrGroup);
            file.Errors()[2].Code.Should().Be((int)ErrorCode.ERR_SemicolonExpected);
            file.Errors()[3].Code.Should().Be((int)ErrorCode.ERR_RbraceExpected);
        }

        [Fact]
        public void TestMemberAfterOrderingExpression()
        {
            var text = "class c { void m() { var q = from x in y orderby e public int Goo; }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(2);
            agg.Members[0].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            agg.Members[1].Kind().Should().Be(SyntaxKind.FieldDeclaration);
            var md = (MethodDeclarationSyntax)agg.Members[0];

            md.Body.Should().NotBeNull();
            md.Body.OpenBraceToken.Should().NotBe(default);
            md.Body.OpenBraceToken.IsMissing.Should().BeFalse();
            md.Body.CloseBraceToken.Should().NotBe(default);
            md.Body.CloseBraceToken.IsMissing.Should().BeTrue();
            md.Body.Statements.Count.Should().Be(1);
            md.Body.Statements[0].Kind().Should().Be(SyntaxKind.LocalDeclarationStatement);
            var ds = (LocalDeclarationStatementSyntax)md.Body.Statements[0];
            ds.Declaration.Variables.Count.Should().Be(1);
            ds.Declaration.Variables[0].Initializer.Should().NotBeNull();
            ds.Declaration.Variables[0].Initializer.Value.Should().NotBeNull();
            ds.Declaration.Variables[0].Initializer.Value.Kind().Should().Be(SyntaxKind.QueryExpression);
            var qx = (QueryExpressionSyntax)ds.Declaration.Variables[0].Initializer.Value;
            qx.Body.Clauses.Count.Should().Be(1);
            qx.FromClause.Kind().Should().Be(SyntaxKind.FromClause);
            qx.Body.Clauses[0].Kind().Should().Be(SyntaxKind.OrderByClause);
            var oc = (OrderByClauseSyntax)qx.Body.Clauses[0];
            oc.OrderByKeyword.Should().NotBe(default);
            oc.OrderByKeyword.IsMissing.Should().BeFalse();
            oc.Orderings.Count.Should().Be(1);
            oc.Orderings[0].Expression.Should().NotBeNull();
            oc.Orderings[0].Expression.Kind().Should().Be(SyntaxKind.IdentifierName);
            var nm = (IdentifierNameSyntax)oc.Orderings[0].Expression;
            nm.IsMissing.Should().BeFalse();

            file.Errors().Length.Should().Be(3);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_ExpectedSelectOrGroup);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_SemicolonExpected);
            file.Errors()[2].Code.Should().Be((int)ErrorCode.ERR_RbraceExpected);
        }

        [Fact]
        public void TestMemberAfterOrderingExpressionAndComma()
        {
            var text = "class c { void m() { var q = from x in y orderby e, public int Goo; }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var agg = (TypeDeclarationSyntax)file.Members[0];
            agg.Members.Count.Should().Be(2);
            agg.Members[0].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            agg.Members[1].Kind().Should().Be(SyntaxKind.FieldDeclaration);
            var md = (MethodDeclarationSyntax)agg.Members[0];

            md.Body.Should().NotBeNull();
            md.Body.OpenBraceToken.Should().NotBe(default);
            md.Body.OpenBraceToken.IsMissing.Should().BeFalse();
            md.Body.CloseBraceToken.Should().NotBe(default);
            md.Body.CloseBraceToken.IsMissing.Should().BeTrue();
            md.Body.Statements.Count.Should().Be(1);
            md.Body.Statements[0].Kind().Should().Be(SyntaxKind.LocalDeclarationStatement);
            var ds = (LocalDeclarationStatementSyntax)md.Body.Statements[0];
            ds.Declaration.Variables.Count.Should().Be(1);
            ds.Declaration.Variables[0].Initializer.Should().NotBeNull();
            ds.Declaration.Variables[0].Initializer.Value.Should().NotBeNull();
            ds.Declaration.Variables[0].Initializer.Value.Kind().Should().Be(SyntaxKind.QueryExpression);
            var qx = (QueryExpressionSyntax)ds.Declaration.Variables[0].Initializer.Value;
            qx.Body.Clauses.Count.Should().Be(1);
            qx.FromClause.Kind().Should().Be(SyntaxKind.FromClause);
            qx.Body.Clauses[0].Kind().Should().Be(SyntaxKind.OrderByClause);
            var oc = (OrderByClauseSyntax)qx.Body.Clauses[0];
            oc.OrderByKeyword.Should().NotBe(default);
            oc.OrderByKeyword.IsMissing.Should().BeFalse();
            oc.Orderings.Count.Should().Be(2);
            oc.Orderings[0].Expression.Should().NotBeNull();
            oc.Orderings[0].Expression.Kind().Should().Be(SyntaxKind.IdentifierName);
            var nm = (IdentifierNameSyntax)oc.Orderings[0].Expression;
            nm.IsMissing.Should().BeFalse();
            oc.Orderings[1].Expression.Should().NotBeNull();
            oc.Orderings[0].Expression.Kind().Should().Be(SyntaxKind.IdentifierName);
            nm = (IdentifierNameSyntax)oc.Orderings[1].Expression;
            nm.IsMissing.Should().BeTrue();

            file.Errors().Length.Should().Be(4);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_InvalidExprTerm);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_ExpectedSelectOrGroup);
            file.Errors()[2].Code.Should().Be((int)ErrorCode.ERR_SemicolonExpected);
            file.Errors()[3].Code.Should().Be((int)ErrorCode.ERR_RbraceExpected);
        }

        [Fact]
        public void PartialInVariableDecl()
        {
            var text = "class C1 { void M1() { int x = 1, partial class y = 2; } }";
            var file = this.ParseTree(text);

            file.Should().NotBeNull();
            file.ToFullString().Should().Be(text);
            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);

            var item1 = (TypeDeclarationSyntax)file.Members[0];
            item1.Identifier.ToString().Should().Be("C1");
            item1.OpenBraceToken.IsMissing.Should().BeFalse();
            item1.Members.Count.Should().Be(2);
            item1.CloseBraceToken.IsMissing.Should().BeFalse();

            var subitem1 = (MethodDeclarationSyntax)item1.Members[0];
            subitem1.Kind().Should().Be(SyntaxKind.MethodDeclaration);
            subitem1.Body.Should().NotBeNull();
            subitem1.Body.OpenBraceToken.IsMissing.Should().BeFalse();
            subitem1.Body.CloseBraceToken.IsMissing.Should().BeTrue();
            subitem1.Body.Statements.Count.Should().Be(1);
            subitem1.Body.Statements[0].Kind().Should().Be(SyntaxKind.LocalDeclarationStatement);
            var decl = (LocalDeclarationStatementSyntax)subitem1.Body.Statements[0];
            decl.SemicolonToken.IsMissing.Should().BeTrue();
            decl.Declaration.Variables.Count.Should().Be(2);
            decl.Declaration.Variables[0].Identifier.ToString().Should().Be("x");
            decl.Declaration.Variables[1].Identifier.IsMissing.Should().BeTrue();
            subitem1.Errors().Length.Should().Be(3);
            subitem1.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_InvalidExprTerm);
            subitem1.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_SemicolonExpected);
            subitem1.Errors()[2].Code.Should().Be((int)ErrorCode.ERR_RbraceExpected);

            var subitem2 = (TypeDeclarationSyntax)item1.Members[1];
            item1.Members[1].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            subitem2.Identifier.ToString().Should().Be("y");
            subitem2.Modifiers[0].ContextualKind().Should().Be(SyntaxKind.PartialKeyword);
            subitem2.OpenBraceToken.IsMissing.Should().BeTrue();
            subitem2.CloseBraceToken.IsMissing.Should().BeTrue();
            subitem2.Errors().Length.Should().Be(3);
            subitem2.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_LbraceExpected);
            subitem2.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_RbraceExpected);
            subitem2.Errors()[2].Code.Should().Be((int)ErrorCode.ERR_InvalidMemberDecl);
        }

        [WorkItem(905394, "DevDiv/Personal")]
        [Fact]
        public void TestThisKeywordInIncompleteLambdaArgumentList()
        {
            var text = @"public class Test
                         {
                             public void Goo()
                             {
                                 var x = ((x, this
                             }
                         }";
            var file = this.ParseTree(text);

            file.ToFullString().Should().Be(text);
            file.ContainsDiagnostics.Should().BeTrue();
        }

        [WorkItem(906986, "DevDiv/Personal")]
        [Fact]
        public void TestIncompleteAttribute()
        {
            var text = @"    [type: F";
            var file = this.ParseTree(text);

            file.ToFullString().Should().Be(text);
            file.ContainsDiagnostics.Should().BeTrue();
        }

        [WorkItem(908952, "DevDiv/Personal")]
        [Fact]
        public void TestNegAttributeOnTypeParameter()
        {
            var text = @"    
                            public class B
                            {
                                void M()
                                {
                                    I<[Test] int> I1=new I<[Test] int>();
                                }
                            } 
                        ";
            var file = this.ParseTree(text);

            file.ToFullString().Should().Be(text);
            file.ContainsDiagnostics.Should().BeTrue();
        }

        [WorkItem(918947, "DevDiv/Personal")]
        [Fact]
        public void TestAtKeywordAsLocalOrParameter()
        {
            var text = @"
class A
{
  public void M()
  {
    int @int = 0;
    if (@int == 1)
    {
      @int = 0;
    }
    MM(@int);
  }
  public void MM(int n) { }
}
";
            var file = this.ParseTree(text);

            file.ToFullString().Should().Be(text);
            file.ContainsDiagnostics.Should().BeFalse();
        }

        [WorkItem(918947, "DevDiv/Personal")]
        [Fact]
        public void TestAtKeywordAsTypeNames()
        {
            var text = @"namespace @namespace
{
    class C1 { }
    class @class : C1 { }
}
";
            var file = this.ParseTree(text);

            file.ToFullString().Should().Be(text);
            file.ContainsDiagnostics.Should().BeFalse();
        }

        [WorkItem(919418, "DevDiv/Personal")]
        [Fact]
        public void TestNegDefaultAsLambdaParameter()
        {
            var text = @"class C
{
    delegate T Func<T>();
    delegate T Func<A0, T>(A0 a0);
    delegate T Func<A0, A1, T>(A0 a0, A1 a1);
    delegate T Func<A0, A1, A2, A3, T>(A0 a0, A1 a1, A2 a2, A3 a3);

    static void X()
    {
        // Func<int,int> f1      = (int @in) => 1;              // ok: @Keyword as parameter name
        Func<int,int> f2      = (int where, int from) => 1;  // ok: contextual keyword as parameter name
        Func<int,int> f3      = (int default) => 1;          // err: Keyword as parameter name
    }
}
";
            var file = this.ParseTree(text);

            file.ToFullString().Should().Be(text);
            file.ContainsDiagnostics.Should().BeTrue();
        }

        [Fact]
        public void TestEmptyUsingDirective()
        {
            var text = @"using;";
            var file = this.ParseTree(text);

            file.ToFullString().Should().Be(text);
            file.Errors().Length.Should().Be(1);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_IdentifierExpected);

            var usings = file.Usings;
            usings.Count.Should().Be(1);
            usings[0].Name.IsMissing.Should().BeTrue();
        }

        [Fact]
        public void TestNumericLiteralInUsingDirective()
        {
            var text = @"using 10;";
            var file = this.ParseTree(text);

            file.ToFullString().Should().Be(text);
            file.Errors().Length.Should().Be(1);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_IdentifierExpected);

            var usings = file.Usings;
            usings.Count.Should().Be(1);
            usings[0].Name.IsMissing.Should().BeTrue();
        }

        [Fact]
        public void TestNamespaceDeclarationInUsingDirective()
        {
            var text = @"using namespace Goo";
            var file = this.ParseTree(text);

            file.ToFullString().Should().Be(text);
            file.Errors().Length.Should().Be(3);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_IdentifierExpectedKW);
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_LbraceExpected);
            file.Errors()[2].Code.Should().Be((int)ErrorCode.ERR_RbraceExpected);

            var usings = file.Usings;
            usings.Count.Should().Be(1);
            usings[0].Name.IsMissing.Should().BeTrue();

            var members = file.Members;
            members.Count.Should().Be(1);

            var namespaceDeclaration = members[0];
            namespaceDeclaration.Kind().Should().Be(SyntaxKind.NamespaceDeclaration);
            ((NamespaceDeclarationSyntax)namespaceDeclaration).Name.IsMissing.Should().BeFalse();
        }

        [Fact]
        public void TestFileScopedNamespaceDeclarationInUsingDirective()
        {
            var text = @"using namespace Goo;";
            var file = this.ParseTree(text);

            file.ToFullString().Should().Be(text);
            file.GetDiagnostics().Verify(
                // (1,7): error CS1041: Identifier expected; 'namespace' is a keyword
                // using namespace Goo;
                Diagnostic(ErrorCode.ERR_IdentifierExpectedKW, "namespace").WithArguments("", "namespace").WithLocation(1, 7));

            var usings = file.Usings;
            usings.Count.Should().Be(1);
            usings[0].Name.IsMissing.Should().BeTrue();

            var members = file.Members;
            members.Count.Should().Be(1);

            var namespaceDeclaration = members[0];
            namespaceDeclaration.Kind().Should().Be(SyntaxKind.FileScopedNamespaceDeclaration);
            ((FileScopedNamespaceDeclarationSyntax)namespaceDeclaration).Name.IsMissing.Should().BeFalse();
        }

        [Fact]
        public void TestContextualKeywordAsFromVariable()
        {
            var text = @"
class C 
{ 
    int x = from equals in new[] { 1, 2, 3 } select 1;
}";
            var file = this.ParseTree(text);

            file.ToFullString().Should().Be(text);
            file.Errors().Length.Should().Be(1);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_IdentifierExpected);
        }

        [WorkItem(537210, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/537210")]
        [Fact]
        public void RegressException4UseValueInAccessor()
        {
            var text = @"public class MyClass
{
    public int MyProp
    {
        set { int value = 0; } // CS0136
    }
    D x;
    int this[int n]
    {
        get { return 0; }
        set { x = (value) => { value++; }; }  // CS0136
    }

    public delegate void D(int n);
    public event D MyEvent
    {
        add { object value = null; } // CS0136
        remove { }
    }
}";
            var file = this.ParseTree(text);

            file.ToFullString().Should().Be(text);
            // file.ContainsDiagnostics.Should().BeTrue(); // CS0136 is not parser error
        }

        [WorkItem(931315, "DevDiv/Personal")]
        [Fact]
        public void RegressException4InvalidOperator()
        {
            var text = @"class A 
{
  public static int operator &&(A a) // CS1019
  {    return 0;   }
}
";
            var file = this.ParseTree(text);

            file.ToFullString().Should().Be(text);
            file.ContainsDiagnostics.Should().BeTrue();
        }

        [WorkItem(931316, "DevDiv/Personal")]
        [Fact]
        public void RegressNoError4NoOperator()
        {
            var text = @"class A 
{
  public static A operator (A a) // CS1019
  {    return a;   }
}
";
            var file = this.ParseTree(text);

            file.ToFullString().Should().Be(text);
            file.ContainsDiagnostics.Should().BeTrue();
        }

        [WorkItem(537214, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/537214")]
        [Fact]
        public void RegressWarning4UseContextKeyword()
        {
            var text = @"class TestClass
{
    int partial { get; set; }
    static int Main()
    {
        TestClass tc = new TestClass();
        tc.partial = 0;
        return 0;
    }
}
";
            var file = this.ParseTree(text);

            file.ToFullString().Should().Be(text);
            file.ContainsDiagnostics.Should().BeFalse();
        }

        [WorkItem(537150, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/537150")]
        [Fact]
        public void ParseStartOfAccessor()
        {
            var text = @"class Program
{
  int this[string s]
  {
    g
  }
}
";
            var file = this.ParseTree(text);

            file.ToFullString().Should().Be(text);
            file.Errors().Length.Should().Be(1);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_GetOrSetExpected);
        }

        [WorkItem(536050, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/536050")]
        [Fact]
        public void ParseMethodWithConstructorInitializer()
        {
            //someone has a typo in the name of their ctor - parse it as a ctor, and accept the initializer 
            var text = @"
class C
{
  CTypo() : base() {
     //body
  }
}
";
            var file = this.ParseTree(text);

            file.ToFullString().Should().Be(text);
            file.Errors().Length.Should().Be(0);

            // CONSIDER: Dev10 actually gives 'CS1002: ; expected', because it thinks you were trying to
            // specify a method without a body.  This is a little silly, since we already know the method
            // isn't abstract.  It might be reasonable to say that an open brace was expected though.

            var classDecl = file.ChildNodesAndTokens()[0];
            classDecl.Kind().Should().Be(SyntaxKind.ClassDeclaration);

            var methodDecl = classDecl.ChildNodesAndTokens()[3];
            methodDecl.Kind().Should().Be(SyntaxKind.ConstructorDeclaration); //not MethodDeclaration
            methodDecl.ContainsDiagnostics.Should().BeFalse();

            var methodBody = methodDecl.ChildNodesAndTokens()[3];
            methodBody.Kind().Should().Be(SyntaxKind.Block);
            methodBody.ContainsDiagnostics.Should().BeFalse();
        }

        [WorkItem(537157, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/537157")]
        [Fact]
        public void MissingInternalNode()
        {
            var text = @"[1]";
            var file = this.ParseTree(text);

            file.ToFullString().Should().Be(text);

            var incompleteMemberDecl = file.ChildNodesAndTokens()[0];
            incompleteMemberDecl.Kind().Should().Be(SyntaxKind.IncompleteMember);
            incompleteMemberDecl.IsMissing.Should().BeFalse();

            var attributeDecl = incompleteMemberDecl.ChildNodesAndTokens()[0];
            attributeDecl.Kind().Should().Be(SyntaxKind.AttributeList);
            attributeDecl.IsMissing.Should().BeFalse();

            var openBracketToken = attributeDecl.ChildNodesAndTokens()[0];
            openBracketToken.Kind().Should().Be(SyntaxKind.OpenBracketToken);
            openBracketToken.IsMissing.Should().BeFalse();

            var attribute = attributeDecl.ChildNodesAndTokens()[1];
            attribute.Kind().Should().Be(SyntaxKind.Attribute);
            attribute.IsMissing.Should().BeTrue();

            var identifierName = attribute.ChildNodesAndTokens()[0];
            identifierName.Kind().Should().Be(SyntaxKind.IdentifierName);
            identifierName.IsMissing.Should().BeTrue();

            var identifierToken = identifierName.ChildNodesAndTokens()[0];
            identifierToken.Kind().Should().Be(SyntaxKind.IdentifierToken);
            identifierToken.IsMissing.Should().BeTrue();
        }

        [WorkItem(538469, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/538469")]
        [Fact]
        public void FromKeyword()
        {
            var text = @"
using System.Collections.Generic;
using System.Linq;
public class QueryExpressionTest
{
    public static int Main()
    {
        int[] expr1 = new int[] { 1, 2, 3, };
        IEnumerable<int> query01 = from value in expr1 select value;
        IEnumerable<int> query02 = from yield in expr1 select yield;
        IEnumerable<int> query03 = from select in expr1 select select;
        return 0;
    }
}";
            var file = this.ParseTree(text);

            file.ToFullString().Should().Be(text);

            file.Errors().Length.Should().Be(3);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_IdentifierExpected); //expecting item name - found "select" keyword
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_InvalidExprTerm); //expecting expression - found "select" keyword
            file.Errors()[2].Code.Should().Be((int)ErrorCode.ERR_SemicolonExpected); //we inserted a missing semicolon in a place we didn't expect
        }

        [WorkItem(538971, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/538971")]
        [Fact]
        public void UnclosedGenericInExplicitInterfaceName()
        {
            var text = @"
interface I<T>
{
    void Goo();
}
 
class C : I<int>
{
    void I<.Goo() { }
}
";
            var file = this.ParseTree(text);

            file.ToFullString().Should().Be(text);

            file.Errors().Length.Should().Be(2);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_TypeExpected); //expecting a type (argument)
            file.Errors()[1].Code.Should().Be((int)ErrorCode.ERR_SyntaxError); //expecting close angle bracket
        }

        [WorkItem(540788, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/540788")]
        [Fact]
        public void IncompleteForEachStatement()
        {
            var text = @"
public class Test
{
    public static void Main(string[] args)
    {
        foreach";

            var srcTree = this.ParseTree(text);

            srcTree.ToFullString().Should().Be(text);
            srcTree.GetLastToken().ToString().Should().Be("foreach");

            // Get the Foreach Node
            var foreachNode = srcTree.GetLastToken().Parent;

            // Verify 3 empty nodes are created by the parser for error recovery.
            foreachNode.ChildNodes().ToList().Count.Should().Be(3);
        }

        [WorkItem(542236, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/542236")]
        [Fact]
        public void InsertOpenBraceBeforeCodes()
        {
            var text = @"{
        this.I = i;
    };
}";

            SyntaxTree syntaxTree = SyntaxFactory.ParseSyntaxTree(text, TestOptions.Regular9);
            syntaxTree.GetCompilationUnitRoot().ToFullString().Should().Be(text);

            // The issue (9391) was exhibited while enumerating the diagnostics
            syntaxTree.GetDiagnostics().Select(d => ((IFormattable)d).ToString(null, EnsureEnglishUICulture.PreferredOrNull)).SequenceEqual(new[]
            {
                "(4,1): error CS1022: Type or namespace definition, or end-of-file expected",
            }).Should().BeTrue();
        }

        [WorkItem(542352, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/542352")]
        [Fact]
        public void IncompleteTopLevelOperator()
        {
            var text = @"
fg implicit//
class C { }
";

            SyntaxTree syntaxTree = SyntaxFactory.ParseSyntaxTree(text);
            syntaxTree.GetCompilationUnitRoot().ToFullString().Should().Be(text);

            // 9553: Several of the locations were incorrect and one was negative
            syntaxTree.GetDiagnostics().Select(d => ((IFormattable)d).ToString(null, EnsureEnglishUICulture.PreferredOrNull)).Should().Equal(new[]
            {
                // Error on the return type, because in C# syntax it goes after the operator and implicit/explicit keywords
                "(2,1): error CS1553: Declaration is not valid; use '+ operator <dest-type> (...' instead",
                // Error on "implicit" because there should be an operator keyword
                "(2,4): error CS1003: Syntax error, 'operator' expected",
                // Error on "implicit" because there should be an operator symbol
                "(2,4): error CS1037: Overloadable operator expected",
                // Missing parameter list and body
                "(2,12): error CS1003: Syntax error, '(' expected",
                "(2,12): error CS1026: ) expected",
                "(2,12): error CS1002: ; expected",
            });
        }

        [WorkItem(545647, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/545647")]
        [Fact]
        public void IncompleteVariableDeclarationAboveDotMemberAccess()
        {
            var text = @"
class C
{
    void Main()
    {
        C
        Console.WriteLine();
    }
}
";

            SyntaxTree syntaxTree = SyntaxFactory.ParseSyntaxTree(text);
            syntaxTree.GetCompilationUnitRoot().ToFullString().Should().Be(text);

            syntaxTree.GetDiagnostics().Select(d => ((IFormattable)d).ToString(null, EnsureEnglishUICulture.PreferredOrNull)).SequenceEqual(new[]
            {
                "(6,10): error CS1001: Identifier expected",
                "(6,10): error CS1002: ; expected",
            }).Should().BeTrue();
        }

        [WorkItem(545647, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/545647")]
        [Fact]
        public void IncompleteVariableDeclarationAbovePointerMemberAccess()
        {
            var text = @"
class C
{
    void Main()
    {
        C
        Console->WriteLine();
    }
}
";

            SyntaxTree syntaxTree = SyntaxFactory.ParseSyntaxTree(text);
            syntaxTree.GetCompilationUnitRoot().ToFullString().Should().Be(text);

            syntaxTree.GetDiagnostics().Select(d => ((IFormattable)d).ToString(null, EnsureEnglishUICulture.PreferredOrNull)).SequenceEqual(new[]
            {
                "(6,10): error CS1001: Identifier expected",
                "(6,10): error CS1002: ; expected",
            }).Should().BeTrue();
        }

        [WorkItem(545647, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/545647")]
        [Fact]
        public void IncompleteVariableDeclarationAboveBinaryExpression()
        {
            var text = @"
class C
{
    void Main()
    {
        C
        A + B;
    }
}
";

            SyntaxTree syntaxTree = SyntaxFactory.ParseSyntaxTree(text);
            syntaxTree.GetCompilationUnitRoot().ToFullString().Should().Be(text);

            syntaxTree.GetDiagnostics().Select(d => ((IFormattable)d).ToString(null, EnsureEnglishUICulture.PreferredOrNull)).SequenceEqual(new[]
            {
                "(6,10): error CS1001: Identifier expected",
                "(6,10): error CS1002: ; expected",
            }).Should().BeTrue();
        }

        [WorkItem(545647, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/545647")]
        [Fact]
        public void IncompleteVariableDeclarationAboveMemberAccess_MultiLine()
        {
            var text = @"
class C
{
    void Main()
    {
        C

        Console.WriteLine();
    }
}
";

            SyntaxTree syntaxTree = SyntaxFactory.ParseSyntaxTree(text);
            syntaxTree.GetCompilationUnitRoot().ToFullString().Should().Be(text);

            syntaxTree.GetDiagnostics().Select(d => ((IFormattable)d).ToString(null, EnsureEnglishUICulture.PreferredOrNull)).SequenceEqual(new[]
            {
                "(6,10): error CS1001: Identifier expected",
                "(6,10): error CS1002: ; expected",
            }).Should().BeTrue();
        }

        [WorkItem(545647, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/545647")]
        [Fact]
        public void IncompleteVariableDeclarationBeforeMemberAccessOnSameLine()
        {
            var text = @"
class C
{
    void Main()
    {
        C Console.WriteLine();
    }
}
";

            SyntaxTree syntaxTree = SyntaxFactory.ParseSyntaxTree(text);
            syntaxTree.GetCompilationUnitRoot().ToFullString().Should().Be(text);

            syntaxTree.GetDiagnostics().Select(d => ((IFormattable)d).ToString(null, EnsureEnglishUICulture.PreferredOrNull)).SequenceEqual(new[]
            {
                "(6,18): error CS1003: Syntax error, ',' expected",
                "(6,19): error CS1002: ; expected",
            }).Should().BeTrue();
        }

        [WorkItem(545647, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/545647")]
        [Fact]
        public void EqualsIsNotAmbiguous()
        {
            var text = @"
class C
{
    void Main()
    {
        C
        A = B;
    }
}
";

            SyntaxTree syntaxTree = SyntaxFactory.ParseSyntaxTree(text);
            syntaxTree.GetCompilationUnitRoot().ToFullString().Should().Be(text);

            syntaxTree.GetDiagnostics().Should().BeEmpty();
        }

        [WorkItem(547120, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/547120")]
        [Fact]
        public void ColonColonInExplicitInterfaceMember()
        {
            var text = @"
_ _::this
";

            SyntaxTree syntaxTree = SyntaxFactory.ParseSyntaxTree(text);
            syntaxTree.GetCompilationUnitRoot().ToFullString().Should().Be(text);

            syntaxTree.GetDiagnostics().Verify(
                // (2,4): error CS1003: Syntax error, '.' expected
                // _ _::this
                Diagnostic(ErrorCode.ERR_SyntaxError, "::").WithArguments("."),
                // (2,10): error CS1003: Syntax error, '[' expected
                // _ _::this
                Diagnostic(ErrorCode.ERR_SyntaxError, "").WithArguments("["),
                // (2,10): error CS1003: Syntax error, ']' expected
                // _ _::this
                Diagnostic(ErrorCode.ERR_SyntaxError, "").WithArguments("]"),
                // (2,10): error CS1514: { expected
                // _ _::this
                Diagnostic(ErrorCode.ERR_LbraceExpected, ""),
                // (2,10): error CS1513: } expected
                // _ _::this
                Diagnostic(ErrorCode.ERR_RbraceExpected, ""));

            CreateCompilation(text).VerifyDiagnostics(
                // (2,4): error CS1003: Syntax error, '.' expected
                // _ _::this
                Diagnostic(ErrorCode.ERR_SyntaxError, "::").WithArguments(".").WithLocation(2, 4),
                // (2,10): error CS1003: Syntax error, '[' expected
                // _ _::this
                Diagnostic(ErrorCode.ERR_SyntaxError, "").WithArguments("[").WithLocation(2, 10),
                // (2,10): error CS1003: Syntax error, ']' expected
                // _ _::this
                Diagnostic(ErrorCode.ERR_SyntaxError, "").WithArguments("]").WithLocation(2, 10),
                // (2,10): error CS1514: { expected
                // _ _::this
                Diagnostic(ErrorCode.ERR_LbraceExpected, "").WithLocation(2, 10),
                // (2,10): error CS1513: } expected
                // _ _::this
                Diagnostic(ErrorCode.ERR_RbraceExpected, "").WithLocation(2, 10),
                // (2,3): error CS0246: The type or namespace name '_' could not be found (are you missing a using directive or an assembly reference?)
                // _ _::this
                Diagnostic(ErrorCode.ERR_SingleTypeNameNotFound, "_").WithArguments("_").WithLocation(2, 3),
                // (2,1): error CS0246: The type or namespace name '_' could not be found (are you missing a using directive or an assembly reference?)
                // _ _::this
                Diagnostic(ErrorCode.ERR_SingleTypeNameNotFound, "_").WithArguments("_").WithLocation(2, 1),
                // error CS1551: Indexers must have at least one parameter
                Diagnostic(ErrorCode.ERR_IndexerNeedsParam).WithLocation(1, 1),
                // (2,3): error CS0538: '_' in explicit interface declaration is not an interface
                // _ _::this
                Diagnostic(ErrorCode.ERR_ExplicitInterfaceImplementationNotInterface, "_").WithArguments("_").WithLocation(2, 3),
                // (2,6): error CS0548: '<invalid-global-code>.this': property or indexer must have at least one accessor
                // _ _::this
                Diagnostic(ErrorCode.ERR_PropertyWithNoAccessors, "this").WithArguments("<invalid-global-code>.this").WithLocation(2, 6));
        }

        [WorkItem(649806, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/649806")]
        [Fact]
        public void Repro649806()
        {
            var source = "a b:: /**/\r\n";
            var tree = SyntaxFactory.ParseSyntaxTree(source);
            var diags = tree.GetDiagnostics();
            diags.Verify(
                // (1,4): error CS1002: ; expected
                // a b:: /**/
                Diagnostic(ErrorCode.ERR_SemicolonExpected, "::").WithLocation(1, 4),
                // (1,4): error CS1001: Identifier expected
                // a b:: /**/
                Diagnostic(ErrorCode.ERR_IdentifierExpected, "::").WithLocation(1, 4),
                // (1,6): error CS1001: Identifier expected
                // a b:: /**/
                Diagnostic(ErrorCode.ERR_IdentifierExpected, "").WithLocation(1, 6),
                // (1,6): error CS1002: ; expected
                // a b:: /**/
                Diagnostic(ErrorCode.ERR_SemicolonExpected, "").WithLocation(1, 6)
                );
        }

        [WorkItem(674564, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/674564")]
        [Fact]
        public void Repro674564()
        {
            var source = @"
class C
{
    int P { set . } }
}";
            var tree = SyntaxFactory.ParseSyntaxTree(source);
            var diags = tree.GetDiagnostics();
            diags.ToArray();
            diags.Verify(
                // We see this diagnostic because the accessor has no open brace.

                // (4,17): error CS1043: { or ; expected
                //     int P { set . } }
                Diagnostic(ErrorCode.ERR_SemiOrLBraceOrArrowExpected, "."),

                // We see this diagnostic because we're trying to skip bad tokens in the block and 
                // the "expected" token (i.e. the one we report when we see something that's not a
                // statement) is close brace.
                // CONSIDER: This diagnostic isn't great.

                // (4,17): error CS1513: } expected
                //     int P { set . } }
                Diagnostic(ErrorCode.ERR_RbraceExpected, "."));
        }

        [WorkItem(680733, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/680733")]
        [Fact]
        public void Repro680733a()
        {
            var source = @"
class Test
{
    public async Task<in{> Bar()
    {
        return 1;
    }
}
";
            AssertEqualRoundtrip(source);
        }

        [WorkItem(680733, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/680733")]
        [Fact]
        public void Repro680733b()
        {
            var source = @"
using System;
using AwesomeAssertions;

class Test
{
    public async Task<[Obsolete]in{> Bar()
    {
        return 1;
    }
}
";
            AssertEqualRoundtrip(source);
        }

        [WorkItem(680739, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/680739")]
        [Fact]
        public void Repro680739()
        {
            var source = @"a b<c..<using.d";
            AssertEqualRoundtrip(source);
        }

        [WorkItem(675600, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/675600")]
        [Fact]
        public void TestBracesToOperatorDoubleGreaterThan()
        {
            AssertEqualRoundtrip(
@"/// <see cref=""operator}}""/>
class C {}");

            AssertEqualRoundtrip(
@"/// <see cref=""operator{{""/>
class C {}");

            AssertEqualRoundtrip(
@"/// <see cref=""operator}=""/>
class C {}");

            AssertEqualRoundtrip(
@"/// <see cref=""operator}}=""/>
class C {}");

            AssertEqualRoundtrip(
@"/// <see cref=""operator}}}=""/>
class C {}");
        }

        private void AssertEqualRoundtrip(string source)
        {
            var tree = SyntaxFactory.ParseSyntaxTree(source);
            var toString = tree.GetRoot().ToFullString();
            toString.Should().Be(source);
        }

        [WorkItem(684816, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/684816")]
        [Fact]
        public void GenericPropertyWithMissingIdentifier()
        {
            var source = @"
class C : I
{
    int I./*missing*/< {
";

            var tree = SyntaxFactory.ParseSyntaxTree(source);
            var toString = tree.GetRoot().ToFullString();
            toString.Should().Be(source);
            tree.GetDiagnostics().Verify(
                // (4,22): error CS1001: Identifier expected
                //     int I./*missing*/< {
                Diagnostic(ErrorCode.ERR_IdentifierExpected, "<"),
                // (4,22): error CS7002: Unexpected use of a generic name
                //     int I./*missing*/< {
                Diagnostic(ErrorCode.ERR_UnexpectedGenericName, "<"),
                // (4,24): error CS1003: Syntax error, '>' expected
                //     int I./*missing*/< {
                Diagnostic(ErrorCode.ERR_SyntaxError, "{").WithArguments(">"),
                // (4,25): error CS1513: } expected
                //     int I./*missing*/< {
                Diagnostic(ErrorCode.ERR_RbraceExpected, ""),
                // (4,25): error CS1513: } expected
                //     int I./*missing*/< {
                Diagnostic(ErrorCode.ERR_RbraceExpected, ""));
        }

        [WorkItem(684816, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/684816")]
        [Fact]
        public void GenericEventWithMissingIdentifier()
        {
            var source = @"
class C : I
{
    event D I./*missing*/< {
";

            var tree = SyntaxFactory.ParseSyntaxTree(source);
            var toString = tree.GetRoot().ToFullString();
            toString.Should().Be(source);
            tree.GetDiagnostics().Verify(
                // (4,26): error CS1001: Identifier expected
                //     event D I./*missing*/< {
                Diagnostic(ErrorCode.ERR_IdentifierExpected, "<"),
                // (4,26): error CS1001: Identifier expected
                //     event D I./*missing*/< {
                Diagnostic(ErrorCode.ERR_IdentifierExpected, "<"),
                // (4,28): error CS1003: Syntax error, '>' expected
                //     event D I./*missing*/< {
                Diagnostic(ErrorCode.ERR_SyntaxError, "{").WithArguments(">"),
                // (4,26): error CS7002: Unexpected use of a generic name
                //     event D I./*missing*/< {
                Diagnostic(ErrorCode.ERR_UnexpectedGenericName, "<"),
                // (4,29): error CS1513: } expected
                //     event D I./*missing*/< {
                Diagnostic(ErrorCode.ERR_RbraceExpected, ""),
                // (4,29): error CS1513: } expected
                //     event D I./*missing*/< {
                Diagnostic(ErrorCode.ERR_RbraceExpected, ""));
        }

        [WorkItem(684816, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/684816")]
        [Fact]
        public void ExplicitImplementationEventWithColonColon()
        {
            var source = @"
class C : I
{
    event D I::
";

            var tree = SyntaxFactory.ParseSyntaxTree(source);
            var toString = tree.GetRoot().ToFullString();
            toString.Should().Be(source);
            tree.GetDiagnostics().Verify(
                // (4,14): error CS0071: An explicit interface implementation of an event must use event accessor syntax
                //     event D I::
                Diagnostic(ErrorCode.ERR_ExplicitEventFieldImpl, "::"),
                // (4,14): error CS0687: The namespace alias qualifier '::' always resolves to a type or namespace so is illegal here. Consider using '.' instead.
                //     event D I::
                Diagnostic(ErrorCode.ERR_AliasQualAsExpression, "::"),
                // (4,16): error CS1513: } expected
                //     event D I::
                Diagnostic(ErrorCode.ERR_RbraceExpected, ""));
        }

        [WorkItem(684816, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/684816")]
        [Fact]
        public void EventNamedThis()
        {
            var source = @"
class C
{
    event System.Action this
";

            var tree = SyntaxFactory.ParseSyntaxTree(source);
            var toString = tree.GetRoot().ToFullString();
            toString.Should().Be(source);
            tree.GetDiagnostics().Verify(
                // (4,25): error CS1001: Identifier expected
                //     event System.Action this
                Diagnostic(ErrorCode.ERR_IdentifierExpected, "this"),
                // (4,29): error CS1514: { expected
                //     event System.Action this
                Diagnostic(ErrorCode.ERR_LbraceExpected, ""),
                // (4,29): error CS1513: } expected
                //     event System.Action this
                Diagnostic(ErrorCode.ERR_RbraceExpected, ""),
                // (4,29): error CS1513: } expected
                //     event System.Action this
                Diagnostic(ErrorCode.ERR_RbraceExpected, ""));
        }

        [WorkItem(697022, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/697022")]
        [Fact]
        public void GenericEnumWithMissingIdentifiers()
        {
            var source = @"enum
<//aaaa
enum
";

            var tree = SyntaxFactory.ParseSyntaxTree(source);
            var toString = tree.GetRoot().ToFullString();
            toString.Should().Be(source);
            tree.GetDiagnostics().ToArray();
        }

        [WorkItem(703809, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/703809")]
        [Fact]
        public void ReplaceOmittedArrayRankWithMissingIdentifier()
        {
            var source = @"fixed a,b {//aaaa
static
";

            var tree = SyntaxFactory.ParseSyntaxTree(source);
            var toString = tree.GetRoot().ToFullString();
            toString.Should().Be(source);
            tree.GetDiagnostics().ToArray();
        }

        [WorkItem(716245, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/716245")]
        [Fact]
        public void ManySkippedTokens()
        {
            const int numTokens = 500000; // Prohibitively slow without fix.
            var source = new string(',', numTokens);
            var tree = SyntaxFactory.ParseSyntaxTree(source);
            var eofToken = ((CompilationUnitSyntax)tree.GetRoot()).EndOfFileToken;
            eofToken.FullWidth.Should().Be(numTokens);
            eofToken.LeadingTrivia.Count.Should().Be(numTokens); // Confirm that we built a list.
        }

        [WorkItem(947819, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/947819")]
        [Fact]
        public void MissingOpenBraceForClass()
        {
            var source = @"namespace n
{
    class c
}
";
            var root = SyntaxFactory.ParseSyntaxTree(source).GetRoot();

            root.ToFullString().Should().Be(source);
            // Verify incomplete class decls don't eat tokens of surrounding nodes
            var classDecl = root.DescendantNodes().OfType<ClassDeclarationSyntax>().Single();
            classDecl.Identifier.IsMissing.Should().BeFalse();
            classDecl.OpenBraceToken.IsMissing.Should().BeTrue();
            classDecl.CloseBraceToken.IsMissing.Should().BeTrue();
            var ns = root.DescendantNodes().OfType<NamespaceDeclarationSyntax>().Single();
            ns.OpenBraceToken.IsMissing.Should().BeFalse();
            ns.CloseBraceToken.IsMissing.Should().BeFalse();
        }

        [WorkItem(947819, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/947819")]
        [Fact]
        public void MissingOpenBraceForClassFileScopedNamespace()
        {
            var source = @"namespace n;

class c
";
            var root = SyntaxFactory.ParseSyntaxTree(source).GetRoot();

            root.ToFullString().Should().Be(source);
            // Verify incomplete class decls don't eat tokens of surrounding nodes
            var classDecl = root.DescendantNodes().OfType<ClassDeclarationSyntax>().Single();
            classDecl.Identifier.IsMissing.Should().BeFalse();
            classDecl.OpenBraceToken.IsMissing.Should().BeTrue();
            classDecl.CloseBraceToken.IsMissing.Should().BeTrue();
            var ns = root.DescendantNodes().OfType<FileScopedNamespaceDeclarationSyntax>().Single();
            ns.SemicolonToken.IsMissing.Should().BeFalse();
        }

        [WorkItem(947819, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/947819")]
        [Fact]
        public void MissingOpenBraceForStruct()
        {
            var source = @"namespace n
{
    struct c : I
}
";
            var root = SyntaxFactory.ParseSyntaxTree(source).GetRoot();

            root.ToFullString().Should().Be(source);
            // Verify incomplete struct decls don't eat tokens of surrounding nodes
            var structDecl = root.DescendantNodes().OfType<StructDeclarationSyntax>().Single();
            structDecl.OpenBraceToken.IsMissing.Should().BeTrue();
            structDecl.CloseBraceToken.IsMissing.Should().BeTrue();
            var ns = root.DescendantNodes().OfType<NamespaceDeclarationSyntax>().Single();
            ns.OpenBraceToken.IsMissing.Should().BeFalse();
            ns.CloseBraceToken.IsMissing.Should().BeFalse();
        }

        [WorkItem(947819, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/947819")]
        [Fact]
        public void MissingNameForStruct()
        {
            var source = @"namespace n
{
    struct : I
    {
    }
}
";
            var root = SyntaxFactory.ParseSyntaxTree(source).GetRoot();

            root.ToFullString().Should().Be(source);
            // Verify incomplete struct decls don't eat tokens of surrounding nodes
            var structDecl = root.DescendantNodes().OfType<StructDeclarationSyntax>().Single();
            structDecl.Identifier.IsMissing.Should().BeTrue();
            structDecl.OpenBraceToken.IsMissing.Should().BeFalse();
            structDecl.CloseBraceToken.IsMissing.Should().BeFalse();
            var ns = root.DescendantNodes().OfType<NamespaceDeclarationSyntax>().Single();
            ns.OpenBraceToken.IsMissing.Should().BeFalse();
            ns.CloseBraceToken.IsMissing.Should().BeFalse();
        }

        [WorkItem(947819, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/947819")]
        [Fact]
        public void MissingNameForClass()
        {
            var source = @"namespace n
{
    class
    {
    }
}
";
            var root = SyntaxFactory.ParseSyntaxTree(source).GetRoot();

            root.ToFullString().Should().Be(source);
            // Verify incomplete class decls don't eat tokens of surrounding nodes
            var classDecl = root.DescendantNodes().OfType<ClassDeclarationSyntax>().Single();
            classDecl.Identifier.IsMissing.Should().BeTrue();
            classDecl.OpenBraceToken.IsMissing.Should().BeFalse();
            classDecl.CloseBraceToken.IsMissing.Should().BeFalse();
            var ns = root.DescendantNodes().OfType<NamespaceDeclarationSyntax>().Single();
            ns.OpenBraceToken.IsMissing.Should().BeFalse();
            ns.CloseBraceToken.IsMissing.Should().BeFalse();
        }
    }
}
