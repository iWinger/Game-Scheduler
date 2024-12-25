using Discord_Bot.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discord_Bot.Utility
{
    internal class RandomHelper
    {
        private User[] Players;

        private Random Random;

        public RandomHelper(User[] players)
        {
            this.Players = players;
            Random = new Random();
        }

        private void combinations(int[] arr, int idx, int start, int[] result, List<List<int>> list)
        {
            if (idx == 0)
            {
                result.ToList().ForEach(i => Console.WriteLine(i.ToString()));
                list.Add(new List<int>(result.ToList()));
                return;
            }
            for(int i = start; i <= arr.Length-idx; i++)
            {
                result[result.Length - idx] = arr[i];
                combinations(arr, idx - 1, i + 1, result,list);

            }
        }
        

        public List<(List<User>, List<User>)> calculateTeams()
        {
            // Generate combinations of n/2 sets of people
            List<(List<User>, List<User>)> teams = new List<(List<User>, List<User>)>();
            List<List<int>> list = new List<List<int>>();
            int[] arr = new int[Players.Length];
            for(int i = 0; i < arr.Length; i++)
            {
                arr[i] = i; // Represents the index of the player
            }
            int k = this.Players.Length / 2;
            combinations(arr, k, 0, new int[k], list);
            
            for(int i = 0; i < list.Count; i++)
            {
                List<int> teamA = list[i].ToList();
                List<int> teamB = AddOtherTeam(teamA,this.Players.Length);
                AddBothTeams(teams,teamA, teamB);
                
            }

            return teams;
        }

        private void AddBothTeams(List<(List<User>,List<User>)> teams, List<int> teamA, List<int> teamB)
        {
            
                List<User> A_Team = new List<User>();
                foreach(int m in teamA)
                {
                    A_Team.Add(this.Players[m]);
                }
                List<User> B_Team = new List<User>();
                foreach (int n in teamB)
                {
                    B_Team.Add(this.Players[n]);
                }

                (List<User>, List<User>) pair = (A_Team, B_Team);
                teams.Add(pair);

            
        }

        public string PrintTeam(List<User> team)
        {
            string players = "";
            for(int i = 0; i < team.Count; i++)
            {
                if (team[i] == null) continue;
                if (i != team.Count - 1)
                    players += $"{team[i].getName()}, ";
                else
                {
                    players += $"{team[i].getName()}";
                }
            }

            return players;
        }

        private List<int> AddOtherTeam(List<int> list,int max)
        {
            var set = new HashSet<int>(list);
            // Calculate the other team
            List<int> teamB = new List<int>();
            for(int i = 0; i < max; i++)
            {
                if (!set.Contains(i))
                {
                    teamB.Add(i);
                }
            }
            return teamB;

        }

        public (List<User>, List<User>) GetRandomTeam(List<(List<User>, List<User>)> teams)
        {
            //Console.WriteLine(teams.Count);
            int idx = Random.Next(teams.Count); // Generates a random index from 0 to players.Length-1

            return teams[idx];

        }


    }
}
