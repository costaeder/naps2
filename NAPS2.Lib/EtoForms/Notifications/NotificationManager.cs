namespace NAPS2.EtoForms.Notifications;

public class NotificationManager
{
    public List<NotificationModel> Notifications { get; } = new();

    public event EventHandler? Updated;

    public event EventHandler? TimersStarting;

    public void Show(NotificationModel notification)
    {
        Notifications.Add(notification);
        Updated?.Invoke(this, EventArgs.Empty);
    }

    public void Hide(NotificationModel notification)
    {
        lock(Notifications)
        {
            if (Notifications.Remove(notification))
            {
                Updated?.Invoke(this, EventArgs.Empty);
            }
        }

       

    }

    public void StartTimers()
    {
        TimersStarting?.Invoke(this, EventArgs.Empty);
    }

    public void InvokeUpdated()
    {
        Updated?.Invoke(this, EventArgs.Empty);
    }
}