﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loqui.Generation
{
    public class ArgsWrapper : IDisposable
    {
        FileGeneration fg;
        List<string[]> args = new List<string[]>(); 
        public bool SemiColon = true;
        string initialLine;
        string suffixLine;

        public ArgsWrapper(
            FileGeneration fg,
            string initialLine = null,
            string suffixLine = null,
            bool semiColon = true)
        {
            this.fg = fg;
            this.SemiColon = semiColon;
            this.initialLine = initialLine;
            this.suffixLine = suffixLine;
        }

        public void Add(params string[] lines)
        {
            foreach (var line in lines)
            {
                args.Add(new string[] { line });
            }
        }

        public void Add(Action<FileGeneration> generator)
        {
            var gen = new FileGeneration();
            generator(gen);
            args.Add(gen.Strings.ToArray());
        }

        public void Dispose()
        {
            if (initialLine != null)
            {
                if (args.Count == 0)
                {
                    fg.AppendLine($"{initialLine}(){suffixLine};");
                    return;
                }
                else if (args.Count == 1
                    && args[0].Length == 1)
                {
                    fg.AppendLine($"{initialLine}({args[0][0]}){suffixLine};");
                    return;
                }
                else
                {
                    fg.AppendLine($"{initialLine}(");
                }
            }
            this.fg.Depth++;
            args.Last(
                each: (arg) =>
                {
                    arg.Last(
                        each: (item, last) =>
                        {
                            fg.AppendLine($"{item}{(last ? "," : string.Empty)}");
                        });
                },
                last: (arg) =>
                {
                    arg.Last(
                        each: (item, last) =>
                        {
                            fg.AppendLine($"{item}{(last ? $"){suffixLine}{(SemiColon ? ";" : string.Empty)}" : string.Empty)}");
                        });
                });
            this.fg.Depth--;
        }
    }
}
