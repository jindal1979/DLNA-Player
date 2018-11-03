﻿using System.Text.RegularExpressions;
using System.Net;
using System.IO;
using System.Diagnostics;

public static class Extentions
{

    public static string ChopOffBefore(this string s, string Before)
    {//Usefull function for chopping up strings
        int End = s.ToUpper().IndexOf(Before.ToUpper());
        if (End > -1)
        {
            return s.Substring(End + Before.Length);
        }
        return s;
    }

    public static string ChopOffAfter(this string s, string After)
    {//Usefull function for chopping up strings
        int End = s.ToUpper().IndexOf(After.ToUpper());
        if (End > -1)
        {
            return s.Substring(0, End);
        }
        return s;
    }

    public static string ReplaceIgnoreCase(this string Source, string Pattern, string Replacement)
    {// using \\$ in the pattern will screw this regex up
        //return Regex.Replace(Source, Pattern, Replacement, RegexOptions.IgnoreCase);

        if (Regex.IsMatch(Source, Pattern, RegexOptions.IgnoreCase))
            Source = Regex.Replace(Source, Pattern, Replacement, RegexOptions.IgnoreCase);
        return Source;
    }
    public static MemoryStream decodeAudio(string file, int format)
    {
        string dec = string.Empty;
        string args = string.Empty;
        if (format == 1)
        {
            dec = "opusdec.exe";
            args = "--rate 48000 --no-dither --float \"" + file + "\" temp.wav";
        }
        else if (format == 2)
        {
            dec = "flac.exe";
            args = "-d \"" + file + "\" -o temp.wav";
        }
        ProcessStartInfo decProcessInfo = new ProcessStartInfo()
        {
            FileName = dec,
            Arguments = args,
            CreateNoWindow = true,
            RedirectStandardOutput = false,
            UseShellExecute = false
        };
        Process.Start(decProcessInfo).WaitForExit();
        MemoryStream decodedWav = new MemoryStream();
        FileStream temp = new FileStream("temp.wav", FileMode.Open);
        temp.CopyTo(decodedWav);
        temp.Close();
        File.Delete("temp.wav");
        return decodedWav;
    }
    public static string[] getMetadata(string file)
    {
        string track = "Unknown";
        string artist = "Unknown";
        string performer = "Unknown";
        ProcessStartInfo ProcessInfo = new ProcessStartInfo()
        {
            FileName = "mediainfo.exe",
            Arguments = "\"" + file + "\"",
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            UseShellExecute = false
        };
        Process process = new Process
        {
            StartInfo = ProcessInfo
        };
        process.Start();
        string line = string.Empty;
        while (!process.HasExited)
        {
            if (!process.StandardOutput.EndOfStream)
            {
                line = process.StandardOutput.ReadLine();
                if (line.Contains("Track name") && !line.Contains("/"))
                    track = line.Split(':')[1].Trim();
                else if (line.Contains("Artist") && !line.Contains("/"))
                    artist = line.Split(':')[1].Trim();
                else if (line.Contains("Performer") && !line.Contains("/"))
                    performer = line.Split(':')[1].Trim();
            }
        }
        if (artist == "Unknown" && performer != "Unknown")
            artist = performer;
        string[] returnString = { track, artist };
        return returnString;
    }
    public static class Helper
    {
        public static string GetMyIP()
        {//Might return the wrong NC ip but you need the one connected to the router
            IPHostEntry host;
            host = Dns.GetHostEntry(Dns.GetHostName());
            string ipAddress = "";
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily.ToString() == "InterNetwork")
                {
                    ipAddress = ip.ToString();
                }
            }
            return ipAddress;

        }
    }
}


