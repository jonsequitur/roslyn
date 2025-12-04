// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#nullable disable

using System;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp.Test.Utilities;
using Microsoft.CodeAnalysis.Text;
using Xunit;
using AwesomeAssertions;

//test

namespace Microsoft.CodeAnalysis.CSharp.UnitTests.IncrementalParsing
{
    // These tests handle changing between constructors/destructors and methods.
    // In addition, changes between get/set and add/remove are also tested
    public class TypeChanges
    {
        [Fact]
        public void ConstructorToDestructor()
        {
            string oldText = @"class construct{
                              public construct(){}   
                              }";

            ParseAndVerify(oldText, validator: oldTree =>
            {
                var newTree = oldTree.WithReplace(16, "construct", "~construct");
                var classType = newTree.GetCompilationUnitRoot().Members[0] as TypeDeclarationSyntax;
                classType.Members[0].Should().BeOfType<DestructorDeclarationSyntax>();
            });
        }

        [Fact]
        public void MethodToConstructor()
        {
            string oldText = @"class construct{
                              public M(){}   
                              }";

            ParseAndVerify(oldText, validator: oldTree =>
            {
                var newTree = oldTree.WithReplace(16, "M", "construct");
                var classType = newTree.GetCompilationUnitRoot().Members[0] as TypeDeclarationSyntax;
                classType.Members[0].Should().BeOfType<ConstructorDeclarationSyntax>();
            });
        }

        [Fact]
        public void ConstructorToMethod()
        {
            string oldText = @"class construct{
                              public construct(){}   
                              }";

            ParseAndVerify(oldText, validator: oldTree =>
            {
                var newTree = oldTree.WithReplace(16, "construct", "M");
                var classType = newTree.GetCompilationUnitRoot().Members[0] as TypeDeclarationSyntax;
                classType.Members[0].Should().BeOfType<ConstructorDeclarationSyntax>();
            });
        }

        [Fact]
        public void DestructorToConstructor()
        {
            string oldText = @"class construct{
                              public ~construct(){}   
                              }";

            ParseAndVerify(oldText, validator: oldTree =>
            {
                var newTree = oldTree.WithReplace(16, "~construct", "construct");
                var classType = newTree.GetCompilationUnitRoot().Members[0] as TypeDeclarationSyntax;
                classType.Members[0].Should().BeOfType<ConstructorDeclarationSyntax>();
            });
        }

        [Fact]
        public void SetToGet()
        {
            string oldText = @"class construct{
                                public int B {get {} }
                              }";

            ParseAndVerify(oldText, validator: oldTree =>
            {
                var newTree = oldTree.WithReplace(16, "get", "set");
                var classType = newTree.GetCompilationUnitRoot().Members[0] as TypeDeclarationSyntax;
                var propertyType = classType.Members[0] as PropertyDeclarationSyntax;
                propertyType.AccessorList.Accessors[0].Kind().Should().Be(SyntaxKind.SetAccessorDeclaration);
            });
        }

        [Fact]
        public void GetToSet()
        {
            string oldText = @"class construct{
                                public int B {set {} }
                              }";

            ParseAndVerify(oldText, validator: oldTree =>
            {
                var newTree = oldTree.WithReplace(16, "set", "get");
                var classType = newTree.GetCompilationUnitRoot().Members[0] as TypeDeclarationSyntax;
                var propertyType = classType.Members[0] as PropertyDeclarationSyntax;
                propertyType.AccessorList.Accessors[0].Kind().Should().Be(SyntaxKind.GetAccessorDeclaration);
            });
        }

        [Fact]
        public void EventAddToRemove()
        {
            string oldText = @"class construct{
                                public event B b {add {} }
                              }";

            ParseAndVerify(oldText, validator: oldTree =>
            {
                var newTree = oldTree.WithReplace(16, "add", "remove");
                var classType = newTree.GetCompilationUnitRoot().Members[0] as TypeDeclarationSyntax;
                var propertyType = classType.Members[0] as EventDeclarationSyntax;
                propertyType.AccessorList.Accessors[0].Kind().Should().Be(SyntaxKind.RemoveAccessorDeclaration);
            });
        }

        [Fact]
        public void EventRemoveToAdd()
        {
            string oldText = @"class construct{
                                public event B b {remove {} }
                              }";

            ParseAndVerify(oldText, validator: oldTree =>
            {
                var newTree = oldTree.WithReplace(16, "remove", "add");
                var classType = newTree.GetCompilationUnitRoot().Members[0] as TypeDeclarationSyntax;
                var propertyType = classType.Members[0] as EventDeclarationSyntax;
                propertyType.AccessorList.Accessors[0].Kind().Should().Be(SyntaxKind.AddAccessorDeclaration);
            });
        }

        #region Helpers
        private static void ParseAndVerify(string text, Action<SyntaxTree> validator)
        {
            ParseAndValidate(text, validator);
            ParseAndValidate(text, validator, TestOptions.Script);
        }

        private static void ParseAndValidate(string text, Action<SyntaxTree> validator, CSharpParseOptions options = null)
        {
            var oldTree = SyntaxFactory.ParseSyntaxTree(text);
            validator(oldTree);
        }
        #endregion
    }
}
