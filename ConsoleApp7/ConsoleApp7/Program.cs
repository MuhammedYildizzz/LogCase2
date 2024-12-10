using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;


public class Program
{
    public static void Main(string[] args)
    {
        string link = "nginx.access.log";
        LogAnalizci.AnalyzeMethod(link);
    }
}
public class LogGirdisi
{
    public string Ip { get; set; }
    public string IcerikId { get; set; }
    public int VeriBoyutu { get; set; }

    public LogGirdisi(string ip, string icerikId, int veriBoyutu)
    {
        Ip = ip;
        IcerikId = icerikId;
        VeriBoyutu = veriBoyutu;
    }
}

public class LogParser
{
    private static readonly Regex LogRegex = new Regex(
        @"^(?<ip>\S+) - - \[(?<timestamp>[^\]]+)\] ""(?<method>\S+) (?<url>\S+) HTTP/\d+\.\d+"" (?<status>\d{3}) (?<size>\d+) ""[^""]*"" ""(?<useragent>[^""]*)"" ""(?<cachestatus>[^""]*)""",
        RegexOptions.Compiled);

    public static LogGirdisi Parsing(string logSatiri)
    {
        var match = LogRegex.Match(logSatiri);
        if (match.Success)
        {
            string ip = match.Groups["ip"].Value;
            string url = match.Groups["url"].Value;


            var icerikIdRegex = new Regex(@"ID_\d+");
            var matchV2 = icerikIdRegex.Match(url);
            var icerikId = match.Success ? matchV2.Value : string.Empty;

            int veriBoyutu = int.Parse(match.Groups["size"].Value);

            return new LogGirdisi(ip, icerikId, veriBoyutu);
        }
        else
        {
            return null;
        }
    }

    
}

public class LogAnalizci
{
    public static void AnalyzeMethod(string dosyaYolu)
    {
        var ipSeti = new HashSet<string>(); // Unique IP'ler
        var icerikSayisi = new Dictionary<string, int>(); // İçerik izlenme sayıları
        int toplamVeriBoyutu = 0;
        var ipIzlenmeSayisi = new Dictionary<string, int>(); // IP'lere göre izlenme sayıları
        string enCokIzlenenIcerik = string.Empty;
        int enCokIzlenenSayisi = 0;
        string enAktifIp = string.Empty;
        int enAktifIpSayisi = 0;
        var dosya =File.ReadAllLines(dosyaYolu);

        foreach (var satir in dosya)
        {
            var logGirdisi = LogParser.Parsing(satir);
            if (logGirdisi != null)
            {
                ipSeti.Add(logGirdisi.Ip);

                if (!icerikSayisi.ContainsKey(logGirdisi.IcerikId))
                {
                    icerikSayisi[logGirdisi.IcerikId] = 0;
                }
                icerikSayisi[logGirdisi.IcerikId]++;

                

                toplamVeriBoyutu =toplamVeriBoyutu+ logGirdisi.VeriBoyutu;

                if (!ipIzlenmeSayisi.ContainsKey(logGirdisi.Ip))
                {
                    ipIzlenmeSayisi[logGirdisi.Ip] = 0;
                }
                var counter =ipIzlenmeSayisi[logGirdisi.Ip];
                counter++;

                if (icerikSayisi[logGirdisi.IcerikId] > enCokIzlenenSayisi)
                {
                    enCokIzlenenSayisi = icerikSayisi[logGirdisi.IcerikId];
                    enCokIzlenenIcerik = logGirdisi.IcerikId;
                }

                
                if (ipIzlenmeSayisi[logGirdisi.Ip] > enAktifIpSayisi)
                {
                    enAktifIpSayisi = ipIzlenmeSayisi[logGirdisi.Ip];
                    enAktifIp = logGirdisi.Ip;
                }
            }
        }
        double gb = 1073741824;
        

        Console.WriteLine("Unique IP sayısı:" +ipSeti.Count);
        Console.WriteLine("Unique içerik sayısı:"+ icerikSayisi.Count);
        Console.WriteLine("Toplam veri:" +(toplamVeriBoyutu /gb)+ " GB");
        Console.WriteLine("En çok izlenen içerik: "+enCokIzlenenIcerik);
    }
}


