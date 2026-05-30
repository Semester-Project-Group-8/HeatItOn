using System;

namespace Frontend.Data;

public class PopupHub
{
    public event Action<string>? MessageReceived;

    public void Send(string message)
    {
        MessageReceived?.Invoke(message);
    }

}