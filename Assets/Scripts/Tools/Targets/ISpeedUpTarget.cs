using System;

public interface ISpeedUpTarget
{
    bool CanSpeedUp { get; }
    DateTime CompletionTimeUtc { get; }
    bool TryCompleteNow();
}
