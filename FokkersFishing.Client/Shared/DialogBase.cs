using MatBlazor;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FokkersFishing.Client
{
    public class DialogBase : BaseMatDialog, IDisposable
    {
        [Inject]
        protected IJSRuntime JsInterop { get; set; }

        new public void Dispose()
        {
            base.Dispose();

            // This solves a bug in MatBlazor that could be solved in future by a NuGet package upgrade.
            // developed against MatBlazor (2.6.2)
            this.JsInterop.InvokeVoidAsync("fudge.makeWindowScrollbarVisibleIfNeeded", null);
        }
    }
}
