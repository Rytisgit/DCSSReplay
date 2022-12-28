using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace DCSSTV.Helpers
{
    class DialogManager
    {
        private static readonly SemaphoreSlim _oneAtATimeAsync = new SemaphoreSlim(1, 1);

        internal static async Task<T> OneAtATimeAsync<T>(Func<Task<T>> show, TimeSpan? timeout, CancellationToken? token)
        {
            var to = timeout ?? TimeSpan.FromHours(1);
            var tk = token ?? new CancellationToken(false);
            if (!await _oneAtATimeAsync.WaitAsync(to, tk))
            {
                throw new Exception($"{nameof(DialogManager)}.{nameof(OneAtATimeAsync)} has timed out.");
            }
            try
            {
                return await show();
            }
            finally
            {
                _oneAtATimeAsync.Release();
            }
        }
    }
}
