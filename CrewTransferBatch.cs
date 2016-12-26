using System.Collections.Generic;

namespace AT_Utils
{
	public static class CrewTransferBatch
	{
		#region Vessel
		public static bool moveCrew(Vessel fromV, Vessel toV, List<ProtoCrewMember> crew, bool spawn = true)
		{
			if(crew.Count == 0) return false;
			var moved = false;
			while(crew.Count > 0)
			{
				var kerbal = crew[0];
				var fromP = fromV.parts.Find(p => p.protoModuleCrew.Contains(kerbal));
				if(fromP == null) continue;
				var toP = toV.parts.Find(p => p.CrewCapacity > p.protoModuleCrew.Count);
				if(toP == null) break;
				move_crew(fromP, toP, kerbal);
				crew.RemoveAt(0);
				moved = true;
			}
			if(moved && spawn) respawnCrew(fromV, toV);
			return crew.Count == 0;
		}

		public static bool moveCrew(Vessel fromV, Vessel toV, bool spawn = true)
		{
			var all = true;
			var moved = false;
			foreach(var fromP in fromV.parts)
			{
				while(fromP.protoModuleCrew.Count > 0)
				{
					var toP = toV.parts.Find(p => p.CrewCapacity > p.protoModuleCrew.Count);
					if(toP == null) break;
					move_crew(fromP, toP, fromP.protoModuleCrew[0]);
					moved = true;
				}
				if(fromP.protoModuleCrew.Count > 0)
				{ all = false; break; }
			}
			if(moved && spawn) respawnCrew(fromV, toV);
			return all;
		}

		public static bool moveCrew(Vessel fromV, Part toP, bool spawn = true)
		{
			if(toP.CrewCapacity <= toP.protoModuleCrew.Count) return false;
			var all = true;
			var moved = false;
			foreach(var fromP in fromV.parts)
			{
				while(toP.protoModuleCrew.Count < toP.CrewCapacity && fromP.protoModuleCrew.Count > 0)
				{
					move_crew(fromP, toP, fromP.protoModuleCrew[0]);
					moved = true;
				}
				if(fromP.protoModuleCrew.Count > 0) 
				{ all = false; break; }
			}
			if(moved && spawn) respawnCrew(fromV, toP.vessel);
			return all;
		}

		public static bool moveCrew(Part fromP, Part toP, bool spawn = true)
		{
			if(fromP.protoModuleCrew.Count == 0 ||
			   toP.CrewCapacity <= toP.protoModuleCrew.Count) return false;
			while(toP.protoModuleCrew.Count < toP.CrewCapacity && fromP.protoModuleCrew.Count > 0)
				move_crew(fromP, toP, fromP.protoModuleCrew[0]);
			if(spawn) respawnCrew(fromP.vessel, toP.vessel);
			return fromP.protoModuleCrew.Count > 0;
		}

		static void move_crew(Part fromP, Part toP, ProtoCrewMember crew)
		{
			fromP.RemoveCrewmember(crew);
			toP.AddCrewmember(crew);
			GameEvents.onCrewTransferred.Fire(new GameEvents.HostedFromToAction<ProtoCrewMember, Part>(crew, fromP, toP));
		}

		public static void respawnCrew(Vessel V)
		{
			Vessel.CrewWasModified(V);
			FlightGlobals.ActiveVessel.DespawnCrew();
			V.StartCoroutine(CallbackUtil.DelayedCallback(1, FlightGlobals.ActiveVessel.SpawnCrew));
		}

		public static void respawnCrew(Vessel fromV, Vessel toV)
		{
			Vessel.CrewWasModified(fromV, toV);
			FlightGlobals.ActiveVessel.DespawnCrew();
			toV.StartCoroutine(CallbackUtil.DelayedCallback(1, FlightGlobals.ActiveVessel.SpawnCrew));
		}
		#endregion

		#region ProtoVessel
		//add some crew to a part
		public static bool addCrew(ProtoPartSnapshot p, List<ProtoCrewMember> crew)
		{
			if(crew.Count == 0) return false;
			if(p.partInfo.partPrefab.CrewCapacity <= p.protoModuleCrew.Count) return false;
			while(p.protoModuleCrew.Count < p.partInfo.partPrefab.CrewCapacity && crew.Count > 0)
			{
				var kerbal = crew[0];
				kerbal.rosterStatus = ProtoCrewMember.RosterStatus.Assigned;
				p.protoCrewNames.Add(kerbal.name);
				p.protoModuleCrew.Add(kerbal);
				crew.RemoveAt(0);
			}
			return true;
		}

		//add some crew to a vessel
		public static void addCrew(ProtoVessel vsl, List<ProtoCrewMember> crew)
		{
			foreach(var p in vsl.protoPartSnapshots)
			{
				if(crew.Count == 0) break;
				addCrew(p, crew);
			}
		}
		#endregion
	}
}

