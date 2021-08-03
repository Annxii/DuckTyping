using System;
using System.Runtime.CompilerServices;

namespace DuckTyping
{
    class DuckTask
    {
        public struct DuckAwaiter<T> : INotifyCompletion
        {
            private T item;

            public DuckAwaiter(T item)
            {
                this.item = item;
            }

            public bool IsCompleted => true;
            public T GetResult() => item;

            public void OnCompleted(Action continuation)
            {
                throw new NotImplementedException();
            }
        }

        private string item;

        public DuckTask(string item)
        {
            this.item = item;
        }

        public DuckAwaiter<string> GetAwaiter()
        {
            return new DuckAwaiter<string>(item);
        }
    }
}
