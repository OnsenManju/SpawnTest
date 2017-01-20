using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;

namespace SpawnTest
{
	[ApiVersion(2, 0)]
	public class Test : TerrariaPlugin
    {
		public override string Name
		{
			get { return "SpawnTest"; }
		}

		public override Version Version
		{
			get { return new Version(1, 0, 0); }
		}

		public override string Author
		{
			get { return ""; }
		}

		public override string Description
		{
			get { return ""; }
		}

		public Test(Main game) : base(game)
		{

		}

		public override void Initialize()
		{
			Commands.ChatCommands.Add(new Command("", FindCommand, "findmob", "fm"));
			Commands.ChatCommands.Add(new Command("tshock.npc.spawnmob", SpawnCommand, "spawntest", "st"));
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
		}

		private void FindCommand(CommandArgs args)
		{
			//var npcs = TShock.Utils.GetNPCByIdOrName(args.Parameters[0]);
			var npcs = _FindNPCByName(args.Parameters[0]);

			if (npcs.Count == 0)
			{
				args.Player.SendErrorMessage("Invalid mob type!");
			}
			else
			{
				args.Player.SendInfoMessage("Search result:");
				foreach (NPC npc in npcs)
				{
					args.Player.SendMessage($"type={npc.type} Main.npcName=\"{Main.npcName[npc.type]}\" NPC.name=\"{npc.name}\" NPC.displayName=\"{npc.displayName}\"", 255, 255, 255);
				}

			}
		}

		private List<NPC> _FindNPCByName(string name)	// based on GetNPCByName()
		{
			var found = new List<NPC>();
			NPC npc = new NPC();
			string nameLower = name.ToLower();
			for (int i = -17; i < Main.maxNPCTypes; i++)
			{
				npc.netDefaults(i);
				if (npc.name.ToLower().Contains(nameLower)) // .StartsWith() --> .Contains()
					found.Add((NPC)npc.Clone());
			}
			return found;
		}

		private void SpawnCommand(CommandArgs args)
		{
			var npcs = _GetNPCByIdOrName(args.Parameters[0]);

			if (npcs.Count == 0)
			{
				args.Player.SendErrorMessage("Invalid mob type!");
			}
			else if (npcs.Count > 1)
			{
				TShock.Utils.SendMultipleMatchError(args.Player, npcs.Select(n => n.name));
			}
			else
			{
				NPC npc = npcs[0];
				int amount = 1;
				if (args.Parameters.Count > 1)
				{
					int.TryParse(args.Parameters[1], out amount);
				}

				//TSPlayer.Server.SpawnNPC(npc.type, npc.name, amount, args.Player.TileX, args.Player.TileY, 50, 20);
				_SpawnNPC(npc.type, npc.displayName, amount, args.Player.TileX, args.Player.TileY, 50, 20);
			}
		}

		private List<NPC> _GetNPCByIdOrName(string idOrName)
		{
			int type = -1;
			if (int.TryParse(idOrName, out type))
			{
				if (type >= Main.maxNPCTypes)
					return new List<NPC>();
				return new List<NPC> { _GetNPCById(type) };
			}
			return _GetNPCByName(idOrName);
		}

		private NPC _GetNPCById(int id)	// same as original
		{
			NPC npc = new NPC();
			npc.netDefaults(id);
			return npc;
		}

		private List<NPC> _GetNPCByName(string name)
		{
			var found = new List<NPC>();
			NPC npc = new NPC();
			string nameLower = name.ToLower();
			for (int i = -17; i < Main.maxNPCTypes; i++)
			{
				npc.netDefaults(i);
				if (npc.displayName.ToLower() == nameLower) // name --> displayName
					return new List<NPC> { npc };
				if (npc.displayName.ToLower().StartsWith(nameLower))    // name --> displayName
					found.Add((NPC)npc.Clone());
			}
			return found;
		}

		private void _SpawnNPC(int type, string displayName, int amount, int startTileX, int startTileY, int tileXRange = 100,
			int tileYRange = 50)
		{
			for (int i = 0; i < amount; i++)
			{
				int spawnTileX;
				int spawnTileY;
				TShock.Utils.GetRandomClearTileWithInRange(startTileX, startTileY, tileXRange, tileYRange, out spawnTileX,
															 out spawnTileY);
				int npcid = NPC.NewNPC(spawnTileX * 16, spawnTileY * 16, type, 0);
				// This is for special slimes
				if (type == 1)
				{
					Main.npc[npcid].SetDefaults(displayName);
				}
				else
				{
					Main.npc[npcid].SetDefaults(type);
				}
			}
		}
	}
}
