# Blazor IJSInProcessObjectReference Dispose Bug Test and Fix

Demonstrates aspnetcore bug #48280 "Exception calling Dispose on IJSInProcessObjectReference instance in Blazor WebAssembly" and a fix from from submitted pull request #48287.

Describe the bug
Calling Dispose on an instance of IJSInProcessObjectReference throws an exception. Only affects Blazor WebAssembly.

Affected versions of DotNet
8.0.0-preview.3.23177.8
8.0.0-preview.4.23260.4


The bug seems to arise from pull #46693. They switched from using IJSUnmarshalledRuntime to JSImport for calling "DotNet.jsCallDispatcher.disposeJSObjectReferenceById" but it fails. The JSImport attribute gives an incorrect location for the function to call.

From
"aspnetcore/src/JSInterop/Microsoft.JSInterop/src/Implementation/JSInProcessObjectReference.cs"

```cs
    [JSImport("DotNet.jsCallDispatcher.disposeJSObjectReferenceById", "blazor-internal")]
    private static partial void DisposeJSObjectReferenceById([JSMarshalAs<JSType.Number>] long id);
```

If switched to the below JSImport it works.
```cs
    [JSImport("globalThis.DotNet.jsCallDispatcher.disposeJSObjectReferenceById")]
    private static partial void DisposeJSObjectReferenceById([JSMarshalAs<JSType.Number>] long id);
```

