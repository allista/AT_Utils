using System.Collections.Generic;

namespace AT_Utils
{
    public static class CrewTransferBatch
    {
        private static bool same_crew_member(ProtoCrewMember a, ProtoCrewMember b) =>
            a.name == b.name && a.gender == b.gender && a.trait == b.trait;


        #region Vessel
        private static ProtoCrewMember get_real_proto_crew_member(
            ProtoCrewMember kerbal,
            IShipconstruct fromShip,
            out Part inPart
        )
        {
            inPart = null;
            foreach(var p in fromShip.Parts)
            {
                var real_kerbal = p.protoModuleCrew.Find(c => same_crew_member(c, kerbal));
                if(real_kerbal != null)
                {
                    inPart = p;
                    return real_kerbal;
                }
            }
            return null;
        }

        private static bool move_crew(Part fromP, Part toP, ProtoCrewMember crew)
        {
            var success = toP.AddCrewmember(crew);
            if(success)
            {
                fromP.RemoveCrewmember(crew);
                GameEvents.onCrewTransferred.Fire(
                    new GameEvents.HostedFromToAction<ProtoCrewMember, Part>(crew, fromP, toP));
            }
            return success;
        }

        private static int move_crew_from_part(Part fromP, Part toP)
        {
            if(fromP == toP)
                return -1;
            var crewToMove = fromP.protoModuleCrew.Count;
            while(fromP.protoModuleCrew.Count > 0)
            {
                if(!move_crew(fromP, toP, fromP.protoModuleCrew[0]))
                    break;
            }
            return crewToMove - fromP.protoModuleCrew.Count;
        }

        private static int move_crew_from_part(Part fromP, IEnumerable<Part> toV)
        {
            var moved = 0;
            foreach(var toP in toV)
            {
                if(toP.CrewCapacity <= toP.protoModuleCrew.Count)
                    continue;
                var numMoved = move_crew_from_part(fromP, toP);
                if(numMoved < 0)
                    continue;
                if(numMoved == 0)
                    break;
                moved += numMoved;
            }
            return moved;
        }

        private static void update_vessel_crew(Vessel fromV, Vessel toV, bool spawn)
        {
            if(fromV == toV)
                Vessel.CrewWasModified(fromV);
            else
                Vessel.CrewWasModified(fromV, toV);
            if(spawn)
                respawnCrew(fromV, toV);
        }

        public static bool moveCrew(Vessel fromV, Vessel toV, IEnumerable<ProtoCrewMember> crew, bool spawn = true)
        {
            if(fromV == toV)
                return false;
            var all = true;
            var moved = false;
            foreach(var kerbal in crew)
            {
                var real_kerbal = get_real_proto_crew_member(kerbal, fromV, out var fromP);
                if(real_kerbal == null)
                    continue;
                var toP = toV.Parts.Find(p => p.CrewCapacity > p.protoModuleCrew.Count);
                if(toP == null)
                {
                    all = false;
                    break;
                }
                if(fromP == toP)
                    continue;
                if(move_crew(fromP, toP, real_kerbal))
                    moved = true;
                else
                    all = false;
            }
            if(moved)
                update_vessel_crew(fromV, toV, spawn);
            return all;
        }

        public static bool moveCrew(Vessel fromV, Part toP, IEnumerable<ProtoCrewMember> crew, bool spawn = true)
        {
            var all = true;
            var moved = false;
            foreach(var kerbal in crew)
            {
                if(toP.CrewCapacity <= toP.protoModuleCrew.Count)
                {
                    all = false;
                    break;
                }
                var real_kerbal = get_real_proto_crew_member(kerbal, fromV, out var fromP);
                if(real_kerbal == null)
                    continue;
                if(fromP == toP)
                    continue;
                if(move_crew(fromP, toP, real_kerbal))
                    moved = true;
                else
                    all = false;
            }
            if(moved)
                update_vessel_crew(fromV, toP.vessel, spawn);
            return all;
        }

        public static bool moveCrew(Vessel fromV, Vessel toV, bool spawn = true)
        {
            var all = true;
            var moved = false;
            foreach(var fromP in fromV.Parts)
            {
                if(move_crew_from_part(fromP, toV.Parts) > 0)
                    moved = true;
                if(fromP.protoModuleCrew.Count > 0)
                {
                    all = false;
                    break;
                }
            }
            if(moved)
                update_vessel_crew(fromV, toV, spawn);
            return all;
        }

        public static bool moveCrew(Vessel fromV, Part toP, bool spawn = true)
        {
            if(toP.CrewCapacity <= toP.protoModuleCrew.Count)
                return false;
            var all = true;
            var moved = false;
            foreach(var fromP in fromV.parts)
            {
                var numMoved = move_crew_from_part(fromP, toP);
                if(numMoved < 0)
                    continue;
                if(numMoved == 0)
                {
                    all = false;
                    break;
                }
                moved = true;
            }
            if(moved)
                update_vessel_crew(fromV, toP.vessel, spawn);
            return all;
        }

        public static bool moveCrew(Part fromP, Part toP, bool spawn = true)
        {
            var moved = move_crew_from_part(fromP, toP) > 0;
            if(moved)
                update_vessel_crew(fromP.vessel, toP.vessel, spawn);
            return moved;
        }

        public static void respawnCrew(Vessel V)
        {
            V.DespawnCrew();
            FlightGlobals.ActiveVessel.StartCoroutine(CallbackUtil.DelayedCallback(1,
                FlightGlobals.ActiveVessel.SpawnCrew));
        }

        public static void respawnCrew(Vessel fromV, Vessel toV)
        {
            if(fromV != toV)
                fromV.DespawnCrew();
            respawnCrew(toV);
        }
        #endregion

        #region ProtoVessel
        //add some crew to a part
        public static bool addCrew(ProtoPartSnapshot p, List<ProtoCrewMember> crew)
        {
            if(crew.Count == 0)
                return false;
            if(p.partInfo.partPrefab.CrewCapacity <= p.protoModuleCrew.Count)
                return false;
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
                if(crew.Count == 0)
                    break;
                addCrew(p, crew);
            }
        }
        #endregion
    }
}
