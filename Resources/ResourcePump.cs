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
        readonly Part part;

        public readonly PartResourceDefinition Resource;
        public double Request { get; private set; }
        public float Requested { get; private set; }
        public float Result { get; private set; }
        public float Ratio => Mathf.Abs(Result / Requested);
        public bool PartialTransfer => Math.Abs(Requested) - Math.Abs(Result) > eps;
        public bool Valid => part != null;

        public ResourcePump(Part part, int res_ID)
        {
            Resource = PartResourceLibrary.Instance.GetDefinition(res_ID);
            if(Resource != null)
                this.part = part;
            else
                Utils.Log("WARNING: Cannot find a resource with '{}' ID in the library.", res_ID);
        }

        public ResourcePump(Part part, string res_name)
        {
            Resource = PartResourceLibrary.Instance.GetDefinition(res_name);
            if(Resource != null)
                this.part = part;
            else
                Utils.Log("WARNING: Cannot find '{}' in the resource library.", res_name);
        }

        public void RequestTransfer(float dR)
        {
            Request += dR;
        }

        public bool TransferResource()
        {
            if(Math.Abs(Request) <= min_request)
                return false;
            Result = (float)part.RequestResource(Resource.id, Request);
            Requested = (float)Request;
            Request = 0;
            return true;
        }

        public void Clear()
        {
            Request = Requested = Result = 0;
        }

        public void Revert()
        {
            if(Result.Equals(0))
                return;
            part.RequestResource(Resource.id, -(double)Result);
            Request = Result;
            Requested = Result = 0;
        }
    }
}
