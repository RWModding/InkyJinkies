using System.Reflection;
using System.Threading;
using MonoMod.RuntimeDetour;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace InkyJinkies;

public static class DebugFix
{
    private static MethodBase Shader_EnableKeyword_Trampoline;
    private static MethodBase Shader_DisableKeyword_Trampoline;

    public static void Apply()
    {
        Shader_EnableKeyword_Trampoline = new NativeDetour(Shader.EnableKeyword, Shader_EnableKeyword).GenerateTrampoline(typeof(Shader).GetMethod(nameof(Shader.EnableKeyword)));
        Shader_DisableKeyword_Trampoline = new NativeDetour(Shader.DisableKeyword, Shader_DisableKeyword).GenerateTrampoline(typeof(Shader).GetMethod(nameof(Shader.DisableKeyword)));
    }

    public static void Shader_EnableKeyword(string keyword)
    {
        if (Plugin.MainThread.ManagedThreadId == Thread.CurrentThread.ManagedThreadId)
        {
            Shader_EnableKeyword_Trampoline.Invoke(null, new object[] { keyword });
        }
        else
        {
            lock (Plugin.RunOnMainThread)
            {
                Plugin.RunOnMainThread.Enqueue(() => Shader_EnableKeyword_Trampoline.Invoke(null, new object[] { keyword }));
            }
        }
    }

    public static void Shader_DisableKeyword(string keyword)
    {
        if (Plugin.MainThread.ManagedThreadId == Thread.CurrentThread.ManagedThreadId)
        {
            Shader_DisableKeyword_Trampoline.Invoke(null, new object[] { keyword });
        }
        else
        {
            lock (Plugin.RunOnMainThread)
            {
                Plugin.RunOnMainThread.Enqueue(() => Shader_DisableKeyword_Trampoline.Invoke(null, new object[] { keyword }));
            }
        }
    }
}