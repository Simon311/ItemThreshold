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
		const int Threshold = 5;

		public override void Initialize()
		{
			ServerApi.Hooks.NetGetData.Register(this, GetData, 10);
			Update.Elapsed += OnUpdate;
			Update.Start();
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

		private void GetData(GetDataEventArgs args)
		{
			if (args.MsgID == PacketTypes.ItemDrop)
			{
				var num = args.Index;
				var TPlayer = Main.player[args.Msg.whoAmI];
				int ItemID = BitConverter.ToInt16(args.Msg.readBuffer, num);
				if (ItemID == 400)
				{
					if (Thresholds[args.Msg.whoAmI] > Threshold && !(TPlayer.difficulty > 0 && (TPlayer.dead || TPlayer.statLife < 1)))
					{
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
			Thresholds = new int[256];
		}

	}
}
