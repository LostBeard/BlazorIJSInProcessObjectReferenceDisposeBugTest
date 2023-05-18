using BlazorIJSObjectRefBugTest;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.JSInterop;
using Microsoft.JSInterop.Implementation;
using System.Reflection;
using System.Runtime.InteropServices.JavaScript;


namespace BlazorJSObjectRefBugTest
{
    public partial class BugFix
    {
        // Below is a slightly modified version of the JSImport fix for demonstration purposes.
        // The JSImport attribute is exactly how it should be in 
        // aspnetcore/src/JSInterop/Microsoft.JSInterop/src/Implementation/JSInProcessObjectReference.cs
        // To fix the bug replace line 47 with the line below
        [JSImport("globalThis.DotNet.jsCallDispatcher.disposeJSObjectReferenceById")]
        public static partial void DisposeJSObjectReferenceById([JSMarshalAs<JSType.Number>] long id);
    }

    public class Program
    {
        static async Task DemoBug(IJSRuntime js, bool useFixedJSImport)
        {
            if (useFixedJSImport)
            {
                Console.WriteLine("Testing fixed Dispose method");
            }
            else
            {
                Console.WriteLine("Testing bugged Dispose method");
            }
            var jsRef = await js.InvokeAsync<IJSInProcessObjectReference>("caches.open", "default");
            // below line proves we have a valid object reference, if we did not it would throw an exception
            var unUsed = jsRef.Invoke<bool>("hasOwnProperty", "doesnotexist");
            // now Dispose using selected method
            try
            {
                if (!useFixedJSImport)
                {
                    jsRef.Dispose();
                }
                else
                {
                    // For demonstrating the fixed JSImport we need the Id of the object to Dispose
                    var jsRefId = (long)typeof(JSObjectReference).GetProperty("Id", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(jsRef);
                    BugFix.DisposeJSObjectReferenceById(jsRefId);
                }
            }
            catch
            {
                Console.WriteLine("Dispose failed");
                return;
            }
            // Test object reference is actually disposed by trying to use it
            try
            {
                // this call will throw an exception becuase jsRef is now disposed
                var hasProp2 = jsRef.Invoke<bool>("hasOwnProperty", "doesnotexist");
            }
            catch
            {
                Console.WriteLine("jsRef is properly disposed");
            }
        }
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");
            builder.RootComponents.Add<HeadOutlet>("head::after");

            var host = builder.Build();

            var js = host.Services.GetRequiredService<IJSRuntime>();

            // Demonstrate bugged Dispose
            await DemoBug(js, useFixedJSImport: false);
            // Output in console (excluding large Assert exception)
            //
            // Testing bugged Dispose method
            // Error: Assert failed: DotNet not found while looking up DotNet.jsCallDispatcher.disposeJSObjectReferenceById ...
            // Dispose failed

            // Demonstrate fixed Dispose
            await DemoBug(js, useFixedJSImport: true);
            // Output in console
            //
            // Testing fixed Dispose method
            // jsRef is properly disposed

            await host.RunAsync();
        }
    }
}
