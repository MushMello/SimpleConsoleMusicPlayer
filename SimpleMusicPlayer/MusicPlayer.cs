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
            WriteLine(
                "Welcome to SMP:\n" +
                "Commands:\n" +
                "Play\n" +
                "Pause\n" +
                "Stop\n" +
                "Help");
            var subscription = ConsoleInput().Subscribe(s => MyMethod(s));

            Thread.Sleep(Timeout.Infinite);
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
        /// Innovative name for a stolen piece of code executing it
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
                bool hasPath = false;
                string path = "";
                try
                {
                    path = input.Split(new[] { ' ' }, 2)[1].Replace("\"", "");
                    hasPath = true;
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
                WriteLine("Commands:\n" +
                "Play\n" +
                "Pause\n" +
                "Stop\n" +
                "Help");
            }
            else
            {
                WriteLine("Unknown command.\n" +
                "Available commands:\n" +
                "Play\n" +
                "Pause\n" +
                "Stop\n" +
                "Help");
            }
        }
        public static void WriteLine(string s)
        {
            Console.Clear();
            Console.WriteLine(s);
            Console.Write("Command: ");
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
