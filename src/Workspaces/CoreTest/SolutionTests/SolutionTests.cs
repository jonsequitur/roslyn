﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Host;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.Options;
using Microsoft.CodeAnalysis.Shared.Extensions;
using Microsoft.CodeAnalysis.Shared.Utilities;
using Microsoft.CodeAnalysis.Test.Utilities;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.VisualStudio.Threading;
using Roslyn.Test.Utilities;
using Roslyn.Utilities;
using Xunit;
using CS = Microsoft.CodeAnalysis.CSharp;

namespace Microsoft.CodeAnalysis.UnitTests
{
    [UseExportProvider]
    public class SolutionTests : TestBase
    {
#nullable enable
        private static readonly MetadataReference s_mscorlib = TestReferences.NetFx.v4_0_30319.mscorlib;
        private static readonly DocumentId s_unrelatedDocumentId = DocumentId.CreateNewId(ProjectId.CreateNewId());

        private Solution CreateSolution()
        {
            return new AdhocWorkspace(MefHostServices.Create(MefHostServices.DefaultAssemblies.Add(typeof(NoCompilationConstants).Assembly))).CurrentSolution;
        }

        private Solution CreateSolutionWithProjectAndDocuments()
        {
            var projectId = ProjectId.CreateNewId();

            return CreateSolution()
                .AddProject(projectId, "goo", "goo.dll", LanguageNames.CSharp)
                .AddDocument(DocumentId.CreateNewId(projectId), "goo.cs", "public class Goo { }")
                .AddAdditionalDocument(DocumentId.CreateNewId(projectId), "add.txt", "text")
                .AddAnalyzerConfigDocument(DocumentId.CreateNewId(projectId), "editorcfg", SourceText.From("config"), filePath: "/a/b");
        }

        [Fact, Trait(Traits.Feature, Traits.Features.Workspace)]
        public void RemoveDocument_Errors()
        {
            var solution = CreateSolution();
            Assert.Throws<ArgumentNullException>(() => solution.RemoveDocument(null!));
            Assert.Throws<InvalidOperationException>(() => solution.RemoveDocument(s_unrelatedDocumentId));
        }

        [Fact, Trait(Traits.Feature, Traits.Features.Workspace)]
        public void RemoveDocuments_Errors()
        {
            var solution = CreateSolution();
            Assert.Throws<ArgumentNullException>(() => solution.RemoveDocuments(default));
            Assert.Throws<InvalidOperationException>(() => solution.RemoveDocuments(ImmutableArray.Create(s_unrelatedDocumentId)));
            Assert.Throws<ArgumentNullException>(() => solution.RemoveDocuments(ImmutableArray.Create((DocumentId)null!)));
        }

        [Fact, Trait(Traits.Feature, Traits.Features.Workspace)]
        public void RemoveAdditionalDocument_Errors()
        {
            var solution = CreateSolution();
            Assert.Throws<ArgumentNullException>(() => solution.RemoveAdditionalDocument(null!));
            Assert.Throws<InvalidOperationException>(() => solution.RemoveAdditionalDocument(s_unrelatedDocumentId));
        }

        [Fact, Trait(Traits.Feature, Traits.Features.Workspace)]
        public void RemoveAdditionalDocuments_Errors()
        {
            var solution = CreateSolution();
            Assert.Throws<ArgumentNullException>(() => solution.RemoveAdditionalDocuments(default));
            Assert.Throws<InvalidOperationException>(() => solution.RemoveAdditionalDocuments(ImmutableArray.Create(s_unrelatedDocumentId)));
            Assert.Throws<ArgumentNullException>(() => solution.RemoveAdditionalDocuments(ImmutableArray.Create((DocumentId)null!)));
        }

        [Fact, Trait(Traits.Feature, Traits.Features.Workspace)]
        public void RemoveAnalyzerConfigDocument_Errors()
        {
            var solution = CreateSolution();
            Assert.Throws<ArgumentNullException>(() => solution.RemoveAnalyzerConfigDocument(null!));
            Assert.Throws<InvalidOperationException>(() => solution.RemoveAnalyzerConfigDocument(s_unrelatedDocumentId));
        }

        [Fact, Trait(Traits.Feature, Traits.Features.Workspace)]
        public void RemoveAnalyzerConfigDocuments_Errors()
        {
            var solution = CreateSolution();
            Assert.Throws<ArgumentNullException>(() => solution.RemoveAnalyzerConfigDocuments(default));
            Assert.Throws<InvalidOperationException>(() => solution.RemoveAnalyzerConfigDocuments(ImmutableArray.Create(s_unrelatedDocumentId)));
            Assert.Throws<ArgumentNullException>(() => solution.RemoveAnalyzerConfigDocuments(ImmutableArray.Create((DocumentId)null!)));
        }

        [Fact, Trait(Traits.Feature, Traits.Features.Workspace)]
        public void WithDocumentName()
        {
            var solution = CreateSolutionWithProjectAndDocuments();
            var documentId = solution.Projects.Single().DocumentIds.Single();
            var name = "new name";

            var newSolution1 = solution.WithDocumentName(documentId, name);
            Assert.Equal(name, newSolution1.GetDocument(documentId)!.Name);

            var newSolution2 = newSolution1.WithDocumentName(documentId, name);
            Assert.Same(newSolution1, newSolution2);

            Assert.Throws<ArgumentNullException>(() => solution.WithDocumentName(documentId, name: null!));

            Assert.Throws<ArgumentNullException>(() => solution.WithDocumentName(null!, name));
            Assert.Throws<InvalidOperationException>(() => solution.WithDocumentName(s_unrelatedDocumentId, name));
        }

        [Fact, Trait(Traits.Feature, Traits.Features.Workspace)]
        public void WithDocumentFolders()
        {
            var solution = CreateSolutionWithProjectAndDocuments();
            var documentId = solution.Projects.Single().DocumentIds.Single();
            var folders = new[] { "folder1", "folder2" };

            var newSolution1 = solution.WithDocumentFolders(documentId, folders);
            Assert.Equal(folders, newSolution1.GetDocument(documentId)!.Folders);

            var newSolution2 = newSolution1.WithDocumentFolders(documentId, folders);
            Assert.Same(newSolution2, newSolution1);

            // empty:
            var newSolution3 = solution.WithDocumentFolders(documentId, new string[0]);
            Assert.Equal(new string[0], newSolution3.GetDocument(documentId)!.Folders);

            var newSolution4 = solution.WithDocumentFolders(documentId, ImmutableArray<string>.Empty);
            Assert.Same(newSolution3, newSolution4);

            var newSolution5 = solution.WithDocumentFolders(documentId, null);
            Assert.Same(newSolution3, newSolution5);

            Assert.Throws<ArgumentNullException>(() => solution.WithDocumentFolders(documentId, folders: new string[] { null! }));

            Assert.Throws<ArgumentNullException>(() => solution.WithDocumentFolders(null!, folders));
            Assert.Throws<InvalidOperationException>(() => solution.WithDocumentFolders(s_unrelatedDocumentId, folders));
        }

        [Fact, Trait(Traits.Feature, Traits.Features.Workspace)]
        public void WithDocumentFilePath()
        {
            var solution = CreateSolutionWithProjectAndDocuments();
            var documentId = solution.Projects.Single().DocumentIds.Single();
            var path = "new path";

            var newSolution1 = solution.WithDocumentFilePath(documentId, path);
            Assert.Equal(path, newSolution1.GetDocument(documentId)!.FilePath);

            var newSolution2 = newSolution1.WithDocumentFilePath(documentId, path);
            Assert.Same(newSolution1, newSolution2);

            // TODO: https://github.com/dotnet/roslyn/issues/37125
            Assert.Throws<ArgumentNullException>(() => solution.WithDocumentFilePath(documentId, filePath: null!));

            Assert.Throws<ArgumentNullException>(() => solution.WithDocumentFilePath(null!, path));
            Assert.Throws<InvalidOperationException>(() => solution.WithDocumentFilePath(s_unrelatedDocumentId, path));
        }

        [Fact, Trait(Traits.Feature, Traits.Features.Workspace)]
        public void WithSourceCodeKind()
        {
            var solution = CreateSolutionWithProjectAndDocuments();
            var documentId = solution.Projects.Single().DocumentIds.Single();

            Assert.Same(solution, solution.WithDocumentSourceCodeKind(documentId, SourceCodeKind.Regular));

            var newSolution1 = solution.WithDocumentSourceCodeKind(documentId, SourceCodeKind.Script);
            Assert.Equal(SourceCodeKind.Script, newSolution1.GetDocument(documentId)!.SourceCodeKind);

            Assert.Throws<ArgumentOutOfRangeException>(() => solution.WithDocumentSourceCodeKind(documentId, (SourceCodeKind)(-1)));

            Assert.Throws<ArgumentNullException>(() => solution.WithDocumentSourceCodeKind(null!, SourceCodeKind.Script));
            Assert.Throws<InvalidOperationException>(() => solution.WithDocumentSourceCodeKind(s_unrelatedDocumentId, SourceCodeKind.Script));
        }

        [Fact, Trait(Traits.Feature, Traits.Features.Workspace), Obsolete]
        public void WithSourceCodeKind_Obsolete()
        {
            var solution = CreateSolutionWithProjectAndDocuments();
            var documentId = solution.Projects.Single().DocumentIds.Single();

            var newSolution = solution.WithDocumentSourceCodeKind(documentId, SourceCodeKind.Interactive);
            Assert.Equal(SourceCodeKind.Script, newSolution.GetDocument(documentId)!.SourceCodeKind);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.Workspace)]
        public void WithDocumentSyntaxRoot()
        {
            var solution = CreateSolutionWithProjectAndDocuments();
            var documentId = solution.Projects.Single().DocumentIds.Single();
            var root = CS.SyntaxFactory.ParseSyntaxTree("class NewClass {}").GetRoot();

            var newSolution1 = solution.WithDocumentSyntaxRoot(documentId, root, PreservationMode.PreserveIdentity);
            Assert.True(newSolution1.GetDocument(documentId)!.TryGetSyntaxRoot(out var actualRoot));
            Assert.Equal(root.ToString(), actualRoot!.ToString());

            // the actual root has a new parent SyntaxTree:
            Assert.NotSame(root, actualRoot);

            var newSolution2 = newSolution1.WithDocumentSyntaxRoot(documentId, actualRoot);
            Assert.Same(newSolution1, newSolution2);

            Assert.Throws<ArgumentOutOfRangeException>(() => solution.WithDocumentSyntaxRoot(documentId, root, (PreservationMode)(-1)));

            Assert.Throws<ArgumentNullException>(() => solution.WithDocumentSyntaxRoot(null!, root));
            Assert.Throws<InvalidOperationException>(() => solution.WithDocumentSyntaxRoot(s_unrelatedDocumentId, root));
        }

        [Fact, Trait(Traits.Feature, Traits.Features.Workspace)]
        [WorkItem(37125, "https://github.com/dotnet/roslyn/issues/41940")]
        public void WithDocumentSyntaxRoot_AnalyzerConfigWithoutFilePath()
        {
            var projectId = ProjectId.CreateNewId();

            var solution = CreateSolution()
                .AddProject(projectId, "goo", "goo.dll", LanguageNames.CSharp)
                .AddDocument(DocumentId.CreateNewId(projectId), "goo.cs", "public class Goo { }")
                .AddAnalyzerConfigDocument(DocumentId.CreateNewId(projectId), "editorcfg", SourceText.From("config"));

            var documentId = solution.Projects.Single().DocumentIds.Single();

            var root = CS.SyntaxFactory.ParseSyntaxTree("class NewClass {}", path: "").GetRoot();
            Assert.Throws<ArgumentException>(() => solution.WithDocumentSyntaxRoot(documentId, root));
        }

        [Fact, Trait(Traits.Feature, Traits.Features.Workspace)]
        public void WithDocumentText_SourceText()
        {
            var solution = CreateSolutionWithProjectAndDocuments();
            var documentId = solution.Projects.Single().DocumentIds.Single();
            var text = SourceText.From("new text");

            var newSolution1 = solution.WithDocumentText(documentId, text, PreservationMode.PreserveIdentity);
            Assert.True(newSolution1.GetDocument(documentId)!.TryGetText(out var actualText));
            Assert.Same(text, actualText);

            var newSolution2 = newSolution1.WithDocumentText(documentId, text, PreservationMode.PreserveIdentity);
            Assert.Same(newSolution1, newSolution2);

            Assert.Throws<ArgumentNullException>(() => solution.WithDocumentText(documentId, (SourceText)null!, PreservationMode.PreserveIdentity));
            Assert.Throws<ArgumentOutOfRangeException>(() => solution.WithDocumentText(documentId, text, (PreservationMode)(-1)));

            Assert.Throws<ArgumentNullException>(() => solution.WithDocumentText((DocumentId)null!, text, PreservationMode.PreserveIdentity));
            Assert.Throws<InvalidOperationException>(() => solution.WithDocumentText(s_unrelatedDocumentId, text, PreservationMode.PreserveIdentity));
        }

        [Fact, Trait(Traits.Feature, Traits.Features.Workspace)]
        public void WithDocumentText_TextAndVersion()
        {
            var solution = CreateSolutionWithProjectAndDocuments();
            var documentId = solution.Projects.Single().DocumentIds.Single();
            var textAndVersion = TextAndVersion.Create(SourceText.From("new text"), VersionStamp.Default);

            var newSolution1 = solution.WithDocumentText(documentId, textAndVersion, PreservationMode.PreserveIdentity);
            Assert.True(newSolution1.GetDocument(documentId)!.TryGetText(out var actualText));
            Assert.True(newSolution1.GetDocument(documentId)!.TryGetTextVersion(out var actualVersion));
            Assert.Same(textAndVersion.Text, actualText);
            Assert.Equal(textAndVersion.Version, actualVersion);

            var newSolution2 = newSolution1.WithDocumentText(documentId, textAndVersion, PreservationMode.PreserveIdentity);
            Assert.Same(newSolution1, newSolution2);

            Assert.Throws<ArgumentNullException>(() => solution.WithDocumentText(documentId, (SourceText)null!, PreservationMode.PreserveIdentity));
            Assert.Throws<ArgumentOutOfRangeException>(() => solution.WithDocumentText(documentId, textAndVersion, (PreservationMode)(-1)));

            Assert.Throws<ArgumentNullException>(() => solution.WithDocumentText((DocumentId)null!, textAndVersion, PreservationMode.PreserveIdentity));
            Assert.Throws<InvalidOperationException>(() => solution.WithDocumentText(s_unrelatedDocumentId, textAndVersion, PreservationMode.PreserveIdentity));
        }

        [Fact, Trait(Traits.Feature, Traits.Features.Workspace)]
        public void WithDocumentText_MultipleDocuments()
        {
            var solution = CreateSolutionWithProjectAndDocuments();
            var documentId = solution.Projects.Single().DocumentIds.Single();
            var text = SourceText.From("new text");

            var newSolution1 = solution.WithDocumentText(new[] { documentId }, text, PreservationMode.PreserveIdentity);
            Assert.True(newSolution1.GetDocument(documentId)!.TryGetText(out var actualText));
            Assert.Same(text, actualText);

            var newSolution2 = newSolution1.WithDocumentText(new[] { documentId }, text, PreservationMode.PreserveIdentity);
            Assert.Same(newSolution1, newSolution2);

            // documents not in solution are skipped: https://github.com/dotnet/roslyn/issues/42029
            Assert.Same(solution, solution.WithDocumentText(new DocumentId[] { null! }, text));
            Assert.Same(solution, solution.WithDocumentText(new DocumentId[] { s_unrelatedDocumentId }, text));

            Assert.Throws<ArgumentNullException>(() => solution.WithDocumentText((DocumentId[])null!, text, PreservationMode.PreserveIdentity));
            Assert.Throws<ArgumentNullException>(() => solution.WithDocumentText(new[] { documentId }, null!, PreservationMode.PreserveIdentity));
            Assert.Throws<ArgumentOutOfRangeException>(() => solution.WithDocumentText(new[] { documentId }, text, (PreservationMode)(-1)));
        }

        [Fact, Trait(Traits.Feature, Traits.Features.Workspace)]
        public void WithAdditionalDocumentText_SourceText()
        {
            var solution = CreateSolutionWithProjectAndDocuments();
            var documentId = solution.Projects.Single().AdditionalDocumentIds.Single();
            var text = SourceText.From("new text");

            var newSolution1 = solution.WithAdditionalDocumentText(documentId, text, PreservationMode.PreserveIdentity);
            Assert.True(newSolution1.GetAdditionalDocument(documentId)!.TryGetText(out var actualText));
            Assert.Same(text, actualText);

            var newSolution2 = newSolution1.WithAdditionalDocumentText(documentId, text, PreservationMode.PreserveIdentity);
            Assert.Same(newSolution1, newSolution2);

            Assert.Throws<ArgumentNullException>(() => solution.WithAdditionalDocumentText(documentId, (SourceText)null!, PreservationMode.PreserveIdentity));
            Assert.Throws<ArgumentOutOfRangeException>(() => solution.WithAdditionalDocumentText(documentId, text, (PreservationMode)(-1)));

            Assert.Throws<ArgumentNullException>(() => solution.WithAdditionalDocumentText((DocumentId)null!, text, PreservationMode.PreserveIdentity));
            Assert.Throws<InvalidOperationException>(() => solution.WithAdditionalDocumentText(s_unrelatedDocumentId, text, PreservationMode.PreserveIdentity));
        }

        [Fact, Trait(Traits.Feature, Traits.Features.Workspace)]
        public void WithAdditionalDocumentText_TextAndVersion()
        {
            var solution = CreateSolutionWithProjectAndDocuments();
            var documentId = solution.Projects.Single().AdditionalDocumentIds.Single();
            var textAndVersion = TextAndVersion.Create(SourceText.From("new text"), VersionStamp.Default);

            var newSolution1 = solution.WithAdditionalDocumentText(documentId, textAndVersion, PreservationMode.PreserveIdentity);
            Assert.True(newSolution1.GetAdditionalDocument(documentId)!.TryGetText(out var actualText));
            Assert.True(newSolution1.GetAdditionalDocument(documentId)!.TryGetTextVersion(out var actualVersion));
            Assert.Same(textAndVersion.Text, actualText);
            Assert.Equal(textAndVersion.Version, actualVersion);

            var newSolution2 = newSolution1.WithAdditionalDocumentText(documentId, textAndVersion, PreservationMode.PreserveIdentity);
            Assert.Same(newSolution1, newSolution2);

            Assert.Throws<ArgumentNullException>(() => solution.WithAdditionalDocumentText(documentId, (SourceText)null!, PreservationMode.PreserveIdentity));
            Assert.Throws<ArgumentOutOfRangeException>(() => solution.WithAdditionalDocumentText(documentId, textAndVersion, (PreservationMode)(-1)));

            Assert.Throws<ArgumentNullException>(() => solution.WithAdditionalDocumentText((DocumentId)null!, textAndVersion, PreservationMode.PreserveIdentity));
            Assert.Throws<InvalidOperationException>(() => solution.WithAdditionalDocumentText(s_unrelatedDocumentId, textAndVersion, PreservationMode.PreserveIdentity));
        }

        [Fact, Trait(Traits.Feature, Traits.Features.Workspace)]
        public void WithAnalyzerConfigDocumentText_SourceText()
        {
            var solution = CreateSolutionWithProjectAndDocuments();
            var documentId = solution.Projects.Single().State.AnalyzerConfigDocumentStates.Single().Key;
            var text = SourceText.From("new text");

            var newSolution1 = solution.WithAnalyzerConfigDocumentText(documentId, text, PreservationMode.PreserveIdentity);
            Assert.True(newSolution1.GetAnalyzerConfigDocument(documentId)!.TryGetText(out var actualText));
            Assert.Same(text, actualText);

            var newSolution2 = newSolution1.WithAnalyzerConfigDocumentText(documentId, text, PreservationMode.PreserveIdentity);
            Assert.Same(newSolution1, newSolution2);

            Assert.Throws<ArgumentNullException>(() => solution.WithAnalyzerConfigDocumentText(documentId, (SourceText)null!, PreservationMode.PreserveIdentity));
            Assert.Throws<ArgumentOutOfRangeException>(() => solution.WithAnalyzerConfigDocumentText(documentId, text, (PreservationMode)(-1)));

            Assert.Throws<ArgumentNullException>(() => solution.WithAnalyzerConfigDocumentText((DocumentId)null!, text, PreservationMode.PreserveIdentity));
            Assert.Throws<InvalidOperationException>(() => solution.WithAnalyzerConfigDocumentText(s_unrelatedDocumentId, text, PreservationMode.PreserveIdentity));
        }

        [Fact, Trait(Traits.Feature, Traits.Features.Workspace)]
        public void WithAnalyzerConfigDocumentText_TextAndVersion()
        {
            var solution = CreateSolutionWithProjectAndDocuments();
            var documentId = solution.Projects.Single().State.AnalyzerConfigDocumentStates.Single().Key;
            var textAndVersion = TextAndVersion.Create(SourceText.From("new text"), VersionStamp.Default);

            var newSolution1 = solution.WithAnalyzerConfigDocumentText(documentId, textAndVersion, PreservationMode.PreserveIdentity);
            Assert.True(newSolution1.GetAnalyzerConfigDocument(documentId)!.TryGetText(out var actualText));
            Assert.True(newSolution1.GetAnalyzerConfigDocument(documentId)!.TryGetTextVersion(out var actualVersion));
            Assert.Same(textAndVersion.Text, actualText);
            Assert.Equal(textAndVersion.Version, actualVersion);

            var newSolution2 = newSolution1.WithAnalyzerConfigDocumentText(documentId, textAndVersion, PreservationMode.PreserveIdentity);
            Assert.Same(newSolution1, newSolution2);

            Assert.Throws<ArgumentNullException>(() => solution.WithAnalyzerConfigDocumentText(documentId, (SourceText)null!, PreservationMode.PreserveIdentity));
            Assert.Throws<ArgumentOutOfRangeException>(() => solution.WithAnalyzerConfigDocumentText(documentId, textAndVersion, (PreservationMode)(-1)));

            Assert.Throws<ArgumentNullException>(() => solution.WithAnalyzerConfigDocumentText((DocumentId)null!, textAndVersion, PreservationMode.PreserveIdentity));
            Assert.Throws<InvalidOperationException>(() => solution.WithAnalyzerConfigDocumentText(s_unrelatedDocumentId, textAndVersion, PreservationMode.PreserveIdentity));
        }

        [Fact, Trait(Traits.Feature, Traits.Features.Workspace)]
        public void WithDocumentTextLoader()
        {
            var solution = CreateSolutionWithProjectAndDocuments();
            var documentId = solution.Projects.Single().DocumentIds.Single();
            var loader = new TestTextLoader("new text");

            var newSolution1 = solution.WithDocumentTextLoader(documentId, loader, PreservationMode.PreserveIdentity);
            Assert.Equal("new text", newSolution1.GetDocument(documentId)!.GetTextSynchronously(CancellationToken.None).ToString());

            // Reusal is not currently implemented: https://github.com/dotnet/roslyn/issues/42028
            var newSolution2 = solution.WithDocumentTextLoader(documentId, loader, PreservationMode.PreserveIdentity);
            Assert.NotSame(newSolution1, newSolution2);

            Assert.Throws<ArgumentNullException>(() => solution.WithDocumentTextLoader(documentId, null!, PreservationMode.PreserveIdentity));
            Assert.Throws<ArgumentOutOfRangeException>(() => solution.WithDocumentTextLoader(documentId, loader, (PreservationMode)(-1)));

            Assert.Throws<ArgumentNullException>(() => solution.WithDocumentTextLoader(null!, loader, PreservationMode.PreserveIdentity));
            Assert.Throws<InvalidOperationException>(() => solution.WithDocumentTextLoader(s_unrelatedDocumentId, loader, PreservationMode.PreserveIdentity));
        }

        [Fact, Trait(Traits.Feature, Traits.Features.Workspace)]
        public void WithAdditionalDocumentTextLoader()
        {
            var solution = CreateSolutionWithProjectAndDocuments();
            var documentId = solution.Projects.Single().AdditionalDocumentIds.Single();
            var loader = new TestTextLoader("new text");

            var newSolution1 = solution.WithAdditionalDocumentTextLoader(documentId, loader, PreservationMode.PreserveIdentity);
            Assert.Equal("new text", newSolution1.GetAdditionalDocument(documentId)!.GetTextSynchronously(CancellationToken.None).ToString());

            // Reusal is not currently implemented: https://github.com/dotnet/roslyn/issues/42028
            var newSolution2 = solution.WithAdditionalDocumentTextLoader(documentId, loader, PreservationMode.PreserveIdentity);
            Assert.NotSame(newSolution1, newSolution2);

            Assert.Throws<ArgumentNullException>(() => solution.WithAdditionalDocumentTextLoader(documentId, null!, PreservationMode.PreserveIdentity));
            Assert.Throws<ArgumentOutOfRangeException>(() => solution.WithAdditionalDocumentTextLoader(documentId, loader, (PreservationMode)(-1)));

            Assert.Throws<ArgumentNullException>(() => solution.WithAdditionalDocumentTextLoader(null!, loader, PreservationMode.PreserveIdentity));
            Assert.Throws<InvalidOperationException>(() => solution.WithAdditionalDocumentTextLoader(s_unrelatedDocumentId, loader, PreservationMode.PreserveIdentity));
        }

        [Fact, Trait(Traits.Feature, Traits.Features.Workspace)]
        public void WithAnalyzerConfigDocumentTextLoader()
        {
            var solution = CreateSolutionWithProjectAndDocuments();
            var documentId = solution.Projects.Single().State.AnalyzerConfigDocumentStates.Single().Key;
            var loader = new TestTextLoader("new text");

            var newSolution1 = solution.WithAnalyzerConfigDocumentTextLoader(documentId, loader, PreservationMode.PreserveIdentity);
            Assert.Equal("new text", newSolution1.GetAnalyzerConfigDocument(documentId)!.GetTextSynchronously(CancellationToken.None).ToString());

            // Reusal is not currently implemented: https://github.com/dotnet/roslyn/issues/42028
            var newSolution2 = solution.WithAnalyzerConfigDocumentTextLoader(documentId, loader, PreservationMode.PreserveIdentity);
            Assert.NotSame(newSolution1, newSolution2);

            Assert.Throws<ArgumentNullException>(() => solution.WithAnalyzerConfigDocumentTextLoader(documentId, null!, PreservationMode.PreserveIdentity));
            Assert.Throws<ArgumentOutOfRangeException>(() => solution.WithAnalyzerConfigDocumentTextLoader(documentId, loader, (PreservationMode)(-1)));

            Assert.Throws<ArgumentNullException>(() => solution.WithAnalyzerConfigDocumentTextLoader(null!, loader, PreservationMode.PreserveIdentity));
            Assert.Throws<InvalidOperationException>(() => solution.WithAnalyzerConfigDocumentTextLoader(s_unrelatedDocumentId, loader, PreservationMode.PreserveIdentity));
        }
#nullable restore
        [Fact, Trait(Traits.Feature, Traits.Features.Workspace)]
        public void TestAddProject()
        {
            var sol = CreateSolution();
            var pid = ProjectId.CreateNewId();
            sol = sol.AddProject(pid, "goo", "goo.dll", LanguageNames.CSharp);
            Assert.True(sol.ProjectIds.Any(), "Solution was expected to have projects");
            Assert.NotNull(pid);
            var project = sol.GetProject(pid);
            Assert.False(project.HasDocuments);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.Workspace)]
        public void TestUpdateAssemblyName()
        {
            var solution = CreateSolution();
            var project1 = ProjectId.CreateNewId();
            solution = solution.AddProject(project1, "goo", "goo.dll", LanguageNames.CSharp);
            solution = solution.WithProjectAssemblyName(project1, "bar");
            var project = solution.GetProject(project1);
            Assert.Equal("bar", project.AssemblyName);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.Workspace)]
        [WorkItem(543964, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/543964")]
        public void MultipleProjectsWithSameDisplayName()
        {
            var solution = CreateSolution();
            var project1 = ProjectId.CreateNewId();
            var project2 = ProjectId.CreateNewId();
            solution = solution.AddProject(project1, "name", "assemblyName", LanguageNames.CSharp);
            solution = solution.AddProject(project2, "name", "assemblyName", LanguageNames.CSharp);
            Assert.Equal(2, solution.GetProjectsByName("name").Count());
        }

        [Fact, Trait(Traits.Feature, Traits.Features.Workspace)]
        public async Task<Solution> TestAddFirstDocumentAsync()
        {
            var pid = ProjectId.CreateNewId();
            var did = DocumentId.CreateNewId(pid);
            var sol = CreateSolution()
                .AddProject(pid, "goo", "goo.dll", LanguageNames.CSharp)
                .AddDocument(did, "goo.cs", "public class Goo { }");

            // verify project & document
            Assert.NotNull(pid);
            var project = sol.GetProject(pid);
            Assert.NotNull(project);
            Assert.True(sol.ContainsProject(pid), "Solution was expected to have project " + pid);
            Assert.True(project.HasDocuments, "Project was expected to have documents");
            Assert.Equal(project, sol.GetProject(pid));
            Assert.NotNull(did);
            var document = sol.GetDocument(did);
            Assert.True(project.ContainsDocument(did), "Project was expected to have document " + did);
            Assert.Equal(document, project.GetDocument(did));
            Assert.Equal(document, sol.GetDocument(did));
            var semantics = await document.GetSemanticModelAsync();
            Assert.NotNull(semantics);

            await ValidateSolutionAndCompilationsAsync(sol);

            return sol;
        }

        [Fact, Trait(Traits.Feature, Traits.Features.Workspace)]
        public async Task TestAddSecondDocumentAsync()
        {
            var sol = await TestAddFirstDocumentAsync();
            var pid = sol.Projects.Single().Id;
            var did = DocumentId.CreateNewId(pid);
            sol = sol.AddDocument(did, "bar.cs", "public class Bar { }");

            // verify project & document
            var project = sol.GetProject(pid);
            Assert.NotNull(project);
            Assert.NotNull(did);
            var document = sol.GetDocument(did);
            Assert.True(project.ContainsDocument(did), "Project was expected to have document " + did);
            Assert.Equal(document, project.GetDocument(did));
            Assert.Equal(document, sol.GetDocument(did));

            await ValidateSolutionAndCompilationsAsync(sol);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.Workspace)]
        public async Task AddTwoDocumentsForSingleProject()
        {
            var projectId = ProjectId.CreateNewId();

            var documentInfo1 = DocumentInfo.Create(DocumentId.CreateNewId(projectId), "file1.cs");
            var documentInfo2 = DocumentInfo.Create(DocumentId.CreateNewId(projectId), "file2.cs");

            var solution = CreateSolution()
                .AddProject(projectId, "goo", "goo.dll", LanguageNames.CSharp)
                .AddDocuments(ImmutableArray.Create(documentInfo1, documentInfo2));

            var project = Assert.Single(solution.Projects);

            var document1 = project.GetDocument(documentInfo1.Id);
            var document2 = project.GetDocument(documentInfo2.Id);

            Assert.NotSame(document1, document2);

            await ValidateSolutionAndCompilationsAsync(solution);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.Workspace)]
        public async Task AddTwoDocumentsForTwoProjects()
        {
            var projectId1 = ProjectId.CreateNewId();
            var projectId2 = ProjectId.CreateNewId();

            var documentInfo1 = DocumentInfo.Create(DocumentId.CreateNewId(projectId1), "file1.cs");
            var documentInfo2 = DocumentInfo.Create(DocumentId.CreateNewId(projectId2), "file2.cs");

            var solution = CreateSolution()
                .AddProject(projectId1, "project1", "project1.dll", LanguageNames.CSharp)
                .AddProject(projectId2, "project2", "project2.dll", LanguageNames.CSharp)
                .AddDocuments(ImmutableArray.Create(documentInfo1, documentInfo2));

            var project1 = solution.GetProject(projectId1);
            var project2 = solution.GetProject(projectId2);

            var document1 = project1.GetDocument(documentInfo1.Id);
            var document2 = project2.GetDocument(documentInfo2.Id);

            Assert.NotSame(document1, document2);
            Assert.NotSame(document1.Project, document2.Project);

            await ValidateSolutionAndCompilationsAsync(solution);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.Workspace)]
        public void AddTwoDocumentsWithMissingProject()
        {
            var projectId1 = ProjectId.CreateNewId();
            var projectId2 = ProjectId.CreateNewId();

            var documentInfo1 = DocumentInfo.Create(DocumentId.CreateNewId(projectId1), "file1.cs");
            var documentInfo2 = DocumentInfo.Create(DocumentId.CreateNewId(projectId2), "file2.cs");

            // We're only adding the first project, but not the second one
            var solution = CreateSolution()
                .AddProject(projectId1, "project1", "project1.dll", LanguageNames.CSharp);

            Assert.ThrowsAny<InvalidOperationException>(() => solution.AddDocuments(ImmutableArray.Create(documentInfo1, documentInfo2)));
        }

        [Fact, Trait(Traits.Feature, Traits.Features.Workspace)]
        public void RemoveZeroDocuments()
        {
            var solution = CreateSolution();

            Assert.Same(solution, solution.RemoveDocuments(ImmutableArray<DocumentId>.Empty));
        }

        [Fact, Trait(Traits.Feature, Traits.Features.Workspace)]
        public async Task RemoveTwoDocuments()
        {
            var projectId = ProjectId.CreateNewId();

            var documentInfo1 = DocumentInfo.Create(DocumentId.CreateNewId(projectId), "file1.cs");
            var documentInfo2 = DocumentInfo.Create(DocumentId.CreateNewId(projectId), "file2.cs");

            var solution = CreateSolution()
                .AddProject(projectId, "project1", "project1.dll", LanguageNames.CSharp)
                .AddDocuments(ImmutableArray.Create(documentInfo1, documentInfo2));

            solution = solution.RemoveDocuments(ImmutableArray.Create(documentInfo1.Id, documentInfo2.Id));

            var finalProject = solution.Projects.Single();
            Assert.Empty(finalProject.Documents);
            Assert.Empty((await finalProject.GetCompilationAsync()).SyntaxTrees);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.Workspace)]
        public void RemoveTwoDocumentsFromDifferentProjects()
        {
            var projectId1 = ProjectId.CreateNewId();
            var projectId2 = ProjectId.CreateNewId();

            var documentInfo1 = DocumentInfo.Create(DocumentId.CreateNewId(projectId1), "file1.cs");
            var documentInfo2 = DocumentInfo.Create(DocumentId.CreateNewId(projectId2), "file2.cs");

            var solution = CreateSolution()
                .AddProject(projectId1, "project1", "project1.dll", LanguageNames.CSharp)
                .AddProject(projectId2, "project2", "project2.dll", LanguageNames.CSharp)
                .AddDocuments(ImmutableArray.Create(documentInfo1, documentInfo2));

            Assert.All(solution.Projects, p => Assert.Single(p.Documents));

            solution = solution.RemoveDocuments(ImmutableArray.Create(documentInfo1.Id, documentInfo2.Id));

            Assert.All(solution.Projects, p => Assert.Empty(p.Documents));
        }

        [Fact, Trait(Traits.Feature, Traits.Features.Workspace)]
        public void RemoveDocumentFromUnrelatedProject()
        {
            var projectId1 = ProjectId.CreateNewId();
            var projectId2 = ProjectId.CreateNewId();

            var documentInfo1 = DocumentInfo.Create(DocumentId.CreateNewId(projectId1), "file1.cs");

            var solution = CreateSolution()
                .AddProject(projectId1, "project1", "project1.dll", LanguageNames.CSharp)
                .AddProject(projectId2, "project2", "project2.dll", LanguageNames.CSharp)
                .AddDocument(documentInfo1);

            // This should throw if we're removing one document from the wrong project. Right now we don't test the RemoveDocument
            // API due to https://github.com/dotnet/roslyn/issues/41211.
            Assert.Throws<ArgumentException>(() => solution.GetProject(projectId2).RemoveDocuments(ImmutableArray.Create(documentInfo1.Id)));
        }

        [Fact, Trait(Traits.Feature, Traits.Features.Workspace)]
        public async Task TestOneCSharpProjectAsync()
        {
            var sol = CreateSolutionWithOneCSharpProject();
            await ValidateSolutionAndCompilationsAsync(sol);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.Workspace)]
        public async Task TestTwoCSharpProjectsAsync()
        {
            var sol = CreateSolutionWithTwoCSharpProjects();
            await ValidateSolutionAndCompilationsAsync(sol);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.Workspace)]
        public async Task TestCrossLanguageProjectsAsync()
        {
            var sol = CreateCrossLanguageSolution();
            await ValidateSolutionAndCompilationsAsync(sol);
        }

        private Solution CreateSolutionWithOneCSharpProject()
        {
            return this.CreateSolution()
                       .AddProject("goo", "goo.dll", LanguageNames.CSharp)
                       .AddMetadataReference(s_mscorlib)
                       .AddDocument("goo.cs", "public class Goo { }")
                       .Project.Solution;
        }

        private Solution CreateSolutionWithTwoCSharpProjects()
        {
            var pm1 = ProjectId.CreateNewId();
            var pm2 = ProjectId.CreateNewId();
            var doc1 = DocumentId.CreateNewId(pm1);
            var doc2 = DocumentId.CreateNewId(pm2);
            return this.CreateSolution()
                       .AddProject(pm1, "goo", "goo.dll", LanguageNames.CSharp)
                       .AddProject(pm2, "bar", "bar.dll", LanguageNames.CSharp)
                       .AddProjectReference(pm2, new ProjectReference(pm1))
                       .AddDocument(doc1, "goo.cs", "public class Goo { }")
                       .AddDocument(doc2, "bar.cs", "public class Bar : Goo { }");
        }

        private Solution CreateCrossLanguageSolution()
        {
            var pm1 = ProjectId.CreateNewId();
            var pm2 = ProjectId.CreateNewId();
            return this.CreateSolution()
                       .AddProject(pm1, "goo", "goo.dll", LanguageNames.CSharp)
                       .AddMetadataReference(pm1, s_mscorlib)
                       .AddProject(pm2, "bar", "bar.dll", LanguageNames.VisualBasic)
                       .AddMetadataReference(pm2, s_mscorlib)
                       .AddProjectReference(pm2, new ProjectReference(pm1))
                       .AddDocument(DocumentId.CreateNewId(pm1), "goo.cs", "public class X { }")
                       .AddDocument(DocumentId.CreateNewId(pm2), "bar.vb", "Public Class Y\r\nInherits X\r\nEnd Class");
        }

        private async Task ValidateSolutionAndCompilationsAsync(Solution solution)
        {
            foreach (var project in solution.Projects)
            {
                Assert.True(solution.ContainsProject(project.Id), "Solution was expected to have project " + project.Id);
                Assert.Equal(project, solution.GetProject(project.Id));

                // these won't always be unique in real-world but should be for these tests
                Assert.Equal(project, solution.GetProjectsByName(project.Name).FirstOrDefault());

                var compilation = await project.GetCompilationAsync();
                Assert.NotNull(compilation);

                // check that the options are the same
                Assert.Equal(project.CompilationOptions, compilation.Options);

                // check that all known metadata references are present in the compilation
                foreach (var meta in project.MetadataReferences)
                {
                    Assert.True(compilation.References.Contains(meta), "Compilation references were expected to contain " + meta);
                }

                // check that all project-to-project reference metadata is present in the compilation
                foreach (var referenced in project.ProjectReferences)
                {
                    if (solution.ContainsProject(referenced.ProjectId))
                    {
                        var referencedMetadata = await solution.State.GetMetadataReferenceAsync(referenced, solution.GetProjectState(project.Id), CancellationToken.None);
                        Assert.NotNull(referencedMetadata);
                        if (referencedMetadata is CompilationReference compilationReference)
                        {
                            compilation.References.Single(r =>
                            {
                                var cr = r as CompilationReference;
                                return cr != null && cr.Compilation == compilationReference.Compilation;
                            });
                        }
                    }
                }

                // check that the syntax trees are the same
                var docs = project.Documents.ToList();
                var trees = compilation.SyntaxTrees.ToList();
                Assert.Equal(docs.Count, trees.Count);

                foreach (var doc in docs)
                {
                    Assert.True(trees.Contains(await doc.GetSyntaxTreeAsync()), "trees list was expected to contain the syntax tree of doc");
                }
            }
        }

#if false
        [Fact(Skip = "641963"), Trait(Traits.Feature, Traits.Features.Workspace)]
        public void TestDeepProjectReferenceTree()
        {
            int projectCount = 5;
            var solution = CreateSolutionWithProjectDependencyChain(projectCount);
            ProjectId[] projectIds = solution.ProjectIds.ToArray();

            Compilation compilation;
            for (int i = 0; i < projectCount; i++)
            {
                Assert.False(solution.GetProject(projectIds[i]).TryGetCompilation(out compilation));
            }

            var top = solution.GetCompilationAsync(projectIds.Last(), CancellationToken.None).Result;
            var partialSolution = solution.GetPartialSolution();
            for (int i = 0; i < projectCount; i++)
            {
                // While holding a compilation, we also hold its references, plus one further level
                // of references alive.  However, the references are only partial Declaration 
                // compilations
                var isPartialAvailable = i >= projectCount - 3;
                var isFinalAvailable = i == projectCount - 1;

                var projectId = projectIds[i];
                Assert.Equal(isFinalAvailable, solution.GetProject(projectId).TryGetCompilation(out compilation));
                Assert.Equal(isPartialAvailable, partialSolution.ProjectIds.Contains(projectId) && partialSolution.GetProject(projectId).TryGetCompilation(out compilation));
            }
        }
#endif

        private Solution CreateSolutionWithProjectDependencyChain(int projectCount)
        {
            var solution = this.CreateNotKeptAliveSolution();
            projectCount = 5;
            var projectIds = Enumerable.Range(0, projectCount).Select(i => ProjectId.CreateNewId()).ToArray();
            for (var i = 0; i < projectCount; i++)
            {
                solution = solution.AddProject(projectIds[i], i.ToString(), i.ToString(), LanguageNames.CSharp);
                if (i >= 1)
                {
                    solution = solution.AddProjectReference(projectIds[i], new ProjectReference(projectIds[i - 1]));
                }
            }

            return solution;
        }

        [WorkItem(636431, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/636431")]
        [Fact, Trait(Traits.Feature, Traits.Features.Workspace)]
        public async Task TestProjectDependencyLoadingAsync()
        {
            var projectCount = 3;
            var solution = CreateSolutionWithProjectDependencyChain(projectCount);
            var projectIds = solution.ProjectIds.ToArray();

            await solution.GetProject(projectIds[0]).GetCompilationAsync(CancellationToken.None);
            await solution.GetProject(projectIds[2]).GetCompilationAsync(CancellationToken.None);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.Workspace)]
        public async Task TestAddMetadataReferencesAsync()
        {
            var mefReference = TestReferences.NetFx.v4_0_30319.System_Core;
            var solution = CreateSolution();
            var project1 = ProjectId.CreateNewId();
            solution = solution.AddProject(project1, "goo", "goo.dll", LanguageNames.CSharp);
            solution = solution.AddMetadataReference(project1, s_mscorlib);

            solution = solution.AddMetadataReference(project1, mefReference);
            var assemblyReference = (IAssemblySymbol)solution.GetProject(project1).GetCompilationAsync().Result.GetAssemblyOrModuleSymbol(mefReference);
            var namespacesAndTypes = assemblyReference.GlobalNamespace.GetAllNamespacesAndTypes(CancellationToken.None);
            var foundSymbol = from symbol in namespacesAndTypes
                              where symbol.Name.Equals("Enumerable")
                              select symbol;
            Assert.Equal(1, foundSymbol.Count());
            solution = solution.RemoveMetadataReference(project1, mefReference);
            assemblyReference = (IAssemblySymbol)solution.GetProject(project1).GetCompilationAsync().Result.GetAssemblyOrModuleSymbol(mefReference);
            Assert.Null(assemblyReference);

            await ValidateSolutionAndCompilationsAsync(solution);
        }

        private class MockDiagnosticAnalyzer : DiagnosticAnalyzer
        {
            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public override void Initialize(AnalysisContext analysisContext)
            {
            }
        }

        [Fact, Trait(Traits.Feature, Traits.Features.Workspace)]
        public void TestProjectDiagnosticAnalyzers()
        {
            var solution = CreateSolution();
            var project1 = ProjectId.CreateNewId();
            solution = solution.AddProject(project1, "goo", "goo.dll", LanguageNames.CSharp);
            Assert.Empty(solution.Projects.Single().AnalyzerReferences);

            DiagnosticAnalyzer analyzer = new MockDiagnosticAnalyzer();
            var analyzerReference = new AnalyzerImageReference(ImmutableArray.Create(analyzer));

            // Test AddAnalyzer
            var newSolution = solution.AddAnalyzerReference(project1, analyzerReference);
            var actualAnalyzerReferences = newSolution.Projects.Single().AnalyzerReferences;
            Assert.Equal(1, actualAnalyzerReferences.Count);
            Assert.Equal(analyzerReference, actualAnalyzerReferences[0]);
            var actualAnalyzers = actualAnalyzerReferences[0].GetAnalyzersForAllLanguages();
            Assert.Equal(1, actualAnalyzers.Length);
            Assert.Equal(analyzer, actualAnalyzers[0]);

            // Test ProjectChanges
            var changes = newSolution.GetChanges(solution).GetProjectChanges().Single();
            var addedAnalyzerReference = changes.GetAddedAnalyzerReferences().Single();
            Assert.Equal(analyzerReference, addedAnalyzerReference);
            var removedAnalyzerReferences = changes.GetRemovedAnalyzerReferences();
            Assert.Empty(removedAnalyzerReferences);
            solution = newSolution;

            // Test RemoveAnalyzer
            solution = solution.RemoveAnalyzerReference(project1, analyzerReference);
            actualAnalyzerReferences = solution.Projects.Single().AnalyzerReferences;
            Assert.Empty(actualAnalyzerReferences);

            // Test AddAnalyzers
            analyzerReference = new AnalyzerImageReference(ImmutableArray.Create(analyzer));
            DiagnosticAnalyzer secondAnalyzer = new MockDiagnosticAnalyzer();
            var secondAnalyzerReference = new AnalyzerImageReference(ImmutableArray.Create(secondAnalyzer));
            var analyzerReferences = new[] { analyzerReference, secondAnalyzerReference };
            solution = solution.AddAnalyzerReferences(project1, analyzerReferences);
            actualAnalyzerReferences = solution.Projects.Single().AnalyzerReferences;
            Assert.Equal(2, actualAnalyzerReferences.Count);
            Assert.Equal(analyzerReference, actualAnalyzerReferences[0]);
            Assert.Equal(secondAnalyzerReference, actualAnalyzerReferences[1]);

            solution = solution.RemoveAnalyzerReference(project1, analyzerReference);
            actualAnalyzerReferences = solution.Projects.Single().AnalyzerReferences;
            Assert.Equal(1, actualAnalyzerReferences.Count);
            Assert.Equal(secondAnalyzerReference, actualAnalyzerReferences[0]);

            // Test WithAnalyzers
            solution = solution.WithProjectAnalyzerReferences(project1, analyzerReferences);
            actualAnalyzerReferences = solution.Projects.Single().AnalyzerReferences;
            Assert.Equal(2, actualAnalyzerReferences.Count);
            Assert.Equal(analyzerReference, actualAnalyzerReferences[0]);
            Assert.Equal(secondAnalyzerReference, actualAnalyzerReferences[1]);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.Workspace)]
        public void TestProjectCompilationOptions()
        {
            var solution = CreateSolution();
            var project1 = ProjectId.CreateNewId();
            solution = solution.AddProject(project1, "goo", "goo.dll", LanguageNames.CSharp);
            solution = solution.AddMetadataReference(project1, s_mscorlib);

            // Compilation Options
            var oldCompOptions = solution.GetProject(project1).CompilationOptions;
            var newCompOptions = new CSharpCompilationOptions(OutputKind.ConsoleApplication, mainTypeName: "After");
            solution = solution.WithProjectCompilationOptions(project1, newCompOptions);
            var newUpdatedCompOptions = solution.GetProject(project1).CompilationOptions;
            Assert.NotEqual(oldCompOptions, newUpdatedCompOptions);
            Assert.Same(newCompOptions, newUpdatedCompOptions);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.Workspace)]
        public void TestProjectParseOptions()
        {
            var solution = CreateSolution();
            var project1 = ProjectId.CreateNewId();
            solution = solution.AddProject(project1, "goo", "goo.dll", LanguageNames.CSharp);
            solution = solution.AddMetadataReference(project1, s_mscorlib);

            // Parse Options
            var oldParseOptions = solution.GetProject(project1).ParseOptions;
            var newParseOptions = new CSharpParseOptions(preprocessorSymbols: new[] { "AFTER" });
            solution = solution.WithProjectParseOptions(project1, newParseOptions);
            var newUpdatedParseOptions = solution.GetProject(project1).ParseOptions;
            Assert.NotEqual(oldParseOptions, newUpdatedParseOptions);
            Assert.Same(newParseOptions, newUpdatedParseOptions);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.Workspace)]
        public async Task TestRemoveProjectAsync()
        {
            var sol = CreateSolution();

            var pid = ProjectId.CreateNewId();
            sol = sol.AddProject(pid, "goo", "goo.dll", LanguageNames.CSharp);
            Assert.True(sol.ProjectIds.Any(), "Solution was expected to have projects");
            Assert.NotNull(pid);
            var project = sol.GetProject(pid);
            Assert.False(project.HasDocuments);

            var sol2 = sol.RemoveProject(pid);
            Assert.False(sol2.ProjectIds.Any());

            await ValidateSolutionAndCompilationsAsync(sol);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.Workspace)]
        public async Task TestRemoveProjectWithReferencesAsync()
        {
            var sol = CreateSolution();

            var pid = ProjectId.CreateNewId();
            var pid2 = ProjectId.CreateNewId();
            sol = sol.AddProject(pid, "goo", "goo.dll", LanguageNames.CSharp)
                   .AddProject(pid2, "bar", "bar.dll", LanguageNames.CSharp)
                   .AddProjectReference(pid2, new ProjectReference(pid));

            Assert.Equal(2, sol.Projects.Count());

            // remove the project that is being referenced
            // this should leave a dangling reference
            var sol2 = sol.RemoveProject(pid);

            Assert.False(sol2.ContainsProject(pid));
            Assert.True(sol2.ContainsProject(pid2), "sol2 was expected to contain project " + pid2);
            Assert.Equal(1, sol2.Projects.Count());
            Assert.True(sol2.GetProject(pid2).AllProjectReferences.Any(r => r.ProjectId == pid), "sol2 project pid2 was expected to contain project reference " + pid);

            await ValidateSolutionAndCompilationsAsync(sol2);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.Workspace)]
        public async Task TestRemoveProjectWithReferencesAndAddItBackAsync()
        {
            var sol = CreateSolution();

            var pid = ProjectId.CreateNewId();
            var pid2 = ProjectId.CreateNewId();
            sol = sol.AddProject(pid, "goo", "goo.dll", LanguageNames.CSharp)
                   .AddProject(pid2, "bar", "bar.dll", LanguageNames.CSharp)
                   .AddProjectReference(pid2, new ProjectReference(pid));

            Assert.Equal(2, sol.Projects.Count());

            // remove the project that is being referenced
            var sol2 = sol.RemoveProject(pid);

            Assert.False(sol2.ContainsProject(pid));
            Assert.True(sol2.ContainsProject(pid2), "sol2 was expected to contain project " + pid2);
            Assert.Equal(1, sol2.Projects.Count());
            Assert.True(sol2.GetProject(pid2).AllProjectReferences.Any(r => r.ProjectId == pid), "sol2 pid2 was expected to contain " + pid);

            var sol3 = sol2.AddProject(pid, "goo", "goo.dll", LanguageNames.CSharp);

            Assert.True(sol3.ContainsProject(pid), "sol3 was expected to contain " + pid);
            Assert.True(sol3.ContainsProject(pid2), "sol3 was expected to contain " + pid2);
            Assert.Equal(2, sol3.Projects.Count());

            await ValidateSolutionAndCompilationsAsync(sol3);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.Workspace)]
        public async Task TestGetSyntaxRootAsync()
        {
            var text = "public class Goo { }";

            var pid = ProjectId.CreateNewId();
            var did = DocumentId.CreateNewId(pid);

            var sol = CreateSolution()
                .AddProject(pid, "goo", "goo.dll", LanguageNames.CSharp)
                .AddDocument(did, "goo.cs", text);

            var document = sol.GetDocument(did);
            Assert.False(document.TryGetSyntaxRoot(out var root));

            root = await document.GetSyntaxRootAsync();
            Assert.NotNull(root);
            Assert.Equal(text, root.ToString());

            Assert.True(document.TryGetSyntaxRoot(out root));
            Assert.NotNull(root);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.Workspace)]
        public async Task TestUpdateDocumentAsync()
        {
            var projectId = ProjectId.CreateNewId();
            var documentId = DocumentId.CreateNewId(projectId);

            var solution1 = CreateSolution()
                .AddProject(projectId, "ProjectName", "AssemblyName", LanguageNames.CSharp)
                .AddDocument(documentId, "DocumentName", SourceText.From("class Class{}"));

            var document = solution1.GetDocument(documentId);
            var newRoot = await Formatter.FormatAsync(document).Result.GetSyntaxRootAsync();
            var solution2 = solution1.WithDocumentSyntaxRoot(documentId, newRoot);

            Assert.NotEqual(solution1, solution2);

            var newText = solution2.GetDocument(documentId).GetTextAsync().Result.ToString();
            Assert.Equal("class Class { }", newText);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.Workspace)]
        public void TestUpdateSyntaxTreeWithAnnotations()
        {
            var text = "public class Goo { }";

            var pid = ProjectId.CreateNewId();
            var did = DocumentId.CreateNewId(pid);

            var sol = CreateSolution()
                .AddProject(pid, "goo", "goo.dll", LanguageNames.CSharp)
                .AddDocument(did, "goo.cs", text);

            var document = sol.GetDocument(did);
            var tree = document.GetSyntaxTreeAsync().Result;
            var root = tree.GetRoot();

            var annotation = new SyntaxAnnotation();
            var annotatedRoot = root.WithAdditionalAnnotations(annotation);

            var sol2 = sol.WithDocumentSyntaxRoot(did, annotatedRoot);
            var doc2 = sol2.GetDocument(did);
            var tree2 = doc2.GetSyntaxTreeAsync().Result;
            var root2 = tree2.GetRoot();
            // text should not be available yet (it should be defer created from the node)
            // and getting the document or root should not cause it to be created.
            Assert.False(tree2.TryGetText(out var text2));

            text2 = tree2.GetText();
            Assert.NotNull(text2);

            Assert.NotSame(tree, tree2);
            Assert.NotSame(annotatedRoot, root2);

            Assert.True(annotatedRoot.IsEquivalentTo(root2));
            Assert.True(root2.HasAnnotation(annotation));
        }

        [Fact, Trait(Traits.Feature, Traits.Features.Workspace)]
        public void TestUpdatingFilePathUpdatesSyntaxTree()
        {
            var projectId = ProjectId.CreateNewId();
            var documentId = DocumentId.CreateNewId(projectId);

            const string OldFilePath = @"Z:\OldFilePath.cs";
            const string NewFilePath = @"Z:\NewFilePath.cs";

            var solution = CreateSolution()
                .AddProject(projectId, "goo", "goo.dll", LanguageNames.CSharp)
                .AddDocument(documentId, "OldFilePath.cs", "public class Goo { }", filePath: OldFilePath);

            // scope so later asserts don't accidentally use oldDocument
            {
                var oldDocument = solution.GetDocument(documentId);
                Assert.Equal(OldFilePath, oldDocument.FilePath);
                Assert.Equal(OldFilePath, oldDocument.GetSyntaxTreeAsync().Result.FilePath);
            }

            solution = solution.WithDocumentFilePath(documentId, NewFilePath);

            {
                var newDocument = solution.GetDocument(documentId);
                Assert.Equal(NewFilePath, newDocument.FilePath);
                Assert.Equal(NewFilePath, newDocument.GetSyntaxTreeAsync().Result.FilePath);
            }
        }

        [Fact(Skip = "https://github.com/dotnet/roslyn/issues/13433"), Trait(Traits.Feature, Traits.Features.Workspace)]
        public void TestSyntaxRootNotKeptAlive()
        {
            var pid = ProjectId.CreateNewId();
            var did = DocumentId.CreateNewId(pid);

            var sol = CreateNotKeptAliveSolution()
                .AddProject(pid, "goo", "goo.dll", LanguageNames.CSharp)
                .AddDocument(did, "goo.cs", "public class Goo { }");

            var observedRoot = GetObservedSyntaxTreeRoot(sol, did);
            observedRoot.AssertReleased();

            // re-get the tree (should recover from storage, not reparse)
            var root = sol.GetDocument(did).GetSyntaxRootAsync().Result;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [Fact, Trait(Traits.Feature, Traits.Features.Workspace)]
        [WorkItem(542736, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/542736")]
        public void TestDocumentChangedOnDiskIsNotObserved()
        {
            var text1 = "public class A {}";
            var text2 = "public class B {}";

            var file = Temp.CreateFile().WriteAllText(text1, Encoding.UTF8);

            // create a solution that evicts from the cache immediately.
            var sol = CreateNotKeptAliveSolution();

            var pid = ProjectId.CreateNewId();
            var did = DocumentId.CreateNewId(pid);

            sol = sol.AddProject(pid, "goo", "goo.dll", LanguageNames.CSharp)
                     .AddDocument(did, "x", new FileTextLoader(file.Path, Encoding.UTF8));

            var observedText = GetObservedText(sol, did, text1);

            // change text on disk & verify it is changed
            file.WriteAllText(text2);
            var textOnDisk = file.ReadAllText();
            Assert.Equal(text2, textOnDisk);

            // stop observing it and let GC reclaim it
            observedText.AssertReleased();

            // if we ask for the same text again we should get the original content
            var observedText2 = sol.GetDocument(did).GetTextAsync().Result;
            Assert.Equal(text1, observedText2.ToString());
        }

        private Solution CreateNotKeptAliveSolution()
        {
            var workspace = new AdhocWorkspace(MefHostServices.Create(TestHost.Assemblies), "NotKeptAlive");
            workspace.TryApplyChanges(workspace.CurrentSolution.WithOptions(workspace.Options
                .WithChangedOption(CacheOptions.RecoverableTreeLengthThreshold, 0)));
            return workspace.CurrentSolution;
        }

        [Fact, Trait(Traits.Feature, Traits.Features.Workspace)]
        public void TestGetTextAsync()
        {
            var pid = ProjectId.CreateNewId();
            var did = DocumentId.CreateNewId(pid);

            var text = "public class C {}";
            var sol = CreateSolution()
                        .AddProject(pid, "goo", "goo.dll", LanguageNames.CSharp)
                        .AddDocument(did, "goo.cs", text);

            var doc = sol.GetDocument(did);

            var docText = doc.GetTextAsync().Result;

            Assert.NotNull(docText);
            Assert.Equal(text, docText.ToString());
        }

        [Fact, Trait(Traits.Feature, Traits.Features.Workspace)]
        public void TestGetLoadedTextAsync()
        {
            var pid = ProjectId.CreateNewId();
            var did = DocumentId.CreateNewId(pid);

            var text = "public class C {}";
            var file = Temp.CreateFile().WriteAllText(text, Encoding.UTF8);

            var sol = CreateSolution()
                        .AddProject(pid, "goo", "goo.dll", LanguageNames.CSharp)
                        .AddDocument(did, "x", new FileTextLoader(file.Path, Encoding.UTF8));

            var doc = sol.GetDocument(did);

            var docText = doc.GetTextAsync().Result;

            Assert.NotNull(docText);
            Assert.Equal(text, docText.ToString());
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [Fact(Skip = "https://github.com/dotnet/roslyn/issues/19427"), Trait(Traits.Feature, Traits.Features.Workspace)]
        public void TestGetRecoveredTextAsync()
        {
            var pid = ProjectId.CreateNewId();
            var did = DocumentId.CreateNewId(pid);

            var text = "public class C {}";
            var sol = CreateNotKeptAliveSolution()
                        .AddProject(pid, "goo", "goo.dll", LanguageNames.CSharp)
                        .AddDocument(did, "goo.cs", text);

            // observe the text and then wait for the references to be GC'd
            var observed = GetObservedText(sol, did, text);
            observed.AssertReleased();

            // get it async and force it to recover from temporary storage
            var doc = sol.GetDocument(did);
            var docText = doc.GetTextAsync().Result;

            Assert.NotNull(docText);
            Assert.Equal(text, docText.ToString());
        }

        [Fact, Trait(Traits.Feature, Traits.Features.Workspace)]
        public void TestGetSyntaxTreeAsync()
        {
            var pid = ProjectId.CreateNewId();
            var did = DocumentId.CreateNewId(pid);

            var text = "public class C {}";
            var sol = CreateSolution()
                        .AddProject(pid, "goo", "goo.dll", LanguageNames.CSharp)
                        .AddDocument(did, "goo.cs", text);

            var doc = sol.GetDocument(did);

            var docTree = doc.GetSyntaxTreeAsync().Result;

            Assert.NotNull(docTree);
            Assert.Equal(text, docTree.GetRoot().ToString());
        }

        [Fact, Trait(Traits.Feature, Traits.Features.Workspace)]
        public void TestGetSyntaxTreeFromLoadedTextAsync()
        {
            var pid = ProjectId.CreateNewId();
            var did = DocumentId.CreateNewId(pid);

            var text = "public class C {}";
            var file = Temp.CreateFile().WriteAllText(text, Encoding.UTF8);

            var sol = CreateSolution()
                        .AddProject(pid, "goo", "goo.dll", LanguageNames.CSharp)
                        .AddDocument(did, "x", new FileTextLoader(file.Path, Encoding.UTF8));

            var doc = sol.GetDocument(did);
            var docTree = doc.GetSyntaxTreeAsync().Result;

            Assert.NotNull(docTree);
            Assert.Equal(text, docTree.GetRoot().ToString());
        }

        [Fact, Trait(Traits.Feature, Traits.Features.Workspace)]
        public void TestGetSyntaxTreeFromAddedTree()
        {
            var pid = ProjectId.CreateNewId();
            var did = DocumentId.CreateNewId(pid);

            var tree = CSharp.SyntaxFactory.ParseSyntaxTree("public class C {}").GetRoot(CancellationToken.None);
            tree = tree.WithAdditionalAnnotations(new SyntaxAnnotation("test"));

            var sol = CreateSolution()
                        .AddProject(pid, "goo", "goo.dll", LanguageNames.CSharp)
                        .AddDocument(did, "x", tree);

            var doc = sol.GetDocument(did);
            var docTree = doc.GetSyntaxRootAsync().Result;

            Assert.NotNull(docTree);
            Assert.True(tree.IsEquivalentTo(docTree));
            Assert.NotNull(docTree.GetAnnotatedNodes("test").Single());
        }

        [Fact, Trait(Traits.Feature, Traits.Features.Workspace)]
        public async Task TestGetSyntaxRootAsync2Async()
        {
            var pid = ProjectId.CreateNewId();
            var did = DocumentId.CreateNewId(pid);

            var text = "public class C {}";
            var sol = CreateSolution()
                        .AddProject(pid, "goo", "goo.dll", LanguageNames.CSharp)
                        .AddDocument(did, "goo.cs", text);

            var doc = sol.GetDocument(did);

            var docRoot = await doc.GetSyntaxRootAsync();

            Assert.NotNull(docRoot);
            Assert.Equal(text, docRoot.ToString());
        }

        [Fact(Skip = "https://github.com/dotnet/roslyn/issues/14954"), Trait(Traits.Feature, Traits.Features.Workspace)]
        public void TestGetRecoveredSyntaxRootAsync()
        {
            var pid = ProjectId.CreateNewId();
            var did = DocumentId.CreateNewId(pid);

            var text = "public class C {}";

            var sol = CreateNotKeptAliveSolution()
                        .AddProject(pid, "goo", "goo.dll", LanguageNames.CSharp)
                        .AddDocument(did, "goo.cs", text);

            // observe the syntax tree root and wait for the references to be GC'd
            var observed = GetObservedSyntaxTreeRoot(sol, did);
            observed.AssertReleased();

            // get it async and force it to be recovered from storage
            var doc = sol.GetDocument(did);
            var docRoot = doc.GetSyntaxRootAsync().Result;

            Assert.NotNull(docRoot);
            Assert.Equal(text, docRoot.ToString());
        }

        [Fact, Trait(Traits.Feature, Traits.Features.Workspace)]
        public void TestGetCompilationAsync()
        {
            var pid = ProjectId.CreateNewId();
            var did = DocumentId.CreateNewId(pid);

            var text = "public class C {}";
            var sol = CreateSolution()
                        .AddProject(pid, "goo", "goo.dll", LanguageNames.CSharp)
                        .AddDocument(did, "goo.cs", text);

            var proj = sol.GetProject(pid);

            var compilation = proj.GetCompilationAsync().Result;

            Assert.NotNull(compilation);
            Assert.Equal(1, compilation.SyntaxTrees.Count());
        }

        [Fact, Trait(Traits.Feature, Traits.Features.Workspace)]
        public void TestGetSemanticModelAsync()
        {
            var pid = ProjectId.CreateNewId();
            var did = DocumentId.CreateNewId(pid);

            var text = "public class C {}";
            var sol = CreateSolution()
                        .AddProject(pid, "goo", "goo.dll", LanguageNames.CSharp)
                        .AddDocument(did, "goo.cs", text);

            var doc = sol.GetDocument(did);

            var docModel = doc.GetSemanticModelAsync().Result;
            Assert.NotNull(docModel);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [Fact(Skip = "https://github.com/dotnet/roslyn/issues/13433"), Trait(Traits.Feature, Traits.Features.Workspace)]
        public void TestGetTextDoesNotKeepTextAlive()
        {
            var pid = ProjectId.CreateNewId();
            var did = DocumentId.CreateNewId(pid);

            var text = "public class C {}";
            var sol = CreateNotKeptAliveSolution()
                        .AddProject(pid, "goo", "goo.dll", LanguageNames.CSharp)
                        .AddDocument(did, "goo.cs", text);

            // observe the text and then wait for the references to be GC'd
            var observed = GetObservedText(sol, did, text);
            observed.AssertReleased();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private ObjectReference<SourceText> GetObservedText(Solution solution, DocumentId documentId, string expectedText = null)
        {
            var observedText = solution.GetDocument(documentId).GetTextAsync().Result;

            if (expectedText != null)
            {
                Assert.Equal(expectedText, observedText.ToString());
            }

            return new ObjectReference<SourceText>(observedText);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [Fact(Skip = "https://github.com/dotnet/roslyn/issues/13433"), Trait(Traits.Feature, Traits.Features.Workspace)]
        public void TestGetTextAsyncDoesNotKeepTextAlive()
        {
            var pid = ProjectId.CreateNewId();
            var did = DocumentId.CreateNewId(pid);

            var text = "public class C {}";
            var sol = CreateNotKeptAliveSolution()
                        .AddProject(pid, "goo", "goo.dll", LanguageNames.CSharp)
                        .AddDocument(did, "goo.cs", text);

            // observe the text and then wait for the references to be GC'd
            var observed = GetObservedTextAsync(sol, did, text);
            observed.AssertReleased();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private ObjectReference<SourceText> GetObservedTextAsync(Solution solution, DocumentId documentId, string expectedText = null)
        {
            var observedText = solution.GetDocument(documentId).GetTextAsync().Result;

            if (expectedText != null)
            {
                Assert.Equal(expectedText, observedText.ToString());
            }

            return new ObjectReference<SourceText>(observedText);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [Fact(Skip = "https://github.com/dotnet/roslyn/issues/13433"), Trait(Traits.Feature, Traits.Features.Workspace)]
        public void TestGetSyntaxRootDoesNotKeepRootAlive()
        {
            var pid = ProjectId.CreateNewId();
            var did = DocumentId.CreateNewId(pid);

            var text = "public class C {}";

            var sol = CreateNotKeptAliveSolution()
                        .AddProject(pid, "goo", "goo.dll", LanguageNames.CSharp)
                        .AddDocument(did, "goo.cs", text);

            // get it async and wait for it to get GC'd
            var observed = GetObservedSyntaxTreeRoot(sol, did);
            observed.AssertReleased();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private ObjectReference<SyntaxNode> GetObservedSyntaxTreeRoot(Solution solution, DocumentId documentId)
        {
            var observedTree = solution.GetDocument(documentId).GetSyntaxRootAsync().Result;
            return new ObjectReference<SyntaxNode>(observedTree);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [Fact(Skip = "https://github.com/dotnet/roslyn/issues/13433"), Trait(Traits.Feature, Traits.Features.Workspace)]
        public void TestGetSyntaxRootAsyncDoesNotKeepRootAlive()
        {
            var pid = ProjectId.CreateNewId();
            var did = DocumentId.CreateNewId(pid);

            var text = "public class C {}";

            var sol = CreateNotKeptAliveSolution()
                        .AddProject(pid, "goo", "goo.dll", LanguageNames.CSharp)
                        .AddDocument(did, "goo.cs", text);

            // get it async and wait for it to get GC'd
            var observed = GetObservedSyntaxTreeRootAsync(sol, did);
            observed.AssertReleased();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private ObjectReference<SyntaxNode> GetObservedSyntaxTreeRootAsync(Solution solution, DocumentId documentId)
        {
            var observedTree = solution.GetDocument(documentId).GetSyntaxRootAsync().Result;
            return new ObjectReference<SyntaxNode>(observedTree);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [Fact(Skip = "https://github.com/dotnet/roslyn/issues/13506"), Trait(Traits.Feature, Traits.Features.Workspace)]
        [WorkItem(13506, "https://github.com/dotnet/roslyn/issues/13506")]
        public void TestRecoverableSyntaxTreeCSharp()
        {
            var pid = ProjectId.CreateNewId();
            var did = DocumentId.CreateNewId(pid);

            var text = @"public class C {
    public void Method1() {}
    public void Method2() {}
    public void Method3() {}
    public void Method4() {}
    public void Method5() {}
    public void Method6() {}
}";

            var sol = CreateNotKeptAliveSolution()
                        .AddProject(pid, "goo", "goo.dll", LanguageNames.CSharp)
                        .AddDocument(did, "goo.cs", text);

            TestRecoverableSyntaxTree(sol, did);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [Fact(Skip = "https://github.com/dotnet/roslyn/issues/13433"), Trait(Traits.Feature, Traits.Features.Workspace)]
        public void TestRecoverableSyntaxTreeVisualBasic()
        {
            var pid = ProjectId.CreateNewId();
            var did = DocumentId.CreateNewId(pid);

            var text = @"Public Class C
    Sub Method1()
    End Sub
    Sub Method2()
    End Sub
    Sub Method3()
    End Sub
    Sub Method4()
    End Sub
    Sub Method5()
    End Sub
    Sub Method6()
    End Sub
End Class";

            var sol = CreateNotKeptAliveSolution()
                        .AddProject(pid, "goo", "goo.dll", LanguageNames.VisualBasic)
                        .AddDocument(did, "goo.vb", text);

            TestRecoverableSyntaxTree(sol, did);
        }

        private void TestRecoverableSyntaxTree(Solution sol, DocumentId did)
        {
            // get it async and wait for it to get GC'd
            var observed = GetObservedSyntaxTreeRootAsync(sol, did);
            observed.AssertReleased();

            var doc = sol.GetDocument(did);

            // access the tree & root again (recover it)
            var tree = doc.GetSyntaxTreeAsync().Result;

            // this should cause reparsing
            var root = tree.GetRoot();

            // prove that the new root is correctly associated with the tree
            Assert.Equal(tree, root.SyntaxTree);

            // reset the syntax root, to make it 'refactored' by adding an attribute
            var newRoot = doc.GetSyntaxRootAsync().Result.WithAdditionalAnnotations(SyntaxAnnotation.ElasticAnnotation);
            var doc2 = doc.Project.Solution.WithDocumentSyntaxRoot(doc.Id, newRoot, PreservationMode.PreserveValue).GetDocument(doc.Id);

            // get it async and wait for it to get GC'd
            var observed2 = GetObservedSyntaxTreeRootAsync(doc2.Project.Solution, did);
            observed2.AssertReleased();

            // access the tree & root again (recover it)
            var tree2 = doc2.GetSyntaxTreeAsync().Result;

            // this should cause deserialization
            var root2 = tree2.GetRoot();

            // prove that the new root is correctly associated with the tree
            Assert.Equal(tree2, root2.SyntaxTree);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [Fact(Skip = "https://github.com/dotnet/roslyn/issues/13433"), Trait(Traits.Feature, Traits.Features.Workspace)]
        public void TestGetCompilationAsyncDoesNotKeepCompilationAlive()
        {
            var pid = ProjectId.CreateNewId();
            var did = DocumentId.CreateNewId(pid);

            var text = "public class C {}";
            var sol = CreateNotKeptAliveSolution()
                        .AddProject(pid, "goo", "goo.dll", LanguageNames.CSharp)
                        .AddDocument(did, "goo.cs", text);

            // get it async and wait for it to get GC'd
            var observed = GetObservedCompilationAsync(sol, pid);
            observed.AssertReleased();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private ObjectReference<Compilation> GetObservedCompilationAsync(Solution solution, ProjectId projectId)
        {
            var observed = solution.GetProject(projectId).GetCompilationAsync().Result;
            return new ObjectReference<Compilation>(observed);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [Fact(Skip = "https://github.com/dotnet/roslyn/issues/13433"), Trait(Traits.Feature, Traits.Features.Workspace)]
        public void TestGetCompilationDoesNotKeepCompilationAlive()
        {
            var pid = ProjectId.CreateNewId();
            var did = DocumentId.CreateNewId(pid);

            var text = "public class C {}";
            var sol = CreateNotKeptAliveSolution()
                        .AddProject(pid, "goo", "goo.dll", LanguageNames.CSharp)
                        .AddDocument(did, "goo.cs", text);

            // get it async and wait for it to get GC'd
            var observed = GetObservedCompilation(sol, pid);
            observed.AssertReleased();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private ObjectReference<Compilation> GetObservedCompilation(Solution solution, ProjectId projectId)
        {
            var observed = solution.GetProject(projectId).GetCompilationAsync().Result;
            return new ObjectReference<Compilation>(observed);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.Workspace)]
        public void TestWorkspaceLanguageServiceOverride()
        {
            var hostServices = MefHostServices.Create(TestHost.Assemblies);

            var ws = new AdhocWorkspace(hostServices, ServiceLayer.Host);
            var service = ws.Services.GetLanguageServices(LanguageNames.CSharp).GetService<ITestLanguageService>();
            Assert.NotNull(service as TestLanguageServiceA);

            var ws2 = new AdhocWorkspace(hostServices, "Quasimodo");
            var service2 = ws2.Services.GetLanguageServices(LanguageNames.CSharp).GetService<ITestLanguageService>();
            Assert.NotNull(service2 as TestLanguageServiceB);
        }

#if false
        [Fact, Trait(Traits.Feature, Traits.Features.Workspace)]
        public void TestSolutionInfo()
        {
            var oldSolutionId = SolutionId.CreateNewId("oldId");
            var oldVersion = VersionStamp.Create();
            var solutionInfo = SolutionInfo.Create(oldSolutionId, oldVersion, null, null);

            var newSolutionId = SolutionId.CreateNewId("newId");
            solutionInfo = solutionInfo.WithId(newSolutionId);
            Assert.NotEqual(oldSolutionId, solutionInfo.Id);
            Assert.Equal(newSolutionId, solutionInfo.Id);
            
            var newVersion = oldVersion.GetNewerVersion();
            solutionInfo = solutionInfo.WithVersion(newVersion);
            Assert.NotEqual(oldVersion, solutionInfo.Version);
            Assert.Equal(newVersion, solutionInfo.Version);

            Assert.Null(solutionInfo.FilePath);
            var newFilePath = @"C:\test\fake.sln";
            solutionInfo = solutionInfo.WithFilePath(newFilePath);
            Assert.Equal(newFilePath, solutionInfo.FilePath);

            Assert.Equal(0, solutionInfo.Projects.Count());
        }
#endif

        private interface ITestLanguageService : ILanguageService
        {
        }

        [ExportLanguageService(typeof(ITestLanguageService), LanguageNames.CSharp, ServiceLayer.Default), Shared]
        private class TestLanguageServiceA : ITestLanguageService
        {
            [ImportingConstructor]
            public TestLanguageServiceA()
            {
            }
        }

        [ExportLanguageService(typeof(ITestLanguageService), LanguageNames.CSharp, "Quasimodo"), Shared]
        private class TestLanguageServiceB : ITestLanguageService
        {
            [ImportingConstructor]
            public TestLanguageServiceB()
            {
            }
        }

        [Fact]
        [WorkItem(666263, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/666263")]
        public async Task TestDocumentFileAccessFailureMissingFile()
        {
            var workspace = new AdhocWorkspace();
            var solution = workspace.CurrentSolution;

            WorkspaceDiagnostic diagnosticFromEvent = null;
            solution.Workspace.WorkspaceFailed += (sender, args) =>
            {
                diagnosticFromEvent = args.Diagnostic;
            };

            var pid = ProjectId.CreateNewId();
            var did = DocumentId.CreateNewId(pid);

            solution = solution.AddProject(pid, "goo", "goo", LanguageNames.CSharp)
                               .AddDocument(did, "x", new FileTextLoader(@"C:\doesnotexist.cs", Encoding.UTF8))
                               .WithDocumentFilePath(did, "document path");

            var doc = solution.GetDocument(did);
            var text = await doc.GetTextAsync().ConfigureAwait(false);

            var diagnostic = await doc.State.GetLoadDiagnosticAsync(CancellationToken.None).ConfigureAwait(false);

            Assert.Equal(@"C:\doesnotexist.cs: (0,0)-(0,0)", diagnostic.Location.GetLineSpan().ToString());
            Assert.Equal(WorkspaceDiagnosticKind.Failure, diagnosticFromEvent.Kind);
            Assert.Equal("", text.ToString());

            // Verify invariant: The compilation is guaranteed to have a syntax tree for each document of the project (even if the contnet fails to load).
            var compilation = await solution.State.GetCompilationAsync(doc.Project.State, CancellationToken.None).ConfigureAwait(false);
            var syntaxTree = compilation.SyntaxTrees.Single();
            Assert.Equal("", syntaxTree.ToString());
        }

        [Fact]
        public void TestGetProjectForAssemblySymbol()
        {
            var pid1 = ProjectId.CreateNewId("p1");
            var pid2 = ProjectId.CreateNewId("p2");
            var pid3 = ProjectId.CreateNewId("p3");
            var did1 = DocumentId.CreateNewId(pid1);
            var did2 = DocumentId.CreateNewId(pid2);
            var did3 = DocumentId.CreateNewId(pid3);

            var text1 = @"
Public Class A
End Class";

            var text2 = @"
Public Class B
End Class
";

            var text3 = @"
public class C : B {
}
";

            var text4 = @"
public class C : A {
}
";

            var solution = new AdhocWorkspace().CurrentSolution
                .AddProject(pid1, "GooA", "Goo.dll", LanguageNames.VisualBasic)
                .AddDocument(did1, "A.vb", text1)
                .AddMetadataReference(pid1, s_mscorlib)
                .AddProject(pid2, "GooB", "Goo2.dll", LanguageNames.VisualBasic)
                .AddDocument(did2, "B.vb", text2)
                .AddMetadataReference(pid2, s_mscorlib)
                .AddProject(pid3, "Bar", "Bar.dll", LanguageNames.CSharp)
                .AddDocument(did3, "C.cs", text3)
                .AddMetadataReference(pid3, s_mscorlib)
                .AddProjectReference(pid3, new ProjectReference(pid1))
                .AddProjectReference(pid3, new ProjectReference(pid2));

            var project3 = solution.GetProject(pid3);
            var comp3 = project3.GetCompilationAsync().Result;
            var classC = comp3.GetTypeByMetadataName("C");
            var projectForBaseType = solution.GetProject(classC.BaseType.ContainingAssembly);
            Assert.Equal(pid2, projectForBaseType.Id);

            // switch base type to A then try again
            var solution2 = solution.WithDocumentText(did3, SourceText.From(text4));
            project3 = solution2.GetProject(pid3);
            comp3 = project3.GetCompilationAsync().Result;
            classC = comp3.GetTypeByMetadataName("C");
            projectForBaseType = solution2.GetProject(classC.BaseType.ContainingAssembly);
            Assert.Equal(pid1, projectForBaseType.Id);
        }

        [WorkItem(1088127, "http://vstfdevdiv:8080/DevDiv2/DevDiv/_workitems/edit/1088127")]
        [Fact]
        public void TestEncodingRetainedAfterTreeChanged()
        {
            var ws = new AdhocWorkspace();
            var proj = ws.AddProject("proj", LanguageNames.CSharp);
            var doc = ws.AddDocument(proj.Id, "a.cs", SourceText.From("public class c { }", Encoding.UTF32));

            Assert.Equal(Encoding.UTF32, doc.GetTextAsync().Result.Encoding);

            // updating root doesn't change original encoding
            var root = doc.GetSyntaxRootAsync().Result;
            var newRoot = root.WithLeadingTrivia(root.GetLeadingTrivia().Add(CS.SyntaxFactory.Whitespace("    ")));
            var newDoc = doc.WithSyntaxRoot(newRoot);

            Assert.Equal(Encoding.UTF32, newDoc.GetTextAsync().Result.Encoding);
        }

        [Fact]
        public void TestProjectWithNoBrokenReferencesHasNoIncompleteReferences()
        {
            var workspace = new AdhocWorkspace();
            var project1 = workspace.AddProject("CSharpProject", LanguageNames.CSharp);
            var project2 = workspace.AddProject(
                ProjectInfo.Create(
                    ProjectId.CreateNewId(),
                    VersionStamp.Create(),
                    "VisualBasicProject",
                    "VisualBasicProject",
                    LanguageNames.VisualBasic,
                    projectReferences: new[] { new ProjectReference(project1.Id) }));

            // Nothing should have incomplete references, and everything should build
            Assert.True(project1.HasSuccessfullyLoadedAsync().Result);
            Assert.True(project2.HasSuccessfullyLoadedAsync().Result);
            Assert.Single(project2.GetCompilationAsync().Result.ExternalReferences);
        }

        [Fact]
        public void TestProjectWithBrokenCrossLanguageReferenceHasIncompleteReferences()
        {
            var workspace = new AdhocWorkspace();
            var project1 = workspace.AddProject("CSharpProject", LanguageNames.CSharp);
            workspace.AddDocument(project1.Id, "Broken.cs", SourceText.From("class "));

            var project2 = workspace.AddProject(
                ProjectInfo.Create(
                    ProjectId.CreateNewId(),
                    VersionStamp.Create(),
                    "VisualBasicProject",
                    "VisualBasicProject",
                    LanguageNames.VisualBasic,
                    projectReferences: new[] { new ProjectReference(project1.Id) }));

            Assert.True(project1.HasSuccessfullyLoadedAsync().Result);
            Assert.False(project2.HasSuccessfullyLoadedAsync().Result);
            Assert.Empty(project2.GetCompilationAsync().Result.ExternalReferences);
        }

        [Fact]
        public void TestFrozenPartialProjectAlwaysIsIncomplete()
        {
            var workspace = new AdhocWorkspace();
            var project1 = workspace.AddProject("CSharpProject", LanguageNames.CSharp);

            var project2 = workspace.AddProject(
                ProjectInfo.Create(
                    ProjectId.CreateNewId(),
                    VersionStamp.Create(),
                    "VisualBasicProject",
                    "VisualBasicProject",
                    LanguageNames.VisualBasic,
                    projectReferences: new[] { new ProjectReference(project1.Id) }));

            var document = workspace.AddDocument(project2.Id, "Test.cs", SourceText.From(""));

            // Nothing should have incomplete references, and everything should build
            var frozenSolution = document.WithFrozenPartialSemantics(CancellationToken.None).Project.Solution;

            Assert.True(frozenSolution.GetProject(project1.Id).HasSuccessfullyLoadedAsync().Result);
            Assert.True(frozenSolution.GetProject(project2.Id).HasSuccessfullyLoadedAsync().Result);
        }

        [Fact]
        public void TestProjectCompletenessWithMultipleProjects()
        {
            GetMultipleProjects(out var csBrokenProject, out var vbNormalProject, out var dependsOnBrokenProject, out var dependsOnVbNormalProject, out var transitivelyDependsOnBrokenProjects, out var transitivelyDependsOnNormalProjects);

            // check flag for a broken project itself
            Assert.False(csBrokenProject.HasSuccessfullyLoadedAsync().Result);

            // check flag for a normal project itself
            Assert.True(vbNormalProject.HasSuccessfullyLoadedAsync().Result);

            // check flag for normal project that directly reference a broken project
            Assert.True(dependsOnBrokenProject.HasSuccessfullyLoadedAsync().Result);

            // check flag for normal project that directly reference only normal project
            Assert.True(dependsOnVbNormalProject.HasSuccessfullyLoadedAsync().Result);

            // check flag for normal project that indirectly reference a borken project
            // normal project -> normal project -> broken project
            Assert.True(transitivelyDependsOnBrokenProjects.HasSuccessfullyLoadedAsync().Result);

            // check flag for normal project that indirectly reference only normal project
            // normal project -> normal project -> normal project
            Assert.True(transitivelyDependsOnNormalProjects.HasSuccessfullyLoadedAsync().Result);
        }

        [Fact]
        public async Task TestMassiveFileSize()
        {
            // set max file length to 1 bytes
            var maxLength = 1;
            var workspace = new AdhocWorkspace(MefHostServices.Create(TestHost.Assemblies), ServiceLayer.Host);
            workspace.TryApplyChanges(workspace.CurrentSolution.WithOptions(workspace.Options
                .WithChangedOption(FileTextLoaderOptions.FileLengthThreshold, maxLength)));

            using var root = new TempRoot();
            var file = root.CreateFile(prefix: "massiveFile", extension: ".cs").WriteAllText("hello");

            var loader = new FileTextLoader(file.Path, Encoding.UTF8);
            var textLength = FileUtilities.GetFileLength(file.Path);

            var expected = string.Format(WorkspacesResources.File_0_size_of_1_exceeds_maximum_allowed_size_of_2, file.Path, textLength, maxLength);
            var exceptionThrown = false;

            try
            {
                // test async one
                var unused = await loader.LoadTextAndVersionAsync(workspace, DocumentId.CreateNewId(ProjectId.CreateNewId()), CancellationToken.None);
            }
            catch (InvalidDataException ex)
            {
                exceptionThrown = true;
                Assert.Equal(expected, ex.Message);
            }

            Assert.True(exceptionThrown);

            exceptionThrown = false;
            try
            {
                // test sync one
                var unused = loader.LoadTextAndVersionSynchronously(workspace, DocumentId.CreateNewId(ProjectId.CreateNewId()), CancellationToken.None);
            }
            catch (InvalidDataException ex)
            {
                exceptionThrown = true;
                Assert.Equal(expected, ex.Message);
            }

            Assert.True(exceptionThrown);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.Workspace)]
        [WorkItem(18697, "https://github.com/dotnet/roslyn/issues/18697")]
        public void TestWithSyntaxTree()
        {
            // get one to get to syntax tree factory
            var dummyProject = CreateNotKeptAliveSolution().AddProject("dummy", "dummy", LanguageNames.CSharp);

            var factory = dummyProject.LanguageServices.SyntaxTreeFactory;

            // create the origin tree
            var strongTree = factory.ParseSyntaxTree("dummy", dummyProject.ParseOptions, SourceText.From("// emtpy"), analyzerConfigOptionsResult: null, CancellationToken.None);

            // create recoverable tree off the original tree
            var recoverableTree = factory.CreateRecoverableTree(
                dummyProject.Id,
                strongTree.FilePath,
                strongTree.Options,
                new ConstantValueSource<TextAndVersion>(TextAndVersion.Create(strongTree.GetText(), VersionStamp.Create(), strongTree.FilePath)),
                strongTree.GetText().Encoding,
                strongTree.GetRoot(),
                strongTree.DiagnosticOptions);

            // create new tree before it ever getting root node
            var newTree = recoverableTree.WithFilePath("different/dummy");

            // this shouldn't throw
            var root = newTree.GetRoot();
        }

        [Fact, Trait(Traits.Feature, Traits.Features.Workspace)]
        public void TestUpdateDocumentsOrder()
        {
            var solution = CreateSolution();
            var pid = ProjectId.CreateNewId();

            VersionStamp GetVersion() => solution.GetProject(pid).Version;
            ImmutableArray<DocumentId> GetDocumentIds() => solution.GetProject(pid).DocumentIds.ToImmutableArray();
            ImmutableArray<SyntaxTree> GetSyntaxTrees()
            {
                return solution.GetProject(pid).GetCompilationAsync().Result.SyntaxTrees.ToImmutableArray();
            }

            solution = solution.AddProject(pid, "test", "test.dll", LanguageNames.CSharp);

            var text1 = "public class Test1 {}";
            var did1 = DocumentId.CreateNewId(pid);
            solution = solution.AddDocument(did1, "test1.cs", text1);

            var text2 = "public class Test2 {}";
            var did2 = DocumentId.CreateNewId(pid);
            solution = solution.AddDocument(did2, "test2.cs", text2);

            var text3 = "public class Test3 {}";
            var did3 = DocumentId.CreateNewId(pid);
            solution = solution.AddDocument(did3, "test3.cs", text3);

            var text4 = "public class Test4 {}";
            var did4 = DocumentId.CreateNewId(pid);
            solution = solution.AddDocument(did4, "test4.cs", text4);

            var text5 = "public class Test5 {}";
            var did5 = DocumentId.CreateNewId(pid);
            solution = solution.AddDocument(did5, "test5.cs", text5);

            var oldVersion = GetVersion();

            solution = solution.WithProjectDocumentsOrder(pid, ImmutableList.CreateRange(new[] { did5, did4, did3, did2, did1 }));

            var newVersion = GetVersion();

            // Make sure we have a new version because the order changed.
            Assert.NotEqual(oldVersion, newVersion);

            var documentIds = GetDocumentIds();

            Assert.Equal(did5, documentIds[0]);
            Assert.Equal(did4, documentIds[1]);
            Assert.Equal(did3, documentIds[2]);
            Assert.Equal(did2, documentIds[3]);
            Assert.Equal(did1, documentIds[4]);

            var syntaxTrees = GetSyntaxTrees();

            Assert.Equal(documentIds.Count(), syntaxTrees.Count());

            Assert.Equal("test5.cs", syntaxTrees[0].FilePath, StringComparer.OrdinalIgnoreCase);
            Assert.Equal("test4.cs", syntaxTrees[1].FilePath, StringComparer.OrdinalIgnoreCase);
            Assert.Equal("test3.cs", syntaxTrees[2].FilePath, StringComparer.OrdinalIgnoreCase);
            Assert.Equal("test2.cs", syntaxTrees[3].FilePath, StringComparer.OrdinalIgnoreCase);
            Assert.Equal("test1.cs", syntaxTrees[4].FilePath, StringComparer.OrdinalIgnoreCase);

            solution = solution.WithProjectDocumentsOrder(pid, ImmutableList.CreateRange(new[] { did5, did4, did3, did2, did1 }));

            var newSameVersion = GetVersion();

            // Make sure we have the same new version because the order hasn't changed.
            Assert.Equal(newVersion, newSameVersion);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.Workspace)]
        public void TestUpdateDocumentsOrderExceptions()
        {
            var solution = CreateSolution();
            var pid = ProjectId.CreateNewId();

            solution = solution.AddProject(pid, "test", "test.dll", LanguageNames.CSharp);

            var text1 = "public class Test1 {}";
            var did1 = DocumentId.CreateNewId(pid);
            solution = solution.AddDocument(did1, "test1.cs", text1);

            var text2 = "public class Test2 {}";
            var did2 = DocumentId.CreateNewId(pid);
            solution = solution.AddDocument(did2, "test2.cs", text2);

            var text3 = "public class Test3 {}";
            var did3 = DocumentId.CreateNewId(pid);
            solution = solution.AddDocument(did3, "test3.cs", text3);

            var text4 = "public class Test4 {}";
            var did4 = DocumentId.CreateNewId(pid);
            solution = solution.AddDocument(did4, "test4.cs", text4);

            var text5 = "public class Test5 {}";
            var did5 = DocumentId.CreateNewId(pid);
            solution = solution.AddDocument(did5, "test5.cs", text5);

            solution = solution.RemoveDocument(did5);

            Assert.Throws<ArgumentOutOfRangeException>(() => solution = solution.WithProjectDocumentsOrder(pid, ImmutableList.Create<DocumentId>()));
            Assert.Throws<ArgumentNullException>(() => solution = solution.WithProjectDocumentsOrder(pid, null));
            Assert.Throws<InvalidOperationException>(() => solution = solution.WithProjectDocumentsOrder(pid, ImmutableList.CreateRange(new[] { did5, did3, did2, did1 })));
            Assert.Throws<ArgumentException>(() => solution = solution.WithProjectDocumentsOrder(pid, ImmutableList.CreateRange(new[] { did3, did2, did1 })));
        }

        [Theory, Trait(Traits.Feature, Traits.Features.Workspace)]
        [CombinatorialData]
        public async Task TestAddingEditorConfigFileWithDiagnosticSeverity([CombinatorialValues(LanguageNames.CSharp, LanguageNames.VisualBasic)] string languageName, bool useRecoverableTrees)
        {
            var solution = useRecoverableTrees ? CreateNotKeptAliveSolution() : CreateSolution();
            var extension = languageName == LanguageNames.CSharp ? ".cs" : ".vb";
            var projectId = ProjectId.CreateNewId();
            var sourceDocumentId = DocumentId.CreateNewId(projectId);

            solution = solution.AddProject(projectId, "Test", "Test.dll", languageName);
            solution = solution.AddDocument(sourceDocumentId, "Test" + extension, "", filePath: @"Z:\Test" + extension);

            var originalSyntaxTree = await solution.GetDocument(sourceDocumentId).GetSyntaxTreeAsync();
            var originalCompilation = await solution.GetProject(projectId).GetCompilationAsync();

            var editorConfigDocumentId = DocumentId.CreateNewId(projectId);
            solution = solution.AddAnalyzerConfigDocuments(ImmutableArray.Create(
                DocumentInfo.Create(
                    editorConfigDocumentId,
                    ".editorconfig",
                    filePath: @"Z:\.editorconfig",
                    loader: TextLoader.From(TextAndVersion.Create(SourceText.From("[*.*]\r\n\r\ndotnet_diagnostic.CA1234.severity = error"), VersionStamp.Default)))));

            var newSyntaxTree = await solution.GetDocument(sourceDocumentId).GetSyntaxTreeAsync();
            var newCompilation = await solution.GetProject(projectId).GetCompilationAsync();

            Assert.NotSame(originalSyntaxTree, newSyntaxTree);
            Assert.NotSame(originalCompilation, newCompilation);

            Assert.True(newCompilation.ContainsSyntaxTree(newSyntaxTree));

            Assert.Single(newSyntaxTree.DiagnosticOptions);
            Assert.Equal(ReportDiagnostic.Error, newSyntaxTree.DiagnosticOptions["CA1234"]);
        }

        [Theory, Trait(Traits.Feature, Traits.Features.Workspace)]
        [CombinatorialData]
        public async Task TestAddingAndRemovingEditorConfigFileWithDiagnosticSeverity([CombinatorialValues(LanguageNames.CSharp, LanguageNames.VisualBasic)] string languageName, bool useRecoverableTrees)
        {
            var solution = useRecoverableTrees ? CreateNotKeptAliveSolution() : CreateSolution();
            var extension = languageName == LanguageNames.CSharp ? ".cs" : ".vb";
            var projectId = ProjectId.CreateNewId();
            var sourceDocumentId = DocumentId.CreateNewId(projectId);

            solution = solution.AddProject(projectId, "Test", "Test.dll", languageName);
            solution = solution.AddDocument(sourceDocumentId, "Test" + extension, "", filePath: @"Z:\Test" + extension);

            var editorConfigDocumentId = DocumentId.CreateNewId(projectId);
            solution = solution.AddAnalyzerConfigDocuments(ImmutableArray.Create(
                DocumentInfo.Create(
                    editorConfigDocumentId,
                    ".editorconfig",
                    filePath: @"Z:\.editorconfig",
                    loader: TextLoader.From(TextAndVersion.Create(SourceText.From("[*.*]\r\n\r\ndotnet_diagnostic.CA1234.severity = error"), VersionStamp.Default)))));

            var syntaxTreeAfterAddingEditorConfig = await solution.GetDocument(sourceDocumentId).GetSyntaxTreeAsync();

            Assert.Single(syntaxTreeAfterAddingEditorConfig.DiagnosticOptions);
            Assert.Equal(ReportDiagnostic.Error, syntaxTreeAfterAddingEditorConfig.DiagnosticOptions["CA1234"]);

            solution = solution.RemoveAnalyzerConfigDocument(editorConfigDocumentId);

            var syntaxTreeAfterRemovingEditorConfig = await solution.GetDocument(sourceDocumentId).GetSyntaxTreeAsync();

            Assert.Empty(syntaxTreeAfterRemovingEditorConfig.DiagnosticOptions);

            var finalCompilation = await solution.GetProject(projectId).GetCompilationAsync();

            Assert.True(finalCompilation.ContainsSyntaxTree(syntaxTreeAfterRemovingEditorConfig));
        }

        [Theory, Trait(Traits.Feature, Traits.Features.Workspace)]
        [CombinatorialData]
        public async Task TestChangingAnEditorConfigFile([CombinatorialValues(LanguageNames.CSharp, LanguageNames.VisualBasic)] string languageName, bool useRecoverableTrees)
        {
            var solution = useRecoverableTrees ? CreateNotKeptAliveSolution() : CreateSolution();
            var extension = languageName == LanguageNames.CSharp ? ".cs" : ".vb";
            var projectId = ProjectId.CreateNewId();
            var sourceDocumentId = DocumentId.CreateNewId(projectId);

            solution = solution.AddProject(projectId, "Test", "Test.dll", languageName);
            solution = solution.AddDocument(sourceDocumentId, "Test" + extension, "", filePath: @"Z:\Test" + extension);

            var editorConfigDocumentId = DocumentId.CreateNewId(projectId);
            solution = solution.AddAnalyzerConfigDocuments(ImmutableArray.Create(
                DocumentInfo.Create(
                    editorConfigDocumentId,
                    ".editorconfig",
                    filePath: @"Z:\.editorconfig",
                    loader: TextLoader.From(TextAndVersion.Create(SourceText.From("[*.*]\r\n\r\ndotnet_diagnostic.CA1234.severity = error"), VersionStamp.Default)))));

            var syntaxTreeBeforeEditorConfigChange = await solution.GetDocument(sourceDocumentId).GetSyntaxTreeAsync();

            Assert.Single(syntaxTreeBeforeEditorConfigChange.DiagnosticOptions);
            Assert.Equal(ReportDiagnostic.Error, syntaxTreeBeforeEditorConfigChange.DiagnosticOptions["CA1234"]);

            solution = solution.WithAnalyzerConfigDocumentTextLoader(
                editorConfigDocumentId,
                TextLoader.From(TextAndVersion.Create(SourceText.From("[*.*]\r\n\r\ndotnet_diagnostic.CA6789.severity = error"), VersionStamp.Default)),
                PreservationMode.PreserveValue);

            var syntaxTreeAfterEditorConfigChange = await solution.GetDocument(sourceDocumentId).GetSyntaxTreeAsync();

            Assert.Single(syntaxTreeAfterEditorConfigChange.DiagnosticOptions);
            Assert.Equal(ReportDiagnostic.Error, syntaxTreeAfterEditorConfigChange.DiagnosticOptions["CA6789"]);

            var finalCompilation = await solution.GetProject(projectId).GetCompilationAsync();

            Assert.True(finalCompilation.ContainsSyntaxTree(syntaxTreeAfterEditorConfigChange));
        }

        [Theory, Trait(Traits.Feature, Traits.Features.Workspace)]
        [CombinatorialData]
        public async Task TestUpdateRootStillCarriesDiagnosticData([CombinatorialValues(LanguageNames.CSharp, LanguageNames.VisualBasic)] string languageName, bool useRecoverableTrees, PreservationMode preservationMode)
        {
            var solution = useRecoverableTrees ? CreateNotKeptAliveSolution() : CreateSolution();
            var extension = languageName == LanguageNames.CSharp ? ".cs" : ".vb";
            var projectId = ProjectId.CreateNewId();
            var sourceDocumentId = DocumentId.CreateNewId(projectId);

            solution = solution.AddProject(projectId, "Test", "Test.dll", languageName);
            solution = solution.AddDocument(sourceDocumentId, "Test" + extension, "", filePath: @"Z:\Test" + extension);

            var editorConfigDocumentId = DocumentId.CreateNewId(projectId);
            solution = solution.AddAnalyzerConfigDocuments(ImmutableArray.Create(
                DocumentInfo.Create(
                    editorConfigDocumentId,
                    ".editorconfig",
                    filePath: @"Z:\.editorconfig",
                    loader: TextLoader.From(TextAndVersion.Create(SourceText.From("[*.*]\r\n\r\ndotnet_diagnostic.CA1234.severity = error"), VersionStamp.Default)))));

            var syntaxTreeBeforeUpdateRoot = await solution.GetDocument(sourceDocumentId).GetSyntaxTreeAsync();

            Assert.Single(syntaxTreeBeforeUpdateRoot.DiagnosticOptions);
            Assert.Equal(ReportDiagnostic.Error, syntaxTreeBeforeUpdateRoot.DiagnosticOptions["CA1234"]);

            // We are adding a new line to the previously empty file
            var newRoot = syntaxTreeBeforeUpdateRoot.WithChangedText(SourceText.From("\r\n"));

            solution = solution.WithDocumentSyntaxRoot(sourceDocumentId, newRoot.GetRoot(), preservationMode);

            var syntaxTreeAfterUpdateRoot = await solution.GetDocument(sourceDocumentId).GetSyntaxTreeAsync();

            Assert.Single(syntaxTreeAfterUpdateRoot.DiagnosticOptions);
            Assert.Equal(ReportDiagnostic.Error, syntaxTreeAfterUpdateRoot.DiagnosticOptions["CA1234"]);

            var finalCompilation = await solution.GetProject(projectId).GetCompilationAsync();

            Assert.True(finalCompilation.ContainsSyntaxTree(syntaxTreeAfterUpdateRoot));
        }

        [Fact, Trait(Traits.Feature, Traits.Features.Workspace)]
        [WorkItem(3705, "https://github.com/dotnet/roslyn/issues/3705")]
        public async Task TestAddingEditorConfigFileWithIsGeneratedCodeOption()
        {
            var solution = CreateSolution();
            var projectId = ProjectId.CreateNewId();
            var sourceDocumentId = DocumentId.CreateNewId(projectId);

            solution = solution.AddProject(projectId, "Test", "Test.dll", LanguageNames.CSharp)
                .WithProjectMetadataReferences(projectId, new[] { TestReferences.NetFx.v4_0_30319.mscorlib })
                .WithProjectCompilationOptions(projectId, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary).WithNullableContextOptions(NullableContextOptions.Enable));
            var src = @"
class C
{
    void M(C? c)
    {
        _ = c.ToString();   // warning CS8602: Dereference of a possibly null reference.
    }
}";
            solution = solution.AddDocument(sourceDocumentId, "Test.cs", src, filePath: @"Z:\Test.cs");

            var originalSyntaxTree = await solution.GetDocument(sourceDocumentId).GetSyntaxTreeAsync();
            var originalCompilation = await solution.GetProject(projectId).GetCompilationAsync();

            // warning CS8602: Dereference of a possibly null reference.
            var diagnostics = originalCompilation.GetDiagnostics();
            var diagnostic = Assert.Single(diagnostics);
            Assert.Equal("CS8602", diagnostic.Id);

            var editorConfigDocumentId = DocumentId.CreateNewId(projectId);
            solution = solution.AddAnalyzerConfigDocuments(ImmutableArray.Create(
                DocumentInfo.Create(
                    editorConfigDocumentId,
                    ".editorconfig",
                    filePath: @"Z:\.editorconfig",
                    loader: TextLoader.From(TextAndVersion.Create(SourceText.From("[*.*]\r\n\r\ngenerated_code = true"), VersionStamp.Default)))));

            var newSyntaxTree = await solution.GetDocument(sourceDocumentId).GetSyntaxTreeAsync();
            var newCompilation = await solution.GetProject(projectId).GetCompilationAsync();

            Assert.NotSame(originalSyntaxTree, newSyntaxTree);
            Assert.NotSame(originalCompilation, newCompilation);

            Assert.True(newCompilation.ContainsSyntaxTree(newSyntaxTree));

            // warning CS8669: The annotation for nullable reference types should only be used in code within a '#nullable' annotations context. Auto-generated code requires an explicit '#nullable' directive in source.
            diagnostics = newCompilation.GetDiagnostics();
            diagnostic = Assert.Single(diagnostics);
            Assert.Contains("CS8669", diagnostic.Id);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.Workspace)]
        public void NoCompilationProjectsHaveNullSyntaxTreesAndSemanticModels()
        {
            var solution = CreateSolution();
            var projectId = ProjectId.CreateNewId();
            var documentId = DocumentId.CreateNewId(projectId);

            solution = solution.AddProject(projectId, "Test", "Test.dll", NoCompilationConstants.LanguageName);
            solution = solution.AddDocument(documentId, "Test.cs", "", filePath: @"Z:\Test.txt");

            var document = solution.GetDocument(documentId)!;

            Assert.False(document.TryGetSyntaxTree(out _));
            Assert.Null(document.GetSyntaxTreeAsync().Result);
            Assert.Null(document.GetSyntaxTreeSynchronously(CancellationToken.None));

            Assert.False(document.TryGetSemanticModel(out _));
            Assert.Null(document.GetSemanticModelAsync().Result);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.Workspace)]
        public void ChangingFilePathOfFileInNoCompilationProjectWorks()
        {
            var solution = CreateSolution();
            var projectId = ProjectId.CreateNewId();
            var documentId = DocumentId.CreateNewId(projectId);

            solution = solution.AddProject(projectId, "Test", "Test.dll", NoCompilationConstants.LanguageName);
            solution = solution.AddDocument(documentId, "Test.cs", "", filePath: @"Z:\Test.txt");

            Assert.Null(solution.GetDocument(documentId)!.GetSyntaxTreeAsync().Result);

            solution = solution.WithDocumentFilePath(documentId, @"Z:\NewPath.txt");

            Assert.Null(solution.GetDocument(documentId)!.GetSyntaxTreeAsync().Result);
        }

        [Fact, Trait(Traits.Feature, Traits.Features.Workspace)]
        public void AddingAndRemovingProjectsUpdatesFilePathMap()
        {
            var solution = CreateSolution();
            var projectId = ProjectId.CreateNewId();
            var editorConfigDocumentId = DocumentId.CreateNewId(projectId);

            const string editorConfigFilePath = @"Z:\.editorconfig";

            var projectInfo =
                ProjectInfo.Create(projectId, VersionStamp.Default, "Test", "Test", LanguageNames.CSharp)
                    .WithAnalyzerConfigDocuments(new[] { DocumentInfo.Create(editorConfigDocumentId, ".editorconfig", filePath: editorConfigFilePath) });


            solution = solution.AddProject(projectInfo);

            Assert.Equal(editorConfigDocumentId, Assert.Single(solution.GetDocumentIdsWithFilePath(editorConfigFilePath)));

            solution = solution.RemoveProject(projectId);

            Assert.Empty(solution.GetDocumentIdsWithFilePath(editorConfigFilePath));
        }

        private static void GetMultipleProjects(
            out Project csBrokenProject,
            out Project vbNormalProject,
            out Project dependsOnBrokenProject,
            out Project dependsOnVbNormalProject,
            out Project transitivelyDependsOnBrokenProjects,
            out Project transitivelyDependsOnNormalProjects)
        {
            var workspace = new AdhocWorkspace();

            csBrokenProject = workspace.AddProject(
                ProjectInfo.Create(
                    ProjectId.CreateNewId(),
                    VersionStamp.Create(),
                    "CSharpProject",
                    "CSharpProject",
                    LanguageNames.CSharp).WithHasAllInformation(hasAllInformation: false));

            vbNormalProject = workspace.AddProject(
                ProjectInfo.Create(
                    ProjectId.CreateNewId(),
                    VersionStamp.Create(),
                    "VisualBasicProject",
                    "VisualBasicProject",
                    LanguageNames.VisualBasic));

            dependsOnBrokenProject = workspace.AddProject(
                ProjectInfo.Create(
                    ProjectId.CreateNewId(),
                    VersionStamp.Create(),
                    "VisualBasicProject",
                    "VisualBasicProject",
                    LanguageNames.VisualBasic,
                    projectReferences: new[] { new ProjectReference(csBrokenProject.Id), new ProjectReference(vbNormalProject.Id) }));

            dependsOnVbNormalProject = workspace.AddProject(
                ProjectInfo.Create(
                    ProjectId.CreateNewId(),
                    VersionStamp.Create(),
                    "CSharpProject",
                    "CSharpProject",
                    LanguageNames.CSharp,
                    projectReferences: new[] { new ProjectReference(vbNormalProject.Id) }));

            transitivelyDependsOnBrokenProjects = workspace.AddProject(
                ProjectInfo.Create(
                    ProjectId.CreateNewId(),
                    VersionStamp.Create(),
                    "CSharpProject",
                    "CSharpProject",
                    LanguageNames.CSharp,
                    projectReferences: new[] { new ProjectReference(dependsOnBrokenProject.Id) }));

            transitivelyDependsOnNormalProjects = workspace.AddProject(
                ProjectInfo.Create(
                    ProjectId.CreateNewId(),
                    VersionStamp.Create(),
                    "VisualBasicProject",
                    "VisualBasicProject",
                    LanguageNames.VisualBasic,
                    projectReferences: new[] { new ProjectReference(dependsOnVbNormalProject.Id) }));
        }

        [Fact, Trait(Traits.Feature, Traits.Features.Workspace)]
        public void TestOptionChangesForLanguagesNotInSolution()
        {
            // Create an empty solution with no projects.
            var s0 = CreateSolution();
            var optionService = s0.Workspace.Services.GetRequiredService<IOptionService>();

            // Apply an option change to a C# option.
            var option = GenerationOptions.PlaceSystemNamespaceFirst;
            var defaultValue = option.DefaultValue;
            var changedValue = !defaultValue;
            var options = s0.Options.WithChangedOption(option, LanguageNames.CSharp, changedValue);

            // Verify option change is preserved even if the solution has no project with that language.
            var s1 = s0.WithOptions(options);
            VerifyOptionSet(s1.Options);

            // Verify option value is preserved on adding a project for a different language.
            var s2 = s1.AddProject("P1", "A1", LanguageNames.VisualBasic).Solution;
            VerifyOptionSet(s2.Options);

            // Verify option value is preserved on roundtriping the option set (serialize and deserialize).
            var s3 = s2.AddProject("P2", "A2", LanguageNames.CSharp).Solution;
            var roundTripOptionSet = SerializeAndDeserialize((SerializableOptionSet)s3.Options, optionService);
            VerifyOptionSet(roundTripOptionSet);

            // Verify option value is preserved on removing a project.
            var s4 = s3.RemoveProject(s3.Projects.Single(p => p.Name == "P2").Id);
            VerifyOptionSet(s4.Options);

            return;

            void VerifyOptionSet(OptionSet optionSet)
            {
                Assert.Equal(changedValue, optionSet.GetOption(option, LanguageNames.CSharp));
                Assert.Equal(defaultValue, optionSet.GetOption(option, LanguageNames.VisualBasic));
            }

            static SerializableOptionSet SerializeAndDeserialize(SerializableOptionSet optionSet, IOptionService optionService)
            {
                using var stream = new MemoryStream();
                using var writer = new ObjectWriter(stream);
                optionSet.Serialize(writer, CancellationToken.None);
                stream.Position = 0;
                using var reader = ObjectReader.TryGetReader(stream);
                return SerializableOptionSet.Deserialize(reader, optionService, CancellationToken.None);
            }
        }
    }
}
