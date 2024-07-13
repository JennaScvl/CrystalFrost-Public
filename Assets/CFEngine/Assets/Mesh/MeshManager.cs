using Microsoft.Extensions.Logging;
using OpenMetaverse;
using System;
using UnityEngine;

namespace CrystalFrost.Assets.Mesh
{
    public interface IMeshManager : IDisposable
    { 
        IDecodedMeshQueue ReadyMeshes { get; }
        void RequestMesh (GameObject gameObject, Primitive primitive, UUID uuid, GameObject meshHolder);
    }

    public class MeshManager : IMeshManager
    {
        private readonly ILogger<MeshManager> _log;
        private readonly IMeshRequestQueue _requestQueue;
        private readonly IMeshDownloadWorker _downloadWorker;
        private readonly IMeshDecodeWorker _decodeWorker;
		private readonly IMeshCacheWorker _meshCache;

		public IDecodedMeshQueue ReadyMeshes { get; }

		public MeshManager(
            ILogger<MeshManager> logger,
            IDecodedMeshQueue readyMeshQueue,
            IMeshRequestQueue requestQueue,
            IMeshDownloadWorker downloadWorker,
            IMeshDecodeWorker decodeWorker,
			IMeshCacheWorker meshCache)
        {
            _log = logger;
            _requestQueue = requestQueue;
            _downloadWorker = downloadWorker;
            _decodeWorker = decodeWorker;
            ReadyMeshes = readyMeshQueue;
            _meshCache = meshCache;
		}

        public void RequestMesh(GameObject gameObject, Primitive primitive, UUID uuid, GameObject meshHolder)
		{
			_log.MeshRequested(uuid);
            MeshRequest request = new MeshRequest
            {
                GameObject = gameObject,
                Primitive = primitive,
                UUID = uuid,
                MeshHolder = meshHolder
            };
			_requestQueue.Enqueue(request);
        }

        public void Dispose()
        {
            _meshCache.Dispose();
            _decodeWorker.Dispose();
            _downloadWorker.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
