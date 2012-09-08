using System.Net;
using System.Threading.Tasks;
using RestSharp;

namespace MarkPad.Infrastructure
{
    public static class RestClientExtensions
    {
        public static Task<IRestResponse> ExecuteAwaitableAsync(this IRestClient client, IRestRequest request)
        {
            var completionSource = new TaskCompletionSource<IRestResponse>();
            client.ExecuteAsync(request, (response, asyncHandle) =>
            {
                if (response.ErrorException != null)
                    completionSource.SetException(new WebException(response.Content, response.ErrorException));
                else
                    completionSource.SetResult(response);
            });
            return completionSource.Task;
        }

        public static Task<IRestResponse<T>> ExecuteAwaitableAsync<T>(this RestClient client, IRestRequest request) where T : new()
        {
            var completionSource = new TaskCompletionSource<IRestResponse<T>>();
            client.ExecuteAsync<T>(request, (response, asyncHandle) =>
            {
                if (response.ErrorException != null)
                    completionSource.SetException(new WebException(response.Content, response.ErrorException));
                else
                    completionSource.SetResult(response);
            });
            return completionSource.Task;
        }
    }

}