using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using HarmonyLib;

namespace K8Lib.Commands
{
    public class ConsoleCommand
    {
        public string CommandName { get; private set; }
        public Action<int> OnCommandSentInt { get; private set; }
        public Action<string> OnCommandSentString { get; private set; }

        public ConsoleCommand(string commandName, Action<int> onCommandSent)
        {
            CommandName = commandName;
            OnCommandSentInt = onCommandSent;
            Patch.AddCommandInt(this);
        }

        public ConsoleCommand(string commandName, Action<string> onCommandSent)
        {
            CommandName = commandName;
            OnCommandSentString = onCommandSent;
            Patch.AddCommandString(this);
        }

        [HarmonyPatch(typeof(ac_DevConsole), "Submit")]
        public class Patch
        {
            private static List<ConsoleCommand> commandsInt = new List<ConsoleCommand>();
            private static List<ConsoleCommand> commandsString = new List<ConsoleCommand>();

            private static List<ConsoleCommand> postfixCommandsInt = new List<ConsoleCommand>();
            private static List<ConsoleCommand> postfixCommandsString = new List<ConsoleCommand>();

            public static void AddCommandInt(ConsoleCommand command)
            {
                if (commandsInt.Count == 0 && commandsString.Count == 0)
                {
                    Harmony harmony = new Harmony(PluginInfo.PLUGIN_GUID);
                    harmony.PatchAll();
                }
                commandsInt.Add(command);
            }

            public static void AddCommandString(ConsoleCommand command)
            {
                if (commandsInt.Count == 0 && commandsString.Count == 0)
                {
                    Harmony harmony = new Harmony(PluginInfo.PLUGIN_GUID);
                    harmony.PatchAll();
                }
                commandsString.Add(command);
            }

            public static bool Prefix(ac_DevConsole __instance, string ConsoleSubmission)
            {
                foreach (var command in commandsInt)
                {
                    if (ConsoleSubmission.Contains(command.CommandName))
                    {
                        string[] digits = Regex.Split(ConsoleSubmission, "\\D+");
                        for (int i = 0; i < digits.Length; i++)
                        {
                            int value;
                            if (int.TryParse(digits[i], out value))
                            {
                                command.OnCommandSentInt?.Invoke(value);
                            }
                        }
                        return true;
                    }
                }

                foreach (var command in commandsString)
                {
                    if (ConsoleSubmission.Contains(command.CommandName))
                    {
                        command.OnCommandSentString?.Invoke(ConsoleSubmission);
                        return true;
                    }
                }

                return true;
            }
        }
    }

    public class PostfixConsoleCommand
    {
        public string CommandName { get; private set; }
        public Action<int> OnCommandSentInt { get; private set; }
        public Action<string> OnCommandSentString { get; private set; }
        public PostfixConsoleCommand(string commandName, Action<int> onCommandSent)
        {
            CommandName = commandName;
            OnCommandSentInt = onCommandSent;
            Patch.AddPostfixCommandInt(this);
        }

        public PostfixConsoleCommand(string commandName, Action<string> onCommandSent)
        {
            CommandName = commandName;
            OnCommandSentString = onCommandSent;
            Patch.AddPostfixCommandString(this);
        }

        [HarmonyPatch(typeof(ac_DevConsole), "Submit")]
        public class Patch
        {
            private static List<PostfixConsoleCommand> postfixCommandsInt = new List<PostfixConsoleCommand>();
            private static List<PostfixConsoleCommand> postfixCommandsString = new List<PostfixConsoleCommand>();

            public static void AddPostfixCommandInt(PostfixConsoleCommand command)
            {
                postfixCommandsInt.Add(command);
            }

            public static void AddPostfixCommandString(PostfixConsoleCommand command)
            {
                postfixCommandsString.Add(command);
            }

            public static void Postfix(ac_DevConsole __instance, string ConsoleSubmission)
            {
                foreach (var command in postfixCommandsInt)
                {
                    if (ConsoleSubmission.Contains(command.CommandName))
                    {
                        string[] digits = Regex.Split(ConsoleSubmission, "\\D+");
                        for (int i = 0; i < digits.Length; i++)
                        {
                            int value;
                            if (int.TryParse(digits[i], out value))
                            {
                                command.OnCommandSentInt?.Invoke(value);
                            }
                        }
                        return;
                    }
                }

                foreach (var command in postfixCommandsString)
                {
                    if (ConsoleSubmission.Contains(command.CommandName))
                    {
                        command.OnCommandSentString?.Invoke(ConsoleSubmission);
                        return;
                    }
                }

                return;
            }
        }
    }
}
