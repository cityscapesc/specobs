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
    using System.ComponentModel;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Transactions;

    public enum HRESULT : int
    {
        /// <summary>
        /// Success code
        /// </summary>
        S_OK = unchecked((int)0x00000000),

        /// <summary>
        /// One or more arguments are invalid
        /// </summary>
        E_INVALIDARG = unchecked((int)0x80070057),

        /// <summary>
        /// The transaction has already been implicitly or explicitly committed or aborted
        /// </summary>
        XACT_E_NOTRANSACTION = unchecked((int)0x8004D00E)
    }

    [ComImport]
    [Guid("79427A2B-F895-40e0-BE79-B57DC82ED231")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IKernelTransaction
    {
        HRESULT GetHandle([Out] out IntPtr ktmHandle);
    }

    public static class TransactedFileHelper
    {
        [Flags]
        private enum MoveFileFlags : int
        {
            /// <summary>
            /// If the file is to be moved to a different volume, the function simulates the move by using the CopyFile and DeleteFile functions.
            /// </summary>
            MOVEFILE_COPY_ALLOWED = unchecked((int)0x00000002)
        }

        [Flags]
        private enum CopyFileFlags : int
        {
            COPY_FILE_COPY_SYMLINK = unchecked((int)0x00000800),
            COPY_FILE_FAIL_IF_EXISTS = unchecked((int)0x00000001),
            COPY_FILE_OPEN_SOURCE_FOR_WRITE = unchecked((int)0x00000004),
            COPY_FILE_RESTARTABLE = unchecked((int)0x00000002),
        }

        public static void MoveFileTransacted(string sourceFileName, string destinationFileName)
        {
            IntPtr transactionHandle = GetTransactionHandle();
            bool success = false;

            // Try to move the file
            success = NativeMethods.MoveFileTransacted(sourceFileName, destinationFileName, IntPtr.Zero, IntPtr.Zero, MoveFileFlags.MOVEFILE_COPY_ALLOWED, transactionHandle);
            if (success)
            {
                return;
            }
            else
            {
                int error = Marshal.GetLastWin32Error();
                string message = string.Format(CultureInfo.InvariantCulture, "There was a problem performing the transacted file move. Source:{0} Destination:{1}", sourceFileName, destinationFileName);
                throw new Win32Exception(error, message);
            }
        }

        public static void CopyFileTransacted(string sourceFileName, string destinationFileName)
        {
            IntPtr transactionHandle = GetTransactionHandle();
            bool success = false;

            // Try to copy the file
            success = NativeMethods.CopyFileTransacted(sourceFileName, destinationFileName, IntPtr.Zero, IntPtr.Zero, CopyFileFlags.COPY_FILE_FAIL_IF_EXISTS, transactionHandle);
            if (success)
            {
                return;
            }
            else
            {
                int error = Marshal.GetLastWin32Error();
                string message = string.Format(CultureInfo.InvariantCulture, "There was a problem performing the transacted file copy. Source:{0} Destination:{1}", sourceFileName, destinationFileName);
                throw new Win32Exception(error, message);
            }
        }

        /// <summary>
        /// Attempts to acquire a handle to a kernel transaction.
        /// </summary>
        /// <exception cref="System.ComponentModel.Win32Exception">Thrown when there was a problem acquiring the kernel transaction handle.</exception>
        /// <returns></returns>
        private static IntPtr GetTransactionHandle()
        {
            IKernelTransaction kernelTransaction = null;
            IntPtr transactionHandle = IntPtr.Zero;
            int error = 0;

            // Try to get a DTC transaction
            kernelTransaction = (IKernelTransaction)TransactionInterop.GetDtcTransaction(Transaction.Current);
            if (kernelTransaction != null)
            {
                // Try to get the handle of the DTC transaction
                HRESULT hresult = kernelTransaction.GetHandle(out transactionHandle);
                if (hresult == HRESULT.S_OK)
                {
                    return transactionHandle;
                }
                else
                {
                    error = (int)hresult;
                }
            }

            throw new Win32Exception(error, "There was a problem acquiring a kernel transaction for the file operation.");
        }

        private static class NativeMethods
        {
            [DllImport("kernel32.dll", EntryPoint = "CopyFileTransactedW",
                SetLastError = true, CharSet = CharSet.Unicode, ExactSpelling = true,
                CallingConvention = CallingConvention.StdCall)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool CopyFileTransacted(string existingFileName, string newFileName, IntPtr progressRoutine, IntPtr progressRoutineData, CopyFileFlags flags, IntPtr transactionHandle);

            [DllImport("kernel32.dll", EntryPoint = "MoveFileTransactedW",
                SetLastError = true, CharSet = CharSet.Unicode, ExactSpelling = true,
                CallingConvention = CallingConvention.StdCall)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool MoveFileTransacted(string existingFileName, string newFileName, IntPtr progressRoutine, IntPtr progressRoutineData, MoveFileFlags flags, IntPtr transactionHandle);
        }
    }
}
