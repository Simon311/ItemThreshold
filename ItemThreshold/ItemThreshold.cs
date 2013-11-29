using System;
using System.Collections.Generic;
using System.Timers;
using System.Reflection;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;

namespace ItemThreshold
{
	[ApiVersion(1, 14)]
	public class ItemThreshold : TerrariaPlugin
	{
		public override Version Version
		{
			get { return Assembly.GetExecutingAssembly().GetName().Version; }
		}

		public override string Name
		{
			get { return "ItemThreshold"; }
		}

		public override string Author
		{
			get { return "Simon311"; }
		}

		public override string Description
		{
			get { return "Adds threshold for dropping items."; }
		}

		public ItemThreshold(Main game)
			: base(game)
		{
			Order = 10;
		}

		static readonly Timer Update = new Timer(1000);
		static int[] Thresholds = new int[256];
		static List<int> CorePlayers = new List<int>();
		internal static int Threshold = 6;

		public override void Initialize()
		{
			ServerApi.Hooks.NetGetData.Register(this, GetData, 10);
			ServerApi.Hooks.GameInitialize.Register(this, Initialize, -10);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				ServerApi.Hooks.NetGetData.Deregister(this, GetData);
				Update.Elapsed -= OnUpdate;
				Update.Stop();
			}
			base.Dispose(disposing);
		}

		private void Initialize(EventArgs e)
		{
			IConfig.Load();
			Update.Elapsed += OnUpdate;
			Update.Start();
		}

		private void GetData(GetDataEventArgs args)
		{
			if (args.MsgID == PacketTypes.ItemDrop)
			{
				var num = args.Index;
				var TPlayer = Main.player[args.Msg.whoAmI];
				int ItemID = BitConverter.ToInt16(args.Msg.readBuffer, num);
				if (ItemID == 400)
				{
					if (Thresholds[args.Msg.whoAmI] > Threshold )
					{
						if (TPlayer.difficulty > 0)
						{
							if (!CorePlayers.Contains(args.Msg.whoAmI)) CorePlayers.Add(args.Msg.whoAmI);
							return;
						}
						TShock.Utils.Kick(TShock.Players[args.Msg.whoAmI], "Item Spam", true);
						Thresholds[args.Msg.whoAmI] = 0;
						args.Handled = true;
					}
					else Thresholds[args.Msg.whoAmI]++;
				}
			}
		}

		private void OnUpdate(object sender, ElapsedEventArgs e)
		{
			if (CorePlayers.Count > 0)
			{
				var C = CorePlayers.Count;
				for (int i = 0; i < C; i++)
				{
					var Player = Main.player[CorePlayers[i]];
					if (Player == null || !Player.active) return;
					if (!Player.dead || Player.statLife > 0) TShock.Utils.Kick(TShock.Players[Player.whoAmi], "Item Spam", true);
				}
			}
			Thresholds = new int[256];
			CorePlayers.Clear();
		}
	}
}
