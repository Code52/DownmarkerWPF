using System;

namespace MarkPad
{
    public interface IShell
    {
        IDisposable DoingWork(string work);
    }
}