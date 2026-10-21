
using System;
using System.Collections.Generic;
using System.Linq;


 


namespace Seedunk.Net.Common
{
    // ---------- 选项 ----------
    public class CommandOption<T>
    {
        public string[] Aliases { get; }
        public string Description { get; }
        public bool Required { get; }
        public T DefaultValue { get; }
        public bool HasDefault { get; }

        public CommandOption(string[] aliases, string description = "",
                      bool required = false, T defaultValue = default)
        {
            Aliases = aliases;
            Description = description;
            Required = required;
            DefaultValue = defaultValue;
            HasDefault = !EqualityComparer<T>.Default.Equals(defaultValue, default);
        }

        public bool Matches(string arg) =>
            Aliases.Contains(arg, StringComparer.OrdinalIgnoreCase);
    }

    // ---------- 位置参数 ----------
    public class CommandArgument<T>
    {
        public string Name { get; }
        public string Description { get; }
        public bool Required { get; }

        public CommandArgument(string name, string description = "", bool required = true)
        {
            Name = name;
            Description = description;
            Required = required;
        }
    }

    // ---------- 解析结果 ----------
    public class CommandParseResult
    {
        private readonly Dictionary<string, object> _options = new();
        private readonly Dictionary<string, object> _arguments = new();

        public void SetOption(string key, object value) => _options[key] = value;
        public void SetArgument(string key, object value) => _arguments[key] = value;

        public T GetOption<T>(CommandOption<T> option)
        {
            var key = option.Aliases[0];
            if (_options.TryGetValue(key, out var v)) return (T)v;
            return option.HasDefault ? option.DefaultValue : default;
        }

        public T GetArgument<T>(CommandArgument<T> argument)
        {
            if (_arguments.TryGetValue(argument.Name, out var v)) return (T)v;
            return default;
        }
    }

    // ---------- 命令 ----------
    public class Command
    {
        public string Name { get; }
        public string Description { get; }
        private readonly List<Command> _subCommands = new();
        private readonly List<object> _options = new();      // Option<T>
        private readonly List<object> _arguments = new();    // Argument<T>
        private Action<CommandParseResult> _handler;

        public Command(string name, string description = "")
        {
            Name = name;
            Description = description;
        }

        public Command AddCommand(Command cmd)
        {
            _subCommands.Add(cmd);
            return this;
        }

        public Command AddOption<T>(CommandOption<T> option)
        {
            _options.Add(option);
            return this;
        }

        public Command AddArgument<T>(CommandArgument<T> argument)
        {
            _arguments.Add(argument);
            return this;
        }

        public Command SetHandler(Action<CommandParseResult> handler)
        {
            _handler = handler;
            return this;
        }

        internal IReadOnlyList<Command> SubCommands => _subCommands;
        internal IReadOnlyList<object> Options => _options;
        internal IReadOnlyList<object> Arguments => _arguments;
        internal Action<CommandParseResult> Handler => _handler;
    }

    // ---------- 解析器 ----------
    public static class CommandParser
    {
        public static int Invoke(Command root, string[] args)
        {
            try
            {
                return Run(root, args);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"错误: {ex.Message}");
                return 1;
            }
        }

        private static int Run(Command root, string[] args)
        {
            // 处理 --help / -h
            if (args.Length == 0 || args.Contains("--help") || args.Contains("-h"))
            {
                if (args.Length > 0 && (args.Contains("--help") || args.Contains("-h")))
                {
                    // 定位到具体命令再显示帮助
                    var cmd = FindCommand(root, args);
                    PrintHelp(cmd);
                    return 0;
                }
                PrintHelp(root);
                return 0;
            }

            // 查找子命令
            Command current = root;
            int index = 0;
            while (index < args.Length)
            {
                var sub = current.SubCommands
                    .FirstOrDefault(c => c.Name.Equals(args[index],
                        StringComparison.OrdinalIgnoreCase));
                if (sub == null) break;
                current = sub;
                index++;
            }

            var result = new CommandParseResult();
            var positional = new List<string>();

            // 解析选项和位置参数
            while (index < args.Length)
            {
                var arg = args[index];

                if (arg.StartsWith("-"))
                {
                    var option = current.Options
                        .FirstOrDefault(o => Matches(o, arg));
                    if (option == null)
                        throw new Exception($"未知选项: {arg}");

                    var optType = option.GetType();
                    var valueType = optType.GetGenericArguments()[0];

                    if (valueType == typeof(bool))
                    {
                        SetOption(result, option, true);
                        index++;
                    }
                    else
                    {
                        if (index + 1 >= args.Length)
                            throw new Exception($"选项 {arg} 缺少值");
                        var raw = args[index + 1];
                        var value = Convert.ChangeType(raw, valueType);
                        SetOption(result, option, value);
                        index += 2;
                    }
                }
                else
                {
                    positional.Add(arg);
                    index++;
                }
            }

            // 绑定位置参数
            for (int i = 0; i < current.Arguments.Count; i++)
            {
                var argDef = current.Arguments[i];
                var argType = argDef.GetType().GetGenericArguments()[0];
                var name = (string)argDef.GetType().GetProperty("Name").GetValue(argDef);

                if (i < positional.Count)
                {
                    var value = Convert.ChangeType(positional[i], argType);
                    result.SetArgument(name, value);
                }
                else
                {
                    var required = (bool)argDef.GetType().GetProperty("Required").GetValue(argDef);
                    if (required) throw new Exception($"缺少参数: {name}");
                }
            }

            // 校验必填选项
            foreach (var opt in current.Options)
            {
                var required = (bool)opt.GetType().GetProperty("Required").GetValue(opt);
                if (!required) continue;
                var aliases = (string[])opt.GetType().GetProperty("Aliases").GetValue(opt);
                var field = result.GetType().GetField("_options",
                     System.Reflection.BindingFlags.NonPublic |
                     System.Reflection.BindingFlags.Instance);

                var dict = field?.GetValue(result) as Dictionary<string, object>;

                if (dict == null || !dict.ContainsKey(aliases[0]))
                {
                    throw new Exception($"缺少必填选项: {aliases[0]}");
                }
            }

            if (current.Handler == null)
                throw new Exception($"命令 {current.Name} 未设置处理逻辑");

            current.Handler(result);
            return 0;
        }

        private static bool Matches(object option, string arg)
        {
            var aliases = (string[])option.GetType().GetProperty("Aliases").GetValue(option);
            return aliases.Contains(arg, StringComparer.OrdinalIgnoreCase);
        }

        private static void SetOption(CommandParseResult result, object option, object value)
        {
            var aliases = (string[])option.GetType().GetProperty("Aliases").GetValue(option);
            result.SetOption(aliases[0], value);
        }

        private static Command FindCommand(Command root, string[] args)
        {
            Command current = root;
            foreach (var a in args)
            {
                if (a.StartsWith("-")) continue;
                var sub = current.SubCommands
                    .FirstOrDefault(c => c.Name.Equals(a, StringComparison.OrdinalIgnoreCase));
                if (sub == null) break;
                current = sub;
            }
            return current;
        }

        private static void PrintHelp(Command cmd)
        {
            Console.WriteLine($"用法: {cmd.Name} [命令] [选项] [参数]");
            Console.WriteLine();
            Console.WriteLine(cmd.Description);
            Console.WriteLine();

            if (cmd.SubCommands.Count > 0)
            {
                Console.WriteLine("命令:");
                foreach (var sub in cmd.SubCommands)
                    Console.WriteLine($"  {sub.Name,-15} {sub.Description}");
                Console.WriteLine();
            }

            if (cmd.Arguments.Count > 0)
            {
                Console.WriteLine("参数:");
                foreach (var a in cmd.Arguments)
                {
                    var name = (string)a.GetType().GetProperty("Name").GetValue(a);
                    var desc = (string)a.GetType().GetProperty("Description").GetValue(a);
                    Console.WriteLine($"  {name,-15} {desc}");
                }
                Console.WriteLine();
            }

            if (cmd.Options.Count > 0)
            {
                Console.WriteLine("选项:");
                foreach (var o in cmd.Options)
                {
                    var aliases = (string[])o.GetType().GetProperty("Aliases").GetValue(o);
                    var desc = (string)o.GetType().GetProperty("Description").GetValue(o);
                    Console.WriteLine($"  {string.Join(", ", aliases),-20} {desc}");
                }
                Console.WriteLine();
            }

            Console.WriteLine("  -h, --help          显示帮助");
        }
    }
}