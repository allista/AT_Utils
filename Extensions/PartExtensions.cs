using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CompoundParts;
using UnityEngine;

namespace AT_Utils
{
    public static class PartExtensions
    {
        #region from MechJeb2 PartExtensions
        public static bool HasModule<T>(this Part p) where T : PartModule => p.Modules.GetModule<T>() != null;

        public static float TotalMass(this Part p) => p.mass + p.GetResourceMass();
        #endregion

        #region Find Modules or Parts
        public static IEnumerable<Part> AllChildren(this Part p)
        {
            foreach(var child in p.children)
            {
                yield return child;
                foreach(var descendant in child.AllChildren())
                    yield return descendant;
            }
        }

        public static IEnumerable<Part> AllConnectedParts(this Part p)
        {
            while(p.parent != null)
                p = p.parent;
            yield return p;
            foreach(var descendant in p.AllChildren())
                yield return descendant;
        }

        public static IEnumerable<Part> AllAttachedParts(this Part p)
        {
            if(p.parent != null)
                yield return p.parent;
            foreach(var child in p.children)
                yield return child;
        }

        public static Part AttachedPartWithModule<T>(this Part p) where T : PartModule =>
            p.AllAttachedParts().FirstOrDefault(c => c.HasModule<T>());

        public static T GetModuleInAttachedPart<T>(this Part p) where T : PartModule
        {
            if(p.parent != null)
            {
                var m = p.parent.Modules.GetModule<T>();
                if(m != null)
                    return m;
            }
            foreach(var c in p.children)
            {
                var m = c.Modules.GetModule<T>();
                if(m != null)
                    return m;
            }
            return null;
        }

        public static List<ModuleT> AllModulesOfType<ModuleT>(
            this Part part,
            ModuleT exception = null
        )
            where ModuleT : PartModule
        {
            var passages = new List<ModuleT>();
            foreach(Part p in part.AllConnectedParts())
                passages.AddRange(
                    from m in p.Modules.OfType<ModuleT>()
                    where exception == null || m != exception
                    select m);
            return passages;
        }

        public static ResourcePump CreateSocket(this Part p) => new ResourcePump(p, Utils.ElectricCharge.id);
        #endregion

        #region Resources and Phys-Props
        public static float TotalCost(this Part p) =>
            p.partInfo != null ? p.partInfo.cost + p.GetModuleCosts(p.partInfo.cost) : 0;

        public static float ResourcesCost(this Part p)
        {
            var cost = 0.0;
            p.Resources.ForEach(r => cost += r.amount * r.info.unitCost);
            return (float)cost;
        }

        public static float MaxResourcesCost(this Part p)
        {
            var cost = 0.0;
            p.Resources.ForEach(r => cost += r.maxAmount * r.info.unitCost);
            return (float)cost;
        }

        public static float DryCost(this Part p) => p.TotalCost() - p.MaxResourcesCost();

        public static float MassWithChildren(this Part p)
        {
            float mass = p.TotalMass();
            p.children.ForEach(ch => mass += ch.MassWithChildren());
            return mass;
        }

        public static bool TryUseResource(this Part part, int resource_id, double amount)
        {
            if(!amount.Equals(0))
            {
                var got = part.RequestResource(resource_id, amount);
                if(Math.Abs(1 - got / amount) > 1e-5)
                {
                    part.RequestResource(resource_id, -got);
                    return false;
                }
            }
            return true;
        }
        #endregion

        #region Actions
        public class CompoundPartReconnect : IDisposable
        {
            private readonly Dictionary<CompoundPartModule, Part> targets =
                new Dictionary<CompoundPartModule, Part>();


            public CompoundPartReconnect(Part part)
            {
                foreach(var p in part.AllConnectedParts())
                {
                    var cp = p as CompoundPart;
                    if(cp == null)
                        continue;
                    var cpm = cp.Modules.GetModule<CompoundPartModule>();
                    if(cpm == null)
                        continue;
                    if(cpm.target == null)
                        continue;
                    targets.Add(cpm, cpm.target);
                    cpm.OnTargetLost();
                }
            }

            private static void set_compound_part_target(CompoundPartModule cpm, Part target)
            {
                if(cpm != null && target != null)
                    cpm.OnTargetSet(target);
            }

            public void Dispose()
            {
                foreach(var cpm_target in targets)
                {
                    cpm_target.Key.StartCoroutine(CallbackUtil.DelayedCallback(1,
                        set_compound_part_target,
                        cpm_target.Key,
                        cpm_target.Value));
                }
                targets.Clear();
            }
        }

        public static CompoundPartReconnect ReconnectCompoundParts(this Part p) =>
            new CompoundPartReconnect(p);

        public static void BreakConnectedCompoundParts(this Part p)
        {
            //break connected compound parts
            foreach(Part part in p.AllConnectedParts())
            {
                var cp = part as CompoundPart;
                if(cp == null)
                    continue;
                var cpm = cp.Modules.GetModule<CompoundPartModule>();
                if(cpm == null)
                    continue;
                cpm.OnTargetLost();
            }
        }

        public static void UpdateOrgPos(this Part part, Part root) =>
            part.orgPos = root.partTransform.InverseTransformPoint(part.partTransform.position);

        public static Vector3 AttachNodeDeltaPos(this Part part, AttachNode node)
        {
            var an = node.attachedPart.FindAttachNodeByPart(part);
            return an != null
                ? (part.partTransform.TransformPoint(node.position)
                   - node.attachedPart.partTransform.TransformPoint(an.position))
                : Vector3.zero;
        }

        public static void UpdateAttachedPartPos(this Part part, AttachNode node)
        {
            if(node != null && node.attachedPart != null)
            {
                var dp = part.AttachNodeDeltaPos(node);
                if(!dp.IsZero())
                    part.UpdateAttachedPartPos(node.attachedPart, dp);
            }
        }

        public static void UpdateAttachedPartPos(this Part part, Part attached_part, Vector3 delta)
        {
            if(HighLogic.LoadedSceneIsFlight && part.vessel != null)
                part.UpdateAttachedPartPosFlight(attached_part, delta);
            else
                part.UpdateAttachedPartPosEditor(attached_part, delta);
        }

        public static void UpdateAttachedPartPosEditor(
            this Part part,
            Part attached_part,
            Vector3 delta
        )
        {
            if(attached_part == part.parent)
            {
                part.partTransform.position -= delta;
                attached_part = attached_part.localRoot;
                attached_part.partTransform.position += delta;
                part.UpdateOrgPos(attached_part);
            }
            else if(attached_part.parent == part)
            {
                attached_part.partTransform.position += delta;
                attached_part.UpdateOrgPos(attached_part.localRoot);
            }
        }

        public class PartJoinRecreate : IDisposable
        {
            public readonly Part part;
            public readonly bool has_part_joint;

            public PartJoinRecreate(Part part)
            {
                this.part = part;
                has_part_joint = part.attachJoint != null;
                if(has_part_joint)
                    part.attachJoint.DestroyJoint();
            }

            public void Dispose()
            {
                if(has_part_joint && part != null)
                {
                    part.CreateAttachJoint(part.attachMode);
                    part.ResetJoints();
                }
            }
        }

        public static void UpdateAttachedPartPosFlight(
            this Part part,
            Part attached_part,
            Vector3 delta
        )
        {
            if(part.vessel != null && attached_part.vessel == part.vessel)
            {
                if(attached_part == part.parent)
                {
                    using(new PartJoinRecreate(part))
                    {
                        part.partTransform.position -= delta;
                        part.UpdateOrgPos(part.vessel.rootPart);
                        part.partTransform.rotation =
                            part.vessel.vesselTransform.rotation * part.orgRot;
                    }
                }
                else if(attached_part.parent == part)
                {
                    using(new PartJoinRecreate(part))
                    {
                        attached_part.partTransform.position += delta;
                        attached_part.UpdateOrgPos(part.vessel.rootPart);
                        attached_part.partTransform.rotation =
                            attached_part.vessel.vesselTransform.rotation * attached_part.orgRot;
                    }
                }
            }
        }

        private static readonly FieldInfo partInertiaTensorFI = typeof(Part).GetField(
            "inertiaTensor",
            BindingFlags.Instance | BindingFlags.NonPublic);

        public static void UpdateInertiaTensor(this Part part)
        {
            if(part.rb == null)
                return;
            part.rb.ResetInertiaTensor();
            var inertiaTensor = part.rb.inertiaTensor / Mathf.Max(1f, part.rb.mass);
            partInertiaTensorFI.SetValue(part, inertiaTensor);
        }

        public static void UpdateCoMOffset(this Part part, Vector3 newCoMOffset)
        {
            part.CoMOffset = newCoMOffset;
            if(part.rb == null)
                return;
            part.rb.centerOfMass = part.CoMOffset;
            part.UpdateInertiaTensor();
        }
        #endregion

        #region Logging
        public static string Title(this Part p) =>
            p != null
                ? p.partInfo != null
                    ? p.partInfo.title
                    : p.name
                : "";

        public static void Log(this MonoBehaviour mb, string msg, params object[] args) =>
            Utils.Log($"{mb.GetID()}: {msg}", args);

        public static void Log(this Part p, string msg, params object[] args) => Utils.Log($"{p.GetID()}: {msg}", args);
        public static void Log(this Part p, string msg) => Utils.Log($"{p.GetID()}: {msg}");
        public static void Debug(this Part p, string msg) => Utils.Debug($"{p.GetID()}: {msg}");
        public static void Info(this Part p, string msg) => Utils.Info($"{p.GetID()}: {msg}");
        public static void Warning(this Part p, string msg) => Utils.Warning($"{p.GetID()}: {msg}");
        public static void Error(this Part p, string msg) => Utils.Error($"{p.GetID()}: {msg}");
        #endregion

        #region Misc
        public static void HighlightAlways(this Part p, Color c)
        {
            p.highlightColor = c;
            p.RecurseHighlight = false;
            p.SetHighlightType(Part.HighlightType.AlwaysOn);
        }

        public static IEnumerable<MeshTransform> AllModelMeshes(this Part p) =>
            p.FindModelComponents<MeshFilter>()
                .Select(c => new MeshTransform(c))
                .Union(p.FindModelComponents<SkinnedMeshRenderer>()
                    .Select(c => new MeshTransform(c)));
        #endregion
    }
}
