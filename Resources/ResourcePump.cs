//   ResourcePump.cs
//
//  Author:
//       Allis Tauri <allista@gmail.com>
//
//  Copyright (c) 2016 Allis Tauri

using System;
using UnityEngine;

namespace AT_Utils
{
    public class ResourcePump
    {
        const float eps = 1e-7f;
        const float min_request = 1e-5f;
        double request;

        readonly Part part;

        public readonly PartResourceDefinition Resource;
        public float Requested { get; private set; }
        public float Result    { get; private set; }
        public float Ratio     { get { return Mathf.Abs(Result/Requested); } }
        public bool  PartialTransfer { get { return Math.Abs(Requested)-Math.Abs(Result) > eps; } }
        public bool  Valid     { get { return part != null; } }

        public ResourcePump(Part part, int res_ID)
        { 
            Resource = PartResourceLibrary.Instance.GetDefinition(res_ID);
            if(Resource != null) this.part = part;
            else Utils.Log("WARNING: Cannot find a resource with '{}' ID in the library.", res_ID);
        }

        public ResourcePump(Part part, string res_name)
        {
            Resource = PartResourceLibrary.Instance.GetDefinition(res_name);
            if(Resource != null) this.part  = part;
            else Utils.Log("WARNING: Cannot find '{}' in the resource library.", res_name);
        }

        public void RequestTransfer(float dR) { request += dR; }

        public bool TransferResource()
        {
            if(Math.Abs(request) <= min_request) return false;
            Result    = (float)part.RequestResource(Resource.id, request);
            Requested = (float)request;
            request   = 0;
            return true;
        }

        public void Clear()
        { request = Requested = Result = 0; }

        public void Revert()
        {
            if(Result.Equals(0)) return;
            part.RequestResource(Resource.id, -(double)Result);
            request = Result; Requested = Result = 0;
        }
    }
}

