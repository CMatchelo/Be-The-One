using System.Threading.Tasks;
using UnityEngine.Localization;
using UnityEngine.ResourceManagement.AsyncOperations;
public class SearchTextLocation
{
    public static Task<string> GetLocalizedStringAsync(string table, string key)
    {
        var localizedString = new LocalizedString(table, key);
        var handle = localizedString.GetLocalizedStringAsync();

        var tcs = new TaskCompletionSource<string>();

        handle.Completed += op =>
        {
            if (op.Status == AsyncOperationStatus.Succeeded)
                tcs.SetResult(op.Result);
            else
                tcs.SetException(new System.Exception("Localization failed"));
        };

        return tcs.Task;
    }
}
