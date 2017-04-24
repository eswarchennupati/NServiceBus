namespace NServiceBus
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Janitor;
    using Persistence;

    [SkipWeaving]
    class DevelopmentSynchronizedStorageSession : CompletableSynchronizedStorageSession
    {
        public DevelopmentSynchronizedStorageSession(SagaManifestCollection sagaManifests)
        {
            this.sagaManifests = sagaManifests;
        }

        public void Dispose()
        {
            foreach (var sagaFile in sagaFiles.Values)
            {
                sagaFile.Dispose();
            }
        }

        public Task CompleteAsync()
        {
            foreach (var sagaFile in sagaFiles.Values)
            {
                sagaFile.Complete();
            }

            return TaskEx.CompletedTask;
        }

        public bool TryOpenAndLockSaga(Guid sagaId, Type entityType, out SagaStorageFile sagaStorageFile)
        {
            var sagaManifest = sagaManifests.GetForEntityType(entityType);

            if (!SagaStorageFile.TryOpen(sagaId, sagaManifest, out sagaStorageFile))
            {
                return false;
            }

            RegisterSagaFile(sagaStorageFile, sagaId, sagaManifest.SagaEntityType);

            return true;
        }


        public SagaStorageFile CreateNew(Guid sagaId, Type entityType)
        {
            var sagaManifest = sagaManifests.GetForEntityType(entityType);

            var sagaFile = SagaStorageFile.Create(sagaId, sagaManifest);

            RegisterSagaFile(sagaFile, sagaId, sagaManifest.SagaEntityType);

            return sagaFile;
        }

        public SagaStorageFile GetSagaFile(IContainSagaData sagaData)
        {
            var sagaFileKey = $"{sagaData.GetType().FullName}{sagaData.Id}";
            SagaStorageFile sagaStorageFile;
            if (!sagaFiles.TryGetValue(sagaFileKey, out sagaStorageFile))
            {
                throw new Exception("The saga should be retrieved with Get method before it's updated or completed.");
            }
            return sagaStorageFile;
        }

        void RegisterSagaFile(SagaStorageFile sagaStorageFile, Guid sagaId, Type sagaDataType)
        {
            sagaFiles[$"{sagaDataType.FullName}{sagaId}"] = sagaStorageFile;
        }

        SagaManifestCollection sagaManifests;
        Dictionary<string, SagaStorageFile> sagaFiles = new Dictionary<string, SagaStorageFile>();
    }
}