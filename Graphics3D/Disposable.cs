using System;

namespace Graphics3D
{
    internal class Disposable : IDisposable
    {
        private readonly Action disposeAction;

        private Disposable(Action action)
        {
            disposeAction = action;
        }

        public static IDisposable Create(Action disposeAction)
        {
            return new Disposable(disposeAction);
        }

        public void Dispose()
        {
            disposeAction();
        }
    }
}
