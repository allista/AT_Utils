using System;
using System.Collections.Generic;

namespace AT_Utils 
{
	public class ResourceProxy : ProtoPartResourceSnapshot
	{
		protected ConfigNode valuesRef;
		protected ProtoPartResourceSnapshot protoRef;

		static ConfigNode resource_values(ProtoPartResourceSnapshot res)
		{
			var node = new ConfigNode("RESOURCE");
			res.Save(node);
			return node;
		}

		public ResourceProxy(PartResource res) : base(res) {}

		public ResourceProxy(ProtoPartResourceSnapshot res)
			: base(resource_values(res))
		{ 
			if(res.resourceRef != null)
				resourceRef = res.resourceRef;
			protoRef = res;
		}

		public ResourceProxy(ConfigNode node_ref)
			: base(node_ref)
		{
			valuesRef = node_ref;
		}

		public void Sync()
		{
			if(resourceRef != null)
			{
				resourceRef.amount = amount;
				resourceRef.maxAmount = maxAmount;
				resourceRef.flowState = flowState;
			}
			if(protoRef != null)
			{
				protoRef.amount = amount;
				protoRef.maxAmount = maxAmount;
				protoRef.flowState = flowState;
			}
			if(valuesRef != null)
				Save(valuesRef);
		}

        public override string ToString()
        {
            return Utils.Format(
                "(res.ref {}, proto.ref {}, have valuesRef: {}, amount {}/{})",
                resourceRef != null? resourceRef.GetHashCode() : 0, 
                protoRef != null? protoRef.GetHashCode() : 0, 
                valuesRef != null, 
                amount,maxAmount
            );
        }
	}

	public class PartProxy : Dictionary<string, ResourceProxy>
	{

		public PartProxy(Part part)
		{
			foreach(var res in part.Resources)
				Add(res.resourceName, new ResourceProxy(res));
		}

		public PartProxy(ProtoPartSnapshot proto_part)
		{
			foreach(var res in proto_part.resources)
				Add(res.resourceName, new ResourceProxy(res));
		}

		public PartProxy(ConfigNode part_node)
		{
			foreach(var res in part_node.GetNodes("RESOURCE"))
			{
				var proxy = new ResourceProxy(res);
				Add(proxy.resourceName, proxy);
			}
		}
	}

	public class VesselResources
	{
		public readonly List<PartProxy> Parts = new List<PartProxy>();
        public readonly ListDict<string, PartProxy> Resources = new ListDict<string, PartProxy>();
		public List<string> resourcesNames { get { return new List<string>(Resources.Keys); } }

		void add_part_proxy(PartProxy proxy)
		{
			Parts.Add(proxy);
            proxy.ForEach(res => Resources.Add(res.Key, proxy));
		}

		public VesselResources(IShipconstruct vessel)
		{ vessel.Parts.ForEach(p => add_part_proxy(new PartProxy(p))); }

		public VesselResources(ProtoVessel proto_vessel)
		{ proto_vessel.protoPartSnapshots.ForEach(p => add_part_proxy(new PartProxy(p))); }

		public VesselResources(ConfigNode vessel_node)
		{ 
			foreach(var part in vessel_node.GetNodes("PART"))
				add_part_proxy(new PartProxy(part));
		}

		/// <summary>
		/// Return the vessel's total capacity for the resource.
		/// If the vessel has no such resource 0.0 is returned.
		/// </summary>
		/// <returns>Total resource capacity.</returns>
		/// <param name="resource">Resource name.</param>
		public double ResourceCapacity(string resource)
		{
			if(!Resources.ContainsKey(resource)) return 0.0;
			double capacity = 0;
			Resources[resource].ForEach(p => capacity += p[resource].maxAmount);
			return capacity;
		}

		/// <summary>
		/// Return the vessel's total available amount of the resource.
		/// If the vessel has no such resource 0.0 is returned.
		/// </summary>
		/// <returns>Total resource amount.</returns>
		/// <param name="resource">Resource name.</param>
		public double ResourceAmount(string resource)
		{
			if(!Resources.ContainsKey(resource)) return 0.0;
			double amount = 0;
			Resources[resource].ForEach(p => amount += p[resource].amount);
			return amount;
		}

		/// <summary>
		/// Transfer a resource into (positive amount) or out of (negative
		/// amount) the vessel. No attempt is made to balance the resource
		/// across parts: they are filled/emptied on a first-come-first-served
		/// basis.
		/// If the vessel has no such resource no action is taken.
		/// Returns the amount of resource not transfered (0 = all has been
		/// transfered).
		/// Based on the code from Extraplanetary Launchpads plugin. Resources.cs module.
		/// </summary>
		/// <returns>The resource.</returns>
		/// <param name="resource">Resource.</param>
		/// <param name="amount">Amount.</param>
		public double TransferResource(string resource, double amount)
		{
			if(!Resources.ContainsKey(resource)) return 0.0;
			foreach(var part in Resources[resource]) 
			{
				var adjust = amount;
				var res = part[resource];
				if(adjust < 0  && -adjust > res.amount)
					// Ensure the resource amount never goes negative
					adjust = -res.amount;
				else if(adjust > 0 &&
				        adjust > (res.maxAmount - res.amount))
					// ensure the resource amount never excees the maximum
					adjust = res.maxAmount - res.amount;
				res.amount += adjust;
				res.Sync();
				amount -= adjust;
			}
			return amount;
		}

        public override string ToString()
        { 
            if(Parts.Count == 0) 
                return "No parts with resources.";
            var ret = "";
            Parts.ForEach(p => { if(p.Count > 0) ret += Utils.Format("{}\n", p); });
            return ret;
        }
	}
	
	public class ResourceManifest
	{
		public string name;
		public double pool;
		
		public double amount;
		public double capacity;
		public double offset;
		
		public double host_amount;
		public double host_capacity;
		
		public double minAmount;
		public double maxAmount;

        public override string ToString()
        {
            return Utils.Format(
                "{}: min {}, cur {}, max {}\n" +
                "transfer: {}",
                name, minAmount, amount, maxAmount,
                offset-amount
            );
        }
	}

	public class ResourceManifestList : List<ResourceManifest>
	{
		public void NewTransfer(VesselResources host, VesselResources target)
		{
			Clear();
			foreach(var r in target.resourcesNames)
			{
				if(host.ResourceCapacity(r) <= 0) continue;
				var rm = new ResourceManifest();
				rm.name          = r;
				rm.amount        = target.ResourceAmount(r);
				rm.capacity      = target.ResourceCapacity(r);
				rm.offset        = rm.amount;
				rm.host_amount   = host.ResourceAmount(r);
				rm.host_capacity = host.ResourceCapacity(r);
				rm.pool          = rm.host_amount + rm.offset;
				rm.minAmount     = Math.Max(0, rm.pool-rm.host_capacity);
				rm.maxAmount     = Math.Min(rm.pool, rm.capacity);
				Add(rm);
			}
		}

		public void UpdateHostInfo(VesselResources host)
		{
			foreach(ResourceManifest r in this)
			{
				r.host_amount = host.ResourceAmount(r.name);
				r.pool        = r.host_amount + r.offset;
				r.minAmount   = Math.Max(0, r.pool-r.host_capacity);
				r.maxAmount   = Math.Min(r.pool, r.capacity);
			}
		}

		public void TransferResources(VesselResources host, VesselResources target, out double deltaMass, out double deltaCost)
		{
			deltaMass = deltaCost = 0;
			if(Count == 0) return;
			foreach(var r in this)
			{
				//transfer resource between host and target
				var a = host.TransferResource(r.name, r.offset-r.amount);
				a = r.amount-r.offset + a;
				var b = target.TransferResource(r.name, a);
				host.TransferResource(r.name, b);
				//update masses
				PartResourceDefinition res_def = PartResourceLibrary.Instance.GetDefinition(r.name);
				if(res_def.density <= 0) continue;
				deltaMass += a*res_def.density;
				deltaCost += a*res_def.unitCost;
			}
			Clear();
		}
	}
}
