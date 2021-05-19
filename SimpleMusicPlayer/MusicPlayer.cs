using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Reactive.Linq;
using System.Reactive.Concurrency;
using System.Windows.Media;
using System.IO;
using System.Globalization;

namespace SimpleMusicPlayer
{
    /// <summary>
    /// Credit: Absoluterealdipper.
    /// This Music player is in no way meant to take seriously.
    /// It's just a little thing I made when I was bored and thought how funny
    /// it would be if you need to use console commands to play music
    /// </summary>
    public class MusicPlayer
    {
        static MediaPlayer player;
        static PlayerStatus status;
        static string media;

        static void Main(string[] args)
        {
            WriteLine("Welcome to SMP:\n" + GetCommands());
            var subscription = ConsoleInput().Subscribe(s => MyMethod(s));

            Thread.Sleep(Timeout.Infinite);
        }

        public static string GetCommands()
        {
            return "Commands:\n" +
                 "Play [opt: pathToFile]\n" +
                 "Pause\n" +
                 "Stop\n" +
                 "Where\n" +
                 "JumpTo [totalSeconds|timeStamp (mm:ss)]\n" +
                 "skiptime [totalSeconds]\n" +
                 "Help";
        }

        private static IObservable<string> ConsoleInput()
        {
            return
                Observable
                    .FromAsync(() => Console.In.ReadLineAsync())
                    .Repeat()
                    .Publish()
                    .RefCount()
                    .SubscribeOn(Scheduler.Default);
        }

        /// <summary>
        /// This Method reads the input and filters out what to do
        /// </summary>
        /// <param name="input"></param>
        public static void MyMethod(string input)
        {
            if (status == 0)
            {
                status = PlayerStatus.Empty | PlayerStatus.Stopped;
            }

            if (player == null)
            {
                player = new MediaPlayer();
            }

            /**
            * If command is play (or contains play, like "player", "playbutton", etc)
            */
            if (input.ToLower().Contains("play"))
            {
                bool hasPath = true;
                string path = "";
                try
                {
                    path = input.Split(new[] { ' ' }, 2)[1].Replace("\"", "");
                    if (String.IsNullOrEmpty(path))
                    {
                        hasPath = false;
                    }
                }
                catch (IndexOutOfRangeException)
                {
                    hasPath = false;
                }

                /**
                 * If player has something to play
                 */
                if (!hasPath)
                {
                    if (!status.HasFlag(PlayerStatus.Empty))
                    {
                        /**
                        * If player is already playing
                        */
                        if (status.HasFlag(PlayerStatus.Playing))
                        {
                            WriteLine($"Player is already playing.");
                        }
                        /**
                        * If player is paused
                        */
                        else if (status.HasFlag(PlayerStatus.Paused))
                        {
                            //Remove the flag "Paused" and add the flag "Playing"
                            status = status & ~PlayerStatus.Paused;
                            status = status | PlayerStatus.Playing;
                            player.Play();
                            WriteLine($"Player is now continuing {Path.GetFileNameWithoutExtension(media)}.");
                        }
                        /**
                        * If player is stopped
                        */
                        else
                        {
                            status = status & ~PlayerStatus.Stopped;
                            status = status | PlayerStatus.Playing;
                            player.Play();
                            WriteLine($"Player is now playing {Path.GetFileNameWithoutExtension(media)}.");
                        }
                    }
                    else
                    {
                        WriteLine("Player has nothing to play.");
                    }
                }
                else
                {
                    try
                    {

                        if (File.Exists(path))
                        {
                            player.Stop();
                            status = status & ~PlayerStatus.Playing;
                            status = status & ~PlayerStatus.Paused;
                            status = status | PlayerStatus.Stopped;
                            player.Open(new Uri(path));
                            media = path;
                            WriteLine($"Player has loaded {Path.GetFileName(path)}");
                            status = status & ~PlayerStatus.Empty;
                            status = status | PlayerStatus.Loaded;
                            MyMethod("play");
                        }
                        else
                        {
                            WriteLine("The specified File does not exist.");
                        }

                    }
                    catch (UriFormatException)
                    {
                        WriteLine("The given path is not valid.");
                    }
                }
            }
            /**
            * If command contains stop
            */
            else if (input.ToLower().Contains("stop"))
            {
                /**
                * If player is already stopped
                */
                if (status.HasFlag(PlayerStatus.Stopped))
                {
                    WriteLine($"Player is already stopped.");
                }
                /**
                * If player is paused
                */
                else if (status.HasFlag(PlayerStatus.Paused))
                {
                    status = status & ~PlayerStatus.Paused;
                    status = status | PlayerStatus.Stopped;
                    player.Stop();
                    WriteLine($"Player stopped.");
                }
                /**
                * If player is playing
                */
                else
                {
                    status = status & ~PlayerStatus.Playing;
                    status = status | PlayerStatus.Stopped;
                    player.Stop();
                    WriteLine($"Player stopped.");
                }
            }
            /**
            * If command contains pause
            */
            else if (input.ToLower().Contains("pause"))
            {
                /**
                * If player is not playing
                */
                if (!status.HasFlag(PlayerStatus.Playing))
                {
                    WriteLine("Player is not playing anything");
                }
                /**
                * If player is playing
                */
                else
                {
                    status = status & ~PlayerStatus.Playing;
                    status = status | PlayerStatus.Paused;
                    player.Pause();
                    WriteLine("Player is now paused.");
                }
            }
            /**
            * If command contains help
            */
            else if (input.ToLower().Contains("help"))
            {
                WriteLine(GetCommands());
            }
            else if (input.ToLower().Contains("where"))
            {
                if (!status.HasFlag(PlayerStatus.Loaded))
                {
                    WriteLine("There is no song loaded.");
                    return;
                }

                var duration = player.NaturalDuration;

                double totalSeconds = duration.TimeSpan.TotalSeconds;
                double currentSecond = player.Position.TotalSeconds;

                double positionMark = totalSeconds / 25;

                int position = (int)(currentSecond / positionMark);

                string bar = "|-------------------------|";

                string s = bar.Substring(position + 2);

                s = "[]" + s;

                bar = bar.Substring(0, position);
                bar = bar + s;

                var currentTime = TimeSpan.FromSeconds(currentSecond);
                var totalTime = TimeSpan.FromSeconds(totalSeconds);

                WriteLine($"{bar}  {currentTime.Minutes.ToString("D2")}:{currentTime.Seconds.ToString("D2")}|{totalTime.Minutes.ToString("D2")}:{totalTime.Seconds.ToString("D2")}");

            }
            else if (input.ToLower().Contains("jumpto"))
            {
                if (!status.HasFlag(PlayerStatus.Loaded))
                {
                    WriteLine("No song is loaded yet.");
                    return;
                }

                string arg = "";

                arg = input.Split(new[] { ' ' }, 2)[1].Replace("\"", "");
                if (String.IsNullOrEmpty(arg))
                {
                    WriteLine("No time was given to jump to.");
                    return;
                }

                int totalSeconds;
                try
                {
                    totalSeconds = Int32.Parse(arg);
                }
                catch (FormatException fe)
                {
                    try
                    {
                        TimeSpan.Parse(arg);

                        arg = "00:" + arg;

                        var myTime = TimeSpan.Parse(arg);

                        totalSeconds = (int)myTime.TotalSeconds;
                    }
                    catch (Exception e)
                    {
                        WriteLine("The given argument is neither a number nor a timestamp");
                        return;
                    }
                }

                player.Position = TimeSpan.FromSeconds(totalSeconds);

                WriteLine($"Jumped to {player.Position.Minutes}:{player.Position.Seconds}");
            }
            else if (input.ToLower().Contains("skiptime"))
            {
                if (!status.HasFlag(PlayerStatus.Loaded))
                {
                    WriteLine("No song is loaded yet.");
                    return;
                }

                string arg = "";
                try
                {
                    arg = input.Split(new[] { ' ' }, 2)[1].Replace("\"", "");
                    if (String.IsNullOrEmpty(arg))
                    {
                        throw new IndexOutOfRangeException();
                    }
                }
                catch (IndexOutOfRangeException)
                {
                    WriteLine("No time was given to skip.");
                    return;
                }

                double time = 0;
                try
                {
                    time = Double.Parse(arg);
                }
                catch (Exception)
                {
                    WriteLine("The given argument was not a number");
                }

                player.Position = player.Position.Add(TimeSpan.FromSeconds(time));

                WriteLine($"Skipped to {player.Position.Minutes}: {player.Position.Seconds}");
            }
            else if (input.ToLower().Contains("exit"))
            {
                player.Stop();
                Environment.Exit(0);
            }
            else
            {
                WriteLine(GetCommands());
            }
        }
        public static void WriteLine(string s)
        {
            Console.Clear();
            Console.WriteLine(s);
        }
    }

    [Flags]
    public enum PlayerStatus
    {
        Playing = 1 << 0,
        Paused = 1 << 1,
        Stopped = 1 << 2,
        Loaded = 1 << 3,
        Empty = 1 << 4
    }
}
