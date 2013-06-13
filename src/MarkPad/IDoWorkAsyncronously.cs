using System;

namespace MarkPad
{
    public interface IDoWorkAsyncronously
    {
        IDisposable DoingWork(string work);
        void UpdateMessage(string newMessage, IDisposable workDisposible);
    }
}