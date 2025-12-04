// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#nullable disable

using System;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp.Test.Utilities;
using Microsoft.CodeAnalysis.Test.Utilities;
using Roslyn.Test.Utilities;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.CodeAnalysis.CSharp.UnitTests
{
    public class DeclarationParsingTests : ParsingTests
    {
        public DeclarationParsingTests(ITestOutputHelper output) : base(output) { }

        protected override SyntaxTree ParseTree(string text, CSharpParseOptions options)
        {
            return SyntaxFactory.ParseSyntaxTree(text, options ?? TestOptions.Regular);
        }

        [Fact]
        public void TestExternAlias()
        {
            var text = "extern alias a;";
            var file = this.ParseFile(text);

            file.Should().NotBeNull();
            file.Externs.Count.Should().Be(1);
            file.ToString().Should().Be(text);
            file.Errors().Length.Should().Be(0);

            var ea = file.Externs[0];

            ea.ExternKeyword.Should().NotBe(default);
            ea.ExternKeyword.Kind().Should().Be(SyntaxKind.ExternKeyword);
            ea.AliasKeyword.Should().NotBe(default);
            ea.AliasKeyword.Kind().Should().Be(SyntaxKind.AliasKeyword);
            ea.AliasKeyword.IsMissing.Should().BeFalse();
            ea.Identifier.Should().NotBe(default);
            ea.Identifier.ToString().Should().Be("a");
            ea.SemicolonToken.Should().NotBe(default);
        }

        [Fact]
        public void TestUsing()
        {
            var text = "using a;";
            var file = this.ParseFile(text);

            file.Should().NotBeNull();
            file.Usings.Count.Should().Be(1);
            file.ToString().Should().Be(text);
            file.Errors().Length.Should().Be(0);

            var ud = file.Usings[0];

            ud.UsingKeyword.Should().NotBe(default);
            ud.UsingKeyword.Kind().Should().Be(SyntaxKind.UsingKeyword);
            ud.Alias.Should().BeNull();
            ud.StaticKeyword == default(SyntaxToken).Should().BeTrue();
            ud.Name.Should().NotBeNull();
            ud.Name.ToString().Should().Be("a");
            ud.SemicolonToken.Should().NotBe(default);
        }

        [Fact]
        public void TestUsingStatic()
        {
            var text = "using static a;";
            var file = this.ParseFile(text);

            file.Should().NotBeNull();
            file.Usings.Count.Should().Be(1);
            file.ToString().Should().Be(text);
            file.Errors().Length.Should().Be(0);

            var ud = file.Usings[0];

            ud.UsingKeyword.Should().NotBe(default);
            ud.UsingKeyword.Kind().Should().Be(SyntaxKind.UsingKeyword);
            ud.StaticKeyword.Kind().Should().Be(SyntaxKind.StaticKeyword);
            ud.Alias.Should().BeNull();
            ud.Name.Should().NotBeNull();
            ud.Name.ToString().Should().Be("a");
            ud.SemicolonToken.Should().NotBe(default);
        }

        [Fact]
        public void TestUsingStaticInWrongOrder()
        {
            var text = "static using a;";
            var file = this.ParseFile(text);

            file.Should().NotBeNull();
            file.Usings.Count.Should().Be(1);
            file.ToFullString().Should().Be(text);

            var errors = file.Errors();
            errors.Length > 0.Should().BeTrue();
            errors[0].Code.Should().Be((int)ErrorCode.ERR_NamespaceUnexpected);
        }

        [Fact]
        public void TestDuplicateStatic()
        {
            var text = "using static static a;";
            var file = this.ParseFile(text);

            file.Should().NotBeNull();
            file.Usings.Count.Should().Be(1);
            file.ToString().Should().Be(text);

            var errors = file.Errors();
            errors.Length > 0.Should().BeTrue();
            errors[0].Code.Should().Be((int)ErrorCode.ERR_IdentifierExpectedKW);
        }

        [Fact]
        public void TestUsingNamespace()
        {
            var text = "using namespace a;";
            var file = this.ParseFile(text);

            file.Should().NotBeNull();
            file.Usings.Count.Should().Be(1);
            file.ToString().Should().Be(text);

            var errors = file.Errors();
            errors.Length > 0.Should().BeTrue();
            errors[0].Code.Should().Be((int)ErrorCode.ERR_IdentifierExpectedKW);
        }

        [Fact]
        public void TestUsingDottedName()
        {
            var text = "using a.b;";
            var file = this.ParseFile(text);

            file.Should().NotBeNull();
            file.Usings.Count.Should().Be(1);
            file.ToString().Should().Be(text);
            file.Errors().Length.Should().Be(0);

            var ud = file.Usings[0];

            ud.UsingKeyword.Should().NotBe(default);
            ud.UsingKeyword.Kind().Should().Be(SyntaxKind.UsingKeyword);
            ud.StaticKeyword == default(SyntaxToken).Should().BeTrue();
            ud.Alias.Should().BeNull();
            ud.Name.Should().NotBeNull();
            ud.Name.ToString().Should().Be("a.b");
            ud.SemicolonToken.Should().NotBe(default);
        }

        [Fact]
        public void TestUsingStaticDottedName()
        {
            var text = "using static a.b;";
            var file = this.ParseFile(text);

            file.Should().NotBeNull();
            file.Usings.Count.Should().Be(1);
            file.ToString().Should().Be(text);
            file.Errors().Length.Should().Be(0);

            var ud = file.Usings[0];

            ud.UsingKeyword.Should().NotBe(default);
            ud.UsingKeyword.Kind().Should().Be(SyntaxKind.UsingKeyword);
            ud.StaticKeyword.Kind().Should().Be(SyntaxKind.StaticKeyword);
            ud.Alias.Should().BeNull();
            ud.Name.Should().NotBeNull();
            ud.Name.ToString().Should().Be("a.b");
            ud.SemicolonToken.Should().NotBe(default);
        }

        [Fact]
        public void TestUsingStaticGenericName()
        {
            var text = "using static a<int?>;";
            var file = this.ParseFile(text);

            file.Should().NotBeNull();
            file.Usings.Count.Should().Be(1);
            file.ToString().Should().Be(text);
            file.Errors().Length.Should().Be(0);

            var ud = file.Usings[0];

            ud.UsingKeyword.Should().NotBe(default);
            ud.UsingKeyword.Kind().Should().Be(SyntaxKind.UsingKeyword);
            ud.StaticKeyword.Kind().Should().Be(SyntaxKind.StaticKeyword);
            ud.Alias.Should().BeNull();
            ud.Name.Should().NotBeNull();
            ud.Name.ToString().Should().Be("a<int?>");
            ud.SemicolonToken.Should().NotBe(default);
        }

        [Fact]
        public void TestUsingAliasName()
        {
            var text = "using a = b;";
            var file = this.ParseFile(text);

            file.Should().NotBeNull();
            file.Usings.Count.Should().Be(1);
            file.ToString().Should().Be(text);
            file.Errors().Length.Should().Be(0);

            var ud = file.Usings[0];

            ud.UsingKeyword.Should().NotBe(default);
            ud.UsingKeyword.Kind().Should().Be(SyntaxKind.UsingKeyword);
            ud.Alias.Should().NotBeNull();
            ud.Alias.Name.Should().NotBeNull();
            ud.Alias.Name.ToString().Should().Be("a");
            ud.Alias.EqualsToken.Should().NotBe(default);
            ud.Name.Should().NotBeNull();
            ud.Name.ToString().Should().Be("b");
            ud.SemicolonToken.Should().NotBe(default);
        }

        [Fact]
        public void TestUsingAliasGenericName()
        {
            var text = "using a = b<c>;";
            var file = this.ParseFile(text);

            file.Should().NotBeNull();
            file.Usings.Count.Should().Be(1);
            file.ToString().Should().Be(text);
            file.Errors().Length.Should().Be(0);

            var ud = file.Usings[0];

            ud.UsingKeyword.Should().NotBe(default);
            ud.UsingKeyword.Kind().Should().Be(SyntaxKind.UsingKeyword);
            ud.Alias.Should().NotBeNull();
            ud.Alias.Name.Should().NotBeNull();
            ud.Alias.Name.ToString().Should().Be("a");
            ud.Alias.EqualsToken.Should().NotBe(default);
            ud.Name.Should().NotBeNull();
            ud.Name.ToString().Should().Be("b<c>");
            ud.SemicolonToken.Should().NotBe(default);
        }

        [Fact]
        public void TestGlobalAttribute()
        {
            var text = "[assembly:a]";
            var file = this.ParseFile(text);

            file.Should().NotBeNull();
            file.AttributeLists.Count.Should().Be(1);
            file.ToString().Should().Be(text);
            file.Errors().Length.Should().Be(0);

            file.AttributeLists[0].Kind().Should().Be(SyntaxKind.AttributeList);
            var ad = (AttributeListSyntax)file.AttributeLists[0];

            ad.OpenBracketToken.Should().NotBe(default);
            ad.Target.Should().NotBeNull();
            ad.Target.Identifier.Should().NotBe(default);
            ad.Target.Identifier.ToString().Should().Be("assembly");
            ad.Target.ColonToken.Should().NotBe(default);
            ad.Attributes.Count.Should().Be(1);
            ad.Attributes[0].Name.Should().NotBeNull();
            ad.Attributes[0].Name.ToString().Should().Be("a");
            ad.Attributes[0].ArgumentList.Should().BeNull();
            ad.CloseBracketToken.Should().NotBe(default);
        }

        [Fact]
        public void TestGlobalAttribute_Verbatim()
        {
            var text = "[@assembly:a]";
            var file = this.ParseFile(text);

            file.Should().NotBeNull();
            file.AttributeLists.Count.Should().Be(1);
            file.ToString().Should().Be(text);
            file.Errors().Length.Should().Be(0);

            file.AttributeLists[0].Kind().Should().Be(SyntaxKind.AttributeList);
            var ad = (AttributeListSyntax)file.AttributeLists[0];

            ad.OpenBracketToken.Should().NotBe(default);
            ad.Target.Should().NotBeNull();
            ad.Target.Identifier.Should().NotBe(default);
            ad.Target.Identifier.ToString().Should().Be("@assembly");
            ad.Target.Identifier.ValueText.Should().Be("assembly");
            ad.Target.Identifier.Kind().Should().Be(SyntaxKind.IdentifierToken);
            ad.Target.Identifier.ToAttributeLocation().Should().Be(AttributeLocation.Assembly);
            ad.Target.ColonToken.Should().NotBe(default);
            ad.Attributes.Count.Should().Be(1);
            ad.Attributes[0].Name.Should().NotBeNull();
            ad.Attributes[0].Name.ToString().Should().Be("a");
            ad.Attributes[0].ArgumentList.Should().BeNull();
            ad.CloseBracketToken.Should().NotBe(default);
        }

        [Fact]
        public void TestGlobalAttribute_Escape()
        {
            var text = @"[as\u0073embly:a]";
            var file = this.ParseFile(text);

            file.Should().NotBeNull();
            file.AttributeLists.Count.Should().Be(1);
            file.ToString().Should().Be(text);
            file.Errors().Length.Should().Be(0);

            file.AttributeLists[0].Kind().Should().Be(SyntaxKind.AttributeList);
            var ad = (AttributeListSyntax)file.AttributeLists[0];

            ad.OpenBracketToken.Should().NotBe(default);
            ad.Target.Should().NotBeNull();
            ad.Target.Identifier.Should().NotBe(default);
            ad.Target.Identifier.ToString().Should().Be(@"as\u0073embly");
            ad.Target.Identifier.ValueText.Should().Be("assembly");
            ad.Target.Identifier.Kind().Should().Be(SyntaxKind.IdentifierToken);
            ad.Target.Identifier.ToAttributeLocation().Should().Be(AttributeLocation.Assembly);
            ad.Target.ColonToken.Should().NotBe(default);
            ad.Attributes.Count.Should().Be(1);
            ad.Attributes[0].Name.Should().NotBeNull();
            ad.Attributes[0].Name.ToString().Should().Be("a");
            ad.Attributes[0].ArgumentList.Should().BeNull();
            ad.CloseBracketToken.Should().NotBe(default);
        }

        [Fact]
        public void TestGlobalModuleAttribute()
        {
            var text = "[module:a]";
            var file = this.ParseFile(text);

            file.Should().NotBeNull();
            file.AttributeLists.Count.Should().Be(1);
            file.ToString().Should().Be(text);
            file.Errors().Length.Should().Be(0);

            file.AttributeLists[0].Kind().Should().Be(SyntaxKind.AttributeList);
            var ad = (AttributeListSyntax)file.AttributeLists[0];

            ad.OpenBracketToken.Should().NotBe(default);
            ad.Target.Should().NotBeNull();
            ad.Target.Identifier.Should().NotBe(default);
            ad.Target.Identifier.ToString().Should().Be("module");
            ad.Target.Identifier.Kind().Should().Be(SyntaxKind.ModuleKeyword);
            ad.Target.ColonToken.Should().NotBe(default);
            ad.Attributes.Count.Should().Be(1);
            ad.Attributes[0].Name.Should().NotBeNull();
            ad.Attributes[0].Name.ToString().Should().Be("a");
            ad.Attributes[0].ArgumentList.Should().BeNull();
            ad.CloseBracketToken.Should().NotBe(default);
        }

        [Fact]
        public void TestGlobalModuleAttribute_Verbatim()
        {
            var text = "[@module:a]";
            var file = this.ParseFile(text);

            file.Should().NotBeNull();
            file.AttributeLists.Count.Should().Be(1);
            file.ToString().Should().Be(text);
            file.Errors().Length.Should().Be(0);

            file.AttributeLists[0].Kind().Should().Be(SyntaxKind.AttributeList);
            var ad = (AttributeListSyntax)file.AttributeLists[0];

            ad.OpenBracketToken.Should().NotBe(default);
            ad.Target.Should().NotBeNull();
            ad.Target.Identifier.Should().NotBe(default);
            ad.Target.Identifier.ToString().Should().Be("@module");
            ad.Target.Identifier.Kind().Should().Be(SyntaxKind.IdentifierToken);
            ad.Target.Identifier.ToAttributeLocation().Should().Be(AttributeLocation.Module);
            ad.Target.ColonToken.Should().NotBe(default);
            ad.Attributes.Count.Should().Be(1);
            ad.Attributes[0].Name.Should().NotBeNull();
            ad.Attributes[0].Name.ToString().Should().Be("a");
            ad.Attributes[0].ArgumentList.Should().BeNull();
            ad.CloseBracketToken.Should().NotBe(default);
        }

        [Fact]
        public void TestGlobalAttributeWithParentheses()
        {
            var text = "[assembly:a()]";
            var file = this.ParseFile(text);

            file.Should().NotBeNull();
            file.AttributeLists.Count.Should().Be(1);
            file.ToString().Should().Be(text);
            file.Errors().Length.Should().Be(0);

            file.AttributeLists[0].Kind().Should().Be(SyntaxKind.AttributeList);
            var ad = (AttributeListSyntax)file.AttributeLists[0];

            ad.OpenBracketToken.Should().NotBe(default);
            ad.Target.Should().NotBeNull();
            ad.Target.Identifier.Should().NotBe(default);
            ad.Target.Identifier.ToString().Should().Be("assembly");
            ad.Target.Identifier.Kind().Should().Be(SyntaxKind.AssemblyKeyword);
            ad.Target.ColonToken.Should().NotBe(default);
            ad.Attributes.Count.Should().Be(1);
            ad.Attributes[0].Name.Should().NotBeNull();
            ad.Attributes[0].Name.ToString().Should().Be("a");
            ad.Attributes[0].ArgumentList.Should().NotBeNull();
            ad.Attributes[0].ArgumentList.OpenParenToken.Should().NotBe(default);
            ad.Attributes[0].ArgumentList.Arguments.Count.Should().Be(0);
            ad.Attributes[0].ArgumentList.CloseParenToken.Should().NotBe(default);
            ad.CloseBracketToken.Should().NotBe(default);
        }

        [Fact]
        public void TestGlobalAttributeWithMultipleArguments()
        {
            var text = "[assembly:a(b, c)]";
            var file = this.ParseFile(text);

            file.Should().NotBeNull();
            file.AttributeLists.Count.Should().Be(1);
            file.ToString().Should().Be(text);
            file.Errors().Length.Should().Be(0);

            file.AttributeLists[0].Kind().Should().Be(SyntaxKind.AttributeList);
            var ad = (AttributeListSyntax)file.AttributeLists[0];

            ad.OpenBracketToken.Should().NotBe(default);
            ad.Target.Should().NotBeNull();
            ad.Target.Identifier.Should().NotBe(default);
            ad.Target.Identifier.ToString().Should().Be("assembly");
            ad.Target.ColonToken.Should().NotBe(default);
            ad.Attributes.Count.Should().Be(1);
            ad.Attributes[0].Name.Should().NotBeNull();
            ad.Attributes[0].Name.ToString().Should().Be("a");
            ad.Attributes[0].ArgumentList.Should().NotBeNull();
            ad.Attributes[0].ArgumentList.OpenParenToken.Should().NotBe(default);
            ad.Attributes[0].ArgumentList.Arguments.Count.Should().Be(2);
            ad.Attributes[0].ArgumentList.Arguments[0].ToString().Should().Be("b");
            ad.Attributes[0].ArgumentList.Arguments[1].ToString().Should().Be("c");
            ad.Attributes[0].ArgumentList.CloseParenToken.Should().NotBe(default);
            ad.CloseBracketToken.Should().NotBe(default);
        }

        [Fact]
        public void TestGlobalAttributeWithNamedArguments()
        {
            var text = "[assembly:a(b = c)]";
            var file = this.ParseFile(text);

            file.Should().NotBeNull();
            file.AttributeLists.Count.Should().Be(1);
            file.ToString().Should().Be(text);
            file.Errors().Length.Should().Be(0);

            file.AttributeLists[0].Kind().Should().Be(SyntaxKind.AttributeList);
            var ad = (AttributeListSyntax)file.AttributeLists[0];

            ad.OpenBracketToken.Should().NotBe(default);
            ad.Target.Should().NotBeNull();
            ad.Target.Identifier.Should().NotBe(default);
            ad.Target.Identifier.ToString().Should().Be("assembly");
            ad.Target.ColonToken.Should().NotBe(default);
            ad.Attributes.Count.Should().Be(1);
            ad.Attributes[0].Name.Should().NotBeNull();
            ad.Attributes[0].Name.ToString().Should().Be("a");
            ad.Attributes[0].ArgumentList.Should().NotBeNull();
            ad.Attributes[0].ArgumentList.OpenParenToken.Should().NotBe(default);
            ad.Attributes[0].ArgumentList.Arguments.Count.Should().Be(1);
            ad.Attributes[0].ArgumentList.Arguments[0].ToString().Should().Be("b = c");
            ad.Attributes[0].ArgumentList.Arguments[0].NameEquals.Should().NotBeNull();
            ad.Attributes[0].ArgumentList.Arguments[0].NameEquals.Name.Should().NotBeNull();
            ad.Attributes[0].ArgumentList.Arguments[0].NameEquals.Name.ToString().Should().Be("b");
            ad.Attributes[0].ArgumentList.Arguments[0].NameEquals.EqualsToken.Should().NotBe(default);
            ad.Attributes[0].ArgumentList.Arguments[0].Expression.Should().NotBeNull();
            ad.Attributes[0].ArgumentList.Arguments[0].Expression.ToString().Should().Be("c");
            ad.Attributes[0].ArgumentList.CloseParenToken.Should().NotBe(default);
            ad.CloseBracketToken.Should().NotBe(default);
        }

        [Fact]
        public void TestGlobalAttributeWithMultipleAttributes()
        {
            var text = "[assembly:a, b]";
            var file = this.ParseFile(text);

            file.Should().NotBeNull();
            file.AttributeLists.Count.Should().Be(1);
            file.ToString().Should().Be(text);
            file.Errors().Length.Should().Be(0);

            file.AttributeLists[0].Kind().Should().Be(SyntaxKind.AttributeList);
            var ad = (AttributeListSyntax)file.AttributeLists[0];

            ad.OpenBracketToken.Should().NotBe(default);
            ad.Target.Should().NotBeNull();
            ad.Target.Identifier.Should().NotBe(default);
            ad.Target.Identifier.ToString().Should().Be("assembly");
            ad.Target.ColonToken.Should().NotBe(default);
            ad.Attributes.Count.Should().Be(2);

            ad.Attributes[0].Name.Should().NotBeNull();
            ad.Attributes[0].Name.ToString().Should().Be("a");
            ad.Attributes[0].ArgumentList.Should().BeNull();

            ad.Attributes[1].Name.Should().NotBeNull();
            ad.Attributes[1].Name.ToString().Should().Be("b");
            ad.Attributes[1].ArgumentList.Should().BeNull();

            ad.CloseBracketToken.Should().NotBe(default);
        }

        [Fact]
        public void TestMultipleGlobalAttributeDeclarations()
        {
            var text = "[assembly:a] [assembly:b]";
            var file = this.ParseFile(text);

            file.Should().NotBeNull();
            file.AttributeLists.Count.Should().Be(2);
            file.ToString().Should().Be(text);
            file.Errors().Length.Should().Be(0);

            file.AttributeLists[0].Kind().Should().Be(SyntaxKind.AttributeList);
            var ad = (AttributeListSyntax)file.AttributeLists[0];
            ad.OpenBracketToken.Should().NotBe(default);
            ad.Target.Should().NotBeNull();
            ad.Target.Identifier.Should().NotBe(default);
            ad.Target.Identifier.ToString().Should().Be("assembly");
            ad.Target.ColonToken.Should().NotBe(default);
            ad.Attributes.Count.Should().Be(1);
            ad.Attributes[0].Name.Should().NotBeNull();
            ad.Attributes[0].Name.ToString().Should().Be("a");
            ad.Attributes[0].ArgumentList.Should().BeNull();
            ad.CloseBracketToken.Should().NotBe(default);

            ad = (AttributeListSyntax)file.AttributeLists[1];
            ad.OpenBracketToken.Should().NotBe(default);
            ad.Target.Should().NotBeNull();
            ad.Target.Identifier.Should().NotBe(default);
            ad.Target.Identifier.ToString().Should().Be("assembly");
            ad.Target.ColonToken.Should().NotBe(default);
            ad.Attributes.Count.Should().Be(1);
            ad.Attributes[0].Name.Should().NotBeNull();
            ad.Attributes[0].Name.ToString().Should().Be("b");
            ad.Attributes[0].ArgumentList.Should().BeNull();
            ad.CloseBracketToken.Should().NotBe(default);
        }

        [Fact]
        public void TestNamespace()
        {
            var text = "namespace a { }";
            var file = this.ParseFile(text);

            file.Should().NotBeNull();
            file.Members.Count.Should().Be(1);
            file.ToString().Should().Be(text);
            file.Errors().Length.Should().Be(0);

            file.Members[0].Kind().Should().Be(SyntaxKind.NamespaceDeclaration);
            var ns = (NamespaceDeclarationSyntax)file.Members[0];
            ns.NamespaceKeyword.Should().NotBe(default);
            ns.Name.Should().NotBeNull();
            ns.Name.ToString().Should().Be("a");
            ns.OpenBraceToken.Should().NotBe(default);
            ns.Usings.Count.Should().Be(0);
            ns.Members.Count.Should().Be(0);
            ns.CloseBraceToken.Should().NotBe(default);
        }

        [Fact]
        public void TestFileScopedNamespace()
        {
            var text = "namespace a;";
            var file = this.ParseFile(text, CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Preview));

            file.Should().NotBeNull();
            file.Members.Count.Should().Be(1);
            file.ToString().Should().Be(text);
            file.Errors().Length.Should().Be(0);

            file.Members[0].Kind().Should().Be(SyntaxKind.FileScopedNamespaceDeclaration);
            var ns = (FileScopedNamespaceDeclarationSyntax)file.Members[0];
            ns.NamespaceKeyword.Should().NotBe(default);
            ns.Name.Should().NotBeNull();
            ns.Name.ToString().Should().Be("a");
            ns.SemicolonToken.Should().NotBe(default);
            ns.Usings.Count.Should().Be(0);
            ns.Members.Count.Should().Be(0);
        }

        [Fact]
        public void TestNamespaceWithDottedName()
        {
            var text = "namespace a.b.c { }";
            var file = this.ParseFile(text);

            file.Should().NotBeNull();
            file.Members.Count.Should().Be(1);
            file.ToString().Should().Be(text);
            file.Errors().Length.Should().Be(0);

            file.Members[0].Kind().Should().Be(SyntaxKind.NamespaceDeclaration);
            var ns = (NamespaceDeclarationSyntax)file.Members[0];
            ns.NamespaceKeyword.Should().NotBe(default);
            ns.Name.Should().NotBeNull();
            ns.Name.ToString().Should().Be("a.b.c");
            ns.OpenBraceToken.Should().NotBe(default);
            ns.Usings.Count.Should().Be(0);
            ns.Members.Count.Should().Be(0);
            ns.CloseBraceToken.Should().NotBe(default);
        }

        [Fact]
        public void TestNamespaceWithUsing()
        {
            var text = "namespace a { using b.c; }";
            var file = this.ParseFile(text);

            file.Should().NotBeNull();
            file.Members.Count.Should().Be(1);
            file.ToString().Should().Be(text);
            file.Errors().Length.Should().Be(0);

            file.Members[0].Kind().Should().Be(SyntaxKind.NamespaceDeclaration);
            var ns = (NamespaceDeclarationSyntax)file.Members[0];
            ns.NamespaceKeyword.Should().NotBe(default);
            ns.Name.Should().NotBeNull();
            ns.Name.ToString().Should().Be("a");
            ns.OpenBraceToken.Should().NotBe(default);
            ns.Usings.Count.Should().Be(1);
            ns.Usings[0].ToString().Should().Be("using b.c;");
            ns.Members.Count.Should().Be(0);
            ns.CloseBraceToken.Should().NotBe(default);
        }

        [Fact]
        public void TestFileScopedNamespaceWithUsing()
        {
            var text = "namespace a; using b.c;";
            var file = this.ParseFile(text, CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Preview));

            file.Should().NotBeNull();
            file.Members.Count.Should().Be(1);
            file.ToString().Should().Be(text);
            file.Errors().Length.Should().Be(0);

            file.Members[0].Kind().Should().Be(SyntaxKind.FileScopedNamespaceDeclaration);
            var ns = (FileScopedNamespaceDeclarationSyntax)file.Members[0];
            ns.NamespaceKeyword.Should().NotBe(default);
            ns.Name.Should().NotBeNull();
            ns.Name.ToString().Should().Be("a");
            ns.SemicolonToken.Should().NotBe(default);
            ns.Usings.Count.Should().Be(1);
            ns.Usings[0].ToString().Should().Be("using b.c;");
            ns.Members.Count.Should().Be(0);
        }

        [Fact]
        public void TestNamespaceWithExternAlias()
        {
            var text = "namespace a { extern alias b; }";
            var file = this.ParseFile(text);

            file.Should().NotBeNull();
            file.Members.Count.Should().Be(1);
            file.ToString().Should().Be(text);
            file.Errors().Length.Should().Be(0);

            file.Members[0].Kind().Should().Be(SyntaxKind.NamespaceDeclaration);
            var ns = (NamespaceDeclarationSyntax)file.Members[0];
            ns.NamespaceKeyword.Should().NotBe(default);
            ns.Name.Should().NotBeNull();
            ns.Name.ToString().Should().Be("a");
            ns.OpenBraceToken.Should().NotBe(default);
            ns.Externs.Count.Should().Be(1);
            ns.Externs[0].ToString().Should().Be("extern alias b;");
            ns.Members.Count.Should().Be(0);
            ns.CloseBraceToken.Should().NotBe(default);
        }

        [Fact]
        public void TestFileScopedNamespaceWithExternAlias()
        {
            var text = "namespace a; extern alias b;";
            var file = this.ParseFile(text, CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Preview));

            file.Should().NotBeNull();
            file.Members.Count.Should().Be(1);
            file.ToString().Should().Be(text);
            file.Errors().Length.Should().Be(0);

            file.Members[0].Kind().Should().Be(SyntaxKind.FileScopedNamespaceDeclaration);
            var ns = (FileScopedNamespaceDeclarationSyntax)file.Members[0];
            ns.NamespaceKeyword.Should().NotBe(default);
            ns.Name.Should().NotBeNull();
            ns.Name.ToString().Should().Be("a");
            ns.SemicolonToken.Should().NotBe(default);
            ns.Externs.Count.Should().Be(1);
            ns.Externs[0].ToString().Should().Be("extern alias b;");
            ns.Members.Count.Should().Be(0);
        }

        [Fact]
        public void TestNamespaceWithExternAliasFollowingUsingBad()
        {
            var text = "namespace a { using b; extern alias c; }";
            var file = this.ParseFile(text);

            file.Should().NotBeNull();
            file.Members.Count.Should().Be(1);
            file.ToFullString().Should().Be(text);
            file.Errors().Length.Should().Be(1);

            file.Members[0].Kind().Should().Be(SyntaxKind.NamespaceDeclaration);
            var ns = (NamespaceDeclarationSyntax)file.Members[0];
            ns.NamespaceKeyword.Should().NotBe(default);
            ns.Name.Should().NotBeNull();
            ns.Name.ToString().Should().Be("a");
            ns.OpenBraceToken.Should().NotBe(default);
            ns.Usings.Count.Should().Be(1);
            ns.Usings[0].ToString().Should().Be("using b;");
            ns.Externs.Count.Should().Be(0);
            ns.Members.Count.Should().Be(0);
            ns.CloseBraceToken.Should().NotBe(default);
        }

        [Fact]
        public void TestNamespaceWithNestedNamespace()
        {
            var text = "namespace a { namespace b { } }";
            var file = this.ParseFile(text);

            file.Should().NotBeNull();
            file.Members.Count.Should().Be(1);
            file.ToString().Should().Be(text);
            file.Errors().Length.Should().Be(0);

            file.Members[0].Kind().Should().Be(SyntaxKind.NamespaceDeclaration);
            var ns = (NamespaceDeclarationSyntax)file.Members[0];
            ns.NamespaceKeyword.Should().NotBe(default);
            ns.Name.Should().NotBeNull();
            ns.Name.ToString().Should().Be("a");
            ns.OpenBraceToken.Should().NotBe(default);
            ns.Usings.Count.Should().Be(0);
            ns.Members.Count.Should().Be(1);
            ns.Members[0].Kind().Should().Be(SyntaxKind.NamespaceDeclaration);
            var ns2 = (NamespaceDeclarationSyntax)ns.Members[0];
            ns2.NamespaceKeyword.Should().NotBe(default);
            ns2.Name.Should().NotBeNull();
            ns2.Name.ToString().Should().Be("b");
            ns2.OpenBraceToken.Should().NotBe(default);
            ns2.Usings.Count.Should().Be(0);
            ns2.Members.Count.Should().Be(0);

            ns.CloseBraceToken.Should().NotBe(default);
        }

        [Fact]
        public void TestClass()
        {
            var text = "class a { }";
            var file = this.ParseFile(text);

            file.Should().NotBeNull();
            file.Members.Count.Should().Be(1);
            file.ToString().Should().Be(text);
            file.Errors().Length.Should().Be(0);

            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var cs = (TypeDeclarationSyntax)file.Members[0];
            cs.AttributeLists.Count.Should().Be(0);
            cs.Modifiers.Count.Should().Be(0);
            cs.Keyword.Should().NotBe(default);
            cs.Keyword.Kind().Should().Be(SyntaxKind.ClassKeyword);
            cs.Identifier.Should().NotBe(default);
            cs.Identifier.ToString().Should().Be("a");
            cs.BaseList.Should().BeNull();
            cs.ConstraintClauses.Count.Should().Be(0);
            cs.OpenBraceToken.Should().NotBe(default);
            cs.Members.Count.Should().Be(0);
            cs.CloseBraceToken.Should().NotBe(default);
        }

        [Fact]
        public void TestClassWithPublic()
        {
            var text = "public class a { }";
            var file = this.ParseFile(text);

            file.Should().NotBeNull();
            file.Members.Count.Should().Be(1);
            file.ToString().Should().Be(text);
            file.Errors().Length.Should().Be(0);

            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var cs = (TypeDeclarationSyntax)file.Members[0];
            cs.AttributeLists.Count.Should().Be(0);
            cs.Modifiers.Count.Should().Be(1);
            cs.Modifiers[0].Kind().Should().Be(SyntaxKind.PublicKeyword);
            cs.Keyword.Should().NotBe(default);
            cs.Keyword.Kind().Should().Be(SyntaxKind.ClassKeyword);
            cs.Identifier.Should().NotBe(default);
            cs.Identifier.ToString().Should().Be("a");
            cs.BaseList.Should().BeNull();
            cs.ConstraintClauses.Count.Should().Be(0);
            cs.OpenBraceToken.Should().NotBe(default);
            cs.Members.Count.Should().Be(0);
            cs.CloseBraceToken.Should().NotBe(default);
        }

        [Fact]
        public void TestClassWithInternal()
        {
            var text = "internal class a { }";
            var file = this.ParseFile(text);

            file.Should().NotBeNull();
            file.Members.Count.Should().Be(1);
            file.ToString().Should().Be(text);
            file.Errors().Length.Should().Be(0);

            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var cs = (TypeDeclarationSyntax)file.Members[0];
            cs.AttributeLists.Count.Should().Be(0);
            cs.Modifiers.Count.Should().Be(1);
            cs.Modifiers[0].Kind().Should().Be(SyntaxKind.InternalKeyword);
            cs.Keyword.Should().NotBe(default);
            cs.Keyword.Kind().Should().Be(SyntaxKind.ClassKeyword);
            cs.Identifier.Should().NotBe(default);
            cs.Identifier.ToString().Should().Be("a");
            cs.BaseList.Should().BeNull();
            cs.ConstraintClauses.Count.Should().Be(0);
            cs.OpenBraceToken.Should().NotBe(default);
            cs.Members.Count.Should().Be(0);
            cs.CloseBraceToken.Should().NotBe(default);
        }

        [Fact]
        public void TestClassWithStatic()
        {
            var text = "static class a { }";
            var file = this.ParseFile(text);

            file.Should().NotBeNull();
            file.Members.Count.Should().Be(1);
            file.ToString().Should().Be(text);
            file.Errors().Length.Should().Be(0);

            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var cs = (TypeDeclarationSyntax)file.Members[0];
            cs.AttributeLists.Count.Should().Be(0);
            cs.Modifiers.Count.Should().Be(1);
            cs.Modifiers[0].Kind().Should().Be(SyntaxKind.StaticKeyword);
            cs.Keyword.Should().NotBe(default);
            cs.Keyword.Kind().Should().Be(SyntaxKind.ClassKeyword);
            cs.Identifier.Should().NotBe(default);
            cs.Identifier.ToString().Should().Be("a");
            cs.BaseList.Should().BeNull();
            cs.ConstraintClauses.Count.Should().Be(0);
            cs.OpenBraceToken.Should().NotBe(default);
            cs.Members.Count.Should().Be(0);
            cs.CloseBraceToken.Should().NotBe(default);
        }

        [Fact]
        public void TestClassWithSealed()
        {
            var text = "sealed class a { }";
            var file = this.ParseFile(text);

            file.Should().NotBeNull();
            file.Members.Count.Should().Be(1);
            file.ToString().Should().Be(text);
            file.Errors().Length.Should().Be(0);

            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var cs = (TypeDeclarationSyntax)file.Members[0];
            cs.AttributeLists.Count.Should().Be(0);
            cs.Modifiers.Count.Should().Be(1);
            cs.Modifiers[0].Kind().Should().Be(SyntaxKind.SealedKeyword);
            cs.Keyword.Should().NotBe(default);
            cs.Keyword.Kind().Should().Be(SyntaxKind.ClassKeyword);
            cs.Identifier.Should().NotBe(default);
            cs.Identifier.ToString().Should().Be("a");
            cs.BaseList.Should().BeNull();
            cs.ConstraintClauses.Count.Should().Be(0);
            cs.OpenBraceToken.Should().NotBe(default);
            cs.Members.Count.Should().Be(0);
            cs.CloseBraceToken.Should().NotBe(default);
        }

        [Fact]
        public void TestClassWithAbstract()
        {
            var text = "abstract class a { }";
            var file = this.ParseFile(text);

            file.Should().NotBeNull();
            file.Members.Count.Should().Be(1);
            file.ToString().Should().Be(text);
            file.Errors().Length.Should().Be(0);

            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var cs = (TypeDeclarationSyntax)file.Members[0];
            cs.AttributeLists.Count.Should().Be(0);
            cs.Modifiers.Count.Should().Be(1);
            cs.Modifiers[0].Kind().Should().Be(SyntaxKind.AbstractKeyword);
            cs.Keyword.Should().NotBe(default);
            cs.Keyword.Kind().Should().Be(SyntaxKind.ClassKeyword);
            cs.Identifier.Should().NotBe(default);
            cs.Identifier.ToString().Should().Be("a");
            cs.BaseList.Should().BeNull();
            cs.ConstraintClauses.Count.Should().Be(0);
            cs.OpenBraceToken.Should().NotBe(default);
            cs.Members.Count.Should().Be(0);
            cs.CloseBraceToken.Should().NotBe(default);
        }

        [Fact]
        public void TestClassWithPartial()
        {
            var text = "partial class a { }";
            var file = this.ParseFile(text);

            file.Should().NotBeNull();
            file.Members.Count.Should().Be(1);
            file.ToString().Should().Be(text);
            file.Errors().Length.Should().Be(0);

            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var cs = (TypeDeclarationSyntax)file.Members[0];
            cs.AttributeLists.Count.Should().Be(0);
            cs.Modifiers.Count.Should().Be(1);
            cs.Modifiers[0].Kind().Should().Be(SyntaxKind.PartialKeyword);
            cs.Keyword.Should().NotBe(default);
            cs.Keyword.Kind().Should().Be(SyntaxKind.ClassKeyword);
            cs.Identifier.Should().NotBe(default);
            cs.Identifier.ToString().Should().Be("a");
            cs.BaseList.Should().BeNull();
            cs.ConstraintClauses.Count.Should().Be(0);
            cs.OpenBraceToken.Should().NotBe(default);
            cs.Members.Count.Should().Be(0);
            cs.CloseBraceToken.Should().NotBe(default);
        }

        [Fact]
        public void TestClassWithAttribute()
        {
            var text = "[attr] class a { }";
            var file = this.ParseFile(text);

            file.Should().NotBeNull();
            file.Members.Count.Should().Be(1);
            file.ToString().Should().Be(text);
            file.Errors().Length.Should().Be(0);

            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var cs = (TypeDeclarationSyntax)file.Members[0];
            cs.AttributeLists.Count.Should().Be(1);
            cs.AttributeLists[0].ToString().Should().Be("[attr]");
            cs.Modifiers.Count.Should().Be(0);
            cs.Keyword.Should().NotBe(default);
            cs.Keyword.Kind().Should().Be(SyntaxKind.ClassKeyword);
            cs.Identifier.Should().NotBe(default);
            cs.Identifier.ToString().Should().Be("a");
            cs.BaseList.Should().BeNull();
            cs.ConstraintClauses.Count.Should().Be(0);
            cs.OpenBraceToken.Should().NotBe(default);
            cs.Members.Count.Should().Be(0);
            cs.CloseBraceToken.Should().NotBe(default);
        }

        [Fact]
        public void TestClassWithMultipleAttributes()
        {
            var text = "[attr1] [attr2] class a { }";
            var file = this.ParseFile(text);

            file.Should().NotBeNull();
            file.Members.Count.Should().Be(1);
            file.ToString().Should().Be(text);
            file.Errors().Length.Should().Be(0);

            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var cs = (TypeDeclarationSyntax)file.Members[0];
            cs.AttributeLists.Count.Should().Be(2);
            cs.AttributeLists[0].ToString().Should().Be("[attr1]");
            cs.AttributeLists[1].ToString().Should().Be("[attr2]");
            cs.Modifiers.Count.Should().Be(0);
            cs.Keyword.Should().NotBe(default);
            cs.Keyword.Kind().Should().Be(SyntaxKind.ClassKeyword);
            cs.Identifier.Should().NotBe(default);
            cs.Identifier.ToString().Should().Be("a");
            cs.BaseList.Should().BeNull();
            cs.ConstraintClauses.Count.Should().Be(0);
            cs.OpenBraceToken.Should().NotBe(default);
            cs.Members.Count.Should().Be(0);
            cs.CloseBraceToken.Should().NotBe(default);
        }

        [Fact]
        public void TestClassWithMultipleAttributesInAList()
        {
            var text = "[attr1, attr2] class a { }";
            var file = this.ParseFile(text);

            file.Should().NotBeNull();
            file.Members.Count.Should().Be(1);
            file.ToString().Should().Be(text);
            file.Errors().Length.Should().Be(0);

            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var cs = (TypeDeclarationSyntax)file.Members[0];
            cs.AttributeLists.Count.Should().Be(1);
            cs.AttributeLists[0].ToString().Should().Be("[attr1, attr2]");
            cs.Modifiers.Count.Should().Be(0);
            cs.Keyword.Should().NotBe(default);
            cs.Keyword.Kind().Should().Be(SyntaxKind.ClassKeyword);
            cs.Identifier.Should().NotBe(default);
            cs.Identifier.ToString().Should().Be("a");
            cs.BaseList.Should().BeNull();
            cs.ConstraintClauses.Count.Should().Be(0);
            cs.OpenBraceToken.Should().NotBe(default);
            cs.Members.Count.Should().Be(0);
            cs.CloseBraceToken.Should().NotBe(default);
        }

        [Fact]
        public void TestClassWithBaseType()
        {
            var text = "class a : b { }";
            var file = this.ParseFile(text);

            file.Should().NotBeNull();
            file.Members.Count.Should().Be(1);
            file.ToString().Should().Be(text);
            file.Errors().Length.Should().Be(0);

            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var cs = (TypeDeclarationSyntax)file.Members[0];
            cs.AttributeLists.Count.Should().Be(0);
            cs.Modifiers.Count.Should().Be(0);
            cs.Keyword.Should().NotBe(default);
            cs.Keyword.Kind().Should().Be(SyntaxKind.ClassKeyword);
            cs.Identifier.Should().NotBe(default);
            cs.Identifier.ToString().Should().Be("a");

            cs.BaseList.Should().NotBeNull();
            cs.BaseList.ColonToken.Should().NotBe(default);
            cs.BaseList.Types.Count.Should().Be(1);
            cs.BaseList.Types[0].Type.ToString().Should().Be("b");

            cs.ConstraintClauses.Count.Should().Be(0);
            cs.OpenBraceToken.Should().NotBe(default);
            cs.Members.Count.Should().Be(0);
            cs.CloseBraceToken.Should().NotBe(default);
        }

        [Fact]
        public void TestClassWithMultipleBases()
        {
            var text = "class a : b, c { }";
            var file = this.ParseFile(text);

            file.Should().NotBeNull();
            file.Members.Count.Should().Be(1);
            file.ToString().Should().Be(text);
            file.Errors().Length.Should().Be(0);

            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var cs = (TypeDeclarationSyntax)file.Members[0];
            cs.AttributeLists.Count.Should().Be(0);
            cs.Modifiers.Count.Should().Be(0);
            cs.Keyword.Should().NotBe(default);
            cs.Keyword.Kind().Should().Be(SyntaxKind.ClassKeyword);
            cs.Identifier.Should().NotBe(default);
            cs.Identifier.ToString().Should().Be("a");

            cs.BaseList.Should().NotBeNull();
            cs.BaseList.ColonToken.Should().NotBe(default);
            cs.BaseList.Types.Count.Should().Be(2);
            cs.BaseList.Types[0].Type.ToString().Should().Be("b");
            cs.BaseList.Types[1].Type.ToString().Should().Be("c");

            cs.ConstraintClauses.Count.Should().Be(0);
            cs.OpenBraceToken.Should().NotBe(default);
            cs.Members.Count.Should().Be(0);
            cs.CloseBraceToken.Should().NotBe(default);
        }

        [Fact]
        public void TestClassWithTypeConstraintBound()
        {
            var text = "class a<b> where b : c { }";
            var file = this.ParseFile(text);

            file.Should().NotBeNull();
            file.Members.Count.Should().Be(1);
            file.ToString().Should().Be(text);
            file.Errors().Length.Should().Be(0);

            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var cs = (TypeDeclarationSyntax)file.Members[0];
            cs.AttributeLists.Count.Should().Be(0);
            cs.Modifiers.Count.Should().Be(0);
            cs.Keyword.Should().NotBe(default);
            cs.Keyword.Kind().Should().Be(SyntaxKind.ClassKeyword);
            cs.Identifier.Should().NotBe(default);
            cs.Identifier.ToString().Should().Be("a");
            cs.TypeParameterList.ToString().Should().Be("<b>");

            cs.BaseList.Should().BeNull();

            cs.ConstraintClauses.Count.Should().Be(1);
            cs.ConstraintClauses[0].WhereKeyword.Should().NotBe(default);
            cs.ConstraintClauses[0].Name.Should().NotBeNull();
            cs.ConstraintClauses[0].Name.ToString().Should().Be("b");
            cs.ConstraintClauses[0].ColonToken.Should().NotBe(default);
            cs.ConstraintClauses[0].ColonToken.IsMissing.Should().BeFalse();
            cs.ConstraintClauses[0].Constraints.Count.Should().Be(1);
            cs.ConstraintClauses[0].Constraints[0].Kind().Should().Be(SyntaxKind.TypeConstraint);
            var bound = (TypeConstraintSyntax)cs.ConstraintClauses[0].Constraints[0];
            bound.Type.Should().NotBeNull();
            bound.Type.ToString().Should().Be("c");

            cs.OpenBraceToken.Should().NotBe(default);
            cs.Members.Count.Should().Be(0);
            cs.CloseBraceToken.Should().NotBe(default);
        }

        [Fact]
        public void TestNonGenericClassWithTypeConstraintBound()
        {
            var text = "class a where b : c { }";
            var file = this.ParseFile(text);

            file.Should().NotBeNull();
            file.Members.Count.Should().Be(1);
            file.ToString().Should().Be(text);

            var errors = file.Errors();
            errors.Length.Should().Be(0);

            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var cs = (TypeDeclarationSyntax)file.Members[0];
            cs.AttributeLists.Count.Should().Be(0);
            cs.Modifiers.Count.Should().Be(0);
            cs.Keyword.Should().NotBe(default);
            cs.Keyword.Kind().Should().Be(SyntaxKind.ClassKeyword);
            cs.Identifier.Should().NotBe(default);
            cs.Identifier.ToString().Should().Be("a");

            cs.BaseList.Should().BeNull();

            cs.ConstraintClauses.Count.Should().Be(1);
            cs.ConstraintClauses[0].WhereKeyword.Should().NotBe(default);
            cs.ConstraintClauses[0].Name.Should().NotBeNull();
            cs.ConstraintClauses[0].Name.ToString().Should().Be("b");
            cs.ConstraintClauses[0].ColonToken.Should().NotBe(default);
            cs.ConstraintClauses[0].ColonToken.IsMissing.Should().BeFalse();
            cs.ConstraintClauses[0].Constraints.Count.Should().Be(1);
            cs.ConstraintClauses[0].Constraints[0].Kind().Should().Be(SyntaxKind.TypeConstraint);
            var bound = (TypeConstraintSyntax)cs.ConstraintClauses[0].Constraints[0];
            bound.Type.Should().NotBeNull();
            bound.Type.ToString().Should().Be("c");

            cs.OpenBraceToken.Should().NotBe(default);
            cs.Members.Count.Should().Be(0);
            cs.CloseBraceToken.Should().NotBe(default);

            CreateCompilation(text).GetDeclarationDiagnostics().Verify(
                // (1,7): warning CS8981: The type name 'a' only contains lower-cased ascii characters. Such names may become reserved for the language.
                // class a where b : c { }
                Diagnostic(ErrorCode.WRN_LowerCaseTypeName, "a").WithArguments("a").WithLocation(1, 7),
                // (1,9): error CS0080: Constraints are not allowed on non-generic declarations
                // class a where b : c { }
                Diagnostic(ErrorCode.ERR_ConstraintOnlyAllowedOnGenericDecl, "where").WithLocation(1, 9));
        }

        [Fact]
        public void TestNonGenericMethodWithTypeConstraintBound()
        {
            var text = "class a { void M() where b : c { } }";

            CreateCompilation(text).GetDeclarationDiagnostics().Verify(
                // (1,7): warning CS8981: The type name 'a' only contains lower-cased ascii characters. Such names may become reserved for the language.
                // class a { void M() where b : c { } }
                Diagnostic(ErrorCode.WRN_LowerCaseTypeName, "a").WithArguments("a").WithLocation(1, 7),
                // (1,20): error CS0080: Constraints are not allowed on non-generic declarations
                // class a { void M() where b : c { } }
                Diagnostic(ErrorCode.ERR_ConstraintOnlyAllowedOnGenericDecl, "where").WithLocation(1, 20));
        }

        [Fact]
        public void TestClassWithNewConstraintBound()
        {
            var text = "class a<b> where b : new() { }";
            var file = this.ParseFile(text);

            file.Should().NotBeNull();
            file.Members.Count.Should().Be(1);
            file.ToString().Should().Be(text);
            file.Errors().Length.Should().Be(0);

            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var cs = (TypeDeclarationSyntax)file.Members[0];
            cs.AttributeLists.Count.Should().Be(0);
            cs.Modifiers.Count.Should().Be(0);
            cs.Keyword.Should().NotBe(default);
            cs.Keyword.Kind().Should().Be(SyntaxKind.ClassKeyword);
            cs.Identifier.Should().NotBe(default);
            cs.Identifier.ToString().Should().Be("a");
            cs.TypeParameterList.ToString().Should().Be("<b>");

            cs.BaseList.Should().BeNull();

            cs.ConstraintClauses.Count.Should().Be(1);
            cs.ConstraintClauses[0].WhereKeyword.Should().NotBe(default);
            cs.ConstraintClauses[0].Name.Should().NotBeNull();
            cs.ConstraintClauses[0].Name.ToString().Should().Be("b");
            cs.ConstraintClauses[0].ColonToken.Should().NotBe(default);
            cs.ConstraintClauses[0].ColonToken.IsMissing.Should().BeFalse();
            cs.ConstraintClauses[0].Constraints.Count.Should().Be(1);
            cs.ConstraintClauses[0].Constraints[0].Kind().Should().Be(SyntaxKind.ConstructorConstraint);
            var bound = (ConstructorConstraintSyntax)cs.ConstraintClauses[0].Constraints[0];
            bound.NewKeyword.Should().NotBe(default);
            bound.NewKeyword.IsMissing.Should().BeFalse();
            bound.OpenParenToken.Should().NotBe(default);
            bound.OpenParenToken.IsMissing.Should().BeFalse();
            bound.CloseParenToken.Should().NotBe(default);
            bound.CloseParenToken.IsMissing.Should().BeFalse();

            cs.OpenBraceToken.Should().NotBe(default);
            cs.Members.Count.Should().Be(0);
            cs.CloseBraceToken.Should().NotBe(default);
        }

        [Fact]
        public void TestClassWithClassConstraintBound()
        {
            var text = "class a<b> where b : class { }";
            var file = this.ParseFile(text);

            file.Should().NotBeNull();
            file.Members.Count.Should().Be(1);
            file.ToString().Should().Be(text);
            file.Errors().Length.Should().Be(0);

            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var cs = (TypeDeclarationSyntax)file.Members[0];
            cs.AttributeLists.Count.Should().Be(0);
            cs.Modifiers.Count.Should().Be(0);
            cs.Keyword.Should().NotBe(default);
            cs.Keyword.Kind().Should().Be(SyntaxKind.ClassKeyword);
            cs.Identifier.Should().NotBe(default);
            cs.Identifier.ToString().Should().Be("a");
            cs.TypeParameterList.ToString().Should().Be("<b>");

            cs.BaseList.Should().BeNull();

            cs.ConstraintClauses.Count.Should().Be(1);
            cs.ConstraintClauses[0].WhereKeyword.Should().NotBe(default);
            cs.ConstraintClauses[0].Name.Should().NotBeNull();
            cs.ConstraintClauses[0].Name.ToString().Should().Be("b");
            cs.ConstraintClauses[0].ColonToken.Should().NotBe(default);
            cs.ConstraintClauses[0].ColonToken.IsMissing.Should().BeFalse();
            cs.ConstraintClauses[0].Constraints.Count.Should().Be(1);
            cs.ConstraintClauses[0].Constraints[0].Kind().Should().Be(SyntaxKind.ClassConstraint);
            var bound = (ClassOrStructConstraintSyntax)cs.ConstraintClauses[0].Constraints[0];
            bound.ClassOrStructKeyword.Should().NotBe(default);
            bound.ClassOrStructKeyword.IsMissing.Should().BeFalse();
            bound.ClassOrStructKeyword.Kind().Should().Be(SyntaxKind.ClassKeyword);

            cs.OpenBraceToken.Should().NotBe(default);
            cs.Members.Count.Should().Be(0);
            cs.CloseBraceToken.Should().NotBe(default);
        }

        [Fact]
        public void TestClassWithStructConstraintBound()
        {
            var text = "class a<b> where b : struct { }";
            var file = this.ParseFile(text);

            file.Should().NotBeNull();
            file.Members.Count.Should().Be(1);
            file.ToString().Should().Be(text);
            file.Errors().Length.Should().Be(0);

            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var cs = (TypeDeclarationSyntax)file.Members[0];
            cs.AttributeLists.Count.Should().Be(0);
            cs.Modifiers.Count.Should().Be(0);
            cs.Keyword.Should().NotBe(default);
            cs.Keyword.Kind().Should().Be(SyntaxKind.ClassKeyword);
            cs.Identifier.Should().NotBe(default);
            cs.Identifier.ToString().Should().Be("a");
            cs.TypeParameterList.ToString().Should().Be("<b>");

            cs.BaseList.Should().BeNull();

            cs.ConstraintClauses.Count.Should().Be(1);
            cs.ConstraintClauses[0].WhereKeyword.Should().NotBe(default);
            cs.ConstraintClauses[0].Name.Should().NotBeNull();
            cs.ConstraintClauses[0].Name.ToString().Should().Be("b");
            cs.ConstraintClauses[0].ColonToken.Should().NotBe(default);
            cs.ConstraintClauses[0].ColonToken.IsMissing.Should().BeFalse();
            cs.ConstraintClauses[0].Constraints.Count.Should().Be(1);
            cs.ConstraintClauses[0].Constraints[0].Kind().Should().Be(SyntaxKind.StructConstraint);
            var bound = (ClassOrStructConstraintSyntax)cs.ConstraintClauses[0].Constraints[0];
            bound.ClassOrStructKeyword.Should().NotBe(default);
            bound.ClassOrStructKeyword.IsMissing.Should().BeFalse();
            bound.ClassOrStructKeyword.Kind().Should().Be(SyntaxKind.StructKeyword);

            cs.OpenBraceToken.Should().NotBe(default);
            cs.Members.Count.Should().Be(0);
            cs.CloseBraceToken.Should().NotBe(default);
        }

        [Fact]
        public void TestClassWithMultipleConstraintBounds()
        {
            var text = "class a<b> where b : class, c, new() { }";
            var file = this.ParseFile(text);

            file.Should().NotBeNull();
            file.Members.Count.Should().Be(1);
            file.ToString().Should().Be(text);
            file.Errors().Length.Should().Be(0);

            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var cs = (TypeDeclarationSyntax)file.Members[0];
            cs.AttributeLists.Count.Should().Be(0);
            cs.Modifiers.Count.Should().Be(0);
            cs.Keyword.Should().NotBe(default);
            cs.Keyword.Kind().Should().Be(SyntaxKind.ClassKeyword);
            cs.Identifier.Should().NotBe(default);
            cs.Identifier.ToString().Should().Be("a");
            cs.TypeParameterList.ToString().Should().Be("<b>");

            cs.BaseList.Should().BeNull();

            cs.ConstraintClauses.Count.Should().Be(1);
            cs.ConstraintClauses[0].WhereKeyword.Should().NotBe(default);
            cs.ConstraintClauses[0].Name.Should().NotBeNull();
            cs.ConstraintClauses[0].Name.ToString().Should().Be("b");
            cs.ConstraintClauses[0].ColonToken.Should().NotBe(default);
            cs.ConstraintClauses[0].ColonToken.IsMissing.Should().BeFalse();
            cs.ConstraintClauses[0].Constraints.Count.Should().Be(3);

            cs.ConstraintClauses[0].Constraints[0].Kind().Should().Be(SyntaxKind.ClassConstraint);
            var classBound = (ClassOrStructConstraintSyntax)cs.ConstraintClauses[0].Constraints[0];
            classBound.ClassOrStructKeyword.Should().NotBe(default);
            classBound.ClassOrStructKeyword.IsMissing.Should().BeFalse();
            classBound.ClassOrStructKeyword.Kind().Should().Be(SyntaxKind.ClassKeyword);

            cs.ConstraintClauses[0].Constraints[1].Kind().Should().Be(SyntaxKind.TypeConstraint);
            var typeBound = (TypeConstraintSyntax)cs.ConstraintClauses[0].Constraints[1];
            typeBound.Type.Should().NotBeNull();
            typeBound.Type.ToString().Should().Be("c");

            cs.ConstraintClauses[0].Constraints[2].Kind().Should().Be(SyntaxKind.ConstructorConstraint);
            var bound = (ConstructorConstraintSyntax)cs.ConstraintClauses[0].Constraints[2];
            bound.NewKeyword.Should().NotBe(default);
            bound.NewKeyword.IsMissing.Should().BeFalse();
            bound.OpenParenToken.Should().NotBe(default);
            bound.OpenParenToken.IsMissing.Should().BeFalse();
            bound.CloseParenToken.Should().NotBe(default);
            bound.CloseParenToken.IsMissing.Should().BeFalse();

            cs.OpenBraceToken.Should().NotBe(default);
            cs.Members.Count.Should().Be(0);
            cs.CloseBraceToken.Should().NotBe(default);
        }

        [Fact]
        public void TestClassWithMultipleConstraints()
        {
            var text = "class a<b> where b : c where b : new() { }";
            var file = this.ParseFile(text);

            file.Should().NotBeNull();
            file.Members.Count.Should().Be(1);
            file.ToString().Should().Be(text);
            file.Errors().Length.Should().Be(0);

            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var cs = (TypeDeclarationSyntax)file.Members[0];
            cs.AttributeLists.Count.Should().Be(0);
            cs.Modifiers.Count.Should().Be(0);
            cs.Keyword.Should().NotBe(default);
            cs.Keyword.Kind().Should().Be(SyntaxKind.ClassKeyword);
            cs.Identifier.Should().NotBe(default);
            cs.TypeParameterList.ToString().Should().Be("<b>");

            cs.BaseList.Should().BeNull();

            cs.ConstraintClauses.Count.Should().Be(2);

            cs.ConstraintClauses[0].WhereKeyword.Should().NotBe(default);
            cs.ConstraintClauses[0].Name.Should().NotBeNull();
            cs.ConstraintClauses[0].Name.ToString().Should().Be("b");
            cs.ConstraintClauses[0].ColonToken.Should().NotBe(default);
            cs.ConstraintClauses[0].ColonToken.IsMissing.Should().BeFalse();
            cs.ConstraintClauses[0].Constraints.Count.Should().Be(1);
            cs.ConstraintClauses[0].Constraints[0].Kind().Should().Be(SyntaxKind.TypeConstraint);
            var typeBound = (TypeConstraintSyntax)cs.ConstraintClauses[0].Constraints[0];
            typeBound.Type.Should().NotBeNull();
            typeBound.Type.ToString().Should().Be("c");

            cs.ConstraintClauses[1].WhereKeyword.Should().NotBe(default);
            cs.ConstraintClauses[1].Name.Should().NotBeNull();
            cs.ConstraintClauses[1].Name.ToString().Should().Be("b");
            cs.ConstraintClauses[1].ColonToken.Should().NotBe(default);
            cs.ConstraintClauses[1].ColonToken.IsMissing.Should().BeFalse();
            cs.ConstraintClauses[1].Constraints.Count.Should().Be(1);
            cs.ConstraintClauses[1].Constraints[0].Kind().Should().Be(SyntaxKind.ConstructorConstraint);
            var bound = (ConstructorConstraintSyntax)cs.ConstraintClauses[1].Constraints[0];
            bound.NewKeyword.Should().NotBe(default);
            bound.NewKeyword.IsMissing.Should().BeFalse();
            bound.OpenParenToken.Should().NotBe(default);
            bound.OpenParenToken.IsMissing.Should().BeFalse();
            bound.CloseParenToken.Should().NotBe(default);
            bound.CloseParenToken.IsMissing.Should().BeFalse();

            cs.OpenBraceToken.Should().NotBe(default);
            cs.Members.Count.Should().Be(0);
            cs.CloseBraceToken.Should().NotBe(default);
        }

        [Fact]
        public void TestClassWithMultipleConstraints001()
        {
            var text = "class a<b> where b : c where b { }";
            var file = this.ParseFile(text);

            file.Should().NotBeNull();
            file.Members.Count.Should().Be(1);
            file.ToString().Should().Be(text);
            file.Errors().Length.Should().Be(2);

            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var cs = (TypeDeclarationSyntax)file.Members[0];
            cs.AttributeLists.Count.Should().Be(0);
            cs.Modifiers.Count.Should().Be(0);
            cs.Keyword.Should().NotBe(default);
            cs.Keyword.Kind().Should().Be(SyntaxKind.ClassKeyword);
            cs.Identifier.Should().NotBe(default);
            cs.TypeParameterList.ToString().Should().Be("<b>");

            cs.BaseList.Should().BeNull();

            cs.ConstraintClauses.Count.Should().Be(2);

            cs.ConstraintClauses[0].WhereKeyword.Should().NotBe(default);
            cs.ConstraintClauses[0].Name.Should().NotBeNull();
            cs.ConstraintClauses[0].Name.ToString().Should().Be("b");
            cs.ConstraintClauses[0].ColonToken.Should().NotBe(default);
            cs.ConstraintClauses[0].ColonToken.IsMissing.Should().BeFalse();
            cs.ConstraintClauses[0].Constraints.Count.Should().Be(1);
            cs.ConstraintClauses[0].Constraints[0].Kind().Should().Be(SyntaxKind.TypeConstraint);
            var typeBound = (TypeConstraintSyntax)cs.ConstraintClauses[0].Constraints[0];
            typeBound.Type.Should().NotBeNull();
            typeBound.Type.ToString().Should().Be("c");

            cs.ConstraintClauses[1].WhereKeyword.Should().NotBe(default);
            cs.ConstraintClauses[1].Name.Should().NotBeNull();
            cs.ConstraintClauses[1].Name.ToString().Should().Be("b");
            cs.ConstraintClauses[1].ColonToken.Should().NotBe(default);
            cs.ConstraintClauses[1].ColonToken.IsMissing.Should().BeTrue();
            cs.ConstraintClauses[1].Constraints.Count.Should().Be(1);
            cs.ConstraintClauses[1].Constraints[0].Kind().Should().Be(SyntaxKind.TypeConstraint);
            var bound = (TypeConstraintSyntax)cs.ConstraintClauses[1].Constraints[0];
            bound.Type.IsMissing.Should().BeTrue();
        }

        [Fact]
        public void TestClassWithMultipleConstraints002()
        {
            var text = "class a<b> where b : c where { }";
            var file = this.ParseFile(text);

            file.Should().NotBeNull();
            file.Members.Count.Should().Be(1);
            file.ToString().Should().Be(text);
            file.Errors().Length.Should().Be(3);

            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var cs = (TypeDeclarationSyntax)file.Members[0];
            cs.AttributeLists.Count.Should().Be(0);
            cs.Modifiers.Count.Should().Be(0);
            cs.Keyword.Should().NotBe(default);
            cs.Keyword.Kind().Should().Be(SyntaxKind.ClassKeyword);
            cs.Identifier.Should().NotBe(default);
            cs.TypeParameterList.ToString().Should().Be("<b>");

            cs.BaseList.Should().BeNull();

            cs.ConstraintClauses.Count.Should().Be(2);

            cs.ConstraintClauses[0].WhereKeyword.Should().NotBe(default);
            cs.ConstraintClauses[0].Name.Should().NotBeNull();
            cs.ConstraintClauses[0].Name.ToString().Should().Be("b");
            cs.ConstraintClauses[0].ColonToken.Should().NotBe(default);
            cs.ConstraintClauses[0].ColonToken.IsMissing.Should().BeFalse();
            cs.ConstraintClauses[0].Constraints.Count.Should().Be(1);
            cs.ConstraintClauses[0].Constraints[0].Kind().Should().Be(SyntaxKind.TypeConstraint);
            var typeBound = (TypeConstraintSyntax)cs.ConstraintClauses[0].Constraints[0];
            typeBound.Type.Should().NotBeNull();
            typeBound.Type.ToString().Should().Be("c");

            cs.ConstraintClauses[1].WhereKeyword.Should().NotBe(default);
            cs.ConstraintClauses[1].Name.IsMissing.Should().BeTrue();
            cs.ConstraintClauses[1].ColonToken.IsMissing.Should().BeTrue();
            cs.ConstraintClauses[1].Constraints.Count.Should().Be(1);
            cs.ConstraintClauses[1].Constraints[0].Kind().Should().Be(SyntaxKind.TypeConstraint);
            var bound = (TypeConstraintSyntax)cs.ConstraintClauses[1].Constraints[0];
            bound.Type.IsMissing.Should().BeTrue();
        }

        [Fact]
        public void TestClassWithMultipleBasesAndConstraints()
        {
            var text = "class a<b> : c, d where b : class, e, new() { }";
            var file = this.ParseFile(text);

            file.Should().NotBeNull();
            file.Members.Count.Should().Be(1);
            file.ToString().Should().Be(text);
            file.Errors().Length.Should().Be(0);

            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var cs = (TypeDeclarationSyntax)file.Members[0];
            cs.AttributeLists.Count.Should().Be(0);
            cs.Modifiers.Count.Should().Be(0);
            cs.Keyword.Should().NotBe(default);
            cs.Keyword.Kind().Should().Be(SyntaxKind.ClassKeyword);
            cs.Identifier.Should().NotBe(default);
            cs.Identifier.ToString().Should().Be("a");
            cs.TypeParameterList.ToString().Should().Be("<b>");

            cs.BaseList.Should().NotBeNull();
            cs.BaseList.ColonToken.Should().NotBe(default);
            cs.BaseList.Types.Count.Should().Be(2);
            cs.BaseList.Types[0].Type.ToString().Should().Be("c");
            cs.BaseList.Types[1].Type.ToString().Should().Be("d");

            cs.ConstraintClauses.Count.Should().Be(1);
            cs.ConstraintClauses[0].WhereKeyword.Should().NotBe(default);
            cs.ConstraintClauses[0].Name.Should().NotBeNull();
            cs.ConstraintClauses[0].Name.ToString().Should().Be("b");
            cs.ConstraintClauses[0].ColonToken.Should().NotBe(default);
            cs.ConstraintClauses[0].ColonToken.IsMissing.Should().BeFalse();
            cs.ConstraintClauses[0].Constraints.Count.Should().Be(3);

            cs.ConstraintClauses[0].Constraints[0].Kind().Should().Be(SyntaxKind.ClassConstraint);
            var classBound = (ClassOrStructConstraintSyntax)cs.ConstraintClauses[0].Constraints[0];
            classBound.ClassOrStructKeyword.Should().NotBe(default);
            classBound.ClassOrStructKeyword.IsMissing.Should().BeFalse();
            classBound.ClassOrStructKeyword.Kind().Should().Be(SyntaxKind.ClassKeyword);

            cs.ConstraintClauses[0].Constraints[1].Kind().Should().Be(SyntaxKind.TypeConstraint);
            var typeBound = (TypeConstraintSyntax)cs.ConstraintClauses[0].Constraints[1];
            typeBound.Type.Should().NotBeNull();
            typeBound.Type.ToString().Should().Be("e");

            cs.ConstraintClauses[0].Constraints[2].Kind().Should().Be(SyntaxKind.ConstructorConstraint);
            var bound = (ConstructorConstraintSyntax)cs.ConstraintClauses[0].Constraints[2];
            bound.NewKeyword.Should().NotBe(default);
            bound.NewKeyword.IsMissing.Should().BeFalse();
            bound.OpenParenToken.Should().NotBe(default);
            bound.OpenParenToken.IsMissing.Should().BeFalse();
            bound.CloseParenToken.Should().NotBe(default);
            bound.CloseParenToken.IsMissing.Should().BeFalse();

            cs.OpenBraceToken.Should().NotBe(default);
            cs.Members.Count.Should().Be(0);
            cs.CloseBraceToken.Should().NotBe(default);
        }

        [Fact]
        public void TestInterface()
        {
            var text = "interface a { }";
            var file = this.ParseFile(text);

            file.Should().NotBeNull();
            file.Members.Count.Should().Be(1);
            file.ToString().Should().Be(text);
            file.Errors().Length.Should().Be(0);

            file.Members[0].Kind().Should().Be(SyntaxKind.InterfaceDeclaration);
            var cs = (TypeDeclarationSyntax)file.Members[0];
            cs.AttributeLists.Count.Should().Be(0);
            cs.Modifiers.Count.Should().Be(0);
            cs.Keyword.Should().NotBe(default);
            cs.Keyword.Kind().Should().Be(SyntaxKind.InterfaceKeyword);
            cs.Identifier.Should().NotBe(default);
            cs.Identifier.ToString().Should().Be("a");
            cs.BaseList.Should().BeNull();
            cs.ConstraintClauses.Count.Should().Be(0);
            cs.OpenBraceToken.Should().NotBe(default);
            cs.CloseBraceToken.Should().NotBe(default);
        }

        [Fact]
        public void TestGenericInterface()
        {
            var text = "interface A<B> { }";
            var file = this.ParseFile(text);

            file.Should().NotBeNull();
            file.Members.Count.Should().Be(1);
            file.ToString().Should().Be(text);
            file.Errors().Length.Should().Be(0);

            file.Members[0].Kind().Should().Be(SyntaxKind.InterfaceDeclaration);
            var cs = (TypeDeclarationSyntax)file.Members[0];
            cs.AttributeLists.Count.Should().Be(0);
            cs.Modifiers.Count.Should().Be(0);
            cs.Keyword.Should().NotBe(default);
            cs.Keyword.Kind().Should().Be(SyntaxKind.InterfaceKeyword);
            cs.Identifier.Should().NotBe(default);
            var gn = cs.TypeParameterList;
            gn.ToString().Should().Be("<B>");
            cs.Identifier.ToString().Should().Be("A");
            gn.Parameters[0].AttributeLists.Count.Should().Be(0);
            gn.Parameters[0].VarianceKeyword.Kind().Should().Be(SyntaxKind.None);
            gn.Parameters[0].Identifier.ToString().Should().Be("B");
            cs.BaseList.Should().BeNull();
            cs.ConstraintClauses.Count.Should().Be(0);
            cs.OpenBraceToken.Should().NotBe(default);
            cs.CloseBraceToken.Should().NotBe(default);
        }

        [Fact]
        public void TestGenericInterfaceWithAttributesAndVariance()
        {
            var text = "interface A<[B] out C> { }";
            var file = this.ParseFile(text);

            file.Should().NotBeNull();
            file.Members.Count.Should().Be(1);
            file.ToString().Should().Be(text);
            file.Errors().Length.Should().Be(0);

            file.Members[0].Kind().Should().Be(SyntaxKind.InterfaceDeclaration);
            var cs = (TypeDeclarationSyntax)file.Members[0];
            cs.AttributeLists.Count.Should().Be(0);
            cs.Modifiers.Count.Should().Be(0);
            cs.Keyword.Should().NotBe(default);
            cs.Keyword.Kind().Should().Be(SyntaxKind.InterfaceKeyword);
            cs.Identifier.Should().NotBe(default);

            var gn = cs.TypeParameterList;
            gn.ToString().Should().Be("<[B] out C>");
            cs.Identifier.ToString().Should().Be("A");
            gn.Parameters[0].AttributeLists.Count.Should().Be(1);
            gn.Parameters[0].AttributeLists[0].Attributes[0].Name.ToString().Should().Be("B");
            gn.Parameters[0].VarianceKeyword.Should().NotBe(default);
            gn.Parameters[0].VarianceKeyword.Kind().Should().Be(SyntaxKind.OutKeyword);
            cs.BaseList.Should().BeNull();
            cs.ConstraintClauses.Count.Should().Be(0);
            cs.OpenBraceToken.Should().NotBe(default);
            cs.CloseBraceToken.Should().NotBe(default);
        }

        [Fact]
        public void TestStruct()
        {
            var text = "struct a { }";
            var file = this.ParseFile(text);

            file.Should().NotBeNull();
            file.Members.Count.Should().Be(1);
            file.ToString().Should().Be(text);
            file.Errors().Length.Should().Be(0);

            file.Members[0].Kind().Should().Be(SyntaxKind.StructDeclaration);
            var cs = (TypeDeclarationSyntax)file.Members[0];
            cs.AttributeLists.Count.Should().Be(0);
            cs.Modifiers.Count.Should().Be(0);
            cs.Keyword.Should().NotBe(default);
            cs.Keyword.Kind().Should().Be(SyntaxKind.StructKeyword);
            cs.Identifier.Should().NotBe(default);
            cs.Identifier.ToString().Should().Be("a");
            cs.BaseList.Should().BeNull();
            cs.ConstraintClauses.Count.Should().Be(0);
            cs.OpenBraceToken.Should().NotBe(default);
            cs.CloseBraceToken.Should().NotBe(default);
        }

        [Fact]
        public void TestNestedClass()
        {
            var text = "class a { class b { } }";
            var file = this.ParseFile(text);

            file.Should().NotBeNull();
            file.Members.Count.Should().Be(1);
            file.ToString().Should().Be(text);
            file.Errors().Length.Should().Be(0);

            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var cs = (TypeDeclarationSyntax)file.Members[0];
            cs.AttributeLists.Count.Should().Be(0);
            cs.Modifiers.Count.Should().Be(0);
            cs.Keyword.Should().NotBe(default);
            cs.Keyword.Kind().Should().Be(SyntaxKind.ClassKeyword);
            cs.Identifier.Should().NotBe(default);
            cs.Identifier.ToString().Should().Be("a");
            cs.BaseList.Should().BeNull();
            cs.ConstraintClauses.Count.Should().Be(0);
            cs.OpenBraceToken.Should().NotBe(default);
            cs.CloseBraceToken.Should().NotBe(default);

            cs.Members.Count.Should().Be(1);

            cs.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            cs = (TypeDeclarationSyntax)cs.Members[0];
            cs.AttributeLists.Count.Should().Be(0);
            cs.Modifiers.Count.Should().Be(0);
            cs.Keyword.Should().NotBe(default);
            cs.Keyword.Kind().Should().Be(SyntaxKind.ClassKeyword);
            cs.Identifier.Should().NotBe(default);
            cs.Identifier.ToString().Should().Be("b");
            cs.BaseList.Should().BeNull();
            cs.ConstraintClauses.Count.Should().Be(0);
            cs.OpenBraceToken.Should().NotBe(default);
            cs.CloseBraceToken.Should().NotBe(default);
        }

        [Fact]
        public void TestNestedPrivateClass()
        {
            var text = "class a { private class b { } }";
            var file = this.ParseFile(text);

            file.Should().NotBeNull();
            file.Members.Count.Should().Be(1);
            file.ToString().Should().Be(text);
            file.Errors().Length.Should().Be(0);

            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var cs = (TypeDeclarationSyntax)file.Members[0];
            cs.AttributeLists.Count.Should().Be(0);
            cs.Modifiers.Count.Should().Be(0);
            cs.Keyword.Should().NotBe(default);
            cs.Keyword.Kind().Should().Be(SyntaxKind.ClassKeyword);
            cs.Identifier.Should().NotBe(default);
            cs.Identifier.ToString().Should().Be("a");
            cs.BaseList.Should().BeNull();
            cs.ConstraintClauses.Count.Should().Be(0);
            cs.OpenBraceToken.Should().NotBe(default);
            cs.CloseBraceToken.Should().NotBe(default);

            cs.Members.Count.Should().Be(1);

            cs.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            cs = (TypeDeclarationSyntax)cs.Members[0];
            cs.AttributeLists.Count.Should().Be(0);
            cs.Modifiers.Count.Should().Be(1);
            cs.Modifiers[0].Kind().Should().Be(SyntaxKind.PrivateKeyword);
            cs.Keyword.Should().NotBe(default);
            cs.Keyword.Kind().Should().Be(SyntaxKind.ClassKeyword);
            cs.Identifier.Should().NotBe(default);
            cs.Identifier.ToString().Should().Be("b");
            cs.BaseList.Should().BeNull();
            cs.ConstraintClauses.Count.Should().Be(0);
            cs.OpenBraceToken.Should().NotBe(default);
            cs.CloseBraceToken.Should().NotBe(default);
        }

        [Fact]
        public void TestNestedProtectedClass()
        {
            var text = "class a { protected class b { } }";
            var file = this.ParseFile(text);

            file.Should().NotBeNull();
            file.Members.Count.Should().Be(1);
            file.ToString().Should().Be(text);
            file.Errors().Length.Should().Be(0);

            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var cs = (TypeDeclarationSyntax)file.Members[0];
            cs.AttributeLists.Count.Should().Be(0);
            cs.Modifiers.Count.Should().Be(0);
            cs.Keyword.Should().NotBe(default);
            cs.Keyword.Kind().Should().Be(SyntaxKind.ClassKeyword);
            cs.Identifier.Should().NotBe(default);
            cs.Identifier.ToString().Should().Be("a");
            cs.BaseList.Should().BeNull();
            cs.ConstraintClauses.Count.Should().Be(0);
            cs.OpenBraceToken.Should().NotBe(default);
            cs.CloseBraceToken.Should().NotBe(default);

            cs.Members.Count.Should().Be(1);

            cs.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            cs = (TypeDeclarationSyntax)cs.Members[0];
            cs.AttributeLists.Count.Should().Be(0);
            cs.Modifiers.Count.Should().Be(1);
            cs.Modifiers[0].Kind().Should().Be(SyntaxKind.ProtectedKeyword);
            cs.Keyword.Should().NotBe(default);
            cs.Keyword.Kind().Should().Be(SyntaxKind.ClassKeyword);
            cs.Identifier.Should().NotBe(default);
            cs.Identifier.ToString().Should().Be("b");
            cs.BaseList.Should().BeNull();
            cs.ConstraintClauses.Count.Should().Be(0);
            cs.OpenBraceToken.Should().NotBe(default);
            cs.CloseBraceToken.Should().NotBe(default);
        }

        [Fact]
        public void TestNestedProtectedInternalClass()
        {
            var text = "class a { protected internal class b { } }";
            var file = this.ParseFile(text);

            file.Should().NotBeNull();
            file.Members.Count.Should().Be(1);
            file.ToString().Should().Be(text);
            file.Errors().Length.Should().Be(0);

            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var cs = (TypeDeclarationSyntax)file.Members[0];
            cs.AttributeLists.Count.Should().Be(0);
            cs.Modifiers.Count.Should().Be(0);
            cs.Keyword.Should().NotBe(default);
            cs.Keyword.Kind().Should().Be(SyntaxKind.ClassKeyword);
            cs.Identifier.Should().NotBe(default);
            cs.Identifier.ToString().Should().Be("a");
            cs.BaseList.Should().BeNull();
            cs.ConstraintClauses.Count.Should().Be(0);
            cs.OpenBraceToken.Should().NotBe(default);
            cs.CloseBraceToken.Should().NotBe(default);

            cs.Members.Count.Should().Be(1);

            cs.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            cs = (TypeDeclarationSyntax)cs.Members[0];
            cs.AttributeLists.Count.Should().Be(0);
            cs.Modifiers.Count.Should().Be(2);
            cs.Modifiers[0].Kind().Should().Be(SyntaxKind.ProtectedKeyword);
            cs.Modifiers[1].Kind().Should().Be(SyntaxKind.InternalKeyword);
            cs.Keyword.Should().NotBe(default);
            cs.Keyword.Kind().Should().Be(SyntaxKind.ClassKeyword);
            cs.Identifier.Should().NotBe(default);
            cs.Identifier.ToString().Should().Be("b");
            cs.BaseList.Should().BeNull();
            cs.ConstraintClauses.Count.Should().Be(0);
            cs.OpenBraceToken.Should().NotBe(default);
            cs.CloseBraceToken.Should().NotBe(default);
        }

        [Fact]
        public void TestNestedInternalProtectedClass()
        {
            var text = "class a { internal protected class b { } }";
            var file = this.ParseFile(text);

            file.Should().NotBeNull();
            file.Members.Count.Should().Be(1);
            file.ToString().Should().Be(text);
            file.Errors().Length.Should().Be(0);

            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var cs = (TypeDeclarationSyntax)file.Members[0];
            cs.AttributeLists.Count.Should().Be(0);
            cs.Modifiers.Count.Should().Be(0);
            cs.Keyword.Should().NotBe(default);
            cs.Keyword.Kind().Should().Be(SyntaxKind.ClassKeyword);
            cs.Identifier.Should().NotBe(default);
            cs.Identifier.ToString().Should().Be("a");
            cs.BaseList.Should().BeNull();
            cs.ConstraintClauses.Count.Should().Be(0);
            cs.OpenBraceToken.Should().NotBe(default);
            cs.CloseBraceToken.Should().NotBe(default);

            cs.Members.Count.Should().Be(1);

            cs.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            cs = (TypeDeclarationSyntax)cs.Members[0];
            cs.AttributeLists.Count.Should().Be(0);
            cs.Modifiers.Count.Should().Be(2);
            cs.Modifiers[0].Kind().Should().Be(SyntaxKind.InternalKeyword);
            cs.Modifiers[1].Kind().Should().Be(SyntaxKind.ProtectedKeyword);
            cs.Keyword.Should().NotBe(default);
            cs.Keyword.Kind().Should().Be(SyntaxKind.ClassKeyword);
            cs.Identifier.Should().NotBe(default);
            cs.Identifier.ToString().Should().Be("b");
            cs.BaseList.Should().BeNull();
            cs.ConstraintClauses.Count.Should().Be(0);
            cs.OpenBraceToken.Should().NotBe(default);
            cs.CloseBraceToken.Should().NotBe(default);
        }

        [Fact]
        public void TestNestedPublicClass()
        {
            var text = "class a { public class b { } }";
            var file = this.ParseFile(text);

            file.Should().NotBeNull();
            file.Members.Count.Should().Be(1);
            file.ToString().Should().Be(text);
            file.Errors().Length.Should().Be(0);

            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var cs = (TypeDeclarationSyntax)file.Members[0];
            cs.AttributeLists.Count.Should().Be(0);
            cs.Modifiers.Count.Should().Be(0);
            cs.Keyword.Should().NotBe(default);
            cs.Keyword.Kind().Should().Be(SyntaxKind.ClassKeyword);
            cs.Identifier.Should().NotBe(default);
            cs.Identifier.ToString().Should().Be("a");
            cs.BaseList.Should().BeNull();
            cs.ConstraintClauses.Count.Should().Be(0);
            cs.OpenBraceToken.Should().NotBe(default);
            cs.CloseBraceToken.Should().NotBe(default);

            cs.Members.Count.Should().Be(1);

            cs.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            cs = (TypeDeclarationSyntax)cs.Members[0];
            cs.AttributeLists.Count.Should().Be(0);
            cs.Modifiers.Count.Should().Be(1);
            cs.Modifiers[0].Kind().Should().Be(SyntaxKind.PublicKeyword);
            cs.Keyword.Should().NotBe(default);
            cs.Keyword.Kind().Should().Be(SyntaxKind.ClassKeyword);
            cs.Identifier.Should().NotBe(default);
            cs.Identifier.ToString().Should().Be("b");
            cs.BaseList.Should().BeNull();
            cs.ConstraintClauses.Count.Should().Be(0);
            cs.OpenBraceToken.Should().NotBe(default);
            cs.CloseBraceToken.Should().NotBe(default);
        }

        [Fact]
        public void TestNestedInternalClass()
        {
            var text = "class a { internal class b { } }";
            var file = this.ParseFile(text);

            file.Should().NotBeNull();
            file.Members.Count.Should().Be(1);
            file.ToString().Should().Be(text);
            file.Errors().Length.Should().Be(0);

            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var cs = (TypeDeclarationSyntax)file.Members[0];
            cs.AttributeLists.Count.Should().Be(0);
            cs.Modifiers.Count.Should().Be(0);
            cs.Keyword.Should().NotBe(default);
            cs.Keyword.Kind().Should().Be(SyntaxKind.ClassKeyword);
            cs.Identifier.Should().NotBe(default);
            cs.Identifier.ToString().Should().Be("a");
            cs.BaseList.Should().BeNull();
            cs.ConstraintClauses.Count.Should().Be(0);
            cs.OpenBraceToken.Should().NotBe(default);
            cs.CloseBraceToken.Should().NotBe(default);

            cs.Members.Count.Should().Be(1);

            cs.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            cs = (TypeDeclarationSyntax)cs.Members[0];
            cs.AttributeLists.Count.Should().Be(0);
            cs.Modifiers.Count.Should().Be(1);
            cs.Modifiers[0].Kind().Should().Be(SyntaxKind.InternalKeyword);
            cs.Keyword.Should().NotBe(default);
            cs.Keyword.Kind().Should().Be(SyntaxKind.ClassKeyword);
            cs.Identifier.Should().NotBe(default);
            cs.Identifier.ToString().Should().Be("b");
            cs.BaseList.Should().BeNull();
            cs.ConstraintClauses.Count.Should().Be(0);
            cs.OpenBraceToken.Should().NotBe(default);
            cs.CloseBraceToken.Should().NotBe(default);
        }

        [Fact]
        public void TestDelegate()
        {
            var text = "delegate a b();";
            var file = this.ParseFile(text);

            file.Should().NotBeNull();
            file.Members.Count.Should().Be(1);
            file.ToString().Should().Be(text);
            file.Errors().Length.Should().Be(0);

            file.Members[0].Kind().Should().Be(SyntaxKind.DelegateDeclaration);
            var ds = (DelegateDeclarationSyntax)file.Members[0];
            ds.DelegateKeyword.Should().NotBe(default);
            ds.ReturnType.Should().NotBeNull();
            ds.ReturnType.ToString().Should().Be("a");
            ds.Identifier.Should().NotBe(default);
            ds.Identifier.ToString().Should().Be("b");
            ds.ParameterList.OpenParenToken.Should().NotBe(default);
            ds.ParameterList.OpenParenToken.IsMissing.Should().BeFalse();
            ds.ParameterList.Parameters.Count.Should().Be(0);
            ds.ParameterList.CloseParenToken.Should().NotBe(default);
            ds.ParameterList.CloseParenToken.IsMissing.Should().BeFalse();
            ds.SemicolonToken.Should().NotBe(default);
            ds.SemicolonToken.IsMissing.Should().BeFalse();
        }

        [Fact]
        public void TestDelegateWithRefReturnType()
        {
            var text = "delegate ref a b();";
            var file = this.ParseFile(text);

            file.Should().NotBeNull();
            file.Members.Count.Should().Be(1);
            file.ToString().Should().Be(text);
            file.Errors().Length.Should().Be(0);

            file.Members[0].Kind().Should().Be(SyntaxKind.DelegateDeclaration);
            var ds = (DelegateDeclarationSyntax)file.Members[0];
            ds.DelegateKeyword.Should().NotBe(default);
            ds.ReturnType.Should().NotBeNull();
            ds.ReturnType.ToString().Should().Be("ref a");
            ds.Identifier.Should().NotBe(default);
            ds.Identifier.ToString().Should().Be("b");
            ds.ParameterList.OpenParenToken.Should().NotBe(default);
            ds.ParameterList.OpenParenToken.IsMissing.Should().BeFalse();
            ds.ParameterList.Parameters.Count.Should().Be(0);
            ds.ParameterList.CloseParenToken.Should().NotBe(default);
            ds.ParameterList.CloseParenToken.IsMissing.Should().BeFalse();
            ds.SemicolonToken.Should().NotBe(default);
            ds.SemicolonToken.IsMissing.Should().BeFalse();
        }

        [CompilerTrait(CompilerFeature.ReadOnlyReferences)]
        [Fact]
        public void TestDelegateWithRefReadonlyReturnType()
        {
            var text = "delegate ref readonly a b();";
            var file = this.ParseFile(text, TestOptions.Regular);

            file.Should().NotBeNull();
            file.Members.Count.Should().Be(1);
            file.ToString().Should().Be(text);
            file.Errors().Length.Should().Be(0);

            file.Members[0].Kind().Should().Be(SyntaxKind.DelegateDeclaration);
            var ds = (DelegateDeclarationSyntax)file.Members[0];
            ds.DelegateKeyword.Should().NotBe(default);
            ds.ReturnType.Should().NotBeNull();
            ds.ReturnType.ToString().Should().Be("ref readonly a");
            ds.Identifier.Should().NotBe(default);
            ds.Identifier.ToString().Should().Be("b");
            ds.ParameterList.OpenParenToken.Should().NotBe(default);
            ds.ParameterList.OpenParenToken.IsMissing.Should().BeFalse();
            ds.ParameterList.Parameters.Count.Should().Be(0);
            ds.ParameterList.CloseParenToken.Should().NotBe(default);
            ds.ParameterList.CloseParenToken.IsMissing.Should().BeFalse();
            ds.SemicolonToken.Should().NotBe(default);
            ds.SemicolonToken.IsMissing.Should().BeFalse();
        }

        [Fact]
        public void TestDelegateWithBuiltInReturnTypes()
        {
            TestDelegateWithBuiltInReturnType(SyntaxKind.VoidKeyword);
            TestDelegateWithBuiltInReturnType(SyntaxKind.BoolKeyword);
            TestDelegateWithBuiltInReturnType(SyntaxKind.SByteKeyword);
            TestDelegateWithBuiltInReturnType(SyntaxKind.IntKeyword);
            TestDelegateWithBuiltInReturnType(SyntaxKind.UIntKeyword);
            TestDelegateWithBuiltInReturnType(SyntaxKind.ShortKeyword);
            TestDelegateWithBuiltInReturnType(SyntaxKind.UShortKeyword);
            TestDelegateWithBuiltInReturnType(SyntaxKind.LongKeyword);
            TestDelegateWithBuiltInReturnType(SyntaxKind.ULongKeyword);
            TestDelegateWithBuiltInReturnType(SyntaxKind.FloatKeyword);
            TestDelegateWithBuiltInReturnType(SyntaxKind.DoubleKeyword);
            TestDelegateWithBuiltInReturnType(SyntaxKind.DecimalKeyword);
            TestDelegateWithBuiltInReturnType(SyntaxKind.StringKeyword);
            TestDelegateWithBuiltInReturnType(SyntaxKind.CharKeyword);
            TestDelegateWithBuiltInReturnType(SyntaxKind.ObjectKeyword);
        }

        private void TestDelegateWithBuiltInReturnType(SyntaxKind builtInType)
        {
            var typeText = SyntaxFacts.GetText(builtInType);
            var text = "delegate " + typeText + " b();";
            var file = this.ParseFile(text);

            file.Should().NotBeNull();
            file.Members.Count.Should().Be(1);
            file.ToString().Should().Be(text);
            file.Errors().Length.Should().Be(0);

            file.Members[0].Kind().Should().Be(SyntaxKind.DelegateDeclaration);
            var ds = (DelegateDeclarationSyntax)file.Members[0];
            ds.DelegateKeyword.Should().NotBe(default);
            ds.ReturnType.Should().NotBeNull();
            ds.ReturnType.ToString().Should().Be(typeText);
            ds.Identifier.Should().NotBe(default);
            ds.Identifier.ToString().Should().Be("b");
            ds.ParameterList.OpenParenToken.Should().NotBe(default);
            ds.ParameterList.OpenParenToken.IsMissing.Should().BeFalse();
            ds.ParameterList.Parameters.Count.Should().Be(0);
            ds.ParameterList.CloseParenToken.Should().NotBe(default);
            ds.ParameterList.CloseParenToken.IsMissing.Should().BeFalse();
            ds.SemicolonToken.Should().NotBe(default);
            ds.SemicolonToken.IsMissing.Should().BeFalse();
        }

        [Fact]
        public void TestDelegateWithBuiltInParameterTypes()
        {
            TestDelegateWithBuiltInParameterType(SyntaxKind.BoolKeyword);
            TestDelegateWithBuiltInParameterType(SyntaxKind.SByteKeyword);
            TestDelegateWithBuiltInParameterType(SyntaxKind.IntKeyword);
            TestDelegateWithBuiltInParameterType(SyntaxKind.UIntKeyword);
            TestDelegateWithBuiltInParameterType(SyntaxKind.ShortKeyword);
            TestDelegateWithBuiltInParameterType(SyntaxKind.UShortKeyword);
            TestDelegateWithBuiltInParameterType(SyntaxKind.LongKeyword);
            TestDelegateWithBuiltInParameterType(SyntaxKind.ULongKeyword);
            TestDelegateWithBuiltInParameterType(SyntaxKind.FloatKeyword);
            TestDelegateWithBuiltInParameterType(SyntaxKind.DoubleKeyword);
            TestDelegateWithBuiltInParameterType(SyntaxKind.DecimalKeyword);
            TestDelegateWithBuiltInParameterType(SyntaxKind.StringKeyword);
            TestDelegateWithBuiltInParameterType(SyntaxKind.CharKeyword);
            TestDelegateWithBuiltInParameterType(SyntaxKind.ObjectKeyword);
        }

        private void TestDelegateWithBuiltInParameterType(SyntaxKind builtInType)
        {
            var typeText = SyntaxFacts.GetText(builtInType);
            var text = "delegate a b(" + typeText + " c);";
            var file = this.ParseFile(text);

            file.Should().NotBeNull();
            file.Members.Count.Should().Be(1);
            file.ToString().Should().Be(text);
            file.Errors().Length.Should().Be(0);

            file.Members[0].Kind().Should().Be(SyntaxKind.DelegateDeclaration);
            var ds = (DelegateDeclarationSyntax)file.Members[0];
            ds.DelegateKeyword.Should().NotBe(default);
            ds.ReturnType.Should().NotBeNull();
            ds.ReturnType.ToString().Should().Be("a");
            ds.Identifier.Should().NotBe(default);
            ds.Identifier.ToString().Should().Be("b");
            ds.ParameterList.OpenParenToken.Should().NotBe(default);
            ds.ParameterList.OpenParenToken.IsMissing.Should().BeFalse();

            ds.ParameterList.Parameters.Count.Should().Be(1);
            ds.ParameterList.Parameters[0].AttributeLists.Count.Should().Be(0);
            ds.ParameterList.Parameters[0].Modifiers.Count.Should().Be(0);
            ds.ParameterList.Parameters[0].Type.Should().NotBeNull();
            ds.ParameterList.Parameters[0].Type.ToString().Should().Be(typeText);
            ds.ParameterList.Parameters[0].Identifier.Should().NotBe(default);
            ds.ParameterList.Parameters[0].Identifier.ToString().Should().Be("c");

            ds.ParameterList.CloseParenToken.Should().NotBe(default);
            ds.ParameterList.CloseParenToken.IsMissing.Should().BeFalse();
            ds.SemicolonToken.Should().NotBe(default);
            ds.SemicolonToken.IsMissing.Should().BeFalse();
        }

        [Fact]
        public void TestDelegateWithParameter()
        {
            var text = "delegate a b(c d);";
            var file = this.ParseFile(text);

            file.Should().NotBeNull();
            file.Members.Count.Should().Be(1);
            file.ToString().Should().Be(text);
            file.Errors().Length.Should().Be(0);

            file.Members[0].Kind().Should().Be(SyntaxKind.DelegateDeclaration);
            var ds = (DelegateDeclarationSyntax)file.Members[0];
            ds.DelegateKeyword.Should().NotBe(default);
            ds.ReturnType.Should().NotBeNull();
            ds.ReturnType.ToString().Should().Be("a");
            ds.Identifier.Should().NotBe(default);
            ds.Identifier.ToString().Should().Be("b");
            ds.ParameterList.OpenParenToken.Should().NotBe(default);
            ds.ParameterList.OpenParenToken.IsMissing.Should().BeFalse();

            ds.ParameterList.Parameters.Count.Should().Be(1);
            ds.ParameterList.Parameters[0].AttributeLists.Count.Should().Be(0);
            ds.ParameterList.Parameters[0].Modifiers.Count.Should().Be(0);
            ds.ParameterList.Parameters[0].Type.Should().NotBeNull();
            ds.ParameterList.Parameters[0].Type.ToString().Should().Be("c");
            ds.ParameterList.Parameters[0].Identifier.Should().NotBe(default);
            ds.ParameterList.Parameters[0].Identifier.ToString().Should().Be("d");

            ds.ParameterList.CloseParenToken.Should().NotBe(default);
            ds.ParameterList.CloseParenToken.IsMissing.Should().BeFalse();
            ds.SemicolonToken.Should().NotBe(default);
            ds.SemicolonToken.IsMissing.Should().BeFalse();
        }

        [Fact]
        public void TestDelegateWithMultipleParameters()
        {
            var text = "delegate a b(c d, e f);";
            var file = this.ParseFile(text);

            file.Should().NotBeNull();
            file.Members.Count.Should().Be(1);
            file.ToString().Should().Be(text);
            file.Errors().Length.Should().Be(0);

            file.Members[0].Kind().Should().Be(SyntaxKind.DelegateDeclaration);
            var ds = (DelegateDeclarationSyntax)file.Members[0];
            ds.DelegateKeyword.Should().NotBe(default);
            ds.ReturnType.Should().NotBeNull();
            ds.ReturnType.ToString().Should().Be("a");
            ds.Identifier.Should().NotBe(default);
            ds.Identifier.ToString().Should().Be("b");
            ds.ParameterList.OpenParenToken.Should().NotBe(default);
            ds.ParameterList.OpenParenToken.IsMissing.Should().BeFalse();

            ds.ParameterList.Parameters.Count.Should().Be(2);
            ds.ParameterList.Parameters[0].AttributeLists.Count.Should().Be(0);
            ds.ParameterList.Parameters[0].Modifiers.Count.Should().Be(0);
            ds.ParameterList.Parameters[0].Type.Should().NotBeNull();
            ds.ParameterList.Parameters[0].Type.ToString().Should().Be("c");
            ds.ParameterList.Parameters[0].Identifier.Should().NotBe(default);
            ds.ParameterList.Parameters[0].Identifier.ToString().Should().Be("d");

            ds.ParameterList.Parameters[1].AttributeLists.Count.Should().Be(0);
            ds.ParameterList.Parameters[1].Modifiers.Count.Should().Be(0);
            ds.ParameterList.Parameters[1].Type.Should().NotBeNull();
            ds.ParameterList.Parameters[1].Type.ToString().Should().Be("e");
            ds.ParameterList.Parameters[1].Identifier.Should().NotBe(default);
            ds.ParameterList.Parameters[1].Identifier.ToString().Should().Be("f");

            ds.ParameterList.CloseParenToken.Should().NotBe(default);
            ds.ParameterList.CloseParenToken.IsMissing.Should().BeFalse();
            ds.SemicolonToken.Should().NotBe(default);
            ds.SemicolonToken.IsMissing.Should().BeFalse();
        }

        [Fact]
        public void TestDelegateWithRefParameter()
        {
            var text = "delegate a b(ref c d);";
            var file = this.ParseFile(text);

            file.Should().NotBeNull();
            file.Members.Count.Should().Be(1);
            file.ToString().Should().Be(text);
            file.Errors().Length.Should().Be(0);

            file.Members[0].Kind().Should().Be(SyntaxKind.DelegateDeclaration);
            var ds = (DelegateDeclarationSyntax)file.Members[0];
            ds.DelegateKeyword.Should().NotBe(default);
            ds.ReturnType.Should().NotBeNull();
            ds.ReturnType.ToString().Should().Be("a");
            ds.Identifier.Should().NotBe(default);
            ds.Identifier.ToString().Should().Be("b");
            ds.ParameterList.OpenParenToken.Should().NotBe(default);
            ds.ParameterList.OpenParenToken.IsMissing.Should().BeFalse();

            ds.ParameterList.Parameters.Count.Should().Be(1);
            ds.ParameterList.Parameters[0].AttributeLists.Count.Should().Be(0);
            ds.ParameterList.Parameters[0].Modifiers.Count.Should().Be(1);
            ds.ParameterList.Parameters[0].Modifiers[0].Kind().Should().Be(SyntaxKind.RefKeyword);
            ds.ParameterList.Parameters[0].Type.Should().NotBeNull();
            ds.ParameterList.Parameters[0].Type.ToString().Should().Be("c");
            ds.ParameterList.Parameters[0].Identifier.Should().NotBe(default);
            ds.ParameterList.Parameters[0].Identifier.ToString().Should().Be("d");

            ds.ParameterList.CloseParenToken.Should().NotBe(default);
            ds.ParameterList.CloseParenToken.IsMissing.Should().BeFalse();
            ds.SemicolonToken.Should().NotBe(default);
            ds.SemicolonToken.IsMissing.Should().BeFalse();
        }

        [Fact]
        public void TestDelegateWithOutParameter()
        {
            var text = "delegate a b(out c d);";
            var file = this.ParseFile(text);

            file.Should().NotBeNull();
            file.Members.Count.Should().Be(1);
            file.ToString().Should().Be(text);
            file.Errors().Length.Should().Be(0);

            file.Members[0].Kind().Should().Be(SyntaxKind.DelegateDeclaration);
            var ds = (DelegateDeclarationSyntax)file.Members[0];
            ds.DelegateKeyword.Should().NotBe(default);
            ds.ReturnType.Should().NotBeNull();
            ds.ReturnType.ToString().Should().Be("a");
            ds.Identifier.Should().NotBe(default);
            ds.Identifier.ToString().Should().Be("b");
            ds.ParameterList.OpenParenToken.Should().NotBe(default);
            ds.ParameterList.OpenParenToken.IsMissing.Should().BeFalse();

            ds.ParameterList.Parameters.Count.Should().Be(1);
            ds.ParameterList.Parameters[0].AttributeLists.Count.Should().Be(0);
            ds.ParameterList.Parameters[0].Modifiers.Count.Should().Be(1);
            ds.ParameterList.Parameters[0].Modifiers[0].Kind().Should().Be(SyntaxKind.OutKeyword);
            ds.ParameterList.Parameters[0].Type.Should().NotBeNull();
            ds.ParameterList.Parameters[0].Type.ToString().Should().Be("c");
            ds.ParameterList.Parameters[0].Identifier.Should().NotBe(default);
            ds.ParameterList.Parameters[0].Identifier.ToString().Should().Be("d");

            ds.ParameterList.CloseParenToken.Should().NotBe(default);
            ds.ParameterList.CloseParenToken.IsMissing.Should().BeFalse();
            ds.SemicolonToken.Should().NotBe(default);
            ds.SemicolonToken.IsMissing.Should().BeFalse();
        }

        [Fact]
        public void TestDelegateWithParamsParameter()
        {
            var text = "delegate a b(params c d);";
            var file = this.ParseFile(text);

            file.Should().NotBeNull();
            file.Members.Count.Should().Be(1);
            file.ToString().Should().Be(text);
            file.Errors().Length.Should().Be(0);

            file.Members[0].Kind().Should().Be(SyntaxKind.DelegateDeclaration);
            var ds = (DelegateDeclarationSyntax)file.Members[0];
            ds.DelegateKeyword.Should().NotBe(default);
            ds.ReturnType.Should().NotBeNull();
            ds.ReturnType.ToString().Should().Be("a");
            ds.Identifier.Should().NotBe(default);
            ds.Identifier.ToString().Should().Be("b");
            ds.ParameterList.OpenParenToken.Should().NotBe(default);
            ds.ParameterList.OpenParenToken.IsMissing.Should().BeFalse();

            ds.ParameterList.Parameters.Count.Should().Be(1);
            ds.ParameterList.Parameters[0].AttributeLists.Count.Should().Be(0);
            ds.ParameterList.Parameters[0].Modifiers.Count.Should().Be(1);
            ds.ParameterList.Parameters[0].Modifiers[0].Kind().Should().Be(SyntaxKind.ParamsKeyword);
            ds.ParameterList.Parameters[0].Type.Should().NotBeNull();
            ds.ParameterList.Parameters[0].Type.ToString().Should().Be("c");
            ds.ParameterList.Parameters[0].Identifier.Should().NotBe(default);
            ds.ParameterList.Parameters[0].Identifier.ToString().Should().Be("d");

            ds.ParameterList.CloseParenToken.Should().NotBe(default);
            ds.ParameterList.CloseParenToken.IsMissing.Should().BeFalse();
            ds.SemicolonToken.Should().NotBe(default);
            ds.SemicolonToken.IsMissing.Should().BeFalse();
        }

        [Fact]
        public void TestDelegateWithArgListParameter()
        {
            var text = "delegate a b(__arglist);";
            var file = this.ParseFile(text);

            file.Should().NotBeNull();
            file.Members.Count.Should().Be(1);
            file.ToString().Should().Be(text);
            var errors = file.Errors();
            errors.Length.Should().Be(0);

            file.Members[0].Kind().Should().Be(SyntaxKind.DelegateDeclaration);
            var ds = (DelegateDeclarationSyntax)file.Members[0];
            ds.DelegateKeyword.Should().NotBe(default);
            ds.ReturnType.Should().NotBeNull();
            ds.ReturnType.ToString().Should().Be("a");
            ds.Identifier.Should().NotBe(default);
            ds.Identifier.ToString().Should().Be("b");
            ds.ParameterList.OpenParenToken.Should().NotBe(default);
            ds.ParameterList.OpenParenToken.IsMissing.Should().BeFalse();

            ds.ParameterList.Parameters.Count.Should().Be(1);
            ds.ParameterList.Parameters[0].AttributeLists.Count.Should().Be(0);
            ds.ParameterList.Parameters[0].Modifiers.Count.Should().Be(0);
            ds.ParameterList.Parameters[0].Type.Should().BeNull();
            ds.ParameterList.Parameters[0].Identifier.Should().NotBe(default);

            ds.ParameterList.CloseParenToken.Should().NotBe(default);
            ds.ParameterList.CloseParenToken.IsMissing.Should().BeFalse();
            ds.SemicolonToken.Should().NotBe(default);
            ds.SemicolonToken.IsMissing.Should().BeFalse();
        }

        [Fact]
        public void TestDelegateWithParameterAttribute()
        {
            var text = "delegate a b([attr] c d);";
            var file = this.ParseFile(text);

            file.Should().NotBeNull();
            file.Members.Count.Should().Be(1);
            file.ToString().Should().Be(text);
            file.Errors().Length.Should().Be(0);

            file.Members[0].Kind().Should().Be(SyntaxKind.DelegateDeclaration);
            var ds = (DelegateDeclarationSyntax)file.Members[0];
            ds.DelegateKeyword.Should().NotBe(default);
            ds.ReturnType.Should().NotBeNull();
            ds.ReturnType.ToString().Should().Be("a");
            ds.Identifier.Should().NotBe(default);
            ds.Identifier.ToString().Should().Be("b");
            ds.ParameterList.OpenParenToken.Should().NotBe(default);
            ds.ParameterList.OpenParenToken.IsMissing.Should().BeFalse();

            ds.ParameterList.Parameters.Count.Should().Be(1);
            ds.ParameterList.Parameters[0].AttributeLists.Count.Should().Be(1);
            ds.ParameterList.Parameters[0].AttributeLists[0].ToString().Should().Be("[attr]");
            ds.ParameterList.Parameters[0].Modifiers.Count.Should().Be(0);
            ds.ParameterList.Parameters[0].Type.Should().NotBeNull();
            ds.ParameterList.Parameters[0].Type.ToString().Should().Be("c");
            ds.ParameterList.Parameters[0].Identifier.Should().NotBe(default);
            ds.ParameterList.Parameters[0].Identifier.ToString().Should().Be("d");

            ds.ParameterList.CloseParenToken.Should().NotBe(default);
            ds.ParameterList.CloseParenToken.IsMissing.Should().BeFalse();
            ds.SemicolonToken.Should().NotBe(default);
            ds.SemicolonToken.IsMissing.Should().BeFalse();
        }

        [Fact]
        public void TestNestedDelegate()
        {
            var text = "class a { delegate b c(); }";
            var file = this.ParseFile(text);

            file.Should().NotBeNull();
            file.Members.Count.Should().Be(1);
            file.ToString().Should().Be(text);
            file.Errors().Length.Should().Be(0);

            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var cs = (TypeDeclarationSyntax)file.Members[0];
            cs.AttributeLists.Count.Should().Be(0);
            cs.Modifiers.Count.Should().Be(0);
            cs.Keyword.Should().NotBe(default);
            cs.Keyword.Kind().Should().Be(SyntaxKind.ClassKeyword);
            cs.Identifier.Should().NotBe(default);
            cs.Identifier.ToString().Should().Be("a");
            cs.BaseList.Should().BeNull();
            cs.ConstraintClauses.Count.Should().Be(0);
            cs.OpenBraceToken.Should().NotBe(default);
            cs.CloseBraceToken.Should().NotBe(default);

            cs.Members.Count.Should().Be(1);

            cs.Members[0].Kind().Should().Be(SyntaxKind.DelegateDeclaration);
            var ds = (DelegateDeclarationSyntax)cs.Members[0];
            ds.DelegateKeyword.Should().NotBe(default);
            ds.ReturnType.Should().NotBeNull();
            ds.ReturnType.ToString().Should().Be("b");
            ds.Identifier.Should().NotBe(default);
            ds.Identifier.ToString().Should().Be("c");
            ds.ParameterList.OpenParenToken.Should().NotBe(default);
            ds.ParameterList.OpenParenToken.IsMissing.Should().BeFalse();
            ds.ParameterList.Parameters.Count.Should().Be(0);
            ds.ParameterList.CloseParenToken.Should().NotBe(default);
            ds.ParameterList.CloseParenToken.IsMissing.Should().BeFalse();
            ds.SemicolonToken.Should().NotBe(default);
            ds.SemicolonToken.IsMissing.Should().BeFalse();
        }

        [Fact]
        public void TestClassMethod()
        {
            var text = "class a { b X() { } }";
            var file = this.ParseFile(text);

            file.Should().NotBeNull();
            file.Members.Count.Should().Be(1);
            file.ToString().Should().Be(text);
            file.Errors().Length.Should().Be(0);

            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var cs = (TypeDeclarationSyntax)file.Members[0];
            cs.AttributeLists.Count.Should().Be(0);
            cs.Modifiers.Count.Should().Be(0);
            cs.Keyword.Should().NotBe(default);
            cs.Keyword.Kind().Should().Be(SyntaxKind.ClassKeyword);
            cs.Identifier.Should().NotBe(default);
            cs.Identifier.ToString().Should().Be("a");
            cs.BaseList.Should().BeNull();
            cs.ConstraintClauses.Count.Should().Be(0);
            cs.OpenBraceToken.Should().NotBe(default);
            cs.CloseBraceToken.Should().NotBe(default);

            cs.Members.Count.Should().Be(1);

            cs.Members[0].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            var ms = (MethodDeclarationSyntax)cs.Members[0];
            ms.AttributeLists.Count.Should().Be(0);
            ms.Modifiers.Count.Should().Be(0);
            ms.ReturnType.Should().NotBeNull();
            ms.ReturnType.ToString().Should().Be("b");
            ms.Identifier.Should().NotBe(default);
            ms.Identifier.ToString().Should().Be("X");
            ms.ParameterList.OpenParenToken.Should().NotBe(default);
            ms.ParameterList.OpenParenToken.IsMissing.Should().BeFalse();
            ms.ParameterList.Parameters.Count.Should().Be(0);
            ms.ParameterList.CloseParenToken.Should().NotBe(default);
            ms.ParameterList.CloseParenToken.IsMissing.Should().BeFalse();
            ms.ConstraintClauses.Count.Should().Be(0);
            ms.Body.Should().NotBeNull();
            ms.Body.OpenBraceToken.Kind().Should().NotBe(SyntaxKind.None);
            ms.Body.CloseBraceToken.Kind().Should().NotBe(SyntaxKind.None);
            ms.SemicolonToken.Kind().Should().Be(SyntaxKind.None);
        }

        [Fact]
        public void TestClassMethodWithRefReturn()
        {
            var text = "class a { ref b X() { } }";
            var file = this.ParseFile(text);

            file.Should().NotBeNull();
            file.Members.Count.Should().Be(1);
            file.ToString().Should().Be(text);
            file.Errors().Length.Should().Be(0);

            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var cs = (TypeDeclarationSyntax)file.Members[0];
            cs.AttributeLists.Count.Should().Be(0);
            cs.Modifiers.Count.Should().Be(0);
            cs.Keyword.Should().NotBe(default);
            cs.Keyword.Kind().Should().Be(SyntaxKind.ClassKeyword);
            cs.Identifier.Should().NotBe(default);
            cs.Identifier.ToString().Should().Be("a");
            cs.BaseList.Should().BeNull();
            cs.ConstraintClauses.Count.Should().Be(0);
            cs.OpenBraceToken.Should().NotBe(default);
            cs.CloseBraceToken.Should().NotBe(default);

            cs.Members.Count.Should().Be(1);

            cs.Members[0].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            var ms = (MethodDeclarationSyntax)cs.Members[0];
            ms.AttributeLists.Count.Should().Be(0);
            ms.ReturnType.Should().NotBeNull();
            ms.ReturnType.ToString().Should().Be("ref b");
            ms.Identifier.Should().NotBe(default);
            ms.Identifier.ToString().Should().Be("X");
            ms.ParameterList.OpenParenToken.Should().NotBe(default);
            ms.ParameterList.OpenParenToken.IsMissing.Should().BeFalse();
            ms.ParameterList.Parameters.Count.Should().Be(0);
            ms.ParameterList.CloseParenToken.Should().NotBe(default);
            ms.ParameterList.CloseParenToken.IsMissing.Should().BeFalse();
            ms.ConstraintClauses.Count.Should().Be(0);
            ms.Body.Should().NotBeNull();
            ms.Body.OpenBraceToken.Kind().Should().NotBe(SyntaxKind.None);
            ms.Body.CloseBraceToken.Kind().Should().NotBe(SyntaxKind.None);
            ms.SemicolonToken.Kind().Should().Be(SyntaxKind.None);
        }

        [CompilerTrait(CompilerFeature.ReadOnlyReferences)]
        [Fact]
        public void TestClassMethodWithRefReadonlyReturn()
        {
            var text = "class a { ref readonly b X() { } }";
            var file = this.ParseFile(text, TestOptions.Regular);

            file.Should().NotBeNull();
            file.Members.Count.Should().Be(1);
            file.ToString().Should().Be(text);
            file.Errors().Length.Should().Be(0);

            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var cs = (TypeDeclarationSyntax)file.Members[0];
            cs.AttributeLists.Count.Should().Be(0);
            cs.Modifiers.Count.Should().Be(0);
            cs.Keyword.Should().NotBe(default);
            cs.Keyword.Kind().Should().Be(SyntaxKind.ClassKeyword);
            cs.Identifier.Should().NotBe(default);
            cs.Identifier.ToString().Should().Be("a");
            cs.BaseList.Should().BeNull();
            cs.ConstraintClauses.Count.Should().Be(0);
            cs.OpenBraceToken.Should().NotBe(default);
            cs.CloseBraceToken.Should().NotBe(default);

            cs.Members.Count.Should().Be(1);

            cs.Members[0].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            var ms = (MethodDeclarationSyntax)cs.Members[0];
            ms.AttributeLists.Count.Should().Be(0);
            ms.ReturnType.Should().NotBeNull();
            ms.ReturnType.ToString().Should().Be("ref readonly b");
            ms.Identifier.Should().NotBe(default);
            ms.Identifier.ToString().Should().Be("X");
            ms.ParameterList.OpenParenToken.Should().NotBe(default);
            ms.ParameterList.OpenParenToken.IsMissing.Should().BeFalse();
            ms.ParameterList.Parameters.Count.Should().Be(0);
            ms.ParameterList.CloseParenToken.Should().NotBe(default);
            ms.ParameterList.CloseParenToken.IsMissing.Should().BeFalse();
            ms.ConstraintClauses.Count.Should().Be(0);
            ms.Body.Should().NotBeNull();
            ms.Body.OpenBraceToken.Kind().Should().NotBe(SyntaxKind.None);
            ms.Body.CloseBraceToken.Kind().Should().NotBe(SyntaxKind.None);
            ms.SemicolonToken.Kind().Should().Be(SyntaxKind.None);
        }

        [Fact]
        public void TestClassMethodWithRef()
        {
            var text = "class a { ref }";
            var file = this.ParseFile(text);

            file.Should().NotBeNull();
            file.Members.Count.Should().Be(1);
            file.ToString().Should().Be(text);
            file.Errors().Length.Should().Be(1);

            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var cs = (TypeDeclarationSyntax)file.Members[0];
            cs.AttributeLists.Count.Should().Be(0);
            cs.Modifiers.Count.Should().Be(0);
            cs.Keyword.Should().NotBe(default);
            cs.Keyword.Kind().Should().Be(SyntaxKind.ClassKeyword);
            cs.Identifier.Should().NotBe(default);
            cs.Identifier.ToString().Should().Be("a");
            cs.BaseList.Should().BeNull();
            cs.ConstraintClauses.Count.Should().Be(0);
            cs.OpenBraceToken.Should().NotBe(default);
            cs.CloseBraceToken.Should().NotBe(default);

            cs.Members.Count.Should().Be(1);

            cs.Members[0].Kind().Should().Be(SyntaxKind.IncompleteMember);
        }

        [CompilerTrait(CompilerFeature.ReadOnlyReferences)]
        [Fact]
        public void TestClassMethodWithRefReadonly()
        {
            var text = "class a { ref readonly }";
            var file = this.ParseFile(text, parseOptions: TestOptions.Regular);

            file.Should().NotBeNull();
            file.Members.Count.Should().Be(1);
            file.ToString().Should().Be(text);
            file.Errors().Length.Should().Be(1);

            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var cs = (TypeDeclarationSyntax)file.Members[0];
            cs.AttributeLists.Count.Should().Be(0);
            cs.Modifiers.Count.Should().Be(0);
            cs.Keyword.Should().NotBe(default);
            cs.Keyword.Kind().Should().Be(SyntaxKind.ClassKeyword);
            cs.Identifier.Should().NotBe(default);
            cs.Identifier.ToString().Should().Be("a");
            cs.BaseList.Should().BeNull();
            cs.ConstraintClauses.Count.Should().Be(0);
            cs.OpenBraceToken.Should().NotBe(default);
            cs.CloseBraceToken.Should().NotBe(default);

            cs.Members.Count.Should().Be(1);

            cs.Members[0].Kind().Should().Be(SyntaxKind.IncompleteMember);
        }

        private void TestClassMethodModifiers(params SyntaxKind[] modifiers)
        {
            var text = "class a { " + string.Join(" ", modifiers.Select(SyntaxFacts.GetText)) + " b X() { } }";
            var file = this.ParseFile(text);

            file.Should().NotBeNull();
            file.Members.Count.Should().Be(1);
            file.ToString().Should().Be(text);
            file.Errors().Length.Should().Be(0);

            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var cs = (TypeDeclarationSyntax)file.Members[0];
            cs.AttributeLists.Count.Should().Be(0);
            cs.Modifiers.Count.Should().Be(0);
            cs.Keyword.Should().NotBe(default);
            cs.Keyword.Kind().Should().Be(SyntaxKind.ClassKeyword);
            cs.Identifier.Should().NotBe(default);
            cs.Identifier.ToString().Should().Be("a");
            cs.BaseList.Should().BeNull();
            cs.ConstraintClauses.Count.Should().Be(0);
            cs.OpenBraceToken.Should().NotBe(default);
            cs.CloseBraceToken.Should().NotBe(default);

            cs.Members.Count.Should().Be(1);

            cs.Members[0].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            var ms = (MethodDeclarationSyntax)cs.Members[0];
            ms.AttributeLists.Count.Should().Be(0);
            ms.Modifiers.Count.Should().Be(modifiers.Length);
            for (int i = 0; i < modifiers.Length; ++i)
            {
                ms.Modifiers[i].Kind().Should().Be(modifiers[i]);
            }
            ms.ReturnType.Should().NotBeNull();
            ms.ReturnType.ToString().Should().Be("b");
            ms.Identifier.Should().NotBe(default);
            ms.Identifier.ToString().Should().Be("X");
            ms.ParameterList.OpenParenToken.Should().NotBe(default);
            ms.ParameterList.OpenParenToken.IsMissing.Should().BeFalse();
            ms.ParameterList.Parameters.Count.Should().Be(0);
            ms.ParameterList.CloseParenToken.Should().NotBe(default);
            ms.ParameterList.CloseParenToken.IsMissing.Should().BeFalse();
            ms.ConstraintClauses.Count.Should().Be(0);
            ms.Body.Should().NotBeNull();
            ms.Body.OpenBraceToken.Kind().Should().NotBe(SyntaxKind.None);
            ms.Body.CloseBraceToken.Kind().Should().NotBe(SyntaxKind.None);
            ms.SemicolonToken.Kind().Should().Be(SyntaxKind.None);
        }

        [Fact]
        public void TestClassMethodAccessModes()
        {
            TestClassMethodModifiers(SyntaxKind.PublicKeyword);
            TestClassMethodModifiers(SyntaxKind.PrivateKeyword);
            TestClassMethodModifiers(SyntaxKind.InternalKeyword);
            TestClassMethodModifiers(SyntaxKind.ProtectedKeyword);
        }

        [Fact]
        public void TestClassMethodModifiersOrder()
        {
            TestClassMethodModifiers(SyntaxKind.PublicKeyword, SyntaxKind.VirtualKeyword);
            TestClassMethodModifiers(SyntaxKind.VirtualKeyword, SyntaxKind.PublicKeyword);
            TestClassMethodModifiers(SyntaxKind.InternalKeyword, SyntaxKind.ProtectedKeyword, SyntaxKind.VirtualKeyword);
            TestClassMethodModifiers(SyntaxKind.InternalKeyword, SyntaxKind.VirtualKeyword, SyntaxKind.ProtectedKeyword);
        }

        [Fact]
        public void TestClassMethodWithPartial()
        {
            var text = "class a { partial void M() { } }";
            var file = this.ParseFile(text);

            file.Should().NotBeNull();
            file.Members.Count.Should().Be(1);
            file.ToString().Should().Be(text);
            file.Errors().Length.Should().Be(0);

            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var cs = (TypeDeclarationSyntax)file.Members[0];
            cs.AttributeLists.Count.Should().Be(0);
            cs.Modifiers.Count.Should().Be(0);
            cs.Keyword.Should().NotBe(default);
            cs.Keyword.Kind().Should().Be(SyntaxKind.ClassKeyword);
            cs.Identifier.Should().NotBe(default);
            cs.Identifier.ToString().Should().Be("a");
            cs.BaseList.Should().BeNull();
            cs.ConstraintClauses.Count.Should().Be(0);
            cs.OpenBraceToken.Should().NotBe(default);
            cs.CloseBraceToken.Should().NotBe(default);

            cs.Members.Count.Should().Be(1);

            cs.Members[0].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            var ms = (MethodDeclarationSyntax)cs.Members[0];
            ms.AttributeLists.Count.Should().Be(0);
            ms.Modifiers.Count.Should().Be(1);
            ms.Modifiers[0].Kind().Should().Be(SyntaxKind.PartialKeyword);
            ms.ReturnType.Should().NotBeNull();
            ms.ReturnType.ToString().Should().Be("void");
            ms.Identifier.Should().NotBe(default);
            ms.Identifier.ToString().Should().Be("M");
            ms.ParameterList.OpenParenToken.Should().NotBe(default);
            ms.ParameterList.OpenParenToken.IsMissing.Should().BeFalse();
            ms.ParameterList.Parameters.Count.Should().Be(0);
            ms.ParameterList.CloseParenToken.Should().NotBe(default);
            ms.ParameterList.CloseParenToken.IsMissing.Should().BeFalse();
            ms.ConstraintClauses.Count.Should().Be(0);
            ms.Body.Should().NotBeNull();
            ms.Body.OpenBraceToken.Kind().Should().NotBe(SyntaxKind.None);
            ms.Body.CloseBraceToken.Kind().Should().NotBe(SyntaxKind.None);
            ms.SemicolonToken.Kind().Should().Be(SyntaxKind.None);
        }

        [Fact]
        public void TestStructMethodWithReadonly()
        {
            var text = "struct a { readonly void M() { } }";
            var file = this.ParseFile(text, TestOptions.Regular);

            file.Should().NotBeNull();
            file.Members.Count.Should().Be(1);
            file.ToString().Should().Be(text);
            file.Errors().Length.Should().Be(0);

            file.Members[0].Kind().Should().Be(SyntaxKind.StructDeclaration);
            var structDecl = (TypeDeclarationSyntax)file.Members[0];
            structDecl.AttributeLists.Count.Should().Be(0);
            structDecl.Modifiers.Count.Should().Be(0);
            structDecl.Keyword.Should().NotBe(default);
            structDecl.Keyword.Kind().Should().Be(SyntaxKind.StructKeyword);
            structDecl.Identifier.Should().NotBe(default);
            structDecl.Identifier.ToString().Should().Be("a");
            structDecl.BaseList.Should().BeNull();
            structDecl.ConstraintClauses.Count.Should().Be(0);
            structDecl.OpenBraceToken.Should().NotBe(default);
            structDecl.CloseBraceToken.Should().NotBe(default);

            structDecl.Members.Count.Should().Be(1);

            structDecl.Members[0].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            var ms = (MethodDeclarationSyntax)structDecl.Members[0];
            ms.AttributeLists.Count.Should().Be(0);
            ms.Modifiers.Count.Should().Be(1);
            ms.Modifiers[0].Kind().Should().Be(SyntaxKind.ReadOnlyKeyword);
            ms.ReturnType.Should().NotBeNull();
            ms.ReturnType.ToString().Should().Be("void");
            ms.Identifier.Should().NotBe(default);
            ms.Identifier.ToString().Should().Be("M");
            ms.ParameterList.OpenParenToken.Should().NotBe(default);
            ms.ParameterList.OpenParenToken.IsMissing.Should().BeFalse();
            ms.ParameterList.Parameters.Count.Should().Be(0);
            ms.ParameterList.CloseParenToken.Should().NotBe(default);
            ms.ParameterList.CloseParenToken.IsMissing.Should().BeFalse();
            ms.ConstraintClauses.Count.Should().Be(0);
            ms.Body.Should().NotBeNull();
            ms.Body.OpenBraceToken.Kind().Should().NotBe(SyntaxKind.None);
            ms.Body.CloseBraceToken.Kind().Should().NotBe(SyntaxKind.None);
            ms.SemicolonToken.Kind().Should().Be(SyntaxKind.None);
        }

        [Fact]
        public void TestReadOnlyRefReturning()
        {
            var text = "struct a { readonly ref readonly int M() { } }";
            var file = this.ParseFile(text, TestOptions.Regular);

            file.Should().NotBeNull();
            file.Members.Count.Should().Be(1);
            file.ToString().Should().Be(text);
            file.Errors().Length.Should().Be(0);

            file.Members[0].Kind().Should().Be(SyntaxKind.StructDeclaration);
            var structDecl = (TypeDeclarationSyntax)file.Members[0];
            structDecl.AttributeLists.Count.Should().Be(0);
            structDecl.Modifiers.Count.Should().Be(0);
            structDecl.Keyword.Should().NotBe(default);
            structDecl.Keyword.Kind().Should().Be(SyntaxKind.StructKeyword);
            structDecl.Identifier.Should().NotBe(default);
            structDecl.Identifier.ToString().Should().Be("a");
            structDecl.BaseList.Should().BeNull();
            structDecl.ConstraintClauses.Count.Should().Be(0);
            structDecl.OpenBraceToken.Should().NotBe(default);
            structDecl.CloseBraceToken.Should().NotBe(default);

            structDecl.Members.Count.Should().Be(1);

            structDecl.Members[0].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            var ms = (MethodDeclarationSyntax)structDecl.Members[0];
            ms.AttributeLists.Count.Should().Be(0);
            ms.Modifiers.Count.Should().Be(1);
            ms.Modifiers[0].Kind().Should().Be(SyntaxKind.ReadOnlyKeyword);
            ms.ReturnType.Kind().Should().Be(SyntaxKind.RefType);
            var rt = (RefTypeSyntax)ms.ReturnType;
            rt.RefKeyword.Kind().Should().Be(SyntaxKind.RefKeyword);
            rt.ReadOnlyKeyword.Kind().Should().Be(SyntaxKind.ReadOnlyKeyword);
            rt.Type.ToString().Should().Be("int");
            ms.Identifier.Should().NotBe(default);
            ms.Identifier.ToString().Should().Be("M");
            ms.ParameterList.OpenParenToken.Should().NotBe(default);
            ms.ParameterList.OpenParenToken.IsMissing.Should().BeFalse();
            ms.ParameterList.Parameters.Count.Should().Be(0);
            ms.ParameterList.CloseParenToken.Should().NotBe(default);
            ms.ParameterList.CloseParenToken.IsMissing.Should().BeFalse();
            ms.ConstraintClauses.Count.Should().Be(0);
            ms.Body.Should().NotBeNull();
            ms.Body.OpenBraceToken.Kind().Should().NotBe(SyntaxKind.None);
            ms.Body.CloseBraceToken.Kind().Should().NotBe(SyntaxKind.None);
            ms.SemicolonToken.Kind().Should().Be(SyntaxKind.None);
        }

        [Fact]
        public void TestStructExpressionPropertyWithReadonly()
        {
            var text = "struct a { readonly int M => 42; }";
            var file = this.ParseFile(text, TestOptions.Regular);

            file.Should().NotBeNull();
            file.Members.Count.Should().Be(1);
            file.ToString().Should().Be(text);
            file.Errors().Length.Should().Be(0);

            file.Members[0].Kind().Should().Be(SyntaxKind.StructDeclaration);
            var structDecl = (TypeDeclarationSyntax)file.Members[0];
            structDecl.AttributeLists.Count.Should().Be(0);
            structDecl.Modifiers.Count.Should().Be(0);
            structDecl.Keyword.Should().NotBe(default);
            structDecl.Keyword.Kind().Should().Be(SyntaxKind.StructKeyword);
            structDecl.Identifier.Should().NotBe(default);
            structDecl.Identifier.ToString().Should().Be("a");
            structDecl.BaseList.Should().BeNull();
            structDecl.ConstraintClauses.Count.Should().Be(0);
            structDecl.OpenBraceToken.Should().NotBe(default);
            structDecl.CloseBraceToken.Should().NotBe(default);

            structDecl.Members.Count.Should().Be(1);

            structDecl.Members[0].Kind().Should().Be(SyntaxKind.PropertyDeclaration);
            var propertySyntax = (PropertyDeclarationSyntax)structDecl.Members[0];
            propertySyntax.AttributeLists.Count.Should().Be(0);
            propertySyntax.Modifiers.Count.Should().Be(1);
            propertySyntax.Modifiers[0].Kind().Should().Be(SyntaxKind.ReadOnlyKeyword);
            propertySyntax.Type.Should().NotBeNull();
            propertySyntax.Type.ToString().Should().Be("int");
            propertySyntax.Identifier.Should().NotBe(default);
            propertySyntax.Identifier.ToString().Should().Be("M");
            propertySyntax.ExpressionBody.Should().NotBeNull();
            propertySyntax.ExpressionBody.ArrowToken.Kind().Should().NotBe(SyntaxKind.None);
            propertySyntax.ExpressionBody.Expression.Should().NotBeNull();
            propertySyntax.SemicolonToken.Kind().Should().Be(SyntaxKind.SemicolonToken);
        }

        [Fact]
        public void TestStructGetterPropertyWithReadonly()
        {
            var text = "struct a { int P { readonly get { return 42; } } }";
            var file = this.ParseFile(text, TestOptions.Regular);

            file.Should().NotBeNull();
            file.Members.Count.Should().Be(1);
            file.ToString().Should().Be(text);
            file.Errors().Length.Should().Be(0);

            file.Members[0].Kind().Should().Be(SyntaxKind.StructDeclaration);
            var structDecl = (TypeDeclarationSyntax)file.Members[0];
            structDecl.AttributeLists.Count.Should().Be(0);
            structDecl.Modifiers.Count.Should().Be(0);
            structDecl.Keyword.Should().NotBe(default);
            structDecl.Keyword.Kind().Should().Be(SyntaxKind.StructKeyword);
            structDecl.Identifier.Should().NotBe(default);
            structDecl.Identifier.ToString().Should().Be("a");
            structDecl.BaseList.Should().BeNull();
            structDecl.ConstraintClauses.Count.Should().Be(0);
            structDecl.OpenBraceToken.Should().NotBe(default);
            structDecl.CloseBraceToken.Should().NotBe(default);

            structDecl.Members.Count.Should().Be(1);

            structDecl.Members[0].Kind().Should().Be(SyntaxKind.PropertyDeclaration);
            var propertySyntax = (PropertyDeclarationSyntax)structDecl.Members[0];
            propertySyntax.AttributeLists.Count.Should().Be(0);
            propertySyntax.Modifiers.Count.Should().Be(0);
            propertySyntax.Type.Should().NotBeNull();
            propertySyntax.Type.ToString().Should().Be("int");
            propertySyntax.Identifier.Should().NotBe(default);
            propertySyntax.Identifier.ToString().Should().Be("P");
            var accessors = propertySyntax.AccessorList.Accessors;
            accessors.Count.Should().Be(1);
            accessors[0].Modifiers.Count.Should().Be(1);
            accessors[0].Modifiers[0].Kind().Should().Be(SyntaxKind.ReadOnlyKeyword);
        }

        [Fact]
        public void TestStructBadExpressionProperty()
        {
            var text =
@"public struct S
{
    public int P readonly => 0;
}
";
            var file = this.ParseFile(text, TestOptions.Regular);

            file.Should().NotBeNull();
            file.Members.Count.Should().Be(1);
            file.ToString().Should().Be(text);

            file.Errors().Length.Should().Be(3);
            (ErrorCode)file.Errors()[0].Code.Should().Be(ErrorCode.ERR_SemicolonExpected);
            (ErrorCode)file.Errors()[1].Code.Should().Be(ErrorCode.ERR_InvalidMemberDecl);
            (ErrorCode)file.Errors()[2].Code.Should().Be(ErrorCode.ERR_InvalidMemberDecl);
        }

        [Fact]
        public void TestClassMethodWithParameter()
        {
            var text = "class a { b X(c d) { } }";
            var file = this.ParseFile(text);

            file.Should().NotBeNull();
            file.Members.Count.Should().Be(1);
            file.ToString().Should().Be(text);
            file.Errors().Length.Should().Be(0);

            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var cs = (TypeDeclarationSyntax)file.Members[0];
            cs.AttributeLists.Count.Should().Be(0);
            cs.Modifiers.Count.Should().Be(0);
            cs.Keyword.Should().NotBe(default);
            cs.Keyword.Kind().Should().Be(SyntaxKind.ClassKeyword);
            cs.Identifier.Should().NotBe(default);
            cs.Identifier.ToString().Should().Be("a");
            cs.BaseList.Should().BeNull();
            cs.ConstraintClauses.Count.Should().Be(0);
            cs.OpenBraceToken.Should().NotBe(default);
            cs.CloseBraceToken.Should().NotBe(default);

            cs.Members.Count.Should().Be(1);

            cs.Members[0].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            var ms = (MethodDeclarationSyntax)cs.Members[0];
            ms.AttributeLists.Count.Should().Be(0);
            ms.Modifiers.Count.Should().Be(0);
            ms.ReturnType.Should().NotBeNull();
            ms.ReturnType.ToString().Should().Be("b");
            ms.Identifier.Should().NotBe(default);
            ms.Identifier.ToString().Should().Be("X");
            ms.ParameterList.OpenParenToken.Should().NotBe(default);
            ms.ParameterList.OpenParenToken.IsMissing.Should().BeFalse();
            ms.ParameterList.Parameters.Count.Should().Be(1);
            ms.ParameterList.Parameters[0].AttributeLists.Count.Should().Be(0);
            ms.ParameterList.Parameters[0].Modifiers.Count.Should().Be(0);
            ms.ParameterList.Parameters[0].Type.Should().NotBeNull();
            ms.ParameterList.Parameters[0].Type.ToString().Should().Be("c");
            ms.ParameterList.Parameters[0].Identifier.Should().NotBe(default);
            ms.ParameterList.Parameters[0].Identifier.ToString().Should().Be("d");
            ms.ParameterList.CloseParenToken.Should().NotBe(default);
            ms.ParameterList.CloseParenToken.IsMissing.Should().BeFalse();
            ms.ConstraintClauses.Count.Should().Be(0);
            ms.Body.Should().NotBeNull();
            ms.Body.OpenBraceToken.Kind().Should().NotBe(SyntaxKind.None);
            ms.Body.CloseBraceToken.Kind().Should().NotBe(SyntaxKind.None);
            ms.SemicolonToken.Kind().Should().Be(SyntaxKind.None);
        }

        [Fact]
        public void TestClassMethodWithMultipleParameters()
        {
            var text = "class a { b X(c d, e f) { } }";
            var file = this.ParseFile(text);

            file.Should().NotBeNull();
            file.Members.Count.Should().Be(1);
            file.ToString().Should().Be(text);
            file.Errors().Length.Should().Be(0);

            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var cs = (TypeDeclarationSyntax)file.Members[0];
            cs.AttributeLists.Count.Should().Be(0);
            cs.Modifiers.Count.Should().Be(0);
            cs.Keyword.Should().NotBe(default);
            cs.Keyword.Kind().Should().Be(SyntaxKind.ClassKeyword);
            cs.Identifier.Should().NotBe(default);
            cs.Identifier.ToString().Should().Be("a");
            cs.BaseList.Should().BeNull();
            cs.ConstraintClauses.Count.Should().Be(0);
            cs.OpenBraceToken.Should().NotBe(default);
            cs.CloseBraceToken.Should().NotBe(default);

            cs.Members.Count.Should().Be(1);

            cs.Members[0].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            var ms = (MethodDeclarationSyntax)cs.Members[0];
            ms.AttributeLists.Count.Should().Be(0);
            ms.Modifiers.Count.Should().Be(0);
            ms.ReturnType.Should().NotBeNull();
            ms.ReturnType.ToString().Should().Be("b");
            ms.Identifier.Should().NotBe(default);
            ms.Identifier.ToString().Should().Be("X");
            ms.ParameterList.OpenParenToken.Should().NotBe(default);
            ms.ParameterList.OpenParenToken.IsMissing.Should().BeFalse();

            ms.ParameterList.Parameters.Count.Should().Be(2);

            ms.ParameterList.Parameters[0].AttributeLists.Count.Should().Be(0);
            ms.ParameterList.Parameters[0].Modifiers.Count.Should().Be(0);
            ms.ParameterList.Parameters[0].Type.Should().NotBeNull();
            ms.ParameterList.Parameters[0].Type.ToString().Should().Be("c");
            ms.ParameterList.Parameters[0].Identifier.Should().NotBe(default);
            ms.ParameterList.Parameters[0].Identifier.ToString().Should().Be("d");

            ms.ParameterList.Parameters[1].AttributeLists.Count.Should().Be(0);
            ms.ParameterList.Parameters[1].Modifiers.Count.Should().Be(0);
            ms.ParameterList.Parameters[1].Type.Should().NotBeNull();
            ms.ParameterList.Parameters[1].Type.ToString().Should().Be("e");
            ms.ParameterList.Parameters[1].Identifier.Should().NotBe(default);
            ms.ParameterList.Parameters[1].Identifier.ToString().Should().Be("f");

            ms.ParameterList.CloseParenToken.Should().NotBe(default);
            ms.ParameterList.CloseParenToken.IsMissing.Should().BeFalse();
            ms.ConstraintClauses.Count.Should().Be(0);
            ms.Body.Should().NotBeNull();
            ms.Body.OpenBraceToken.Kind().Should().NotBe(SyntaxKind.None);
            ms.Body.CloseBraceToken.Kind().Should().NotBe(SyntaxKind.None);
            ms.SemicolonToken.Kind().Should().Be(SyntaxKind.None);
        }

        private void TestClassMethodWithParameterModifier(SyntaxKind mod)
        {
            var text = "class a { b X(" + SyntaxFacts.GetText(mod) + " c d) { } }";
            var file = this.ParseFile(text);

            file.Should().NotBeNull();
            file.Members.Count.Should().Be(1);
            file.ToString().Should().Be(text);
            file.Errors().Length.Should().Be(0);

            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var cs = (TypeDeclarationSyntax)file.Members[0];
            cs.AttributeLists.Count.Should().Be(0);
            cs.Modifiers.Count.Should().Be(0);
            cs.Keyword.Should().NotBe(default);
            cs.Keyword.Kind().Should().Be(SyntaxKind.ClassKeyword);
            cs.Identifier.Should().NotBe(default);
            cs.Identifier.ToString().Should().Be("a");
            cs.BaseList.Should().BeNull();
            cs.ConstraintClauses.Count.Should().Be(0);
            cs.OpenBraceToken.Should().NotBe(default);
            cs.CloseBraceToken.Should().NotBe(default);

            cs.Members.Count.Should().Be(1);

            cs.Members[0].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            var ms = (MethodDeclarationSyntax)cs.Members[0];
            ms.AttributeLists.Count.Should().Be(0);
            ms.Modifiers.Count.Should().Be(0);
            ms.ReturnType.Should().NotBeNull();
            ms.ReturnType.ToString().Should().Be("b");
            ms.Identifier.Should().NotBe(default);
            ms.Identifier.ToString().Should().Be("X");
            ms.ParameterList.OpenParenToken.Should().NotBe(default);
            ms.ParameterList.OpenParenToken.IsMissing.Should().BeFalse();

            ms.ParameterList.Parameters.Count.Should().Be(1);

            ms.ParameterList.Parameters[0].AttributeLists.Count.Should().Be(0);
            ms.ParameterList.Parameters[0].Modifiers.Count.Should().Be(1);
            ms.ParameterList.Parameters[0].Modifiers[0].Kind().Should().Be(mod);
            ms.ParameterList.Parameters[0].Type.Should().NotBeNull();
            ms.ParameterList.Parameters[0].Type.ToString().Should().Be("c");
            ms.ParameterList.Parameters[0].Identifier.Should().NotBe(default);
            ms.ParameterList.Parameters[0].Identifier.ToString().Should().Be("d");

            ms.ParameterList.CloseParenToken.Should().NotBe(default);
            ms.ParameterList.CloseParenToken.IsMissing.Should().BeFalse();
            ms.ConstraintClauses.Count.Should().Be(0);
            ms.Body.Should().NotBeNull();
            ms.Body.OpenBraceToken.Kind().Should().NotBe(SyntaxKind.None);
            ms.Body.CloseBraceToken.Kind().Should().NotBe(SyntaxKind.None);
            ms.SemicolonToken.Kind().Should().Be(SyntaxKind.None);
        }

        [Fact]
        public void TestClassMethodWithParameterModifiers()
        {
            TestClassMethodWithParameterModifier(SyntaxKind.RefKeyword);
            TestClassMethodWithParameterModifier(SyntaxKind.OutKeyword);
            TestClassMethodWithParameterModifier(SyntaxKind.ParamsKeyword);
            TestClassMethodWithParameterModifier(SyntaxKind.ThisKeyword);
        }

        [Fact]
        public void TestClassMethodWithArgListParameter()
        {
            var text = "class a { b X(__arglist) { } }";
            var file = this.ParseFile(text);

            file.Should().NotBeNull();
            file.Members.Count.Should().Be(1);
            file.ToString().Should().Be(text);
            file.Errors().Length.Should().Be(0);

            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var cs = (TypeDeclarationSyntax)file.Members[0];
            cs.AttributeLists.Count.Should().Be(0);
            cs.Modifiers.Count.Should().Be(0);
            cs.Keyword.Should().NotBe(default);
            cs.Keyword.Kind().Should().Be(SyntaxKind.ClassKeyword);
            cs.Identifier.Should().NotBe(default);
            cs.Identifier.ToString().Should().Be("a");
            cs.BaseList.Should().BeNull();
            cs.ConstraintClauses.Count.Should().Be(0);
            cs.OpenBraceToken.Should().NotBe(default);
            cs.CloseBraceToken.Should().NotBe(default);

            cs.Members.Count.Should().Be(1);

            cs.Members[0].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            var ms = (MethodDeclarationSyntax)cs.Members[0];
            ms.AttributeLists.Count.Should().Be(0);
            ms.Modifiers.Count.Should().Be(0);
            ms.ReturnType.Should().NotBeNull();
            ms.ReturnType.ToString().Should().Be("b");
            ms.Identifier.Should().NotBe(default);
            ms.Identifier.ToString().Should().Be("X");
            ms.ParameterList.OpenParenToken.Should().NotBe(default);
            ms.ParameterList.OpenParenToken.IsMissing.Should().BeFalse();

            ms.ParameterList.Parameters.Count.Should().Be(1);

            ms.ParameterList.Parameters[0].AttributeLists.Count.Should().Be(0);
            ms.ParameterList.Parameters[0].Modifiers.Count.Should().Be(0);
            ms.ParameterList.Parameters[0].Type.Should().BeNull();
            ms.ParameterList.Parameters[0].Identifier.Should().NotBe(default);
            ms.ParameterList.Parameters[0].Identifier.Kind().Should().Be(SyntaxKind.ArgListKeyword);

            ms.ParameterList.CloseParenToken.Should().NotBe(default);
            ms.ParameterList.CloseParenToken.IsMissing.Should().BeFalse();
            ms.ConstraintClauses.Count.Should().Be(0);
            ms.Body.Should().NotBeNull();
            ms.Body.OpenBraceToken.Kind().Should().NotBe(SyntaxKind.None);
            ms.Body.CloseBraceToken.Kind().Should().NotBe(SyntaxKind.None);
            ms.SemicolonToken.Kind().Should().Be(SyntaxKind.None);
        }

        [Fact]
        public void TestClassMethodWithBuiltInReturnTypes()
        {
            TestClassMethodWithBuiltInReturnType(SyntaxKind.VoidKeyword);
            TestClassMethodWithBuiltInReturnType(SyntaxKind.BoolKeyword);
            TestClassMethodWithBuiltInReturnType(SyntaxKind.SByteKeyword);
            TestClassMethodWithBuiltInReturnType(SyntaxKind.IntKeyword);
            TestClassMethodWithBuiltInReturnType(SyntaxKind.UIntKeyword);
            TestClassMethodWithBuiltInReturnType(SyntaxKind.ShortKeyword);
            TestClassMethodWithBuiltInReturnType(SyntaxKind.UShortKeyword);
            TestClassMethodWithBuiltInReturnType(SyntaxKind.LongKeyword);
            TestClassMethodWithBuiltInReturnType(SyntaxKind.ULongKeyword);
            TestClassMethodWithBuiltInReturnType(SyntaxKind.FloatKeyword);
            TestClassMethodWithBuiltInReturnType(SyntaxKind.DoubleKeyword);
            TestClassMethodWithBuiltInReturnType(SyntaxKind.DecimalKeyword);
            TestClassMethodWithBuiltInReturnType(SyntaxKind.StringKeyword);
            TestClassMethodWithBuiltInReturnType(SyntaxKind.CharKeyword);
            TestClassMethodWithBuiltInReturnType(SyntaxKind.ObjectKeyword);
        }

        private void TestClassMethodWithBuiltInReturnType(SyntaxKind type)
        {
            var typeText = SyntaxFacts.GetText(type);
            var text = "class a { " + typeText + " M() { } }";
            var file = this.ParseFile(text);

            file.Should().NotBeNull();
            file.Members.Count.Should().Be(1);
            file.ToString().Should().Be(text);
            file.Errors().Length.Should().Be(0);

            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var cs = (TypeDeclarationSyntax)file.Members[0];
            cs.AttributeLists.Count.Should().Be(0);
            cs.Modifiers.Count.Should().Be(0);
            cs.Keyword.Should().NotBe(default);
            cs.Keyword.Kind().Should().Be(SyntaxKind.ClassKeyword);
            cs.Identifier.Should().NotBe(default);
            cs.Identifier.ToString().Should().Be("a");
            cs.BaseList.Should().BeNull();
            cs.ConstraintClauses.Count.Should().Be(0);
            cs.OpenBraceToken.Should().NotBe(default);
            cs.CloseBraceToken.Should().NotBe(default);

            cs.Members.Count.Should().Be(1);

            cs.Members[0].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            var ms = (MethodDeclarationSyntax)cs.Members[0];
            ms.AttributeLists.Count.Should().Be(0);
            ms.Modifiers.Count.Should().Be(0);
            ms.ReturnType.Should().NotBeNull();
            ms.ReturnType.ToString().Should().Be(typeText);
            ms.Identifier.Should().NotBe(default);
            ms.Identifier.ToString().Should().Be("M");
            ms.ParameterList.OpenParenToken.Should().NotBe(default);
            ms.ParameterList.OpenParenToken.IsMissing.Should().BeFalse();
            ms.ParameterList.Parameters.Count.Should().Be(0);
            ms.ParameterList.CloseParenToken.Should().NotBe(default);
            ms.ParameterList.CloseParenToken.IsMissing.Should().BeFalse();
            ms.ConstraintClauses.Count.Should().Be(0);
            ms.Body.Should().NotBeNull();
            ms.Body.OpenBraceToken.Kind().Should().NotBe(SyntaxKind.None);
            ms.Body.CloseBraceToken.Kind().Should().NotBe(SyntaxKind.None);
            ms.SemicolonToken.Kind().Should().Be(SyntaxKind.None);
        }

        [Fact]
        public void TestClassMethodWithBuiltInParameterTypes()
        {
            TestClassMethodWithBuiltInParameterType(SyntaxKind.BoolKeyword);
            TestClassMethodWithBuiltInParameterType(SyntaxKind.SByteKeyword);
            TestClassMethodWithBuiltInParameterType(SyntaxKind.IntKeyword);
            TestClassMethodWithBuiltInParameterType(SyntaxKind.UIntKeyword);
            TestClassMethodWithBuiltInParameterType(SyntaxKind.ShortKeyword);
            TestClassMethodWithBuiltInParameterType(SyntaxKind.UShortKeyword);
            TestClassMethodWithBuiltInParameterType(SyntaxKind.LongKeyword);
            TestClassMethodWithBuiltInParameterType(SyntaxKind.ULongKeyword);
            TestClassMethodWithBuiltInParameterType(SyntaxKind.FloatKeyword);
            TestClassMethodWithBuiltInParameterType(SyntaxKind.DoubleKeyword);
            TestClassMethodWithBuiltInParameterType(SyntaxKind.DecimalKeyword);
            TestClassMethodWithBuiltInParameterType(SyntaxKind.StringKeyword);
            TestClassMethodWithBuiltInParameterType(SyntaxKind.CharKeyword);
            TestClassMethodWithBuiltInParameterType(SyntaxKind.ObjectKeyword);
        }

        private void TestClassMethodWithBuiltInParameterType(SyntaxKind type)
        {
            var typeText = SyntaxFacts.GetText(type);
            var text = "class a { b X(" + typeText + " c) { } }";
            var file = this.ParseFile(text);

            file.Should().NotBeNull();
            file.Members.Count.Should().Be(1);
            file.ToString().Should().Be(text);
            file.Errors().Length.Should().Be(0);

            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var cs = (TypeDeclarationSyntax)file.Members[0];
            cs.AttributeLists.Count.Should().Be(0);
            cs.Modifiers.Count.Should().Be(0);
            cs.Keyword.Should().NotBe(default);
            cs.Keyword.Kind().Should().Be(SyntaxKind.ClassKeyword);
            cs.Identifier.Should().NotBe(default);
            cs.Identifier.ToString().Should().Be("a");
            cs.BaseList.Should().BeNull();
            cs.ConstraintClauses.Count.Should().Be(0);
            cs.OpenBraceToken.Should().NotBe(default);
            cs.CloseBraceToken.Should().NotBe(default);

            cs.Members.Count.Should().Be(1);

            cs.Members[0].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            var ms = (MethodDeclarationSyntax)cs.Members[0];
            ms.AttributeLists.Count.Should().Be(0);
            ms.Modifiers.Count.Should().Be(0);
            ms.ReturnType.Should().NotBeNull();
            ms.ReturnType.ToString().Should().Be("b");
            ms.Identifier.Should().NotBe(default);
            ms.Identifier.ToString().Should().Be("X");
            ms.ParameterList.OpenParenToken.Should().NotBe(default);
            ms.ParameterList.OpenParenToken.IsMissing.Should().BeFalse();
            ms.ParameterList.Parameters.Count.Should().Be(1);
            ms.ParameterList.Parameters[0].AttributeLists.Count.Should().Be(0);
            ms.ParameterList.Parameters[0].Modifiers.Count.Should().Be(0);
            ms.ParameterList.Parameters[0].Type.Should().NotBeNull();
            ms.ParameterList.Parameters[0].Type.ToString().Should().Be(typeText);
            ms.ParameterList.Parameters[0].Identifier.Should().NotBe(default);
            ms.ParameterList.Parameters[0].Identifier.ToString().Should().Be("c");
            ms.ParameterList.CloseParenToken.Should().NotBe(default);
            ms.ParameterList.CloseParenToken.IsMissing.Should().BeFalse();
            ms.ConstraintClauses.Count.Should().Be(0);
            ms.Body.Should().NotBeNull();
            ms.Body.OpenBraceToken.Kind().Should().NotBe(SyntaxKind.None);
            ms.Body.CloseBraceToken.Kind().Should().NotBe(SyntaxKind.None);
            ms.SemicolonToken.Kind().Should().Be(SyntaxKind.None);
        }

        [Fact]
        public void TestGenericClassMethod()
        {
            var text = "class a { b<c> M() { } }";
            var file = this.ParseFile(text);

            file.Should().NotBeNull();
            file.Members.Count.Should().Be(1);
            file.ToString().Should().Be(text);
            file.Errors().Length.Should().Be(0);

            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var cs = (TypeDeclarationSyntax)file.Members[0];
            cs.AttributeLists.Count.Should().Be(0);
            cs.Modifiers.Count.Should().Be(0);
            cs.Keyword.Should().NotBe(default);
            cs.Keyword.Kind().Should().Be(SyntaxKind.ClassKeyword);
            cs.Identifier.Should().NotBe(default);
            cs.Identifier.ToString().Should().Be("a");
            cs.BaseList.Should().BeNull();
            cs.ConstraintClauses.Count.Should().Be(0);
            cs.OpenBraceToken.Should().NotBe(default);
            cs.CloseBraceToken.Should().NotBe(default);

            cs.Members.Count.Should().Be(1);

            cs.Members[0].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            var ms = (MethodDeclarationSyntax)cs.Members[0];
            ms.AttributeLists.Count.Should().Be(0);
            ms.Modifiers.Count.Should().Be(0);
            ms.ReturnType.Should().NotBeNull();
            ms.ReturnType.ToString().Should().Be("b<c>");
            ms.Identifier.Should().NotBe(default);
            ms.Identifier.ToString().Should().Be("M");
            ms.ParameterList.OpenParenToken.Should().NotBe(default);
            ms.ParameterList.OpenParenToken.IsMissing.Should().BeFalse();
            ms.ParameterList.Parameters.Count.Should().Be(0);
            ms.ParameterList.CloseParenToken.Should().NotBe(default);
            ms.ParameterList.CloseParenToken.IsMissing.Should().BeFalse();
            ms.ConstraintClauses.Count.Should().Be(0);
            ms.Body.Should().NotBeNull();
            ms.Body.OpenBraceToken.Kind().Should().NotBe(SyntaxKind.None);
            ms.Body.CloseBraceToken.Kind().Should().NotBe(SyntaxKind.None);
            ms.SemicolonToken.Kind().Should().Be(SyntaxKind.None);
        }

        [Fact]
        public void TestGenericClassMethodWithTypeConstraintBound()
        {
            var text = "class a { b X<c>() where b : d { } }";
            var file = this.ParseFile(text);

            file.Should().NotBeNull();
            file.Members.Count.Should().Be(1);
            file.ToString().Should().Be(text);
            file.Errors().Length.Should().Be(0);

            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var cs = (TypeDeclarationSyntax)file.Members[0];
            cs.AttributeLists.Count.Should().Be(0);
            cs.Modifiers.Count.Should().Be(0);
            cs.Keyword.Should().NotBe(default);
            cs.Keyword.Kind().Should().Be(SyntaxKind.ClassKeyword);
            cs.Identifier.Should().NotBe(default);
            cs.Identifier.ToString().Should().Be("a");
            cs.BaseList.Should().BeNull();
            cs.ConstraintClauses.Count.Should().Be(0);
            cs.OpenBraceToken.Should().NotBe(default);
            cs.CloseBraceToken.Should().NotBe(default);

            cs.Members.Count.Should().Be(1);

            cs.Members[0].Kind().Should().Be(SyntaxKind.MethodDeclaration);
            var ms = (MethodDeclarationSyntax)cs.Members[0];
            ms.AttributeLists.Count.Should().Be(0);
            ms.Modifiers.Count.Should().Be(0);
            ms.ReturnType.Should().NotBeNull();
            ms.ReturnType.ToString().Should().Be("b");
            ms.Identifier.Should().NotBe(default);
            ms.TypeParameterList.Should().NotBeNull();
            ms.Identifier.ToString().Should().Be("X");
            ms.TypeParameterList.ToString().Should().Be("<c>");
            ms.ParameterList.OpenParenToken.Should().NotBe(default);
            ms.ParameterList.OpenParenToken.IsMissing.Should().BeFalse();
            ms.ParameterList.Parameters.Count.Should().Be(0);
            ms.ParameterList.CloseParenToken.Should().NotBe(default);
            ms.ParameterList.CloseParenToken.IsMissing.Should().BeFalse();

            ms.ConstraintClauses.Count.Should().Be(1);
            ms.ConstraintClauses[0].WhereKeyword.Should().NotBe(default);
            ms.ConstraintClauses[0].Name.Should().NotBeNull();
            ms.ConstraintClauses[0].Name.ToString().Should().Be("b");
            ms.ConstraintClauses[0].ColonToken.Should().NotBe(default);
            ms.ConstraintClauses[0].ColonToken.IsMissing.Should().BeFalse();
            ms.ConstraintClauses[0].Constraints.Count.Should().Be(1);
            ms.ConstraintClauses[0].Constraints[0].Kind().Should().Be(SyntaxKind.TypeConstraint);
            var typeBound = (TypeConstraintSyntax)ms.ConstraintClauses[0].Constraints[0];
            typeBound.Type.Should().NotBeNull();
            typeBound.Type.ToString().Should().Be("d");

            ms.Body.Should().NotBeNull();
            ms.Body.OpenBraceToken.Kind().Should().NotBe(SyntaxKind.None);
            ms.Body.CloseBraceToken.Kind().Should().NotBe(SyntaxKind.None);
            ms.SemicolonToken.Kind().Should().Be(SyntaxKind.None);
        }

        [WorkItem(899685, "DevDiv/Personal")]
        [Fact]
        public void TestGenericClassConstructor()
        {
            var text = @"
class Class1<T>{
    public Class1() { }
}
";
            var file = this.ParseFile(text);

            file.Should().NotBeNull();
            file.Members.Count.Should().Be(1);

            // verify that we can roundtrip
            file.ToFullString().Should().Be(text);

            // verify that we don't produce any errors
            file.Errors().Length.Should().Be(0);
        }

        [Fact]
        public void TestClassConstructor()
        {
            var text = "class a { a() { } }";
            var file = this.ParseFile(text);

            file.Should().NotBeNull();
            file.Members.Count.Should().Be(1);
            file.ToString().Should().Be(text);
            file.Errors().Length.Should().Be(0);

            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var cs = (TypeDeclarationSyntax)file.Members[0];
            cs.AttributeLists.Count.Should().Be(0);
            cs.Modifiers.Count.Should().Be(0);
            cs.Keyword.Should().NotBe(default);
            cs.Keyword.Kind().Should().Be(SyntaxKind.ClassKeyword);
            cs.Identifier.Should().NotBe(default);
            cs.Identifier.ToString().Should().Be("a");
            cs.BaseList.Should().BeNull();
            cs.ConstraintClauses.Count.Should().Be(0);
            cs.OpenBraceToken.Kind().Should().NotBe(SyntaxKind.None);
            cs.CloseBraceToken.Kind().Should().NotBe(SyntaxKind.None);
            cs.SemicolonToken.Kind().Should().Be(SyntaxKind.None);

            cs.Members.Count.Should().Be(1);

            cs.Members[0].Kind().Should().Be(SyntaxKind.ConstructorDeclaration);
            var cn = (ConstructorDeclarationSyntax)cs.Members[0];
            cn.AttributeLists.Count.Should().Be(0);
            cn.Modifiers.Count.Should().Be(0);
            cn.Body.Should().NotBeNull();
            cn.Body.OpenBraceToken.Should().NotBe(default);
            cn.Body.CloseBraceToken.Should().NotBe(default);
        }

        private void TestClassConstructorWithModifier(SyntaxKind mod)
        {
            var text = "class a { " + SyntaxFacts.GetText(mod) + " a() { } }";
            var file = this.ParseFile(text);

            file.Should().NotBeNull();
            file.Members.Count.Should().Be(1);
            file.ToString().Should().Be(text);
            file.Errors().Length.Should().Be(0);

            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var cs = (TypeDeclarationSyntax)file.Members[0];
            cs.AttributeLists.Count.Should().Be(0);
            cs.Modifiers.Count.Should().Be(0);
            cs.Keyword.Should().NotBe(default);
            cs.Keyword.Kind().Should().Be(SyntaxKind.ClassKeyword);
            cs.Identifier.Should().NotBe(default);
            cs.Identifier.ToString().Should().Be("a");
            cs.BaseList.Should().BeNull();
            cs.ConstraintClauses.Count.Should().Be(0);
            cs.OpenBraceToken.Kind().Should().NotBe(SyntaxKind.None);
            cs.CloseBraceToken.Kind().Should().NotBe(SyntaxKind.None);
            cs.SemicolonToken.Kind().Should().Be(SyntaxKind.None);

            cs.Members.Count.Should().Be(1);

            cs.Members[0].Kind().Should().Be(SyntaxKind.ConstructorDeclaration);
            var cn = (ConstructorDeclarationSyntax)cs.Members[0];
            cn.AttributeLists.Count.Should().Be(0);
            cn.Modifiers.Count.Should().Be(1);
            cn.Modifiers[0].Kind().Should().Be(mod);
            cn.Body.Should().NotBeNull();
            cn.Body.OpenBraceToken.Should().NotBe(default);
            cn.Body.CloseBraceToken.Should().NotBe(default);
        }

        [Fact]
        public void TestClassConstructorWithModifiers()
        {
            TestClassConstructorWithModifier(SyntaxKind.PublicKeyword);
            TestClassConstructorWithModifier(SyntaxKind.PrivateKeyword);
            TestClassConstructorWithModifier(SyntaxKind.ProtectedKeyword);
            TestClassConstructorWithModifier(SyntaxKind.InternalKeyword);
            TestClassConstructorWithModifier(SyntaxKind.StaticKeyword);
        }

        [Fact]
        public void TestClassDestructor()
        {
            var text = "class a { ~a() { } }";
            var file = this.ParseFile(text);

            file.Should().NotBeNull();
            file.Members.Count.Should().Be(1);
            file.ToString().Should().Be(text);
            file.Errors().Length.Should().Be(0);

            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var cs = (TypeDeclarationSyntax)file.Members[0];
            cs.AttributeLists.Count.Should().Be(0);
            cs.Modifiers.Count.Should().Be(0);
            cs.Keyword.Should().NotBe(default);
            cs.Keyword.Kind().Should().Be(SyntaxKind.ClassKeyword);
            cs.Identifier.Should().NotBe(default);
            cs.Identifier.ToString().Should().Be("a");
            cs.BaseList.Should().BeNull();
            cs.ConstraintClauses.Count.Should().Be(0);
            cs.OpenBraceToken.Kind().Should().NotBe(SyntaxKind.None);
            cs.CloseBraceToken.Kind().Should().NotBe(SyntaxKind.None);
            cs.SemicolonToken.Kind().Should().Be(SyntaxKind.None);

            cs.Members.Count.Should().Be(1);

            cs.Members[0].Kind().Should().Be(SyntaxKind.DestructorDeclaration);
            var cn = (DestructorDeclarationSyntax)cs.Members[0];
            cn.TildeToken.Should().NotBe(default);
            cn.AttributeLists.Count.Should().Be(0);
            cn.Modifiers.Count.Should().Be(0);
            cn.Body.Should().NotBeNull();
            cn.Body.OpenBraceToken.Should().NotBe(default);
            cn.Body.CloseBraceToken.Should().NotBe(default);
        }

        [Fact]
        public void TestClassField()
        {
            var text = "class a { b c; }";
            var file = this.ParseFile(text);

            file.Should().NotBeNull();
            file.Members.Count.Should().Be(1);
            file.ToString().Should().Be(text);
            file.Errors().Length.Should().Be(0);

            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var cs = (TypeDeclarationSyntax)file.Members[0];
            cs.AttributeLists.Count.Should().Be(0);
            cs.Modifiers.Count.Should().Be(0);
            cs.Keyword.Should().NotBe(default);
            cs.Keyword.Kind().Should().Be(SyntaxKind.ClassKeyword);
            cs.Identifier.Should().NotBe(default);
            cs.Identifier.ToString().Should().Be("a");
            cs.BaseList.Should().BeNull();
            cs.ConstraintClauses.Count.Should().Be(0);
            cs.OpenBraceToken.Should().NotBe(default);
            cs.CloseBraceToken.Should().NotBe(default);

            cs.Members.Count.Should().Be(1);

            cs.Members[0].Kind().Should().Be(SyntaxKind.FieldDeclaration);
            var fs = (FieldDeclarationSyntax)cs.Members[0];
            fs.AttributeLists.Count.Should().Be(0);
            fs.Modifiers.Count.Should().Be(0);
            fs.Declaration.Type.Should().NotBeNull();
            fs.Declaration.Type.ToString().Should().Be("b");
            fs.Declaration.Variables.Count.Should().Be(1);
            fs.Declaration.Variables[0].Identifier.Should().NotBe(default);
            fs.Declaration.Variables[0].Identifier.ToString().Should().Be("c");
            fs.Declaration.Variables[0].ArgumentList.Should().BeNull();
            fs.Declaration.Variables[0].Initializer.Should().BeNull();
            fs.SemicolonToken.Should().NotBe(default);
            fs.SemicolonToken.IsMissing.Should().BeFalse();
        }

        [Fact]
        public void TestClassFieldWithBuiltInTypes()
        {
            TestClassFieldWithBuiltInType(SyntaxKind.BoolKeyword);
            TestClassFieldWithBuiltInType(SyntaxKind.SByteKeyword);
            TestClassFieldWithBuiltInType(SyntaxKind.IntKeyword);
            TestClassFieldWithBuiltInType(SyntaxKind.UIntKeyword);
            TestClassFieldWithBuiltInType(SyntaxKind.ShortKeyword);
            TestClassFieldWithBuiltInType(SyntaxKind.UShortKeyword);
            TestClassFieldWithBuiltInType(SyntaxKind.LongKeyword);
            TestClassFieldWithBuiltInType(SyntaxKind.ULongKeyword);
            TestClassFieldWithBuiltInType(SyntaxKind.FloatKeyword);
            TestClassFieldWithBuiltInType(SyntaxKind.DoubleKeyword);
            TestClassFieldWithBuiltInType(SyntaxKind.DecimalKeyword);
            TestClassFieldWithBuiltInType(SyntaxKind.StringKeyword);
            TestClassFieldWithBuiltInType(SyntaxKind.CharKeyword);
            TestClassFieldWithBuiltInType(SyntaxKind.ObjectKeyword);
        }

        private void TestClassFieldWithBuiltInType(SyntaxKind type)
        {
            var typeText = SyntaxFacts.GetText(type);
            var text = "class a { " + typeText + " c; }";
            var file = this.ParseFile(text);

            file.Should().NotBeNull();
            file.Members.Count.Should().Be(1);
            file.ToString().Should().Be(text);
            file.Errors().Length.Should().Be(0);

            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var cs = (TypeDeclarationSyntax)file.Members[0];
            cs.AttributeLists.Count.Should().Be(0);
            cs.Modifiers.Count.Should().Be(0);
            cs.Keyword.Should().NotBe(default);
            cs.Keyword.Kind().Should().Be(SyntaxKind.ClassKeyword);
            cs.Identifier.Should().NotBe(default);
            cs.Identifier.ToString().Should().Be("a");
            cs.BaseList.Should().BeNull();
            cs.ConstraintClauses.Count.Should().Be(0);
            cs.OpenBraceToken.Should().NotBe(default);
            cs.CloseBraceToken.Should().NotBe(default);

            cs.Members.Count.Should().Be(1);

            cs.Members[0].Kind().Should().Be(SyntaxKind.FieldDeclaration);
            var fs = (FieldDeclarationSyntax)cs.Members[0];
            fs.AttributeLists.Count.Should().Be(0);
            fs.Modifiers.Count.Should().Be(0);
            fs.Declaration.Type.Should().NotBeNull();
            fs.Declaration.Type.ToString().Should().Be(typeText);
            fs.Declaration.Variables.Count.Should().Be(1);
            fs.Declaration.Variables[0].Identifier.Should().NotBe(default);
            fs.Declaration.Variables[0].Identifier.ToString().Should().Be("c");
            fs.Declaration.Variables[0].ArgumentList.Should().BeNull();
            fs.Declaration.Variables[0].Initializer.Should().BeNull();
            fs.SemicolonToken.Should().NotBe(default);
            fs.SemicolonToken.IsMissing.Should().BeFalse();
        }

        private void TestClassFieldModifier(SyntaxKind mod)
        {
            var text = "class a { " + SyntaxFacts.GetText(mod) + " b c; }";
            var file = this.ParseFile(text);

            file.Should().NotBeNull();
            file.Members.Count.Should().Be(1);
            file.ToString().Should().Be(text);
            file.Errors().Length.Should().Be(0);

            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var cs = (TypeDeclarationSyntax)file.Members[0];
            cs.AttributeLists.Count.Should().Be(0);
            cs.Modifiers.Count.Should().Be(0);
            cs.Keyword.Should().NotBe(default);
            cs.Keyword.Kind().Should().Be(SyntaxKind.ClassKeyword);
            cs.Identifier.Should().NotBe(default);
            cs.Identifier.ToString().Should().Be("a");
            cs.BaseList.Should().BeNull();
            cs.ConstraintClauses.Count.Should().Be(0);
            cs.OpenBraceToken.Should().NotBe(default);
            cs.CloseBraceToken.Should().NotBe(default);

            cs.Members.Count.Should().Be(1);

            cs.Members[0].Kind().Should().Be(SyntaxKind.FieldDeclaration);
            var fs = (FieldDeclarationSyntax)cs.Members[0];
            fs.AttributeLists.Count.Should().Be(0);
            fs.Modifiers.Count.Should().Be(1);
            fs.Modifiers[0].Kind().Should().Be(mod);
            fs.Declaration.Type.Should().NotBeNull();
            fs.Declaration.Type.ToString().Should().Be("b");
            fs.Declaration.Variables.Count.Should().Be(1);
            fs.Declaration.Variables[0].Identifier.Should().NotBe(default);
            fs.Declaration.Variables[0].Identifier.ToString().Should().Be("c");
            fs.Declaration.Variables[0].ArgumentList.Should().BeNull();
            fs.Declaration.Variables[0].Initializer.Should().BeNull();
            fs.SemicolonToken.Should().NotBe(default);
            fs.SemicolonToken.IsMissing.Should().BeFalse();
        }

        [Fact]
        public void TestClassFieldModifiers()
        {
            TestClassFieldModifier(SyntaxKind.PublicKeyword);
            TestClassFieldModifier(SyntaxKind.PrivateKeyword);
            TestClassFieldModifier(SyntaxKind.ProtectedKeyword);
            TestClassFieldModifier(SyntaxKind.InternalKeyword);
            TestClassFieldModifier(SyntaxKind.StaticKeyword);
            TestClassFieldModifier(SyntaxKind.ReadOnlyKeyword);
            TestClassFieldModifier(SyntaxKind.VolatileKeyword);
            TestClassFieldModifier(SyntaxKind.ExternKeyword);
        }

        private void TestClassEventFieldModifier(SyntaxKind mod)
        {
            var text = "class a { " + SyntaxFacts.GetText(mod) + " event b c; }";
            var file = this.ParseFile(text);

            file.Should().NotBeNull();
            file.Members.Count.Should().Be(1);
            file.ToString().Should().Be(text);
            file.Errors().Length.Should().Be(0);

            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var cs = (TypeDeclarationSyntax)file.Members[0];
            cs.AttributeLists.Count.Should().Be(0);
            cs.Modifiers.Count.Should().Be(0);
            cs.Keyword.Should().NotBe(default);
            cs.Keyword.Kind().Should().Be(SyntaxKind.ClassKeyword);
            cs.Identifier.Should().NotBe(default);
            cs.Identifier.ToString().Should().Be("a");
            cs.BaseList.Should().BeNull();
            cs.ConstraintClauses.Count.Should().Be(0);
            cs.OpenBraceToken.Should().NotBe(default);
            cs.CloseBraceToken.Should().NotBe(default);

            cs.Members.Count.Should().Be(1);

            cs.Members[0].Kind().Should().Be(SyntaxKind.EventFieldDeclaration);
            var fs = (EventFieldDeclarationSyntax)cs.Members[0];
            fs.AttributeLists.Count.Should().Be(0);
            fs.Modifiers.Count.Should().Be(1);
            fs.Modifiers[0].Kind().Should().Be(mod);
            fs.EventKeyword.Should().NotBe(default);
            fs.Declaration.Type.Should().NotBeNull();
            fs.Declaration.Type.ToString().Should().Be("b");
            fs.Declaration.Variables.Count.Should().Be(1);
            fs.Declaration.Variables[0].Identifier.Should().NotBe(default);
            fs.Declaration.Variables[0].Identifier.ToString().Should().Be("c");
            fs.Declaration.Variables[0].ArgumentList.Should().BeNull();
            fs.Declaration.Variables[0].Initializer.Should().BeNull();
            fs.SemicolonToken.Should().NotBe(default);
            fs.SemicolonToken.IsMissing.Should().BeFalse();
        }

        [Fact]
        public void TestClassEventFieldModifiers()
        {
            TestClassEventFieldModifier(SyntaxKind.PublicKeyword);
            TestClassEventFieldModifier(SyntaxKind.PrivateKeyword);
            TestClassEventFieldModifier(SyntaxKind.ProtectedKeyword);
            TestClassEventFieldModifier(SyntaxKind.InternalKeyword);
            TestClassEventFieldModifier(SyntaxKind.StaticKeyword);
            TestClassEventFieldModifier(SyntaxKind.ReadOnlyKeyword);
            TestClassEventFieldModifier(SyntaxKind.VolatileKeyword);
            TestClassEventFieldModifier(SyntaxKind.ExternKeyword);
        }

        [Fact]
        public void TestClassConstField()
        {
            var text = "class a { const b c = d; }";
            var file = this.ParseFile(text);

            file.Should().NotBeNull();
            file.Members.Count.Should().Be(1);
            file.ToString().Should().Be(text);
            file.Errors().Length.Should().Be(0);

            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var cs = (TypeDeclarationSyntax)file.Members[0];
            cs.AttributeLists.Count.Should().Be(0);
            cs.Modifiers.Count.Should().Be(0);
            cs.Keyword.Should().NotBe(default);
            cs.Keyword.Kind().Should().Be(SyntaxKind.ClassKeyword);
            cs.Identifier.Should().NotBe(default);
            cs.Identifier.ToString().Should().Be("a");
            cs.BaseList.Should().BeNull();
            cs.ConstraintClauses.Count.Should().Be(0);
            cs.OpenBraceToken.Should().NotBe(default);
            cs.CloseBraceToken.Should().NotBe(default);

            cs.Members.Count.Should().Be(1);

            cs.Members[0].Kind().Should().Be(SyntaxKind.FieldDeclaration);
            var fs = (FieldDeclarationSyntax)cs.Members[0];
            fs.AttributeLists.Count.Should().Be(0);
            fs.Modifiers.Count.Should().Be(1);
            fs.Modifiers[0].Kind().Should().Be(SyntaxKind.ConstKeyword);
            fs.Declaration.Type.Should().NotBeNull();
            fs.Declaration.Type.ToString().Should().Be("b");
            fs.Declaration.Variables.Count.Should().Be(1);
            fs.Declaration.Variables[0].Identifier.Should().NotBe(default);
            fs.Declaration.Variables[0].Identifier.ToString().Should().Be("c");
            fs.Declaration.Variables[0].ArgumentList.Should().BeNull();
            fs.Declaration.Variables[0].Initializer.Should().NotBeNull();
            fs.Declaration.Variables[0].Initializer.EqualsToken.Should().NotBe(default);
            fs.Declaration.Variables[0].Initializer.Value.Should().NotBeNull();
            fs.Declaration.Variables[0].Initializer.Value.ToString().Should().Be("d");
            fs.SemicolonToken.Should().NotBe(default);
            fs.SemicolonToken.IsMissing.Should().BeFalse();
        }

        [Fact]
        public void TestClassFieldWithInitializer()
        {
            var text = "class a { b c = e; }";
            var file = this.ParseFile(text);

            file.Should().NotBeNull();
            file.Members.Count.Should().Be(1);
            file.ToString().Should().Be(text);
            file.Errors().Length.Should().Be(0);

            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var cs = (TypeDeclarationSyntax)file.Members[0];
            cs.AttributeLists.Count.Should().Be(0);
            cs.Modifiers.Count.Should().Be(0);
            cs.Keyword.Should().NotBe(default);
            cs.Keyword.Kind().Should().Be(SyntaxKind.ClassKeyword);
            cs.Identifier.Should().NotBe(default);
            cs.Identifier.ToString().Should().Be("a");
            cs.BaseList.Should().BeNull();
            cs.ConstraintClauses.Count.Should().Be(0);
            cs.OpenBraceToken.Should().NotBe(default);
            cs.CloseBraceToken.Should().NotBe(default);

            cs.Members.Count.Should().Be(1);

            cs.Members[0].Kind().Should().Be(SyntaxKind.FieldDeclaration);
            var fs = (FieldDeclarationSyntax)cs.Members[0];
            fs.AttributeLists.Count.Should().Be(0);
            fs.Modifiers.Count.Should().Be(0);
            fs.Declaration.Type.Should().NotBeNull();
            fs.Declaration.Type.ToString().Should().Be("b");
            fs.Declaration.Variables.Count.Should().Be(1);
            fs.Declaration.Variables[0].Identifier.Should().NotBe(default);
            fs.Declaration.Variables[0].Identifier.ToString().Should().Be("c");
            fs.Declaration.Variables[0].ArgumentList.Should().BeNull();
            fs.Declaration.Variables[0].Initializer.Should().NotBeNull();
            fs.Declaration.Variables[0].Initializer.EqualsToken.Should().NotBe(default);
            fs.Declaration.Variables[0].Initializer.Value.Should().NotBeNull();
            fs.Declaration.Variables[0].Initializer.Value.ToString().Should().Be("e");
            fs.SemicolonToken.Should().NotBe(default);
            fs.SemicolonToken.IsMissing.Should().BeFalse();
        }

        [Fact]
        public void TestClassFieldWithArrayInitializer()
        {
            var text = "class a { b c = { }; }";
            var file = this.ParseFile(text);

            file.Should().NotBeNull();
            file.Members.Count.Should().Be(1);
            file.ToString().Should().Be(text);
            file.Errors().Length.Should().Be(0);

            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var cs = (TypeDeclarationSyntax)file.Members[0];
            cs.AttributeLists.Count.Should().Be(0);
            cs.Modifiers.Count.Should().Be(0);
            cs.Keyword.Should().NotBe(default);
            cs.Keyword.Kind().Should().Be(SyntaxKind.ClassKeyword);
            cs.Identifier.Should().NotBe(default);
            cs.Identifier.ToString().Should().Be("a");
            cs.BaseList.Should().BeNull();
            cs.ConstraintClauses.Count.Should().Be(0);
            cs.OpenBraceToken.Should().NotBe(default);
            cs.CloseBraceToken.Should().NotBe(default);

            cs.Members.Count.Should().Be(1);

            cs.Members[0].Kind().Should().Be(SyntaxKind.FieldDeclaration);
            var fs = (FieldDeclarationSyntax)cs.Members[0];
            fs.AttributeLists.Count.Should().Be(0);
            fs.Modifiers.Count.Should().Be(0);
            fs.Declaration.Type.Should().NotBeNull();
            fs.Declaration.Type.ToString().Should().Be("b");
            fs.Declaration.Variables.Count.Should().Be(1);
            fs.Declaration.Variables[0].Identifier.Should().NotBe(default);
            fs.Declaration.Variables[0].Identifier.ToString().Should().Be("c");
            fs.Declaration.Variables[0].ArgumentList.Should().BeNull();
            fs.Declaration.Variables[0].Initializer.Should().NotBeNull();
            fs.Declaration.Variables[0].Initializer.EqualsToken.Should().NotBe(default);
            fs.Declaration.Variables[0].Initializer.Value.Should().NotBeNull();
            fs.Declaration.Variables[0].Initializer.Value.Kind().Should().Be(SyntaxKind.ArrayInitializerExpression);
            fs.Declaration.Variables[0].Initializer.Value.ToString().Should().Be("{ }");
            fs.SemicolonToken.Should().NotBe(default);
            fs.SemicolonToken.IsMissing.Should().BeFalse();
        }

        [Fact]
        public void TestClassFieldWithMultipleVariables()
        {
            var text = "class a { b c, d, e; }";
            var file = this.ParseFile(text);

            file.Should().NotBeNull();
            file.Members.Count.Should().Be(1);
            file.ToString().Should().Be(text);
            file.Errors().Length.Should().Be(0);

            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var cs = (TypeDeclarationSyntax)file.Members[0];
            cs.AttributeLists.Count.Should().Be(0);
            cs.Modifiers.Count.Should().Be(0);
            cs.Keyword.Should().NotBe(default);
            cs.Keyword.Kind().Should().Be(SyntaxKind.ClassKeyword);
            cs.Identifier.Should().NotBe(default);
            cs.Identifier.ToString().Should().Be("a");
            cs.BaseList.Should().BeNull();
            cs.ConstraintClauses.Count.Should().Be(0);
            cs.OpenBraceToken.Should().NotBe(default);
            cs.CloseBraceToken.Should().NotBe(default);

            cs.Members.Count.Should().Be(1);

            cs.Members[0].Kind().Should().Be(SyntaxKind.FieldDeclaration);
            var fs = (FieldDeclarationSyntax)cs.Members[0];
            fs.AttributeLists.Count.Should().Be(0);
            fs.Modifiers.Count.Should().Be(0);
            fs.Declaration.Type.Should().NotBeNull();
            fs.Declaration.Type.ToString().Should().Be("b");

            fs.Declaration.Variables.Count.Should().Be(3);

            fs.Declaration.Variables[0].Identifier.Should().NotBe(default);
            fs.Declaration.Variables[0].Identifier.ToString().Should().Be("c");
            fs.Declaration.Variables[0].ArgumentList.Should().BeNull();
            fs.Declaration.Variables[0].Initializer.Should().BeNull();

            fs.Declaration.Variables[1].Identifier.Should().NotBe(default);
            fs.Declaration.Variables[1].Identifier.ToString().Should().Be("d");
            fs.Declaration.Variables[1].ArgumentList.Should().BeNull();
            fs.Declaration.Variables[1].Initializer.Should().BeNull();

            fs.Declaration.Variables[2].Identifier.Should().NotBe(default);
            fs.Declaration.Variables[2].Identifier.ToString().Should().Be("e");
            fs.Declaration.Variables[2].ArgumentList.Should().BeNull();
            fs.Declaration.Variables[2].Initializer.Should().BeNull();

            fs.SemicolonToken.Should().NotBe(default);
            fs.SemicolonToken.IsMissing.Should().BeFalse();
        }

        [Fact]
        public void TestClassFieldWithMultipleVariablesAndInitializers()
        {
            var text = "class a { b c = x, d = y, e = z; }";
            var file = this.ParseFile(text);

            file.Should().NotBeNull();
            file.Members.Count.Should().Be(1);
            file.ToString().Should().Be(text);
            file.Errors().Length.Should().Be(0);

            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var cs = (TypeDeclarationSyntax)file.Members[0];
            cs.AttributeLists.Count.Should().Be(0);
            cs.Modifiers.Count.Should().Be(0);
            cs.Keyword.Should().NotBe(default);
            cs.Keyword.Kind().Should().Be(SyntaxKind.ClassKeyword);
            cs.Identifier.Should().NotBe(default);
            cs.Identifier.ToString().Should().Be("a");
            cs.BaseList.Should().BeNull();
            cs.ConstraintClauses.Count.Should().Be(0);
            cs.OpenBraceToken.Should().NotBe(default);
            cs.CloseBraceToken.Should().NotBe(default);

            cs.Members.Count.Should().Be(1);

            cs.Members[0].Kind().Should().Be(SyntaxKind.FieldDeclaration);
            var fs = (FieldDeclarationSyntax)cs.Members[0];
            fs.AttributeLists.Count.Should().Be(0);
            fs.Modifiers.Count.Should().Be(0);
            fs.Declaration.Type.Should().NotBeNull();
            fs.Declaration.Type.ToString().Should().Be("b");

            fs.Declaration.Variables.Count.Should().Be(3);

            fs.Declaration.Variables[0].Identifier.Should().NotBe(default);
            fs.Declaration.Variables[0].Identifier.ToString().Should().Be("c");
            fs.Declaration.Variables[0].ArgumentList.Should().BeNull();
            fs.Declaration.Variables[0].Initializer.Should().NotBeNull();
            fs.Declaration.Variables[0].Initializer.EqualsToken.Should().NotBe(default);
            fs.Declaration.Variables[0].Initializer.Value.Should().NotBeNull();
            fs.Declaration.Variables[0].Initializer.Value.ToString().Should().Be("x");

            fs.Declaration.Variables[1].Identifier.Should().NotBe(default);
            fs.Declaration.Variables[1].Identifier.ToString().Should().Be("d");
            fs.Declaration.Variables[1].ArgumentList.Should().BeNull();
            fs.Declaration.Variables[1].Initializer.Should().NotBeNull();
            fs.Declaration.Variables[1].Initializer.EqualsToken.Should().NotBe(default);
            fs.Declaration.Variables[1].Initializer.Value.Should().NotBeNull();
            fs.Declaration.Variables[1].Initializer.Value.ToString().Should().Be("y");

            fs.Declaration.Variables[2].Identifier.Should().NotBe(default);
            fs.Declaration.Variables[2].Identifier.ToString().Should().Be("e");
            fs.Declaration.Variables[2].ArgumentList.Should().BeNull();
            fs.Declaration.Variables[2].Initializer.Should().NotBeNull();
            fs.Declaration.Variables[2].Initializer.EqualsToken.Should().NotBe(default);
            fs.Declaration.Variables[2].Initializer.Value.Should().NotBeNull();
            fs.Declaration.Variables[2].Initializer.Value.ToString().Should().Be("z");

            fs.SemicolonToken.Should().NotBe(default);
            fs.SemicolonToken.IsMissing.Should().BeFalse();
        }

        [Fact]
        public void TestClassFixedField()
        {
            var text = "class a { fixed b c[10]; }";
            var file = this.ParseFile(text);

            file.Should().NotBeNull();
            file.Members.Count.Should().Be(1);
            file.ToString().Should().Be(text);
            file.Errors().Length.Should().Be(0);

            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var cs = (TypeDeclarationSyntax)file.Members[0];
            cs.AttributeLists.Count.Should().Be(0);
            cs.Modifiers.Count.Should().Be(0);
            cs.Keyword.Should().NotBe(default);
            cs.Keyword.Kind().Should().Be(SyntaxKind.ClassKeyword);
            cs.Identifier.Should().NotBe(default);
            cs.Identifier.ToString().Should().Be("a");
            cs.BaseList.Should().BeNull();
            cs.ConstraintClauses.Count.Should().Be(0);
            cs.OpenBraceToken.Should().NotBe(default);
            cs.CloseBraceToken.Should().NotBe(default);

            cs.Members.Count.Should().Be(1);

            cs.Members[0].Kind().Should().Be(SyntaxKind.FieldDeclaration);
            var fs = (FieldDeclarationSyntax)cs.Members[0];
            fs.AttributeLists.Count.Should().Be(0);
            fs.Modifiers.Count.Should().Be(1);
            fs.Modifiers[0].Kind().Should().Be(SyntaxKind.FixedKeyword);
            fs.Declaration.Type.Should().NotBeNull();
            fs.Declaration.Type.ToString().Should().Be("b");
            fs.Declaration.Variables.Count.Should().Be(1);
            fs.Declaration.Variables[0].Identifier.Should().NotBe(default);
            fs.Declaration.Variables[0].Identifier.ToString().Should().Be("c");
            fs.Declaration.Variables[0].ArgumentList.Should().NotBeNull();
            fs.Declaration.Variables[0].ArgumentList.OpenBracketToken.Should().NotBe(default);
            fs.Declaration.Variables[0].ArgumentList.CloseBracketToken.Should().NotBe(default);
            fs.Declaration.Variables[0].ArgumentList.Arguments.Count.Should().Be(1);
            fs.Declaration.Variables[0].ArgumentList.Arguments[0].ToString().Should().Be("10");
            fs.Declaration.Variables[0].Initializer.Should().BeNull();
            fs.SemicolonToken.Should().NotBe(default);
            fs.SemicolonToken.IsMissing.Should().BeFalse();
        }

        [Fact]
        public void TestClassProperty()
        {
            var text = "class a { b c { get; set; } }";
            var file = this.ParseFile(text);

            file.Should().NotBeNull();
            file.Members.Count.Should().Be(1);
            file.ToString().Should().Be(text);
            file.Errors().Length.Should().Be(0);

            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var cs = (TypeDeclarationSyntax)file.Members[0];
            cs.AttributeLists.Count.Should().Be(0);
            cs.Modifiers.Count.Should().Be(0);
            cs.Keyword.Should().NotBe(default);
            cs.Keyword.Kind().Should().Be(SyntaxKind.ClassKeyword);
            cs.Identifier.Should().NotBe(default);
            cs.Identifier.ToString().Should().Be("a");
            cs.BaseList.Should().BeNull();
            cs.ConstraintClauses.Count.Should().Be(0);
            cs.OpenBraceToken.Should().NotBe(default);
            cs.CloseBraceToken.Should().NotBe(default);

            cs.Members.Count.Should().Be(1);

            cs.Members[0].Kind().Should().Be(SyntaxKind.PropertyDeclaration);
            var ps = (PropertyDeclarationSyntax)cs.Members[0];
            ps.AttributeLists.Count.Should().Be(0);
            ps.Modifiers.Count.Should().Be(0);
            ps.Type.Should().NotBeNull();
            ps.Type.ToString().Should().Be("b");
            ps.Identifier.Should().NotBe(default);
            ps.Identifier.ToString().Should().Be("c");

            ps.AccessorList.OpenBraceToken.Should().NotBe(default);
            ps.AccessorList.CloseBraceToken.Should().NotBe(default);
            ps.AccessorList.Accessors.Count.Should().Be(2);

            ps.AccessorList.Accessors[0].AttributeLists.Count.Should().Be(0);
            ps.AccessorList.Accessors[0].Modifiers.Count.Should().Be(0);
            ps.AccessorList.Accessors[0].Keyword.Should().NotBe(default);
            ps.AccessorList.Accessors[0].Keyword.Kind().Should().Be(SyntaxKind.GetKeyword);
            ps.AccessorList.Accessors[0].Body.Should().BeNull();
            ps.AccessorList.Accessors[0].SemicolonToken.Should().NotBe(default);

            ps.AccessorList.Accessors[1].AttributeLists.Count.Should().Be(0);
            ps.AccessorList.Accessors[1].Modifiers.Count.Should().Be(0);
            ps.AccessorList.Accessors[1].Keyword.Should().NotBe(default);
            ps.AccessorList.Accessors[1].Keyword.Kind().Should().Be(SyntaxKind.SetKeyword);
            ps.AccessorList.Accessors[1].Body.Should().BeNull();
            ps.AccessorList.Accessors[1].SemicolonToken.Should().NotBe(default);
        }

        [Fact]
        public void TestClassPropertyWithRefReturn()
        {
            var text = "class a { ref b c { get; set; } }";
            var file = this.ParseFile(text);

            file.Should().NotBeNull();
            file.Members.Count.Should().Be(1);
            file.ToString().Should().Be(text);
            file.Errors().Length.Should().Be(0);

            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var cs = (TypeDeclarationSyntax)file.Members[0];
            cs.AttributeLists.Count.Should().Be(0);
            cs.Modifiers.Count.Should().Be(0);
            cs.Keyword.Should().NotBe(default);
            cs.Keyword.Kind().Should().Be(SyntaxKind.ClassKeyword);
            cs.Identifier.Should().NotBe(default);
            cs.Identifier.ToString().Should().Be("a");
            cs.BaseList.Should().BeNull();
            cs.ConstraintClauses.Count.Should().Be(0);
            cs.OpenBraceToken.Should().NotBe(default);
            cs.CloseBraceToken.Should().NotBe(default);

            cs.Members.Count.Should().Be(1);

            cs.Members[0].Kind().Should().Be(SyntaxKind.PropertyDeclaration);
            var ps = (PropertyDeclarationSyntax)cs.Members[0];
            ps.AttributeLists.Count.Should().Be(0);
            ps.Modifiers.Count.Should().Be(0);
            ps.Type.Should().NotBeNull();
            ps.Type.ToString().Should().Be("ref b");
            ps.Identifier.Should().NotBe(default);
            ps.Identifier.ToString().Should().Be("c");

            ps.AccessorList.OpenBraceToken.Should().NotBe(default);
            ps.AccessorList.CloseBraceToken.Should().NotBe(default);
            ps.AccessorList.Accessors.Count.Should().Be(2);

            ps.AccessorList.Accessors[0].AttributeLists.Count.Should().Be(0);
            ps.AccessorList.Accessors[0].Modifiers.Count.Should().Be(0);
            ps.AccessorList.Accessors[0].Keyword.Should().NotBe(default);
            ps.AccessorList.Accessors[0].Keyword.Kind().Should().Be(SyntaxKind.GetKeyword);
            ps.AccessorList.Accessors[0].Body.Should().BeNull();
            ps.AccessorList.Accessors[0].SemicolonToken.Should().NotBe(default);

            ps.AccessorList.Accessors[1].AttributeLists.Count.Should().Be(0);
            ps.AccessorList.Accessors[1].Modifiers.Count.Should().Be(0);
            ps.AccessorList.Accessors[1].Keyword.Should().NotBe(default);
            ps.AccessorList.Accessors[1].Keyword.Kind().Should().Be(SyntaxKind.SetKeyword);
            ps.AccessorList.Accessors[1].Body.Should().BeNull();
            ps.AccessorList.Accessors[1].SemicolonToken.Should().NotBe(default);
        }

        [CompilerTrait(CompilerFeature.ReadOnlyReferences)]
        [Fact]
        public void TestClassPropertyWithRefReadonlyReturn()
        {
            var text = "class a { ref readonly b c { get; set; } }";
            var file = this.ParseFile(text, TestOptions.Regular);

            file.Should().NotBeNull();
            file.Members.Count.Should().Be(1);
            file.ToString().Should().Be(text);
            file.Errors().Length.Should().Be(0);

            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var cs = (TypeDeclarationSyntax)file.Members[0];
            cs.AttributeLists.Count.Should().Be(0);
            cs.Modifiers.Count.Should().Be(0);
            cs.Keyword.Should().NotBe(default);
            cs.Keyword.Kind().Should().Be(SyntaxKind.ClassKeyword);
            cs.Identifier.Should().NotBe(default);
            cs.Identifier.ToString().Should().Be("a");
            cs.BaseList.Should().BeNull();
            cs.ConstraintClauses.Count.Should().Be(0);
            cs.OpenBraceToken.Should().NotBe(default);
            cs.CloseBraceToken.Should().NotBe(default);

            cs.Members.Count.Should().Be(1);

            cs.Members[0].Kind().Should().Be(SyntaxKind.PropertyDeclaration);
            var ps = (PropertyDeclarationSyntax)cs.Members[0];
            ps.AttributeLists.Count.Should().Be(0);
            ps.Modifiers.Count.Should().Be(0);
            ps.Type.Should().NotBeNull();
            ps.Type.ToString().Should().Be("ref readonly b");
            ps.Identifier.Should().NotBe(default);
            ps.Identifier.ToString().Should().Be("c");

            ps.AccessorList.OpenBraceToken.Should().NotBe(default);
            ps.AccessorList.CloseBraceToken.Should().NotBe(default);
            ps.AccessorList.Accessors.Count.Should().Be(2);

            ps.AccessorList.Accessors[0].AttributeLists.Count.Should().Be(0);
            ps.AccessorList.Accessors[0].Modifiers.Count.Should().Be(0);
            ps.AccessorList.Accessors[0].Keyword.Should().NotBe(default);
            ps.AccessorList.Accessors[0].Keyword.Kind().Should().Be(SyntaxKind.GetKeyword);
            ps.AccessorList.Accessors[0].Body.Should().BeNull();
            ps.AccessorList.Accessors[0].SemicolonToken.Should().NotBe(default);

            ps.AccessorList.Accessors[1].AttributeLists.Count.Should().Be(0);
            ps.AccessorList.Accessors[1].Modifiers.Count.Should().Be(0);
            ps.AccessorList.Accessors[1].Keyword.Should().NotBe(default);
            ps.AccessorList.Accessors[1].Keyword.Kind().Should().Be(SyntaxKind.SetKeyword);
            ps.AccessorList.Accessors[1].Body.Should().BeNull();
            ps.AccessorList.Accessors[1].SemicolonToken.Should().NotBe(default);
        }

        [Fact]
        public void TestClassPropertyWithBuiltInTypes()
        {
            TestClassPropertyWithBuiltInType(SyntaxKind.BoolKeyword);
            TestClassPropertyWithBuiltInType(SyntaxKind.SByteKeyword);
            TestClassPropertyWithBuiltInType(SyntaxKind.IntKeyword);
            TestClassPropertyWithBuiltInType(SyntaxKind.UIntKeyword);
            TestClassPropertyWithBuiltInType(SyntaxKind.ShortKeyword);
            TestClassPropertyWithBuiltInType(SyntaxKind.UShortKeyword);
            TestClassPropertyWithBuiltInType(SyntaxKind.LongKeyword);
            TestClassPropertyWithBuiltInType(SyntaxKind.ULongKeyword);
            TestClassPropertyWithBuiltInType(SyntaxKind.FloatKeyword);
            TestClassPropertyWithBuiltInType(SyntaxKind.DoubleKeyword);
            TestClassPropertyWithBuiltInType(SyntaxKind.DecimalKeyword);
            TestClassPropertyWithBuiltInType(SyntaxKind.StringKeyword);
            TestClassPropertyWithBuiltInType(SyntaxKind.CharKeyword);
            TestClassPropertyWithBuiltInType(SyntaxKind.ObjectKeyword);
        }

        private void TestClassPropertyWithBuiltInType(SyntaxKind type)
        {
            var typeText = SyntaxFacts.GetText(type);
            var text = "class a { " + typeText + " c { get; set; } }";
            var file = this.ParseFile(text);

            file.Should().NotBeNull();
            file.Members.Count.Should().Be(1);
            file.ToString().Should().Be(text);
            file.Errors().Length.Should().Be(0);

            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var cs = (TypeDeclarationSyntax)file.Members[0];
            cs.AttributeLists.Count.Should().Be(0);
            cs.Modifiers.Count.Should().Be(0);
            cs.Keyword.Should().NotBe(default);
            cs.Keyword.Kind().Should().Be(SyntaxKind.ClassKeyword);
            cs.Identifier.Should().NotBe(default);
            cs.Identifier.ToString().Should().Be("a");
            cs.BaseList.Should().BeNull();
            cs.ConstraintClauses.Count.Should().Be(0);
            cs.OpenBraceToken.Should().NotBe(default);
            cs.CloseBraceToken.Should().NotBe(default);

            cs.Members.Count.Should().Be(1);

            cs.Members[0].Kind().Should().Be(SyntaxKind.PropertyDeclaration);
            var ps = (PropertyDeclarationSyntax)cs.Members[0];
            ps.AttributeLists.Count.Should().Be(0);
            ps.Modifiers.Count.Should().Be(0);
            ps.Type.Should().NotBeNull();
            ps.Type.ToString().Should().Be(typeText);
            ps.Identifier.Should().NotBe(default);
            ps.Identifier.ToString().Should().Be("c");

            ps.AccessorList.OpenBraceToken.Should().NotBe(default);
            ps.AccessorList.CloseBraceToken.Should().NotBe(default);
            ps.AccessorList.Accessors.Count.Should().Be(2);

            ps.AccessorList.Accessors[0].AttributeLists.Count.Should().Be(0);
            ps.AccessorList.Accessors[0].Modifiers.Count.Should().Be(0);
            ps.AccessorList.Accessors[0].Keyword.Should().NotBe(default);
            ps.AccessorList.Accessors[0].Keyword.Kind().Should().Be(SyntaxKind.GetKeyword);
            ps.AccessorList.Accessors[0].Body.Should().BeNull();
            ps.AccessorList.Accessors[0].SemicolonToken.Should().NotBe(default);

            ps.AccessorList.Accessors[1].AttributeLists.Count.Should().Be(0);
            ps.AccessorList.Accessors[1].Modifiers.Count.Should().Be(0);
            ps.AccessorList.Accessors[1].Keyword.Should().NotBe(default);
            ps.AccessorList.Accessors[1].Keyword.Kind().Should().Be(SyntaxKind.SetKeyword);
            ps.AccessorList.Accessors[1].Body.Should().BeNull();
            ps.AccessorList.Accessors[1].SemicolonToken.Should().NotBe(default);
        }

        [Fact]
        public void TestClassPropertyWithBodies()
        {
            var text = "class a { b c { get { } set { } } }";
            var file = this.ParseFile(text);

            file.Should().NotBeNull();
            file.Members.Count.Should().Be(1);
            file.ToString().Should().Be(text);
            file.Errors().Length.Should().Be(0);

            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var cs = (TypeDeclarationSyntax)file.Members[0];
            cs.AttributeLists.Count.Should().Be(0);
            cs.Modifiers.Count.Should().Be(0);
            cs.Keyword.Should().NotBe(default);
            cs.Keyword.Kind().Should().Be(SyntaxKind.ClassKeyword);
            cs.Identifier.Should().NotBe(default);
            cs.Identifier.ToString().Should().Be("a");
            cs.BaseList.Should().BeNull();
            cs.ConstraintClauses.Count.Should().Be(0);
            cs.OpenBraceToken.Should().NotBe(default);
            cs.CloseBraceToken.Should().NotBe(default);

            cs.Members.Count.Should().Be(1);

            cs.Members[0].Kind().Should().Be(SyntaxKind.PropertyDeclaration);
            var ps = (PropertyDeclarationSyntax)cs.Members[0];
            ps.AttributeLists.Count.Should().Be(0);
            ps.Modifiers.Count.Should().Be(0);
            ps.Type.Should().NotBeNull();
            ps.Type.ToString().Should().Be("b");
            ps.Identifier.Should().NotBe(default);
            ps.Identifier.ToString().Should().Be("c");

            ps.AccessorList.OpenBraceToken.Should().NotBe(default);
            ps.AccessorList.CloseBraceToken.Should().NotBe(default);
            ps.AccessorList.Accessors.Count.Should().Be(2);

            ps.AccessorList.Accessors[0].AttributeLists.Count.Should().Be(0);
            ps.AccessorList.Accessors[0].Modifiers.Count.Should().Be(0);
            ps.AccessorList.Accessors[0].Keyword.Should().NotBe(default);
            ps.AccessorList.Accessors[0].Keyword.Kind().Should().Be(SyntaxKind.GetKeyword);
            ps.AccessorList.Accessors[0].Body.Should().NotBeNull();
            ps.AccessorList.Accessors[0].SemicolonToken.Kind().Should().Be(SyntaxKind.None);

            ps.AccessorList.Accessors[1].AttributeLists.Count.Should().Be(0);
            ps.AccessorList.Accessors[1].Modifiers.Count.Should().Be(0);
            ps.AccessorList.Accessors[1].Keyword.Should().NotBe(default);
            ps.AccessorList.Accessors[1].Keyword.Kind().Should().Be(SyntaxKind.SetKeyword);
            ps.AccessorList.Accessors[1].Body.Should().NotBeNull();
            ps.AccessorList.Accessors[1].SemicolonToken.Kind().Should().Be(SyntaxKind.None);
        }

        [Fact]
        public void TestClassAutoPropertyWithInitializer()
        {
            var text = "class a { b c { get; set; } = d; }";
            var file = this.ParseFile(text);

            file.Should().NotBeNull();
            file.Members.Count.Should().Be(1);
            file.ToString().Should().Be(text);
            file.Errors().Length.Should().Be(0);

            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var cs = (ClassDeclarationSyntax)file.Members[0];
            cs.AttributeLists.Count.Should().Be(0);
            cs.Modifiers.Count.Should().Be(0);
            cs.Keyword.Should().NotBe(default);
            cs.Keyword.Kind().Should().Be(SyntaxKind.ClassKeyword);
            cs.Identifier.Should().NotBe(default);
            cs.Identifier.ToString().Should().Be("a");
            cs.BaseList.Should().BeNull();
            cs.ConstraintClauses.Count.Should().Be(0);
            cs.OpenBraceToken.Should().NotBe(default);
            cs.CloseBraceToken.Should().NotBe(default);

            cs.Members.Count.Should().Be(1);

            cs.Members[0].Kind().Should().Be(SyntaxKind.PropertyDeclaration);
            var ps = (PropertyDeclarationSyntax)cs.Members[0];
            ps.AttributeLists.Count.Should().Be(0);
            ps.Modifiers.Count.Should().Be(0);
            ps.Type.Should().NotBeNull();
            ps.Type.ToString().Should().Be("b");
            ps.Identifier.Should().NotBe(default);
            ps.Identifier.ToString().Should().Be("c");

            ps.AccessorList.OpenBraceToken.Should().NotBe(default);
            ps.AccessorList.CloseBraceToken.Should().NotBe(default);
            ps.AccessorList.Accessors.Count.Should().Be(2);

            ps.AccessorList.Accessors[0].AttributeLists.Count.Should().Be(0);
            ps.AccessorList.Accessors[0].Modifiers.Count.Should().Be(0);
            ps.AccessorList.Accessors[0].Keyword.Should().NotBe(default);
            ps.AccessorList.Accessors[0].Keyword.Kind().Should().Be(SyntaxKind.GetKeyword);
            ps.AccessorList.Accessors[0].Body.Should().BeNull();

            ps.AccessorList.Accessors[1].AttributeLists.Count.Should().Be(0);
            ps.AccessorList.Accessors[1].Modifiers.Count.Should().Be(0);
            ps.AccessorList.Accessors[1].Keyword.Should().NotBe(default);
            ps.AccessorList.Accessors[1].Keyword.Kind().Should().Be(SyntaxKind.SetKeyword);
            ps.AccessorList.Accessors[1].Body.Should().BeNull();

            ps.Initializer.Should().NotBeNull();
            ps.Initializer.Value.Should().NotBeNull();
            ps.Initializer.Value.ToString().Should().Be("d");
        }

        [Fact]
        public void InitializerOnNonAutoProp()
        {
            var text = "class C { int P { set {} } = 0; }";
            var file = this.ParseFile(text);

            file.Should().NotBeNull();
            file.Errors().Length.Should().Be(0);

            file.Members.Count.Should().Be(1);
            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var cs = (ClassDeclarationSyntax)file.Members[0];

            cs.Members.Count.Should().Be(1);
            cs.Members[0].Kind().Should().Be(SyntaxKind.PropertyDeclaration);
            var ps = (PropertyDeclarationSyntax)cs.Members[0];
            ps.Errors().Length.Should().Be(0);
        }

        [Fact]
        public void TestClassPropertyOrEventWithValue()
        {
            TestClassPropertyWithValue(SyntaxKind.GetAccessorDeclaration, SyntaxKind.GetKeyword, SyntaxKind.IdentifierToken);
            TestClassPropertyWithValue(SyntaxKind.SetAccessorDeclaration, SyntaxKind.SetKeyword, SyntaxKind.IdentifierToken);
            TestClassEventWithValue(SyntaxKind.AddAccessorDeclaration, SyntaxKind.AddKeyword, SyntaxKind.IdentifierToken);
            TestClassEventWithValue(SyntaxKind.RemoveAccessorDeclaration, SyntaxKind.RemoveKeyword, SyntaxKind.IdentifierToken);
        }

        private void TestClassPropertyWithValue(SyntaxKind accessorKind, SyntaxKind accessorKeyword, SyntaxKind tokenKind)
        {
            bool isEvent = accessorKeyword == SyntaxKind.AddKeyword || accessorKeyword == SyntaxKind.RemoveKeyword;
            var text = "class a { " + (isEvent ? "event" : string.Empty) + " b c { " + SyntaxFacts.GetText(accessorKeyword) + " { x = value; } } }";
            var file = this.ParseFile(text);

            file.Should().NotBeNull();
            file.Members.Count.Should().Be(1);
            file.ToString().Should().Be(text);
            file.Errors().Length.Should().Be(0);

            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var cs = (TypeDeclarationSyntax)file.Members[0];
            cs.AttributeLists.Count.Should().Be(0);
            cs.Modifiers.Count.Should().Be(0);
            cs.Keyword.Should().NotBe(default);
            cs.Keyword.Kind().Should().Be(SyntaxKind.ClassKeyword);
            cs.Identifier.Should().NotBe(default);
            cs.Identifier.ToString().Should().Be("a");
            cs.BaseList.Should().BeNull();
            cs.ConstraintClauses.Count.Should().Be(0);
            cs.OpenBraceToken.Should().NotBe(default);
            cs.CloseBraceToken.Should().NotBe(default);

            cs.Members.Count.Should().Be(1);

            cs.Members[0].Kind().Should().Be(isEvent ? SyntaxKind.EventDeclaration : SyntaxKind.PropertyDeclaration);
            var ps = (PropertyDeclarationSyntax)cs.Members[0];
            ps.AttributeLists.Count.Should().Be(0);
            ps.Modifiers.Count.Should().Be(0);
            ps.Type.Should().NotBeNull();
            ps.Type.ToString().Should().Be("b");
            ps.Identifier.Should().NotBe(default);
            ps.Identifier.ToString().Should().Be("c");

            ps.AccessorList.OpenBraceToken.Should().NotBe(default);
            ps.AccessorList.CloseBraceToken.Should().NotBe(default);
            ps.AccessorList.Accessors.Count.Should().Be(1);

            ps.AccessorList.Accessors[0].AttributeLists.Count.Should().Be(0);
            ps.AccessorList.Accessors[0].Modifiers.Count.Should().Be(0);
            ps.AccessorList.Accessors[0].Kind().Should().Be(accessorKind);
            ps.AccessorList.Accessors[0].Keyword.Should().NotBe(default);
            ps.AccessorList.Accessors[0].Keyword.Kind().Should().Be(accessorKeyword);
            ps.AccessorList.Accessors[0].SemicolonToken.Kind().Should().Be(SyntaxKind.None);
            var body = ps.AccessorList.Accessors[0].Body;
            body.Should().NotBeNull();
            body.Statements.Count.Should().Be(1);
            body.Statements[0].Kind().Should().Be(SyntaxKind.ExpressionStatement);
            var es = (ExpressionStatementSyntax)body.Statements[0];
            es.Expression.Should().NotBeNull();
            es.Expression.Kind().Should().Be(SyntaxKind.SimpleAssignmentExpression);
            var bx = (AssignmentExpressionSyntax)es.Expression;
            bx.Right.Kind().Should().Be(SyntaxKind.IdentifierName);
            ((IdentifierNameSyntax)bx.Right).Identifier.Kind().Should().Be(tokenKind);
        }

        private void TestClassEventWithValue(SyntaxKind accessorKind, SyntaxKind accessorKeyword, SyntaxKind tokenKind)
        {
            var text = "class a { event b c { " + SyntaxFacts.GetText(accessorKeyword) + " { x = value; } } }";
            var file = this.ParseFile(text);

            file.Should().NotBeNull();
            file.Members.Count.Should().Be(1);
            file.ToString().Should().Be(text);
            file.Errors().Length.Should().Be(0);

            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var cs = (TypeDeclarationSyntax)file.Members[0];
            cs.AttributeLists.Count.Should().Be(0);
            cs.Modifiers.Count.Should().Be(0);
            cs.Keyword.Should().NotBe(default);
            cs.Keyword.Kind().Should().Be(SyntaxKind.ClassKeyword);
            cs.Identifier.Should().NotBe(default);
            cs.Identifier.ToString().Should().Be("a");
            cs.BaseList.Should().BeNull();
            cs.ConstraintClauses.Count.Should().Be(0);
            cs.OpenBraceToken.Should().NotBe(default);
            cs.CloseBraceToken.Should().NotBe(default);

            cs.Members.Count.Should().Be(1);

            cs.Members[0].Kind().Should().Be(SyntaxKind.EventDeclaration);
            var es = (EventDeclarationSyntax)cs.Members[0];
            es.AttributeLists.Count.Should().Be(0);
            es.Modifiers.Count.Should().Be(0);
            es.Type.Should().NotBeNull();
            es.Type.ToString().Should().Be("b");
            es.Identifier.Should().NotBe(default);
            es.Identifier.ToString().Should().Be("c");

            es.AccessorList.OpenBraceToken.Should().NotBe(default);
            es.AccessorList.CloseBraceToken.Should().NotBe(default);
            es.AccessorList.Accessors.Count.Should().Be(1);

            es.AccessorList.Accessors[0].AttributeLists.Count.Should().Be(0);
            es.AccessorList.Accessors[0].Modifiers.Count.Should().Be(0);
            es.AccessorList.Accessors[0].Kind().Should().Be(accessorKind);
            es.AccessorList.Accessors[0].Keyword.Should().NotBe(default);
            es.AccessorList.Accessors[0].Keyword.Kind().Should().Be(accessorKeyword);
            es.AccessorList.Accessors[0].SemicolonToken.Kind().Should().Be(SyntaxKind.None);
            var body = es.AccessorList.Accessors[0].Body;
            body.Should().NotBeNull();
            body.Statements.Count.Should().Be(1);
            body.Statements[0].Kind().Should().Be(SyntaxKind.ExpressionStatement);
            var xs = (ExpressionStatementSyntax)body.Statements[0];
            xs.Expression.Should().NotBeNull();
            xs.Expression.Kind().Should().Be(SyntaxKind.SimpleAssignmentExpression);
            var bx = (AssignmentExpressionSyntax)xs.Expression;
            bx.Right.Kind().Should().Be(SyntaxKind.IdentifierName);
            ((IdentifierNameSyntax)bx.Right).Identifier.Kind().Should().Be(tokenKind);
        }

        private void TestClassPropertyWithModifier(SyntaxKind mod)
        {
            var text = "class a { " + SyntaxFacts.GetText(mod) + " b c { get; set; } }";
            var file = this.ParseFile(text);

            file.Should().NotBeNull();
            file.Members.Count.Should().Be(1);
            file.ToString().Should().Be(text);
            file.Errors().Length.Should().Be(0);

            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var cs = (TypeDeclarationSyntax)file.Members[0];
            cs.AttributeLists.Count.Should().Be(0);
            cs.Modifiers.Count.Should().Be(0);
            cs.Keyword.Should().NotBe(default);
            cs.Keyword.Kind().Should().Be(SyntaxKind.ClassKeyword);
            cs.Identifier.Should().NotBe(default);
            cs.Identifier.ToString().Should().Be("a");
            cs.BaseList.Should().BeNull();
            cs.ConstraintClauses.Count.Should().Be(0);
            cs.OpenBraceToken.Should().NotBe(default);
            cs.CloseBraceToken.Should().NotBe(default);

            cs.Members.Count.Should().Be(1);

            cs.Members[0].Kind().Should().Be(SyntaxKind.PropertyDeclaration);
            var ps = (PropertyDeclarationSyntax)cs.Members[0];
            ps.AttributeLists.Count.Should().Be(0);
            ps.Modifiers.Count.Should().Be(1);
            ps.Modifiers[0].Kind().Should().Be(mod);
            ps.Type.Should().NotBeNull();
            ps.Type.ToString().Should().Be("b");
            ps.Identifier.Should().NotBe(default);
            ps.Identifier.ToString().Should().Be("c");

            ps.AccessorList.OpenBraceToken.Should().NotBe(default);
            ps.AccessorList.CloseBraceToken.Should().NotBe(default);
            ps.AccessorList.Accessors.Count.Should().Be(2);

            ps.AccessorList.Accessors[0].AttributeLists.Count.Should().Be(0);
            ps.AccessorList.Accessors[0].Modifiers.Count.Should().Be(0);
            ps.AccessorList.Accessors[0].Keyword.Should().NotBe(default);
            ps.AccessorList.Accessors[0].Keyword.Kind().Should().Be(SyntaxKind.GetKeyword);
            ps.AccessorList.Accessors[0].Body.Should().BeNull();
            ps.AccessorList.Accessors[0].SemicolonToken.Should().NotBe(default);

            ps.AccessorList.Accessors[1].AttributeLists.Count.Should().Be(0);
            ps.AccessorList.Accessors[1].Modifiers.Count.Should().Be(0);
            ps.AccessorList.Accessors[1].Keyword.Should().NotBe(default);
            ps.AccessorList.Accessors[1].Keyword.Kind().Should().Be(SyntaxKind.SetKeyword);
            ps.AccessorList.Accessors[1].Body.Should().BeNull();
            ps.AccessorList.Accessors[1].SemicolonToken.Should().NotBe(default);
        }

        [Fact]
        public void TestClassPropertyWithModifiers()
        {
            TestClassPropertyWithModifier(SyntaxKind.PublicKeyword);
            TestClassPropertyWithModifier(SyntaxKind.PrivateKeyword);
            TestClassPropertyWithModifier(SyntaxKind.ProtectedKeyword);
            TestClassPropertyWithModifier(SyntaxKind.InternalKeyword);
            TestClassPropertyWithModifier(SyntaxKind.StaticKeyword);
            TestClassPropertyWithModifier(SyntaxKind.AbstractKeyword);
            TestClassPropertyWithModifier(SyntaxKind.VirtualKeyword);
            TestClassPropertyWithModifier(SyntaxKind.OverrideKeyword);
            TestClassPropertyWithModifier(SyntaxKind.NewKeyword);
            TestClassPropertyWithModifier(SyntaxKind.SealedKeyword);
        }

        [Fact]
        public void TestClassPropertyWithAccessorModifiers()
        {
            TestClassPropertyWithModifier(SyntaxKind.PublicKeyword);
            TestClassPropertyWithModifier(SyntaxKind.PrivateKeyword);
            TestClassPropertyWithModifier(SyntaxKind.ProtectedKeyword);
            TestClassPropertyWithModifier(SyntaxKind.InternalKeyword);
            TestClassPropertyWithModifier(SyntaxKind.AbstractKeyword);
            TestClassPropertyWithModifier(SyntaxKind.VirtualKeyword);
            TestClassPropertyWithModifier(SyntaxKind.OverrideKeyword);
            TestClassPropertyWithModifier(SyntaxKind.NewKeyword);
            TestClassPropertyWithModifier(SyntaxKind.SealedKeyword);
        }

        [Fact]
        public void TestClassPropertyExplicit()
        {
            var text = "class a { b I.c { get; set; } }";
            var file = this.ParseFile(text);

            file.Should().NotBeNull();
            file.Members.Count.Should().Be(1);
            file.ToString().Should().Be(text);
            file.Errors().Length.Should().Be(0);

            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var cs = (TypeDeclarationSyntax)file.Members[0];
            cs.AttributeLists.Count.Should().Be(0);
            cs.Modifiers.Count.Should().Be(0);
            cs.Keyword.Should().NotBe(default);
            cs.Keyword.Kind().Should().Be(SyntaxKind.ClassKeyword);
            cs.Identifier.Should().NotBe(default);
            cs.Identifier.ToString().Should().Be("a");
            cs.BaseList.Should().BeNull();
            cs.ConstraintClauses.Count.Should().Be(0);
            cs.OpenBraceToken.Should().NotBe(default);
            cs.CloseBraceToken.Should().NotBe(default);

            cs.Members.Count.Should().Be(1);

            cs.Members[0].Kind().Should().Be(SyntaxKind.PropertyDeclaration);
            var ps = (PropertyDeclarationSyntax)cs.Members[0];
            ps.AttributeLists.Count.Should().Be(0);
            ps.Modifiers.Count.Should().Be(0);
            ps.Type.Should().NotBeNull();
            ps.Type.ToString().Should().Be("b");
            ps.Identifier.Should().NotBe(default);
            ps.ExplicitInterfaceSpecifier.Should().NotBeNull();
            ps.ExplicitInterfaceSpecifier.Name.ToString().Should().Be("I");
            ps.Identifier.ToString().Should().Be("c");

            ps.AccessorList.OpenBraceToken.Should().NotBe(default);
            ps.AccessorList.CloseBraceToken.Should().NotBe(default);
            ps.AccessorList.Accessors.Count.Should().Be(2);

            ps.AccessorList.Accessors[0].AttributeLists.Count.Should().Be(0);
            ps.AccessorList.Accessors[0].Modifiers.Count.Should().Be(0);
            ps.AccessorList.Accessors[0].Keyword.Should().NotBe(default);
            ps.AccessorList.Accessors[0].Keyword.Kind().Should().Be(SyntaxKind.GetKeyword);
            ps.AccessorList.Accessors[0].Body.Should().BeNull();
            ps.AccessorList.Accessors[0].SemicolonToken.Should().NotBe(default);

            ps.AccessorList.Accessors[1].AttributeLists.Count.Should().Be(0);
            ps.AccessorList.Accessors[1].Modifiers.Count.Should().Be(0);
            ps.AccessorList.Accessors[1].Keyword.Should().NotBe(default);
            ps.AccessorList.Accessors[1].Keyword.Kind().Should().Be(SyntaxKind.SetKeyword);
            ps.AccessorList.Accessors[1].Body.Should().BeNull();
            ps.AccessorList.Accessors[1].SemicolonToken.Should().NotBe(default);
        }

        [Fact]
        public void TestClassEventProperty()
        {
            var text = "class a { event b c { add { } remove { } } }";
            var file = this.ParseFile(text);

            file.Should().NotBeNull();
            file.Members.Count.Should().Be(1);
            file.ToString().Should().Be(text);
            file.Errors().Length.Should().Be(0);

            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var cs = (TypeDeclarationSyntax)file.Members[0];
            cs.AttributeLists.Count.Should().Be(0);
            cs.Modifiers.Count.Should().Be(0);
            cs.Keyword.Should().NotBe(default);
            cs.Keyword.Kind().Should().Be(SyntaxKind.ClassKeyword);
            cs.Identifier.Should().NotBe(default);
            cs.Identifier.ToString().Should().Be("a");
            cs.BaseList.Should().BeNull();
            cs.ConstraintClauses.Count.Should().Be(0);
            cs.OpenBraceToken.Should().NotBe(default);
            cs.CloseBraceToken.Should().NotBe(default);

            cs.Members.Count.Should().Be(1);

            cs.Members[0].Kind().Should().Be(SyntaxKind.EventDeclaration);
            var es = (EventDeclarationSyntax)cs.Members[0];
            es.AttributeLists.Count.Should().Be(0);
            es.Modifiers.Count.Should().Be(0);
            es.EventKeyword.Should().NotBe(default);
            es.Type.Should().NotBeNull();
            es.Type.ToString().Should().Be("b");
            es.Identifier.Should().NotBe(default);
            es.Identifier.ToString().Should().Be("c");

            es.AccessorList.OpenBraceToken.Should().NotBe(default);
            es.AccessorList.CloseBraceToken.Should().NotBe(default);
            es.AccessorList.Accessors.Count.Should().Be(2);

            es.AccessorList.Accessors[0].AttributeLists.Count.Should().Be(0);
            es.AccessorList.Accessors[0].Modifiers.Count.Should().Be(0);
            es.AccessorList.Accessors[0].Keyword.Should().NotBe(default);
            es.AccessorList.Accessors[0].Keyword.Kind().Should().Be(SyntaxKind.AddKeyword);
            es.AccessorList.Accessors[0].Body.Should().NotBeNull();
            es.AccessorList.Accessors[0].SemicolonToken.Kind().Should().Be(SyntaxKind.None);

            es.AccessorList.Accessors[1].AttributeLists.Count.Should().Be(0);
            es.AccessorList.Accessors[1].Modifiers.Count.Should().Be(0);
            es.AccessorList.Accessors[1].Keyword.Should().NotBe(default);
            es.AccessorList.Accessors[1].Keyword.Kind().Should().Be(SyntaxKind.RemoveKeyword);
            es.AccessorList.Accessors[1].Body.Should().NotBeNull();
            es.AccessorList.Accessors[1].SemicolonToken.Kind().Should().Be(SyntaxKind.None);
        }

        private void TestClassEventPropertyWithModifier(SyntaxKind mod)
        {
            var text = "class a { " + SyntaxFacts.GetText(mod) + " event b c { add { } remove { } } }";
            var file = this.ParseFile(text);

            file.Should().NotBeNull();
            file.Members.Count.Should().Be(1);
            file.ToString().Should().Be(text);
            file.Errors().Length.Should().Be(0);

            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var cs = (TypeDeclarationSyntax)file.Members[0];
            cs.AttributeLists.Count.Should().Be(0);
            cs.Modifiers.Count.Should().Be(0);
            cs.Keyword.Should().NotBe(default);
            cs.Keyword.Kind().Should().Be(SyntaxKind.ClassKeyword);
            cs.Identifier.Should().NotBe(default);
            cs.Identifier.ToString().Should().Be("a");
            cs.BaseList.Should().BeNull();
            cs.ConstraintClauses.Count.Should().Be(0);
            cs.OpenBraceToken.Should().NotBe(default);
            cs.CloseBraceToken.Should().NotBe(default);

            cs.Members.Count.Should().Be(1);

            cs.Members[0].Kind().Should().Be(SyntaxKind.EventDeclaration);
            var es = (EventDeclarationSyntax)cs.Members[0];
            es.AttributeLists.Count.Should().Be(0);
            es.Modifiers.Count.Should().Be(1);
            es.Modifiers[0].Kind().Should().Be(mod);
            es.EventKeyword.Should().NotBe(default);
            es.Type.Should().NotBeNull();
            es.Type.ToString().Should().Be("b");
            es.Identifier.Should().NotBe(default);
            es.Identifier.ToString().Should().Be("c");

            es.AccessorList.OpenBraceToken.Should().NotBe(default);
            es.AccessorList.CloseBraceToken.Should().NotBe(default);
            es.AccessorList.Accessors.Count.Should().Be(2);

            es.AccessorList.Accessors[0].AttributeLists.Count.Should().Be(0);
            es.AccessorList.Accessors[0].Modifiers.Count.Should().Be(0);
            es.AccessorList.Accessors[0].Keyword.Should().NotBe(default);
            es.AccessorList.Accessors[0].Keyword.Kind().Should().Be(SyntaxKind.AddKeyword);
            es.AccessorList.Accessors[0].Body.Should().NotBeNull();
            es.AccessorList.Accessors[0].SemicolonToken.Kind().Should().Be(SyntaxKind.None);

            es.AccessorList.Accessors[1].AttributeLists.Count.Should().Be(0);
            es.AccessorList.Accessors[1].Modifiers.Count.Should().Be(0);
            es.AccessorList.Accessors[1].Keyword.Should().NotBe(default);
            es.AccessorList.Accessors[1].Keyword.Kind().Should().Be(SyntaxKind.RemoveKeyword);
            es.AccessorList.Accessors[1].Body.Should().NotBeNull();
            es.AccessorList.Accessors[1].SemicolonToken.Kind().Should().Be(SyntaxKind.None);
        }

        [Fact]
        public void TestClassEventPropertyWithModifiers()
        {
            TestClassEventPropertyWithModifier(SyntaxKind.PublicKeyword);
            TestClassEventPropertyWithModifier(SyntaxKind.PrivateKeyword);
            TestClassEventPropertyWithModifier(SyntaxKind.ProtectedKeyword);
            TestClassEventPropertyWithModifier(SyntaxKind.InternalKeyword);
            TestClassEventPropertyWithModifier(SyntaxKind.StaticKeyword);
            TestClassEventPropertyWithModifier(SyntaxKind.AbstractKeyword);
            TestClassEventPropertyWithModifier(SyntaxKind.VirtualKeyword);
            TestClassEventPropertyWithModifier(SyntaxKind.OverrideKeyword);
        }

        [Fact]
        public void TestClassEventPropertyWithAccessorModifiers()
        {
            TestClassEventPropertyWithModifier(SyntaxKind.PublicKeyword);
            TestClassEventPropertyWithModifier(SyntaxKind.PrivateKeyword);
            TestClassEventPropertyWithModifier(SyntaxKind.ProtectedKeyword);
            TestClassEventPropertyWithModifier(SyntaxKind.InternalKeyword);
            TestClassEventPropertyWithModifier(SyntaxKind.AbstractKeyword);
            TestClassEventPropertyWithModifier(SyntaxKind.VirtualKeyword);
            TestClassEventPropertyWithModifier(SyntaxKind.OverrideKeyword);
            TestClassEventPropertyWithModifier(SyntaxKind.NewKeyword);
            TestClassEventPropertyWithModifier(SyntaxKind.SealedKeyword);
        }

        [Fact]
        public void TestClassEventPropertyExplicit()
        {
            var text = "class a { event b I.c { add { } remove { } } }";
            var file = this.ParseFile(text);

            file.Should().NotBeNull();
            file.Members.Count.Should().Be(1);
            file.ToString().Should().Be(text);
            file.Errors().Length.Should().Be(0);

            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var cs = (TypeDeclarationSyntax)file.Members[0];
            cs.AttributeLists.Count.Should().Be(0);
            cs.Modifiers.Count.Should().Be(0);
            cs.Keyword.Should().NotBe(default);
            cs.Keyword.Kind().Should().Be(SyntaxKind.ClassKeyword);
            cs.Identifier.Should().NotBe(default);
            cs.Identifier.ToString().Should().Be("a");
            cs.BaseList.Should().BeNull();
            cs.ConstraintClauses.Count.Should().Be(0);
            cs.OpenBraceToken.Should().NotBe(default);
            cs.CloseBraceToken.Should().NotBe(default);

            cs.Members.Count.Should().Be(1);

            cs.Members[0].Kind().Should().Be(SyntaxKind.EventDeclaration);
            var es = (EventDeclarationSyntax)cs.Members[0];
            es.AttributeLists.Count.Should().Be(0);
            es.Modifiers.Count.Should().Be(0);
            es.EventKeyword.Should().NotBe(default);
            es.Type.Should().NotBeNull();
            es.Type.ToString().Should().Be("b");
            es.Identifier.Should().NotBe(default);
            es.ExplicitInterfaceSpecifier.Should().NotBeNull();
            es.ExplicitInterfaceSpecifier.Name.ToString().Should().Be("I");
            es.Identifier.ToString().Should().Be("c");

            es.AccessorList.OpenBraceToken.Should().NotBe(default);
            es.AccessorList.CloseBraceToken.Should().NotBe(default);
            es.AccessorList.Accessors.Count.Should().Be(2);

            es.AccessorList.Accessors[0].AttributeLists.Count.Should().Be(0);
            es.AccessorList.Accessors[0].Modifiers.Count.Should().Be(0);
            es.AccessorList.Accessors[0].Keyword.Should().NotBe(default);
            es.AccessorList.Accessors[0].Keyword.Kind().Should().Be(SyntaxKind.AddKeyword);
            es.AccessorList.Accessors[0].Body.Should().NotBeNull();
            es.AccessorList.Accessors[0].SemicolonToken.Kind().Should().Be(SyntaxKind.None);

            es.AccessorList.Accessors[1].AttributeLists.Count.Should().Be(0);
            es.AccessorList.Accessors[1].Modifiers.Count.Should().Be(0);
            es.AccessorList.Accessors[1].Keyword.Should().NotBe(default);
            es.AccessorList.Accessors[1].Keyword.Kind().Should().Be(SyntaxKind.RemoveKeyword);
            es.AccessorList.Accessors[1].Body.Should().NotBeNull();
            es.AccessorList.Accessors[1].SemicolonToken.Kind().Should().Be(SyntaxKind.None);
        }

        [Fact]
        public void TestClassIndexer()
        {
            var text = "class a { b this[c d] { get; set; } }";
            var file = this.ParseFile(text);

            file.Should().NotBeNull();
            file.Members.Count.Should().Be(1);
            file.ToString().Should().Be(text);
            file.Errors().Length.Should().Be(0);

            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var cs = (TypeDeclarationSyntax)file.Members[0];
            cs.AttributeLists.Count.Should().Be(0);
            cs.Modifiers.Count.Should().Be(0);
            cs.Keyword.Should().NotBe(default);
            cs.Keyword.Kind().Should().Be(SyntaxKind.ClassKeyword);
            cs.Identifier.Should().NotBe(default);
            cs.Identifier.ToString().Should().Be("a");
            cs.BaseList.Should().BeNull();
            cs.ConstraintClauses.Count.Should().Be(0);
            cs.OpenBraceToken.Should().NotBe(default);
            cs.CloseBraceToken.Should().NotBe(default);

            cs.Members.Count.Should().Be(1);

            cs.Members[0].Kind().Should().Be(SyntaxKind.IndexerDeclaration);
            var ps = (IndexerDeclarationSyntax)cs.Members[0];
            ps.AttributeLists.Count.Should().Be(0);
            ps.Modifiers.Count.Should().Be(0);
            ps.Type.Should().NotBeNull();
            ps.Type.ToString().Should().Be("b");
            ps.ThisKeyword.Should().NotBe(default);
            ps.ThisKeyword.ToString().Should().Be("this");

            ps.ParameterList.Should().NotBeNull(); // used with indexer property
            ps.ParameterList.OpenBracketToken.Should().NotBe(default);
            ps.ParameterList.OpenBracketToken.Kind().Should().Be(SyntaxKind.OpenBracketToken);
            ps.ParameterList.CloseBracketToken.Should().NotBe(default);
            ps.ParameterList.CloseBracketToken.Kind().Should().Be(SyntaxKind.CloseBracketToken);
            ps.ParameterList.Parameters.Count.Should().Be(1);
            ps.ParameterList.Parameters[0].AttributeLists.Count.Should().Be(0);
            ps.ParameterList.Parameters[0].Modifiers.Count.Should().Be(0);
            ps.ParameterList.Parameters[0].Type.Should().NotBeNull();
            ps.ParameterList.Parameters[0].Type.ToString().Should().Be("c");
            ps.ParameterList.Parameters[0].Identifier.Should().NotBe(default);
            ps.ParameterList.Parameters[0].Identifier.ToString().Should().Be("d");

            ps.AccessorList.OpenBraceToken.Should().NotBe(default);
            ps.AccessorList.CloseBraceToken.Should().NotBe(default);
            ps.AccessorList.Accessors.Count.Should().Be(2);

            ps.AccessorList.Accessors[0].AttributeLists.Count.Should().Be(0);
            ps.AccessorList.Accessors[0].Modifiers.Count.Should().Be(0);
            ps.AccessorList.Accessors[0].Keyword.Should().NotBe(default);
            ps.AccessorList.Accessors[0].Keyword.Kind().Should().Be(SyntaxKind.GetKeyword);
            ps.AccessorList.Accessors[0].Body.Should().BeNull();
            ps.AccessorList.Accessors[0].SemicolonToken.Should().NotBe(default);

            ps.AccessorList.Accessors[1].AttributeLists.Count.Should().Be(0);
            ps.AccessorList.Accessors[1].Modifiers.Count.Should().Be(0);
            ps.AccessorList.Accessors[1].Keyword.Should().NotBe(default);
            ps.AccessorList.Accessors[1].Keyword.Kind().Should().Be(SyntaxKind.SetKeyword);
            ps.AccessorList.Accessors[1].Body.Should().BeNull();
            ps.AccessorList.Accessors[1].SemicolonToken.Should().NotBe(default);
        }

        [Fact]
        public void TestClassIndexerWithRefReturn()
        {
            var text = "class a { ref b this[c d] { get; set; } }";
            var file = this.ParseFile(text);

            file.Should().NotBeNull();
            file.Members.Count.Should().Be(1);
            file.ToString().Should().Be(text);
            file.Errors().Length.Should().Be(0);

            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var cs = (TypeDeclarationSyntax)file.Members[0];
            cs.AttributeLists.Count.Should().Be(0);
            cs.Modifiers.Count.Should().Be(0);
            cs.Keyword.Should().NotBe(default);
            cs.Keyword.Kind().Should().Be(SyntaxKind.ClassKeyword);
            cs.Identifier.Should().NotBe(default);
            cs.Identifier.ToString().Should().Be("a");
            cs.BaseList.Should().BeNull();
            cs.ConstraintClauses.Count.Should().Be(0);
            cs.OpenBraceToken.Should().NotBe(default);
            cs.CloseBraceToken.Should().NotBe(default);

            cs.Members.Count.Should().Be(1);

            cs.Members[0].Kind().Should().Be(SyntaxKind.IndexerDeclaration);
            var ps = (IndexerDeclarationSyntax)cs.Members[0];
            ps.AttributeLists.Count.Should().Be(0);
            ps.Modifiers.Count.Should().Be(0);
            ps.Type.Should().NotBeNull();
            ps.Type.ToString().Should().Be("ref b");
            ps.ThisKeyword.Should().NotBe(default);
            ps.ThisKeyword.ToString().Should().Be("this");

            ps.ParameterList.Should().NotBeNull(); // used with indexer property
            ps.ParameterList.OpenBracketToken.Should().NotBe(default);
            ps.ParameterList.OpenBracketToken.Kind().Should().Be(SyntaxKind.OpenBracketToken);
            ps.ParameterList.CloseBracketToken.Should().NotBe(default);
            ps.ParameterList.CloseBracketToken.Kind().Should().Be(SyntaxKind.CloseBracketToken);
            ps.ParameterList.Parameters.Count.Should().Be(1);
            ps.ParameterList.Parameters[0].AttributeLists.Count.Should().Be(0);
            ps.ParameterList.Parameters[0].Modifiers.Count.Should().Be(0);
            ps.ParameterList.Parameters[0].Type.Should().NotBeNull();
            ps.ParameterList.Parameters[0].Type.ToString().Should().Be("c");
            ps.ParameterList.Parameters[0].Identifier.Should().NotBe(default);
            ps.ParameterList.Parameters[0].Identifier.ToString().Should().Be("d");

            ps.AccessorList.OpenBraceToken.Should().NotBe(default);
            ps.AccessorList.CloseBraceToken.Should().NotBe(default);
            ps.AccessorList.Accessors.Count.Should().Be(2);

            ps.AccessorList.Accessors[0].AttributeLists.Count.Should().Be(0);
            ps.AccessorList.Accessors[0].Modifiers.Count.Should().Be(0);
            ps.AccessorList.Accessors[0].Keyword.Should().NotBe(default);
            ps.AccessorList.Accessors[0].Keyword.Kind().Should().Be(SyntaxKind.GetKeyword);
            ps.AccessorList.Accessors[0].Body.Should().BeNull();
            ps.AccessorList.Accessors[0].SemicolonToken.Should().NotBe(default);

            ps.AccessorList.Accessors[1].AttributeLists.Count.Should().Be(0);
            ps.AccessorList.Accessors[1].Modifiers.Count.Should().Be(0);
            ps.AccessorList.Accessors[1].Keyword.Should().NotBe(default);
            ps.AccessorList.Accessors[1].Keyword.Kind().Should().Be(SyntaxKind.SetKeyword);
            ps.AccessorList.Accessors[1].Body.Should().BeNull();
            ps.AccessorList.Accessors[1].SemicolonToken.Should().NotBe(default);
        }

        [CompilerTrait(CompilerFeature.ReadOnlyReferences)]
        [Fact]
        public void TestClassIndexerWithRefReadonlyReturn()
        {
            var text = "class a { ref readonly b this[c d] { get; set; } }";
            var file = this.ParseFile(text, TestOptions.Regular);

            file.Should().NotBeNull();
            file.Members.Count.Should().Be(1);
            file.ToString().Should().Be(text);
            file.Errors().Length.Should().Be(0);

            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var cs = (TypeDeclarationSyntax)file.Members[0];
            cs.AttributeLists.Count.Should().Be(0);
            cs.Modifiers.Count.Should().Be(0);
            cs.Keyword.Should().NotBe(default);
            cs.Keyword.Kind().Should().Be(SyntaxKind.ClassKeyword);
            cs.Identifier.Should().NotBe(default);
            cs.Identifier.ToString().Should().Be("a");
            cs.BaseList.Should().BeNull();
            cs.ConstraintClauses.Count.Should().Be(0);
            cs.OpenBraceToken.Should().NotBe(default);
            cs.CloseBraceToken.Should().NotBe(default);

            cs.Members.Count.Should().Be(1);

            cs.Members[0].Kind().Should().Be(SyntaxKind.IndexerDeclaration);
            var ps = (IndexerDeclarationSyntax)cs.Members[0];
            ps.AttributeLists.Count.Should().Be(0);
            ps.Modifiers.Count.Should().Be(0);
            ps.Type.Should().NotBeNull();
            ps.Type.ToString().Should().Be("ref readonly b");
            ps.ThisKeyword.Should().NotBe(default);
            ps.ThisKeyword.ToString().Should().Be("this");

            ps.ParameterList.Should().NotBeNull(); // used with indexer property
            ps.ParameterList.OpenBracketToken.Should().NotBe(default);
            ps.ParameterList.OpenBracketToken.Kind().Should().Be(SyntaxKind.OpenBracketToken);
            ps.ParameterList.CloseBracketToken.Should().NotBe(default);
            ps.ParameterList.CloseBracketToken.Kind().Should().Be(SyntaxKind.CloseBracketToken);
            ps.ParameterList.Parameters.Count.Should().Be(1);
            ps.ParameterList.Parameters[0].AttributeLists.Count.Should().Be(0);
            ps.ParameterList.Parameters[0].Modifiers.Count.Should().Be(0);
            ps.ParameterList.Parameters[0].Type.Should().NotBeNull();
            ps.ParameterList.Parameters[0].Type.ToString().Should().Be("c");
            ps.ParameterList.Parameters[0].Identifier.Should().NotBe(default);
            ps.ParameterList.Parameters[0].Identifier.ToString().Should().Be("d");

            ps.AccessorList.OpenBraceToken.Should().NotBe(default);
            ps.AccessorList.CloseBraceToken.Should().NotBe(default);
            ps.AccessorList.Accessors.Count.Should().Be(2);

            ps.AccessorList.Accessors[0].AttributeLists.Count.Should().Be(0);
            ps.AccessorList.Accessors[0].Modifiers.Count.Should().Be(0);
            ps.AccessorList.Accessors[0].Keyword.Should().NotBe(default);
            ps.AccessorList.Accessors[0].Keyword.Kind().Should().Be(SyntaxKind.GetKeyword);
            ps.AccessorList.Accessors[0].Body.Should().BeNull();
            ps.AccessorList.Accessors[0].SemicolonToken.Should().NotBe(default);

            ps.AccessorList.Accessors[1].AttributeLists.Count.Should().Be(0);
            ps.AccessorList.Accessors[1].Modifiers.Count.Should().Be(0);
            ps.AccessorList.Accessors[1].Keyword.Should().NotBe(default);
            ps.AccessorList.Accessors[1].Keyword.Kind().Should().Be(SyntaxKind.SetKeyword);
            ps.AccessorList.Accessors[1].Body.Should().BeNull();
            ps.AccessorList.Accessors[1].SemicolonToken.Should().NotBe(default);
        }

        [Fact]
        public void TestClassIndexerWithMultipleParameters()
        {
            var text = "class a { b this[c d, e f] { get; set; } }";
            var file = this.ParseFile(text);

            file.Should().NotBeNull();
            file.Members.Count.Should().Be(1);
            file.ToString().Should().Be(text);
            file.Errors().Length.Should().Be(0);

            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var cs = (TypeDeclarationSyntax)file.Members[0];
            cs.AttributeLists.Count.Should().Be(0);
            cs.Modifiers.Count.Should().Be(0);
            cs.Keyword.Should().NotBe(default);
            cs.Keyword.Kind().Should().Be(SyntaxKind.ClassKeyword);
            cs.Identifier.Should().NotBe(default);
            cs.Identifier.ToString().Should().Be("a");
            cs.BaseList.Should().BeNull();
            cs.ConstraintClauses.Count.Should().Be(0);
            cs.OpenBraceToken.Should().NotBe(default);
            cs.CloseBraceToken.Should().NotBe(default);

            cs.Members.Count.Should().Be(1);

            cs.Members[0].Kind().Should().Be(SyntaxKind.IndexerDeclaration);
            var ps = (IndexerDeclarationSyntax)cs.Members[0];
            ps.AttributeLists.Count.Should().Be(0);
            ps.Modifiers.Count.Should().Be(0);
            ps.Type.Should().NotBeNull();
            ps.Type.ToString().Should().Be("b");
            ps.ThisKeyword.Should().NotBe(default);
            ps.ThisKeyword.ToString().Should().Be("this");

            ps.ParameterList.Should().NotBeNull(); // used with indexer property
            ps.ParameterList.OpenBracketToken.Should().NotBe(default);
            ps.ParameterList.OpenBracketToken.Kind().Should().Be(SyntaxKind.OpenBracketToken);
            ps.ParameterList.CloseBracketToken.Should().NotBe(default);
            ps.ParameterList.CloseBracketToken.Kind().Should().Be(SyntaxKind.CloseBracketToken);

            ps.ParameterList.Parameters.Count.Should().Be(2);

            ps.ParameterList.Parameters[0].AttributeLists.Count.Should().Be(0);
            ps.ParameterList.Parameters[0].Modifiers.Count.Should().Be(0);
            ps.ParameterList.Parameters[0].Type.Should().NotBeNull();
            ps.ParameterList.Parameters[0].Type.ToString().Should().Be("c");
            ps.ParameterList.Parameters[0].Identifier.Should().NotBe(default);
            ps.ParameterList.Parameters[0].Identifier.ToString().Should().Be("d");

            ps.ParameterList.Parameters[1].AttributeLists.Count.Should().Be(0);
            ps.ParameterList.Parameters[1].Modifiers.Count.Should().Be(0);
            ps.ParameterList.Parameters[1].Type.Should().NotBeNull();
            ps.ParameterList.Parameters[1].Type.ToString().Should().Be("e");
            ps.ParameterList.Parameters[1].Identifier.Should().NotBe(default);
            ps.ParameterList.Parameters[1].Identifier.ToString().Should().Be("f");

            ps.AccessorList.OpenBraceToken.Should().NotBe(default);
            ps.AccessorList.CloseBraceToken.Should().NotBe(default);
            ps.AccessorList.Accessors.Count.Should().Be(2);

            ps.AccessorList.Accessors[0].AttributeLists.Count.Should().Be(0);
            ps.AccessorList.Accessors[0].Modifiers.Count.Should().Be(0);
            ps.AccessorList.Accessors[0].Keyword.Should().NotBe(default);
            ps.AccessorList.Accessors[0].Keyword.Kind().Should().Be(SyntaxKind.GetKeyword);
            ps.AccessorList.Accessors[0].Body.Should().BeNull();
            ps.AccessorList.Accessors[0].SemicolonToken.Should().NotBe(default);

            ps.AccessorList.Accessors[1].AttributeLists.Count.Should().Be(0);
            ps.AccessorList.Accessors[1].Modifiers.Count.Should().Be(0);
            ps.AccessorList.Accessors[1].Keyword.Should().NotBe(default);
            ps.AccessorList.Accessors[1].Keyword.Kind().Should().Be(SyntaxKind.SetKeyword);
            ps.AccessorList.Accessors[1].Body.Should().BeNull();
            ps.AccessorList.Accessors[1].SemicolonToken.Should().NotBe(default);
        }

        [Fact]
        public void TestClassIndexerExplicit()
        {
            var text = "class a { b I.this[c d] { get; set; } }";
            var file = this.ParseFile(text);

            file.Should().NotBeNull();
            file.Members.Count.Should().Be(1);
            file.ToString().Should().Be(text);
            file.Errors().Length.Should().Be(0);

            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var cs = (TypeDeclarationSyntax)file.Members[0];
            cs.AttributeLists.Count.Should().Be(0);
            cs.Modifiers.Count.Should().Be(0);
            cs.Keyword.Should().NotBe(default);
            cs.Keyword.Kind().Should().Be(SyntaxKind.ClassKeyword);
            cs.Identifier.Should().NotBe(default);
            cs.Identifier.ToString().Should().Be("a");
            cs.BaseList.Should().BeNull();
            cs.ConstraintClauses.Count.Should().Be(0);
            cs.OpenBraceToken.Should().NotBe(default);
            cs.CloseBraceToken.Should().NotBe(default);

            cs.Members.Count.Should().Be(1);

            cs.Members[0].Kind().Should().Be(SyntaxKind.IndexerDeclaration);
            var ps = (IndexerDeclarationSyntax)cs.Members[0];
            ps.AttributeLists.Count.Should().Be(0);
            ps.Modifiers.Count.Should().Be(0);
            ps.Type.Should().NotBeNull();
            ps.Type.ToString().Should().Be("b");
            ps.ExplicitInterfaceSpecifier.Should().NotBeNull();
            ps.ExplicitInterfaceSpecifier.Name.ToString().Should().Be("I");
            ps.ExplicitInterfaceSpecifier.DotToken.ToString().Should().Be(".");
            ps.ThisKeyword.ToString().Should().Be("this");

            ps.ParameterList.Should().NotBeNull(); // used with indexer property
            ps.ParameterList.OpenBracketToken.Should().NotBe(default);
            ps.ParameterList.OpenBracketToken.Kind().Should().Be(SyntaxKind.OpenBracketToken);
            ps.ParameterList.CloseBracketToken.Should().NotBe(default);
            ps.ParameterList.CloseBracketToken.Kind().Should().Be(SyntaxKind.CloseBracketToken);
            ps.ParameterList.Parameters.Count.Should().Be(1);
            ps.ParameterList.Parameters[0].AttributeLists.Count.Should().Be(0);
            ps.ParameterList.Parameters[0].Modifiers.Count.Should().Be(0);
            ps.ParameterList.Parameters[0].Type.Should().NotBeNull();
            ps.ParameterList.Parameters[0].Type.ToString().Should().Be("c");
            ps.ParameterList.Parameters[0].Identifier.Should().NotBe(default);
            ps.ParameterList.Parameters[0].Identifier.ToString().Should().Be("d");

            ps.AccessorList.OpenBraceToken.Should().NotBe(default);
            ps.AccessorList.CloseBraceToken.Should().NotBe(default);
            ps.AccessorList.Accessors.Count.Should().Be(2);

            ps.AccessorList.Accessors[0].AttributeLists.Count.Should().Be(0);
            ps.AccessorList.Accessors[0].Modifiers.Count.Should().Be(0);
            ps.AccessorList.Accessors[0].Keyword.Should().NotBe(default);
            ps.AccessorList.Accessors[0].Keyword.Kind().Should().Be(SyntaxKind.GetKeyword);
            ps.AccessorList.Accessors[0].Body.Should().BeNull();
            ps.AccessorList.Accessors[0].SemicolonToken.Should().NotBe(default);

            ps.AccessorList.Accessors[1].AttributeLists.Count.Should().Be(0);
            ps.AccessorList.Accessors[1].Modifiers.Count.Should().Be(0);
            ps.AccessorList.Accessors[1].Keyword.Should().NotBe(default);
            ps.AccessorList.Accessors[1].Keyword.Kind().Should().Be(SyntaxKind.SetKeyword);
            ps.AccessorList.Accessors[1].Body.Should().BeNull();
            ps.AccessorList.Accessors[1].SemicolonToken.Should().NotBe(default);
        }

        private void TestClassBinaryOperatorMethod(SyntaxKind op1)
        {
            var text = "class a { b operator " + SyntaxFacts.GetText(op1) + " (c d, e f) { } }";
            var file = this.ParseFile(text);

            file.Should().NotBeNull();
            file.Members.Count.Should().Be(1);
            file.ToString().Should().Be(text);
            file.Errors().Length.Should().Be(0);

            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var cs = (TypeDeclarationSyntax)file.Members[0];
            cs.AttributeLists.Count.Should().Be(0);
            cs.Modifiers.Count.Should().Be(0);
            cs.Keyword.Should().NotBe(default);
            cs.Keyword.Kind().Should().Be(SyntaxKind.ClassKeyword);
            cs.Identifier.Should().NotBe(default);
            cs.Identifier.ToString().Should().Be("a");
            cs.BaseList.Should().BeNull();
            cs.ConstraintClauses.Count.Should().Be(0);
            cs.OpenBraceToken.Should().NotBe(default);
            cs.CloseBraceToken.Should().NotBe(default);

            cs.Members.Count.Should().Be(1);

            cs.Members[0].Kind().Should().Be(SyntaxKind.OperatorDeclaration);
            var ps = (OperatorDeclarationSyntax)cs.Members[0];
            ps.AttributeLists.Count.Should().Be(0);
            ps.Modifiers.Count.Should().Be(0);
            ps.ReturnType.Should().NotBeNull();
            ps.ReturnType.ToString().Should().Be("b");
            ps.OperatorKeyword.Should().NotBe(default);
            ps.OperatorKeyword.Kind().Should().Be(SyntaxKind.OperatorKeyword);
            ps.OperatorToken.Should().NotBe(default);
            ps.OperatorToken.Kind().Should().Be(op1);
            ps.ParameterList.OpenParenToken.Should().NotBe(default);
            ps.ParameterList.CloseParenToken.Should().NotBe(default);
            ps.Body.Should().NotBeNull();

            ps.ParameterList.Parameters.Count.Should().Be(2);

            ps.ParameterList.Parameters[0].AttributeLists.Count.Should().Be(0);
            ps.ParameterList.Parameters[0].Modifiers.Count.Should().Be(0);
            ps.ParameterList.Parameters[0].Type.Should().NotBeNull();
            ps.ParameterList.Parameters[0].Type.ToString().Should().Be("c");
            ps.ParameterList.Parameters[0].Identifier.Should().NotBe(default);
            ps.ParameterList.Parameters[0].Identifier.ToString().Should().Be("d");

            ps.ParameterList.Parameters[1].AttributeLists.Count.Should().Be(0);
            ps.ParameterList.Parameters[1].Modifiers.Count.Should().Be(0);
            ps.ParameterList.Parameters[1].Type.Should().NotBeNull();
            ps.ParameterList.Parameters[1].Type.ToString().Should().Be("e");
            ps.ParameterList.Parameters[1].Identifier.Should().NotBe(default);
            ps.ParameterList.Parameters[1].Identifier.ToString().Should().Be("f");
        }

        [Fact]
        public void TestClassBinaryOperatorMethods()
        {
            TestClassBinaryOperatorMethod(SyntaxKind.PlusToken);
            TestClassBinaryOperatorMethod(SyntaxKind.MinusToken);
            TestClassBinaryOperatorMethod(SyntaxKind.AsteriskToken);
            TestClassBinaryOperatorMethod(SyntaxKind.SlashToken);
            TestClassBinaryOperatorMethod(SyntaxKind.PercentToken);
            TestClassBinaryOperatorMethod(SyntaxKind.CaretToken);
            TestClassBinaryOperatorMethod(SyntaxKind.AmpersandToken);
            TestClassBinaryOperatorMethod(SyntaxKind.BarToken);

            // TestClassBinaryOperatorMethod(SyntaxKind.AmpersandAmpersandToken);
            // TestClassBinaryOperatorMethod(SyntaxKind.BarBarToken);
            TestClassBinaryOperatorMethod(SyntaxKind.LessThanToken);
            TestClassBinaryOperatorMethod(SyntaxKind.LessThanEqualsToken);
            TestClassBinaryOperatorMethod(SyntaxKind.LessThanLessThanToken);
            TestClassBinaryOperatorMethod(SyntaxKind.GreaterThanToken);
            TestClassBinaryOperatorMethod(SyntaxKind.GreaterThanEqualsToken);
            TestClassBinaryOperatorMethod(SyntaxKind.EqualsEqualsToken);
            TestClassBinaryOperatorMethod(SyntaxKind.ExclamationEqualsToken);
        }

        [Fact]
        public void TestClassRightShiftOperatorMethod()
        {
            var text = "class a { b operator >> (c d, e f) { } }";
            var file = this.ParseFile(text);

            file.Should().NotBeNull();
            file.Members.Count.Should().Be(1);
            file.ToString().Should().Be(text);
            file.Errors().Length.Should().Be(0);

            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var cs = (TypeDeclarationSyntax)file.Members[0];
            cs.AttributeLists.Count.Should().Be(0);
            cs.Modifiers.Count.Should().Be(0);
            cs.Keyword.Should().NotBe(default);
            cs.Keyword.Kind().Should().Be(SyntaxKind.ClassKeyword);
            cs.Identifier.Should().NotBe(default);
            cs.Identifier.ToString().Should().Be("a");
            cs.BaseList.Should().BeNull();
            cs.ConstraintClauses.Count.Should().Be(0);
            cs.OpenBraceToken.Should().NotBe(default);
            cs.CloseBraceToken.Should().NotBe(default);

            cs.Members.Count.Should().Be(1);

            cs.Members[0].Kind().Should().Be(SyntaxKind.OperatorDeclaration);
            var ps = (OperatorDeclarationSyntax)cs.Members[0];
            ps.AttributeLists.Count.Should().Be(0);
            ps.Modifiers.Count.Should().Be(0);
            ps.ReturnType.Should().NotBeNull();
            ps.ReturnType.ToString().Should().Be("b");
            ps.OperatorKeyword.Should().NotBe(default);
            ps.OperatorKeyword.Kind().Should().Be(SyntaxKind.OperatorKeyword);
            ps.OperatorToken.Should().NotBe(default);
            ps.OperatorToken.Kind().Should().Be(SyntaxKind.GreaterThanGreaterThanToken);
            ps.ParameterList.OpenParenToken.Should().NotBe(default);
            ps.ParameterList.CloseParenToken.Should().NotBe(default);
            ps.Body.Should().NotBeNull();

            ps.ParameterList.Parameters.Count.Should().Be(2);

            ps.ParameterList.Parameters[0].AttributeLists.Count.Should().Be(0);
            ps.ParameterList.Parameters[0].Modifiers.Count.Should().Be(0);
            ps.ParameterList.Parameters[0].Type.Should().NotBeNull();
            ps.ParameterList.Parameters[0].Type.ToString().Should().Be("c");
            ps.ParameterList.Parameters[0].Identifier.Should().NotBe(default);
            ps.ParameterList.Parameters[0].Identifier.ToString().Should().Be("d");

            ps.ParameterList.Parameters[1].AttributeLists.Count.Should().Be(0);
            ps.ParameterList.Parameters[1].Modifiers.Count.Should().Be(0);
            ps.ParameterList.Parameters[1].Type.Should().NotBeNull();
            ps.ParameterList.Parameters[1].Type.ToString().Should().Be("e");
            ps.ParameterList.Parameters[1].Identifier.Should().NotBe(default);
            ps.ParameterList.Parameters[1].Identifier.ToString().Should().Be("f");
        }

        [Fact]
        public void TestClassUnsignedRightShiftOperatorMethod()
        {
            var text = "class a { b operator >>> (c d, e f) { } }";
            var file = this.ParseFile(text);

            UsingNode(text, file);

            N(SyntaxKind.CompilationUnit);
            {
                N(SyntaxKind.ClassDeclaration);
                {
                    N(SyntaxKind.ClassKeyword);
                    N(SyntaxKind.IdentifierToken, "a");
                    N(SyntaxKind.OpenBraceToken);
                    N(SyntaxKind.OperatorDeclaration);
                    {
                        N(SyntaxKind.IdentifierName);
                        {
                            N(SyntaxKind.IdentifierToken, "b");
                        }
                        N(SyntaxKind.OperatorKeyword);
                        N(SyntaxKind.GreaterThanGreaterThanGreaterThanToken);
                        N(SyntaxKind.ParameterList);
                        {
                            N(SyntaxKind.OpenParenToken);
                            N(SyntaxKind.Parameter);
                            {
                                N(SyntaxKind.IdentifierName);
                                {
                                    N(SyntaxKind.IdentifierToken, "c");
                                }
                                N(SyntaxKind.IdentifierToken, "d");
                            }
                            N(SyntaxKind.CommaToken);
                            N(SyntaxKind.Parameter);
                            {
                                N(SyntaxKind.IdentifierName);
                                {
                                    N(SyntaxKind.IdentifierToken, "e");
                                }
                                N(SyntaxKind.IdentifierToken, "f");
                            }
                            N(SyntaxKind.CloseParenToken);
                        }
                        N(SyntaxKind.Block);
                        {
                            N(SyntaxKind.OpenBraceToken);
                            N(SyntaxKind.CloseBraceToken);
                        }
                    }
                    N(SyntaxKind.CloseBraceToken);
                }
                N(SyntaxKind.EndOfFileToken);
            }
            EOF();
        }

        private void TestClassUnaryOperatorMethod(SyntaxKind op1)
        {
            var text = "class a { b operator " + SyntaxFacts.GetText(op1) + " (c d) { } }";
            var file = this.ParseFile(text);

            file.Should().NotBeNull();
            file.Members.Count.Should().Be(1);
            file.ToString().Should().Be(text);
            file.Errors().Length.Should().Be(0);

            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var cs = (TypeDeclarationSyntax)file.Members[0];
            cs.AttributeLists.Count.Should().Be(0);
            cs.Modifiers.Count.Should().Be(0);
            cs.Keyword.Should().NotBe(default);
            cs.Keyword.Kind().Should().Be(SyntaxKind.ClassKeyword);
            cs.Identifier.Should().NotBe(default);
            cs.Identifier.ToString().Should().Be("a");
            cs.BaseList.Should().BeNull();
            cs.ConstraintClauses.Count.Should().Be(0);
            cs.OpenBraceToken.Should().NotBe(default);
            cs.CloseBraceToken.Should().NotBe(default);

            cs.Members.Count.Should().Be(1);

            cs.Members[0].Kind().Should().Be(SyntaxKind.OperatorDeclaration);
            var ps = (OperatorDeclarationSyntax)cs.Members[0];
            ps.AttributeLists.Count.Should().Be(0);
            ps.Modifiers.Count.Should().Be(0);
            ps.ReturnType.Should().NotBeNull();
            ps.ReturnType.ToString().Should().Be("b");
            ps.OperatorKeyword.Should().NotBe(default);
            ps.OperatorKeyword.Kind().Should().Be(SyntaxKind.OperatorKeyword);
            ps.OperatorToken.Should().NotBe(default);
            ps.OperatorToken.Kind().Should().Be(op1);
            ps.ParameterList.OpenParenToken.Should().NotBe(default);
            ps.ParameterList.CloseParenToken.Should().NotBe(default);
            ps.Body.Should().NotBeNull();

            ps.ParameterList.Parameters.Count.Should().Be(1);

            ps.ParameterList.Parameters[0].AttributeLists.Count.Should().Be(0);
            ps.ParameterList.Parameters[0].Modifiers.Count.Should().Be(0);
            ps.ParameterList.Parameters[0].Type.Should().NotBeNull();
            ps.ParameterList.Parameters[0].Type.ToString().Should().Be("c");
            ps.ParameterList.Parameters[0].Identifier.Should().NotBe(default);
            ps.ParameterList.Parameters[0].Identifier.ToString().Should().Be("d");
        }

        [Fact]
        public void TestClassUnaryOperatorMethods()
        {
            TestClassUnaryOperatorMethod(SyntaxKind.PlusToken);
            TestClassUnaryOperatorMethod(SyntaxKind.MinusToken);
            TestClassUnaryOperatorMethod(SyntaxKind.TildeToken);
            TestClassUnaryOperatorMethod(SyntaxKind.ExclamationToken);
            TestClassUnaryOperatorMethod(SyntaxKind.PlusPlusToken);
            TestClassUnaryOperatorMethod(SyntaxKind.MinusMinusToken);
            TestClassUnaryOperatorMethod(SyntaxKind.TrueKeyword);
            TestClassUnaryOperatorMethod(SyntaxKind.FalseKeyword);
        }

        [Fact]
        public void TestClassImplicitConversionOperatorMethod()
        {
            var text = "class a { implicit operator b (c d) { } }";
            var file = this.ParseFile(text);

            file.Should().NotBeNull();
            file.Members.Count.Should().Be(1);
            file.ToString().Should().Be(text);
            file.Errors().Length.Should().Be(0);

            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var cs = (TypeDeclarationSyntax)file.Members[0];
            cs.AttributeLists.Count.Should().Be(0);
            cs.Modifiers.Count.Should().Be(0);
            cs.Keyword.Should().NotBe(default);
            cs.Keyword.Kind().Should().Be(SyntaxKind.ClassKeyword);
            cs.Identifier.Should().NotBe(default);
            cs.Identifier.ToString().Should().Be("a");
            cs.BaseList.Should().BeNull();
            cs.ConstraintClauses.Count.Should().Be(0);
            cs.OpenBraceToken.Should().NotBe(default);
            cs.CloseBraceToken.Should().NotBe(default);

            cs.Members.Count.Should().Be(1);

            cs.Members[0].Kind().Should().Be(SyntaxKind.ConversionOperatorDeclaration);
            var ms = (ConversionOperatorDeclarationSyntax)cs.Members[0];
            ms.AttributeLists.Count.Should().Be(0);
            ms.Modifiers.Count.Should().Be(0);
            ms.ImplicitOrExplicitKeyword.Should().NotBe(default);
            ms.ImplicitOrExplicitKeyword.Kind().Should().Be(SyntaxKind.ImplicitKeyword);
            ms.OperatorKeyword.Should().NotBe(default);
            ms.OperatorKeyword.Kind().Should().Be(SyntaxKind.OperatorKeyword);
            ms.Type.Should().NotBeNull();
            ms.Type.ToString().Should().Be("b");
            ms.ParameterList.OpenParenToken.Should().NotBe(default);
            ms.ParameterList.CloseParenToken.Should().NotBe(default);

            ms.ParameterList.Parameters.Count.Should().Be(1);
            ms.ParameterList.Parameters[0].AttributeLists.Count.Should().Be(0);
            ms.ParameterList.Parameters[0].Modifiers.Count.Should().Be(0);
            ms.ParameterList.Parameters[0].Type.Should().NotBeNull();
            ms.ParameterList.Parameters[0].Type.ToString().Should().Be("c");
            ms.ParameterList.Parameters[0].Identifier.Should().NotBe(default);
            ms.ParameterList.Parameters[0].Identifier.ToString().Should().Be("d");
        }

        [Fact]
        public void TestClassExplicitConversionOperatorMethod()
        {
            var text = "class a { explicit operator b (c d) { } }";
            var file = this.ParseFile(text);

            file.Should().NotBeNull();
            file.Members.Count.Should().Be(1);
            file.ToString().Should().Be(text);
            file.Errors().Length.Should().Be(0);

            file.Members[0].Kind().Should().Be(SyntaxKind.ClassDeclaration);
            var cs = (TypeDeclarationSyntax)file.Members[0];
            cs.AttributeLists.Count.Should().Be(0);
            cs.Modifiers.Count.Should().Be(0);
            cs.Keyword.Should().NotBe(default);
            cs.Keyword.Kind().Should().Be(SyntaxKind.ClassKeyword);
            cs.Identifier.Should().NotBe(default);
            cs.Identifier.ToString().Should().Be("a");
            cs.BaseList.Should().BeNull();
            cs.ConstraintClauses.Count.Should().Be(0);
            cs.OpenBraceToken.Should().NotBe(default);
            cs.CloseBraceToken.Should().NotBe(default);

            cs.Members.Count.Should().Be(1);

            cs.Members[0].Kind().Should().Be(SyntaxKind.ConversionOperatorDeclaration);
            var ms = (ConversionOperatorDeclarationSyntax)cs.Members[0];
            ms.AttributeLists.Count.Should().Be(0);
            ms.Modifiers.Count.Should().Be(0);
            ms.ImplicitOrExplicitKeyword.Should().NotBe(default);
            ms.ImplicitOrExplicitKeyword.Kind().Should().Be(SyntaxKind.ExplicitKeyword);
            ms.OperatorKeyword.Should().NotBe(default);
            ms.OperatorKeyword.Kind().Should().Be(SyntaxKind.OperatorKeyword);
            ms.Type.Should().NotBeNull();
            ms.Type.ToString().Should().Be("b");
            ms.ParameterList.OpenParenToken.Should().NotBe(default);
            ms.ParameterList.CloseParenToken.Should().NotBe(default);

            ms.ParameterList.Parameters.Count.Should().Be(1);
            ms.ParameterList.Parameters[0].AttributeLists.Count.Should().Be(0);
            ms.ParameterList.Parameters[0].Modifiers.Count.Should().Be(0);
            ms.ParameterList.Parameters[0].Type.Should().NotBeNull();
            ms.ParameterList.Parameters[0].Type.ToString().Should().Be("c");
            ms.ParameterList.Parameters[0].Identifier.Should().NotBe(default);
            ms.ParameterList.Parameters[0].Identifier.ToString().Should().Be("d");
        }

        [Fact]
        public void TestNamespaceDeclarationsBadNames()
        {
            var text = "namespace A::B { }";
            var file = this.ParseFile(text);

            file.Should().NotBeNull();
            file.Members.Count.Should().Be(1);
            file.Errors().Length.Should().Be(0);
            file.ToString().Should().Be(text);

            var ns = (NamespaceDeclarationSyntax)file.Members[0];
            ns.Errors().Length.Should().Be(0);
            ns.Name.Kind().Should().Be(SyntaxKind.AliasQualifiedName);

            text = "namespace A<B> { }";
            file = this.ParseFile(text);

            file.Should().NotBeNull();
            file.Members.Count.Should().Be(1);
            file.Errors().Length.Should().Be(0);
            file.ToString().Should().Be(text);

            ns = (NamespaceDeclarationSyntax)file.Members[0];
            ns.Errors().Length.Should().Be(0);
            ns.Name.Kind().Should().Be(SyntaxKind.GenericName);

            text = "namespace A<,> { }";
            file = this.ParseFile(text);

            file.Should().NotBeNull();
            file.Members.Count.Should().Be(1);
            file.Errors().Length.Should().Be(0);
            file.ToString().Should().Be(text);

            ns = (NamespaceDeclarationSyntax)file.Members[0];
            ns.Errors().Length.Should().Be(0);
            ns.Name.Kind().Should().Be(SyntaxKind.GenericName);
        }

        [Fact]
        public void TestNamespaceDeclarationsBadNames1()
        {
            var text = @"namespace A::B { }";
            CreateCompilation(text).VerifyDiagnostics(
                // (1,11): error CS7000: Unexpected use of an aliased name
                // namespace A::B { }
                Diagnostic(ErrorCode.ERR_UnexpectedAliasedName, "A::B").WithLocation(1, 11));
        }

        [Fact]
        public void TestNamespaceDeclarationsBadNames2()
        {
            var text = @"namespace A<B> { }";
            CreateCompilation(text).VerifyDiagnostics(
                // (1,11): error CS7002: Unexpected use of a generic name
                // namespace A<B> { }
                Diagnostic(ErrorCode.ERR_UnexpectedGenericName, "A<B>").WithLocation(1, 11));
        }

        [Fact]
        public void TestNamespaceDeclarationsBadNames3()
        {
            var text = @"namespace A<,> { }";
            CreateCompilation(text).VerifyDiagnostics(
                // (1,11): error CS7002: Unexpected use of a generic name
                // namespace A<,> { }
                Diagnostic(ErrorCode.ERR_UnexpectedGenericName, "A<,>").WithLocation(1, 11));
        }

        [WorkItem(537690, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/537690")]
        [Fact]
        public void TestMissingSemicolonAfterListInitializer()
        {
            var text = @"using System;
using System.Linq;
using AwesomeAssertions;
class Program {
  static void Main() {
    var r = new List<int>() { 3, 3 }
    var s = 2;
  }
}
";
            var file = this.ParseFile(text);
            file.Errors().Length.Should().Be(1);
            file.Errors()[0].Code.Should().Be((int)ErrorCode.ERR_SemicolonExpected);
        }

        [Fact]
        public void TestPartialPartial()
        {
            var text = @"
partial class PartialPartial
{
    int i = 1;
    partial partial void PM();
    partial partial void PM()
    {
        i = 0;
    }
    static int Main()
    {
        PartialPartial t = new PartialPartial();
        t.PM();
        return t.i;
    }
}
";
            // These errors aren't great.  Ideally we can improve things in the future.
            CreateCompilation(text).VerifyDiagnostics(
                // (5,13): error CS1525: Invalid expression term 'partial'
                //     partial partial void PM();
                Diagnostic(ErrorCode.ERR_InvalidExprTerm, "partial").WithArguments("partial").WithLocation(5, 13),
                // (5,13): error CS1002: ; expected
                //     partial partial void PM();
                Diagnostic(ErrorCode.ERR_SemicolonExpected, "partial").WithLocation(5, 13),
                // (6,13): error CS1525: Invalid expression term 'partial'
                //     partial partial void PM()
                Diagnostic(ErrorCode.ERR_InvalidExprTerm, "partial").WithArguments("partial").WithLocation(6, 13),
                // (6,13): error CS1002: ; expected
                //     partial partial void PM()
                Diagnostic(ErrorCode.ERR_SemicolonExpected, "partial").WithLocation(6, 13),
                // (6,13): error CS0102: The type 'PartialPartial' already contains a definition for ''
                //     partial partial void PM()
                Diagnostic(ErrorCode.ERR_DuplicateNameInClass, "").WithArguments("PartialPartial", "").WithLocation(6, 13),
                // (5,5): error CS0246: The type or namespace name 'partial' could not be found (are you missing a using directive or an assembly reference?)
                //     partial partial void PM();
                Diagnostic(ErrorCode.ERR_SingleTypeNameNotFound, "partial").WithArguments("partial").WithLocation(5, 5),
                // (6,5): error CS0246: The type or namespace name 'partial' could not be found (are you missing a using directive or an assembly reference?)
                //     partial partial void PM()
                Diagnostic(ErrorCode.ERR_SingleTypeNameNotFound, "partial").WithArguments("partial").WithLocation(6, 5));
        }

        [Fact]
        public void TestPartialEnum()
        {
            var text = @"partial enum E{}";
            CreateCompilationWithMscorlib45(text).VerifyDiagnostics(
                // (1,14): error CS0267: The 'partial' modifier can only appear immediately before 'class', 'record', 'struct', 'interface', or a method return type.
                // partial enum E{}
                Diagnostic(ErrorCode.ERR_PartialMisplaced, "E").WithLocation(1, 14));
        }

        [WorkItem(539120, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/539120")]
        [Fact]
        public void TestEscapedConstructor()
        {
            var text = @"
class @class
{
    public @class()
    {
    }
}
";
            var file = this.ParseFile(text);
            file.Errors().Length.Should().Be(0);
        }

        [WorkItem(536956, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/536956")]
        [Fact]
        public void TestAnonymousMethodWithDefaultParameter()
        {
            var text = @"
delegate void F(int x);
class C {
   void M() {
     F f = delegate (int x = 0) { };
   }
}
";
            var file = this.ParseFile(text);

            file.Should().NotBeNull();
            file.Errors().Length.Should().Be(0);

            CreateCompilation(text).VerifyDiagnostics(
                // (5,28): error CS1065: Default values are not valid in this context.
                //      F f = delegate (int x = 0) { };
                Diagnostic(ErrorCode.ERR_DefaultValueNotAllowed, "=").WithLocation(5, 28));
        }

        [WorkItem(537865, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/537865")]
        [Fact]
        public void RegressIfDevTrueUnicode()
        {
            var text = @"
class P
{
static void Main()
{
#if tru\u0065
System.Console.WriteLine(""Good, backwards compatible"");
#else
System.Console.WriteLine(""Bad, breaking change"");
#endif
}
}
";

            TestConditionalCompilation(text, desiredText: "Good", undesiredText: "Bad");
        }

        [WorkItem(537815, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/537815")]
        [Fact]
        public void RegressLongDirectiveIdentifierDefn()
        {
            var text = @"
//130 chars (max is 128)
#define A234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890
class P
{
static void Main()
{
//first 128 chars of defined value
#if A2345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678
System.Console.WriteLine(""Good, backwards compatible"");
#else
System.Console.WriteLine(""Bad, breaking change"");
#endif
}
}
";

            TestConditionalCompilation(text, desiredText: "Good", undesiredText: "Bad");
        }

        [WorkItem(537815, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/537815")]
        [Fact]
        public void RegressLongDirectiveIdentifierUse()
        {
            var text = @"
//128 chars (max)
#define A2345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678
class P
{
static void Main()
{
//defined value + two chars (larger than max)
#if A234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890
System.Console.WriteLine(""Good, backwards compatible"");
#else
System.Console.WriteLine(""Bad, breaking change"");
#endif
}
}
";

            TestConditionalCompilation(text, desiredText: "Good", undesiredText: "Bad");
        }

        //Expects a single class, containing a single method, containing a single statement.
        //Presumably, the statement depends on a conditional compilation directive.
        private void TestConditionalCompilation(string text, string desiredText, string undesiredText)
        {
            var file = this.ParseFile(text);

            file.Should().NotBeNull();
            file.Members.Count.Should().Be(1);
            file.Errors().Length.Should().Be(0);
            file.ToFullString().Should().Be(text);

            var @class = (TypeDeclarationSyntax)file.Members[0];
            var mainMethod = (MethodDeclarationSyntax)@class.Members[0];

            mainMethod.Body.Should().NotBeNull();
            mainMethod.Body.Statements.Count.Should().Be(1);

            var statement = mainMethod.Body.Statements[0];
            var stmtText = statement.ToString();

            //make sure we compiled out the right statement
            stmtText.Should().Contain(desiredText);
            stmtText.Should().NotContain(undesiredText);
        }

        [Fact]
        public void TestBadlyPlacedParams()
        {
            var text1 = @"
class C 
{
   void M(params int[] i, int j)  {}
}";
            var text2 = @"
class C 
{
   void M(__arglist, int j)  {}
}";

            CreateCompilation(text1).VerifyDiagnostics(
                // (4,11): error CS0231: A params parameter must be the last parameter in a parameter list
                //    void M(params int[] i, int j)  {}
                Diagnostic(ErrorCode.ERR_ParamsLast, "params int[] i").WithLocation(4, 11));
            CreateCompilation(text2).VerifyDiagnostics(
                // (4,11): error CS0257: An __arglist parameter must be the last parameter in a parameter list
                //    void M(__arglist, int j)  {}
                Diagnostic(ErrorCode.ERR_VarargsLast, "__arglist").WithLocation(4, 11));
        }

        [Fact]
        public void ValidFixedBufferTypes()
        {
            var text = @"
unsafe struct s
{
    public fixed bool _Type1[10];
    internal fixed int _Type3[10];
    private fixed short _Type4[10];
    unsafe fixed long _Type5[10];
    new fixed char _Type6[10];    
}
";
            var file = this.ParseFile(text);
            file.Errors().Length.Should().Be(0);
        }

        [Fact]
        public void ValidFixedBufferTypesMultipleDeclarationsOnSameLine()
        {
            var text = @"
unsafe struct s
{
    public fixed bool _Type1[10], _Type2[10], _Type3[20];
}
";
            var file = this.ParseFile(text);
            file.Errors().Length.Should().Be(0);
        }

        [Fact]
        public void ValidFixedBufferTypesWithCountFromConstantOrLiteral()
        {
            var text = @"
unsafe struct s
{
    public const int abc = 10;
    public fixed bool _Type1[abc];
    public fixed bool _Type2[20];
    }
";
            var file = this.ParseFile(text);
            file.Errors().Length.Should().Be(0);
        }

        [Fact]
        public void ValidFixedBufferTypesAllValidTypes()
        {
            var text = @"
unsafe struct s
{
    public fixed bool _Type1[10]; 
    public fixed byte _Type12[10]; 
    public fixed int _Type2[10]; 
    public fixed short _Type3[10]; 
    public fixed long _Type4[10]; 
    public fixed char _Type5[10]; 
    public fixed sbyte _Type6[10]; 
    public fixed ushort _Type7[10]; 
    public fixed uint _Type8[10]; 
    public fixed ulong _Type9[10]; 
    public fixed float _Type10[10]; 
    public fixed double _Type11[10];     
 }


";
            var file = this.ParseFile(text);
            file.Errors().Length.Should().Be(0);
        }

        [Fact]
        public void CS0071_01()
        {
            UsingTree(@"
public interface I2 { }
public interface I1
{
    event System.Action I2.P10;
}
");
            N(SyntaxKind.CompilationUnit);
            {
                N(SyntaxKind.InterfaceDeclaration);
                {
                    N(SyntaxKind.PublicKeyword);
                    N(SyntaxKind.InterfaceKeyword);
                    N(SyntaxKind.IdentifierToken, "I2");
                    N(SyntaxKind.OpenBraceToken);
                    N(SyntaxKind.CloseBraceToken);
                }
                N(SyntaxKind.InterfaceDeclaration);
                {
                    N(SyntaxKind.PublicKeyword);
                    N(SyntaxKind.InterfaceKeyword);
                    N(SyntaxKind.IdentifierToken, "I1");
                    N(SyntaxKind.OpenBraceToken);
                    N(SyntaxKind.EventDeclaration);
                    {
                        N(SyntaxKind.EventKeyword);
                        N(SyntaxKind.QualifiedName);
                        {
                            N(SyntaxKind.IdentifierName);
                            {
                                N(SyntaxKind.IdentifierToken, "System");
                            }
                            N(SyntaxKind.DotToken);
                            N(SyntaxKind.IdentifierName);
                            {
                                N(SyntaxKind.IdentifierToken, "Action");
                            }
                        }
                        N(SyntaxKind.ExplicitInterfaceSpecifier);
                        {
                            N(SyntaxKind.IdentifierName);
                            {
                                N(SyntaxKind.IdentifierToken, "I2");
                            }
                            N(SyntaxKind.DotToken);
                        }
                        N(SyntaxKind.IdentifierToken, "P10");
                        N(SyntaxKind.SemicolonToken);
                    }
                    N(SyntaxKind.CloseBraceToken);
                }
                N(SyntaxKind.EndOfFileToken);
            }
            EOF();
        }

        [Fact]
        public void CS0071_02()
        {
            UsingTree(@"
public interface I2 { }
public interface I1
{
    event System.Action I2.
P10;
}
");
            N(SyntaxKind.CompilationUnit);
            {
                N(SyntaxKind.InterfaceDeclaration);
                {
                    N(SyntaxKind.PublicKeyword);
                    N(SyntaxKind.InterfaceKeyword);
                    N(SyntaxKind.IdentifierToken, "I2");
                    N(SyntaxKind.OpenBraceToken);
                    N(SyntaxKind.CloseBraceToken);
                }
                N(SyntaxKind.InterfaceDeclaration);
                {
                    N(SyntaxKind.PublicKeyword);
                    N(SyntaxKind.InterfaceKeyword);
                    N(SyntaxKind.IdentifierToken, "I1");
                    N(SyntaxKind.OpenBraceToken);
                    N(SyntaxKind.EventDeclaration);
                    {
                        N(SyntaxKind.EventKeyword);
                        N(SyntaxKind.QualifiedName);
                        {
                            N(SyntaxKind.IdentifierName);
                            {
                                N(SyntaxKind.IdentifierToken, "System");
                            }
                            N(SyntaxKind.DotToken);
                            N(SyntaxKind.IdentifierName);
                            {
                                N(SyntaxKind.IdentifierToken, "Action");
                            }
                        }
                        N(SyntaxKind.ExplicitInterfaceSpecifier);
                        {
                            N(SyntaxKind.IdentifierName);
                            {
                                N(SyntaxKind.IdentifierToken, "I2");
                            }
                            N(SyntaxKind.DotToken);
                        }
                        N(SyntaxKind.IdentifierToken, "P10");
                        N(SyntaxKind.SemicolonToken);
                    }
                    N(SyntaxKind.CloseBraceToken);
                }
                N(SyntaxKind.EndOfFileToken);
            }
            EOF();
        }

        [Fact]
        public void CS0071_03()
        {
            UsingTree(@"
public interface I2 { }
public interface I1
{
    event System.Action I2.
P10
}
",
                // (5,27): error CS0071: An explicit interface implementation of an event must use event accessor syntax
                //     event System.Action I2.
                Diagnostic(ErrorCode.ERR_ExplicitEventFieldImpl, ".").WithLocation(5, 27),
                // (7,1): error CS1519: Invalid token '}' in class, record, struct, or interface member declaration
                // }
                Diagnostic(ErrorCode.ERR_InvalidMemberDecl, "}").WithArguments("}").WithLocation(7, 1));
            N(SyntaxKind.CompilationUnit);
            {
                N(SyntaxKind.InterfaceDeclaration);
                {
                    N(SyntaxKind.PublicKeyword);
                    N(SyntaxKind.InterfaceKeyword);
                    N(SyntaxKind.IdentifierToken, "I2");
                    N(SyntaxKind.OpenBraceToken);
                    N(SyntaxKind.CloseBraceToken);
                }
                N(SyntaxKind.InterfaceDeclaration);
                {
                    N(SyntaxKind.PublicKeyword);
                    N(SyntaxKind.InterfaceKeyword);
                    N(SyntaxKind.IdentifierToken, "I1");
                    N(SyntaxKind.OpenBraceToken);
                    N(SyntaxKind.EventDeclaration);
                    {
                        N(SyntaxKind.EventKeyword);
                        N(SyntaxKind.QualifiedName);
                        {
                            N(SyntaxKind.IdentifierName);
                            {
                                N(SyntaxKind.IdentifierToken, "System");
                            }
                            N(SyntaxKind.DotToken);
                            N(SyntaxKind.IdentifierName);
                            {
                                N(SyntaxKind.IdentifierToken, "Action");
                            }
                        }
                        N(SyntaxKind.ExplicitInterfaceSpecifier);
                        {
                            N(SyntaxKind.IdentifierName);
                            {
                                N(SyntaxKind.IdentifierToken, "I2");
                            }
                            N(SyntaxKind.DotToken);
                        }
                        M(SyntaxKind.IdentifierToken);
                        M(SyntaxKind.AccessorList);
                        {
                            M(SyntaxKind.OpenBraceToken);
                            M(SyntaxKind.CloseBraceToken);
                        }
                    }
                    N(SyntaxKind.IncompleteMember);
                    {
                        N(SyntaxKind.IdentifierName);
                        {
                            N(SyntaxKind.IdentifierToken, "P10");
                        }
                    }
                    N(SyntaxKind.CloseBraceToken);
                }
                N(SyntaxKind.EndOfFileToken);
            }
            EOF();
        }

        [Fact]
        public void CS0071_04()
        {
            UsingTree(@"
public interface I2 { }
public interface I1
{
    event System.Action I2.P10
}
",
                // (5,27): error CS0071: An explicit interface implementation of an event must use event accessor syntax
                //     event System.Action I2.P10
                Diagnostic(ErrorCode.ERR_ExplicitEventFieldImpl, ".").WithLocation(5, 27));
            N(SyntaxKind.CompilationUnit);
            {
                N(SyntaxKind.InterfaceDeclaration);
                {
                    N(SyntaxKind.PublicKeyword);
                    N(SyntaxKind.InterfaceKeyword);
                    N(SyntaxKind.IdentifierToken, "I2");
                    N(SyntaxKind.OpenBraceToken);
                    N(SyntaxKind.CloseBraceToken);
                }
                N(SyntaxKind.InterfaceDeclaration);
                {
                    N(SyntaxKind.PublicKeyword);
                    N(SyntaxKind.InterfaceKeyword);
                    N(SyntaxKind.IdentifierToken, "I1");
                    N(SyntaxKind.OpenBraceToken);
                    N(SyntaxKind.EventDeclaration);
                    {
                        N(SyntaxKind.EventKeyword);
                        N(SyntaxKind.QualifiedName);
                        {
                            N(SyntaxKind.IdentifierName);
                            {
                                N(SyntaxKind.IdentifierToken, "System");
                            }
                            N(SyntaxKind.DotToken);
                            N(SyntaxKind.IdentifierName);
                            {
                                N(SyntaxKind.IdentifierToken, "Action");
                            }
                        }
                        N(SyntaxKind.ExplicitInterfaceSpecifier);
                        {
                            N(SyntaxKind.IdentifierName);
                            {
                                N(SyntaxKind.IdentifierToken, "I2");
                            }
                            N(SyntaxKind.DotToken);
                        }
                        N(SyntaxKind.IdentifierToken, "P10");
                        M(SyntaxKind.AccessorList);
                        {
                            M(SyntaxKind.OpenBraceToken);
                            M(SyntaxKind.CloseBraceToken);
                        }
                    }
                    N(SyntaxKind.CloseBraceToken);
                }
                N(SyntaxKind.EndOfFileToken);
            }
            EOF();
        }

        [Fact]
        [WorkItem(4826, "https://github.com/dotnet/roslyn/pull/4826")]
        public void NonAccessorAfterIncompleteProperty()
        {
            UsingTree(@"
class C
{
    int A { get { return this.
    public int B;
}
",
                // (4,31): error CS1001: Identifier expected
                //     int A { get { return this.
                Diagnostic(ErrorCode.ERR_IdentifierExpected, "").WithLocation(4, 31),
                // (4,31): error CS1002: ; expected
                //     int A { get { return this.
                Diagnostic(ErrorCode.ERR_SemicolonExpected, "").WithLocation(4, 31),
                // (4,31): error CS1513: } expected
                //     int A { get { return this.
                Diagnostic(ErrorCode.ERR_RbraceExpected, "").WithLocation(4, 31),
                // (4,31): error CS1513: } expected
                //     int A { get { return this.
                Diagnostic(ErrorCode.ERR_RbraceExpected, "").WithLocation(4, 31));

            N(SyntaxKind.CompilationUnit);
            {
                N(SyntaxKind.ClassDeclaration);
                {
                    N(SyntaxKind.ClassKeyword);
                    N(SyntaxKind.IdentifierToken, "C");
                    N(SyntaxKind.OpenBraceToken);
                    N(SyntaxKind.PropertyDeclaration);
                    {
                        N(SyntaxKind.PredefinedType);
                        {
                            N(SyntaxKind.IntKeyword);
                        }
                        N(SyntaxKind.IdentifierToken, "A");
                        N(SyntaxKind.AccessorList);
                        {
                            N(SyntaxKind.OpenBraceToken);
                            N(SyntaxKind.GetAccessorDeclaration);
                            {
                                N(SyntaxKind.GetKeyword);
                                N(SyntaxKind.Block);
                                {
                                    N(SyntaxKind.OpenBraceToken);
                                    N(SyntaxKind.ReturnStatement);
                                    {
                                        N(SyntaxKind.ReturnKeyword);
                                        N(SyntaxKind.SimpleMemberAccessExpression);
                                        {
                                            N(SyntaxKind.ThisExpression);
                                            {
                                                N(SyntaxKind.ThisKeyword);
                                            }
                                            N(SyntaxKind.DotToken);
                                            M(SyntaxKind.IdentifierName);
                                            {
                                                M(SyntaxKind.IdentifierToken);
                                            }
                                        }
                                        M(SyntaxKind.SemicolonToken);
                                    }
                                    M(SyntaxKind.CloseBraceToken);
                                }
                            }
                            M(SyntaxKind.CloseBraceToken);
                        }
                    }
                    N(SyntaxKind.FieldDeclaration);
                    {
                        N(SyntaxKind.PublicKeyword);
                        N(SyntaxKind.VariableDeclaration);
                        {
                            N(SyntaxKind.PredefinedType);
                            {
                                N(SyntaxKind.IntKeyword);
                            }
                            N(SyntaxKind.VariableDeclarator);
                            {
                                N(SyntaxKind.IdentifierToken, "B");
                            }
                        }
                        N(SyntaxKind.SemicolonToken);
                    }
                    N(SyntaxKind.CloseBraceToken);
                }
                N(SyntaxKind.EndOfFileToken);
            }
            EOF();
        }

        [Fact]
        public void TupleArgument01()
        {
            var text = @"
class C1
{
    static (T, T) Test1<T>(int a, (byte, byte) arg0)
    {
        return default((T, T));
    }

    static (T, T) Test2<T>(ref (byte, byte) arg0)
    {
        return default((T, T));
    }
}
";
            var file = this.ParseFile(text);
            file.Errors().Length.Should().Be(0);
        }

        [Fact]
        public void TupleArgument02()
        {
            var text = @"
class C1
{
    static (T, T) Test3<T>((byte, byte) arg0)
    {
        return default((T, T));
    }

    (T, T) Test3<T>((byte a, byte b)[] arg0)
    {
        return default((T, T));
    }
}
";
            var file = this.ParseFile(text);
            file.Errors().Length.Should().Be(0);
        }

        [Fact]
        [WorkItem(13578, "https://github.com/dotnet/roslyn/issues/13578")]
        [CompilerTrait(CompilerFeature.ExpressionBody)]
        public void ExpressionBodiedCtorDtorProp()
        {
            UsingTree(@"
class C
{
    C() : base() => M();
    C() => M();
    ~C() => M();
    int P { set => M(); }
}
");

            N(SyntaxKind.CompilationUnit);
            {
                N(SyntaxKind.ClassDeclaration);
                {
                    N(SyntaxKind.ClassKeyword);
                    N(SyntaxKind.IdentifierToken);
                    N(SyntaxKind.OpenBraceToken);
                    N(SyntaxKind.ConstructorDeclaration);
                    {
                        N(SyntaxKind.IdentifierToken);
                        N(SyntaxKind.ParameterList);
                        {
                            N(SyntaxKind.OpenParenToken);
                            N(SyntaxKind.CloseParenToken);
                        }
                        N(SyntaxKind.BaseConstructorInitializer);
                        {
                            N(SyntaxKind.ColonToken);
                            N(SyntaxKind.BaseKeyword);
                            N(SyntaxKind.ArgumentList);
                            {
                                N(SyntaxKind.OpenParenToken);
                                N(SyntaxKind.CloseParenToken);
                            }
                        }
                        N(SyntaxKind.ArrowExpressionClause);
                        {
                            N(SyntaxKind.EqualsGreaterThanToken);
                            N(SyntaxKind.InvocationExpression);
                            {
                                N(SyntaxKind.IdentifierName);
                                {
                                    N(SyntaxKind.IdentifierToken);
                                }
                                N(SyntaxKind.ArgumentList);
                                {
                                    N(SyntaxKind.OpenParenToken);
                                    N(SyntaxKind.CloseParenToken);
                                }
                            }
                        }
                        N(SyntaxKind.SemicolonToken);
                    }
                    N(SyntaxKind.ConstructorDeclaration);
                    {
                        N(SyntaxKind.IdentifierToken);
                        N(SyntaxKind.ParameterList);
                        {
                            N(SyntaxKind.OpenParenToken);
                            N(SyntaxKind.CloseParenToken);
                        }
                        N(SyntaxKind.ArrowExpressionClause);
                        {
                            N(SyntaxKind.EqualsGreaterThanToken);
                            N(SyntaxKind.InvocationExpression);
                            {
                                N(SyntaxKind.IdentifierName);
                                {
                                    N(SyntaxKind.IdentifierToken);
                                }
                                N(SyntaxKind.ArgumentList);
                                {
                                    N(SyntaxKind.OpenParenToken);
                                    N(SyntaxKind.CloseParenToken);
                                }
                            }
                        }
                        N(SyntaxKind.SemicolonToken);
                    }
                    N(SyntaxKind.DestructorDeclaration);
                    {
                        N(SyntaxKind.TildeToken);
                        N(SyntaxKind.IdentifierToken);
                        N(SyntaxKind.ParameterList);
                        {
                            N(SyntaxKind.OpenParenToken);
                            N(SyntaxKind.CloseParenToken);
                        }
                        N(SyntaxKind.ArrowExpressionClause);
                        {
                            N(SyntaxKind.EqualsGreaterThanToken);
                            N(SyntaxKind.InvocationExpression);
                            {
                                N(SyntaxKind.IdentifierName);
                                {
                                    N(SyntaxKind.IdentifierToken);
                                }
                                N(SyntaxKind.ArgumentList);
                                {
                                    N(SyntaxKind.OpenParenToken);
                                    N(SyntaxKind.CloseParenToken);
                                }
                            }
                        }
                        N(SyntaxKind.SemicolonToken);
                    }
                    N(SyntaxKind.PropertyDeclaration);
                    {
                        N(SyntaxKind.PredefinedType);
                        {
                            N(SyntaxKind.IntKeyword);
                        }
                        N(SyntaxKind.IdentifierToken);
                        N(SyntaxKind.AccessorList);
                        {
                            N(SyntaxKind.OpenBraceToken);
                            N(SyntaxKind.SetAccessorDeclaration);
                            {
                                N(SyntaxKind.SetKeyword);
                                N(SyntaxKind.ArrowExpressionClause);
                                {
                                    N(SyntaxKind.EqualsGreaterThanToken);
                                    N(SyntaxKind.InvocationExpression);
                                    {
                                        N(SyntaxKind.IdentifierName);
                                        {
                                            N(SyntaxKind.IdentifierToken);
                                        }
                                        N(SyntaxKind.ArgumentList);
                                        {
                                            N(SyntaxKind.OpenParenToken);
                                            N(SyntaxKind.CloseParenToken);
                                        }
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
        }

        [Fact]
        public void ParseOutVar()
        {
            var tree = UsingTree(@"
class C
{
    void Goo()
    {
        M(out var x);
    }
}", options: TestOptions.Regular.WithTuplesFeature());
            N(SyntaxKind.CompilationUnit);
            {
                N(SyntaxKind.ClassDeclaration);
                {
                    N(SyntaxKind.ClassKeyword);
                    N(SyntaxKind.IdentifierToken, "C");
                    N(SyntaxKind.OpenBraceToken);
                    N(SyntaxKind.MethodDeclaration);
                    {
                        N(SyntaxKind.PredefinedType);
                        {
                            N(SyntaxKind.VoidKeyword);
                        }
                        N(SyntaxKind.IdentifierToken, "Goo");
                        N(SyntaxKind.ParameterList);
                        {
                            N(SyntaxKind.OpenParenToken);
                            N(SyntaxKind.CloseParenToken);
                        }
                        N(SyntaxKind.Block);
                        {
                            N(SyntaxKind.OpenBraceToken);
                            N(SyntaxKind.ExpressionStatement);
                            {
                                N(SyntaxKind.InvocationExpression);
                                {
                                    N(SyntaxKind.IdentifierName);
                                    {
                                        N(SyntaxKind.IdentifierToken, "M");
                                    }
                                    N(SyntaxKind.ArgumentList);
                                    {
                                        N(SyntaxKind.OpenParenToken);
                                        N(SyntaxKind.Argument);
                                        {
                                            N(SyntaxKind.OutKeyword);
                                            N(SyntaxKind.DeclarationExpression);
                                            {
                                                N(SyntaxKind.IdentifierName);
                                                {
                                                    N(SyntaxKind.IdentifierToken, "var");
                                                }
                                                N(SyntaxKind.SingleVariableDesignation);
                                                {
                                                    N(SyntaxKind.IdentifierToken, "x");
                                                }
                                            }
                                        }
                                        N(SyntaxKind.CloseParenToken);
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
        public void TestPartiallyWrittenConstraintClauseInBaseList1()
        {
            var tree = UsingTree(@"
class C<T> : where
",
                // (2,19): error CS1514: { expected
                // class C<T> : where
                Diagnostic(ErrorCode.ERR_LbraceExpected, "").WithLocation(2, 19),
                // (2,19): error CS1513: } expected
                // class C<T> : where
                Diagnostic(ErrorCode.ERR_RbraceExpected, "").WithLocation(2, 19));
            N(SyntaxKind.CompilationUnit);
            {
                N(SyntaxKind.ClassDeclaration);
                {
                    N(SyntaxKind.ClassKeyword);
                    N(SyntaxKind.IdentifierToken, "C");
                    N(SyntaxKind.TypeParameterList);
                    {
                        N(SyntaxKind.LessThanToken);
                        N(SyntaxKind.TypeParameter);
                        {
                            N(SyntaxKind.IdentifierToken, "T");
                        }
                        N(SyntaxKind.GreaterThanToken);
                    }
                    N(SyntaxKind.BaseList);
                    {
                        N(SyntaxKind.ColonToken);
                        N(SyntaxKind.SimpleBaseType);
                        {
                            N(SyntaxKind.IdentifierName);
                            {
                                N(SyntaxKind.IdentifierToken, "where");
                            }
                        }
                    }
                    M(SyntaxKind.OpenBraceToken);
                    M(SyntaxKind.CloseBraceToken);
                }
                N(SyntaxKind.EndOfFileToken);
            }
            EOF();
        }

        [Fact]
        public void TestPartiallyWrittenConstraintClauseInBaseList2()
        {
            var tree = UsingTree(@"
class C<T> : where T
",
                // (2,20): error CS1003: Syntax error, ',' expected
                // class C<T> : where T
                Diagnostic(ErrorCode.ERR_SyntaxError, "T").WithArguments(",").WithLocation(2, 20),
                // (2,21): error CS1514: { expected
                // class C<T> : where T
                Diagnostic(ErrorCode.ERR_LbraceExpected, "").WithLocation(2, 21),
                // (2,21): error CS1513: } expected
                // class C<T> : where T
                Diagnostic(ErrorCode.ERR_RbraceExpected, "").WithLocation(2, 21));
            N(SyntaxKind.CompilationUnit);
            {
                N(SyntaxKind.ClassDeclaration);
                {
                    N(SyntaxKind.ClassKeyword);
                    N(SyntaxKind.IdentifierToken, "C");
                    N(SyntaxKind.TypeParameterList);
                    {
                        N(SyntaxKind.LessThanToken);
                        N(SyntaxKind.TypeParameter);
                        {
                            N(SyntaxKind.IdentifierToken, "T");
                        }
                        N(SyntaxKind.GreaterThanToken);
                    }
                    N(SyntaxKind.BaseList);
                    {
                        N(SyntaxKind.ColonToken);
                        N(SyntaxKind.SimpleBaseType);
                        {
                            N(SyntaxKind.IdentifierName);
                            {
                                N(SyntaxKind.IdentifierToken, "where");
                            }
                        }
                        M(SyntaxKind.CommaToken);
                        N(SyntaxKind.SimpleBaseType);
                        {
                            N(SyntaxKind.IdentifierName);
                            {
                                N(SyntaxKind.IdentifierToken, "T");
                            }
                        }
                    }
                    M(SyntaxKind.OpenBraceToken);
                    M(SyntaxKind.CloseBraceToken);
                }
                N(SyntaxKind.EndOfFileToken);
            }
            EOF();
        }

        [Fact]
        public void TestPartiallyWrittenConstraintClauseInBaseList3()
        {
            var tree = UsingTree(@"
class C<T> : where T :
",
                // (2,14): error CS1031: Type expected
                // class C<T> : where T :
                Diagnostic(ErrorCode.ERR_TypeExpected, "where").WithLocation(2, 14),
                // (2,23): error CS1031: Type expected
                // class C<T> : where T :
                Diagnostic(ErrorCode.ERR_TypeExpected, "").WithLocation(2, 23),
                // (2,23): error CS1514: { expected
                // class C<T> : where T :
                Diagnostic(ErrorCode.ERR_LbraceExpected, "").WithLocation(2, 23),
                // (2,23): error CS1513: } expected
                // class C<T> : where T :
                Diagnostic(ErrorCode.ERR_RbraceExpected, "").WithLocation(2, 23));
            N(SyntaxKind.CompilationUnit);
            {
                N(SyntaxKind.ClassDeclaration);
                {
                    N(SyntaxKind.ClassKeyword);
                    N(SyntaxKind.IdentifierToken, "C");
                    N(SyntaxKind.TypeParameterList);
                    {
                        N(SyntaxKind.LessThanToken);
                        N(SyntaxKind.TypeParameter);
                        {
                            N(SyntaxKind.IdentifierToken, "T");
                        }
                        N(SyntaxKind.GreaterThanToken);
                    }
                    N(SyntaxKind.BaseList);
                    {
                        N(SyntaxKind.ColonToken);
                        M(SyntaxKind.SimpleBaseType);
                        {
                            M(SyntaxKind.IdentifierName);
                            {
                                M(SyntaxKind.IdentifierToken);
                            }
                        }
                    }
                    N(SyntaxKind.TypeParameterConstraintClause);
                    {
                        N(SyntaxKind.WhereKeyword);
                        N(SyntaxKind.IdentifierName);
                        {
                            N(SyntaxKind.IdentifierToken, "T");
                        }
                        N(SyntaxKind.ColonToken);
                        M(SyntaxKind.TypeConstraint);
                        {
                            M(SyntaxKind.IdentifierName);
                            {
                                M(SyntaxKind.IdentifierToken);
                            }
                        }
                    }
                    M(SyntaxKind.OpenBraceToken);
                    M(SyntaxKind.CloseBraceToken);
                }
                N(SyntaxKind.EndOfFileToken);
            }
            EOF();
        }

        [Fact]
        public void TestPartiallyWrittenConstraintClauseInBaseList4()
        {
            var tree = UsingTree(@"
class C<T> : where T : X
",
                // (2,14): error CS1031: Type expected
                // class C<T> : where T : X
                Diagnostic(ErrorCode.ERR_TypeExpected, "where").WithLocation(2, 14),
                // (2,25): error CS1514: { expected
                // class C<T> : where T : X
                Diagnostic(ErrorCode.ERR_LbraceExpected, "").WithLocation(2, 25),
                // (2,25): error CS1513: } expected
                // class C<T> : where T : X
                Diagnostic(ErrorCode.ERR_RbraceExpected, "").WithLocation(2, 25));
            N(SyntaxKind.CompilationUnit);
            {
                N(SyntaxKind.ClassDeclaration);
                {
                    N(SyntaxKind.ClassKeyword);
                    N(SyntaxKind.IdentifierToken, "C");
                    N(SyntaxKind.TypeParameterList);
                    {
                        N(SyntaxKind.LessThanToken);
                        N(SyntaxKind.TypeParameter);
                        {
                            N(SyntaxKind.IdentifierToken, "T");
                        }
                        N(SyntaxKind.GreaterThanToken);
                    }
                    N(SyntaxKind.BaseList);
                    {
                        N(SyntaxKind.ColonToken);
                        M(SyntaxKind.SimpleBaseType);
                        {
                            M(SyntaxKind.IdentifierName);
                            {
                                M(SyntaxKind.IdentifierToken);
                            }
                        }
                    }
                    N(SyntaxKind.TypeParameterConstraintClause);
                    {
                        N(SyntaxKind.WhereKeyword);
                        N(SyntaxKind.IdentifierName);
                        {
                            N(SyntaxKind.IdentifierToken, "T");
                        }
                        N(SyntaxKind.ColonToken);
                        N(SyntaxKind.TypeConstraint);
                        {
                            N(SyntaxKind.IdentifierName);
                            {
                                N(SyntaxKind.IdentifierToken, "X");
                            }
                        }
                    }
                    M(SyntaxKind.OpenBraceToken);
                    M(SyntaxKind.CloseBraceToken);
                }
                N(SyntaxKind.EndOfFileToken);
            }
            EOF();
        }

        [Fact]
        [WorkItem(23833, "https://github.com/dotnet/roslyn/issues/23833")]
        public void ProduceErrorsOnRef_Properties_Ref_Get()
        {
            var code = @"
class Program
{
    public int P
    {
        ref get => throw null;
    }
}";

            CreateCompilation(code).VerifyDiagnostics(
                // (6,13): error CS0106: The modifier 'ref' is not valid for this item
                //         ref get => throw null;
                Diagnostic(ErrorCode.ERR_BadMemberFlag, "get").WithArguments("ref").WithLocation(6, 13));
        }

        [Fact]
        [WorkItem(23833, "https://github.com/dotnet/roslyn/issues/23833")]
        public void ProduceErrorsOnRef_Properties_Ref_Get_SecondModifier()
        {
            var code = @"
class Program
{
    public int P
    {
        abstract ref get => throw null;
    }
}";

            CreateCompilation(code).VerifyDiagnostics(
                // (6,22): error CS0106: The modifier 'abstract' is not valid for this item
                //         abstract ref get => throw null;
                Diagnostic(ErrorCode.ERR_BadMemberFlag, "get").WithArguments("abstract").WithLocation(6, 22),
                // (6,22): error CS0106: The modifier 'ref' is not valid for this item
                //         abstract ref get => throw null;
                Diagnostic(ErrorCode.ERR_BadMemberFlag, "get").WithArguments("ref").WithLocation(6, 22));
        }

        [Fact]
        [WorkItem(23833, "https://github.com/dotnet/roslyn/issues/23833")]
        public void ProduceErrorsOnRef_Properties_Ref_Set()
        {
            var code = @"
class Program
{
    public int P
    {
        ref set => throw null;
    }
}";

            CreateCompilation(code).VerifyDiagnostics(
                // (6,13): error CS0106: The modifier 'ref' is not valid for this item
                //         ref set => throw null;
                Diagnostic(ErrorCode.ERR_BadMemberFlag, "set").WithArguments("ref").WithLocation(6, 13));
        }

        [Fact]
        [WorkItem(23833, "https://github.com/dotnet/roslyn/issues/23833")]
        public void ProduceErrorsOnRef_Properties_Ref_Set_SecondModifier()
        {
            var code = @"
class Program
{
    public int P
    {
        abstract ref set => throw null;
    }
}";

            CreateCompilation(code).VerifyDiagnostics(
                // (6,22): error CS0106: The modifier 'abstract' is not valid for this item
                //         abstract ref set => throw null;
                Diagnostic(ErrorCode.ERR_BadMemberFlag, "set").WithArguments("abstract").WithLocation(6, 22),
                // (6,22): error CS0106: The modifier 'ref' is not valid for this item
                //         abstract ref set => throw null;
                Diagnostic(ErrorCode.ERR_BadMemberFlag, "set").WithArguments("ref").WithLocation(6, 22));
        }

        [Fact]
        [WorkItem(23833, "https://github.com/dotnet/roslyn/issues/23833")]
        public void ProduceErrorsOnRef_Events_Ref()
        {
            var code = @"
public class Program
{
    event System.EventHandler E
    {
        ref add => throw null; 
        ref remove => throw null; 
    }
}";

            CreateCompilation(code).VerifyDiagnostics(
                // (6,9): error CS1609: Modifiers cannot be placed on event accessor declarations
                //         ref add => throw null; 
                Diagnostic(ErrorCode.ERR_NoModifiersOnAccessor, "ref").WithLocation(6, 9),
                // (7,9): error CS1609: Modifiers cannot be placed on event accessor declarations
                //         ref remove => throw null; 
                Diagnostic(ErrorCode.ERR_NoModifiersOnAccessor, "ref").WithLocation(7, 9));
        }

        [Fact]
        [WorkItem(23833, "https://github.com/dotnet/roslyn/issues/23833")]
        public void ProduceErrorsOnRef_Events_Ref_SecondModifier()
        {
            var code = @"
public class Program
{
    event System.EventHandler E
    {
        abstract ref add => throw null; 
        abstract ref remove => throw null; 
    }
}";

            CreateCompilation(code).VerifyDiagnostics(
                // (6,9): error CS1609: Modifiers cannot be placed on event accessor declarations
                //         abstract ref add => throw null; 
                Diagnostic(ErrorCode.ERR_NoModifiersOnAccessor, "abstract").WithLocation(6, 9),
                // (7,9): error CS1609: Modifiers cannot be placed on event accessor declarations
                //         abstract ref remove => throw null; 
                Diagnostic(ErrorCode.ERR_NoModifiersOnAccessor, "abstract").WithLocation(7, 9));
        }

        [Fact]
        public void NullableClassConstraint_01()
        {
            var tree = UsingNode(@"
class C<T> where T : class {}
");

            N(SyntaxKind.CompilationUnit);
            {
                N(SyntaxKind.ClassDeclaration);
                {
                    N(SyntaxKind.ClassKeyword);
                    N(SyntaxKind.IdentifierToken, "C");
                    N(SyntaxKind.TypeParameterList);
                    {
                        N(SyntaxKind.LessThanToken);
                        N(SyntaxKind.TypeParameter);
                        {
                            N(SyntaxKind.IdentifierToken, "T");
                        }
                        N(SyntaxKind.GreaterThanToken);
                    }
                    N(SyntaxKind.TypeParameterConstraintClause);
                    {
                        N(SyntaxKind.WhereKeyword);
                        N(SyntaxKind.IdentifierName);
                        {
                            N(SyntaxKind.IdentifierToken, "T");
                        }
                        N(SyntaxKind.ColonToken);
                        N(SyntaxKind.ClassConstraint);
                        {
                            N(SyntaxKind.ClassKeyword);
                        }
                    }
                    N(SyntaxKind.OpenBraceToken);
                    N(SyntaxKind.CloseBraceToken);
                }
                N(SyntaxKind.EndOfFileToken);
            }
            EOF();
        }

        [Fact]
        public void NullableClassConstraint_02()
        {
            var tree = UsingNode(@"
class C<T> where T : struct {}
");

            N(SyntaxKind.CompilationUnit);
            {
                N(SyntaxKind.ClassDeclaration);
                {
                    N(SyntaxKind.ClassKeyword);
                    N(SyntaxKind.IdentifierToken, "C");
                    N(SyntaxKind.TypeParameterList);
                    {
                        N(SyntaxKind.LessThanToken);
                        N(SyntaxKind.TypeParameter);
                        {
                            N(SyntaxKind.IdentifierToken, "T");
                        }
                        N(SyntaxKind.GreaterThanToken);
                    }
                    N(SyntaxKind.TypeParameterConstraintClause);
                    {
                        N(SyntaxKind.WhereKeyword);
                        N(SyntaxKind.IdentifierName);
                        {
                            N(SyntaxKind.IdentifierToken, "T");
                        }
                        N(SyntaxKind.ColonToken);
                        N(SyntaxKind.StructConstraint);
                        {
                            N(SyntaxKind.StructKeyword);
                        }
                    }
                    N(SyntaxKind.OpenBraceToken);
                    N(SyntaxKind.CloseBraceToken);
                }
                N(SyntaxKind.EndOfFileToken);
            }
            EOF();
        }

        [Fact]
        public void NullableClassConstraint_03()
        {
            var tree = UsingNode(@"
class C<T> where T : class? {}
");

            N(SyntaxKind.CompilationUnit);
            {
                N(SyntaxKind.ClassDeclaration);
                {
                    N(SyntaxKind.ClassKeyword);
                    N(SyntaxKind.IdentifierToken, "C");
                    N(SyntaxKind.TypeParameterList);
                    {
                        N(SyntaxKind.LessThanToken);
                        N(SyntaxKind.TypeParameter);
                        {
                            N(SyntaxKind.IdentifierToken, "T");
                        }
                        N(SyntaxKind.GreaterThanToken);
                    }
                    N(SyntaxKind.TypeParameterConstraintClause);
                    {
                        N(SyntaxKind.WhereKeyword);
                        N(SyntaxKind.IdentifierName);
                        {
                            N(SyntaxKind.IdentifierToken, "T");
                        }
                        N(SyntaxKind.ColonToken);
                        N(SyntaxKind.ClassConstraint);
                        {
                            N(SyntaxKind.ClassKeyword);
                            N(SyntaxKind.QuestionToken);
                        }
                    }
                    N(SyntaxKind.OpenBraceToken);
                    N(SyntaxKind.CloseBraceToken);
                }
                N(SyntaxKind.EndOfFileToken);
            }
            EOF();
        }

        [Fact]
        public void NullableClassConstraint_04()
        {
            var tree = UsingNode(@"
class C<T> where T : struct? {}
", TestOptions.Regular,
                // (2,28): error CS1073: Unexpected token '?'
                // class C<T> where T : struct? {}
                Diagnostic(ErrorCode.ERR_UnexpectedToken, "?").WithArguments("?").WithLocation(2, 28)
);

            N(SyntaxKind.CompilationUnit);
            {
                N(SyntaxKind.ClassDeclaration);
                {
                    N(SyntaxKind.ClassKeyword);
                    N(SyntaxKind.IdentifierToken, "C");
                    N(SyntaxKind.TypeParameterList);
                    {
                        N(SyntaxKind.LessThanToken);
                        N(SyntaxKind.TypeParameter);
                        {
                            N(SyntaxKind.IdentifierToken, "T");
                        }
                        N(SyntaxKind.GreaterThanToken);
                    }
                    N(SyntaxKind.TypeParameterConstraintClause);
                    {
                        N(SyntaxKind.WhereKeyword);
                        N(SyntaxKind.IdentifierName);
                        {
                            N(SyntaxKind.IdentifierToken, "T");
                        }
                        N(SyntaxKind.ColonToken);
                        N(SyntaxKind.StructConstraint);
                        {
                            N(SyntaxKind.StructKeyword);
                            N(SyntaxKind.QuestionToken);
                        }
                    }
                    N(SyntaxKind.OpenBraceToken);
                    N(SyntaxKind.CloseBraceToken);
                }
                N(SyntaxKind.EndOfFileToken);
            }
            EOF();
        }

        [Fact]
        public void NullableClassConstraint_05()
        {
            var tree = UsingNode(@"
class C<T> where T : class? {}
", TestOptions.Regular7_3);

            N(SyntaxKind.CompilationUnit);
            {
                N(SyntaxKind.ClassDeclaration);
                {
                    N(SyntaxKind.ClassKeyword);
                    N(SyntaxKind.IdentifierToken, "C");
                    N(SyntaxKind.TypeParameterList);
                    {
                        N(SyntaxKind.LessThanToken);
                        N(SyntaxKind.TypeParameter);
                        {
                            N(SyntaxKind.IdentifierToken, "T");
                        }
                        N(SyntaxKind.GreaterThanToken);
                    }
                    N(SyntaxKind.TypeParameterConstraintClause);
                    {
                        N(SyntaxKind.WhereKeyword);
                        N(SyntaxKind.IdentifierName);
                        {
                            N(SyntaxKind.IdentifierToken, "T");
                        }
                        N(SyntaxKind.ColonToken);
                        N(SyntaxKind.ClassConstraint);
                        {
                            N(SyntaxKind.ClassKeyword);
                            N(SyntaxKind.QuestionToken);
                        }
                    }
                    N(SyntaxKind.OpenBraceToken);
                    N(SyntaxKind.CloseBraceToken);
                }
                N(SyntaxKind.EndOfFileToken);
            }
            EOF();
        }

        [Fact]
        public void NullableClassConstraint_06()
        {
            var tree = UsingNode(@"
class C<T> where T : struct? {}
", TestOptions.Regular7_3,
                // (2,28): error CS1073: Unexpected token '?'
                // class C<T> where T : struct? {}
                Diagnostic(ErrorCode.ERR_UnexpectedToken, "?").WithArguments("?").WithLocation(2, 28)
);

            N(SyntaxKind.CompilationUnit);
            {
                N(SyntaxKind.ClassDeclaration);
                {
                    N(SyntaxKind.ClassKeyword);
                    N(SyntaxKind.IdentifierToken, "C");
                    N(SyntaxKind.TypeParameterList);
                    {
                        N(SyntaxKind.LessThanToken);
                        N(SyntaxKind.TypeParameter);
                        {
                            N(SyntaxKind.IdentifierToken, "T");
                        }
                        N(SyntaxKind.GreaterThanToken);
                    }
                    N(SyntaxKind.TypeParameterConstraintClause);
                    {
                        N(SyntaxKind.WhereKeyword);
                        N(SyntaxKind.IdentifierName);
                        {
                            N(SyntaxKind.IdentifierToken, "T");
                        }
                        N(SyntaxKind.ColonToken);
                        N(SyntaxKind.StructConstraint);
                        {
                            N(SyntaxKind.StructKeyword);
                            N(SyntaxKind.QuestionToken);
                        }
                    }
                    N(SyntaxKind.OpenBraceToken);
                    N(SyntaxKind.CloseBraceToken);
                }
                N(SyntaxKind.EndOfFileToken);
            }
            EOF();
        }

        [Fact]
        public void TestMethodDeclarationNullValidation()
        {
            UsingStatement(@"void M(string name!!) { }", options: TestOptions.RegularPreview,
                // (1,19): error CS8989: The 'parameter null-checking' feature is not supported.
                // void M(string name!!) { }
                Diagnostic(ErrorCode.ERR_ParameterNullCheckingNotSupported, "!").WithLocation(1, 19));
            N(SyntaxKind.LocalFunctionStatement);
            {
                N(SyntaxKind.PredefinedType);
                {
                    N(SyntaxKind.VoidKeyword);
                }
                N(SyntaxKind.IdentifierToken, "M");
                N(SyntaxKind.ParameterList);
                {
                    N(SyntaxKind.OpenParenToken);
                    N(SyntaxKind.Parameter);
                    {
                        N(SyntaxKind.PredefinedType);
                        {
                            N(SyntaxKind.StringKeyword);
                        }
                        N(SyntaxKind.IdentifierToken, "name");
                    }
                    N(SyntaxKind.CloseParenToken);
                }
                N(SyntaxKind.Block);
                {
                    N(SyntaxKind.OpenBraceToken);
                    N(SyntaxKind.CloseBraceToken);
                }
            }
        }

        [Fact]
        public void TestMethodDeclarationNullValidation_SingleExclamation()
        {
            UsingStatement(@"void M(string name!) { }", options: TestOptions.RegularPreview,
                // (1,19): error CS8989: The 'parameter null-checking' feature is not supported.
                // void M(string name!) { }
                Diagnostic(ErrorCode.ERR_ParameterNullCheckingNotSupported, "!").WithLocation(1, 19));

            N(SyntaxKind.LocalFunctionStatement);
            {
                N(SyntaxKind.PredefinedType);
                {
                    N(SyntaxKind.VoidKeyword);
                }
                N(SyntaxKind.IdentifierToken, "M");
                N(SyntaxKind.ParameterList);
                {
                    N(SyntaxKind.OpenParenToken);
                    N(SyntaxKind.Parameter);
                    {
                        N(SyntaxKind.PredefinedType);
                        {
                            N(SyntaxKind.StringKeyword);
                        }
                        N(SyntaxKind.IdentifierToken, "name");
                    }
                    N(SyntaxKind.CloseParenToken);
                }
                N(SyntaxKind.Block);
                {
                    N(SyntaxKind.OpenBraceToken);
                    N(SyntaxKind.CloseBraceToken);
                }
            }
            EOF();
        }

        [Fact]
        public void TestMethodDeclarationNullValidation_SingleExclamation_ExtraTrivia()
        {
            UsingStatement(@"void M(string name
                /*comment1*/!/*comment2*/) { }", options: TestOptions.RegularPreview,
                // (2,29): error CS8989: The 'parameter null-checking' feature is not supported.
                //                 /*comment1*/!/*comment2*/) { }
                Diagnostic(ErrorCode.ERR_ParameterNullCheckingNotSupported, "!").WithLocation(2, 29));

            N(SyntaxKind.LocalFunctionStatement);
            {
                N(SyntaxKind.PredefinedType);
                {
                    N(SyntaxKind.VoidKeyword);
                }
                N(SyntaxKind.IdentifierToken, "M");
                N(SyntaxKind.ParameterList);
                {
                    N(SyntaxKind.OpenParenToken);
                    N(SyntaxKind.Parameter);
                    {
                        N(SyntaxKind.PredefinedType);
                        {
                            N(SyntaxKind.StringKeyword);
                        }
                        N(SyntaxKind.IdentifierToken, "name");
                    }
                    N(SyntaxKind.CloseParenToken);
                }
                N(SyntaxKind.Block);
                {
                    N(SyntaxKind.OpenBraceToken);
                    N(SyntaxKind.CloseBraceToken);
                }
            }
            EOF();
        }

        [Fact]
        public void TestOptParamMethodDeclarationWithNullValidation()
        {
            UsingStatement(@"void M(string name!! = null) { }", options: TestOptions.RegularPreview,
                // (1,19): error CS8989: The 'parameter null-checking' feature is not supported.
                // void M(string name!! = null) { }
                Diagnostic(ErrorCode.ERR_ParameterNullCheckingNotSupported, "!").WithLocation(1, 19));
            N(SyntaxKind.LocalFunctionStatement);
            {
                N(SyntaxKind.PredefinedType);
                {
                    N(SyntaxKind.VoidKeyword);
                }
                N(SyntaxKind.IdentifierToken, "M");
                N(SyntaxKind.ParameterList);
                {
                    N(SyntaxKind.OpenParenToken);
                    N(SyntaxKind.Parameter);
                    {
                        N(SyntaxKind.PredefinedType);
                        {
                            N(SyntaxKind.StringKeyword);
                        }
                        N(SyntaxKind.IdentifierToken, "name");
                        N(SyntaxKind.EqualsValueClause);
                        {
                            N(SyntaxKind.EqualsToken);
                            N(SyntaxKind.NullLiteralExpression);
                            {
                                N(SyntaxKind.NullKeyword);
                            }
                        }
                    }
                    N(SyntaxKind.CloseParenToken);
                }
                N(SyntaxKind.Block);
                {
                    N(SyntaxKind.OpenBraceToken);
                    N(SyntaxKind.CloseBraceToken);
                }
            }
        }

        [Fact]
        public void TestOptParamMethodDeclarationWithNullValidationNoSpaces()
        {
            UsingStatement(@"void M(string name!!=null) { }", options: TestOptions.RegularPreview,
                // (1,19): error CS8989: The 'parameter null-checking' feature is not supported.
                // void M(string name!!=null) { }
                Diagnostic(ErrorCode.ERR_ParameterNullCheckingNotSupported, "!").WithLocation(1, 19));
            N(SyntaxKind.LocalFunctionStatement);
            {
                N(SyntaxKind.PredefinedType);
                {
                    N(SyntaxKind.VoidKeyword);
                }
                N(SyntaxKind.IdentifierToken, "M");
                N(SyntaxKind.ParameterList);
                {
                    N(SyntaxKind.OpenParenToken);
                    N(SyntaxKind.Parameter);
                    {
                        N(SyntaxKind.PredefinedType);
                        {
                            N(SyntaxKind.StringKeyword);
                        }
                        N(SyntaxKind.IdentifierToken, "name");
                        N(SyntaxKind.EqualsValueClause);
                        {
                            N(SyntaxKind.EqualsToken);
                            N(SyntaxKind.NullLiteralExpression);
                            {
                                N(SyntaxKind.NullKeyword);
                            }
                        }
                    }
                    N(SyntaxKind.CloseParenToken);
                }
                N(SyntaxKind.Block);
                {
                    N(SyntaxKind.OpenBraceToken);
                    N(SyntaxKind.CloseBraceToken);
                }
            }
        }

        [Fact]
        public void TestNullCheckedArgList1()
        {
            UsingStatement(@"void M(__arglist!) { }", options: TestOptions.RegularPreview,
                    // (1,17): error CS1003: Syntax error, ',' expected
                    // void M(__arglist!) { }
                    Diagnostic(ErrorCode.ERR_SyntaxError, "!").WithArguments(",").WithLocation(1, 17));
            N(SyntaxKind.LocalFunctionStatement);
            {
                N(SyntaxKind.PredefinedType);
                {
                    N(SyntaxKind.VoidKeyword);
                }
                N(SyntaxKind.IdentifierToken, "M");
                N(SyntaxKind.ParameterList);
                {
                    N(SyntaxKind.OpenParenToken);
                    N(SyntaxKind.Parameter);
                    {
                        N(SyntaxKind.ArgListKeyword);
                    }
                    N(SyntaxKind.CloseParenToken);
                }
                N(SyntaxKind.Block);
                {
                    N(SyntaxKind.OpenBraceToken);
                    N(SyntaxKind.CloseBraceToken);
                }
            }
            EOF();
        }

        [Fact]
        public void TestNullCheckedArgList2()
        {
            UsingStatement(@"void M(__arglist!!) { }", options: TestOptions.RegularPreview,
                    // (1,17): error CS1003: Syntax error, ',' expected
                    // void M(__arglist!!) { }
                    Diagnostic(ErrorCode.ERR_SyntaxError, "!").WithArguments(",").WithLocation(1, 17));
            N(SyntaxKind.LocalFunctionStatement);
            {
                N(SyntaxKind.PredefinedType);
                {
                    N(SyntaxKind.VoidKeyword);
                }
                N(SyntaxKind.IdentifierToken, "M");
                N(SyntaxKind.ParameterList);
                {
                    N(SyntaxKind.OpenParenToken);
                    N(SyntaxKind.Parameter);
                    {
                        N(SyntaxKind.ArgListKeyword);
                    }
                    N(SyntaxKind.CloseParenToken);
                }
                N(SyntaxKind.Block);
                {
                    N(SyntaxKind.OpenBraceToken);
                    N(SyntaxKind.CloseBraceToken);
                }
            }
            EOF();
        }

        [Fact]
        public void TestNullCheckedArgList3()
        {
            UsingStatement(@"void M(__arglist!! = null) { }", options: TestOptions.RegularPreview,
                    // (1,17): error CS1003: Syntax error, ',' expected
                    // void M(__arglist!! = null) { }
                    Diagnostic(ErrorCode.ERR_SyntaxError, "!").WithArguments(",").WithLocation(1, 17));
            N(SyntaxKind.LocalFunctionStatement);
            {
                N(SyntaxKind.PredefinedType);
                {
                    N(SyntaxKind.VoidKeyword);
                }
                N(SyntaxKind.IdentifierToken, "M");
                N(SyntaxKind.ParameterList);
                {
                    N(SyntaxKind.OpenParenToken);
                    N(SyntaxKind.Parameter);
                    {
                        N(SyntaxKind.ArgListKeyword);
                    }
                    N(SyntaxKind.CloseParenToken);
                }
                N(SyntaxKind.Block);
                {
                    N(SyntaxKind.OpenBraceToken);
                    N(SyntaxKind.CloseBraceToken);
                }
            }
            EOF();
        }

        [Fact]
        public void TestNullCheckedArgList4()
        {
            UsingStatement(@"void M(__arglist!!= null) { }", options: TestOptions.RegularPreview,
                    // (1,17): error CS1003: Syntax error, ',' expected
                    // void M(__arglist!!= null) { }
                    Diagnostic(ErrorCode.ERR_SyntaxError, "!").WithArguments(",").WithLocation(1, 17));
            N(SyntaxKind.LocalFunctionStatement);
            {
                N(SyntaxKind.PredefinedType);
                {
                    N(SyntaxKind.VoidKeyword);
                }
                N(SyntaxKind.IdentifierToken, "M");
                N(SyntaxKind.ParameterList);
                {
                    N(SyntaxKind.OpenParenToken);
                    N(SyntaxKind.Parameter);
                    {
                        N(SyntaxKind.ArgListKeyword);
                    }
                    N(SyntaxKind.CloseParenToken);
                }
                N(SyntaxKind.Block);
                {
                    N(SyntaxKind.OpenBraceToken);
                    N(SyntaxKind.CloseBraceToken);
                }
            }
            EOF();
        }

        [Fact]
        public void TestNullCheckedArgList5()
        {
            UsingStatement(@"void M(__arglist[]!!= null) { }", options: TestOptions.RegularPreview,
                // (1,17): error CS1003: Syntax error, ',' expected
                // void M(__arglist[]!!= null) { }
                Diagnostic(ErrorCode.ERR_SyntaxError, "[").WithArguments(",").WithLocation(1, 17),
                // (1,18): error CS1001: Identifier expected
                // void M(__arglist[]!!= null) { }
                Diagnostic(ErrorCode.ERR_IdentifierExpected, "]").WithLocation(1, 18),
                // (1,19): error CS1031: Type expected
                // void M(__arglist[]!!= null) { }
                Diagnostic(ErrorCode.ERR_TypeExpected, "!").WithLocation(1, 19),
                // (1,19): error CS1001: Identifier expected
                // void M(__arglist[]!!= null) { }
                Diagnostic(ErrorCode.ERR_IdentifierExpected, "!").WithLocation(1, 19),
                // (1,19): error CS8989: The 'parameter null-checking' feature is not supported.
                // void M(__arglist[]!!= null) { }
                Diagnostic(ErrorCode.ERR_ParameterNullCheckingNotSupported, "!").WithLocation(1, 19));
            N(SyntaxKind.LocalFunctionStatement);
            {
                N(SyntaxKind.PredefinedType);
                {
                    N(SyntaxKind.VoidKeyword);
                }
                N(SyntaxKind.IdentifierToken, "M");
                N(SyntaxKind.ParameterList);
                {
                    N(SyntaxKind.OpenParenToken);
                    N(SyntaxKind.Parameter);
                    {
                        N(SyntaxKind.ArgListKeyword);
                    }
                    M(SyntaxKind.CommaToken);
                    N(SyntaxKind.Parameter);
                    {
                        N(SyntaxKind.AttributeList);
                        {
                            N(SyntaxKind.OpenBracketToken);
                            M(SyntaxKind.Attribute);
                            {
                                M(SyntaxKind.IdentifierName);
                                {
                                    M(SyntaxKind.IdentifierToken);
                                }
                            }
                            N(SyntaxKind.CloseBracketToken);
                        }
                        M(SyntaxKind.IdentifierName);
                        {
                            M(SyntaxKind.IdentifierToken);
                        }
                        M(SyntaxKind.IdentifierToken);
                        N(SyntaxKind.EqualsValueClause);
                        {
                            N(SyntaxKind.EqualsToken);
                            N(SyntaxKind.NullLiteralExpression);
                            {
                                N(SyntaxKind.NullKeyword);
                            }
                        }
                    }
                    N(SyntaxKind.CloseParenToken);
                }
                N(SyntaxKind.Block);
                {
                    N(SyntaxKind.OpenBraceToken);
                    N(SyntaxKind.CloseBraceToken);
                }
            }
            EOF();
        }

        [Fact]
        public void TestArgListWithBrackets()
        {
            UsingStatement(@"void M(__arglist[]) { }", options: TestOptions.RegularPreview,
                    // (1,17): error CS1003: Syntax error, ',' expected
                    // void M(__arglist[]) { }
                    Diagnostic(ErrorCode.ERR_SyntaxError, "[").WithArguments(",").WithLocation(1, 17),
                    // (1,18): error CS1001: Identifier expected
                    // void M(__arglist[]) { }
                    Diagnostic(ErrorCode.ERR_IdentifierExpected, "]").WithLocation(1, 18),
                    // (1,19): error CS1031: Type expected
                    // void M(__arglist[]) { }
                    Diagnostic(ErrorCode.ERR_TypeExpected, ")").WithLocation(1, 19),
                    // (1,19): error CS1001: Identifier expected
                    // void M(__arglist[]) { }
                    Diagnostic(ErrorCode.ERR_IdentifierExpected, ")").WithLocation(1, 19));
            N(SyntaxKind.LocalFunctionStatement);
            {
                N(SyntaxKind.PredefinedType);
                {
                    N(SyntaxKind.VoidKeyword);
                }
                N(SyntaxKind.IdentifierToken, "M");
                N(SyntaxKind.ParameterList);
                {
                    N(SyntaxKind.OpenParenToken);
                    N(SyntaxKind.Parameter);
                    {
                        N(SyntaxKind.ArgListKeyword);
                    }
                    M(SyntaxKind.CommaToken);
                    N(SyntaxKind.Parameter);
                    {
                        N(SyntaxKind.AttributeList);
                        {
                            N(SyntaxKind.OpenBracketToken);
                            M(SyntaxKind.Attribute);
                            {
                                M(SyntaxKind.IdentifierName);
                                {
                                    M(SyntaxKind.IdentifierToken);
                                }
                            }
                            N(SyntaxKind.CloseBracketToken);
                        }
                        M(SyntaxKind.IdentifierName);
                        {
                            M(SyntaxKind.IdentifierToken);
                        }
                        M(SyntaxKind.IdentifierToken);
                    }
                    N(SyntaxKind.CloseParenToken);
                }
                N(SyntaxKind.Block);
                {
                    N(SyntaxKind.OpenBraceToken);
                    N(SyntaxKind.CloseBraceToken);
                }
            }
            EOF();
        }

        [Fact]
        public void TestArgListWithDefaultValue()
        {
            UsingStatement(@"void M(__arglist = null) { }", options: TestOptions.RegularPreview,
                    // (1,18): error CS1003: Syntax error, ',' expected
                    // void M(__arglist = null) { }
                    Diagnostic(ErrorCode.ERR_SyntaxError, "=").WithArguments(",").WithLocation(1, 18));
            N(SyntaxKind.LocalFunctionStatement);
            {
                N(SyntaxKind.PredefinedType);
                {
                    N(SyntaxKind.VoidKeyword);
                }
                N(SyntaxKind.IdentifierToken, "M");
                N(SyntaxKind.ParameterList);
                {
                    N(SyntaxKind.OpenParenToken);
                    N(SyntaxKind.Parameter);
                    {
                        N(SyntaxKind.ArgListKeyword);
                    }
                    N(SyntaxKind.CloseParenToken);
                }
                N(SyntaxKind.Block);
                {
                    N(SyntaxKind.OpenBraceToken);
                    N(SyntaxKind.CloseBraceToken);
                }
            }
            EOF();
        }

        [Fact]
        public void TestNullCheckedArgWithLeadingSpace()
        {
            UsingStatement(@"void M(string name !!=null) { }", options: TestOptions.RegularPreview,
                // (1,20): error CS8989: The 'parameter null-checking' feature is not supported.
                // void M(string name !!=null) { }
                Diagnostic(ErrorCode.ERR_ParameterNullCheckingNotSupported, "!").WithLocation(1, 20));
            N(SyntaxKind.LocalFunctionStatement);
            {
                N(SyntaxKind.PredefinedType);
                {
                    N(SyntaxKind.VoidKeyword);
                }
                N(SyntaxKind.IdentifierToken, "M");
                N(SyntaxKind.ParameterList);
                {
                    N(SyntaxKind.OpenParenToken);
                    N(SyntaxKind.Parameter);
                    {
                        N(SyntaxKind.PredefinedType);
                        {
                            N(SyntaxKind.StringKeyword);
                        }
                        N(SyntaxKind.IdentifierToken, "name");
                        N(SyntaxKind.EqualsValueClause);
                        {
                            N(SyntaxKind.EqualsToken);
                            N(SyntaxKind.NullLiteralExpression);
                            {
                                N(SyntaxKind.NullKeyword);
                            }
                        }
                    }
                    N(SyntaxKind.CloseParenToken);
                }
                N(SyntaxKind.Block);
                {
                    N(SyntaxKind.OpenBraceToken);
                    N(SyntaxKind.CloseBraceToken);
                }
            }
        }

        [Fact]
        public void TestNullCheckedArgWithLeadingNewLine()
        {
            UsingStatement(@"void M(string name!!=null) { }", options: TestOptions.RegularPreview,
                // (1,19): error CS8989: The 'parameter null-checking' feature is not supported.
                // void M(string name!!=null) { }
                Diagnostic(ErrorCode.ERR_ParameterNullCheckingNotSupported, "!").WithLocation(1, 19));
            N(SyntaxKind.LocalFunctionStatement);
            {
                N(SyntaxKind.PredefinedType);
                {
                    N(SyntaxKind.VoidKeyword);
                }
                N(SyntaxKind.IdentifierToken, "M");
                N(SyntaxKind.ParameterList);
                {
                    N(SyntaxKind.OpenParenToken);
                    N(SyntaxKind.Parameter);
                    {
                        N(SyntaxKind.PredefinedType);
                        {
                            N(SyntaxKind.StringKeyword);
                        }
                        N(SyntaxKind.IdentifierToken, "name");
                        N(SyntaxKind.EqualsValueClause);
                        {
                            N(SyntaxKind.EqualsToken);
                            N(SyntaxKind.NullLiteralExpression);
                            {
                                N(SyntaxKind.NullKeyword);
                            }
                        }
                    }
                    N(SyntaxKind.CloseParenToken);
                }
                N(SyntaxKind.Block);
                {
                    N(SyntaxKind.OpenBraceToken);
                    N(SyntaxKind.CloseBraceToken);
                }
            }
        }

        [Fact]
        public void TestNullCheckedArgWithTrailingSpace()
        {
            UsingStatement(@"void M(string name!!= null) { }", options: TestOptions.RegularPreview,
                // (1,19): error CS8989: The 'parameter null-checking' feature is not supported.
                // void M(string name!!= null) { }
                Diagnostic(ErrorCode.ERR_ParameterNullCheckingNotSupported, "!").WithLocation(1, 19));
            N(SyntaxKind.LocalFunctionStatement);
            {
                N(SyntaxKind.PredefinedType);
                {
                    N(SyntaxKind.VoidKeyword);
                }
                N(SyntaxKind.IdentifierToken, "M");
                N(SyntaxKind.ParameterList);
                {
                    N(SyntaxKind.OpenParenToken);
                    N(SyntaxKind.Parameter);
                    {
                        N(SyntaxKind.PredefinedType);
                        {
                            N(SyntaxKind.StringKeyword);
                        }
                        N(SyntaxKind.IdentifierToken, "name");
                        N(SyntaxKind.EqualsValueClause);
                        {
                            N(SyntaxKind.EqualsToken);
                            N(SyntaxKind.NullLiteralExpression);
                            {
                                N(SyntaxKind.NullKeyword);
                            }
                        }
                    }
                    N(SyntaxKind.CloseParenToken);
                }
                N(SyntaxKind.Block);
                {
                    N(SyntaxKind.OpenBraceToken);
                    N(SyntaxKind.CloseBraceToken);
                }
            }
        }

        [Fact]
        public void TestNullCheckedArgWithTrailingNewLine()
        {
            UsingStatement(@"void M(string name!!=null) { }", options: TestOptions.RegularPreview,
                // (1,19): error CS8989: The 'parameter null-checking' feature is not supported.
                // void M(string name!!=null) { }
                Diagnostic(ErrorCode.ERR_ParameterNullCheckingNotSupported, "!").WithLocation(1, 19));
            N(SyntaxKind.LocalFunctionStatement);
            {
                N(SyntaxKind.PredefinedType);
                {
                    N(SyntaxKind.VoidKeyword);
                }
                N(SyntaxKind.IdentifierToken, "M");
                N(SyntaxKind.ParameterList);
                {
                    N(SyntaxKind.OpenParenToken);
                    N(SyntaxKind.Parameter);
                    {
                        N(SyntaxKind.PredefinedType);
                        {
                            N(SyntaxKind.StringKeyword);
                        }
                        N(SyntaxKind.IdentifierToken, "name");
                        N(SyntaxKind.EqualsValueClause);
                        {
                            N(SyntaxKind.EqualsToken);
                            N(SyntaxKind.NullLiteralExpression);
                            {
                                N(SyntaxKind.NullKeyword);
                            }
                        }
                    }
                    N(SyntaxKind.CloseParenToken);
                }
                N(SyntaxKind.Block);
                {
                    N(SyntaxKind.OpenBraceToken);
                    N(SyntaxKind.CloseBraceToken);
                }
            }
        }

        [Fact]
        public void TestNullCheckedArgWithSpaceInbetween()
        {
            UsingStatement(@"void M(string name! !=null) { }", options: TestOptions.RegularPreview,
                // (1,19): error CS8989: The 'parameter null-checking' feature is not supported.
                // void M(string name! !=null) { }
                Diagnostic(ErrorCode.ERR_ParameterNullCheckingNotSupported, "!").WithLocation(1, 19));
            N(SyntaxKind.LocalFunctionStatement);
            {
                N(SyntaxKind.PredefinedType);
                {
                    N(SyntaxKind.VoidKeyword);
                }
                N(SyntaxKind.IdentifierToken, "M");
                N(SyntaxKind.ParameterList);
                {
                    N(SyntaxKind.OpenParenToken);
                    N(SyntaxKind.Parameter);
                    {
                        N(SyntaxKind.PredefinedType);
                        {
                            N(SyntaxKind.StringKeyword);
                        }
                        N(SyntaxKind.IdentifierToken, "name");
                        N(SyntaxKind.EqualsValueClause);
                        {
                            N(SyntaxKind.EqualsToken);
                            N(SyntaxKind.NullLiteralExpression);
                            {
                                N(SyntaxKind.NullKeyword);
                            }
                        }
                    }
                    N(SyntaxKind.CloseParenToken);
                }
                N(SyntaxKind.Block);
                {
                    N(SyntaxKind.OpenBraceToken);
                    N(SyntaxKind.CloseBraceToken);
                }
            }
            EOF();
        }

        [Fact]
        public void TestNullCheckedArgWithSpaceAfterParam()
        {
            UsingStatement(@"void M(string name !!=null) { }", options: TestOptions.RegularPreview,
                // (1,20): error CS8989: The 'parameter null-checking' feature is not supported.
                // void M(string name !!=null) { }
                Diagnostic(ErrorCode.ERR_ParameterNullCheckingNotSupported, "!").WithLocation(1, 20));
            N(SyntaxKind.LocalFunctionStatement);
            {
                N(SyntaxKind.PredefinedType);
                {
                    N(SyntaxKind.VoidKeyword);
                }
                N(SyntaxKind.IdentifierToken, "M");
                N(SyntaxKind.ParameterList);
                {
                    N(SyntaxKind.OpenParenToken);
                    N(SyntaxKind.Parameter);
                    {
                        N(SyntaxKind.PredefinedType);
                        {
                            N(SyntaxKind.StringKeyword);
                        }
                        N(SyntaxKind.IdentifierToken, "name");
                        N(SyntaxKind.EqualsValueClause);
                        {
                            N(SyntaxKind.EqualsToken);
                            N(SyntaxKind.NullLiteralExpression);
                            {
                                N(SyntaxKind.NullKeyword);
                            }
                        }
                    }
                    N(SyntaxKind.CloseParenToken);
                }
                N(SyntaxKind.Block);
                {
                    N(SyntaxKind.OpenBraceToken);
                    N(SyntaxKind.CloseBraceToken);
                }
            }
        }

        [Fact]
        public void TestNullCheckedArgWithSpaceAfterBangs()
        {
            UsingStatement(@"void M(string name! ! =null) { }", options: TestOptions.RegularPreview,
                // (1,19): error CS8989: The 'parameter null-checking' feature is not supported.
                // void M(string name! ! =null) { }
                Diagnostic(ErrorCode.ERR_ParameterNullCheckingNotSupported, "!").WithLocation(1, 19));
            N(SyntaxKind.LocalFunctionStatement);
            {
                N(SyntaxKind.PredefinedType);
                {
                    N(SyntaxKind.VoidKeyword);
                }
                N(SyntaxKind.IdentifierToken, "M");
                N(SyntaxKind.ParameterList);
                {
                    N(SyntaxKind.OpenParenToken);
                    N(SyntaxKind.Parameter);
                    {
                        N(SyntaxKind.PredefinedType);
                        {
                            N(SyntaxKind.StringKeyword);
                        }
                        N(SyntaxKind.IdentifierToken, "name");
                        N(SyntaxKind.EqualsValueClause);
                        {
                            N(SyntaxKind.EqualsToken);
                            N(SyntaxKind.NullLiteralExpression);
                            {
                                N(SyntaxKind.NullKeyword);
                            }
                        }
                    }
                    N(SyntaxKind.CloseParenToken);
                }
                N(SyntaxKind.Block);
                {
                    N(SyntaxKind.OpenBraceToken);
                    N(SyntaxKind.CloseBraceToken);
                }
            }
            EOF();
        }

        [Fact]
        public void TestNullCheckedArgWithSpaceBeforeBangs()
        {
            UsingStatement(@"void M(string name ! !=null) { }", options: TestOptions.RegularPreview,
                // (1,20): error CS8989: The 'parameter null-checking' feature is not supported.
                // void M(string name ! !=null) { }
                Diagnostic(ErrorCode.ERR_ParameterNullCheckingNotSupported, "!").WithLocation(1, 20));
            N(SyntaxKind.LocalFunctionStatement);
            {
                N(SyntaxKind.PredefinedType);
                {
                    N(SyntaxKind.VoidKeyword);
                }
                N(SyntaxKind.IdentifierToken, "M");
                N(SyntaxKind.ParameterList);
                {
                    N(SyntaxKind.OpenParenToken);
                    N(SyntaxKind.Parameter);
                    {
                        N(SyntaxKind.PredefinedType);
                        {
                            N(SyntaxKind.StringKeyword);
                        }
                        N(SyntaxKind.IdentifierToken, "name");
                        N(SyntaxKind.EqualsValueClause);
                        {
                            N(SyntaxKind.EqualsToken);
                            N(SyntaxKind.NullLiteralExpression);
                            {
                                N(SyntaxKind.NullKeyword);
                            }
                        }
                    }
                    N(SyntaxKind.CloseParenToken);
                }
                N(SyntaxKind.Block);
                {
                    N(SyntaxKind.OpenBraceToken);
                    N(SyntaxKind.CloseBraceToken);
                }
            }
            EOF();
        }

        [Fact]
        public void TestNullCheckedArgWithSpaceAfterEquals()
        {
            UsingStatement(@"void M(string name!!= null) { }", options: TestOptions.RegularPreview,
                // (1,19): error CS8989: The 'parameter null-checking' feature is not supported.
                // void M(string name!!= null) { }
                Diagnostic(ErrorCode.ERR_ParameterNullCheckingNotSupported, "!").WithLocation(1, 19));
            N(SyntaxKind.LocalFunctionStatement);
            {
                N(SyntaxKind.PredefinedType);
                {
                    N(SyntaxKind.VoidKeyword);
                }
                N(SyntaxKind.IdentifierToken, "M");
                N(SyntaxKind.ParameterList);
                {
                    N(SyntaxKind.OpenParenToken);
                    N(SyntaxKind.Parameter);
                    {
                        N(SyntaxKind.PredefinedType);
                        {
                            N(SyntaxKind.StringKeyword);
                        }
                        N(SyntaxKind.IdentifierToken, "name");
                        N(SyntaxKind.EqualsValueClause);
                        {
                            N(SyntaxKind.EqualsToken);
                            N(SyntaxKind.NullLiteralExpression);
                            {
                                N(SyntaxKind.NullKeyword);
                            }
                        }
                    }
                    N(SyntaxKind.CloseParenToken);
                }
                N(SyntaxKind.Block);
                {
                    N(SyntaxKind.OpenBraceToken);
                    N(SyntaxKind.CloseBraceToken);
                }
            }
        }

        [Fact]
        public void TestMethodDeclarationNullValidation_ExtraEquals()
        {
            UsingStatement(@"void M(string name!!= = null) { }", options: TestOptions.RegularPreview,
                // (1,19): error CS8989: The 'parameter null-checking' feature is not supported.
                // void M(string name!!= = null) { }
                Diagnostic(ErrorCode.ERR_ParameterNullCheckingNotSupported, "!").WithLocation(1, 19),
                // (1,23): error CS1525: Invalid expression term '='
                // void M(string name!!= = null) { }
                Diagnostic(ErrorCode.ERR_InvalidExprTerm, "=").WithArguments("=").WithLocation(1, 23));

            N(SyntaxKind.LocalFunctionStatement);
            {
                N(SyntaxKind.PredefinedType);
                {
                    N(SyntaxKind.VoidKeyword);
                }
                N(SyntaxKind.IdentifierToken, "M");
                N(SyntaxKind.ParameterList);
                {
                    N(SyntaxKind.OpenParenToken);
                    N(SyntaxKind.Parameter);
                    {
                        N(SyntaxKind.PredefinedType);
                        {
                            N(SyntaxKind.StringKeyword);
                        }
                        N(SyntaxKind.IdentifierToken, "name");
                        N(SyntaxKind.EqualsValueClause);
                        {
                            N(SyntaxKind.EqualsToken);
                            N(SyntaxKind.SimpleAssignmentExpression);
                            {
                                M(SyntaxKind.IdentifierName);
                                {
                                    M(SyntaxKind.IdentifierToken);
                                }
                                N(SyntaxKind.EqualsToken);
                                N(SyntaxKind.NullLiteralExpression);
                                {
                                    N(SyntaxKind.NullKeyword);
                                }
                            }
                        }
                    }
                    N(SyntaxKind.CloseParenToken);
                }
                N(SyntaxKind.Block);
                {
                    N(SyntaxKind.OpenBraceToken);
                    N(SyntaxKind.CloseBraceToken);
                }
            }
            EOF();
        }

        [Fact]
        public void TestNullCheckedMethod()
        {
            UsingTree(@"
class C
{
    public void M(string x!!) { }
}", options: TestOptions.RegularPreview,
                // (4,27): error CS8989: The 'parameter null-checking' feature is not supported.
                //     public void M(string x!!) { }
                Diagnostic(ErrorCode.ERR_ParameterNullCheckingNotSupported, "!").WithLocation(4, 27));
            N(SyntaxKind.CompilationUnit);
            {
                N(SyntaxKind.ClassDeclaration);
                {
                    N(SyntaxKind.ClassKeyword);
                    N(SyntaxKind.IdentifierToken, "C");
                    N(SyntaxKind.OpenBraceToken);
                    N(SyntaxKind.MethodDeclaration);
                    {
                        N(SyntaxKind.PublicKeyword);
                        N(SyntaxKind.PredefinedType);
                        {
                            N(SyntaxKind.VoidKeyword);
                        }
                        N(SyntaxKind.IdentifierToken, "M");
                        N(SyntaxKind.ParameterList);
                        {
                            N(SyntaxKind.OpenParenToken);
                            N(SyntaxKind.Parameter);
                            {
                                N(SyntaxKind.PredefinedType);
                                {
                                    N(SyntaxKind.StringKeyword);
                                }
                                N(SyntaxKind.IdentifierToken, "x");
                            }
                            N(SyntaxKind.CloseParenToken);
                        }
                        N(SyntaxKind.Block);
                        {
                            N(SyntaxKind.OpenBraceToken);
                            N(SyntaxKind.CloseBraceToken);
                        }
                    }
                    N(SyntaxKind.CloseBraceToken);
                }
                N(SyntaxKind.EndOfFileToken);
            }
        }

        [Fact]
        public void TestNullCheckedConstructor()
        {
            UsingTree(@"
class C
{
    public C(string x!!) { }
}", options: TestOptions.RegularPreview,
                // (4,22): error CS8989: The 'parameter null-checking' feature is not supported.
                //     public C(string x!!) { }
                Diagnostic(ErrorCode.ERR_ParameterNullCheckingNotSupported, "!").WithLocation(4, 22));
            N(SyntaxKind.CompilationUnit);
            {
                N(SyntaxKind.ClassDeclaration);
                {
                    N(SyntaxKind.ClassKeyword);
                    N(SyntaxKind.IdentifierToken, "C");
                    N(SyntaxKind.OpenBraceToken);
                    N(SyntaxKind.ConstructorDeclaration);
                    {
                        N(SyntaxKind.PublicKeyword);
                        N(SyntaxKind.IdentifierToken, "C");
                        N(SyntaxKind.ParameterList);
                        {
                            N(SyntaxKind.OpenParenToken);
                            N(SyntaxKind.Parameter);
                            {
                                N(SyntaxKind.PredefinedType);
                                {
                                    N(SyntaxKind.StringKeyword);
                                }
                                N(SyntaxKind.IdentifierToken, "x");
                            }
                            N(SyntaxKind.CloseParenToken);
                        }
                        N(SyntaxKind.Block);
                        {
                            N(SyntaxKind.OpenBraceToken);
                            N(SyntaxKind.CloseBraceToken);
                        }
                    }
                    N(SyntaxKind.CloseBraceToken);
                }
                N(SyntaxKind.EndOfFileToken);
            }
        }

        [Fact]
        public void TestNullCheckedOperator()
        {
            UsingTree(@"
class Box
{
    public static int operator+ (Box b!!, Box c) 
    {
        return 2;
    }
}", options: TestOptions.RegularPreview,
                // (4,39): error CS8989: The 'parameter null-checking' feature is not supported.
                //     public static int operator+ (Box b!!, Box c) 
                Diagnostic(ErrorCode.ERR_ParameterNullCheckingNotSupported, "!").WithLocation(4, 39));

            N(SyntaxKind.CompilationUnit);
            {
                N(SyntaxKind.ClassDeclaration);
                {
                    N(SyntaxKind.ClassKeyword);
                    N(SyntaxKind.IdentifierToken);
                    N(SyntaxKind.OpenBraceToken);
                    N(SyntaxKind.OperatorDeclaration);
                    {
                        N(SyntaxKind.PublicKeyword);
                        N(SyntaxKind.StaticKeyword);
                        N(SyntaxKind.PredefinedType);
                        {
                            N(SyntaxKind.IntKeyword);
                        }
                        N(SyntaxKind.OperatorKeyword);
                        N(SyntaxKind.PlusToken);
                        N(SyntaxKind.ParameterList);
                        {
                            N(SyntaxKind.OpenParenToken);
                            N(SyntaxKind.Parameter);
                            {
                                N(SyntaxKind.IdentifierName);
                                {
                                    N(SyntaxKind.IdentifierToken);
                                }
                                N(SyntaxKind.IdentifierToken);
                            }
                            N(SyntaxKind.CommaToken);
                            N(SyntaxKind.Parameter);
                            {
                                N(SyntaxKind.IdentifierName);
                                {
                                    N(SyntaxKind.IdentifierToken);
                                }
                                N(SyntaxKind.IdentifierToken);
                            }
                            N(SyntaxKind.CloseParenToken);
                            N(SyntaxKind.Block);
                            {
                                N(SyntaxKind.OpenBraceToken);
                                N(SyntaxKind.ReturnStatement);
                                {
                                    N(SyntaxKind.ReturnKeyword);
                                    N(SyntaxKind.NumericLiteralExpression);
                                    {
                                        N(SyntaxKind.NumericLiteralToken);
                                    }
                                }
                                N(SyntaxKind.SemicolonToken);
                                N(SyntaxKind.CloseBraceToken);
                            }
                        }
                    }
                    N(SyntaxKind.CloseBraceToken);
                }
            }
            N(SyntaxKind.EndOfFileToken);
        }

        [Fact]
        public void TestAnonymousDelegateNullChecking()
        {
            UsingTree(@"
delegate void Del(int x!!);
Del d = delegate(int k!!) { /* ... */ };", options: TestOptions.RegularPreview,
                // (2,24): error CS8989: The 'parameter null-checking' feature is not supported.
                // delegate void Del(int x!!);
                Diagnostic(ErrorCode.ERR_ParameterNullCheckingNotSupported, "!").WithLocation(2, 24),
                // (3,1): error CS8803: Top-level statements must precede namespace and type declarations.
                // Del d = delegate(int k!!) { /* ... */ };
                Diagnostic(ErrorCode.ERR_TopLevelStatementAfterNamespaceOrType, "Del d = delegate(int k!!) { /* ... */ };").WithLocation(3, 1),
                // (3,23): error CS8989: The 'parameter null-checking' feature is not supported.
                // Del d = delegate(int k!!) { /* ... */ };
                Diagnostic(ErrorCode.ERR_ParameterNullCheckingNotSupported, "!").WithLocation(3, 23));
            N(SyntaxKind.CompilationUnit);
            {
                N(SyntaxKind.DelegateDeclaration);
                {
                    N(SyntaxKind.DelegateKeyword);
                    N(SyntaxKind.PredefinedType);
                    {
                        N(SyntaxKind.VoidKeyword);
                    }
                    N(SyntaxKind.IdentifierToken, "Del");
                    N(SyntaxKind.ParameterList);
                    {
                        N(SyntaxKind.OpenParenToken);
                        N(SyntaxKind.Parameter);
                        {
                            N(SyntaxKind.PredefinedType);
                            {
                                N(SyntaxKind.IntKeyword);
                            }
                            N(SyntaxKind.IdentifierToken, "x");
                        }
                        N(SyntaxKind.CloseParenToken);
                    }
                    N(SyntaxKind.SemicolonToken);
                }
                N(SyntaxKind.GlobalStatement);
                {
                    N(SyntaxKind.LocalDeclarationStatement);
                    {
                        N(SyntaxKind.VariableDeclaration);
                        {
                            N(SyntaxKind.IdentifierName);
                            {
                                N(SyntaxKind.IdentifierToken, "Del");
                            }
                            N(SyntaxKind.VariableDeclarator);
                            {
                                N(SyntaxKind.IdentifierToken, "d");
                                N(SyntaxKind.EqualsValueClause);
                                {
                                    N(SyntaxKind.EqualsToken);
                                    N(SyntaxKind.AnonymousMethodExpression);
                                    {
                                        N(SyntaxKind.DelegateKeyword);
                                        N(SyntaxKind.ParameterList);
                                        {
                                            N(SyntaxKind.OpenParenToken);
                                            N(SyntaxKind.Parameter);
                                            {
                                                N(SyntaxKind.PredefinedType);
                                                {
                                                    N(SyntaxKind.IntKeyword);
                                                }
                                                N(SyntaxKind.IdentifierToken, "k");
                                            }
                                            N(SyntaxKind.CloseParenToken);
                                        }
                                        N(SyntaxKind.Block);
                                        {
                                            N(SyntaxKind.OpenBraceToken);
                                            N(SyntaxKind.CloseBraceToken);
                                        }
                                    }
                                }
                            }
                        }
                        N(SyntaxKind.SemicolonToken);
                    }
                }
                N(SyntaxKind.EndOfFileToken);
            }
        }

        [Fact, WorkItem(30102, "https://github.com/dotnet/roslyn/issues/30102")]
        public void IncompleteGenericInBaseList1()
        {
            var tree = UsingNode(@"
class B : A<int
{
}
", TestOptions.Regular7_3,
                // (2,16): error CS1003: Syntax error, '>' expected
                // class B : A<int
                Diagnostic(ErrorCode.ERR_SyntaxError, "").WithArguments(">").WithLocation(2, 16));

            N(SyntaxKind.CompilationUnit);
            {
                N(SyntaxKind.ClassDeclaration);
                {
                    N(SyntaxKind.ClassKeyword);
                    N(SyntaxKind.IdentifierToken, "B");
                    N(SyntaxKind.BaseList);
                    {
                        N(SyntaxKind.ColonToken);
                        N(SyntaxKind.SimpleBaseType);
                        {
                            N(SyntaxKind.GenericName);
                            {
                                N(SyntaxKind.IdentifierToken, "A");
                                N(SyntaxKind.TypeArgumentList);
                                {
                                    N(SyntaxKind.LessThanToken);
                                    N(SyntaxKind.PredefinedType);
                                    {
                                        N(SyntaxKind.IntKeyword);
                                    }
                                    M(SyntaxKind.GreaterThanToken);
                                }
                            }
                        }
                    }
                    N(SyntaxKind.OpenBraceToken);
                    N(SyntaxKind.CloseBraceToken);
                }
                N(SyntaxKind.EndOfFileToken);
            }
            EOF();
        }

        [Fact, WorkItem(35236, "https://github.com/dotnet/roslyn/issues/35236")]
        public void TestNamespaceWithDotDot1()
        {
            var text = @"namespace a..b { }";
            var tree = UsingNode(
                text, TestOptions.Regular7_3,
                // (1,13): error CS1001: Identifier expected
                // namespace a..b { }
                Diagnostic(ErrorCode.ERR_IdentifierExpected, ".").WithLocation(1, 13));

            // verify that we can roundtrip
            tree.ToFullString().Should().Be(text);

            N(SyntaxKind.CompilationUnit);
            {
                N(SyntaxKind.NamespaceDeclaration);
                {
                    N(SyntaxKind.NamespaceKeyword);
                    N(SyntaxKind.QualifiedName);
                    {
                        N(SyntaxKind.QualifiedName);
                        {
                            N(SyntaxKind.IdentifierName);
                            {
                                N(SyntaxKind.IdentifierToken, "a");
                            }
                            N(SyntaxKind.DotToken);
                            M(SyntaxKind.IdentifierName);
                            {
                                M(SyntaxKind.IdentifierToken);
                            }
                        }
                        N(SyntaxKind.DotToken);
                        N(SyntaxKind.IdentifierName);
                        {
                            N(SyntaxKind.IdentifierToken, "b");
                        }
                    }
                    N(SyntaxKind.OpenBraceToken);
                    N(SyntaxKind.CloseBraceToken);
                }
                N(SyntaxKind.EndOfFileToken);
            }
            EOF();
        }

        [Fact, WorkItem(30102, "https://github.com/dotnet/roslyn/issues/30102")]
        public void IncompleteGenericInBaseList2()
        {
            var tree = UsingNode(@"
class B<X, Y> : A<int
    where X : Y
{
}
", TestOptions.Regular7_3,
                // (2,22): error CS1003: Syntax error, '>' expected
                // class B<X, Y> : A<int
                Diagnostic(ErrorCode.ERR_SyntaxError, "").WithArguments(">").WithLocation(2, 22));

            N(SyntaxKind.CompilationUnit);
            {
                N(SyntaxKind.ClassDeclaration);
                {
                    N(SyntaxKind.ClassKeyword);
                    N(SyntaxKind.IdentifierToken, "B");
                    N(SyntaxKind.TypeParameterList);
                    {
                        N(SyntaxKind.LessThanToken);
                        N(SyntaxKind.TypeParameter);
                        {
                            N(SyntaxKind.IdentifierToken, "X");
                        }
                        N(SyntaxKind.CommaToken);
                        N(SyntaxKind.TypeParameter);
                        {
                            N(SyntaxKind.IdentifierToken, "Y");
                        }
                        N(SyntaxKind.GreaterThanToken);
                    }
                    N(SyntaxKind.BaseList);
                    {
                        N(SyntaxKind.ColonToken);
                        N(SyntaxKind.SimpleBaseType);
                        {
                            N(SyntaxKind.GenericName);
                            {
                                N(SyntaxKind.IdentifierToken, "A");
                                N(SyntaxKind.TypeArgumentList);
                                {
                                    N(SyntaxKind.LessThanToken);
                                    N(SyntaxKind.PredefinedType);
                                    {
                                        N(SyntaxKind.IntKeyword);
                                    }
                                    M(SyntaxKind.GreaterThanToken);
                                }
                            }
                        }
                    }
                    N(SyntaxKind.TypeParameterConstraintClause);
                    {
                        N(SyntaxKind.WhereKeyword);
                        N(SyntaxKind.IdentifierName);
                        {
                            N(SyntaxKind.IdentifierToken, "X");
                        }
                        N(SyntaxKind.ColonToken);
                        N(SyntaxKind.TypeConstraint);
                        {
                            N(SyntaxKind.IdentifierName);
                            {
                                N(SyntaxKind.IdentifierToken, "Y");
                            }
                        }
                    }
                    N(SyntaxKind.OpenBraceToken);
                    N(SyntaxKind.CloseBraceToken);
                }
                N(SyntaxKind.EndOfFileToken);
            }
            EOF();
        }

        [Fact, WorkItem(30102, "https://github.com/dotnet/roslyn/issues/30102")]
        public void TestExtraneousColonInBaseList()
        {
            var text = @"
class A : B : C
{
}
";
            CreateCompilation(text, parseOptions: TestOptions.Regular7_3).VerifyDiagnostics(
                // (2,11): error CS0246: The type or namespace name 'B' could not be found (are you missing a using directive or an assembly reference?)
                // class A : B : C
                Diagnostic(ErrorCode.ERR_SingleTypeNameNotFound, "B").WithArguments("B").WithLocation(2, 11),
                // (2,13): error CS1514: { expected
                // class A : B : C
                Diagnostic(ErrorCode.ERR_LbraceExpected, ":").WithLocation(2, 13),
                // (2,13): error CS1513: } expected
                // class A : B : C
                Diagnostic(ErrorCode.ERR_RbraceExpected, ":").WithLocation(2, 13),
                // (2,13): error CS1022: Type or namespace definition, or end-of-file expected
                // class A : B : C
                Diagnostic(ErrorCode.ERR_EOFExpected, ":").WithLocation(2, 13),
                // (2,15): error CS8803: Top-level statements must precede namespace and type declarations.
                // class A : B : C
                Diagnostic(ErrorCode.ERR_TopLevelStatementAfterNamespaceOrType, @"C
{
").WithLocation(2, 15),
                // (2,15): error CS8370: Feature 'top-level statements' is not available in C# 7.3. Please use language version 9.0 or greater.
                // class A : B : C
                Diagnostic(ErrorCode.ERR_FeatureNotAvailableInVersion7_3, @"C
{
").WithArguments("top-level statements", "9.0").WithLocation(2, 15),
                // (2,15): error CS0246: The type or namespace name 'C' could not be found (are you missing a using directive or an assembly reference?)
                // class A : B : C
                Diagnostic(ErrorCode.ERR_SingleTypeNameNotFound, "C").WithArguments("C").WithLocation(2, 15),
                // (2,16): error CS1001: Identifier expected
                // class A : B : C
                Diagnostic(ErrorCode.ERR_IdentifierExpected, "").WithLocation(2, 16),
                // (2,16): error CS1003: Syntax error, ',' expected
                // class A : B : C
                Diagnostic(ErrorCode.ERR_SyntaxError, "").WithArguments(",").WithLocation(2, 16),
                // (3,2): error CS1002: ; expected
                // {
                Diagnostic(ErrorCode.ERR_SemicolonExpected, "").WithLocation(3, 2),
                // (4,1): error CS1022: Type or namespace definition, or end-of-file expected
                // }
                Diagnostic(ErrorCode.ERR_EOFExpected, "}").WithLocation(4, 1));

            var tree = UsingNode(text, TestOptions.Regular7_3,
                // (2,13): error CS1514: { expected
                // class A : B : C
                Diagnostic(ErrorCode.ERR_LbraceExpected, ":").WithLocation(2, 13),
                // (2,13): error CS1513: } expected
                // class A : B : C
                Diagnostic(ErrorCode.ERR_RbraceExpected, ":").WithLocation(2, 13),
                // (2,13): error CS1022: Type or namespace definition, or end-of-file expected
                // class A : B : C
                Diagnostic(ErrorCode.ERR_EOFExpected, ":").WithLocation(2, 13),
                // (2,15): error CS8803: Top-level statements must precede namespace and type declarations.
                // class A : B : C
                Diagnostic(ErrorCode.ERR_TopLevelStatementAfterNamespaceOrType, @"C
{
").WithLocation(2, 15),
                // (2,16): error CS1001: Identifier expected
                // class A : B : C
                Diagnostic(ErrorCode.ERR_IdentifierExpected, "").WithLocation(2, 16),
                // (2,16): error CS1003: Syntax error, ',' expected
                // class A : B : C
                Diagnostic(ErrorCode.ERR_SyntaxError, "").WithArguments(",").WithLocation(2, 16),
                // (3,2): error CS1002: ; expected
                // {
                Diagnostic(ErrorCode.ERR_SemicolonExpected, "").WithLocation(3, 2),
                // (4,1): error CS1022: Type or namespace definition, or end-of-file expected
                // }
                Diagnostic(ErrorCode.ERR_EOFExpected, "}").WithLocation(4, 1));

            N(SyntaxKind.CompilationUnit);
            {
                N(SyntaxKind.ClassDeclaration);
                {
                    N(SyntaxKind.ClassKeyword);
                    N(SyntaxKind.IdentifierToken, "A");
                    N(SyntaxKind.BaseList);
                    {
                        N(SyntaxKind.ColonToken);
                        N(SyntaxKind.SimpleBaseType);
                        {
                            N(SyntaxKind.IdentifierName);
                            {
                                N(SyntaxKind.IdentifierToken, "B");
                            }
                        }
                    }
                    M(SyntaxKind.OpenBraceToken);
                    M(SyntaxKind.CloseBraceToken);
                }
                N(SyntaxKind.GlobalStatement);
                {
                    N(SyntaxKind.LocalDeclarationStatement);
                    {
                        N(SyntaxKind.VariableDeclaration);
                        {
                            N(SyntaxKind.IdentifierName);
                            {
                                N(SyntaxKind.IdentifierToken, "C");
                            }
                            M(SyntaxKind.VariableDeclarator);
                            {
                                M(SyntaxKind.IdentifierToken);
                            }
                        }
                        M(SyntaxKind.SemicolonToken);
                    }
                }
                N(SyntaxKind.EndOfFileToken);
            }
            EOF();
        }

        [Fact, WorkItem(35236, "https://github.com/dotnet/roslyn/issues/35236")]
        public void TestNamespaceWithDotDot2()
        {
            var text = @"namespace a
                    ..b { }";

            var tree = UsingNode(
                text, TestOptions.Regular7_3,
                // (2,22): error CS1001: Identifier expected
                //                     ..b { }
                Diagnostic(ErrorCode.ERR_IdentifierExpected, ".").WithLocation(2, 22));

            // verify that we can roundtrip
            tree.ToFullString().Should().Be(text);

            N(SyntaxKind.CompilationUnit);
            {
                N(SyntaxKind.NamespaceDeclaration);
                {
                    N(SyntaxKind.NamespaceKeyword);
                    N(SyntaxKind.QualifiedName);
                    {
                        N(SyntaxKind.QualifiedName);
                        {
                            N(SyntaxKind.IdentifierName);
                            {
                                N(SyntaxKind.IdentifierToken, "a");
                            }
                            N(SyntaxKind.DotToken);
                            M(SyntaxKind.IdentifierName);
                            {
                                M(SyntaxKind.IdentifierToken);
                            }
                        }
                        N(SyntaxKind.DotToken);
                        N(SyntaxKind.IdentifierName);
                        {
                            N(SyntaxKind.IdentifierToken, "b");
                        }
                    }
                    N(SyntaxKind.OpenBraceToken);
                    N(SyntaxKind.CloseBraceToken);
                }
                N(SyntaxKind.EndOfFileToken);
            }
            EOF();
        }

        [Fact, WorkItem(35236, "https://github.com/dotnet/roslyn/issues/35236")]
        public void TestNamespaceWithDotDot3()
        {
            var text = @"namespace a..
b { }";
            var tree = UsingNode(
                text, TestOptions.Regular7_3,
                // (1,13): error CS1001: Identifier expected
                // namespace a..
                Diagnostic(ErrorCode.ERR_IdentifierExpected, ".").WithLocation(1, 13));

            // verify that we can roundtrip
            tree.ToFullString().Should().Be(text);

            N(SyntaxKind.CompilationUnit);
            {
                N(SyntaxKind.NamespaceDeclaration);
                {
                    N(SyntaxKind.NamespaceKeyword);
                    N(SyntaxKind.QualifiedName);
                    {
                        N(SyntaxKind.QualifiedName);
                        {
                            N(SyntaxKind.IdentifierName);
                            {
                                N(SyntaxKind.IdentifierToken, "a");
                            }
                            N(SyntaxKind.DotToken);
                            M(SyntaxKind.IdentifierName);
                            {
                                M(SyntaxKind.IdentifierToken);
                            }
                        }
                        N(SyntaxKind.DotToken);
                        N(SyntaxKind.IdentifierName);
                        {
                            N(SyntaxKind.IdentifierToken, "b");
                        }
                    }
                    N(SyntaxKind.OpenBraceToken);
                    N(SyntaxKind.CloseBraceToken);
                }
                N(SyntaxKind.EndOfFileToken);
            }
            EOF();
        }

        [Fact, WorkItem(35236, "https://github.com/dotnet/roslyn/issues/35236")]
        public void TestNamespaceWithDotDot4()
        {
            var text = @"namespace a
                    ..
b { }";
            var tree = UsingNode(
                text, TestOptions.Regular7_3,
                // (2,22): error CS1001: Identifier expected
                //                     ..
                Diagnostic(ErrorCode.ERR_IdentifierExpected, ".").WithLocation(2, 22));

            // verify that we can roundtrip
            tree.ToFullString().Should().Be(text);
            N(SyntaxKind.CompilationUnit);
            {
                N(SyntaxKind.NamespaceDeclaration);
                {
                    N(SyntaxKind.NamespaceKeyword);
                    N(SyntaxKind.QualifiedName);
                    {
                        N(SyntaxKind.QualifiedName);
                        {
                            N(SyntaxKind.IdentifierName);
                            {
                                N(SyntaxKind.IdentifierToken, "a");
                            }
                            N(SyntaxKind.DotToken);
                            M(SyntaxKind.IdentifierName);
                            {
                                M(SyntaxKind.IdentifierToken);
                            }
                        }
                        N(SyntaxKind.DotToken);
                        N(SyntaxKind.IdentifierName);
                        {
                            N(SyntaxKind.IdentifierToken, "b");
                        }
                    }
                    N(SyntaxKind.OpenBraceToken);
                    N(SyntaxKind.CloseBraceToken);
                }
                N(SyntaxKind.EndOfFileToken);
            }
            EOF();
        }

        [Theory]
        [CombinatorialData]
        public void DefaultConstraint_01(bool useCSharp8)
        {
            var test = @"class C<T> where T : default { }";

            CreateCompilation(test, parseOptions: useCSharp8 ? TestOptions.Regular8 : TestOptions.Regular9).VerifyDiagnostics(
                useCSharp8
                    ? new[]
                    {
                        // (1,22): error CS8400: Feature 'default type parameter constraints' is not available in C# 8.0. Please use language version 9.0 or greater.
                        // class C<T> where T : default { }
                        Diagnostic(ErrorCode.ERR_FeatureNotAvailableInVersion8, "default").WithArguments("default type parameter constraints", "9.0").WithLocation(1, 22),
                        // (1,22): error CS8823: The 'default' constraint is valid on override and explicit interface implementation methods only.
                        // class C<T> where T : default { }
                        Diagnostic(ErrorCode.ERR_DefaultConstraintOverrideOnly, "default").WithLocation(1, 22)
                    }
                    : new[]
                    {
                        // (1,22): error CS8823: The 'default' constraint is valid on override and explicit interface implementation methods only.
                        // class C<T> where T : default { }
                        Diagnostic(ErrorCode.ERR_DefaultConstraintOverrideOnly, "default").WithLocation(1, 22)
                    });

            UsingNode(
                test,
                useCSharp8 ? TestOptions.Regular8 : TestOptions.Regular9);

            N(SyntaxKind.CompilationUnit);
            {
                N(SyntaxKind.ClassDeclaration);
                {
                    N(SyntaxKind.ClassKeyword);
                    N(SyntaxKind.IdentifierToken, "C");
                    N(SyntaxKind.TypeParameterList);
                    {
                        N(SyntaxKind.LessThanToken);
                        N(SyntaxKind.TypeParameter);
                        {
                            N(SyntaxKind.IdentifierToken, "T");
                        }
                        N(SyntaxKind.GreaterThanToken);
                    }
                    N(SyntaxKind.TypeParameterConstraintClause);
                    {
                        N(SyntaxKind.WhereKeyword);
                        N(SyntaxKind.IdentifierName);
                        {
                            N(SyntaxKind.IdentifierToken, "T");
                        }
                        N(SyntaxKind.ColonToken);
                        N(SyntaxKind.DefaultConstraint);
                        {
                            N(SyntaxKind.DefaultKeyword);
                        }
                    }
                    N(SyntaxKind.OpenBraceToken);
                    N(SyntaxKind.CloseBraceToken);
                }
                N(SyntaxKind.EndOfFileToken);
            }
            EOF();
        }

        [Fact]
        public void DefaultConstraint_02()
        {
            UsingNode(
@"class C<T, U>
    where T : default
    where U : default { }");

            N(SyntaxKind.CompilationUnit);
            {
                N(SyntaxKind.ClassDeclaration);
                {
                    N(SyntaxKind.ClassKeyword);
                    N(SyntaxKind.IdentifierToken, "C");
                    N(SyntaxKind.TypeParameterList);
                    {
                        N(SyntaxKind.LessThanToken);
                        N(SyntaxKind.TypeParameter);
                        {
                            N(SyntaxKind.IdentifierToken, "T");
                        }
                        N(SyntaxKind.CommaToken);
                        N(SyntaxKind.TypeParameter);
                        {
                            N(SyntaxKind.IdentifierToken, "U");
                        }
                        N(SyntaxKind.GreaterThanToken);
                    }
                    N(SyntaxKind.TypeParameterConstraintClause);
                    {
                        N(SyntaxKind.WhereKeyword);
                        N(SyntaxKind.IdentifierName);
                        {
                            N(SyntaxKind.IdentifierToken, "T");
                        }
                        N(SyntaxKind.ColonToken);
                        N(SyntaxKind.DefaultConstraint);
                        {
                            N(SyntaxKind.DefaultKeyword);
                        }
                    }
                    N(SyntaxKind.TypeParameterConstraintClause);
                    {
                        N(SyntaxKind.WhereKeyword);
                        N(SyntaxKind.IdentifierName);
                        {
                            N(SyntaxKind.IdentifierToken, "U");
                        }
                        N(SyntaxKind.ColonToken);
                        N(SyntaxKind.DefaultConstraint);
                        {
                            N(SyntaxKind.DefaultKeyword);
                        }
                    }
                    N(SyntaxKind.OpenBraceToken);
                    N(SyntaxKind.CloseBraceToken);
                }
                N(SyntaxKind.EndOfFileToken);
            }
            EOF();
        }

        [Theory]
        [CombinatorialData]
        public void DefaultConstraint_03(bool useCSharp8)
        {
            var test =
@"class C<T, U>
    where T : struct, default
    where U : default, class { }";

            CreateCompilation(test, parseOptions: useCSharp8 ? TestOptions.Regular8 : TestOptions.Regular9).VerifyDiagnostics(
                useCSharp8
                ? new[] {
                    // (2,23): error CS8400: Feature 'default type parameter constraints' is not available in C# 8.0. Please use language version 9.0 or greater.
                    //     where T : struct, default
                    Diagnostic(ErrorCode.ERR_FeatureNotAvailableInVersion8, "default").WithArguments("default type parameter constraints", "9.0").WithLocation(2, 23),
                    // (2,23): error CS8823: The 'default' constraint is valid on override and explicit interface implementation methods only.
                    //     where T : struct, default
                    Diagnostic(ErrorCode.ERR_DefaultConstraintOverrideOnly, "default").WithLocation(2, 23),
                    // (2,23): error CS0449: The 'class', 'struct', 'unmanaged', 'notnull', and 'default' constraints cannot be combined or duplicated, and must be specified first in the constraints list.
                    //     where T : struct, default
                    Diagnostic(ErrorCode.ERR_TypeConstraintsMustBeUniqueAndFirst, "default").WithLocation(2, 23),
                    // (3,15): error CS8400: Feature 'default type parameter constraints' is not available in C# 8.0. Please use language version 9.0 or greater.
                    //     where U : default, class { }
                    Diagnostic(ErrorCode.ERR_FeatureNotAvailableInVersion8, "default").WithArguments("default type parameter constraints", "9.0").WithLocation(3, 15),
                    // (3,15): error CS8823: The 'default' constraint is valid on override and explicit interface implementation methods only.
                    //     where U : default, class { }
                    Diagnostic(ErrorCode.ERR_DefaultConstraintOverrideOnly, "default").WithLocation(3, 15),
                    // (3,24): error CS0449: The 'class', 'struct', 'unmanaged', 'notnull', and 'default' constraints cannot be combined or duplicated, and must be specified first in the constraints list.
                    //     where U : default, class { }
                    Diagnostic(ErrorCode.ERR_TypeConstraintsMustBeUniqueAndFirst, "class").WithLocation(3, 24) }
                : new[] {
                    // (2,23): error CS8823: The 'default' constraint is valid on override and explicit interface implementation methods only.
                    //     where T : struct, default
                    Diagnostic(ErrorCode.ERR_DefaultConstraintOverrideOnly, "default").WithLocation(2, 23),
                    // (2,23): error CS0449: The 'class', 'struct', 'unmanaged', 'notnull', and 'default' constraints cannot be combined or duplicated, and must be specified first in the constraints list.
                    //     where T : struct, default
                    Diagnostic(ErrorCode.ERR_TypeConstraintsMustBeUniqueAndFirst, "default").WithLocation(2, 23),
                    // (3,15): error CS8823: The 'default' constraint is valid on override and explicit interface implementation methods only.
                    //     where U : default, class { }
                    Diagnostic(ErrorCode.ERR_DefaultConstraintOverrideOnly, "default").WithLocation(3, 15),
                    // (3,24): error CS0449: The 'class', 'struct', 'unmanaged', 'notnull', and 'default' constraints cannot be combined or duplicated, and must be specified first in the constraints list.
                    //     where U : default, class { }
                    Diagnostic(ErrorCode.ERR_TypeConstraintsMustBeUniqueAndFirst, "class").WithLocation(3, 24) });

            UsingNode(test,
                useCSharp8 ? TestOptions.Regular8 : TestOptions.Regular9);

            N(SyntaxKind.CompilationUnit);
            {
                N(SyntaxKind.ClassDeclaration);
                {
                    N(SyntaxKind.ClassKeyword);
                    N(SyntaxKind.IdentifierToken, "C");
                    N(SyntaxKind.TypeParameterList);
                    {
                        N(SyntaxKind.LessThanToken);
                        N(SyntaxKind.TypeParameter);
                        {
                            N(SyntaxKind.IdentifierToken, "T");
                        }
                        N(SyntaxKind.CommaToken);
                        N(SyntaxKind.TypeParameter);
                        {
                            N(SyntaxKind.IdentifierToken, "U");
                        }
                        N(SyntaxKind.GreaterThanToken);
                    }
                    N(SyntaxKind.TypeParameterConstraintClause);
                    {
                        N(SyntaxKind.WhereKeyword);
                        N(SyntaxKind.IdentifierName);
                        {
                            N(SyntaxKind.IdentifierToken, "T");
                        }
                        N(SyntaxKind.ColonToken);
                        N(SyntaxKind.StructConstraint);
                        {
                            N(SyntaxKind.StructKeyword);
                        }
                        N(SyntaxKind.CommaToken);
                        N(SyntaxKind.DefaultConstraint);
                        {
                            N(SyntaxKind.DefaultKeyword);
                        }
                    }
                    N(SyntaxKind.TypeParameterConstraintClause);
                    {
                        N(SyntaxKind.WhereKeyword);
                        N(SyntaxKind.IdentifierName);
                        {
                            N(SyntaxKind.IdentifierToken, "U");
                        }
                        N(SyntaxKind.ColonToken);
                        N(SyntaxKind.DefaultConstraint);
                        {
                            N(SyntaxKind.DefaultKeyword);
                        }
                        N(SyntaxKind.CommaToken);
                        N(SyntaxKind.ClassConstraint);
                        {
                            N(SyntaxKind.ClassKeyword);
                        }
                    }
                    N(SyntaxKind.OpenBraceToken);
                    N(SyntaxKind.CloseBraceToken);
                }
                N(SyntaxKind.EndOfFileToken);
            }
            EOF();
        }

        [Fact]
        public void DefaultConstraint_04()
        {
            UsingNode(
@"class C<T, U>
    where T : struct default
    where U : default class { }");

            N(SyntaxKind.CompilationUnit);
            {
                N(SyntaxKind.ClassDeclaration);
                {
                    N(SyntaxKind.ClassKeyword);
                    N(SyntaxKind.IdentifierToken, "C");
                    N(SyntaxKind.TypeParameterList);
                    {
                        N(SyntaxKind.LessThanToken);
                        N(SyntaxKind.TypeParameter);
                        {
                            N(SyntaxKind.IdentifierToken, "T");
                        }
                        N(SyntaxKind.CommaToken);
                        N(SyntaxKind.TypeParameter);
                        {
                            N(SyntaxKind.IdentifierToken, "U");
                        }
                        N(SyntaxKind.GreaterThanToken);
                    }
                    N(SyntaxKind.TypeParameterConstraintClause);
                    {
                        N(SyntaxKind.WhereKeyword);
                        N(SyntaxKind.IdentifierName);
                        {
                            N(SyntaxKind.IdentifierToken, "T");
                        }
                        N(SyntaxKind.ColonToken);
                        N(SyntaxKind.StructConstraint);
                        {
                            N(SyntaxKind.StructKeyword);
                        }
                        M(SyntaxKind.CommaToken);
                        N(SyntaxKind.DefaultConstraint);
                        {
                            N(SyntaxKind.DefaultKeyword);
                        }
                    }
                    N(SyntaxKind.TypeParameterConstraintClause);
                    {
                        N(SyntaxKind.WhereKeyword);
                        N(SyntaxKind.IdentifierName);
                        {
                            N(SyntaxKind.IdentifierToken, "U");
                        }
                        N(SyntaxKind.ColonToken);
                        N(SyntaxKind.DefaultConstraint);
                        {
                            N(SyntaxKind.DefaultKeyword);
                        }
                        M(SyntaxKind.CommaToken);
                        N(SyntaxKind.ClassConstraint);
                        {
                            N(SyntaxKind.ClassKeyword);
                        }
                    }
                    N(SyntaxKind.OpenBraceToken);
                    N(SyntaxKind.CloseBraceToken);
                }
                N(SyntaxKind.EndOfFileToken);
            }
            EOF();
        }
    }
}
