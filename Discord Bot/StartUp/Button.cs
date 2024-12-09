using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discord_Bot.StartUp
{
    internal class Button
    {
        private string name { get; set; }


        public Button(string name)
        {
            this.name = name;

        }

        public ComponentBuilder Spawn(string type)
        {
            var builder = new ComponentBuilder().WithButton(this.name, type);
            return builder;

        }
    }
}
