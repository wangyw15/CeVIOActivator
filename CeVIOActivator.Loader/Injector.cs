using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace CeVIOActivator.Loader
{
    /// <summary>
    /// Injector for patches
    /// </summary>
    /// Shell Code from https://clymb3r.wordpress.com/2013/05/26/implementing-remote-loadlibrary-and-remote-getprocaddress-using-powershell-and-assembly/
    /// Assembly compiled by https://defuse.ca/online-x86-assembler.htm
    internal class Injector
    {
        public IntPtr ProcessHandle { get; }

        public Injector(IntPtr processHandle)
        {
             ProcessHandle = processHandle;
        }

        private IntPtr Allocate(uint dwSize, Win32API.MemoryProtection flProtect = Win32API.MemoryProtection.ReadWrite)
        {
            var pMem = Win32API.VirtualAllocEx(
                ProcessHandle, IntPtr.Zero, dwSize, Win32API.AllocationType.Commit | Win32API.AllocationType.Reserve, flProtect);
            if (pMem == IntPtr.Zero)
            {
                throw new Exception($"VirtualAllocEx failed. Error code: {Marshal.GetLastWin32Error()}");
            }
            return pMem;
        }

        private bool Free(IntPtr pMemory)
        {
            return Win32API.VirtualFreeEx(ProcessHandle, pMemory, 0, Win32API.AllocationType.Release);
        }

        private void Write(IntPtr pMemory, byte[] content)
        {
            var size = (uint)content.Length;
            if (!Win32API.WriteProcessMemory(ProcessHandle, pMemory, content, size, out ulong written) || written != size)
            {
                throw new Exception($"WriteProcessMemory for shell code failed. Error code: {Marshal.GetLastWin32Error()}");
            }
        }

        private byte[] Read(IntPtr pMemory, int size)
        {
            var buffer = Marshal.AllocHGlobal(sizeof(ulong));
            if (!Win32API.ReadProcessMemory(ProcessHandle, pMemory, buffer, sizeof(ulong), out ulong read_size) || read_size != (ulong)size)
            {
                throw new Exception($"ReadProcessMemory failed. Error code: {Marshal.GetLastWin32Error()}");
            }
            var result = new byte[size];
            Marshal.Copy(buffer, result, 0, size);
            Marshal.FreeHGlobal(buffer);
            return result;
        }

        private T Read<T>(IntPtr pMemory, ulong size)
        {
            var buffer = Marshal.AllocHGlobal(sizeof(ulong));
            if (!Win32API.ReadProcessMemory(ProcessHandle, pMemory, buffer, sizeof(ulong), out ulong read_size) || read_size != size)
            {
                throw new Exception($"ReadProcessMemory failed. Error code: {Marshal.GetLastWin32Error()}");
            }
            var result = (T)Marshal.PtrToStructure(buffer, typeof(T));
            Marshal.FreeHGlobal(buffer);
            return result;
        }

        private void Wait(IntPtr hThread)
        {
            var result = Win32API.WaitForSingleObject(hThread, Win32API.INFINITE);
            if (result == Win32API.WAIT_FAILED)
            {
                throw new Exception($"WaitForSingleObject for shell code failed. Error code: {Marshal.GetLastWin32Error()}");
            }
        }

        private void RunShellCode(byte[] shellCode)
        {
            // inject shell code
            var shellCodeSize = (uint)shellCode.Length;
            var pShellCode = Allocate(shellCodeSize, Win32API.MemoryProtection.ExecuteReadWrite);
            Write(pShellCode, shellCode);

            // run
            var hThread = Win32API.CreateRemoteThread(ProcessHandle, IntPtr.Zero, 0, pShellCode, IntPtr.Zero, 0, out _);
            if (hThread == IntPtr.Zero)
            {
                throw new Exception($"CreateRemoteThread failed. Error code: {Marshal.GetLastWin32Error()}");
            }

            // wait for complete
            Wait(hThread);

            // free memory
            Free(pShellCode);
        }

        private IntPtr CreateString(string content)
        {
            // allocate memory
            var bytes = Encoding.ASCII.GetBytes(content + '\0');
            var size = (uint)bytes.Length;
            var pStr = Allocate(size);

            // copy string
            Write(pStr, bytes);
            return pStr;
        }

        public IntPtr Inject(string libPath)
        {
            // get assembly path address
            var pLibPath = CreateString(libPath);

            // get LoadLibrary address
            var hKernel32 = Win32API.GetModuleHandle(Win32API.KERNEL32);
            var pLoadLibrary = Win32API.GetProcAddress(hKernel32, nameof(Win32API.LoadLibraryA));

            // save module address
            var pResult = Allocate(sizeof(ulong));

            // generate LoadLibrary shell code
            var loadLibraryShellCode1 = new byte[]
            {
                                                                               // ; Save rsp and setup stack for function call
                0x53,                                                          // push   rbx
                0x48, 0x89, 0xe3,                                              // mov    rbx, rsp
                0x48, 0x83, 0xec, 0x20,                                        // sub    rsp, 0x20
                0x66, 0x83, 0xe4, 0xc0,                                        // and     sp, 0xffc0
                                                                               // ; Call LoadLibraryA
                0x48, 0xb9, // 0xef, 0xcd, 0xab, 0x89, 0x67, 0x45, 0x23, 0x01, // movabs rcx, 0x0123456789abcdef  ; Address to the string of library name
            };
            var loadLibraryShellCode2 = new byte[]
            {
                0x48, 0xba, // 0xef, 0xcd, 0xab, 0x89, 0x67, 0x45, 0x23, 0x01, // movabs rdx, 0x0123456789abcdef  ; Address of LoadLibrary
            };
            var loadLibraryShellCode3 = new byte[]
            {
                0xff, 0xd2,                                                    // call   rdx
                                                                               // ; Store the result
                0x48, 0xba, // 0xef, 0xcd, 0xab, 0x89, 0x67, 0x45, 0x23, 0x01, // movabs rdx, 0x0123456789abcdef  ; Address to save result
            };
            var loadLibraryShellCode4 = new byte[]
            {
                0x48, 0x89, 0x02,                                              // mov    QWORD PTR [rdx], rax
                                                                               // ; Restore stack
                0x48, 0x89, 0xdc,                                              // mov    rsp, rbx
                0x5b,                                                          // pop    rbx
                0xc3,                                                          // ret
            };
            var shellCodeList = new List<byte>();
            shellCodeList.AddRange(loadLibraryShellCode1);
            shellCodeList.AddRange(pLibPath.ToBytes());
            shellCodeList.AddRange(loadLibraryShellCode2);
            shellCodeList.AddRange(pLoadLibrary.ToBytes());
            shellCodeList.AddRange(loadLibraryShellCode3);
            shellCodeList.AddRange(pResult.ToBytes());
            shellCodeList.AddRange(loadLibraryShellCode4);

            // run shell code
            //Win32API.CreateRemoteThread(ProcessHandle, IntPtr.Zero, 0, pLoadLibrary, pLibPath, 0, out var hThread);
            RunShellCode(shellCodeList.ToArray());

            // get injected module handle
            var hInjectModule = Read<IntPtr>(pResult, sizeof(ulong));

            // free allocated memory
            Free(pLibPath);
            Free(pResult);

            return hInjectModule;
        }

        public uint Call(IntPtr hModule, string procName)
        {
            // get GetProcAddress address
            var hKernel32 = Win32API.GetModuleHandle(Win32API.KERNEL32);
            var pGetProcAddress = Win32API.GetProcAddress(hKernel32, nameof(Win32API.GetProcAddress));

            // get proc name address
            var pProcName = CreateString(procName);

            // save module address
            var pResult = Allocate(sizeof(ulong));

            // generate LoadLibrary shell code
            var getProcAddressShellCode1 = new byte[]
            {
                                                                               // ; Save state of rbx and stack
                0x53,                                                          // push   rbx
                0x48, 0x89, 0xe3,                                              // mov    rbx, rsp
                                                                               // ; Set up stack for function call to GetProcAddress
                0x48, 0x83, 0xec, 0x20,                                        // sub    rsp, 0x20
                0x66, 0x83, 0xe4, 0xc0,                                        // and     sp, 0xffc0
                                                                               // ; Call GetProcAddress
                0x48, 0xb9, // 0xef, 0xcd, 0xab, 0x89, 0x67, 0x45, 0x23, 0x01, // movabs rcx, 0x123456789abcdef  ; Address of the module
            };
            var getProcAddressShellCode2 = new byte[]
            {
                0x48, 0xba, // 0xef, 0xcd, 0xab, 0x89, 0x67, 0x45, 0x23, 0x01, // movabs rdx, 0x123456789abcdef  ; Address to the string of proc name
            };
            var getProcAddressShellCode3 = new byte[]
            {
                0x48, 0xb8, // 0xef, 0xcd, 0xab, 0x89, 0x67, 0x45, 0x23, 0x01, // movabs rax, 0x123456789abcdef  ; Address of GetProcAddress
            };
            var getProcAddressShellCode4 = new byte[]
            {
                0xff, 0xd0,                                                    // call   rax
                                                                               // ; Store the result
                0x48, 0xb9, // 0xef, 0xcd, 0xab, 0x89, 0x67, 0x45, 0x23, 0x01, // movabs rcx, 0x123456789abcdef  ; Address to save result
            };
            var getProcAddressShellCode5 = new byte[]
            {
                0x48, 0x89, 0x01,                                              // mov    QWORD PTR [rcx], rax
                                                                               // ; Restore stack
                0x48, 0x89, 0xdc,                                              // mov    rsp, rbx
                0x5b,                                                          // pop    rbx
                0xc3,                                                          // ret
            };
            var shellCodeList = new List<byte>();
            shellCodeList.AddRange(getProcAddressShellCode1);
            shellCodeList.AddRange(hModule.ToBytes());
            shellCodeList.AddRange(getProcAddressShellCode2);
            shellCodeList.AddRange(pProcName.ToBytes());
            shellCodeList.AddRange(getProcAddressShellCode3);
            shellCodeList.AddRange(pGetProcAddress.ToBytes());
            shellCodeList.AddRange(getProcAddressShellCode4);
            shellCodeList.AddRange(pResult.ToBytes());
            shellCodeList.AddRange(getProcAddressShellCode5);

            // run shell code
            RunShellCode(shellCodeList.ToArray());

            // call proc
            var pProc = Read<IntPtr>(pResult, sizeof(ulong));
            Thread.Sleep(1); // I don't know but it just works
            var hThread = Win32API.CreateRemoteThread(ProcessHandle, IntPtr.Zero, 0, pProc, IntPtr.Zero, 0, out _);
            if (hThread == IntPtr.Zero)
            {
                throw new Exception($"CreateRemoteThread failed. Error code: {Marshal.GetLastWin32Error()}");
            }
            Wait(hThread);

            if (!Win32API.GetExitCodeThread(hThread, out uint exitCode))
            {
                throw new Exception($"GetExitCodeThread failed. Error code: {Marshal.GetLastWin32Error()}");
            }

            // free memory
            Free(pProcName);
            Free(pResult);

            return exitCode;
        }
    }

    public static class Helper
    {
        public static byte[] ToBytes(this IntPtr ptr)
        {
            return BitConverter.GetBytes(ptr.ToInt64());
        }
    }
}
