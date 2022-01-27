using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GANServer
{
  public class Configuration
  {
    public int MainServerPort;
    public string CartoonGANIP;
    public int CartoonGANPort;
    public string CartoonizeIP;
    public int CartoonizePort;
    public string ArtlineIP;
    public int ArtlinePort;
    public string PaintsChainerIP;
    public int PaintsChainerPort;

    public static Configuration Load()
    {
      Configuration config = new Configuration();
      string currentDirectory = Directory.GetCurrentDirectory();
      if (File.Exists($"{currentDirectory}\\config.ini") == false)
      {
        config.MainServerPort = 14536;
        config.CartoonGANIP = "127.0.0.1";
        config.CartoonGANPort = 9999;
        config.CartoonizeIP = "127.0.0.1";
        config.CartoonizePort = 8888;
        config.ArtlineIP = "127.0.0.1";
        config.ArtlinePort = 6666;
        config.PaintsChainerIP = "127.0.0.1";
        config.PaintsChainerPort = 11111;


        using (FileStream stream = File.Open($"{currentDirectory}\\config.ini", FileMode.CreateNew))
        {
          using (StreamWriter writer = new StreamWriter(stream))
          {
            string jsonText = JsonConvert.SerializeObject(config);
            writer.WriteLine(jsonText);
          }
        }
      }

      using (FileStream stream = File.Open($"{currentDirectory}\\config.ini", FileMode.Open))
      {
        using (StreamReader reader = new StreamReader(stream))
        {
          string text = reader.ReadToEnd();
          var jobj = JObject.Parse(text);
          config = JsonConvert.DeserializeObject<Configuration>(text);
        }
      }

      return config;
    }
  }


}
