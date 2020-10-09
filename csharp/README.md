# C# wrapper class for orlp-ed25519

In order to use this, just copy the [`OrlpEd25519Context`](https://github.com/GlitchedPolygons/GlitchEd25519/blob/master/csharp/OrlpEd25519.cs) 
class into your own C# project and manually copy the [`lib/`](https://github.com/GlitchedPolygons/GlitchEd25519/tree/master/csharp/lib) folder into your
own project's build output directory (otherwise the `OrlpEd25519Context` wrapper class doesn't know where to load the DLL/shared lib from; it needs to be in that specific path).
