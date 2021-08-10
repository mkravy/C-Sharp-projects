using System;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.IO;
using CsvHelper;
using System.Collections;
using System.Globalization;
using System.Linq;
using System.Collections.Generic;

namespace Elo_Corners
{
    class Program
    {
        static void list(string line, string[] teams, string del, int i, double[] attack, double[] defence)
        {
            var str = line.Split(del);

            //str[0] - tag
            //str[1] - date
            //str[2] - home team
            //str[3] - guest team
            //str[4] - home corner
            //str[5] - guest corner

            if (teams.Contains(str[2])) { }
            else
            {
                for (int l = 0; l < 100; ++l)
                {
                    if (teams[l] == null)
                    {
                        teams[l] = str[2];
                        attack[l] = 1;
                        defence[l] = 1;
                        break;
                    }
                }
            }

            if (teams.Contains(str[3])) { }
            else
            {
                for (int l = 0; l < 100; ++l)
                {
                    if (teams[l] == null)
                    {
                        teams[l] = str[3];
                        attack[l] = 1;
                        defence[l] = 1;
                        break;
                    }
                }
            }
            ++i;
        }

        static void writelist(string[] teams, string del, double[] attack, double[] defence, int[] cf, int[] ca, string pathratelist)
        {
            //str[0] - tag
            //str[1] - date
            //str[2] - home team
            //str[3] - guest team
            //str[4] - home score
            //str[5] - guest score
            //str[6] - home corner
            //str[7] - guest corner

            string upstring = "[0]" + del + "[1]" + del + "[2]" + del + "[3]" + del + "[4]" + del + "[5]" + Environment.NewLine;
            File.WriteAllText(pathratelist, upstring);

            for (int k = 0; teams[k] != null; ++k)
            {
                string team = k + 1 + del + teams[k] + del + attack[k] + del + defence[k] + del + cf[k] + del + ca[k] + Environment.NewLine;
                File.AppendAllText(pathratelist, team);
            }
        }

        static void readlist(string line, string[] teams, string del, double[] attack, double[] defence, int a, int[] cf, int[] ca)
        {
            //str[0] - number
            //str[1] - team
            //str[2] - attack
            //str[3] - defence

            var str = line.Split(del);
            teams[a] = str[1];
            attack[a] = double.Parse(str[2]);
            defence[a] = double.Parse(str[3]);
        }

        static void ratecalc(string line, string[] teams, string del, double[] attack, double[] defence, int[] cf, int[] ca)
        {
            //str[0] - tag
            //str[1] - date
            //str[2] - home team
            //str[3] - guest team
            //str[4] - home corner
            //str[5] - guest corner

            var str = line.Split(del);
            var i1 = Array.IndexOf(teams, str[2]);
            var i2 = Array.IndexOf(teams, str[3]);

            var k1 = Math.Round((double.Parse(str[4]) - (attack[i1] + 0.5 + defence[i2]) / 2) / 10, 2);
            var k2 = Math.Round((double.Parse(str[5]) - (attack[i2] + 0.5 + defence[i1]) / 2) / 10, 2);
            double dif1 = Math.Pow(10, k1) - 1;
            double dif2 = Math.Pow(10, k2) - 1;

            Console.WriteLine(teams[i1] + "(" + attack[i1] + ", " + defence[i1] + ") " + str[4] + " - " + str[5] + " (" + attack[i2] + ", " + defence[i2] + ")" + teams[i2]);

            attack[i1] = Math.Round(attack[i1] + dif1, 2);
            attack[i2] = Math.Round(attack[i2] + dif2, 2);
            defence[i1] = Math.Round(defence[i1] + (Math.Pow(10, k2) - 1), 2);
            defence[i2] = Math.Round(defence[i2] + (Math.Pow(10, k1) - 1), 2);
            cf[i1] = cf[i1] + int.Parse(str[4]);
            ca[i1] = ca[i1] + int.Parse(str[5]);
            cf[i2] = cf[i2] + int.Parse(str[5]);
            ca[i2] = ca[i2] + int.Parse(str[4]);
            Console.WriteLine(teams[i1] + " - " + attack[i1] + " - " + defence[i1] + " - " + cf[i1] + " - " + ca[i1]);
            Console.WriteLine(teams[i2] + " - " + attack[i2] + " - " + defence[i2] + " - " + cf[i2] + " - " + ca[i2]);
            Console.WriteLine("============");
        }

        static void predictcalc(string line, string[] teams, string del, double[] attack, double[] defence, string pathbets)
        {
            //str[0] - date
            //str[1] - home team
            //str[2] - guest team
            //str[3] - total
            
            Random rand = new Random();
            List<int> predict = new List<int>();
            var str = line.Split(del);
            var i1 = Array.IndexOf(teams, str[1]);
            var i2 = Array.IndexOf(teams, str[2]);

            var total = str[3];
            int sum = 0;
            int sumtotal = 0;
            int totalunder = 0;
            //int time = 1000;
            int time = 5000; //Totally 10k
            int randlimit = 2000; //*1000

            Console.WriteLine(teams[i1] + " - " + attack[i1] + " - " + defence[i1]);
            Console.WriteLine(teams[i2] + " - " + attack[i2] + " - " + defence[i2]);

            for (int a = 0; a < time; ++a)
            {
                var a1 = Math.Round(rand.Next(0, randlimit) * attack[i1] / 1000 + rand.Next(0, randlimit) * defence[i2] / 1000, 0);
                predict.Add(Convert.ToInt32(a1));

                var a2 = Math.Round(rand.Next(0, randlimit) * attack[i2] / 1000 + rand.Next(0, randlimit) * defence[i1] / 1000, 0);
                predict.Add(Convert.ToInt32(a2));
            }

            foreach(int preds in predict)
            {
                if (preds<Convert.ToDouble(total))
                {
                    sumtotal++;
                }
            }

            sum = predict.Count;
            
            var percent = (float) 100*sumtotal / sum;
            var percentunder = (float)100 * (sum - sumtotal) / sum;
            var bet = (float) sum / sumtotal;
            var betunder = (float) sum / (sum - sumtotal);
            totalunder = sum - sumtotal;

            Console.WriteLine("\nTotal Over " + total + ":");
            Console.WriteLine("chance = " +Math.Round(percent,2) + "%");
            Console.WriteLine("rate = " + Math.Round(bet, 4));
            
            Console.WriteLine("\nTotal Under " + total + ":");
            Console.WriteLine("chance = " + Math.Round(percentunder, 2) + "%");
            Console.WriteLine("rate = " + Math.Round(betunder, 4) + "\n\nXXX XXX\n");

                if (!File.Exists(pathbets))
                {
                    string upstring = "[0]" + del + "[1]" + del + "[2]" + del + "[3]" + del + "[4]" + Environment.NewLine;
                    File.WriteAllText(pathbets, upstring);
                }
                    
                string team = teams[i1] + del + teams[i2] + del + total + del + Math.Round(bet, 4) + del + Math.Round(betunder, 4) + Environment.NewLine;
                File.AppendAllText(pathbets, team);

            sum = 0;
        }

        static void Main(string[] args)
        {
            string temp = Environment.CurrentDirectory;
            // Set the path and filename variable "path", filename being MyTest.csv in this example.
            string pathDB = "C:\\Users\\micha\\Desktop\\Elo corners\\DB\\GERMANY Bundesliga.csv";
            string pathratelist = "C:\\Users\\micha\\Desktop\\Elo corners\\ratelist\\GERMANY Bundesliga.csv";
            string pathpredict = "C:\\Users\\micha\\Desktop\\Elo corners\\predict\\GERMANY Bundesliga.csv";
            string pathbets = "C:\\Users\\micha\\Desktop\\Elo corners\\bets\\GERMANY Bundesliga.csv";
            string del = "; ";

            string[] teams = new string[20];
            double[] rating = new double[20];
            double[] attack = new double[20];
            double[] defence = new double[20];
            int[] cf = new int[20];
            int[] ca = new int[20];

            using (StreamReader sr = new StreamReader(pathDB, System.Text.Encoding.Default))
            {
                string line;
                sr.ReadLine();
                while ((line = sr.ReadLine()) != null)
                {
                    //Составление списка
                    //list(line, teams, del, i, attack, defence);
                }
            }

            //Рассчет рейтинга через матчи
            using (StreamReader sr = new StreamReader(pathDB, System.Text.Encoding.Default))
            {
                string line;
                sr.ReadLine();
                while ((line = sr.ReadLine()) != null)
                {
                    //ratecalc(line, teams, del, attack, defence, cf, ca);
                }
            }

            //Запись рейтинга в файл
            //writelist(teams, del, attack, defence, cf, ca, pathrate);

            //Чтение рейтинга из файла
            using (StreamReader sr = new StreamReader(pathratelist, System.Text.Encoding.Default))
            {
                string line;
                sr.ReadLine();
                int a = 0;
                while ((line = sr.ReadLine()) != null)
                {
                    readlist(line, teams, del, attack, defence, a, cf, ca);
                    ++a;
                }
            }

            //Расчет вероятностей
            using (StreamReader sr = new StreamReader(pathpredict, System.Text.Encoding.Default))
            {
                string line;
                sr.ReadLine();
                while ((line = sr.ReadLine()) != null)
                {
                    predictcalc(line, teams, del, attack, defence, pathbets);
                }
            }
        }
    }
}
