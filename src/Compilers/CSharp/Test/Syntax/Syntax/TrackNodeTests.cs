// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Test.Utilities;
using Xunit;
using InternalSyntax = Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;
using AwesomeAssertions;

namespace Microsoft.CodeAnalysis.CSharp.UnitTests
{
    public class TrackNodeTests
    {
        [Fact]
        public void TestGetCurrentNodeAfterTrackNodesReturnsCurrentNode()
        {
            var expr = SyntaxFactory.ParseExpression("a + b");
            var a = expr.DescendantNodes().OfType<IdentifierNameSyntax>().First(n => n.Identifier.Text == "a");
            var trackedExpr = expr.TrackNodes(a);
            var currentA = trackedExpr.GetCurrentNode(a);
            currentA.Should().NotBeNull();
            currentA.ToString().Should().Be("a");
        }

        [Fact]
        public void TestGetCurrentNodesAfterTrackNodesReturnsSingletonSequence()
        {
            var expr = SyntaxFactory.ParseExpression("a + b");
            var a = expr.DescendantNodes().OfType<IdentifierNameSyntax>().First(n => n.Identifier.Text == "a");
            var trackedExpr = expr.TrackNodes(a);
            var currentAs = trackedExpr.GetCurrentNodes(a);
            currentAs.Should().NotBeNull();
            currentAs.Count().Should().Be(1);
            currentAs.ElementAt(0).ToString().Should().Be("a");
        }

        [Fact]
        public void TestGetCurrentNodeWithoutTrackNodesReturnsNull()
        {
            var expr = SyntaxFactory.ParseExpression("a + b");
            var a = expr.DescendantNodes().OfType<IdentifierNameSyntax>().First(n => n.Identifier.Text == "a");
            var currentA = expr.GetCurrentNode(a);
            currentA.Should().BeNull();
        }

        [Fact]
        public void TestGetCurrentNodesWithoutTrackNodesReturnsEmptySequence()
        {
            var expr = SyntaxFactory.ParseExpression("a + b");
            var a = expr.DescendantNodes().OfType<IdentifierNameSyntax>().First(n => n.Identifier.Text == "a");
            var currentAs = expr.GetCurrentNodes(a);
            currentAs.Should().NotBeNull();
            currentAs.Count().Should().Be(0);
        }

        [Fact]
        public void TestGetCurrentNodeAfterEditReturnsCurrentNode()
        {
            var expr = SyntaxFactory.ParseExpression("a + b");
            var originalA = expr.DescendantNodes().OfType<IdentifierNameSyntax>().First(n => n.Identifier.Text == "a");
            var trackedExpr = expr.TrackNodes(originalA);
            var currentA = trackedExpr.GetCurrentNode(originalA);
            var newA = currentA.WithLeadingTrivia(SyntaxFactory.Comment("/* ayup */"));
            var replacedExpr = trackedExpr.ReplaceNode(currentA, newA);
            var latestA = replacedExpr.GetCurrentNode(originalA);
            latestA.Should().NotBeNull();
            newA.Should().NotBeSameAs(latestA); // not the same reference
            latestA.ToFullString().Should().Be(newA.ToFullString());
        }

        [Fact]
        public void TestGetCurrentNodeAfterEditReturnsSingletonSequence()
        {
            var expr = SyntaxFactory.ParseExpression("a + b");
            var originalA = expr.DescendantNodes().OfType<IdentifierNameSyntax>().First(n => n.Identifier.Text == "a");
            var trackedExpr = expr.TrackNodes(originalA);
            var currentA = trackedExpr.GetCurrentNode(originalA);
            var newA = currentA.WithLeadingTrivia(SyntaxFactory.Comment("/* ayup */"));
            var replacedExpr = trackedExpr.ReplaceNode(currentA, newA);
            var latestAs = replacedExpr.GetCurrentNodes(originalA);
            latestAs.Should().NotBeNull();
            latestAs.Count().Should().Be(1);
            latestAs.ElementAt(0).ToFullString().Should().Be(newA.ToFullString());
        }

        [WorkItem(1070667, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/1070667")]
        [Fact]
        public void TestGetCurrentNodeAfterRemovalReturnsNull()
        {
            var expr = SyntaxFactory.ParseExpression("a + b");
            var originalA = expr.DescendantNodes().OfType<IdentifierNameSyntax>().First(n => n.Identifier.Text == "a");
            var trackedExpr = expr.TrackNodes(originalA);
            var currentA = trackedExpr.GetCurrentNode(originalA);
            var replacedExpr = trackedExpr.ReplaceNode(currentA, SyntaxFactory.IdentifierName("c"));
            var latestA = replacedExpr.GetCurrentNode(originalA);
            latestA.Should().BeNull();
        }

        [Fact]
        public void TestGetCurrentNodesAfterRemovalEmptySequence()
        {
            var expr = SyntaxFactory.ParseExpression("a + b");
            var originalA = expr.DescendantNodes().OfType<IdentifierNameSyntax>().First(n => n.Identifier.Text == "a");
            var trackedExpr = expr.TrackNodes(originalA);
            var currentA = trackedExpr.GetCurrentNode(originalA);
            var replacedExpr = trackedExpr.ReplaceNode(currentA, SyntaxFactory.IdentifierName("c"));
            var latestAs = replacedExpr.GetCurrentNodes(originalA);
            latestAs.Should().NotBeNull();
            latestAs.Count().Should().Be(0);
        }

        [Fact]
        public void TestGetCurrentNodeAfterAddingMultipleThrows()
        {
            var expr = SyntaxFactory.ParseExpression("a + b");
            var originalA = expr.DescendantNodes().OfType<IdentifierNameSyntax>().First(n => n.Identifier.Text == "a");
            var trackedExpr = expr.TrackNodes(originalA);
            var currentA = trackedExpr.GetCurrentNode(originalA);
            // replace all identifiers with same 'a'
            var replacedExpr = trackedExpr.ReplaceNodes(trackedExpr.DescendantNodes().OfType<IdentifierNameSyntax>(), (original, changed) => currentA);
            FluentActions.Invoking(() => replacedExpr.GetCurrentNode(originalA)).Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void TestGetCurrentNodeAfterAddingMultipleReturnsMultiple()
        {
            var expr = SyntaxFactory.ParseExpression("a + b");
            var originalA = expr.DescendantNodes().OfType<IdentifierNameSyntax>().First(n => n.Identifier.Text == "a");
            var trackedExpr = expr.TrackNodes(originalA);
            var currentA = trackedExpr.GetCurrentNode(originalA);
            // replace all identifiers with same 'a'
            var replacedExpr = trackedExpr.ReplaceNodes(trackedExpr.DescendantNodes().OfType<IdentifierNameSyntax>(), (original, changed) => currentA);
            var nodes = replacedExpr.GetCurrentNodes(originalA).ToList();
            nodes.Count.Should().Be(2);
            nodes[0].ToString().Should().Be("a");
            nodes[1].ToString().Should().Be("a");
        }

        [Fact]
        public void TestTrackNodesWithMultipleTracksAllNodes()
        {
            var expr = SyntaxFactory.ParseExpression("a + b + c");
            var ids = expr.DescendantNodes().OfType<IdentifierNameSyntax>().ToList();
            var trackedExpr = expr.TrackNodes(ids);

            ids.Count.Should().Be(3);

            foreach (var id in ids)
            {
                var currentId = trackedExpr.GetCurrentNode(id);
                currentId.Should().NotBeNull();
                currentId.Should().NotBeSameAs(id);
                currentId.ToString().Should().Be(id.ToString());
            }
        }

        [Fact]
        public void TestTrackNodesWithNoNodesTracksNothing()
        {
            var expr = SyntaxFactory.ParseExpression("a + b + c");
            var ids = expr.DescendantNodes().OfType<IdentifierNameSyntax>().ToList();

            var trackedExpr = expr.TrackNodes();

            ids.Count.Should().Be(3);

            foreach (var id in ids)
            {
                var currentId = trackedExpr.GetCurrentNode(id);
                currentId.Should().BeNull();
            }
        }

        [Fact]
        public void TestTrackNodeThatIsNotInTheSubtreeThrows()
        {
            var expr = SyntaxFactory.ParseExpression("a + b");
            FluentActions.Invoking(() => expr.TrackNodes(SyntaxFactory.IdentifierName("c"))).Should().Throw<ArgumentException>();
        }
    }
}
