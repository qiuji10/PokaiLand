using System;
using Cysharp.Threading.Tasks;

namespace PokaiLand.Utility
{
    public class RpcHandler
    {
        private UniTaskCompletionSource<bool> _promise;

        public UniTask Run(Action serverRpc)
        {
            _promise = new UniTaskCompletionSource<bool>();
            serverRpc();
            return _promise.Task;
        }

        public void SetCompleted()
        {
            _promise.TrySetResult(true);
        }
    
        public void SetException(Exception exception)
        {
            _promise.TrySetException(exception);
        }
    }
    
    public class RpcHandler<T>
    {
        private UniTaskCompletionSource<T> _promise;

        public UniTask<T> Run(Action serverRpc)
        {
            _promise = new UniTaskCompletionSource<T>();
            serverRpc();
            return _promise.Task;
        }

        public void SetResult(T result)
        {
            _promise.TrySetResult(result);
        }
        
        public void SetException(Exception exception)
        {
            _promise.TrySetException(exception);
        }
    }
}