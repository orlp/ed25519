using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Runtime.InteropServices;

namespace OrlpEd25519
{
    /// <summary>
    /// orlp-ed25519 context class that wraps the native C functions from the orlp-ed25519 library. <para> </para>
    /// Copy this class into your own C# project and then don't forget to
    /// copy the lib/ folder to your own project's build output directory!
    /// </summary>
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
            [DllImport("kernel32.dll")] private static extern IntPtr LoadLibrary(string fileName);

            [DllImport("kernel32.dll")] private static extern int FreeLibrary(IntPtr handle);

            [DllImport("kernel32.dll")] private static extern IntPtr GetProcAddress(IntPtr handle, string procedureName);

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

            [DllImport("libdl.so")] private static extern IntPtr dlopen(String fileName, int flags);

            [DllImport("libdl.so")] private static extern IntPtr dlsym(IntPtr handle, String symbol);

            [DllImport("libdl.so")] private static extern int dlclose(IntPtr handle);

            [DllImport("libdl.so")] private static extern IntPtr dlerror();

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

            [DllImport("libdl.dylib")] private static extern IntPtr dlopen(String fileName, int flags);

            [DllImport("libdl.dylib")] private static extern IntPtr dlsym(IntPtr handle, String symbol);

            [DllImport("libdl.dylib")] private static extern int dlclose(IntPtr handle);

            [DllImport("libdl.dylib")] private static extern IntPtr dlerror();

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

        private delegate int CreateSeedDelegate(
            [MarshalAs(UnmanagedType.LPArray, SizeConst = 32), Out] byte[] outputSeed
        );

        private delegate void CreateKeypairDelegate(
            [MarshalAs(UnmanagedType.LPArray, SizeConst = 32), Out] byte[] outputPublicKey,
            [MarshalAs(UnmanagedType.LPArray, SizeConst = 64), Out] byte[] outputPrivateKey,
            [MarshalAs(UnmanagedType.LPArray), In] byte[] inputSeed
        );

        private delegate void SignDelegate(
            [MarshalAs(UnmanagedType.LPArray, SizeConst = 64), Out] byte[] outputSignature,
            [MarshalAs(UnmanagedType.LPArray), In] byte[] inputMessage,
            [MarshalAs(UnmanagedType.U8), In] ulong inputMessageLength,
            [MarshalAs(UnmanagedType.LPArray), In] byte[] inputPublicKey,
            [MarshalAs(UnmanagedType.LPArray), In] byte[] inputPrivateKey
        );

        private delegate int VerifyDelegate(
            [MarshalAs(UnmanagedType.LPArray), In] byte[] inputSignature,
            [MarshalAs(UnmanagedType.LPArray), In] byte[] inputMessage,
            [MarshalAs(UnmanagedType.U8), In] ulong inputMessageLength,
            [MarshalAs(UnmanagedType.LPArray), In] byte[] inputPublicKey
        );

        private delegate void AddScalarDelegate(
            [MarshalAs(UnmanagedType.LPArray, SizeConst = 32), In, Out] byte[] publicKey,
            [MarshalAs(UnmanagedType.LPArray, SizeConst = 64), In, Out] byte[] privateKey,
            [MarshalAs(UnmanagedType.LPArray), In] byte[] inputScalar
        );

        private delegate void KeyExchangeDelegate(
            [MarshalAs(UnmanagedType.LPArray, SizeConst = 32), Out] byte[] outputSharedSecret,
            [MarshalAs(UnmanagedType.LPArray), In] byte[] inputPublicKey,
            [MarshalAs(UnmanagedType.LPArray), In] byte[] inputPrivateKey
        );

        private delegate void Ref10KeyConversionDelegate(
            [MarshalAs(UnmanagedType.LPArray), In] byte[] inputPrivateKey,
            [MarshalAs(UnmanagedType.LPArray, SizeConst = 64), Out] byte[] outputPrivateKey
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
        private Ref10KeyConversionDelegate ref10KeyConversionDelegate;

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

            if (!string.IsNullOrEmpty(sharedLibPathOverride))
            {
                LoadedLibraryPath = sharedLibPathOverride;
            }
            else
            {
                string cpu = RuntimeInformation.ProcessArchitecture switch
                {
                    Architecture.X64 => "x64",
                    Architecture.X86 => "x86",
                    Architecture.Arm => "armeabi-v7a",
                    Architecture.Arm64 => "arm64-v8a",
                    _ => throw new PlatformNotSupportedException("CPU Architecture not supported!")
                };

                string path = Path.Combine(Path.GetFullPath(Path.GetDirectoryName(Assembly.GetCallingAssembly().Location) ?? "."), "lib", cpu, os);

                if (!Directory.Exists(path))
                {
                    throw new PlatformNotSupportedException($"Shared library not found in {path} and/or unsupported CPU architecture. Please don't forget to copy the shared libraries/DLL into the 'lib/{{CPU_ARCHITECTURE}}/{{OS}}/{{SHARED_LIB_FILE}}' folder of your output build directory. ");
                }

                bool found = false;
                foreach (string file in Directory.GetFiles(path))
                {
                    if (file.ToLower().Contains("ed25519"))
                    {
                        LoadedLibraryPath = Path.GetFullPath(Path.Combine(path, file));
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    throw new FileLoadException($"Shared library not found in {path} and/or unsupported CPU architecture. Please don't forget to copy the shared libraries/DLL into the 'lib/{{CPU_ARCHITECTURE}}/{{OS}}/{{SHARED_LIB_FILE}}' folder of your output build directory. ");
                }
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

            IntPtr keyConvert = loadUtils.GetProcAddress(lib, "ed25519_key_convert_ref10_to_orlp");
            if (keyConvert == IntPtr.Zero)
            {
                goto hell;
            }

            createSeedDelegate = Marshal.GetDelegateForFunctionPointer<CreateSeedDelegate>(createSeed);
            createKeypairDelegate = Marshal.GetDelegateForFunctionPointer<CreateKeypairDelegate>(createKeypair);
            signDelegate = Marshal.GetDelegateForFunctionPointer<SignDelegate>(sign);
            verifyDelegate = Marshal.GetDelegateForFunctionPointer<VerifyDelegate>(verify);
            addScalarDelegate = Marshal.GetDelegateForFunctionPointer<AddScalarDelegate>(addScalar);
            keyExchangeDelegate = Marshal.GetDelegateForFunctionPointer<KeyExchangeDelegate>(keyExchange);
            ref10KeyConversionDelegate = Marshal.GetDelegateForFunctionPointer<Ref10KeyConversionDelegate>(keyConvert);

            return;

            hell:
            throw new Exception($"Failed to load one or more functions from the orlp-ed25519 shared library \"{LoadedLibraryPath}\"!");
        }

        public void Dispose()
        {
            loadUtils.FreeLibrary(lib);
        }

        /// <summary>
        /// Creates a 32B random seed usable for key generation.
        /// </summary>
        /// <returns>Returns 0 on success, and <c>null</c> on failure.</returns>
        public byte[] CreateSeed()
        {
            byte[] output = new byte[32];
            return createSeedDelegate(output) == 0 ? output : null;
        }

        /// <summary>
        /// Creates a new key pair from the given seed.
        /// </summary>
        /// <param name="seed">32B seed value to use for key generation.</param>
        /// <param name="outputPublicKey">Output byte array into which the 32B public key will be written.</param>
        /// <param name="outputPrivateKey">Output byte array into which the 64B private key will be written.</param>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="seed"/> argument is either <c>null</c> or too small (needs to be at least 32B).</exception>
        public void CreateKeypair(byte[] seed, out byte[] outputPublicKey, out byte[] outputPrivateKey)
        {
            if (seed is null || seed.Length < 32)
            {
                throw new ArgumentException("Seed parameter null or too small!");
            }

            byte[] publicKey = new byte[32];
            byte[] privateKey = new byte[64];

            createKeypairDelegate(publicKey, privateKey, seed);

            outputPublicKey = publicKey;
            outputPrivateKey = privateKey;
        }

        /// <summary>
        /// Creates a signature of the given message with the given key pair.
        /// </summary>
        /// <param name="message">The message bytes to sign.</param>
        /// <param name="publicKey">Public key bytes (32B byte[] array).</param>
        /// <param name="privateKey">Private key bytes (64B byte[] array).</param>
        /// <returns>The signature as a 64B byte[] array.</returns>
        /// <exception cref="ArgumentNullException">Thrown when one or more arguments were <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown if key arrays of invalid length were passed into the function.</exception>
        public byte[] Sign(byte[] message, byte[] publicKey, byte[] privateKey)
        {
            if (message is null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            if (publicKey is null)
            {
                throw new ArgumentNullException(nameof(publicKey));
            }

            if (privateKey is null)
            {
                throw new ArgumentNullException(nameof(privateKey));
            }

            if (publicKey.Length != 32 || privateKey.Length != 64)
            {
                throw new ArgumentException("Public or private key of invalid length passed... Please pass only 32B arrays for the public key and 64B for the private key!");
            }

            byte[] output = new byte[64];
            for (int i = 0; i < output.Length; i++)
            {
                output[i] = 0x00;
            }

            signDelegate(output, message, (ulong)message.LongLength, publicKey, privateKey);
            return output;
        }

        /// <summary>
        /// Verifies the signature of a given message using a specific public key.
        /// </summary>
        /// <param name="signature">The signature to verify (64B byte[] array).</param>
        /// <param name="message">The message whose signature you want to verify.</param>
        /// <param name="publicKey">The public key with which to verify the signature (32B byte[] array).</param>
        /// <returns>Whether the signature is valid or not.</returns>
        /// <exception cref="ArgumentNullException">Thrown if one or more input parameter arrays were <c>null</c>.</exception>
        public bool Verify(byte[] signature, byte[] message, byte[] publicKey)
        {
            if (signature is null)
            {
                throw new ArgumentNullException(nameof(signature));
            }

            if (message is null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            if (publicKey is null)
            {
                throw new ArgumentNullException(nameof(publicKey));
            }

            if (signature.Length != 64 || publicKey.Length != 32)
            {
                return false;
            }

            return verifyDelegate(signature, message, (ulong)message.LongLength, publicKey) == 1;
        }

        /// <summary>
        /// Adds a scalar to the given key pair where scalar is a 32 byte buffer
        /// (possibly generated with <see cref="CreateSeed"/>), generating a new key pair.<para> </para>
        /// You can calculate the public key sum without knowing the private key and vice versa
        /// by passing in <c>null</c> for the key you don't know.<para> </para>
        /// This is useful for enforcing randomness on a key pair by a third party while only knowing the public key, among other things.<para> </para>
        /// Warning: the last bit of the scalar is ignored - if comparing scalars make sure to clear it with <c>scalar[31] &= 127</c>.
        /// </summary>
        /// <param name="publicKey">Public key (this will be overwritten with the added scalar output public key).</param>
        /// <param name="privateKey">Private key (this will be overwritten with the added scalar output private key).</param>
        /// <param name="scalar">The scalar to add (32B array).</param>
        /// <exception cref="ArgumentNullException">Thrown if the <paramref name="scalar"/> argument is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown if the passed arrays are both null or have invalid length. <paramref name="publicKey"/> and <paramref name="scalar"/> must be a 32B byte[] array, <paramref name="privateKey"/> 64B</exception>
        public void AddScalar(byte[] publicKey, byte[] privateKey, byte[] scalar)
        {
            if (scalar is null)
            {
                throw new ArgumentNullException(nameof(scalar));
            }

            if (scalar.Length < 32)
            {
                throw new ArgumentException($"{nameof(scalar)} argument must be a 32B array!", nameof(scalar));
            }

            if (publicKey is null && privateKey is null)
            {
                throw new ArgumentException("Both keys null!");
            }

            if ((publicKey != null && publicKey.Length != 32) || (privateKey != null && privateKey.Length != 64))
            {
                throw new ArgumentException("One or more arguments invalid: public key and scalar must be a 32B byte[] array, private key a 64B array!");
            }

            addScalarDelegate(publicKey, privateKey, scalar);
        }

        /// <summary>
        /// Performs a key exchange on the given public key and private key, producing a shared secret.<para> </para>
        /// It is recommended to hash the shared secret before using it.
        /// </summary>
        /// <param name="inputPublicKey">Public key bytes (32B byte[] array).</param>
        /// <param name="inputPrivateKey">Private key bytes (64B byte[] array).</param>
        /// <returns>The shared secret 32B byte[] array.</returns>
        /// <exception cref="ArgumentNullException">Thrown if one or both input key parameter arrays were <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown if one or both passed keys have an invalid length (private key needs to be exactly 64B and public key 32B).</exception>
        public byte[] KeyExchange(byte[] inputPublicKey, byte[] inputPrivateKey)
        {
            if (inputPublicKey is null)
            {
                throw new ArgumentNullException(nameof(inputPublicKey));
            }

            if (inputPrivateKey is null)
            {
                throw new ArgumentNullException(nameof(inputPrivateKey));
            }

            if (inputPublicKey.Length != 32)
            {
                throw new ArgumentException($"{nameof(inputPublicKey)} argument doesn't have the correct public key byte array length of 32B");
            }

            if (inputPrivateKey.Length != 64)
            {
                throw new ArgumentException($"{nameof(inputPrivateKey)} argument doesn't have the correct private key byte array length of 64B");
            }

            byte[] output = new byte[32];
            keyExchangeDelegate(output, inputPublicKey, inputPrivateKey);
            return output;
        }

        /// <summary>
        /// Converts a Ref10 private key (as generated e.g. from libsodium) into the orlp-ed25519 private key format.
        /// </summary>
        /// <param name="ref10PrivateKey">The 64B private key to convert.</param>
        /// <returns>The converted 64B private orlp-ed25519 key.</returns>
        /// <exception cref="ArgumentException">Thrown if the passed <paramref name="ref10PrivateKey"/> array is <c>null</c> or not exactly 64 bytes long.</exception>
        public byte[] ConvertRef10ToOrlpPrivateKeyFormat(byte[] ref10PrivateKey)
        {
            if (ref10PrivateKey is null || ref10PrivateKey.Length != 64)
            {
                throw new ArgumentException("Null or invalid private key: needs to be a 64B ref10 private key (e.g. one generated by libsodium).");
            }

            byte[] output = new byte[64];
            ref10KeyConversionDelegate(ref10PrivateKey, output);
            return output;
        }
    }


    //  --------------------------------------------------------------------
    //  ------------------------------> DEMO <------------------------------
    //  --------------------------------------------------------------------


    /// <summary>
    /// Just an example class, don't copy this to your project!
    /// </summary>
    internal class OrlpEd25519ContextExample
    {
        static void Main()
        {
            static string BytesToHexString(byte[] bytes)
            {
                return BitConverter.ToString(bytes).Replace("-", "");
            }

            Console.WriteLine("\n--- orlp-ed25519 ---\n");

            const string messageString = "Rise and shine, Dr. Freeman... rise and... shine!";

            using var ed25519 = new OrlpEd25519Context();

            byte[] seed = ed25519.CreateSeed();

            Console.WriteLine($"Generated seed: {BytesToHexString(seed)}\n");

            ed25519.CreateKeypair(seed, out byte[] publicKey, out byte[] privateKey);

            Console.WriteLine($"Generated public key: {BytesToHexString(publicKey)}\n");
            Console.WriteLine($"Generated private key: {BytesToHexString(privateKey)}\n");

            byte[] message = Encoding.UTF8.GetBytes(messageString);

            Console.WriteLine($"Message: {messageString}\n");

            byte[] signature = ed25519.Sign(message, publicKey, privateKey);

            Console.WriteLine($"Signature: {BytesToHexString(signature)}\n");

            bool valid = ed25519.Verify(signature, message, publicKey);

            Console.WriteLine($"Valid: {valid}\n");

            byte[] seed2 = ed25519.CreateSeed();

            ed25519.AddScalar(publicKey, privateKey, seed2);

            Console.WriteLine($"Public key after adding scalar \"{BytesToHexString(seed2)}\": {BytesToHexString(publicKey)}\n");
            Console.WriteLine($"Private key after adding scalar \"{BytesToHexString(seed2)}\": {BytesToHexString(privateKey)}\n");
        }
    }
}