using System.IO;
using TShockAPI;
using Newtonsoft.Json;

namespace ItemThreshold
{
	public class IConfig
	{
		public int Threshold = 6;
		public static string savepath { get { return Path.Combine(TShock.SavePath, "IThreshold.json"); } }
		public static IConfig Config { get; internal set; }
		
		public static void Load()
		{
			if (!File.Exists(savepath)) File.WriteAllText(savepath, JsonConvert.SerializeObject(Config = new IConfig(), Formatting.Indented));
			else
			{
				Config = JsonConvert.DeserializeObject<IConfig>(File.ReadAllText(savepath));
				File.WriteAllText(savepath, JsonConvert.SerializeObject(Config, Formatting.Indented));
			}
			ItemThreshold.Threshold = Config.Threshold;
		}
	}
}
