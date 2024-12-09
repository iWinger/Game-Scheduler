using Discord_Bot.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Discord_Bot.Games
{
    internal class Repository
    {
        /* Information on the user*/
        private List<User> Users;

        private List<Post> Posts;

        private Dictionary<ulong, Post> Dict;
        public Dictionary<ulong, User> Map;
        
       
        public Repository()
        {
            this.Users = new List<User>();
            this.Posts = new List<Post>();
            this.Dict = new Dictionary<ulong, Post>();
            this.Map = new Dictionary<ulong, User>();
            
        }

        public List<User> GetUsers() { return this.Users; }

        public List<Post> GetPosts() { return this.Posts; }

        public Dictionary<ulong, Post> GetDict() { return this.Dict; }
    }
}
