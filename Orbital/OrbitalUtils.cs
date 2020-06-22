namespace AT_Utils
{
    public static partial class Utils
    {
        public static Vector3d Orbital2NodeDeltaV(Orbit o, Vector3d orbitalDeltaV, double UT)
        {
            var norm = o.GetOrbitNormal().normalized;
            var prograde = o.getOrbitalVelocityAtUT(UT).normalized;
            var radial = Vector3d.Cross(prograde, norm).normalized;
            return new Vector3d(Vector3d.Dot(orbitalDeltaV, radial),
                Vector3d.Dot(orbitalDeltaV, norm),
                Vector3d.Dot(orbitalDeltaV, prograde));
        }

        public static Vector3d Node2OrbitalDeltaV(Orbit o, Vector3d nodeDeltaV, double UT)
        {
            var norm = o.GetOrbitNormal().normalized;
            var prograde = o.getOrbitalVelocityAtUT(UT).normalized;
            var radial = Vector3d.Cross(prograde, norm).normalized;
            return radial * nodeDeltaV.x + norm * nodeDeltaV.y + prograde * nodeDeltaV.z;
        }

        public static Vector3d Node2OrbitalDeltaV(ManeuverNode node, Orbit o = null) =>
            Node2OrbitalDeltaV(o ?? node.patch, node.DeltaV, node.UT);

        public static void AddNode(Vessel vessel, Vector3d orbitalDeltaV, double UT)
        {
            var node = vessel.patchedConicSolver.AddManeuverNode(UT);
            node.DeltaV = Orbital2NodeDeltaV(node.patch, orbitalDeltaV, UT);
            vessel.patchedConicSolver.UpdateFlightPlan();
        }

        public static void AddNodeRaw(Vessel vessel, Vector3d nodeDeltaV, double UT)
        {
            var node = vessel.patchedConicSolver.AddManeuverNode(UT);
            node.DeltaV = nodeDeltaV;
            vessel.patchedConicSolver.UpdateFlightPlan();
        }

        public static void CopyNode(ManeuverNode node, Vessel toVessel) =>
            AddNodeRaw(toVessel, node.DeltaV, node.UT);

        public static bool CopyNodeFromFlightPlanNode(Vessel fromVessel, int index, Vessel toVessel)
        {
            if(index < 0
               || index >= fromVessel.flightPlanNode.CountNodes
               || toVessel.patchedConicSolver == null)
                return false;
            var node = new ManeuverNode();
            node.Load(fromVessel.flightPlanNode.nodes[index]);
            var nodeCopy = toVessel.patchedConicSolver.AddManeuverNode(node.UT);
            nodeCopy.DeltaV = node.DeltaV;
            toVessel.patchedConicSolver.UpdateFlightPlan();
            return true;
        }

        public static void CopyNodeToFlightPlanNode(ManeuverNode node, Vessel toVessel) =>
            AddNodeRawToFlightPlanNode(toVessel, node.DeltaV, node.UT);

        public static void AddNodeRawToFlightPlanNode(Vessel vessel, Vector3d nodeDeltaV, double UT)
        {
            var newNodeCfg = new ConfigNode("MANEUVER");
            var newNode = new ManeuverNode { UT = UT, DeltaV = nodeDeltaV };
            newNode.Save(newNodeCfg);
            var newFlightPlan = new ConfigNode("FLIGHTPLAN");
            if(vessel.flightPlanNode.CountNodes > 0)
                foreach(ConfigNode n in vessel.flightPlanNode.nodes)
                {
                    var nodeUTs = n.GetValue("UT");
                    if(string.IsNullOrEmpty(nodeUTs))
                        continue;
                    if(!double.TryParse(nodeUTs, out var nodeUT))
                        continue;
                    if(nodeUT < UT)
                        newFlightPlan.AddNode(n);
                    else
                    {
                        newFlightPlan.AddNode(newNodeCfg);
                        newFlightPlan.AddNode(n);
                    }
                }
            else
                newFlightPlan.AddNode(newNodeCfg);
            vessel.flightPlanNode = newFlightPlan;
            vessel.Log("New flightPlanNode: {}", vessel.flightPlanNode); //debug
        }
    }
}
