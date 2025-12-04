// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#nullable disable

using System;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Test.Utilities;
using Xunit;
using AwesomeAssertions;

namespace Microsoft.CodeAnalysis.CSharp.UnitTests
{
    public class SerializationTests
    {
        [Fact]
        public void RoundTripSyntaxNode()
        {
            var text = "public class C {}";
            var tree = SyntaxFactory.ParseSyntaxTree(text);
            var root = tree.GetCompilationUnitRoot();

            var stream = new MemoryStream();
            root.SerializeTo(stream);

            stream.Position = 0;

            var droot = CSharpSyntaxNode.DeserializeFrom(stream);
            var dtext = droot.ToFullString();

            droot.IsEquivalentTo(tree.GetCompilationUnitRoot()).Should().BeTrue();
        }

        [Fact]
        public void RoundTripSyntaxNodeWithDiagnostics()
        {
            var text = "public class C {";
            var tree = SyntaxFactory.ParseSyntaxTree(text);
            var root = tree.GetCompilationUnitRoot();
            root.Errors().Length.Should().Be(1);

            var stream = new MemoryStream();
            root.SerializeTo(stream);

            stream.Position = 0;

            var droot = CSharpSyntaxNode.DeserializeFrom(stream);
            var dtext = droot.ToFullString();

            dtext.Should().Be(text);
            droot.Errors().Length.Should().Be(1);
            droot.IsEquivalentTo(tree.GetCompilationUnitRoot()).Should().BeTrue();
            droot.Errors()[0].GetMessage().Should().Be(root.Errors()[0].GetMessage());
        }

        [Fact]
        public void RoundTripSyntaxNodeWithAnnotation()
        {
            var text = "public class C {}";
            var tree = SyntaxFactory.ParseSyntaxTree(text);
            var annotation = new SyntaxAnnotation();
            var root = tree.GetCompilationUnitRoot().WithAdditionalAnnotations(annotation);
            root.ContainsAnnotations.Should().BeTrue();
            root.HasAnnotation(annotation).Should().BeTrue();

            var stream = new MemoryStream();
            root.SerializeTo(stream);

            stream.Position = 0;

            var droot = CSharpSyntaxNode.DeserializeFrom(stream);
            var dtext = droot.ToFullString();

            dtext.Should().Be(text);
            droot.ContainsAnnotations.Should().BeTrue();
            droot.HasAnnotation(annotation).Should().BeTrue();
            droot.IsEquivalentTo(tree.GetCompilationUnitRoot()).Should().BeTrue();
        }

        [Fact]
        public void RoundTripSyntaxNodeWithMultipleReferencesToSameAnnotation()
        {
            var text = "public class C {}";
            var tree = SyntaxFactory.ParseSyntaxTree(text);
            var annotation = new SyntaxAnnotation();
            var root = tree.GetCompilationUnitRoot().WithAdditionalAnnotations(annotation, annotation);
            root.ContainsAnnotations.Should().BeTrue();
            root.HasAnnotation(annotation).Should().BeTrue();

            var stream = new MemoryStream();
            root.SerializeTo(stream);

            stream.Position = 0;

            var droot = CSharpSyntaxNode.DeserializeFrom(stream);
            var dtext = droot.ToFullString();

            dtext.Should().Be(text);
            droot.ContainsAnnotations.Should().BeTrue();
            droot.HasAnnotation(annotation).Should().BeTrue();
            droot.IsEquivalentTo(tree.GetCompilationUnitRoot()).Should().BeTrue();
        }

        [Fact]
        public void RoundTripSyntaxNodeWithSpecialAnnotation()
        {
            var text = "public class C {}";
            var tree = SyntaxFactory.ParseSyntaxTree(text);
            var annotation = new SyntaxAnnotation("TestAnnotation", "this is a test");
            var root = tree.GetCompilationUnitRoot().WithAdditionalAnnotations(annotation);
            root.ContainsAnnotations.Should().BeTrue();
            root.HasAnnotation(annotation).Should().BeTrue();

            var stream = new MemoryStream();
            root.SerializeTo(stream);

            stream.Position = 0;

            var droot = CSharpSyntaxNode.DeserializeFrom(stream);
            var dtext = droot.ToFullString();

            dtext.Should().Be(text);
            droot.ContainsAnnotations.Should().BeTrue();
            droot.HasAnnotation(annotation).Should().BeTrue();
            droot.IsEquivalentTo(tree.GetCompilationUnitRoot()).Should().BeTrue();

            var dannotation = droot.GetAnnotations("TestAnnotation").SingleOrDefault();
            dannotation.Should().NotBeNull();
            dannotation.Should().NotBeSameAs(annotation); // not exact same instance
            dannotation.Should().Be(annotation); // equivalent though
        }

        [Fact]
        public void RoundTripSyntaxNodeWithAnnotationsRemoved()
        {
            var text = "public class C {}";
            var tree = SyntaxFactory.ParseSyntaxTree(text);
            var annotation1 = new SyntaxAnnotation("annotation1");
            var root = tree.GetCompilationUnitRoot().WithAdditionalAnnotations(annotation1);
            root.ContainsAnnotations.Should().BeTrue();
            root.HasAnnotation(annotation1).Should().BeTrue();
            var removedRoot = root.WithoutAnnotations(annotation1);
            removedRoot.ContainsAnnotations.Should().BeFalse();
            removedRoot.HasAnnotation(annotation1).Should().BeFalse();

            var stream = new MemoryStream();
            removedRoot.SerializeTo(stream);

            stream.Position = 0;

            var droot = CSharpSyntaxNode.DeserializeFrom(stream);

            droot.ContainsAnnotations.Should().BeFalse();
            droot.HasAnnotation(annotation1).Should().BeFalse();

            var annotation2 = new SyntaxAnnotation("annotation2");

            var doubleAnnoRoot = droot.WithAdditionalAnnotations(annotation1, annotation2);
            doubleAnnoRoot.ContainsAnnotations.Should().BeTrue();
            doubleAnnoRoot.HasAnnotation(annotation1).Should().BeTrue();
            doubleAnnoRoot.HasAnnotation(annotation2).Should().BeTrue();
            var removedDoubleAnnoRoot = doubleAnnoRoot.WithoutAnnotations(annotation1, annotation2);
            removedDoubleAnnoRoot.ContainsAnnotations.Should().BeFalse();
            removedDoubleAnnoRoot.HasAnnotation(annotation1).Should().BeFalse();
            removedDoubleAnnoRoot.HasAnnotation(annotation2).Should().BeFalse();

            stream = new MemoryStream();
            removedRoot.SerializeTo(stream);

            stream.Position = 0;

            droot = CSharpSyntaxNode.DeserializeFrom(stream);

            droot.ContainsAnnotations.Should().BeFalse();
            droot.HasAnnotation(annotation1).Should().BeFalse();
            droot.HasAnnotation(annotation2).Should().BeFalse();
        }

        [Fact]
        public void RoundTripSyntaxNodeWithAnnotationRemovedWithMultipleReference()
        {
            var text = "public class C {}";
            var tree = SyntaxFactory.ParseSyntaxTree(text);
            var annotation1 = new SyntaxAnnotation("MyAnnotationId", "SomeData");
            var root = tree.GetCompilationUnitRoot().WithAdditionalAnnotations(annotation1, annotation1);
            root.ContainsAnnotations.Should().BeTrue();
            root.HasAnnotation(annotation1).Should().BeTrue();
            var removedRoot = root.WithoutAnnotations(annotation1);
            removedRoot.ContainsAnnotations.Should().BeFalse();
            removedRoot.HasAnnotation(annotation1).Should().BeFalse();

            var stream = new MemoryStream();
            removedRoot.SerializeTo(stream);

            stream.Position = 0;

            var droot = CSharpSyntaxNode.DeserializeFrom(stream);

            droot.ContainsAnnotations.Should().BeFalse();
            droot.HasAnnotation(annotation1).Should().BeFalse();
        }

        private static void RoundTrip(string text, bool expectRecursive = true)
        {
            var tree = SyntaxFactory.ParseSyntaxTree(text);
            var root = tree.GetCompilationUnitRoot();
            var originalText = root.ToFullString();

            var stream = new MemoryStream();
            root.SerializeTo(stream);

            stream.Position = 0;
            var newRoot = CSharpSyntaxNode.DeserializeFrom(stream);
            var newText = newRoot.ToFullString();

            newRoot.IsEquivalentTo(tree.GetCompilationUnitRoot()).Should().BeTrue();
            newText.Should().Be(originalText);
        }

        [Fact]
        public void RoundTripXmlDocComment()
        {
            RoundTrip(@"/// <summary>XML Doc comment</summary>
class C { }");
        }

        [Fact]
        public void RoundTripCharLiteralWithIllegalUnicodeValue()
        {
            RoundTrip(@"public class C { char c = '\uDC00'; }");
        }

        [Fact]
        public void RoundTripCharLiteralWithIllegalUnicodeValue2()
        {
            RoundTrip(@"public class C { char c = '\");
        }

        [Fact]
        public void RoundTripCharLiteralWithIllegalUnicodeValue3()
        {
            RoundTrip(@"public class C { char c = '\u");
        }

        [Fact]
        public void RoundTripCharLiteralWithIllegalUnicodeValue4()
        {
            RoundTrip(@"public class C { char c = '\uDC00DC");
        }

        [Fact]
        public void RoundTripStringLiteralWithIllegalUnicodeValue()
        {
            RoundTrip(@"public class C { string s = ""\uDC00""; }");
        }

        [Fact]
        public void RoundTripStringLiteralWithUnicodeCharacters()
        {
            RoundTrip(@"public class C { string s = ""Юникод""; }");
        }

        [Fact]
        public void RoundTripStringLiteralWithUnicodeCharacters2()
        {
            RoundTrip(@"public class C { string c = ""\U0002A6A5𪚥""; }");
        }

        [Fact, WorkItem(1038237, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/1038237")]
        public void RoundTripPragmaDirective()
        {
            var text = @"#pragma disable warning CS0618";

            var tree = SyntaxFactory.ParseSyntaxTree(text);
            var root = tree.GetCompilationUnitRoot();
            root.ContainsDirectives.Should().BeTrue();

            var stream = new MemoryStream();
            root.SerializeTo(stream);

            stream.Position = 0;

            var newRoot = CSharpSyntaxNode.DeserializeFrom(stream);
            newRoot.ContainsDirectives.Should().BeTrue();
        }

        [Fact]
        public void RoundTripDeepSyntaxNode()
        {
            // trees with excessively deep expressions tend to overflow the stack when using recursive encoding.
            // test that the tree is successfully serialized using non-recursive encoding.
            var text = @"
public class C
{
    public string B = " + string.Join(" + ", Enumerable.Range(0, 1000).Select(i => "\"" + i.ToString() + "\"").ToArray()) + @";
}";

            // serialization should fail to encode stream using recursive object encoding and
            // succeed with non-recursive object encoding.
            RoundTrip(text, expectRecursive: false);
        }
    }
}
