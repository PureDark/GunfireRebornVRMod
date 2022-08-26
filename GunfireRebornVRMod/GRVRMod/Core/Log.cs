﻿using BepInEx.Logging;

namespace VRMod.Core
{
    public static class Log
    {
        static ManualLogSource log;

        public static void Setup(ManualLogSource log)
        {
            Log.log = log;
        }

        public static void Warning(string msg)
        {
            log.LogWarning(msg);
        }

        public static void Error(string msg)
        {
            log.LogError(msg);
        }

        public static void Info(string msg)
        {
            log.LogInfo(msg);
        }

        public static void Debug(string msg)
        {
            log.LogDebug(msg);
        }
    }
}
