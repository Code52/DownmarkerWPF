using System;

namespace MarkPad
{
    public interface IDoWorkAsyncronously
    {
        IDisposable DoingWork(string work);
        IDisposable UpdateMessage(string newMessage, IDisposable workDisposible);
    }
}