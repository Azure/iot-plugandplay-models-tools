// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.IoT.ModelsRepository.CommandLine
{
    // Alternative to enum to avoid casting.
    public static class ReturnCodes
    {
        public const int Success = 0;
        public const int InvalidArguments = 1;
        public const int ValidationError = 2;
        public const int ResolutionError = 3;
        public const int ProcessingError = 4;
    }
}
