﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#nullable enable

using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis.Options;
using Microsoft.CodeAnalysis.Options.Providers;

namespace Microsoft.CodeAnalysis.ValidateFormatString
{
    internal class ValidateFormatStringOption
    {
        public static PerLanguageOption2<bool> ReportInvalidPlaceholdersInStringDotFormatCalls =
            new PerLanguageOption2<bool>(
                nameof(ValidateFormatStringOption),
                nameof(ReportInvalidPlaceholdersInStringDotFormatCalls),
                defaultValue: true,
                storageLocations: new RoamingProfileStorageLocation("TextEditor.%LANGUAGE%.Specific.WarnOnInvalidStringDotFormatCalls"));
    }

    [ExportOptionProvider, Shared]
    internal class ValidateFormatStringOptionProvider : IOptionProvider
    {
        [ImportingConstructor]
        public ValidateFormatStringOptionProvider()
        {
        }

        public ImmutableArray<IOption> Options { get; } = ImmutableArray.Create<IOption>(
            ValidateFormatStringOption.ReportInvalidPlaceholdersInStringDotFormatCalls);
    }
}
