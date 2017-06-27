// Copyright (c) Microsoft Corporation
//
// All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance 
// with the License.  You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0 
//
// THIS CODE IS PROVIDED ON AN *AS IS* BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, EITHER
// EXPRESS OR IMPLIED, INCLUDING WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR CONDITIONS OF TITLE,
// FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABLITY OR NON-INFRINGEMENT.
//
// See the Apache Version 2.0 License for specific language governing permissions and limitations under the License.

namespace Microsoft.Spectrum.Common
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Security;
    using System.Security.Permissions;
    using System.Threading;

    public static class FileHelper
    {
        public static void RemoveReadOnlyAttribute(string path)
        {
            FileAttributes attributes = File.GetAttributes(path);
            if ((attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
            {
                attributes &= ~FileAttributes.ReadOnly;
                File.SetAttributes(path, attributes);
            }
        }

        /// <summary>
        /// Blocks the current thread until the specified file has finished writing.
        /// </summary>
        public static void WaitForFileWriteCompletion(string inputPath)
        {
            // TODO: Harden this a bit. Can we catch something more explicit so that we are not
            // spinning on exceptions?
            // TODO: Should there also be an explicit timeout?
            bool fileWritten = false;
            int currentTry = 1;
            while (!fileWritten && (currentTry < 1000))
            {
                try
                {
                    // This call will throw an IOException if the file is current being written to
                    using (var file = File.Open(inputPath, FileMode.Open, FileAccess.Read, FileShare.None))
                    {
                        fileWritten = true;
                    }
                }
                catch (IOException)
                {
                    // Do a linear backoff on the retry delay
                    // 25ms, 50ms, 75ms, etc
                    Thread.Sleep(25 * currentTry); // TODO: seems dangerous given the 1000 retries...
                    currentTry++;
                }
            }
        }

        public static void EnsureDirectoryExists(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }  
        }

        public static bool DirectoryHasPermission(string directoryPath, FileIOPermissionAccess access)
        {
            var permission = new FileIOPermission(access, directoryPath);
            var permissionSet = new PermissionSet(PermissionState.None);
            permissionSet.AddPermission(permission);
            bool hasAccess = permissionSet.IsSubsetOf(AppDomain.CurrentDomain.PermissionSet);
            return hasAccess;
        }

        public static string GetUniqueFileName(string fullPath)
        {
            return GetUniqueFileName(Path.GetDirectoryName(fullPath), Path.GetFileName(fullPath));
        }

        public static string GetUniqueFileName(string directoryPath, string path)
        {
            string filename = Path.GetFileName(path);
            string uniquePath = Path.Combine(directoryPath, filename);

            int i = 1;
            while (File.Exists(uniquePath))
            {
                filename = string.Format(CultureInfo.InvariantCulture, "{0}({1}){2}", Path.GetFileNameWithoutExtension(path), i, Path.GetExtension(path));
                uniquePath = Path.Combine(directoryPath, filename);
                i++;
            }

            return uniquePath;
        }
    }
}
