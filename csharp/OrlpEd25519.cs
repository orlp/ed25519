using System;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;

namespace OrlpEd25519
{
    public class OrlpEd25519Context : IDisposable
    {
        #region Shared library loaders (per platform implementations)

        private interface ISharedLibLoadUtils
        {
            IntPtr LoadLibrary(string fileName);
            void FreeLibrary(IntPtr handle);
            IntPtr GetProcAddress(IntPtr handle, string name);
        }

        private class SharedLibLoadUtilsWindows : ISharedLibLoadUtils
        {
            [DllImport("kernel32.dll")]
            private static extern IntPtr LoadLibrary(string fileName);

            [DllImport("kernel32.dll")]
            private static extern int FreeLibrary(IntPtr handle);

            [DllImport("kernel32.dll")]
            private static extern IntPtr GetProcAddress(IntPtr handle, string procedureName);

            void ISharedLibLoadUtils.FreeLibrary(IntPtr handle)
            {
                FreeLibrary(handle);
            }

            IntPtr ISharedLibLoadUtils.GetProcAddress(IntPtr dllHandle, string name)
            {
                return GetProcAddress(dllHandle, name);
            }

            IntPtr ISharedLibLoadUtils.LoadLibrary(string fileName)
            {
                return LoadLibrary(fileName);
            }
        }

        private class SharedLibLoadUtilsLinux : ISharedLibLoadUtils
        {
            const int RTLD_NOW = 2;

            [DllImport("libdl.so")]
            private static extern IntPtr dlopen(String fileName, int flags);

            [DllImport("libdl.so")]
            private static extern IntPtr dlsym(IntPtr handle, String symbol);

            [DllImport("libdl.so")]
            private static extern int dlclose(IntPtr handle);

            [DllImport("libdl.so")]
            private static extern IntPtr dlerror();

            public IntPtr LoadLibrary(string fileName)
            {
                return dlopen(fileName, RTLD_NOW);
            }

            public void FreeLibrary(IntPtr handle)
            {
                dlclose(handle);
            }

            public IntPtr GetProcAddress(IntPtr dllHandle, string name)
            {
                dlerror();
                IntPtr res = dlsym(dllHandle, name);
                IntPtr err = dlerror();
                if (err != IntPtr.Zero)
                {
                    throw new Exception("dlsym: " + Marshal.PtrToStringAnsi(err));
                }

                return res;
            }
        }

        private class SharedLibLoadUtilsMac : ISharedLibLoadUtils
        {
            const int RTLD_NOW = 2;

            [DllImport("libdl.dylib")]
            private static extern IntPtr dlopen(String fileName, int flags);

            [DllImport("libdl.dylib")]
            private static extern IntPtr dlsym(IntPtr handle, String symbol);

            [DllImport("libdl.dylib")]
            private static extern int dlclose(IntPtr handle);

            [DllImport("libdl.dylib")]
            private static extern IntPtr dlerror();

            public IntPtr LoadLibrary(string fileName)
            {
                return dlopen(fileName, RTLD_NOW);
            }

            public void FreeLibrary(IntPtr handle)
            {
                dlclose(handle);
            }

            public IntPtr GetProcAddress(IntPtr dllHandle, string name)
            {
                dlerror();
                IntPtr res = dlsym(dllHandle, name);
                IntPtr err = dlerror();
                if (err != IntPtr.Zero)
                {
                    throw new Exception("dlsym: " + Marshal.PtrToStringAnsi(err));
                }

                return res;
            }
        }

        #endregion

        #region Function mapping

        /*
         * 
#ifndef ED25519_NO_SEED
int ORLP_ED25519_DECLSPEC ed25519_create_seed(unsigned char *seed);
#endif

void ORLP_ED25519_DECLSPEC ed25519_create_keypair(unsigned char *public_key, unsigned char *private_key, const unsigned char *seed);
void ORLP_ED25519_DECLSPEC ed25519_sign(unsigned char *signature, const unsigned char *message, size_t message_len, const unsigned char *public_key, const unsigned char *private_key);
int ORLP_ED25519_DECLSPEC ed25519_verify(const unsigned char *signature, const unsigned char *message, size_t message_len, const unsigned char *public_key);
void ORLP_ED25519_DECLSPEC ed25519_add_scalar(unsigned char *public_key, unsigned char *private_key, const unsigned char *scalar);
void ORLP_ED25519_DECLSPEC ed25519_key_exchange(unsigned char *shared_secret, const unsigned char *public_key, const unsigned char *private_key);

         */

        private delegate int CreateSeedDelegate(
            [MarshalAs(UnmanagedType.LPArray)] byte[] outputSeed
        );

        private delegate void CreateKeypairDelegate(
            [MarshalAs(UnmanagedType.LPArray)] byte[] outputPublicKey,
            [MarshalAs(UnmanagedType.LPArray)] byte[] outputPrivateKey,
            [MarshalAs(UnmanagedType.LPArray)] byte[] inputSeed
        );

        private delegate void SignDelegate(
            [MarshalAs(UnmanagedType.LPArray)] byte[] outputSignature,
            [MarshalAs(UnmanagedType.LPArray)] byte[] inputMessage,
            [MarshalAs(UnmanagedType.U8)] ulong inputMessageLength,
            [MarshalAs(UnmanagedType.LPArray)] byte[] inputPublicKey,
            [MarshalAs(UnmanagedType.LPArray)] byte[] inputPrivateKey
        );

        private delegate int VerifyDelegate(
            [MarshalAs(UnmanagedType.LPArray)] byte[] outputSignature,
            [MarshalAs(UnmanagedType.LPArray)] byte[] inputMessage,
            [MarshalAs(UnmanagedType.U8)] ulong inputMessageLength,
            [MarshalAs(UnmanagedType.LPArray)] byte[] inputPublicKey
        );

        private delegate void AddScalarDelegate(
            [MarshalAs(UnmanagedType.LPArray)] byte[] outputPublicKey,
            [MarshalAs(UnmanagedType.LPArray)] byte[] outputPrivateKey,
            [MarshalAs(UnmanagedType.LPArray)] byte[] inputScalar
        );

        private delegate void KeyExchangeDelegate(
            [MarshalAs(UnmanagedType.LPArray)] byte[] outputSharedSecret,
            [MarshalAs(UnmanagedType.LPArray)] byte[] inputPublicKey,
            [MarshalAs(UnmanagedType.LPArray)] byte[] inputPrivateKey
        );

        #endregion

        private IntPtr lib;
        private ISharedLibLoadUtils loadUtils = null;

        private CreateSeedDelegate createSeedDelegate;
        private CreateKeypairDelegate createKeypairDelegate;
        private SignDelegate signDelegate;
        private VerifyDelegate verifyDelegate;
        private AddScalarDelegate addScalarDelegate;
        private KeyExchangeDelegate keyExchangeDelegate;

        /// <summary>
        /// Absolute path to the orlp-ed25519 shared library that is currently loaded into memory.
        /// </summary>
        public string LoadedLibraryPath { get; }

        public OrlpEd25519Context(string sharedLibPathOverride = null)
        {
            string os;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                os = "windows";
                loadUtils = new SharedLibLoadUtilsWindows();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                os = "linux";
                loadUtils = new SharedLibLoadUtilsLinux();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                os = "mac";
                loadUtils = new SharedLibLoadUtilsMac();
            }
            else
            {
                throw new PlatformNotSupportedException("Unsupported OS");
            }

            if (string.IsNullOrEmpty(sharedLibPathOverride))
            {
                StringBuilder pathBuilder = new StringBuilder(256);
                pathBuilder.Append("lib/");

                switch (RuntimeInformation.ProcessArchitecture)
                {
                    case Architecture.X64:
                        pathBuilder.Append("x64/");
                        break;
                    case Architecture.X86:
                        pathBuilder.Append("x86/");
                        break;
                    case Architecture.Arm:
                        pathBuilder.Append("armeabi-v7a/");
                        break;
                    case Architecture.Arm64:
                        pathBuilder.Append("arm64-v8a/");
                        break;
                }

                if (!Directory.Exists(pathBuilder.ToString()))
                {
                    throw new PlatformNotSupportedException($"orlp-ed25519 shared library not found in {pathBuilder.ToString()} and/or unsupported CPU architecture. Please don't forget to copy the orlp-ed25519 shared libraries/DLL into the 'lib/{{CPU_ARCHITECTURE}}/{{OS}}/{{SHARED_LIB_FILE}}' folder of your output build directory.");
                }

                pathBuilder.Append(os);
                pathBuilder.Append('/');

                string[] l = Directory.GetFiles(pathBuilder.ToString());
                if (l == null || l.Length != 1)
                {
                    throw new FileLoadException("There should only be exactly one orlp-ed25519 shared library file per supported platform!");
                }

                pathBuilder.Append(Path.GetFileName(l[0]));
                LoadedLibraryPath = Path.GetFullPath(pathBuilder.ToString());
                pathBuilder.Clear();
            }
            else
            {
                LoadedLibraryPath = sharedLibPathOverride;
            }

            lib = loadUtils.LoadLibrary(LoadedLibraryPath);
            if (lib == IntPtr.Zero)
            {
                goto hell; // The gates of hell opened, and out came the beginning of marshalling, DLL hell and C# interop...
            }

            IntPtr createSeed = loadUtils.GetProcAddress(lib, "ed25519_create_seed");
            if (createSeed == IntPtr.Zero)
            {
                goto hell;
            }

            IntPtr createKeypair = loadUtils.GetProcAddress(lib, "ed25519_create_keypair");
            if (createKeypair == IntPtr.Zero)
            {
                goto hell;
            }

            IntPtr sign = loadUtils.GetProcAddress(lib, "ed25519_sign");
            if (sign == IntPtr.Zero)
            {
                goto hell;
            }

            IntPtr verify = loadUtils.GetProcAddress(lib, "ed25519_verify");
            if (verify == IntPtr.Zero)
            {
                goto hell;
            }

            IntPtr addScalar = loadUtils.GetProcAddress(lib, "ed25519_add_scalar");
            if (addScalar == IntPtr.Zero)
            {
                goto hell;
            }

            IntPtr keyExchange = loadUtils.GetProcAddress(lib, "ed25519_key_exchange");
            if (keyExchange == IntPtr.Zero)
            {
                goto hell;
            }

            createSeedDelegate = Marshal.GetDelegateForFunctionPointer<CreateSeedDelegate>(createSeed);
            createKeypairDelegate = Marshal.GetDelegateForFunctionPointer<CreateKeypairDelegate>(createKeypair);
            signDelegate = Marshal.GetDelegateForFunctionPointer<SignDelegate>(sign);
            verifyDelegate = Marshal.GetDelegateForFunctionPointer<VerifyDelegate>(verify);
            addScalarDelegate = Marshal.GetDelegateForFunctionPointer<AddScalarDelegate>(addScalar);
            keyExchangeDelegate = Marshal.GetDelegateForFunctionPointer<KeyExchangeDelegate>(keyExchange);

            return;

            hell:
            throw new Exception($"Failed to load one or more functions from the orlp-ed25519 shared library \"{LoadedLibraryPath}\"!");
        }

        public void Dispose()
        {
            loadUtils.FreeLibrary(lib);
        }
    }

    internal class OrlpEd25519ContextExample
    {
        static void Main()
        {
            Console.WriteLine("--- orlp-ed25519 ---\n");
        }
    }
}