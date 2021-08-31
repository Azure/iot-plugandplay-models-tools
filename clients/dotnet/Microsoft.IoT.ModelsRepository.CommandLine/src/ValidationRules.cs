// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.IoT.ModelsRepository.CommandLine
{
    internal class ValidationRules
    {
        readonly bool _parseDtdl;
        readonly bool _ensureFilePlacement;
        readonly bool _ensureContentRootType;
        readonly bool _ensureDtmiNamespace;

        public ValidationRules(bool parseDtdl, bool ensureFilePlacement, bool ensureContentRootType, bool ensureDtmiNamespace)
        {
            _parseDtdl = parseDtdl;
            _ensureFilePlacement = ensureFilePlacement;
            _ensureContentRootType = ensureContentRootType;
            _ensureDtmiNamespace = ensureDtmiNamespace;
        }

        public ValidationRules():this(parseDtdl: true, ensureFilePlacement: true, ensureContentRootType: true, ensureDtmiNamespace: true)
        {
        }

        public bool ParseDtdl
        {
            get
            {
                return _parseDtdl;
            }
        }

        public bool EnsureFilePlacement
        {
            get
            {
                return _ensureFilePlacement;
            }
        }

        public bool EnsureContentRootType
        {
            get
            {
                return _ensureContentRootType;
            }
        }

        public bool EnsureDtmiNamespace
        {
            get
            {
                return _ensureDtmiNamespace;
            }
        }

        public static ValidationRules GetJustParseRules()
        {
            return new ValidationRules(
                parseDtdl: true,
                ensureFilePlacement: false,
                ensureContentRootType: false,
                ensureDtmiNamespace: false);
        }
    }
}
