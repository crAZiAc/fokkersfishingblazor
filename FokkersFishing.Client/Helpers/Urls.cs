using System;
namespace FokkersFishing.Client
{
    public class UrlState
    {
        public string BackendUrl { get; set; }
        public event Action OnChange;

        public void SetUrl(string url)
        {
            BackendUrl = url;
            NotifyStateChanged();
        }
        private void NotifyStateChanged()
        {
            OnChange?.Invoke();
        }
    } // end c
} // end ns
