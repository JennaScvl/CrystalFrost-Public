using Microsoft.Extensions.Logging;
using OpenMetaverse;
using System;
using UnityEngine;

namespace CrystalFrost.Assets.Mesh
{
    /// <summary>
    /// Defines an interface for a manager that handles mesh assets.
    /// </summary>
    public interface IMeshManager : IDisposable
    {
        /// <summary>
        /// Gets the queue of decoded meshes that are ready for use.
        /// </summary>
        IDecodedMeshQueue ReadyMeshes { get; }
        /// <summary>
        /// Requests a mesh asset.
        /// </summary>
        /// <param name="gameObject">The GameObject associated with the mesh.</param>
        /// <param name="primitive">The primitive associated with the mesh.</param>
        /// <param name="uuid">The UUID of the mesh asset.</param>
        /// <param name="meshHolder">The GameObject that will hold the mesh.</param>
        void RequestMesh (GameObject gameObject, Primitive primitive, UUID uuid, GameObject meshHolder);
    }

    /// <summary>
    /// Manages the entire lifecycle of mesh assets, from requesting and downloading to decoding and caching.
    /// </summary>
    public class MeshManager : IMeshManager
    {
        private readonly ILogger<MeshManager> _log;
        private readonly IMeshRequestQueue _requestQueue;
        private readonly IMeshDownloadWorker _downloadWorker;
        private readonly IMeshDecodeWorker _decodeWorker;
		private readonly IMeshCacheWorker _meshCache;

		public IDecodedMeshQueue ReadyMeshes { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MeshManager"/> class.
        /// </summary>
        /// <param name="logger">The logger for recording messages.</param>
        /// <param name="readyMeshQueue">The queue for decoded meshes.</param>
        /// <param name="requestQueue">The queue for mesh requests.</param>
        /// <param name="downloadWorker">The worker for downloading meshes.</param>
        /// <param name="decodeWorker">The worker for decoding meshes.</param>
        /// <param name="meshCache">The worker for caching meshes.</param>
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

        /// <summary>
        /// Enqueues a request for a mesh asset.
        /// </summary>
        /// <param name="gameObject">The GameObject associated with the mesh.</param>
        /// <param name="primitive">The primitive associated with the mesh.</param>
        /// <param name="uuid">The UUID of the mesh asset.</param>
        /// <param name="meshHolder">The GameObject that will hold the mesh.</param>
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

        /// <summary>
        /// Releases all resources used by the <see cref="MeshManager"/> object.
        /// </summary>
        public void Dispose()
        {
            _meshCache.Dispose();
            _decodeWorker.Dispose();
            _downloadWorker.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
