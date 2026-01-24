using System;

public static class EventBus
{
    // Zeo 射线点到物体时调用。
    public static Action<string> OnInteractionTriggered;

    // Echo对话结束时调用，告诉 Zeo 恢复玩家控制。
    public static Action OnDialogueEnded;
}