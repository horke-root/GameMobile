using System;
using UnityEngine;

public static class EventManager
{
    public static event Action<Transform> OnPlayerSpawned;
    public static Transform ActivePlayer { get; set; }

    public static void TriggerPlayerSpawned(Transform playerTransform)
    {
        ActivePlayer = playerTransform;

        Debug.Log($"[EventManager] TriggerPlayerSpawned called. Transform={playerTransform.name}");

        if (OnPlayerSpawned == null)
        {
            Debug.LogWarning("[EventManager] OnPlayerSpawned has ZERO subscribers!");
            return;
        }

        var invocationList = OnPlayerSpawned.GetInvocationList();
        Debug.Log($"[EventManager] OnPlayerSpawned has {invocationList.Length} subscriber(s)");

        foreach (Delegate del in invocationList)
        {
            var handler = del as Action<Transform>;
            if (handler == null) continue;

            try
            {
                if (handler.Target is MonoBehaviour mb && mb == null)
                {
                    Debug.LogWarning($"[EventManager] Removing dead subscriber: {handler.Method.DeclaringType?.Name}.{handler.Method.Name}");
                    OnPlayerSpawned -= handler;
                    continue;
                }

                string targetName = handler.Target is MonoBehaviour liveMb
                    ? $"{liveMb.GetType().Name} on '{liveMb.gameObject.name}'"
                    : handler.Method.DeclaringType?.Name ?? "unknown";
                Debug.Log($"[EventManager] Invoking subscriber: {targetName}.{handler.Method.Name}");

                handler.Invoke(playerTransform);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[EventManager] Exception in subscriber: {ex}");
            }
        }
    }

    public static void ClearAllSubscriptions()
    {
        OnPlayerSpawned = null;
        ActivePlayer = null;
    }

    public static int GetSubscriberCount()
    {
        if (OnPlayerSpawned == null) return 0;
        return OnPlayerSpawned.GetInvocationList().Length;
    }
}
